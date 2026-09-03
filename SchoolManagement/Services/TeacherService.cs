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
    public interface ITeacherService
    {
        Task<TeacherListViewModel> GetTeachersAsync(int schoolId, string? search, TeacherStatus? status, int page, int pageSize);
        Task<Teacher?> GetTeacherByIdAsync(int id, int? schoolId = null);
        Task<Teacher> CreateTeacherAsync(TeacherCreateEditViewModel model, string createdBy);
        Task<Teacher?> UpdateTeacherAsync(TeacherCreateEditViewModel model, string updatedBy);
        Task<bool> DeleteTeacherAsync(int id, int? schoolId = null);
        Task<TeacherProfileViewModel?> GetTeacherProfileAsync(int id, int? schoolId = null);
        Task<List<Teacher>> GetActiveTeachersAsync(int schoolId);
    }

    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public TeacherService(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        public async Task<TeacherListViewModel> GetTeachersAsync(int schoolId, string? search, TeacherStatus? status, int page, int pageSize)
        {
            var query = _context.Teachers
                .Include(t => t.ClassTeacherSections).ThenInclude(s => s.Class)
                .Where(t => t.SchoolId == schoolId && t.IsActive);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(term) ||
                                         t.EmployeeId.ToLower().Contains(term) ||
                                         t.Mobile.Contains(term) ||
                                         (t.Subject != null && t.Subject.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var pageIndex = Math.Max(1, page);

            var items = await query
                .OrderBy(t => t.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new TeacherListViewModel
            {
                Teachers = items,
                SearchTerm = search,
                SelectedStatus = status,
                PageIndex = pageIndex,
                TotalPages = totalPages > 0 ? totalPages : 1,
                TotalCount = totalCount
            };
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int id, int? schoolId = null)
        {
            var query = _context.Teachers
                .Include(t => t.ClassTeacherSections).ThenInclude(s => s.Class)
                .Include(t => t.School)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(t => t.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teacher> CreateTeacherAsync(TeacherCreateEditViewModel model, string createdBy)
        {
            string? photoPath = null;
            if (model.PhotoFile != null)
                photoPath = await _fileService.UploadFileAsync(model.PhotoFile, "teachers");

            var teacher = new Teacher
            {
                SchoolId = model.SchoolId,
                EmployeeId = model.EmployeeId,
                Name = model.Name,
                Photo = photoPath,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                Qualification = model.Qualification,
                Experience = model.Experience,
                Subject = model.Subject,
                Designation = model.Designation,
                Mobile = model.Mobile,
                Email = model.Email,
                Address = model.Address,
                JoiningDate = model.JoiningDate,
                Status = model.Status,
                CreatedDate = DateTime.UtcNow
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            if (model.AssignedSectionId.HasValue && model.AssignedSectionId.Value > 0)
            {
                var section = await _context.Sections.FindAsync(model.AssignedSectionId.Value);
                if (section != null)
                {
                    section.ClassTeacherId = teacher.Id;
                    await _context.SaveChangesAsync();
                }
            }

            await _auditService.LogAsync("Create", "Teacher", teacher.Id.ToString(), $"Registered teacher: {teacher.Name} (EmpId: {teacher.EmployeeId})", model.SchoolId);
            return teacher;
        }

        public async Task<Teacher?> UpdateTeacherAsync(TeacherCreateEditViewModel model, string updatedBy)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == model.Id && t.SchoolId == model.SchoolId);
            if (teacher == null) return null;

            if (model.PhotoFile != null)
            {
                _fileService.DeleteFile(teacher.Photo);
                teacher.Photo = await _fileService.UploadFileAsync(model.PhotoFile, "teachers");
            }

            teacher.EmployeeId = model.EmployeeId;
            teacher.Name = model.Name;
            teacher.Gender = model.Gender;
            teacher.DateOfBirth = model.DateOfBirth;
            teacher.Qualification = model.Qualification;
            teacher.Experience = model.Experience;
            teacher.Subject = model.Subject;
            teacher.Designation = model.Designation;
            teacher.Mobile = model.Mobile;
            teacher.Email = model.Email;
            teacher.Address = model.Address;
            teacher.JoiningDate = model.JoiningDate;
            teacher.Status = model.Status;
            teacher.UpdatedDate = DateTime.UtcNow;

            // Update assigned section if requested
            if (model.AssignedSectionId.HasValue)
            {
                var oldSections = await _context.Sections.Where(s => s.ClassTeacherId == teacher.Id).ToListAsync();
                foreach (var s in oldSections) s.ClassTeacherId = null;

                if (model.AssignedSectionId.Value > 0)
                {
                    var newSection = await _context.Sections.FindAsync(model.AssignedSectionId.Value);
                    if (newSection != null) newSection.ClassTeacherId = teacher.Id;
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "Teacher", teacher.Id.ToString(), $"Updated teacher: {teacher.Name}", model.SchoolId);
            return teacher;
        }

        public async Task<bool> DeleteTeacherAsync(int id, int? schoolId = null)
        {
            var query = _context.Teachers.AsQueryable();
            if (schoolId.HasValue)
                query = query.Where(t => t.SchoolId == schoolId.Value);

            var teacher = await query.FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return false;

            teacher.IsActive = false;
            teacher.Status = TeacherStatus.Resigned;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Delete", "Teacher", id.ToString(), $"Deactivated teacher: {teacher.Name}", teacher.SchoolId);
            return true;
        }

        public async Task<TeacherProfileViewModel?> GetTeacherProfileAsync(int id, int? schoolId = null)
        {
            var teacher = await GetTeacherByIdAsync(id, schoolId);
            if (teacher == null) return null;

            var attendances = await _context.TeacherAttendances
                .Where(a => a.TeacherId == id)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            var totalDays = attendances.Count;
            var presentDays = attendances.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveDays = attendances.Count(a => a.Status == AttendanceStatus.Leave);
            var lateDays = attendances.Count(a => a.Status == AttendanceStatus.Late);

            var monthly = attendances
                .GroupBy(a => new { a.AttendanceDate.Year, a.AttendanceDate.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(6)
                .Select(g => new MonthAttendanceSummary
                {
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TotalDays = g.Count(),
                    Present = g.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = g.Count(a => a.Status == AttendanceStatus.Absent),
                    Leave = g.Count(a => a.Status == AttendanceStatus.Leave),
                    Late = g.Count(a => a.Status == AttendanceStatus.Late)
                }).ToList();

            var sections = await _context.Sections
                .Include(s => s.Class)
                .Where(s => s.ClassTeacherId == id)
                .ToListAsync();

            return new TeacherProfileViewModel
            {
                Teacher = teacher,
                TotalWorkingDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                LateDays = lateDays,
                RecentAttendances = attendances.Take(30).ToList(),
                MonthlyAttendances = monthly,
                AssignedSections = sections
            };
        }

        public async Task<List<Teacher>> GetActiveTeachersAsync(int schoolId)
        {
            return await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active)
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
