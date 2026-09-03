using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class StudentListViewModel
    {
        public List<Student> Students { get; set; } = new();
        public int? SelectedClassId { get; set; }
        public int? SelectedSectionId { get; set; }
        public string? SearchTerm { get; set; }
        public StudentStatus? SelectedStatus { get; set; }

        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }

        // Pagination
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
    }

    public class StudentCreateEditViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [Required]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Section")]
        public int SectionId { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Admission Number")]
        public string AdmissionNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Roll Number")]
        public int RollNumber { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Student Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Student Photo")]
        public IFormFile? PhotoFile { get; set; }
        public string? ExistingPhoto { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-10);

        [Required]
        public string Gender { get; set; } = "Male";

        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        // Parent Information
        [Required]
        [MaxLength(100)]
        [Display(Name = "Father's Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Mother's Name")]
        public string MotherName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Guardian's Name")]
        public string? GuardianName { get; set; }

        [Required]
        [MaxLength(20)]
        [Phone]
        [Display(Name = "Father's Mobile")]
        public string FatherMobile { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        [Display(Name = "Mother's Mobile")]
        public string? MotherMobile { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Display(Name = "PIN Code")]
        public string PinCode { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Admission Date")]
        public DateTime AdmissionDate { get; set; } = DateTime.Today;

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        // Dropdowns
        public SelectList? AcademicYears { get; set; }
        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
    }

    public class StudentProfileViewModel
    {
        public Student Student { get; set; } = null!;
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        public double AttendancePercentage =>
            TotalWorkingDays > 0 ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 2) : 0.0;

        public List<StudentAttendance> RecentAttendances { get; set; } = new();
        public List<MonthAttendanceSummary> MonthlyAttendances { get; set; } = new();
    }

    public class MonthAttendanceSummary
    {
        public string MonthName { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
        public int Late { get; set; }
        public double Percentage => TotalDays > 0 ? Math.Round((double)Present / TotalDays * 100, 1) : 0.0;
    }
}
