using System;
using System.Security.Claims;

namespace SchoolManagement.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetSchoolId(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            var claim = user.FindFirst("SchoolId");
            if (claim != null && int.TryParse(claim.Value, out var schoolId))
            {
                return schoolId;
            }

            return null;
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            if (user == null) return "User";
            return user.FindFirst("FullName")?.Value ?? user.Identity?.Name ?? "User";
        }

        public static bool IsSuperAdmin(this ClaimsPrincipal user)
        {
            return user?.IsInRole("Super Admin") ?? false;
        }

        public static bool IsSchoolAdmin(this ClaimsPrincipal user)
        {
            return user?.IsInRole("School Admin") ?? false;
        }

        public static bool IsPrincipal(this ClaimsPrincipal user)
        {
            return user?.IsInRole("Principal") ?? false;
        }

        public static bool IsTeacher(this ClaimsPrincipal user)
        {
            return user?.IsInRole("Teacher") ?? false;
        }

        public static bool IsStudent(this ClaimsPrincipal user)
        {
            return user?.IsInRole("Student") ?? false;
        }

        public static bool IsParent(this ClaimsPrincipal user)
        {
            return user?.IsInRole("Parent") ?? false;
        }
    }
}
