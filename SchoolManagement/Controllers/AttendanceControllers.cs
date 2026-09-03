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
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;

        public AttendanceController(IAttendanceService attendanceService, ApplicationDbContext context)
        {
            _attendanceService = attendanceService;
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

        [HttpGet]
        public async Task<IActionResult> StudentAttendance(int? classId, int? sectionId, DateTime? date, int? academicYearId)
        {
            var schoolId = GetEffectiveSchoolId();
            var targetDate = date ?? DateTime.Today;

            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.SchoolId == schoolId && (academicYearId.HasValue ? a.Id == academicYearId.Value : a.IsCurrent));

            if (currentYear == null)
            {
                TempData["ErrorMessage"] = "Please create and activate an Academic Year first.";
                return RedirectToAction("Index", "AcademicYear");
            }

            var classes = await _context.Classes.Where(c => c.SchoolId == schoolId && c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            var selectedClass = classId.HasValue ? classes.FirstOrDefault(c => c.Id == classId.Value) : classes.FirstOrDefault();

            StudentAttendanceSheetViewModel model;

            if (selectedClass != null)
            {
                var sections = await _context.Sections.Where(s => s.ClassId == selectedClass.Id && s.IsActive).OrderBy(s => s.Name).ToListAsync();
                var selectedSection = sectionId.HasValue ? sections.FirstOrDefault(s => s.Id == sectionId.Value) : sections.FirstOrDefault();

                if (selectedSection != null)
                {
                    model = await _attendanceService.GetStudentAttendanceSheetAsync(schoolId, selectedClass.Id, selectedSection.Id, targetDate, currentYear.Id);
                }
                else
                {
                    model = new StudentAttendanceSheetViewModel
                    {
                        SchoolId = schoolId,
                        ClassId = selectedClass.Id,
                        AttendanceDate = targetDate,
                        AcademicYearId = currentYear.Id
                    };
                }

                model.Sections = new SelectList(sections, "Id", "Name", selectedSection?.Id);
            }
            else
            {
                model = new StudentAttendanceSheetViewModel
                {
                    SchoolId = schoolId,
                    AttendanceDate = targetDate,
                    AcademicYearId = currentYear.Id
                };
            }

            model.Classes = new SelectList(classes, "Id", "Name", selectedClass?.Id);
            var academicYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync();
            model.AcademicYears = new SelectList(academicYears, "Id", "Name", currentYear.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStudentAttendance(StudentAttendanceSheetViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            var result = await _attendanceService.SaveStudentAttendanceAsync(model, User.Identity?.Name ?? "User");
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(StudentAttendance), new
            {
                classId = model.ClassId,
                sectionId = model.SectionId,
                date = model.AttendanceDate.ToString("yyyy-MM-dd"),
                academicYearId = model.AcademicYearId
            });
        }

        [HttpPost]
        public async Task<JsonResult> SaveStudentAttendanceAjax([FromBody] StudentAttendanceSheetViewModel model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid data payload." });

            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            var result = await _attendanceService.SaveStudentAttendanceAsync(model, User.Identity?.Name ?? "User");
            return Json(new { success = result.Success, message = result.Message, count = result.Count });
        }

        [HttpGet]
        public async Task<IActionResult> DailyReport(DateTime? date)
        {
            var schoolId = GetEffectiveSchoolId();
            var targetDate = date ?? DateTime.Today;
            var summary = await _attendanceService.GetDailySummaryAsync(schoolId, targetDate);
            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> Analytics(AttendanceAnalyticsViewModel? filter)
        {
            var schoolId = GetEffectiveSchoolId();
            filter ??= new AttendanceAnalyticsViewModel();
            filter.SchoolId = schoolId;

            if (filter.DateFrom == default) filter.DateFrom = DateTime.Today.AddDays(-30);
            if (filter.DateTo == default) filter.DateTo = DateTime.Today;

            var result = await _attendanceService.GetAttendanceAnalyticsAsync(filter);

            var classes = await _context.Classes.Where(c => c.SchoolId == schoolId && c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            result.Classes = new SelectList(classes, "Id", "Name", filter.ClassId);

            if (filter.ClassId.HasValue)
            {
                var sections = await _context.Sections.Where(s => s.ClassId == filter.ClassId.Value && s.IsActive).OrderBy(s => s.Name).ToListAsync();
                result.Sections = new SelectList(sections, "Id", "Name", filter.SectionId);
            }

            return View(result);
        }
    }

    [Authorize]
    public class TeacherAttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ApplicationDbContext _context;

        public TeacherAttendanceController(IAttendanceService attendanceService, ApplicationDbContext context)
        {
            _attendanceService = attendanceService;
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

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date, int? academicYearId)
        {
            var schoolId = GetEffectiveSchoolId();
            var targetDate = date ?? DateTime.Today;

            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.SchoolId == schoolId && (academicYearId.HasValue ? a.Id == academicYearId.Value : a.IsCurrent));

            if (currentYear == null)
            {
                TempData["ErrorMessage"] = "Please create and activate an Academic Year first.";
                return RedirectToAction("Index", "AcademicYear");
            }

            var model = await _attendanceService.GetTeacherAttendanceSheetAsync(schoolId, targetDate, currentYear.Id);

            var academicYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync();
            model.AcademicYears = new SelectList(academicYears, "Id", "Name", currentYear.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(TeacherAttendanceSheetViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            var result = await _attendanceService.SaveTeacherAttendanceAsync(model, User.Identity?.Name ?? "User");
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new
            {
                date = model.AttendanceDate.ToString("yyyy-MM-dd"),
                academicYearId = model.AcademicYearId
            });
        }

        [HttpPost]
        public async Task<JsonResult> SaveAjax([FromBody] TeacherAttendanceSheetViewModel model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid data." });

            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            var result = await _attendanceService.SaveTeacherAttendanceAsync(model, User.Identity?.Name ?? "User");
            return Json(new { success = result.Success, message = result.Message, count = result.Count });
        }
    }
}
