using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class ClassListViewModel
    {
        public List<ClassItemViewModel> Classes { get; set; } = new();
    }

    public class ClassItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int SectionCount { get; set; }
        public int StudentCount { get; set; }
        public List<Section> Sections { get; set; } = new();
    }

    public class ClassCreateEditViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Class Name (e.g. Class 1, Class 10, Nursery)")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;
    }

    public class SectionCreateEditViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Section Name (e.g. A, B, C)")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        public int Capacity { get; set; } = 40;

        [Display(Name = "Class Teacher")]
        public int? ClassTeacherId { get; set; }

        public SelectList? Classes { get; set; }
        public SelectList? Teachers { get; set; }
    }

    public class ClassDashboardViewModel
    {
        public Class Class { get; set; } = null!;
        public Section Section { get; set; } = null!;
        public Teacher? ClassTeacher { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;

        public int TotalStudents { get; set; }
        public int Present { get; set; }
        public int Absent { get; set; }
        public int Leave { get; set; }
        public int Late { get; set; }
        public double AttendancePercentage => TotalStudents > 0 ? Math.Round((double)Present / TotalStudents * 100, 1) : 0.0;

        public List<ClassStudentAttendanceItem> Students { get; set; } = new();
    }

    public class ClassStudentAttendanceItem
    {
        public int StudentId { get; set; }
        public int RollNumber { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? Photo { get; set; }
        public AttendanceStatus? Status { get; set; }
        public string? Remarks { get; set; }
    }
}
