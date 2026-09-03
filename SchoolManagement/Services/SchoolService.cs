using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Models;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Services
{
    public interface ISchoolService
    {
        Task<List<School>> GetAllSchoolsAsync();
        Task<List<School>> GetActiveSchoolsAsync();
        Task<School?> GetSchoolByIdAsync(int id);
        Task<School?> GetSchoolByCodeAsync(string code);
        Task<School> CreateSchoolAsync(SchoolCreateEditViewModel model, string createdBy);
        Task<School?> UpdateSchoolAsync(SchoolCreateEditViewModel model, string updatedBy);
        Task<bool> DeleteSchoolAsync(int id);
        Task<SchoolPublicProfileViewModel?> GetPublicProfileAsync(string schoolCode);
        Task<SchoolAvailabilityViewModel> GetSchoolAvailabilityOverviewAsync();
    }

    public class SchoolService : ISchoolService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public SchoolService(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        public async Task<List<School>> GetAllSchoolsAsync()
        {
            return await _context.Schools
                .Include(s => s.Principal)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<School>> GetActiveSchoolsAsync()
        {
            return await _context.Schools
                .Where(s => s.IsActive && s.Status == SchoolStatus.Active)
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<School?> GetSchoolByIdAsync(int id)
        {
            return await _context.Schools
                .Include(s => s.Principal)
                .Include(s => s.AcademicYears)
                .Include(s => s.Classes)
                .Include(s => s.Images)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<School?> GetSchoolByCodeAsync(string code)
        {
            return await _context.Schools
                .Include(s => s.Principal)
                .Include(s => s.AcademicYears)
                .FirstOrDefaultAsync(s => s.Code.ToLower() == code.ToLower());
        }

        public async Task<School> CreateSchoolAsync(SchoolCreateEditViewModel model, string createdBy)
        {
            string? logoPath = null;
            string? bannerPath = null;
            string? principalPhotoPath = null;

            if (model.LogoFile != null)
                logoPath = await _fileService.UploadFileAsync(model.LogoFile, "schools");

            if (model.BannerFile != null)
                bannerPath = await _fileService.UploadFileAsync(model.BannerFile, "schools");

            if (model.PrincipalPhotoFile != null)
                principalPhotoPath = await _fileService.UploadFileAsync(model.PrincipalPhotoFile, "schools");

            var school = new School
            {
                Name = model.Name,
                Code = model.Code.ToUpper(),
                RegistrationNumber = model.RegistrationNumber,
                Logo = logoPath,
                Banner = bannerPath,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PinCode = model.PinCode,
                Phone = model.Phone,
                Email = model.Email,
                Website = model.Website,
                EstablishedYear = model.EstablishedYear,
                Status = model.Status,
                About = model.About,
                Vision = model.Vision,
                Mission = model.Mission,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(model.PrincipalName))
            {
                school.Principal = new Principal
                {
                    Name = model.PrincipalName,
                    Photo = principalPhotoPath,
                    Qualification = model.PrincipalQualification,
                    Experience = model.PrincipalExperience,
                    Phone = model.PrincipalPhone,
                    Email = model.PrincipalEmail,
                    Message = model.PrincipalMessage,
                    Vision = model.Vision,
                    CreatedDate = DateTime.UtcNow
                };
            }

            _context.Schools.Add(school);
            await _context.SaveChangesAsync();

            // Create default Academic Year
            var currentYear = DateTime.Now.Year;
            var academicYear = new AcademicYear
            {
                SchoolId = school.Id,
                Name = $"{currentYear}-{currentYear + 1}",
                StartDate = new DateTime(currentYear, 4, 1),
                EndDate = new DateTime(currentYear + 1, 3, 31),
                IsCurrent = true,
                IsActive = true
            };
            _context.AcademicYears.Add(academicYear);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Create", "School", school.Id.ToString(), $"Created school: {school.Name}", school.Id);
            return school;
        }

        public async Task<School?> UpdateSchoolAsync(SchoolCreateEditViewModel model, string updatedBy)
        {
            var school = await _context.Schools
                .Include(s => s.Principal)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (school == null) return null;

            if (model.LogoFile != null)
            {
                _fileService.DeleteFile(school.Logo);
                school.Logo = await _fileService.UploadFileAsync(model.LogoFile, "schools");
            }

            if (model.BannerFile != null)
            {
                _fileService.DeleteFile(school.Banner);
                school.Banner = await _fileService.UploadFileAsync(model.BannerFile, "schools");
            }

            school.Name = model.Name;
            school.Code = model.Code.ToUpper();
            school.RegistrationNumber = model.RegistrationNumber;
            school.Address = model.Address;
            school.City = model.City;
            school.State = model.State;
            school.PinCode = model.PinCode;
            school.Phone = model.Phone;
            school.Email = model.Email;
            school.Website = model.Website;
            school.EstablishedYear = model.EstablishedYear;
            school.Status = model.Status;
            school.About = model.About;
            school.Vision = model.Vision;
            school.Mission = model.Mission;
            school.UpdatedBy = updatedBy;
            school.UpdatedDate = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.PrincipalName))
            {
                if (school.Principal == null)
                {
                    school.Principal = new Principal
                    {
                        SchoolId = school.Id,
                        Name = model.PrincipalName,
                        Qualification = model.PrincipalQualification,
                        Experience = model.PrincipalExperience,
                        Phone = model.PrincipalPhone,
                        Email = model.PrincipalEmail,
                        Message = model.PrincipalMessage,
                        Vision = model.Vision
                    };
                }
                else
                {
                    school.Principal.Name = model.PrincipalName;
                    school.Principal.Qualification = model.PrincipalQualification;
                    school.Principal.Experience = model.PrincipalExperience;
                    school.Principal.Phone = model.PrincipalPhone;
                    school.Principal.Email = model.PrincipalEmail;
                    school.Principal.Message = model.PrincipalMessage;
                    school.Principal.Vision = model.Vision;
                    school.Principal.UpdatedDate = DateTime.UtcNow;
                }

                if (model.PrincipalPhotoFile != null)
                {
                    _fileService.DeleteFile(school.Principal.Photo);
                    school.Principal.Photo = await _fileService.UploadFileAsync(model.PrincipalPhotoFile, "schools");
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "School", school.Id.ToString(), $"Updated school: {school.Name}", school.Id);
            return school;
        }

        public async Task<bool> DeleteSchoolAsync(int id)
        {
            var school = await _context.Schools.FindAsync(id);
            if (school == null) return false;

            school.IsActive = false;
            school.Status = SchoolStatus.Inactive;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Deactivate", "School", id.ToString(), $"Deactivated school: {school.Name}", id);
            return true;
        }

        public async Task<SchoolPublicProfileViewModel?> GetPublicProfileAsync(string schoolCode)
        {
            var school = await _context.Schools
                .Include(s => s.Principal)
                .Include(s => s.Images)
                .Include(s => s.Classes).ThenInclude(c => c.Sections)
                .Include(s => s.Notifications.Where(n => n.IsActive).OrderByDescending(n => n.PublishDate).Take(5))
                .FirstOrDefaultAsync(s => s.Code.ToLower() == schoolCode.ToLower() && s.IsActive);

            if (school == null) return null;

            var studentCount = await _context.Students.CountAsync(s => s.SchoolId == school.Id && s.IsActive && s.Status == StudentStatus.Active);
            var teacherCount = await _context.Teachers.CountAsync(t => t.SchoolId == school.Id && t.IsActive && t.Status == TeacherStatus.Active);
            var classCount = await _context.Classes.CountAsync(c => c.SchoolId == school.Id && c.IsActive);

            return new SchoolPublicProfileViewModel
            {
                School = school,
                Principal = school.Principal,
                GalleryImages = school.Images.OrderByDescending(i => i.IsCoverImage).ThenByDescending(i => i.UploadDate).Take(12).ToList(),
                TotalStudents = studentCount,
                TotalTeachers = teacherCount,
                TotalClasses = classCount,
                Classes = school.Classes.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToList(),
                Notices = school.Notifications.ToList()
            };
        }

        public async Task<SchoolAvailabilityViewModel> GetSchoolAvailabilityOverviewAsync()
        {
            var today = DateTime.Today;
            var schools = await _context.Schools.Where(s => s.IsActive).ToListAsync();
            var result = new SchoolAvailabilityViewModel();

            foreach (var school in schools)
            {
                var totalStudents = await _context.Students.CountAsync(s => s.SchoolId == school.Id && s.IsActive && s.Status == StudentStatus.Active);
                var totalTeachers = await _context.Teachers.CountAsync(t => t.SchoolId == school.Id && t.IsActive && t.Status == TeacherStatus.Active);
                var totalClasses = await _context.Classes.CountAsync(c => c.SchoolId == school.Id && c.IsActive);

                var presentStudents = await _context.StudentAttendances
                    .CountAsync(a => a.SchoolId == school.Id && a.AttendanceDate == today && a.Status == AttendanceStatus.Present);

                var presentTeachers = await _context.TeacherAttendances
                    .CountAsync(a => a.SchoolId == school.Id && a.AttendanceDate == today && a.Status == AttendanceStatus.Present);

                var rate = totalStudents > 0 ? Math.Round((double)presentStudents / totalStudents * 100, 1) : 0.0;
                var isOpen = school.Status == SchoolStatus.Active && today.DayOfWeek != DayOfWeek.Sunday;

                result.Schools.Add(new SchoolAvailabilityItem
                {
                    SchoolId = school.Id,
                    SchoolName = school.Name,
                    SchoolCode = school.Code,
                    Status = school.Status,
                    TotalStudents = totalStudents,
                    PresentStudentsToday = presentStudents,
                    TotalTeachers = totalTeachers,
                    PresentTeachersToday = presentTeachers,
                    TotalClasses = totalClasses,
                    AttendanceRate = rate,
                    IsOpenToday = isOpen
                });
            }

            return result;
        }
    }
}
