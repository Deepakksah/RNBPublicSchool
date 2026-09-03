using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IClassService _classService;
        private readonly ApplicationDbContext _context;

        public StudentController(
            IStudentService studentService,
            IClassService classService,
            ApplicationDbContext context)
        {
            _studentService = studentService;
            _classService = classService;
            _context = context;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;

            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            if (sessionSchoolId.HasValue) return sessionSchoolId.Value;

            return _context.Schools.FirstOrDefault()?.Id ?? 1;
        }

        public async Task<IActionResult> Index(int? classId, int? sectionId, string? search, StudentStatus? status, int page = 1)
        {
            var schoolId = GetEffectiveSchoolId();
            var model = await _studentService.GetStudentsAsync(schoolId, classId, sectionId, search, status, page, 15);

            var classes = await _context.Classes.Where(c => c.SchoolId == schoolId && c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            model.Classes = new SelectList(classes, "Id", "Name", classId);

            if (classId.HasValue)
            {
                var sections = await _context.Sections.Where(s => s.ClassId == classId.Value && s.IsActive).OrderBy(s => s.Name).ToListAsync();
                model.Sections = new SelectList(sections, "Id", "Name", sectionId);
            }

            return View(model);
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Create()
        {
            var schoolId = GetEffectiveSchoolId();
            var vm = new StudentCreateEditViewModel { SchoolId = schoolId };
            await PopulateDropDownsAsync(vm, schoolId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model, schoolId);
                return View(model);
            }

            // Check duplicate admission number within school
            var exists = await _context.Students.AnyAsync(s => s.SchoolId == schoolId && s.AdmissionNumber.ToLower() == model.AdmissionNumber.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError("AdmissionNumber", "A student with this admission number already exists in this school.");
                await PopulateDropDownsAsync(model, schoolId);
                return View(model);
            }

            try
            {
                var student = await _studentService.CreateStudentAsync(model, User.Identity?.Name ?? "Admin");
                TempData["SuccessMessage"] = $"Student '{student.Name}' admitted successfully!";
                return RedirectToAction(nameof(Details), new { id = student.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateDropDownsAsync(model, schoolId);
                return View(model);
            }
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Edit(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var student = await _studentService.GetStudentByIdAsync(id, schoolId);
            if (student == null) return NotFound();

            var vm = new StudentCreateEditViewModel
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
                MotherName = student.MotherName,
                GuardianName = student.GuardianName,
                FatherMobile = student.FatherMobile,
                MotherMobile = student.MotherMobile,
                Email = student.Email,
                Address = student.Address,
                City = student.City,
                State = student.State,
                PinCode = student.PinCode,
                AdmissionDate = student.AdmissionDate,
                Status = student.Status
            };

            await PopulateDropDownsAsync(vm, schoolId, student.ClassId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StudentCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model, schoolId, model.ClassId);
                return View(model);
            }

            var updated = await _studentService.UpdateStudentAsync(model, User.Identity?.Name ?? "Admin");
            if (updated == null) return NotFound();

            TempData["SuccessMessage"] = $"Student '{updated.Name}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var profile = await _studentService.GetStudentProfileAsync(id, schoolId);
            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var result = await _studentService.DeleteStudentAsync(id, schoolId);
            if (result)
                TempData["SuccessMessage"] = "Student record marked as Left/Inactive.";
            else
                TempData["ErrorMessage"] = "Unable to delete student.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetSectionsByClass(int classId)
        {
            var schoolId = GetEffectiveSchoolId();
            var sections = await _classService.GetSectionsByClassAsync(classId, schoolId);
            return Json(sections.Select(s => new { id = s.Id, name = s.Name }));
        }

        private async Task PopulateDropDownsAsync(StudentCreateEditViewModel model, int schoolId, int? selectedClassId = null)
        {
            var academicYears = await _context.AcademicYears
                .Where(a => a.SchoolId == schoolId && a.IsActive)
                .OrderByDescending(a => a.IsCurrent)
                .ToListAsync();

            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            model.AcademicYears = new SelectList(academicYears, "Id", "Name", model.AcademicYearId);
            model.Classes = new SelectList(classes, "Id", "Name", model.ClassId);

            var classToUse = selectedClassId ?? model.ClassId;
            if (classToUse == 0 && classes.Count > 0) classToUse = classes[0].Id;

            var sections = await _context.Sections
                .Where(s => s.ClassId == classToUse && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            model.Sections = new SelectList(sections, "Id", "Name", model.SectionId);
        }
    }
}
