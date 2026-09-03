using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class SchoolCreateEditViewModel
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

        [Display(Name = "School Logo File")]
        public IFormFile? LogoFile { get; set; }
        public string? ExistingLogo { get; set; }

        [Display(Name = "School Banner File")]
        public IFormFile? BannerFile { get; set; }
        public string? ExistingBanner { get; set; }

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
        public int EstablishedYear { get; set; } = DateTime.Now.Year;

        public SchoolStatus Status { get; set; } = SchoolStatus.Active;

        [Display(Name = "About School")]
        public string? About { get; set; }

        public string? Vision { get; set; }

        public string? Mission { get; set; }

        // Principal Info
        [MaxLength(100)]
        [Display(Name = "Principal Name")]
        public string? PrincipalName { get; set; }

        [Display(Name = "Principal Photo File")]
        public IFormFile? PrincipalPhotoFile { get; set; }
        public string? ExistingPrincipalPhoto { get; set; }

        [MaxLength(150)]
        [Display(Name = "Principal Qualification")]
        public string? PrincipalQualification { get; set; }

        [MaxLength(100)]
        [Display(Name = "Principal Experience")]
        public string? PrincipalExperience { get; set; }

        [MaxLength(20)]
        [Display(Name = "Principal Phone")]
        public string? PrincipalPhone { get; set; }

        [MaxLength(150)]
        [Display(Name = "Principal Email")]
        public string? PrincipalEmail { get; set; }

        [Display(Name = "Principal Message")]
        public string? PrincipalMessage { get; set; }
    }

    public class SchoolPublicProfileViewModel
    {
        public School School { get; set; } = null!;
        public Principal? Principal { get; set; }
        public List<SchoolImage> GalleryImages { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public List<Class> Classes { get; set; } = new();
        public List<Notification> Notices { get; set; } = new();
    }

    public class SchoolAvailabilityViewModel
    {
        public List<SchoolAvailabilityItem> Schools { get; set; } = new();
    }

    public class SchoolAvailabilityItem
    {
        public int SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolCode { get; set; } = string.Empty;
        public SchoolStatus Status { get; set; }
        public int TotalStudents { get; set; }
        public int PresentStudentsToday { get; set; }
        public int TotalTeachers { get; set; }
        public int PresentTeachersToday { get; set; }
        public int TotalClasses { get; set; }
        public double AttendanceRate { get; set; }
        public bool IsOpenToday { get; set; }
    }
}
