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
    public class TeacherController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly ApplicationDbContext _context;

        public TeacherController(ITeacherService teacherService, ApplicationDbContext context)
        {
            _teacherService = teacherService;
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

        public async Task<IActionResult> Index(string? search, TeacherStatus? status, int page = 1)
        {
            var schoolId = GetEffectiveSchoolId();
            var model = await _teacherService.GetTeachersAsync(schoolId, search, status, page, 15);
            return View(model);
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Create()
        {
            var schoolId = GetEffectiveSchoolId();
            var vm = new TeacherCreateEditViewModel { SchoolId = schoolId };
            await PopulateSectionsAsync(vm, schoolId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateSectionsAsync(model, schoolId);
                return View(model);
            }

            // Check duplicate Employee ID
            var exists = await _context.Teachers.AnyAsync(t => t.SchoolId == schoolId && t.EmployeeId.ToLower() == model.EmployeeId.Trim().ToLower());
            if (exists)
            {
                ModelState.AddModelError("EmployeeId", "A teacher with this Employee ID already exists in this school.");
                await PopulateSectionsAsync(model, schoolId);
                return View(model);
            }

            try
            {
                var teacher = await _teacherService.CreateTeacherAsync(model, User.Identity?.Name ?? "Admin");
                TempData["SuccessMessage"] = $"Teacher '{teacher.Name}' registered successfully!";
                return RedirectToAction(nameof(Details), new { id = teacher.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSectionsAsync(model, schoolId);
                return View(model);
            }
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Edit(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var teacher = await _teacherService.GetTeacherByIdAsync(id, schoolId);
            if (teacher == null) return NotFound();

            var currentAssignedSection = await _context.Sections
                .FirstOrDefaultAsync(s => s.ClassTeacherId == teacher.Id && s.SchoolId == schoolId);

            var vm = new TeacherCreateEditViewModel
            {
                Id = teacher.Id,
                SchoolId = teacher.SchoolId,
                EmployeeId = teacher.EmployeeId,
                Name = teacher.Name,
                ExistingPhoto = teacher.Photo,
                Gender = teacher.Gender,
                DateOfBirth = teacher.DateOfBirth,
                Qualification = teacher.Qualification,
                Experience = teacher.Experience,
                Subject = teacher.Subject,
                Designation = teacher.Designation,
                Mobile = teacher.Mobile,
                Email = teacher.Email,
                Address = teacher.Address,
                JoiningDate = teacher.JoiningDate,
                Status = teacher.Status,
                AssignedSectionId = currentAssignedSection?.Id
            };

            await PopulateSectionsAsync(vm, schoolId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TeacherCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateSectionsAsync(model, schoolId);
                return View(model);
            }

            var updated = await _teacherService.UpdateTeacherAsync(model, User.Identity?.Name ?? "Admin");
            if (updated == null) return NotFound();

            TempData["SuccessMessage"] = $"Teacher '{updated.Name}' updated successfully!";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var profile = await _teacherService.GetTeacherProfileAsync(id, schoolId);
            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var result = await _teacherService.DeleteTeacherAsync(id, schoolId);
            if (result)
                TempData["SuccessMessage"] = "Teacher marked as Resigned/Inactive.";
            else
                TempData["ErrorMessage"] = "Unable to delete teacher.";

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateSectionsAsync(TeacherCreateEditViewModel model, int schoolId)
        {
            var sections = await _context.Sections
                .Include(s => s.Class)
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .OrderBy(s => s.Class.DisplayOrder).ThenBy(s => s.Name)
                .Select(s => new { s.Id, DisplayName = $"{s.Class.Name} - Section {s.Name}" })
                .ToListAsync();

            model.Sections = new SelectList(sections, "Id", "DisplayName", model.AssignedSectionId);
        }
    }
}
