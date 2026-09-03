using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManagement.Models
{
    public enum StudentStatus
    {
        Active = 1,
        Inactive = 2,
        Transferred = 3,
        LeftSchool = 4
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Leave = 3,
        Late = 4
    }

    public class Student
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        public int AcademicYearId { get; set; }
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [Required]
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }
        public virtual Section Section { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Admission Number")]
        public string AdmissionNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Roll Number")]
        public int RollNumber { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Student Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Photo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = "Male"; // Male, Female, Other

        [MaxLength(10)]
        [Display(Name = "Blood Group")]
        public string? BloodGroup { get; set; }

        // Parents & Guardians
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

        // Address
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
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow.Date;

        public StudentStatus Status { get; set; } = StudentStatus.Active;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string? UserId { get; set; }

        // Navigation
        public virtual ICollection<StudentAttendance> Attendances { get; set; } = new List<StudentAttendance>();
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }

    public class Parent
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        public string? UserId { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }

    public class StudentParent
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public int ParentId { get; set; }
        public virtual Parent Parent { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Relationship { get; set; } = "Father"; // Father, Mother, Guardian
    }

    public class StudentAttendance
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        public int AcademicYearId { get; set; }
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [Required]
        public int ClassId { get; set; }
        public virtual Class Class { get; set; } = null!;

        [Required]
        public int SectionId { get; set; }
        public virtual Section Section { get; set; } = null!;

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string? RecordedBy { get; set; }
    }

    public class TeacherAttendance
    {
        public int Id { get; set; }

        [Required]
        public int SchoolId { get; set; }
        public virtual School School { get; set; } = null!;

        [Required]
        public int AcademicYearId { get; set; }
        public virtual AcademicYear AcademicYear { get; set; } = null!;

        [Required]
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime AttendanceDate { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
        public string? RecordedBy { get; set; }
    }
}
