using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagement.Models;

namespace SchoolManagement.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            // Ensure database and any newly added tables are created
            await context.Database.EnsureCreatedAsync();

            try
            {
                var ddl = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Parents')
BEGIN
    CREATE TABLE [Parents] (
        [Id] int NOT NULL IDENTITY,
        [SchoolId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Mobile] nvarchar(20) NOT NULL,
        [Email] nvarchar(150) NULL,
        [Occupation] nvarchar(100) NULL,
        [Address] nvarchar(250) NULL,
        [UserId] nvarchar(450) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_Parents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Parents_Schools_SchoolId] FOREIGN KEY ([SchoolId]) REFERENCES [Schools] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Parents_SchoolId] ON [Parents] ([SchoolId]);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentParents')
BEGIN
    CREATE TABLE [StudentParents] (
        [Id] int NOT NULL IDENTITY,
        [StudentId] int NOT NULL,
        [ParentId] int NOT NULL,
        [Relationship] nvarchar(50) NOT NULL DEFAULT 'Father',
        CONSTRAINT [PK_StudentParents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StudentParents_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_StudentParents_Parents_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Parents] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_StudentParents_StudentId] ON [StudentParents] ([StudentId]);
    CREATE INDEX [IX_StudentParents_ParentId] ON [StudentParents] ([ParentId]);
