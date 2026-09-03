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
    public interface IStudentService
    {
        Task<StudentListViewModel> GetStudentsAsync(int schoolId, int? classId, int? sectionId, string? search, StudentStatus? status, int page, int pageSize);
        Task<Student?> GetStudentByIdAsync(int id, int? schoolId = null);
        Task<Student> CreateStudentAsync(StudentCreateEditViewModel model, string createdBy);
        Task<Student?> UpdateStudentAsync(StudentCreateEditViewModel model, string updatedBy);
        Task<bool> DeleteStudentAsync(int id, int? schoolId = null);
        Task<StudentProfileViewModel?> GetStudentProfileAsync(int id, int? schoolId = null);
        Task<List<Student>> GetStudentsBySectionAsync(int schoolId, int classId, int sectionId);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public StudentService(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        public async Task<StudentListViewModel> GetStudentsAsync(int schoolId, int? classId, int? sectionId, string? search, StudentStatus? status, int page, int pageSize)
        {
            var query = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Include(s => s.AcademicYear)
                .Where(s => s.SchoolId == schoolId && s.IsActive);

            if (classId.HasValue && classId > 0)
                query = query.Where(s => s.ClassId == classId.Value);

            if (sectionId.HasValue && sectionId > 0)
                query = query.Where(s => s.SectionId == sectionId.Value);

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(term) ||
                                         s.AdmissionNumber.ToLower().Contains(term) ||
                                         s.FatherName.ToLower().Contains(term) ||
                                         s.FatherMobile.Contains(term));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var pageIndex = Math.Max(1, page);

            var items = await query
                .OrderBy(s => s.Class.DisplayOrder)
                .ThenBy(s => s.Section.Name)
                .ThenBy(s => s.RollNumber)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new StudentListViewModel
            {
                Students = items,
                SelectedClassId = classId,
                SelectedSectionId = sectionId,
                SearchTerm = search,
                SelectedStatus = status,
                PageIndex = pageIndex,
                TotalPages = totalPages > 0 ? totalPages : 1,
                TotalCount = totalCount
            };
        }

        public async Task<Student?> GetStudentByIdAsync(int id, int? schoolId = null)
        {
            var query = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Include(s => s.AcademicYear)
                .Include(s => s.School)
                .Include(s => s.StudentParents).ThenInclude(sp => sp.Parent)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student> CreateStudentAsync(StudentCreateEditViewModel model, string createdBy)
        {
            string? photoPath = null;
            if (model.PhotoFile != null)
                photoPath = await _fileService.UploadFileAsync(model.PhotoFile, "students");

            var student = new Student
            {
                SchoolId = model.SchoolId,
                AcademicYearId = model.AcademicYearId,
                ClassId = model.ClassId,
                SectionId = model.SectionId,
                AdmissionNumber = model.AdmissionNumber,
                RollNumber = model.RollNumber,
                Name = model.Name,
                Photo = photoPath,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                BloodGroup = model.BloodGroup,
                FatherName = model.FatherName,
                MotherName = model.MotherName,
                GuardianName = model.GuardianName,
                FatherMobile = model.FatherMobile,
                MotherMobile = model.MotherMobile,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                State = model.State,
                PinCode = model.PinCode,
                AdmissionDate = model.AdmissionDate,
                Status = model.Status,
                CreatedDate = DateTime.UtcNow
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Link/create parent entity
            var parent = new Parent
            {
                SchoolId = model.SchoolId,
                Name = model.FatherName,
                Mobile = model.FatherMobile,
                Email = model.Email,
                Address = model.Address
            };
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            _context.StudentParents.Add(new StudentParent
            {
                StudentId = student.Id,
                ParentId = parent.Id,
                Relationship = "Father"
            });
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Create", "Student", student.Id.ToString(), $"Admitted student: {student.Name} (Adm: {student.AdmissionNumber})", model.SchoolId);
            return student;
        }

        public async Task<Student?> UpdateStudentAsync(StudentCreateEditViewModel model, string updatedBy)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == model.Id && s.SchoolId == model.SchoolId);
            if (student == null) return null;

            if (model.PhotoFile != null)
            {
                _fileService.DeleteFile(student.Photo);
                student.Photo = await _fileService.UploadFileAsync(model.PhotoFile, "students");
            }

            student.AcademicYearId = model.AcademicYearId;
            student.ClassId = model.ClassId;
            student.SectionId = model.SectionId;
            student.AdmissionNumber = model.AdmissionNumber;
            student.RollNumber = model.RollNumber;
            student.Name = model.Name;
            student.DateOfBirth = model.DateOfBirth;
            student.Gender = model.Gender;
            student.BloodGroup = model.BloodGroup;
            student.FatherName = model.FatherName;
            student.MotherName = model.MotherName;
            student.GuardianName = model.GuardianName;
            student.FatherMobile = model.FatherMobile;
            student.MotherMobile = model.MotherMobile;
            student.Email = model.Email;
            student.Address = model.Address;
            student.City = model.City;
            student.State = model.State;
            student.PinCode = model.PinCode;
            student.AdmissionDate = model.AdmissionDate;
            student.Status = model.Status;
            student.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "Student", student.Id.ToString(), $"Updated student: {student.Name}", model.SchoolId);
            return student;
        }

        public async Task<bool> DeleteStudentAsync(int id, int? schoolId = null)
        {
            var query = _context.Students.AsQueryable();
            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            var student = await query.FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return false;

            student.IsActive = false;
            student.Status = StudentStatus.LeftSchool;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Delete", "Student", id.ToString(), $"Deactivated student: {student.Name}", student.SchoolId);
            return true;
        }

        public async Task<StudentProfileViewModel?> GetStudentProfileAsync(int id, int? schoolId = null)
        {
            var student = await GetStudentByIdAsync(id, schoolId);
            if (student == null) return null;

            var attendances = await _context.StudentAttendances
                .Where(a => a.StudentId == id)
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

            return new StudentProfileViewModel
            {
                Student = student,
                TotalWorkingDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                LateDays = lateDays,
                RecentAttendances = attendances.Take(30).ToList(),
                MonthlyAttendances = monthly
            };
        }

        public async Task<List<Student>> GetStudentsBySectionAsync(int schoolId, int classId, int sectionId)
        {
            return await _context.Students
                .Where(s => s.SchoolId == schoolId && s.ClassId == classId && s.SectionId == sectionId && s.IsActive && s.Status == StudentStatus.Active)
                .OrderBy(s => s.RollNumber)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
