using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class DEOConsoleViewModel
    {
        public List<Student> RecentAdmissions { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TodayAdmissionsCount { get; set; }
        public int ThisMonthAdmissionsCount { get; set; }
        public string? SearchTerm { get; set; }
        public int? SelectedClassId { get; set; }
        public int? SelectedSectionId { get; set; }
        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
    }

    public class DEOAdmissionViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Academic Year is required")]
        [Display(Name = "Academic Session")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Class selection is required")]
        [Display(Name = "Class / Standard")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Section selection is required")]
        [Display(Name = "Section / Classroom")]
        public int SectionId { get; set; }

        [Required(ErrorMessage = "Admission Number is required")]
        [MaxLength(50)]
        [Display(Name = "Admission Number")]
        public string AdmissionNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll Number is required")]
        [Range(1, 200, ErrorMessage = "Roll number must be between 1 and 200")]
        [Display(Name = "Roll Number")]
        public int RollNumber { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        [MaxLength(100)]
        [Display(Name = "Student Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Student Passport Photo")]
        public IFormFile? PhotoFile { get; set; }
        public string? ExistingPhoto { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-6);

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = "Male";

        [MaxLength(10)]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; } = "O+";

        [Required(ErrorMessage = "Father's Name is required")]
        [MaxLength(100)]
        [Display(Name = "Father's Full Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Father's Mobile Number is required")]
        [MaxLength(20)]
        [Phone]
        [Display(Name = "Father's Mobile Number")]
        public string FatherMobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mother's Name is required")]
        [MaxLength(100)]
        [Display(Name = "Mother's Full Name")]
        public string MotherName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone]
        [Display(Name = "Mother's Mobile Number")]
        public string? MotherMobile { get; set; }

        [MaxLength(100)]
        [Display(Name = "Guardian's Name (if any)")]
        public string? GuardianName { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        [Display(Name = "Email Address (Optional)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address / Village / Mohalla is required")]
        [MaxLength(250)]
        [Display(Name = "Residential Address / Location")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "City / Block / Town")]
        public string City { get; set; } = "Piro";

        [Required]
        [MaxLength(100)]
        [Display(Name = "State")]
        public string State { get; set; } = "Bihar";

        [Required]
        [MaxLength(20)]
        [Display(Name = "PIN Code")]
        public string PinCode { get; set; } = "802207";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Admission")]
        public DateTime AdmissionDate { get; set; } = DateTime.Today;

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        public SelectList? AcademicYears { get; set; }
        public SelectList? Classes { get; set; }
        public SelectList? Sections { get; set; }
    }

    public class DEOTeachersViewModel
    {
        public List<Teacher> Teachers { get; set; } = new();
        public List<Section> Sections { get; set; } = new();
        public int TotalTeachers { get; set; }
        public int AssignedClassTeachersCount { get; set; }
        public string? SearchTerm { get; set; }
    }
}
