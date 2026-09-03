using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class StudentAttendanceSheetViewModel
    {
        public int SchoolId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Section")]
        public int SectionId { get; set; }

        public int AcademicYearId { get; set; }

        public string? ClassName { get; set; }
        public string? SectionName { get; set; }

        public bool IsAlreadyMarked { get; set; }

        public List<StudentAttendanceEntryItem> Students { get; set; } = new();

        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
        public SelectList? AcademicYears { get; set; }
    }

    public class StudentAttendanceEntryItem
    {
        public int StudentId { get; set; }
        public int RollNumber { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Remarks { get; set; }
    }

    public class TeacherAttendanceSheetViewModel
    {
        public int SchoolId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; } = DateTime.Today;

        public int AcademicYearId { get; set; }
        public bool IsAlreadyMarked { get; set; }

        public List<TeacherAttendanceEntryItem> Teachers { get; set; } = new();

        public SelectList? AcademicYears { get; set; }
    }

    public class TeacherAttendanceEntryItem
    {
        public int TeacherId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Remarks { get; set; }
    }

    public class AttendanceAnalyticsViewModel
    {
        public int? SchoolId { get; set; }
        public int? ClassId { get; set; }
        public int? SectionId { get; set; }
        public int? StudentId { get; set; }
        public int? TeacherId { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateFrom { get; set; } = DateTime.Today.AddDays(-30);

        [DataType(DataType.Date)]
        public DateTime DateTo { get; set; } = DateTime.Today;

        public int? AcademicYearId { get; set; }

        // Totals
        public int TotalRecords { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLeave { get; set; }
        public int TotalLate { get; set; }
        public double AttendancePercentage =>
            TotalRecords > 0 ? Math.Round((double)TotalPresent / TotalRecords * 100, 2) : 0.0;

        // Chart Data
        public List<string> ChartDates { get; set; } = new();
        public List<int> ChartPresentCounts { get; set; } = new();
        public List<int> ChartAbsentCounts { get; set; } = new();

        public SelectList? Schools { get; set; }
        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
        public SelectList? AcademicYears { get; set; }
    }

    public class DailySummaryViewModel
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public School School { get; set; } = null!;

        // Student Stats
        public int TotalStudents { get; set; }
        public int StudentsPresent { get; set; }
        public int StudentsAbsent { get; set; }
        public int StudentsLeave { get; set; }
        public int StudentsLate { get; set; }
        public double StudentAttendancePercentage =>
            TotalStudents > 0 ? Math.Round((double)StudentsPresent / TotalStudents * 100, 2) : 0.0;

        // Teacher Stats
        public int TotalTeachers { get; set; }
        public int TeachersPresent { get; set; }
        public int TeachersAbsent { get; set; }
        public int TeachersLeave { get; set; }
        public int TeachersLate { get; set; }
        public double TeacherAttendancePercentage =>
            TotalTeachers > 0 ? Math.Round((double)TeachersPresent / TotalTeachers * 100, 2) : 0.0;

        public List<ClassAttendanceSummary> ClassSummaries { get; set; } = new();
    }
}
