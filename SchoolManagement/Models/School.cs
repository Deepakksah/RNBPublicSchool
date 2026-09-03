using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Models
{
    public enum SchoolStatus
    {
        Active = 1,
        Inactive = 2,
        Suspended = 3,
        UnderMaintenance = 4
    }

    public class School
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "School Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "School Code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        [MaxLength(255)]
        [Display(Name = "School Logo")]
        public string? Logo { get; set; }

        [MaxLength(255)]
        [Display(Name = "School Banner")]
        public string? Banner { get; set; }

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
        [MaxLength(20)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        [Url]
        public string? Website { get; set; }

        [Display(Name = "Established Year")]
        public int EstablishedYear { get; set; }

        public SchoolStatus Status { get; set; } = SchoolStatus.Active;

        [Display(Name = "About School")]
        public string? About { get; set; }

        public string? Vision { get; set; }

        public string? Mission { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Principal? Principal { get; set; }
        public virtual ICollection<SchoolImage> Images { get; set; } = new List<SchoolImage>();
        public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }

    public class Principal
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Principal Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        [Display(Name = "Principal Photo")]
        public string? Photo { get; set; }

        [MaxLength(150)]
        public string? Qualification { get; set; }

        [MaxLength(100)]
        public string? Experience { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "Principal Message")]
        public string? Message { get; set; }

        public string? Vision { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }

    public class SchoolImage
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = "Campus"; // Campus, Building, Classroom, Library, Laboratory, Computer Lab, Playground, Sports, Events, Cultural Activities, Other

        public bool IsCoverImage { get; set; } = false;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string? UploadedBy { get; set; }
    }

    public class AcademicYear
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Academic Year Name (e.g. 2026-27)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public bool IsCurrent { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<StudentAttendance> StudentAttendances { get; set; } = new List<StudentAttendance>();
        public virtual ICollection<TeacherAttendance> TeacherAttendances { get; set; } = new List<TeacherAttendance>();
        public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    }
}
