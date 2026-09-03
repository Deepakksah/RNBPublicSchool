using System;
using System.Collections.Generic;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class SuperAdminDashboardViewModel
    {
        public int TotalSchools { get; set; }
        public int ActiveSchools { get; set; }
        public int InactiveSchools { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int StudentsPresentToday { get; set; }
        public int StudentsAbsentToday { get; set; }
        public int TeachersPresentToday { get; set; }
        public int TeachersAbsentToday { get; set; }
        public int TotalClasses { get; set; }

        public double StudentAttendancePercentage =>
            (StudentsPresentToday + StudentsAbsentToday) > 0
                ? Math.Round((double)StudentsPresentToday / (StudentsPresentToday + StudentsAbsentToday) * 100, 2)
                : 0.0;

        public double TeacherAttendancePercentage =>
            (TeachersPresentToday + TeachersAbsentToday) > 0
                ? Math.Round((double)TeachersPresentToday / (TeachersPresentToday + TeachersAbsentToday) * 100, 2)
                : 0.0;

        public List<SchoolSummaryItem> SchoolSummaries { get; set; } = new();
        public List<RecentActivityItem> RecentActivities { get; set; } = new();

        // Chart Data Properties
        public List<string> SchoolNames { get; set; } = new();
        public List<int> SchoolStudentCounts { get; set; } = new();
        public List<int> SchoolTeacherCounts { get; set; } = new();
        public List<string> MonthlyLabels { get; set; } = new();
        public List<double> MonthlyStudentAttendanceRate { get; set; } = new();
    }

    public class SchoolDashboardViewModel
    {
        public School School { get; set; } = null!;
        public AcademicYear? CurrentAcademicYear { get; set; }

        public int TotalStudents { get; set; }
        public int PresentStudents { get; set; }
        public int AbsentStudents { get; set; }
        public int LeaveStudents { get; set; }
        public int LateStudents { get; set; }
        public double StudentAttendancePercentage { get; set; }

        public int TotalTeachers { get; set; }
        public int PresentTeachers { get; set; }
        public int AbsentTeachers { get; set; }
        public int LeaveTeachers { get; set; }
        public int LateTeachers { get; set; }
        public double TeacherAttendancePercentage { get; set; }

        public int TotalClasses { get; set; }
        public int TotalSections { get; set; }

        // Class-wise today's stats
        public List<ClassAttendanceSummary> ClassAttendanceSummaries { get; set; } = new();

        // Recent notifications
        public List<Notification> RecentNotifications { get; set; } = new();
        // Upcoming holidays
        public List<Holiday> UpcomingHolidays { get; set; } = new();

        // Chart data for daily trend (last 7 days)
        public List<string> DailyTrendDates { get; set; } = new();
        public List<double> DailyStudentRates { get; set; } = new();
        public List<double> DailyTeacherRates { get; set; } = new();
    }

    public class SchoolSummaryItem
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int ClassCount { get; set; }
        public double TodayAttendanceRate { get; set; }
    }

    public class RecentActivityItem
    {
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class ClassAttendanceSummary
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? ClassTeacherName { get; set; }
        public int TotalStudents { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
        public int Late { get; set; }
        public double AttendancePercentage => TotalStudents > 0 ? Math.Round((double)Present / TotalStudents * 100, 1) : 0.0;
    }
}
