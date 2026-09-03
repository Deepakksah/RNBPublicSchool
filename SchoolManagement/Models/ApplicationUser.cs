using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SchoolManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int? SchoolId { get; set; }
        public virtual School? School { get; set; }

        public string? ProfilePicture { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }

        public int? StudentId { get; set; }
        public virtual Student? Student { get; set; }

        public int? TeacherId { get; set; }
        public virtual Teacher? Teacher { get; set; }

        public int? ParentId { get; set; }
        public virtual Parent? Parent { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
