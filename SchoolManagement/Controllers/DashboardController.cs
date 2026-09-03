using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Extensions;
using SchoolManagement.Models;
using SchoolManagement.Services;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISchoolService _schoolService;
        private readonly ApplicationDbContext _context;

        public DashboardController(IDashboardService dashboardService, ISchoolService schoolService, ApplicationDbContext context)
        {
            _dashboardService = dashboardService;
            _schoolService = schoolService;
            _context = context;
        }

        public async Task<IActionResult> Index(int? schoolId = null)
        {
            var isSuperAdmin = User.IsSuperAdmin();
            var userSchoolId = User.GetSchoolId();
            var isTeacher = User.IsTeacher();

            // SCOPED CLASSROOM VIEW FOR TEACHER / FACULTY
            if (isTeacher)
            {
                var userName = User.Identity?.Name ?? "";
                var teacher = await _context.Teachers
                    .Include(t => t.ClassTeacherSections)
                        .ThenInclude(s => s.Class)
                    .Include(t => t.ClassTeacherSections)
                        .ThenInclude(s => s.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                    .FirstOrDefaultAsync(t => (t.UserId == userName || t.Email == userName || t.Mobile == userName) && t.IsActive);

                if (teacher == null)
                {
                    // Fallback to first active teacher in school for demo/preview
                    var effectiveSchoolId = userSchoolId ?? 4;
                    teacher = await _context.Teachers
                        .Include(t => t.ClassTeacherSections)
                            .ThenInclude(s => s.Class)
                        .Include(t => t.ClassTeacherSections)
                            .ThenInclude(s => s.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                        .FirstOrDefaultAsync(t => t.SchoolId == effectiveSchoolId && t.IsActive);
                }

                if (teacher != null)
                {
                    var assignedSection = teacher.ClassTeacherSections.FirstOrDefault();
                    var today = DateTime.Today;
                    var students = assignedSection != null
                        ? await _context.Students
                            .Where(s => s.SectionId == assignedSection.Id && s.IsActive && s.Status == StudentStatus.Active)
                            .OrderBy(s => s.RollNumber)
                            .ToListAsync()
                        : new List<Student>();

                    var todayAtt = assignedSection != null
                        ? await _context.StudentAttendances
                            .Where(a => a.SectionId == assignedSection.Id && a.AttendanceDate == today)
                            .ToListAsync()
                        : new List<StudentAttendance>();

                    var p = todayAtt.Count(a => a.Status == AttendanceStatus.Present);
                    var ab = todayAtt.Count(a => a.Status == AttendanceStatus.Absent);
                    var l = todayAtt.Count(a => a.Status == AttendanceStatus.Leave);
                    var lt = todayAtt.Count(a => a.Status == AttendanceStatus.Late);
                    var total = students.Count;
                    var rate = total > 0 ? Math.Round((double)p / total * 100, 1) : 0.0;

                    var notices = await _context.Notifications
                        .Where(n => n.IsActive)
                        .OrderByDescending(n => n.PublishDate)
                        .Take(5)
                        .ToListAsync();

                    var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                    var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                    var holidays = await _context.Holidays
                        .Where(h => h.IsActive && h.HolidayDate >= firstDayOfMonth && h.HolidayDate <= lastDayOfMonth)
                        .OrderBy(h => h.HolidayDate)
                        .ToListAsync();

                    if (!holidays.Any())
                    {
                        holidays = await _context.Holidays
                            .Where(h => h.IsActive && h.HolidayDate >= today)
                            .OrderBy(h => h.HolidayDate)
                            .Take(5)
                            .ToListAsync();
                    }

                    var teacherModel = new TeacherClassroomViewModel
                    {
                        Teacher = teacher,
                        AssignedSection = assignedSection,
                        TotalStudents = total,
                        TodayPresent = p,
                        TodayAbsent = ab,
                        TodayLeave = l,
                        TodayLate = lt,
                        TodayAttendancePercentage = rate,
                        IsAttendanceMarkedToday = todayAtt.Any(),
                        Students = students,
                        RecentNotices = notices,
                        UpcomingHolidays = holidays
                    };

                    return View("TeacherClassroom", teacherModel);
                }
            }

            // If user is Super Admin and has NOT chosen a specific school, render the global Multi-School Super Admin Dashboard
            if (isSuperAdmin && !schoolId.HasValue && !userSchoolId.HasValue)
            {
                var superModel = await _dashboardService.GetSuperAdminDashboardAsync();
                ViewBag.IsSuperAdminOverview = true;
                ViewBag.AllSchools = await _schoolService.GetAllSchoolsAsync();
                return View("SuperAdminIndex", superModel);
            }

            // Determine effective school ID
            int targetSchoolId;
            if (isSuperAdmin && schoolId.HasValue)
            {
                targetSchoolId = schoolId.Value;
                // Store in session for school-switching convenience
                HttpContext.Session.SetInt32("SelectedSchoolId", targetSchoolId);
            }
            else if (userSchoolId.HasValue)
            {
                targetSchoolId = userSchoolId.Value;
            }
            else
            {
                var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
                if (sessionSchoolId.HasValue)
                {
                    targetSchoolId = sessionSchoolId.Value;
                }
                else
                {
                    var schools = await _schoolService.GetActiveSchoolsAsync();
                    if (schools.Count == 0)
                    {
                        return RedirectToAction("Create", "School");
                    }
                    targetSchoolId = schools[0].Id;
                }
            }

            var schoolModel = await _dashboardService.GetSchoolDashboardAsync(targetSchoolId);
            if (schoolModel == null)
            {
                TempData["ErrorMessage"] = "Selected school could not be found.";
                return RedirectToAction("Index", "School");
            }

            ViewBag.IsSuperAdminOverview = false;
            ViewBag.AllSchools = await _schoolService.GetAllSchoolsAsync();
            ViewBag.ActiveSchoolId = targetSchoolId;

            return View("Index", schoolModel);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        public IActionResult SwitchSchool(int selectedSchoolId)
        {
            HttpContext.Session.SetInt32("SelectedSchoolId", selectedSchoolId);
            return RedirectToAction("Index", new { schoolId = selectedSchoolId });
        }
    }
}
