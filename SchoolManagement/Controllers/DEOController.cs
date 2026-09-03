using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Extensions;
using SchoolManagement.Models;
using SchoolManagement.Services;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class DEOController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DEOController(
            ApplicationDbContext context,
            IFileService _fileService,
            IAuditService auditService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this._fileService = _fileService;
            _auditService = auditService;
            _userManager = userManager;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;

            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            if (sessionSchoolId.HasValue) return sessionSchoolId.Value;

            return _context.Schools.FirstOrDefault()?.Id ?? 4;
        }

        // DEO Operator Dashboard
        public async Task<IActionResult> Index(string? search, string? searchTerm, int? classId, int? selectedClassId, int? sectionId, int? selectedSectionId, int page = 1)
        {
            var schoolId = GetEffectiveSchoolId();
            int pageSize = 20;

            var effectiveSearch = !string.IsNullOrWhiteSpace(search) ? search : searchTerm;
            var effectiveClassId = classId ?? selectedClassId;
            var effectiveSectionId = sectionId ?? selectedSectionId;

            var query = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Include(s => s.AcademicYear)
                .Where(s => s.SchoolId == schoolId && s.IsActive);

            if (effectiveClassId.HasValue)
                query = query.Where(s => s.ClassId == effectiveClassId.Value);

            if (effectiveSectionId.HasValue)
                query = query.Where(s => s.SectionId == effectiveSectionId.Value);

            if (!string.IsNullOrWhiteSpace(effectiveSearch))
            {
                var term = effectiveSearch.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                         s.AdmissionNumber.ToLower().Contains(term) ||
                                         s.FatherName.ToLower().Contains(term) ||
                                         s.FatherMobile.Contains(term) ||
                                         s.Address.ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var pageIndex = Math.Max(1, page);

            var students = await query
                .OrderByDescending(s => s.AdmissionDate)
                .ThenByDescending(s => s.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var today = DateTime.UtcNow.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var todayCount = await _context.Students.CountAsync(s => s.SchoolId == schoolId && s.IsActive && s.AdmissionDate.Date == today);
            var monthCount = await _context.Students.CountAsync(s => s.SchoolId == schoolId && s.IsActive && s.AdmissionDate >= firstDayOfMonth);
            var allStudentsCount = await _context.Students.CountAsync(s => s.SchoolId == schoolId && s.IsActive);

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var sections = classId.HasValue
                ? await _context.Sections.Where(s => s.ClassId == classId.Value && s.IsActive).ToListAsync()
                : await _context.Sections.Where(s => s.SchoolId == schoolId && s.IsActive).ToListAsync();

            var viewModel = new DEOConsoleViewModel
            {
                RecentAdmissions = students,
                TotalStudents = allStudentsCount,
                TodayAdmissionsCount = todayCount,
                ThisMonthAdmissionsCount = monthCount,
                SearchTerm = search,
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                Classes = new SelectList(classes, "Id", "Name", classId),
                Sections = new SelectList(sections, "Id", "Name", sectionId),
                PageIndex = pageIndex,
                TotalPages = Math.Max(1, totalPages),
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        // GET: DEO/Admission (Fast Entry Form)
        [HttpGet]
        public async Task<IActionResult> Admission(int? classId, int? sectionId)
        {
            var schoolId = GetEffectiveSchoolId();
            var academicYear = await _context.AcademicYears
                .Where(a => a.SchoolId == schoolId && a.IsActive)
                .OrderByDescending(a => a.IsCurrent)
                .FirstOrDefaultAsync();

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var targetClassId = classId ?? classes.FirstOrDefault()?.Id ?? 0;
            var sections = await _context.Sections
                .Where(s => s.ClassId == targetClassId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var targetSectionId = sectionId ?? sections.FirstOrDefault()?.Id ?? 0;

            // Calculate Next Roll Number in chosen section
            int nextRoll = 1;
            if (targetSectionId > 0)
            {
                var maxRoll = await _context.Students
                    .Where(s => s.SectionId == targetSectionId && s.IsActive)
                    .MaxAsync(s => (int?)s.RollNumber) ?? 0;
                nextRoll = maxRoll + 1;
            }

            // Auto-generate suggested Admission No
            var totalCount = await _context.Students.CountAsync(s => s.SchoolId == schoolId) + 1;
            var suggestedAdmNo = $"RNB-{DateTime.Today.Year}-{totalCount:D4}";

            var model = new DEOAdmissionViewModel
            {
                SchoolId = schoolId,
                AcademicYearId = academicYear?.Id ?? 0,
                ClassId = targetClassId,
                SectionId = targetSectionId,
                AdmissionNumber = suggestedAdmNo,
                RollNumber = nextRoll,
                AdmissionDate = DateTime.Today,
                City = "Piro",
                State = "Bihar",
                PinCode = "802207",
                AcademicYears = new SelectList(await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync(), "Id", "Name", academicYear?.Id),
                Classes = new SelectList(classes, "Id", "Name", targetClassId),
                Sections = new SelectList(sections, "Id", "Name", targetSectionId)
            };

            return View(model);
        }

        // POST: DEO/Admission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admission(DEOAdmissionViewModel model, string? submitAction)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            // Check duplicate admission number
            var existingAdm = await _context.Students
                .AnyAsync(s => s.SchoolId == schoolId && s.AdmissionNumber == model.AdmissionNumber && s.IsActive);
            if (existingAdm)
            {
                ModelState.AddModelError("AdmissionNumber", "Admission Number already exists for another student.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, schoolId);
                return View(model);
            }

            // Handle Photo Upload
            string? photoPath = null;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                photoPath = await _fileService.UploadFileAsync(model.PhotoFile, "uploads/students");
            }

            // Automatic sequential roll number allocation if not explicitly specified
            int finalRollNumber = model.RollNumber;
            if (finalRollNumber <= 0)
            {
                var maxRoll = await _context.Students
                    .Where(s => s.SectionId == model.SectionId && s.IsActive)
                    .MaxAsync(s => (int?)s.RollNumber) ?? 0;
                finalRollNumber = maxRoll + 1;
            }

            var student = new Student
            {
                SchoolId = schoolId,
                AcademicYearId = model.AcademicYearId,
                ClassId = model.ClassId,
                SectionId = model.SectionId,
                AdmissionNumber = model.AdmissionNumber.Trim(),
                RollNumber = finalRollNumber,
                Name = model.Name.Trim(),
                Photo = photoPath,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                FatherName = model.FatherName.Trim(),
                FatherMobile = model.FatherMobile.Trim(),
                MotherName = model.MotherName.Trim(),
                MotherMobile = model.MotherMobile?.Trim(),
                GuardianName = model.GuardianName?.Trim(),
                Email = model.Email?.Trim(),
                Address = model.Address.Trim(),
                City = string.IsNullOrWhiteSpace(model.City) ? "Piro" : model.City.Trim(),
                State = string.IsNullOrWhiteSpace(model.State) ? "Bihar" : model.State.Trim(),
                PinCode = string.IsNullOrWhiteSpace(model.PinCode) ? "802207" : model.PinCode.Trim(),
                AdmissionDate = model.AdmissionDate,
                Status = StudentStatus.Active,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("DEO_Admission", "Student", student.Id.ToString(), $"DEO Admitted student {student.Name} (Adm No: {student.AdmissionNumber}) to Class {student.ClassId}-{student.SectionId}");

            TempData["SuccessMessage"] = $"Student '{student.Name}' admitted successfully with Adm No: {student.AdmissionNumber}!";

            if (submitAction == "SaveAndPrint")
            {
                return RedirectToAction(nameof(Slip), new { id = student.Id });
            }
            if (submitAction == "SaveAndNew")
            {
                return RedirectToAction(nameof(Admission), new { classId = model.ClassId, sectionId = model.SectionId });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DEO/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId && s.IsActive);

            if (student == null) return NotFound();

            var model = new DEOAdmissionViewModel
            {
                Id = student.Id,
                SchoolId = student.SchoolId,
                AcademicYearId = student.AcademicYearId,
                ClassId = student.ClassId,
                SectionId = student.SectionId,
                AdmissionNumber = student.AdmissionNumber,
                RollNumber = student.RollNumber,
                Name = student.Name,
                ExistingPhoto = student.Photo,
                DateOfBirth = student.DateOfBirth,
                Gender = student.Gender,
                BloodGroup = student.BloodGroup,
                FatherName = student.FatherName,
                FatherMobile = student.FatherMobile,
                MotherName = student.MotherName,
                MotherMobile = student.MotherMobile,
                GuardianName = student.GuardianName,
                Email = student.Email,
                Address = student.Address,
                City = student.City,
                State = student.State,
                PinCode = student.PinCode,
                AdmissionDate = student.AdmissionDate,
                Status = student.Status
            };

            await PopulateDropdowns(model, schoolId);
            return View(model);
        }

        // POST: DEO/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DEOAdmissionViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            if (id != model.Id) return BadRequest();

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId && s.IsActive);
            if (student == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model, schoolId);
                return View(model);
            }

            // Handle Photo replacement
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                student.Photo = await _fileService.UploadFileAsync(model.PhotoFile, "uploads/students");
            }

            student.AcademicYearId = model.AcademicYearId;
            student.ClassId = model.ClassId;
            student.SectionId = model.SectionId;
            student.RollNumber = model.RollNumber;
            student.Name = model.Name.Trim();
            student.DateOfBirth = model.DateOfBirth;
            student.Gender = model.Gender;
            student.BloodGroup = model.BloodGroup;
            student.FatherName = model.FatherName.Trim();
            student.FatherMobile = model.FatherMobile.Trim();
            student.MotherName = model.MotherName.Trim();
            student.MotherMobile = model.MotherMobile?.Trim();
            student.GuardianName = model.GuardianName?.Trim();
            student.Email = model.Email?.Trim();
            student.Address = model.Address.Trim();
            student.City = model.City.Trim();
            student.State = model.State.Trim();
            student.PinCode = model.PinCode.Trim();
            student.Status = model.Status;
            student.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("DEO_Edit", "Student", student.Id.ToString(), $"DEO Updated details for student {student.Name}");

            TempData["SuccessMessage"] = $"Student '{student.Name}' details updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: DEO/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId);
            if (student != null)
            {
                student.IsActive = false;
                student.Status = StudentStatus.LeftSchool;
                student.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("DEO_Delete", "Student", student.Id.ToString(), $"DEO Marked student {student.Name} as inactive/left");
                TempData["SuccessMessage"] = $"Student '{student.Name}' removed from active registry.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DEO/Slip/{id} (Printable Official Admission Slip)
        [HttpGet]
        public async Task<IActionResult> Slip(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Include(s => s.AcademicYear)
                .Include(s => s.School)
                .FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == schoolId);

            if (student == null) return NotFound();

            return View(student);
        }

        // AJAX Helper: Get Sections for a Class
        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections = await _context.Sections
                .Where(s => s.ClassId == classId && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name, s.Capacity })
                .ToListAsync();

            return Json(sections);
        }

        // AJAX Helper: Get Next Roll Number in a Section
        [HttpGet]
        public async Task<IActionResult> GetNextRoll(int sectionId)
        {
            var maxRoll = await _context.Students
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .MaxAsync(s => (int?)s.RollNumber) ?? 0;

            return Json(new { nextRoll = maxRoll + 1 });
        }

        // GET: DEO/Teachers (Faculty Directory & Login Accounts Desk)
        [HttpGet]
        public async Task<IActionResult> Teachers(string? search)
        {
            var schoolId = GetEffectiveSchoolId();
            var teachersQuery = _context.Teachers
                .Include(t => t.ClassTeacherSections)
                    .ThenInclude(ct => ct.Class)
                .Where(t => t.SchoolId == schoolId && t.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                teachersQuery = teachersQuery.Where(t => 
                    t.Name.ToLower().Contains(term) || 
                    t.Designation.ToLower().Contains(term) || 
                    t.Email.ToLower().Contains(term) || 
                    t.Mobile.Contains(term) ||
                    t.EmployeeId.ToLower().Contains(term) ||
                    (t.Subject != null && t.Subject.ToLower().Contains(term)));
            }

            var teachers = await teachersQuery.OrderBy(t => t.Name).ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .ToListAsync();

            var assignedCount = sections.Count(s => s.ClassTeacherId.HasValue);

            var model = new DEOTeachersViewModel
            {
                Teachers = teachers,
                Sections = sections,
                TotalTeachers = teachers.Count,
                AssignedClassTeachersCount = assignedCount,
                SearchTerm = search
            };

            return View(model);
        }

        // GET: DEO/ClassTeachers (Dedicated Class Teacher Allocation Matrix)
        [HttpGet]
        public async Task<IActionResult> ClassTeachers(string? search)
        {
            var schoolId = GetEffectiveSchoolId();

            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();

            var sectionsQuery = _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Include(s => s.Students)
                .Where(s => s.SchoolId == schoolId && s.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                sectionsQuery = sectionsQuery.Where(s => 
                    s.Name.ToLower().Contains(term) ||
                    (s.Class != null && s.Class.Name.ToLower().Contains(term)) ||
                    (s.ClassTeacher != null && s.ClassTeacher.Name.ToLower().Contains(term)));
            }

            var sections = await sectionsQuery
                .OrderBy(s => s.Class.DisplayOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();

            var assignedCount = sections.Count(s => s.ClassTeacherId.HasValue);

            var model = new DEOTeachersViewModel
            {
                Teachers = teachers,
                Sections = sections,
                TotalTeachers = teachers.Count,
                AssignedClassTeachersCount = assignedCount,
                SearchTerm = search
            };

            return View(model);
        }

        // POST: DEO/AssignClassTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignClassTeacher(int sectionId, int? teacherId, string? returnUrl)
        {
            var schoolId = GetEffectiveSchoolId();
            var section = await _context.Sections
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.SchoolId == schoolId);

            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToAction(nameof(ClassTeachers));
            }

            section.ClassTeacherId = teacherId;
            await _context.SaveChangesAsync();

            string teacherName = "Unassigned";
            if (teacherId.HasValue)
            {
                var teacher = await _context.Teachers.FindAsync(teacherId.Value);
                if (teacher != null) teacherName = teacher.Name;
            }

            await _auditService.LogAsync("Assign Class Teacher", "Section", section.Id.ToString(), $"Assigned {teacherName} as Class Teacher of {section.Class?.Name} - Section {section.Name}", schoolId);
            TempData["SuccessMessage"] = $"Class Teacher updated for {section.Class?.Name} - Section {section.Name} to {teacherName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(ClassTeachers));
        }

        // POST: DEO/CreateTeacherAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacherAccount(int teacherId, string? customUsername, string? customPassword)
        {
            var schoolId = GetEffectiveSchoolId();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == teacherId && t.SchoolId == schoolId);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction(nameof(Teachers));
            }

            var cleanName = teacher.Name.Split(' ').FirstOrDefault()?.ToLower() ?? "teacher";
            var username = !string.IsNullOrWhiteSpace(customUsername) 
                ? customUsername.Trim().ToLower() 
                : $"teacher.{cleanName}";
            var password = !string.IsNullOrWhiteSpace(customPassword) ? customPassword.Trim() : "Teacher@123";
            var email = !string.IsNullOrWhiteSpace(teacher.Email) ? teacher.Email : $"{username}@schoolmanagement.com";

            // Check if user already exists
            var existingUser = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var resetResult = await _userManager.ResetPasswordAsync(existingUser, token, password);
                if (resetResult.Succeeded)
                {
                    teacher.UserId = existingUser.Id;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"✅ Login updated for {teacher.Name}! Username: {existingUser.UserName} | Password: {password}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Failed to reset password: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}";
                }
                return RedirectToAction(nameof(Teachers));
            }

            // Create new Identity User
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FullName = teacher.Name,
                SchoolId = schoolId,
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Teacher");
                await _userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                if (user.SchoolId.HasValue)
                {
                    await _userManager.AddClaimAsync(user, new Claim("SchoolId", user.SchoolId.Value.ToString()));
                }

                teacher.UserId = user.Id;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("Create Teacher Login", "Teacher", teacher.Id.ToString(), $"DEO created login credentials for {teacher.Name} (Username: {username})", schoolId);

                TempData["SuccessMessage"] = $"🎉 Login Account Created for {teacher.Name}! Username: {username} | Password: {password} (Role: Teacher)";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to create login: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Teachers));
        }

        // POST: DEO/GenerateAllTeacherAccounts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAllTeacherAccounts()
        {
            var schoolId = GetEffectiveSchoolId();
            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active)
                .ToListAsync();

            int createdCount = 0;
            foreach (var t in teachers)
            {
                var cleanName = t.Name.Split(' ').FirstOrDefault()?.ToLower() ?? "teacher";
                var username = $"teacher.{cleanName}_{t.Id}";
                var email = !string.IsNullOrWhiteSpace(t.Email) ? t.Email : $"{username}@schoolmanagement.com";

                var existingUser = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FullName = t.Name,
                        SchoolId = schoolId,
                        EmailConfirmed = true,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, "Teacher@123");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Teacher");
                        await _userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                        await _userManager.AddClaimAsync(user, new Claim("SchoolId", schoolId.ToString()));
                        t.UserId = user.Id;
                        createdCount++;
                    }
                }
                else
                {
                    t.UserId = existingUser.Id;
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Batch Teacher Accounts", "Teacher", "All", $"DEO batch generated {createdCount} teacher login accounts", schoolId);

            TempData["SuccessMessage"] = $"✅ Batch processing complete: {createdCount} new teacher login accounts generated with default password 'Teacher@123'!";
            return RedirectToAction(nameof(Teachers));
        }

        private async Task PopulateDropdowns(DEOAdmissionViewModel model, int schoolId)
        {
            var academicYears = await _context.AcademicYears
                .Where(a => a.SchoolId == schoolId && a.IsActive)
                .ToListAsync();

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var sections = await _context.Sections
                .Where(s => s.ClassId == model.ClassId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            model.AcademicYears = new SelectList(academicYears, "Id", "Name", model.AcademicYearId);
            model.Classes = new SelectList(classes, "Id", "Name", model.ClassId);
            model.Sections = new SelectList(sections, "Id", "Name", model.SectionId);
        }
    }
}
