using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class TeacherListViewModel
    {
        public List<Teacher> Teachers { get; set; } = new();
        public string? SearchTerm { get; set; }
        public TeacherStatus? SelectedStatus { get; set; }
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalCount { get; set; }
    }

    public class TeacherCreateEditViewModel
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Employee ID is required")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teacher Name is required")]
        [Display(Name = "Teacher Full Name")]
        public string Name { get; set; } = string.Empty;

        public IFormFile? PhotoFile { get; set; }
        public string? ExistingPhoto { get; set; }

        [Required]
        public string Gender { get; set; } = "Female";

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-30);

        [Required]
        public string Qualification { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public string? Subject { get; set; }

        [Required]
        public string Designation { get; set; } = "Senior Teacher";

        [Required]
        [Phone]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        public TeacherStatus Status { get; set; } = TeacherStatus.Active;

        [Display(Name = "Assign as Class Teacher")]
        public int? AssignedSectionId { get; set; }

        public SelectList? Sections { get; set; }
    }

    public class TeacherProfileViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int LateDays { get; set; }
        public double AttendancePercentage => TotalWorkingDays > 0 ? Math.Round((double)PresentDays / TotalWorkingDays * 100, 2) : 0.0;
        public List<TeacherAttendance> RecentAttendances { get; set; } = new();
        public List<MonthAttendanceSummary> MonthlyAttendances { get; set; } = new();
        public List<Section> AssignedSections { get; set; } = new();
    }

    public class TeacherClassroomViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public Section? AssignedSection { get; set; }
        public int TotalStudents { get; set; }
        public int TodayPresent { get; set; }
        public int TodayAbsent { get; set; }
        public int TodayLeave { get; set; }
        public int TodayLate { get; set; }
        public double TodayAttendancePercentage { get; set; }
        public bool IsAttendanceMarkedToday { get; set; }
        public List<Student> Students { get; set; } = new();
        public List<Notification> RecentNotices { get; set; } = new();
        public List<Holiday> UpcomingHolidays { get; set; } = new();
    }
}
