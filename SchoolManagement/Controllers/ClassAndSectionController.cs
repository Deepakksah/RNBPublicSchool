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
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly ApplicationDbContext _context;

        public ClassController(IClassService classService, ApplicationDbContext context)
        {
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

        public async Task<IActionResult> Index()
        {
            var schoolId = GetEffectiveSchoolId();
            var classes = await _classService.GetClassesWithSectionsAsync(schoolId);
            return View(classes);
        }

        [Authorize(Roles = "Super Admin,School Admin")]
        public IActionResult Create()
        {
            var schoolId = GetEffectiveSchoolId();
            return View(new ClassCreateEditViewModel { SchoolId = schoolId });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid) return View(model);

            await _classService.CreateClassAsync(model);
            TempData["SuccessMessage"] = $"Class '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Super Admin,School Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var entity = await _classService.GetClassByIdAsync(id, schoolId);
            if (entity == null) return NotFound();

            var vm = new ClassCreateEditViewModel
            {
                Id = entity.Id,
                SchoolId = entity.SchoolId,
                Name = entity.Name,
                DisplayOrder = entity.DisplayOrder
            };
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid) return View(model);

            var updated = await _classService.UpdateClassAsync(model);
            if (updated == null) return NotFound();

            TempData["SuccessMessage"] = $"Class '{model.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var success = await _classService.DeleteClassAsync(id, schoolId);
            if (success) TempData["SuccessMessage"] = "Class deleted successfully.";
            else TempData["ErrorMessage"] = "Unable to delete class.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Dashboard(int classId, int sectionId, DateTime? date)
        {
            var schoolId = GetEffectiveSchoolId();
            var targetDate = date ?? DateTime.Today;

            var model = await _classService.GetClassDashboardAsync(classId, sectionId, targetDate, schoolId);
            if (model == null) return NotFound();

            return View(model);
        }
    }

    [Authorize]
    public class SectionController : Controller
    {
        private readonly IClassService _classService;
        private readonly ApplicationDbContext _context;

        public SectionController(IClassService classService, ApplicationDbContext context)
        {
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

        [Authorize(Roles = "Super Admin,School Admin")]
        public async Task<IActionResult> Create(int? classId)
        {
            var schoolId = GetEffectiveSchoolId();
            var vm = new SectionCreateEditViewModel
            {
                SchoolId = schoolId,
                ClassId = classId ?? 0
            };

            await PopulateDropDownsAsync(vm, schoolId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SectionCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model, schoolId);
                return View(model);
            }

            await _classService.CreateSectionAsync(model);
            TempData["SuccessMessage"] = $"Section '{model.Name}' created successfully.";
            return RedirectToAction("Index", "Class");
        }

        [Authorize(Roles = "Super Admin,School Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var section = await _classService.GetSectionByIdAsync(id, schoolId);
            if (section == null) return NotFound();

            var vm = new SectionCreateEditViewModel
            {
                Id = section.Id,
                SchoolId = section.SchoolId,
                ClassId = section.ClassId,
                Name = section.Name,
                RoomNumber = section.RoomNumber,
                Capacity = section.Capacity,
                ClassTeacherId = section.ClassTeacherId
            };

            await PopulateDropDownsAsync(vm, schoolId);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SectionCreateEditViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                await PopulateDropDownsAsync(model, schoolId);
                return View(model);
            }

            var updated = await _classService.UpdateSectionAsync(model);
            if (updated == null) return NotFound();

            TempData["SuccessMessage"] = $"Section '{model.Name}' updated successfully.";
            return RedirectToAction("Index", "Class");
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var success = await _classService.DeleteSectionAsync(id, schoolId);
            if (success) TempData["SuccessMessage"] = "Section deleted successfully.";
            else TempData["ErrorMessage"] = "Unable to delete section.";

            return RedirectToAction("Index", "Class");
        }

        private async Task PopulateDropDownsAsync(SectionCreateEditViewModel model, int schoolId)
        {
            var classes = await _context.Classes
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active)
                .OrderBy(t => t.Name)
                .ToListAsync();

            model.Classes = new SelectList(classes, "Id", "Name", model.ClassId);
            model.Teachers = new SelectList(teachers, "Id", "Name", model.ClassTeacherId);
        }
    }
}
