using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class ReportFilterViewModel
    {
        public int? SchoolId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Today;

        public string ReportType { get; set; } = "StudentAttendance"; // StudentAttendance, TeacherAttendance, ClassAttendance, SchoolSummary, LeaveReport, AbsentList

        public SelectList? Schools { get; set; }
        public SelectList? AcademicYears { get; set; }
        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
        public SelectList? Students { get; set; }
        public SelectList? Teachers { get; set; }
    }

    public class StudentAttendanceReportItem
    {
        public int StudentId { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public int RollNumber { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        public double AttendancePercentage => TotalWorkingDays > 0 ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 1) : 0.0;
    }

    public class TeacherAttendanceReportItem
    {
        public int TeacherId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        public double AttendancePercentage => TotalWorkingDays > 0 ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 1) : 0.0;
    }

    public class ClassAttendanceReportItem
    {
        public string ClassName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string? ClassTeacherName { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public double AverageAttendanceRate { get; set; }
    }

    public class SchoolSummaryReportViewModel
    {
        public School School { get; set; } = null!;
        public AcademicYear? AcademicYear { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        public int TotalStudents { get; set; }
        public int TotalMaleStudents { get; set; }
        public int TotalFemaleStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalMaleTeachers { get; set; }
        public int TotalFemaleTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSections { get; set; }

        public double OverallStudentAttendanceRate { get; set; }
        public double OverallTeacherAttendanceRate { get; set; }

        public List<ClassAttendanceReportItem> ClassSummaries { get; set; } = new();
    }
}
