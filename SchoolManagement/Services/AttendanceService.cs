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
    public interface IAttendanceService
    {
        Task<StudentAttendanceSheetViewModel> GetStudentAttendanceSheetAsync(int schoolId, int classId, int sectionId, DateTime date, int academicYearId);
        Task<(bool Success, string Message, int Count)> SaveStudentAttendanceAsync(StudentAttendanceSheetViewModel model, string recordedBy);
        Task<TeacherAttendanceSheetViewModel> GetTeacherAttendanceSheetAsync(int schoolId, DateTime date, int academicYearId);
        Task<(bool Success, string Message, int Count)> SaveTeacherAttendanceAsync(TeacherAttendanceSheetViewModel model, string recordedBy);
        Task<DailySummaryViewModel> GetDailySummaryAsync(int schoolId, DateTime date);
        Task<AttendanceAnalyticsViewModel> GetAttendanceAnalyticsAsync(AttendanceAnalyticsViewModel filter);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AttendanceService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<StudentAttendanceSheetViewModel> GetStudentAttendanceSheetAsync(int schoolId, int classId, int sectionId, DateTime date, int academicYearId)
        {
            var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId && c.SchoolId == schoolId);
            var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == sectionId && s.ClassId == classId && s.SchoolId == schoolId);

            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId && s.ClassId == classId && s.SectionId == sectionId && s.IsActive && s.Status == StudentStatus.Active)
                .OrderBy(s => s.RollNumber)
                .AsNoTracking()
                .ToListAsync();

            var existingAttendance = await _context.StudentAttendances
                .Where(a => a.SchoolId == schoolId && a.ClassId == classId && a.SectionId == sectionId && a.AttendanceDate == date.Date && a.AcademicYearId == academicYearId)
                .ToDictionaryAsync(a => a.StudentId, a => a);

            var isAlreadyMarked = existingAttendance.Any();

            var studentItems = students.Select(st =>
            {
                var hasRec = existingAttendance.TryGetValue(st.Id, out var att);
                return new StudentAttendanceEntryItem
                {
                    StudentId = st.Id,
                    RollNumber = st.RollNumber,
                    AdmissionNumber = st.AdmissionNumber,
                    StudentName = st.Name,
                    Photo = st.Photo,
                    Status = hasRec && att != null ? att.Status : AttendanceStatus.Present,
                    Remarks = hasRec && att != null ? att.Remarks : null
                };
            }).ToList();

            return new StudentAttendanceSheetViewModel
            {
                SchoolId = schoolId,
                ClassId = classId,
                SectionId = sectionId,
                ClassName = @class?.Name ?? string.Empty,
                SectionName = section?.Name ?? string.Empty,
                AttendanceDate = date.Date,
                AcademicYearId = academicYearId,
                IsAlreadyMarked = isAlreadyMarked,
                Students = studentItems
            };
        }

        public async Task<(bool Success, string Message, int Count)> SaveStudentAttendanceAsync(StudentAttendanceSheetViewModel model, string recordedBy)
        {
            if (model.Students == null || !model.Students.Any())
                return (false, "No students found to mark attendance.", 0);

            var date = model.AttendanceDate.Date;

            // Load existing records to update or insert
            var existingRecords = await _context.StudentAttendances
                .Where(a => a.SchoolId == model.SchoolId && a.ClassId == model.ClassId && a.SectionId == model.SectionId && a.AttendanceDate == date && a.AcademicYearId == model.AcademicYearId)
                .ToListAsync();

            var existingMap = existingRecords.ToDictionary(a => a.StudentId);
            int savedCount = 0;

            foreach (var item in model.Students)
            {
                if (existingMap.TryGetValue(item.StudentId, out var existing))
                {
                    existing.Status = item.Status;
                    existing.Remarks = item.Remarks;
                    existing.RecordedDate = DateTime.UtcNow;
                    existing.RecordedBy = recordedBy;
                }
                else
                {
                    var newAtt = new StudentAttendance
                    {
                        SchoolId = model.SchoolId,
                        AcademicYearId = model.AcademicYearId,
                        ClassId = model.ClassId,
                        SectionId = model.SectionId,
                        StudentId = item.StudentId,
                        AttendanceDate = date,
                        Status = item.Status,
                        Remarks = item.Remarks,
                        RecordedDate = DateTime.UtcNow,
                        RecordedBy = recordedBy
                    };
                    _context.StudentAttendances.Add(newAtt);
                }
                savedCount++;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("AttendanceMarked", "StudentAttendance", $"{model.ClassId}_{model.SectionId}_{date:yyyyMMdd}", $"Marked attendance for {savedCount} students on {date:d}", model.SchoolId);

            return (true, $"Attendance successfully saved for {savedCount} students.", savedCount);
        }

        public async Task<TeacherAttendanceSheetViewModel> GetTeacherAttendanceSheetAsync(int schoolId, DateTime date, int academicYearId)
        {
            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active)
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();

            var existingAttendance = await _context.TeacherAttendances
                .Where(a => a.SchoolId == schoolId && a.AttendanceDate == date.Date && a.AcademicYearId == academicYearId)
                .ToDictionaryAsync(a => a.TeacherId, a => a);

            var isAlreadyMarked = existingAttendance.Any();

            var teacherItems = teachers.Select(t =>
            {
                var hasRec = existingAttendance.TryGetValue(t.Id, out var att);
                return new TeacherAttendanceEntryItem
                {
                    TeacherId = t.Id,
                    EmployeeId = t.EmployeeId,
                    TeacherName = t.Name,
                    Designation = t.Designation,
                    Photo = t.Photo,
                    Status = hasRec && att != null ? att.Status : AttendanceStatus.Present,
                    Remarks = hasRec && att != null ? att.Remarks : null
                };
            }).ToList();

            return new TeacherAttendanceSheetViewModel
            {
                SchoolId = schoolId,
                AttendanceDate = date.Date,
                AcademicYearId = academicYearId,
                IsAlreadyMarked = isAlreadyMarked,
                Teachers = teacherItems
            };
        }

        public async Task<(bool Success, string Message, int Count)> SaveTeacherAttendanceAsync(TeacherAttendanceSheetViewModel model, string recordedBy)
        {
            if (model.Teachers == null || !model.Teachers.Any())
                return (false, "No teachers found to mark attendance.", 0);

            var date = model.AttendanceDate.Date;

            var existingRecords = await _context.TeacherAttendances
                .Where(a => a.SchoolId == model.SchoolId && a.AttendanceDate == date && a.AcademicYearId == model.AcademicYearId)
                .ToListAsync();

            var existingMap = existingRecords.ToDictionary(a => a.TeacherId);
            int savedCount = 0;

            foreach (var item in model.Teachers)
            {
                if (existingMap.TryGetValue(item.TeacherId, out var existing))
                {
                    existing.Status = item.Status;
                    existing.Remarks = item.Remarks;
                    existing.RecordedDate = DateTime.UtcNow;
                    existing.RecordedBy = recordedBy;
                }
                else
                {
                    var newAtt = new TeacherAttendance
                    {
                        SchoolId = model.SchoolId,
                        AcademicYearId = model.AcademicYearId,
                        TeacherId = item.TeacherId,
                        AttendanceDate = date,
                        Status = item.Status,
                        Remarks = item.Remarks,
                        RecordedDate = DateTime.UtcNow,
                        RecordedBy = recordedBy
                    };
                    _context.TeacherAttendances.Add(newAtt);
                }
                savedCount++;
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("AttendanceMarked", "TeacherAttendance", $"{model.SchoolId}_{date:yyyyMMdd}", $"Marked attendance for {savedCount} teachers on {date:d}", model.SchoolId);

            return (true, $"Teacher attendance successfully saved for {savedCount} staff members.", savedCount);
        }

        public async Task<DailySummaryViewModel> GetDailySummaryAsync(int schoolId, DateTime date)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            var dateOnly = date.Date;

            var totalStudents = await _context.Students.CountAsync(s => s.SchoolId == schoolId && s.IsActive && s.Status == StudentStatus.Active);
            var totalTeachers = await _context.Teachers.CountAsync(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active);

            var studentAtt = await _context.StudentAttendances
                .Where(a => a.SchoolId == schoolId && a.AttendanceDate == dateOnly)
                .ToListAsync();

            var teacherAtt = await _context.TeacherAttendances
                .Where(a => a.SchoolId == schoolId && a.AttendanceDate == dateOnly)
                .ToListAsync();

            var sections = await _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Include(s => s.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                .Where(s => s.SchoolId == schoolId && s.IsActive)
                .OrderBy(s => s.Class.DisplayOrder).ThenBy(s => s.Name)
                .ToListAsync();

            var classSummaries = sections.Select(sec =>
            {
                var secAtt = studentAtt.Where(a => a.SectionId == sec.Id).ToList();
                return new ClassAttendanceSummary
                {
                    ClassId = sec.ClassId,
                    ClassName = sec.Class.Name,
                    SectionId = sec.Id,
                    SectionName = sec.Name,
                    ClassTeacherName = sec.ClassTeacher?.Name,
                    TotalStudents = sec.Students.Count,
                    Present = secAtt.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = secAtt.Count(a => a.Status == AttendanceStatus.Absent),
                    Leave = secAtt.Count(a => a.Status == AttendanceStatus.Leave),
                    Late = secAtt.Count(a => a.Status == AttendanceStatus.Late)
                };
            }).ToList();

            return new DailySummaryViewModel
            {
                Date = dateOnly,
                School = school!,
                TotalStudents = totalStudents,
                StudentsPresent = studentAtt.Count(a => a.Status == AttendanceStatus.Present),
                StudentsAbsent = studentAtt.Count(a => a.Status == AttendanceStatus.Absent),
                StudentsLeave = studentAtt.Count(a => a.Status == AttendanceStatus.Leave),
                StudentsLate = studentAtt.Count(a => a.Status == AttendanceStatus.Late),
                TotalTeachers = totalTeachers,
                TeachersPresent = teacherAtt.Count(a => a.Status == AttendanceStatus.Present),
                TeachersAbsent = teacherAtt.Count(a => a.Status == AttendanceStatus.Absent),
                TeachersLeave = teacherAtt.Count(a => a.Status == AttendanceStatus.Leave),
                TeachersLate = teacherAtt.Count(a => a.Status == AttendanceStatus.Late),
                ClassSummaries = classSummaries
            };
        }

        public async Task<AttendanceAnalyticsViewModel> GetAttendanceAnalyticsAsync(AttendanceAnalyticsViewModel filter)
        {
            var query = _context.StudentAttendances
                .Where(a => a.AttendanceDate >= filter.DateFrom.Date && a.AttendanceDate <= filter.DateTo.Date);

            if (filter.SchoolId.HasValue && filter.SchoolId > 0)
                query = query.Where(a => a.SchoolId == filter.SchoolId.Value);

            if (filter.ClassId.HasValue && filter.ClassId > 0)
                query = query.Where(a => a.ClassId == filter.ClassId.Value);

            if (filter.SectionId.HasValue && filter.SectionId > 0)
                query = query.Where(a => a.SectionId == filter.SectionId.Value);

            if (filter.StudentId.HasValue && filter.StudentId > 0)
                query = query.Where(a => a.StudentId == filter.StudentId.Value);

            var attendances = await query.ToListAsync();

            filter.TotalRecords = attendances.Count;
            filter.TotalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present);
            filter.TotalAbsent = attendances.Count(a => a.Status == AttendanceStatus.Absent);
            filter.TotalLeave = attendances.Count(a => a.Status == AttendanceStatus.Leave);
            filter.TotalLate = attendances.Count(a => a.Status == AttendanceStatus.Late);

            var groupedByDate = attendances
                .GroupBy(a => a.AttendanceDate)
                .OrderBy(g => g.Key)
                .ToList();

            filter.ChartDates = groupedByDate.Select(g => g.Key.ToString("dd MMM")).ToList();
            filter.ChartPresentCounts = groupedByDate.Select(g => g.Count(a => a.Status == AttendanceStatus.Present)).ToList();
            filter.ChartAbsentCounts = groupedByDate.Select(g => g.Count(a => a.Status == AttendanceStatus.Absent)).ToList();

            return filter;
        }
    }
}
