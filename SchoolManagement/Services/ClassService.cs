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
    public interface IClassService
    {
        Task<List<ClassItemViewModel>> GetClassesWithSectionsAsync(int schoolId);
        Task<Class?> GetClassByIdAsync(int id, int? schoolId = null);
        Task<Class> CreateClassAsync(ClassCreateEditViewModel model);
        Task<Class?> UpdateClassAsync(ClassCreateEditViewModel model);
        Task<bool> DeleteClassAsync(int id, int? schoolId = null);

        Task<List<Section>> GetSectionsByClassAsync(int classId, int? schoolId = null);
        Task<Section?> GetSectionByIdAsync(int id, int? schoolId = null);
        Task<Section> CreateSectionAsync(SectionCreateEditViewModel model);
        Task<Section?> UpdateSectionAsync(SectionCreateEditViewModel model);
        Task<bool> DeleteSectionAsync(int id, int? schoolId = null);

        Task<ClassDashboardViewModel?> GetClassDashboardAsync(int classId, int sectionId, DateTime date, int? schoolId = null);
    }

    public class ClassService : IClassService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public ClassService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<List<ClassItemViewModel>> GetClassesWithSectionsAsync(int schoolId)
        {
            var classes = await _context.Classes
                .Include(c => c.Sections.Where(s => s.IsActive)).ThenInclude(s => s.ClassTeacher)
                .Include(c => c.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                .Where(c => c.SchoolId == schoolId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

            return classes.Select(c => new ClassItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                DisplayOrder = c.DisplayOrder,
                SectionCount = c.Sections.Count,
                StudentCount = c.Students.Count,
                Sections = c.Sections.ToList()
            }).ToList();
        }

        public async Task<Class?> GetClassByIdAsync(int id, int? schoolId = null)
        {
            var query = _context.Classes
                .Include(c => c.Sections).ThenInclude(s => s.ClassTeacher)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(c => c.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Class> CreateClassAsync(ClassCreateEditViewModel model)
        {
            var entity = new Class
            {
                SchoolId = model.SchoolId,
                Name = model.Name,
                DisplayOrder = model.DisplayOrder,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Create", "Class", entity.Id.ToString(), $"Created class: {entity.Name}", model.SchoolId);
            return entity;
        }

        public async Task<Class?> UpdateClassAsync(ClassCreateEditViewModel model)
        {
            var entity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == model.Id && c.SchoolId == model.SchoolId);
            if (entity == null) return null;

            entity.Name = model.Name;
            entity.DisplayOrder = model.DisplayOrder;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "Class", entity.Id.ToString(), $"Updated class: {entity.Name}", model.SchoolId);
            return entity;
        }

        public async Task<bool> DeleteClassAsync(int id, int? schoolId = null)
        {
            var query = _context.Classes.AsQueryable();
            if (schoolId.HasValue) query = query.Where(c => c.SchoolId == schoolId.Value);

            var entity = await query.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null) return false;

            entity.IsActive = false;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Delete", "Class", id.ToString(), $"Deactivated class: {entity.Name}", entity.SchoolId);
            return true;
        }

        public async Task<List<Section>> GetSectionsByClassAsync(int classId, int? schoolId = null)
        {
            var query = _context.Sections
                .Include(s => s.ClassTeacher)
                .Where(s => s.ClassId == classId && s.IsActive);

            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            return await query.OrderBy(s => s.Name).AsNoTracking().ToListAsync();
        }

        public async Task<Section?> GetSectionByIdAsync(int id, int? schoolId = null)
        {
            var query = _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .AsQueryable();

            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            return await query.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Section> CreateSectionAsync(SectionCreateEditViewModel model)
        {
            var section = new Section
            {
                SchoolId = model.SchoolId,
                ClassId = model.ClassId,
                Name = model.Name,
                RoomNumber = model.RoomNumber,
                Capacity = model.Capacity,
                ClassTeacherId = model.ClassTeacherId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _context.Sections.Add(section);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Create", "Section", section.Id.ToString(), $"Created section: {section.Name}", model.SchoolId);
            return section;
        }

        public async Task<Section?> UpdateSectionAsync(SectionCreateEditViewModel model)
        {
            var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == model.Id && s.SchoolId == model.SchoolId);
            if (section == null) return null;

            section.ClassId = model.ClassId;
            section.Name = model.Name;
            section.RoomNumber = model.RoomNumber;
            section.Capacity = model.Capacity;
            section.ClassTeacherId = model.ClassTeacherId;

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "Section", section.Id.ToString(), $"Updated section: {section.Name}", model.SchoolId);
            return section;
        }

        public async Task<bool> DeleteSectionAsync(int id, int? schoolId = null)
        {
            var query = _context.Sections.AsQueryable();
            if (schoolId.HasValue) query = query.Where(s => s.SchoolId == schoolId.Value);

            var entity = await query.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null) return false;

            entity.IsActive = false;
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Delete", "Section", id.ToString(), $"Deactivated section: {entity.Name}", entity.SchoolId);
            return true;
        }

        public async Task<ClassDashboardViewModel?> GetClassDashboardAsync(int classId, int sectionId, DateTime date, int? schoolId = null)
        {
            var query = _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Where(s => s.Id == sectionId && s.ClassId == classId);

            if (schoolId.HasValue)
                query = query.Where(s => s.SchoolId == schoolId.Value);

            var section = await query.FirstOrDefaultAsync();
            if (section == null) return null;

            var students = await _context.Students
                .Where(s => s.ClassId == classId && s.SectionId == sectionId && s.IsActive && s.Status == StudentStatus.Active)
                .OrderBy(s => s.RollNumber)
                .ToListAsync();

            var attendances = await _context.StudentAttendances
                .Where(a => a.ClassId == classId && a.SectionId == sectionId && a.AttendanceDate == date.Date)
                .ToDictionaryAsync(a => a.StudentId, a => a);

            var studentItems = new List<ClassStudentAttendanceItem>();
            int present = 0, absent = 0, leave = 0, late = 0;

            foreach (var st in students)
            {
                AttendanceStatus? status = null;
                string? remarks = null;

                if (attendances.TryGetValue(st.Id, out var att))
                {
                    status = att.Status;
                    remarks = att.Remarks;

                    if (att.Status == AttendanceStatus.Present) present++;
                    else if (att.Status == AttendanceStatus.Absent) absent++;
                    else if (att.Status == AttendanceStatus.Leave) leave++;
                    else if (att.Status == AttendanceStatus.Late) late++;
                }

                studentItems.Add(new ClassStudentAttendanceItem
                {
                    StudentId = st.Id,
                    RollNumber = st.RollNumber,
                    AdmissionNumber = st.AdmissionNumber,
                    StudentName = st.Name,
                    Photo = st.Photo,
                    Status = status,
                    Remarks = remarks
                });
            }

            return new ClassDashboardViewModel
            {
                Class = section.Class,
                Section = section,
                ClassTeacher = section.ClassTeacher,
                Date = date,
                TotalStudents = students.Count,
                Present = present,
                Absent = absent,
                Leave = leave,
                Late = late,
                Students = studentItems
            };
        }
    }
}
