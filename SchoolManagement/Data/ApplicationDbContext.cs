using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Models;

namespace SchoolManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<School> Schools { get; set; } = null!;
        public DbSet<Principal> Principals { get; set; } = null!;
        public DbSet<SchoolImage> SchoolImages { get; set; } = null!;
        public DbSet<AcademicYear> AcademicYears { get; set; } = null!;
        public DbSet<Class> Classes { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<TeacherSubject> TeacherSubjects { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Parent> Parents { get; set; } = null!;
        public DbSet<StudentParent> StudentParents { get; set; } = null!;
        public DbSet<StudentAttendance> StudentAttendances { get; set; } = null!;
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Holiday> Holidays { get; set; } = null!;
        public DbSet<AdmissionInquiry> AdmissionInquiries { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // School unique code
            builder.Entity<School>()
                .HasIndex(s => s.Code)
                .IsUnique();

            // Principal 1-to-1 with School
            builder.Entity<Principal>()
                .HasOne(p => p.School)
                .WithOne(s => s.Principal)
                .HasForeignKey<Principal>(p => p.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // Academic Year
            builder.Entity<AcademicYear>()
                .HasOne(a => a.School)
                .WithMany(s => s.AcademicYears)
                .HasForeignKey(a => a.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Class
            builder.Entity<Class>()
                .HasOne(c => c.School)
                .WithMany(s => s.Classes)
                .HasForeignKey(c => c.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Section
            builder.Entity<Section>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Section>()
                .HasOne(s => s.School)
                .WithMany()
                .HasForeignKey(s => s.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Section>()
                .HasOne(s => s.ClassTeacher)
                .WithMany(t => t.ClassTeacherSections)
                .HasForeignKey(s => s.ClassTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Teacher
            builder.Entity<Teacher>()
                .HasOne(t => t.School)
                .WithMany(s => s.Teachers)
                .HasForeignKey(t => t.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Teacher>()
                .HasIndex(t => new { t.SchoolId, t.EmployeeId })
                .IsUnique();

            // TeacherSubject
            builder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Teacher)
                .WithMany(t => t.TeacherSubjects)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Subject)
                .WithMany(s => s.TeacherSubjects)
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Student
            builder.Entity<Student>()
                .HasOne(st => st.School)
                .WithMany(s => s.Students)
                .HasForeignKey(st => st.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasOne(st => st.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(st => st.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasOne(st => st.Section)
                .WithMany(s => s.Students)
                .HasForeignKey(st => st.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasOne(st => st.AcademicYear)
                .WithMany(ay => ay.Students)
                .HasForeignKey(st => st.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Student>()
                .HasIndex(st => new { st.SchoolId, st.AdmissionNumber })
                .IsUnique();

            // StudentParent Many-to-Many
            builder.Entity<StudentParent>()
                .HasOne(sp => sp.Student)
                .WithMany(st => st.StudentParents)
                .HasForeignKey(sp => sp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentParent>()
                .HasOne(sp => sp.Parent)
                .WithMany(p => p.StudentParents)
                .HasForeignKey(sp => sp.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentAttendance UNIQUE Rule: StudentId + AttendanceDate + AcademicYearId
            builder.Entity<StudentAttendance>()
                .HasIndex(sa => new { sa.StudentId, sa.AttendanceDate, sa.AcademicYearId })
                .IsUnique();

            builder.Entity<StudentAttendance>()
                .HasOne(sa => sa.School)
                .WithMany()
                .HasForeignKey(sa => sa.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAttendance>()
                .HasOne(sa => sa.Class)
                .WithMany()
                .HasForeignKey(sa => sa.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAttendance>()
                .HasOne(sa => sa.Section)
                .WithMany(sec => sec.StudentAttendances)
                .HasForeignKey(sa => sa.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAttendance>()
                .HasOne(sa => sa.AcademicYear)
                .WithMany(ay => ay.StudentAttendances)
                .HasForeignKey(sa => sa.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAttendance>()
                .HasOne(sa => sa.Student)
                .WithMany(st => st.Attendances)
                .HasForeignKey(sa => sa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // TeacherAttendance UNIQUE Rule: TeacherId + AttendanceDate + AcademicYearId
            builder.Entity<TeacherAttendance>()
                .HasIndex(ta => new { ta.TeacherId, ta.AttendanceDate, ta.AcademicYearId })
                .IsUnique();

            builder.Entity<TeacherAttendance>()
                .HasOne(ta => ta.School)
                .WithMany()
                .HasForeignKey(ta => ta.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherAttendance>()
                .HasOne(ta => ta.AcademicYear)
                .WithMany(ay => ay.TeacherAttendances)
                .HasForeignKey(ta => ta.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TeacherAttendance>()
                .HasOne(ta => ta.Teacher)
                .WithMany(t => t.Attendances)
                .HasForeignKey(ta => ta.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notifications
            builder.Entity<Notification>()
                .HasOne(n => n.School)
                .WithMany(s => s.Notifications)
                .HasForeignKey(n => n.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Holidays
            builder.Entity<Holiday>()
                .HasOne(h => h.School)
                .WithMany(s => s.Holidays)
                .HasForeignKey(h => h.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Holiday>()
                .HasOne(h => h.AcademicYear)
                .WithMany(ay => ay.Holidays)
                .HasForeignKey(h => h.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // Performance Indexes
            builder.Entity<StudentAttendance>().HasIndex(a => a.SchoolId);
            builder.Entity<StudentAttendance>().HasIndex(a => a.ClassId);
            builder.Entity<StudentAttendance>().HasIndex(a => a.SectionId);
            builder.Entity<StudentAttendance>().HasIndex(a => a.AttendanceDate);

            builder.Entity<TeacherAttendance>().HasIndex(a => a.SchoolId);
            builder.Entity<TeacherAttendance>().HasIndex(a => a.AttendanceDate);

            builder.Entity<Student>().HasIndex(s => s.SchoolId);
            builder.Entity<Student>().HasIndex(s => s.ClassId);
            builder.Entity<Student>().HasIndex(s => s.SectionId);

            builder.Entity<Teacher>().HasIndex(t => t.SchoolId);
            builder.Entity<Class>().HasIndex(c => c.SchoolId);
            builder.Entity<Section>().HasIndex(s => s.SchoolId);
            builder.Entity<AuditLog>().HasIndex(a => a.SchoolId);
            builder.Entity<AuditLog>().HasIndex(a => a.DateTime);
        }
    }
}
