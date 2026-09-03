using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models
{
    public enum NotificationType
    {
        General = 1,
        Attendance = 2,
        Holiday = 3,
        Event = 4,
        Emergency = 5,
        SchoolNotice = 6
    }

    public enum TargetAudience
    {
        All = 1,
        TeachersOnly = 2,
        StudentsOnly = 3,
        ParentsOnly = 4,
        StaffOnly = 5
    }

    public class Notification
    {
        public int Id { get; set; }

        public int? SchoolId { get; set; } // Null for system-wide notifications
        public virtual School? School { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.General;

        public TargetAudience Audience { get; set; } = TargetAudience.All;

        [DataType(DataType.Date)]
        public DateTime PublishDate { get; set; } = DateTime.UtcNow.Date;

        [DataType(DataType.Date)]
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(255)]
        public string? AttachmentPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
    }

    public class Holiday
    {
        public int Id { get; set; }

        public int? SchoolId { get; set; } // Null for national/system holidays
        public virtual School? School { get; set; }

        public int AcademicYearId { get; set; }
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        [Display(Name = "Holiday Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Holiday Date")]
        public DateTime HolidayDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date (if multi-day)")]
        public DateTime? EndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }

    public class AdmissionInquiry
    {
        public int Id { get; set; }

        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required(ErrorMessage = "Student Name is required")]
        [MaxLength(100)]
        [Display(Name = "Student Full Name")]
        public string StudentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select the applying class")]
        [MaxLength(50)]
        [Display(Name = "Applying for Class / Standard")]
        public string ApplyingForClass { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-6);

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Father's Name is required")]
        [MaxLength(100)]
        [Display(Name = "Father's / Guardian's Full Name")]
        public string FatherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Mobile Number is required")]
        [Phone]
        [MaxLength(20)]
        [Display(Name = "Contact Mobile Number (WhatsApp)")]
        public string FatherMobile { get; set; } = string.Empty;

        [MaxLength(100)]
        [Display(Name = "Mother's Full Name")]
        public string? MotherName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        [Display(Name = "Email Address (Optional)")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(250)]
        [Display(Name = "Residential Address / Village / Locality")]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = "Piro";

        [MaxLength(100)]
        public string State { get; set; } = "Bihar";

        [MaxLength(20)]
        public string PinCode { get; set; } = "802207";

        [MaxLength(200)]
        [Display(Name = "Previous School Attended (if any)")]
        public string? PreviousSchool { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Questions / Specific Inquiries")]
        public string? QueryMessage { get; set; }

        public string Status { get; set; } = "New"; // New, Contacted, Admitted, Rejected

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class AuditLog
    {
        public int Id { get; set; }

        public int? SchoolId { get; set; }
        public virtual School? School { get; set; }

        [MaxLength(100)]
        public string? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = "Anonymous";

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // Login, Logout, Create, Update, Delete, AttendanceMarked, etc.

        [Required]
        [MaxLength(100)]
        public string Entity { get; set; } = string.Empty; // Student, Teacher, Attendance, School, etc.

        [MaxLength(50)]
        public string? EntityId { get; set; }

        [MaxLength(1000)]
        public string? Details { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }

    public class SystemSetting
    {
        public int Id { get; set; }

        public int? SchoolId { get; set; } // Null for global settings
        public virtual School? School { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    }
}
