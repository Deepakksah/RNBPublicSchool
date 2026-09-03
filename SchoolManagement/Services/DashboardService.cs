using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Services
{
    public interface IDashboardService
    {
        Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardAsync();
        Task<SchoolDashboardViewModel?> GetSchoolDashboardAsync(int schoolId);
    }

    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SuperAdminDashboardViewModel> GetSuperAdminDashboardAsync()
        {
            var today = DateTime.Today;

            var schools = await _context.Schools.Where(s => s.IsActive).ToListAsync();
            var totalSchools = schools.Count;
            var activeSchools = schools.Count(s => s.Status == SchoolStatus.Active);
            var inactiveSchools = schools.Count(s => s.Status != SchoolStatus.Active);

            var totalStudents = await _context.Students.CountAsync(s => s.IsActive && s.Status == StudentStatus.Active);
            var totalTeachers = await _context.Teachers.CountAsync(t => t.IsActive && t.Status == TeacherStatus.Active);
            var totalClasses = await _context.Classes.CountAsync(c => c.IsActive);

            var studentsPresent = await _context.StudentAttendances
                .CountAsync(a => a.AttendanceDate == today && a.Status == AttendanceStatus.Present);

            var studentsAbsent = await _context.StudentAttendances
                .CountAsync(a => a.AttendanceDate == today && a.Status == AttendanceStatus.Absent);

            var teachersPresent = await _context.TeacherAttendances
                .CountAsync(a => a.AttendanceDate == today && a.Status == AttendanceStatus.Present);

            var teachersAbsent = await _context.TeacherAttendances
                .CountAsync(a => a.AttendanceDate == today && a.Status == AttendanceStatus.Absent);

            var schoolSummaries = new List<SchoolSummaryItem>();
            var schoolNames = new List<string>();
            var schoolStudentCounts = new List<int>();
            var schoolTeacherCounts = new List<int>();

            foreach (var sc in schools)
            {
                var scStudents = await _context.Students.CountAsync(s => s.SchoolId == sc.Id && s.IsActive && s.Status == StudentStatus.Active);
                var scTeachers = await _context.Teachers.CountAsync(t => t.SchoolId == sc.Id && t.IsActive && t.Status == TeacherStatus.Active);
                var scClasses = await _context.Classes.CountAsync(c => c.SchoolId == sc.Id && c.IsActive);

                var scPresent = await _context.StudentAttendances
                    .CountAsync(a => a.SchoolId == sc.Id && a.AttendanceDate == today && a.Status == AttendanceStatus.Present);

                var rate = scStudents > 0 ? Math.Round((double)scPresent / scStudents * 100, 1) : 0.0;

                schoolSummaries.Add(new SchoolSummaryItem
                {
                    SchoolId = sc.Id,
                    SchoolName = sc.Name,
                    SchoolCode = sc.Code,
                    Status = sc.Status.ToString(),
                    StudentCount = scStudents,
                    TeacherCount = scTeachers,
                    ClassCount = scClasses,
                    TodayAttendanceRate = rate
                });

                schoolNames.Add(sc.Name);
                schoolStudentCounts.Add(scStudents);
                schoolTeacherCounts.Add(scTeachers);
            }

            var recentActivities = await _context.AuditLogs
                .OrderByDescending(a => a.DateTime)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    UserName = a.UserName,
                    Action = a.Action,
                    Entity = a.Entity,
                    Details = a.Details,
                    DateTime = a.DateTime
                })
                .ToListAsync();

            // Monthly attendance rate (last 6 months)
            var monthlyLabels = new List<string>();
            var monthlyRates = new List<double>();

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = today.AddMonths(-i);
                var mStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                var mEnd = mStart.AddMonths(1).AddDays(-1);

                var totalInMonth = await _context.StudentAttendances
                    .CountAsync(a => a.AttendanceDate >= mStart && a.AttendanceDate <= mEnd);

                var presentInMonth = await _context.StudentAttendances
                    .CountAsync(a => a.AttendanceDate >= mStart && a.AttendanceDate <= mEnd && a.Status == AttendanceStatus.Present);

                var rate = totalInMonth > 0 ? Math.Round((double)presentInMonth / totalInMonth * 100, 1) : 0.0;

                monthlyLabels.Add(mStart.ToString("MMM yyyy"));
                monthlyRates.Add(rate);
            }

            return new SuperAdminDashboardViewModel
            {
                TotalSchools = totalSchools,
                ActiveSchools = activeSchools,
                InactiveSchools = inactiveSchools,
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                StudentsPresentToday = studentsPresent,
                StudentsAbsentToday = studentsAbsent,
                TeachersPresentToday = teachersPresent,
                TeachersAbsentToday = teachersAbsent,
                TotalClasses = totalClasses,
                SchoolSummaries = schoolSummaries,
                RecentActivities = recentActivities,
                SchoolNames = schoolNames,
                SchoolStudentCounts = schoolStudentCounts,
                SchoolTeacherCounts = schoolTeacherCounts,
                MonthlyLabels = monthlyLabels,
                MonthlyStudentAttendanceRate = monthlyRates
            };
        }

        public async Task<SchoolDashboardViewModel?> GetSchoolDashboardAsync(int schoolId)
        {
            var school = await _context.Schools
                .Include(s => s.Principal)
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null) return null;

            var currentYear = await _context.AcademicYears
                .FirstOrDefaultAsync(a => a.SchoolId == schoolId && a.IsCurrent && a.IsActive);

            var today = DateTime.Today;

            var totalStudents = await _context.Students.CountAsync(s => s.SchoolId == schoolId && s.IsActive && s.Status == StudentStatus.Active);
            var totalTeachers = await _context.Teachers.CountAsync(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active);
            var totalClasses = await _context.Classes.CountAsync(c => c.SchoolId == schoolId && c.IsActive);
            var totalSections = await _context.Sections.CountAsync(s => s.SchoolId == schoolId && s.IsActive);

            var studentAtt = await _context.StudentAttendances
                .Where(a => a.SchoolId == schoolId && a.AttendanceDate == today)
                .ToListAsync();

            var teacherAtt = await _context.TeacherAttendances
                .Where(a => a.SchoolId == schoolId && a.AttendanceDate == today)
                .ToListAsync();

            var presentStudents = studentAtt.Count(a => a.Status == AttendanceStatus.Present);
            var absentStudents = studentAtt.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveStudents = studentAtt.Count(a => a.Status == AttendanceStatus.Leave);
            var lateStudents = studentAtt.Count(a => a.Status == AttendanceStatus.Late);

            var studentRate = totalStudents > 0 ? Math.Round((double)presentStudents / totalStudents * 100, 2) : 0.0;

            var presentTeachers = teacherAtt.Count(a => a.Status == AttendanceStatus.Present);
            var absentTeachers = teacherAtt.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveTeachers = teacherAtt.Count(a => a.Status == AttendanceStatus.Leave);
            var lateTeachers = teacherAtt.Count(a => a.Status == AttendanceStatus.Late);

            var teacherRate = totalTeachers > 0 ? Math.Round((double)presentTeachers / totalTeachers * 100, 2) : 0.0;

            // Class-wise summary for today (Optimized with AsSplitQuery and AsNoTracking)
            var sections = await _context.Sections
                .AsNoTracking()
                .AsSplitQuery()
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Include(s => s.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .OrderBy(s => s.Class.DisplayOrder).ThenBy(s => s.Name)
                .ToListAsync();

            var classSummaries = sections.Select(sec =>
            {
                var secAtt = studentAtt.Where(a => a.SectionId == sec.Id).ToList();
                return new ClassAttendanceSummary
                {
                    ClassId = sec.ClassId,
                    ClassName = sec.Class.Name,
                    SectionId = sec.Id,
                    SectionName = sec.Name,
                    ClassTeacherName = sec.ClassTeacher?.Name,
                    TotalStudents = sec.Students.Count,
                    Present = secAtt.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = secAtt.Count(a => a.Status == AttendanceStatus.Absent),
                    Leave = secAtt.Count(a => a.Status == AttendanceStatus.Leave),
                    Late = secAtt.Count(a => a.Status == AttendanceStatus.Late)
                };
            }).ToList();

            // Notifications & Holidays
            var notifications = await _context.Notifications
                .Where(n => (n.SchoolId == schoolId || n.SchoolId == null) && n.IsActive)
                .OrderByDescending(n => n.PublishDate)
                .Take(5)
                .ToListAsync();

            // Holidays for Current Month (e.g. September 2026)
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var holidays = await _context.Holidays
                .Where(h => (h.SchoolId == schoolId || h.SchoolId == null) && h.IsActive && h.HolidayDate >= firstDayOfMonth && h.HolidayDate <= lastDayOfMonth)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            if (!holidays.Any())
            {
                holidays = await _context.Holidays
                    .Where(h => (h.SchoolId == schoolId || h.SchoolId == null) && h.IsActive && h.HolidayDate >= today)
                    .OrderBy(h => h.HolidayDate)
                    .Take(5)
                    .ToListAsync();
            }

            // Daily trend (past 7 days)
            var trendDates = new List<string>();
            var dailyStudentRates = new List<double>();
            var dailyTeacherRates = new List<double>();

            for (int i = 6; i >= 0; i--)
            {
                var d = today.AddDays(-i);
                var sPres = await _context.StudentAttendances.CountAsync(a => a.SchoolId == schoolId && a.AttendanceDate == d && a.Status == AttendanceStatus.Present);
                var tPres = await _context.TeacherAttendances.CountAsync(a => a.SchoolId == schoolId && a.AttendanceDate == d && a.Status == AttendanceStatus.Present);

                trendDates.Add(d.ToString("dd MMM"));
                dailyStudentRates.Add(totalStudents > 0 ? Math.Round((double)sPres / totalStudents * 100, 1) : 0.0);
                dailyTeacherRates.Add(totalTeachers > 0 ? Math.Round((double)tPres / totalTeachers * 100, 1) : 0.0);
            }

            return new SchoolDashboardViewModel
            {
                School = school,
                CurrentAcademicYear = currentYear,
                TotalStudents = totalStudents,
                PresentStudents = presentStudents,
                AbsentStudents = absentStudents,
                LeaveStudents = leaveStudents,
                LateStudents = lateStudents,
                StudentAttendancePercentage = studentRate,
                TotalTeachers = totalTeachers,
                PresentTeachers = presentTeachers,
                AbsentTeachers = absentTeachers,
                LeaveTeachers = leaveTeachers,
                LateTeachers = lateTeachers,
                TeacherAttendancePercentage = teacherRate,
                TotalClasses = totalClasses,
                TotalSections = totalSections,
                ClassAttendanceSummaries = classSummaries,
                RecentNotifications = notifications,
                UpcomingHolidays = holidays,
                DailyTrendDates = trendDates,
                DailyStudentRates = dailyStudentRates,
                DailyTeacherRates = dailyTeacherRates
            };
        }
    }
}
