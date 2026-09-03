using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Extensions;
using SchoolManagement.Services;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IExportService _exportService;
        private readonly ApplicationDbContext _context;

        public ReportsController(
            IReportService reportService,
            IExportService exportService,
            ApplicationDbContext context)
        {
            _reportService = reportService;
            _exportService = exportService;
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
        public async Task<IActionResult> StudentAttendance(ReportFilterViewModel? filter, int page = 1)
        {
            var schoolId = GetEffectiveSchoolId();
            filter ??= new ReportFilterViewModel();
            filter.SchoolId = schoolId;
            filter.ReportType = "StudentAttendance";

            var items = await _reportService.GetStudentAttendanceReportAsync(filter);
            
            int pageSize = 25;
            int totalCount = items.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int pageIndex = Math.Max(1, page);

            var pagedItems = items
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ReportData = pagedItems;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = Math.Max(1, totalPages);
            ViewBag.TotalCount = totalCount;

            await PopulateFilterDropDownsAsync(filter, schoolId);

            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> TeacherAttendance(ReportFilterViewModel? filter)
        {
            var schoolId = GetEffectiveSchoolId();
            filter ??= new ReportFilterViewModel();
            filter.SchoolId = schoolId;
            filter.ReportType = "TeacherAttendance";

            var items = await _reportService.GetTeacherAttendanceReportAsync(filter);
            ViewBag.ReportData = items;
            await PopulateFilterDropDownsAsync(filter, schoolId);

            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> ClassAttendance(ReportFilterViewModel? filter)
        {
            var schoolId = GetEffectiveSchoolId();
            filter ??= new ReportFilterViewModel();
            filter.SchoolId = schoolId;
            filter.ReportType = "ClassAttendance";

            var items = await _reportService.GetClassAttendanceReportAsync(filter);
            ViewBag.ReportData = items;
            await PopulateFilterDropDownsAsync(filter, schoolId);

            return View(filter);
        }

        [HttpGet]
        public async Task<IActionResult> SchoolSummary(int? academicYearId)
        {
            var schoolId = GetEffectiveSchoolId();
            var report = await _reportService.GetSchoolSummaryReportAsync(schoolId, academicYearId);
            if (report == null) return NotFound();

            var academicYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync();
            ViewBag.AcademicYears = new SelectList(academicYears, "Id", "Name", academicYearId);

            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string reportType, int? classId, int? sectionId, DateTime dateFrom, DateTime dateTo)
        {
            var schoolId = GetEffectiveSchoolId();
            var filter = new ReportFilterViewModel
            {
                SchoolId = schoolId,
                ClassId = classId,
                SectionId = sectionId,
                DateFrom = dateFrom == default ? DateTime.Today.AddDays(-30) : dateFrom,
                DateTo = dateTo == default ? DateTime.Today : dateTo
            };

            if (reportType == "TeacherAttendance")
            {
                var data = await _reportService.GetTeacherAttendanceReportAsync(filter);
                var excelBytes = _exportService.ExportToExcel(data, "TeacherAttendance");
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"TeacherAttendance_{DateTime.Today:yyyyMMdd}.xlsx");
            }
            else if (reportType == "ClassAttendance")
            {
                var data = await _reportService.GetClassAttendanceReportAsync(filter);
                var excelBytes = _exportService.ExportToExcel(data, "ClassAttendance");
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ClassAttendance_{DateTime.Today:yyyyMMdd}.xlsx");
            }
            else
            {
                var data = await _reportService.GetStudentAttendanceReportAsync(filter);
                var excelBytes = _exportService.ExportToExcel(data, "StudentAttendance");
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"StudentAttendance_{DateTime.Today:yyyyMMdd}.xlsx");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string reportType, int? classId, int? sectionId, DateTime dateFrom, DateTime dateTo)
        {
            var schoolId = GetEffectiveSchoolId();
            var filter = new ReportFilterViewModel
            {
                SchoolId = schoolId,
                ClassId = classId,
                SectionId = sectionId,
                DateFrom = dateFrom == default ? DateTime.Today.AddDays(-30) : dateFrom,
                DateTo = dateTo == default ? DateTime.Today : dateTo
            };

            if (reportType == "TeacherAttendance")
            {
                var data = await _reportService.GetTeacherAttendanceReportAsync(filter);
                var csvBytes = _exportService.ExportToCsv(data);
                return File(csvBytes, "text/csv", $"TeacherAttendance_{DateTime.Today:yyyyMMdd}.csv");
            }
            else if (reportType == "ClassAttendance")
            {
                var data = await _reportService.GetClassAttendanceReportAsync(filter);
                var csvBytes = _exportService.ExportToCsv(data);
                return File(csvBytes, "text/csv", $"ClassAttendance_{DateTime.Today:yyyyMMdd}.csv");
            }
            else
            {
                var data = await _reportService.GetStudentAttendanceReportAsync(filter);
                var csvBytes = _exportService.ExportToCsv(data);
                return File(csvBytes, "text/csv", $"StudentAttendance_{DateTime.Today:yyyyMMdd}.csv");
            }
        }

        private async Task PopulateFilterDropDownsAsync(ReportFilterViewModel filter, int schoolId)
        {
            var classes = await _context.Classes.Where(c => c.SchoolId == schoolId && c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync();
            filter.Classes = new SelectList(classes, "Id", "Name", filter.ClassId);

            if (filter.ClassId.HasValue)
            {
                var sections = await _context.Sections.Where(s => s.ClassId == filter.ClassId.Value && s.IsActive).OrderBy(s => s.Name).ToListAsync();
                filter.Sections = new SelectList(sections, "Id", "Name", filter.SectionId);
            }

            var teachers = await _context.Teachers.Where(t => t.SchoolId == schoolId && t.IsActive).OrderBy(t => t.Name).ToListAsync();
            filter.Teachers = new SelectList(teachers, "Id", "Name", filter.TeacherId);
        }
    }
}