END
";
                await context.Database.ExecuteSqlRawAsync(ddl);
            }
            catch { }

            // 1. Seed Roles
            string[] roleNames = {
                "Super Admin",
                "School Admin",
                "Principal",
                "Teacher",
                "Accountant",
                "Receptionist",
                "Parent",
                "Student"
            };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new ApplicationRole(roleName)
                    {
                        Description = $"{roleName} role with specific system permissions"
                    });
                }
            }

            // 2. Seed Super Admin User
            var superAdminEmail = "superadmin@schoolmanagement.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = superAdminEmail,
                    FullName = "System Super Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(superAdminUser, "Admin@123");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "Super Admin");
                    await userManager.AddClaimAsync(superAdminUser, new Claim("FullName", superAdminUser.FullName));
                }
            }

            // 3. Seed Schools if not already seeded or update flagship school
            var rnbSchool = await context.Schools.FirstOrDefaultAsync(s => s.Code == "RNB-PIRO" || s.Name.Contains("R N B Public School"));
            if (rnbSchool == null)
            {
                var today = DateTime.Today;
                var currentYear = today.Year;

                // --- SCHOOL 1: R N B Public School, Piro, Bhojpur, Bihar ---
                rnbSchool = new School
                {
                    Name = "R N B Public School, Piro",
                    Code = "RNB-PIRO",
                    RegistrationNumber = "REG-BH-BHOJPUR-802207",
                    Logo = "/images/default-school.png",
                    Banner = "/images/default-banner.png",
                    Address = "8CM3+PH4, Station Road, Piro",
                    City = "Piro, Bhojpur",
                    State = "Bihar",
                    PinCode = "802207",
                    Phone = "+91 94708 35418",
                    Email = "info@rnbpublicschool.com",
                    Website = "https://facebook.com/RNBPublicSchool",
                    EstablishedYear = 2012,
                    Status = SchoolStatus.Active,
                    About = "R N B Public School Piro (आर एन बी पब्लिक स्कूल पिरो) is the premier institution in Bhojpur, dedicated to excellence in education, character building, and holistic child development.",
                    Vision = "To nurture future leaders with moral integrity, scientific curiosity, and academic excellence in Bhojpur.",
                    Mission = "Empowering every student through quality English-medium education, modern smart classrooms, and co-curricular enrichment."
                };

                rnbSchool.Principal = new Principal
                {
                    Name = "Dr. R. N. Bharti",
                    Qualification = "M.Sc., M.Ed., Ph.D.",
                    Experience = "20 Years in Academic Leadership",
                    Phone = "+91 96611 60546",
                    Email = "principal@rnbpublicschool.com",
                    Message = "Welcome to R N B Public School, Piro! We are committed to providing the finest education in Bhojpur with modern facilities, disciplined atmosphere, and caring faculty.",
                    Vision = "Every child at R N B Public School shall excel in academics, ethics, and sports."
                };

                context.Schools.Add(rnbSchool);
                await context.SaveChangesAsync();

                // Academic Year for RNB
                var ay1 = new AcademicYear
                {
                    SchoolId = rnbSchool.Id,
                    Name = $"{currentYear}-{currentYear + 1}",
                    StartDate = new DateTime(currentYear, 4, 1),
                    EndDate = new DateTime(currentYear + 1, 3, 31),
                    IsCurrent = true,
                    IsActive = true
                };
                context.AcademicYears.Add(ay1);
                await context.SaveChangesAsync();

                // School Admin for RNB
                var rnbAdmin = new ApplicationUser
                {
                    UserName = "admin.rnb",
                    Email = "admin@rnbpublicschool.com",
                    FullName = "RNB Admin Piro",
                    SchoolId = rnbSchool.Id,
                    EmailConfirmed = true,
                    IsActive = true
                };
                var resAdmin = await userManager.CreateAsync(rnbAdmin, "Admin@123");
                if (resAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(rnbAdmin, "School Admin");
                    await userManager.AddClaimAsync(rnbAdmin, new Claim("SchoolId", rnbSchool.Id.ToString()));
                    await userManager.AddClaimAsync(rnbAdmin, new Claim("FullName", rnbAdmin.FullName));
                }

                // Classes for RNB
                var classNames = new[] { "Nursery", "LKG", "UKG", "Class 1", "Class 2", "Class 3", "Class 4", "Class 5", "Class 6", "Class 7", "Class 8", "Class 9", "Class 10" };
                var createdClasses = new List<Class>();

                int order = 1;
                foreach (var cName in classNames)
                {
                    var cls = new Class
                    {
                        SchoolId = rnbSchool.Id,
                        Name = cName,
                        DisplayOrder = order++,
                        IsActive = true
                    };
                    context.Classes.Add(cls);
                    createdClasses.Add(cls);
                }
                await context.SaveChangesAsync();

                // Teachers for RNB
                var teachersData = new[]
                {
                    ("T-001", "Rakesh Kumar Pandey", "Male", "M.Sc. Mathematics, B.Ed.", "Mathematics", "Senior Teacher", "rakesh.p@rnbpublicschool.com", "9470835418"),
                    ("T-002", "Sunita Kumari", "Female", "M.A. English Literature", "English", "Senior Teacher", "sunita.k@rnbpublicschool.com", "9661160546"),
                    ("T-003", "Alok Ranjan Singh", "Male", "M.Sc. Physics", "Science", "Head of Department", "alok.s@rnbpublicschool.com", "9470811223"),
                    ("T-004", "Priyanka Sharma", "Female", "M.A. Hindi, B.Ed.", "Hindi", "Teacher", "priyanka.s@rnbpublicschool.com", "9470822334"),
                    ("T-005", "Amitabh Verma", "Male", "MCA, B.Ed.", "Computer Science", "Teacher", "amitabh.v@rnbpublicschool.com", "9470833445")
                };

                var createdTeachers = new List<Teacher>();
                foreach (var t in teachersData)
                {
                    var teacher = new Teacher
                    {
                        SchoolId = rnbSchool.Id,
                        EmployeeId = t.Item1,
                        Name = t.Item2,
                        Gender = t.Item3,
                        DateOfBirth = new DateTime(1986, 6, 15),
                        Qualification = t.Item4,
                        Subject = t.Item5,
                        Designation = t.Item6,
                        Email = t.Item7,
                        Mobile = t.Item8,
                        Address = "Piro, Bhojpur, Bihar 802207",
                        JoiningDate = new DateTime(2018, 4, 1),
                        Status = TeacherStatus.Active
                    };
                    context.Teachers.Add(teacher);
                    createdTeachers.Add(teacher);
                }
                await context.SaveChangesAsync();

                // Sections
                var createdSections = new List<Section>();
                int tIndex = 0;
                foreach (var cls in createdClasses.Take(6))
                {
                    var secA = new Section
                    {
                        SchoolId = rnbSchool.Id,
                        ClassId = cls.Id,
                        Name = "A",
                        RoomNumber = $"R-{cls.DisplayOrder}01",
                        Capacity = 40,
                        ClassTeacherId = createdTeachers[tIndex % createdTeachers.Count].Id,
                        IsActive = true
                    };
                    var secB = new Section
                    {
                        SchoolId = rnbSchool.Id,
                        ClassId = cls.Id,
                        Name = "B",
                        RoomNumber = $"R-{cls.DisplayOrder}02",
                        Capacity = 40,
                        ClassTeacherId = createdTeachers[(tIndex + 1) % createdTeachers.Count].Id,
                        IsActive = true
                    };
                    tIndex++;
                    context.Sections.AddRange(secA, secB);
                    createdSections.AddRange(new[] { secA, secB });
                }
                await context.SaveChangesAsync();

                // Students for RNB
                var firstNames = new[] { "Aditya", "Ananya", "Rohan", "Sneha", "Aryan", "Pooja", "Vikas", "Shreya", "Aman", "Kavya", "Rahul", "Neha" };
                var lastNames = new[] { "Singh", "Pandey", "Kumar", "Mishra", "Choudhary", "Tiwari", "Yadav", "Verma", "Gupta", "Sharma" };

                var createdStudents = new List<Student>();
                int rollGen = 1;
                int admGen = 2001;

                foreach (var sec in createdSections.Take(4))
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var fname = firstNames[(i + sec.Id) % firstNames.Length];
                        var lname = lastNames[(i * 2 + sec.Id) % lastNames.Length];
                        var student = new Student
                        {
                            SchoolId = rnbSchool.Id,
                            AcademicYearId = ay1.Id,
                            ClassId = sec.ClassId,
                            SectionId = sec.Id,
                            AdmissionNumber = $"RNB-{admGen++}",
                            RollNumber = rollGen++,
                            Name = $"{fname} {lname}",
                            DateOfBirth = new DateTime(2015, 5, 20).AddMonths(i),
                            Gender = i % 2 == 0 ? "Male" : "Female",
                            BloodGroup = "B+",
                            FatherName = $"Brijesh {lname}",
                            MotherName = $"Anita Devi",
                            FatherMobile = $"94708{admGen:D5}",
                            Address = "Station Road, Piro",
                            City = "Piro",
                            State = "Bihar",
                            PinCode = "802207",
                            AdmissionDate = new DateTime(2023, 4, 1),
                            Status = StudentStatus.Active
                        };
                        context.Students.Add(student);
                        createdStudents.Add(student);
                    }
                    rollGen = 1;
                }
                await context.SaveChangesAsync();

                // Attendance records
                for (int dayOffset = 5; dayOffset >= 0; dayOffset--)
                {
                    var attDate = today.AddDays(-dayOffset);
                    if (attDate.DayOfWeek == DayOfWeek.Sunday) continue;

                    foreach (var st in createdStudents)
                    {
                        var status = (st.RollNumber % 8 == 0) ? AttendanceStatus.Absent :
                                     (st.RollNumber % 12 == 0) ? AttendanceStatus.Late : AttendanceStatus.Present;

                        context.StudentAttendances.Add(new StudentAttendance
                        {
                            SchoolId = rnbSchool.Id,
                            AcademicYearId = ay1.Id,
                            ClassId = st.ClassId,
                            SectionId = st.SectionId,
                            StudentId = st.Id,
                            AttendanceDate = attDate,
                            Status = status,
                            RecordedBy = "AutoSeed",
                            RecordedDate = DateTime.UtcNow
                        });
                    }

                    foreach (var t in createdTeachers)
                    {
                        context.TeacherAttendances.Add(new TeacherAttendance
                        {
                            SchoolId = rnbSchool.Id,
                            AcademicYearId = ay1.Id,
                            TeacherId = t.Id,
                            AttendanceDate = attDate,
                            Status = AttendanceStatus.Present,
                            RecordedBy = "AutoSeed",
                            RecordedDate = DateTime.UtcNow
                        });
                    }
                }
                await context.SaveChangesAsync();

                // Gallery Images
                var galleryImages = new[]
                {
                    ("R N B Public School Campus Piro", "Campus", "/images/gallery-campus.png", true),
                    ("Smart Interactive Classrooms", "Classroom", "/images/gallery-lab.png", false),
                    ("School Library & Reading Zone", "Library", "/images/gallery-library.png", false),
                    ("Annual Sports & Athletic Events", "Sports", "/images/gallery-sports.png", false)
                };

                foreach (var g in galleryImages)
                {
                    context.SchoolImages.Add(new SchoolImage
                    {
                        SchoolId = rnbSchool.Id,
                        Title = g.Item1,
                        Category = g.Item2,
                        ImagePath = g.Item3,
                        IsCoverImage = g.Item4,
                        UploadDate = DateTime.UtcNow
                    });
                }

                // Notices
                context.Notifications.Add(new Notification
                {
                    SchoolId = rnbSchool.Id,
                    Title = "Admissions Open for Session 2026-27",
                    Description = "Admissions are now open for Nursery to Class 10 at R N B Public School, Piro. Visit the school office or call 94708 35418 for prospectus and entrance examination details.",
                    Type = NotificationType.General,
                    Audience = TargetAudience.All,
                    PublishDate = today
                });

                context.Notifications.Add(new Notification
                {
                    SchoolId = rnbSchool.Id,
                    Title = "Parent-Teacher Interaction Meeting",
                    Description = "Monthly parent-teacher meeting will be held on the upcoming Saturday at the Piro campus from 9:30 AM onwards.",
                    Type = NotificationType.SchoolNotice,
                    Audience = TargetAudience.ParentsOnly,
                    PublishDate = today.AddDays(-2)
                });

                // Holidays
                context.Holidays.Add(new Holiday
                {
                    SchoolId = rnbSchool.Id,
                    AcademicYearId = ay1.Id,
                    Name = "Chhath Puja Holidays",
                    HolidayDate = new DateTime(currentYear, 11, 10),
                    EndDate = new DateTime(currentYear, 11, 13),
                    Description = "Grand Chhath festival holiday break"
                });

                context.Holidays.Add(new Holiday
                {
                    SchoolId = rnbSchool.Id,
                    AcademicYearId = ay1.Id,
                    Name = "Gandhi Jayanti",
                    HolidayDate = new DateTime(currentYear, 10, 2),
                    Description = "National Holiday"
                });

                await context.SaveChangesAsync();
            }

            // Ensure full 12-month holidays are seeded
            await HolidaySeeder.SeedAllHolidaysAsync(context);
        }
    }
}
