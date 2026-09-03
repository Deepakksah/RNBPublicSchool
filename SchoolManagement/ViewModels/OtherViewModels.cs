using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolManagement.Models;

namespace SchoolManagement.ViewModels
{
    public class GalleryViewModel
    {
        public int SchoolId { get; set; }
        public string? SelectedCategory { get; set; }
        public List<SchoolImage> Images { get; set; } = new();
        public List<string> Categories { get; set; } = new();
    }

    public class GalleryUploadViewModel
    {
        public int SchoolId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; } = "Campus";

        [Required]
        [Display(Name = "Upload Images (JPG, PNG, WEBP)")]
        public List<IFormFile> ImageFiles { get; set; } = new();

        public bool SetAsCover { get; set; } = false;

        public SelectList? Categories { get; set; }
    }

    public class UserListViewModel
    {
        public List<UserItemViewModel> Users { get; set; } = new();
    }

    public class UserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SchoolName { get; set; }
        public int? SchoolId { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserCreateEditViewModel
    {
        public string? Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Assigned School")]
        public int? SchoolId { get; set; }

        [Required]
        [Display(Name = "User Role")]
        public string Role { get; set; } = "School Admin";

        public bool IsActive { get; set; } = true;

        public SelectList? Schools { get; set; }
        public SelectList? Roles { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        [Display(Name = "Email or Username")]
        public string EmailOrUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
