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
    public interface IReportService
    {
        Task<List<StudentAttendanceReportItem>> GetStudentAttendanceReportAsync(ReportFilterViewModel filter);
        Task<List<TeacherAttendanceReportItem>> GetTeacherAttendanceReportAsync(ReportFilterViewModel filter);
        Task<List<ClassAttendanceReportItem>> GetClassAttendanceReportAsync(ReportFilterViewModel filter);
        Task<SchoolSummaryReportViewModel?> GetSchoolSummaryReportAsync(int schoolId, int? academicYearId);
    }

    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StudentAttendanceReportItem>> GetStudentAttendanceReportAsync(ReportFilterViewModel filter)
        {
            var studentQuery = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Where(s => s.IsActive && s.Status == StudentStatus.Active);

            if (filter.SchoolId.HasValue && filter.SchoolId > 0)
                studentQuery = studentQuery.Where(s => s.SchoolId == filter.SchoolId.Value);

            if (filter.ClassId.HasValue && filter.ClassId > 0)
                studentQuery = studentQuery.Where(s => s.ClassId == filter.ClassId.Value);

            if (filter.SectionId.HasValue && filter.SectionId > 0)
                studentQuery = studentQuery.Where(s => s.SectionId == filter.SectionId.Value);

            if (filter.StudentId.HasValue && filter.StudentId > 0)
                studentQuery = studentQuery.Where(s => s.Id == filter.StudentId.Value);

            var students = await studentQuery
                .OrderBy(s => s.Class.DisplayOrder)
                .ThenBy(s => s.Section.Name)
                .ThenBy(s => s.RollNumber)
                .ToListAsync();

            var studentIds = students.Select(s => s.Id).ToList();

            var attendances = await _context.StudentAttendances
                .Where(a => studentIds.Contains(a.StudentId) &&
                            a.AttendanceDate >= filter.DateFrom.Date &&
                            a.AttendanceDate <= filter.DateTo.Date)
                .ToListAsync();

            var result = new List<StudentAttendanceReportItem>();

            foreach (var st in students)
            {
                var stAtt = attendances.Where(a => a.StudentId == st.Id).ToList();
                var total = stAtt.Count;
                var present = stAtt.Count(a => a.Status == AttendanceStatus.Present);
                var absent = stAtt.Count(a => a.Status == AttendanceStatus.Absent);
                var leave = stAtt.Count(a => a.Status == AttendanceStatus.Leave);
                var late = stAtt.Count(a => a.Status == AttendanceStatus.Late);

                result.Add(new StudentAttendanceReportItem
                {
                    StudentId = st.Id,
                    AdmissionNumber = st.AdmissionNumber,
                    RollNumber = st.RollNumber,
                    StudentName = st.Name,
                    ClassName = st.Class.Name,
                    SectionName = st.Section.Name,
                    TotalWorkingDays = total,
                    PresentDays = present,
                    AbsentDays = absent,
                    LeaveDays = leave,
                    LateDays = late
                });
            }

            return result;
        }

        public async Task<List<TeacherAttendanceReportItem>> GetTeacherAttendanceReportAsync(ReportFilterViewModel filter)
        {
            var teacherQuery = _context.Teachers
                .Where(t => t.IsActive && t.Status == TeacherStatus.Active);

            if (filter.SchoolId.HasValue && filter.SchoolId > 0)
                teacherQuery = teacherQuery.Where(t => t.SchoolId == filter.SchoolId.Value);

            if (filter.TeacherId.HasValue && filter.TeacherId > 0)
                teacherQuery = teacherQuery.Where(t => t.Id == filter.TeacherId.Value);

            var teachers = await teacherQuery
                .OrderBy(t => t.Name)
                .ToListAsync();

            var teacherIds = teachers.Select(t => t.Id).ToList();

            var attendances = await _context.TeacherAttendances
                .Where(a => teacherIds.Contains(a.TeacherId) &&
                            a.AttendanceDate >= filter.DateFrom.Date &&
                            a.AttendanceDate <= filter.DateTo.Date)
                .ToListAsync();

            var result = new List<TeacherAttendanceReportItem>();

            foreach (var t in teachers)
            {
                var tAtt = attendances.Where(a => a.TeacherId == t.Id).ToList();
                var total = tAtt.Count;
                var present = tAtt.Count(a => a.Status == AttendanceStatus.Present);
                var absent = tAtt.Count(a => a.Status == AttendanceStatus.Absent);
                var leave = tAtt.Count(a => a.Status == AttendanceStatus.Leave);
                var late = tAtt.Count(a => a.Status == AttendanceStatus.Late);

                result.Add(new TeacherAttendanceReportItem
                {
                    TeacherId = t.Id,
                    EmployeeId = t.EmployeeId,
                    TeacherName = t.Name,
                    Designation = t.Designation,
                    Subject = t.Subject,
                    TotalWorkingDays = total,
                    PresentDays = present,
                    AbsentDays = absent,
                    LeaveDays = leave,
                    LateDays = late
                });
            }

            return result;
        }

        public async Task<List<ClassAttendanceReportItem>> GetClassAttendanceReportAsync(ReportFilterViewModel filter)
        {
            var sectionQuery = _context.Sections
                .Include(s => s.Class)
                .Include(s => s.ClassTeacher)
                .Include(s => s.Students.Where(st => st.IsActive && st.Status == StudentStatus.Active))
                .Where(s => s.IsActive);

            if (filter.SchoolId.HasValue && filter.SchoolId > 0)
                sectionQuery = sectionQuery.Where(s => s.SchoolId == filter.SchoolId.Value);

            if (filter.ClassId.HasValue && filter.ClassId > 0)
                sectionQuery = sectionQuery.Where(s => s.ClassId == filter.ClassId.Value);

            if (filter.SectionId.HasValue && filter.SectionId > 0)
                sectionQuery = sectionQuery.Where(s => s.Id == filter.SectionId.Value);

            var sections = await sectionQuery
                .OrderBy(s => s.Class.DisplayOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();

            var sectionIds = sections.Select(s => s.Id).ToList();

            var attendances = await _context.StudentAttendances
                .Where(a => sectionIds.Contains(a.SectionId) &&
                            a.AttendanceDate >= filter.DateFrom.Date &&
                            a.AttendanceDate <= filter.DateTo.Date)
                .ToListAsync();

            var result = new List<ClassAttendanceReportItem>();

            foreach (var sec in sections)
            {
                var secAtt = attendances.Where(a => a.SectionId == sec.Id).ToList();
                var total = secAtt.Count;
                var present = secAtt.Count(a => a.Status == AttendanceStatus.Present);
                var absent = secAtt.Count(a => a.Status == AttendanceStatus.Absent);
                var leave = secAtt.Count(a => a.Status == AttendanceStatus.Leave);

                var rate = total > 0 ? Math.Round((double)present / total * 100, 1) : 0.0;

                result.Add(new ClassAttendanceReportItem
                {
                    ClassName = sec.Class.Name,
                    SectionName = sec.Name,
                    ClassTeacherName = sec.ClassTeacher?.Name,
                    TotalStudents = sec.Students.Count,
                    TotalPresent = present,
                    TotalAbsent = absent,
                    TotalLeave = leave,
                    AverageAttendanceRate = rate
                });
            }

            return result;
        }

        public async Task<SchoolSummaryReportViewModel?> GetSchoolSummaryReportAsync(int schoolId, int? academicYearId)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Id == schoolId);
            if (school == null) return null;

            var academicYear = academicYearId.HasValue
                ? await _context.AcademicYears.FirstOrDefaultAsync(a => a.Id == academicYearId.Value)
                : await _context.AcademicYears.FirstOrDefaultAsync(a => a.SchoolId == schoolId && a.IsCurrent);

            var students = await _context.Students
                .Where(s => s.SchoolId == schoolId && s.IsActive && s.Status == StudentStatus.Active)
                .ToListAsync();

            var teachers = await _context.Teachers
                .Where(t => t.SchoolId == schoolId && t.IsActive && t.Status == TeacherStatus.Active)
                .ToListAsync();

            var totalClasses = await _context.Classes.CountAsync(c => c.SchoolId == schoolId && c.IsActive);
            var totalSections = await _context.Sections.CountAsync(s => s.SchoolId == schoolId && s.IsActive);

            var studentAttendances = await _context.StudentAttendances
                .Where(a => a.SchoolId == schoolId)
                .ToListAsync();

            var teacherAttendances = await _context.TeacherAttendances
                .Where(a => a.SchoolId == schoolId)
                .ToListAsync();

            var overallStudentRate = studentAttendances.Any()
                ? Math.Round((double)studentAttendances.Count(a => a.Status == AttendanceStatus.Present) / studentAttendances.Count * 100, 1)
                : 0.0;

            var overallTeacherRate = teacherAttendances.Any()
                ? Math.Round((double)teacherAttendances.Count(a => a.Status == AttendanceStatus.Present) / teacherAttendances.Count * 100, 1)
                : 0.0;

            var filter = new ReportFilterViewModel
            {
                SchoolId = schoolId,
                DateFrom = DateTime.Today.AddDays(-30),
                DateTo = DateTime.Today
            };
            var classSummaries = await GetClassAttendanceReportAsync(filter);

            return new SchoolSummaryReportViewModel
            {
                School = school,
                AcademicYear = academicYear,
                TotalStudents = students.Count,
                TotalMaleStudents = students.Count(s => s.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase)),
                TotalFemaleStudents = students.Count(s => s.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase)),
                TotalTeachers = teachers.Count,
                TotalMaleTeachers = teachers.Count(t => t.Gender.Equals("Male", StringComparison.OrdinalIgnoreCase)),
                TotalFemaleTeachers = teachers.Count(t => t.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase)),
                TotalClasses = totalClasses,
                TotalSections = totalSections,
                OverallStudentAttendanceRate = overallStudentRate,
                OverallTeacherAttendanceRate = overallTeacherRate,
                ClassSummaries = classSummaries
            };
        }
    }
}
