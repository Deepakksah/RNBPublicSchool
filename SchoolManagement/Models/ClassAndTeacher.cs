using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Class Name (e.g. Class 1, Class 10, Nursery)")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 1;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public class Section
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Display(Name = "Section Name (e.g. A, B, C)")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Room Number")]
        public string? RoomNumber { get; set; }

        public int Capacity { get; set; } = 40;

        public int? ClassTeacherId { get; set; }
        public virtual Teacher? ClassTeacher { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();
    }

    public class Subject
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Code { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
    }

    public enum TeacherStatus
    {
        Active = 1,
        Inactive = 2,
        Resigned = 3,
        OnLeave = 4
    }

    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Teacher Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Photo { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = "Male"; // Male, Female, Other

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(150)]
        public string? Qualification { get; set; }

        [MaxLength(100)]
        public string? Experience { get; set; }

        [MaxLength(100)]
        [Display(Name = "Primary Subject")]
        public string? Subject { get; set; }

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = "Teacher";

        [Required]
        [MaxLength(20)]
        [Phone]
        public string Mobile { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        public TeacherStatus Status { get; set; } = TeacherStatus.Active;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        // User account association
        public string? UserId { get; set; }

        // Navigation
        public virtual ICollection<TeacherSubject> TeacherSubjects { get; set; } = new List<TeacherSubject>();
        public virtual ICollection<Section> ClassTeacherSections { get; set; } = new List<Section>();
        public virtual ICollection<TeacherAttendance> Attendances { get; set; } = new List<TeacherAttendance>();
    }

    public class TeacherSubject
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;
    }
}
