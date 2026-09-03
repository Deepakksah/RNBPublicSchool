-- ==========================================================================================
-- HEAVY DATA SEED SCRIPT FOR R N B PUBLIC SCHOOL, PIRO & COMPLETE SYSTEM
-- 25 Teachers, 60 Students per Section across all Classes, 30 Days of Realistic Attendance
-- ==========================================================================================

USE [SchoolDB];
GO

SET NOCOUNT ON;

DECLARE @SchoolId INT;
SELECT TOP 1 @SchoolId = Id FROM Schools WHERE Code = 'RNB-PIRO' OR Name LIKE '%R N B%';

IF @SchoolId IS NULL
BEGIN
    SELECT TOP 1 @SchoolId = Id FROM Schools;
END

PRINT 'Seeding data for SchoolId: ' + CAST(@SchoolId AS VARCHAR(10));

-- 1. Ensure Academic Year 2026-27
DECLARE @AcademicYearId INT;
SELECT TOP 1 @AcademicYearId = Id FROM AcademicYears WHERE SchoolId = @SchoolId AND IsCurrent = 1;

IF @AcademicYearId IS NULL
BEGIN
    INSERT INTO AcademicYears (SchoolId, Name, StartDate, EndDate, IsCurrent, IsActive, CreatedDate)
    VALUES (@SchoolId, '2026-27', '2026-04-01', '2027-03-31', 1, 1, SYSUTCDATETIME());
    SET @AcademicYearId = SCOPE_IDENTITY();
END

-- 2. Seed 25 Teachers
PRINT 'Seeding 25 Teachers...';

DECLARE @TeacherNames TABLE (
    Id INT IDENTITY(1,1),
    Name NVARCHAR(100),
    Gender NVARCHAR(20),
    Designation NVARCHAR(100),
    Subject NVARCHAR(100),
    Qualification NVARCHAR(150),
    Mobile NVARCHAR(20)
);

INSERT INTO @TeacherNames (Name, Gender, Designation, Subject, Qualification, Mobile) VALUES
(N'Rakesh Kumar Pandey', 'Male', 'Senior Teacher & HOD', 'Mathematics', 'M.Sc. Maths, B.Ed.', '9470835418'),
(N'Sunita Kumari', 'Female', 'Senior Teacher', 'English', 'M.A. English, B.Ed.', '9661160546'),
(N'Alok Ranjan Singh', 'Male', 'PGT Teacher', 'Physics', 'M.Sc. Physics', '9470811201'),
(N'Priyanka Sharma', 'Female', 'TGT Teacher', 'Hindi', 'M.A. Hindi, B.Ed.', '9470811202'),
(N'Amitabh Verma', 'Male', 'Computer Faculty', 'Computer Science', 'MCA, B.Tech', '9470811203'),
(N'Manoj Kumar Gupta', 'Male', 'PGT Teacher', 'Chemistry', 'M.Sc. Chemistry', '9470811204'),
(N'Anjali Kumari Singh', 'Female', 'TGT Teacher', 'Biology', 'M.Sc. Botany, B.Ed.', '9470811205'),
(N'Deepak Kumar Mishra', 'Male', 'TGT Teacher', 'Social Science', 'M.A. History, B.Ed.', '9470811206'),
(N'Pooja Rani', 'Female', 'PRT Teacher', 'Environmental Studies', 'B.Sc., D.El.Ed.', '9470811207'),
(N'Vikramaditya Tiwari', 'Male', 'Senior Faculty', 'Sanskrit', 'M.A. Sanskrit, Acharya', '9470811208'),
(N'Shashi Bhushan Rai', 'Male', 'Senior Teacher', 'General Science', 'M.Sc. Zoology', '9470811209'),
(N'Kavita Devi', 'Female', 'PRT Teacher', 'Mathematics (Primary)', 'B.Sc., B.Ed.', '9470811210'),
(N'Rajeshwar Prasad', 'Male', 'TGT Teacher', 'Geography', 'M.A. Geography', '9470811211'),
(N'Neha Kumari', 'Female', 'PRT Teacher', 'English (Primary)', 'B.A. English, D.El.Ed.', '9470811212'),
(N'Santosh Kumar Yadav', 'Male', 'Physical Education Director', 'Physical Education & Sports', 'M.P.Ed.', '9470811213'),
(N'Archana Pathak', 'Female', 'Art & Craft Instructor', 'Fine Arts & Craft', 'M.F.A.', '9470811214'),
(N'Sanjay Kumar Choubey', 'Male', 'TGT Teacher', 'Civics & Pol. Science', 'M.A. Pol Science', '9470811215'),
(N'Renu Bala', 'Female', 'Pre-Primary Coordinator', 'Kindergarten Activity', 'N.T.T., M.A. Psychology', '9470811216'),
(N'Abhishek Kumar Srivastava', 'Male', 'Computer Teacher', 'Information Technology', 'BCA, MCA', '9470811217'),
(N'Babita Singh', 'Female', 'PRT Teacher', 'Hindi (Primary)', 'M.A. Hindi', '9470811218'),
(N'Mukesh Kumar Ojha', 'Male', 'Music & Cultural Teacher', 'Music & Vocal', 'Sangeet Prabhakar', '9470811219'),
(N'Poonam Kumari', 'Female', 'PRT Teacher', 'General Knowledge', 'B.A., B.Ed.', '9470811220'),
(N'Dharmendra Kumar', 'Male', 'Lab Instructor', 'Science Practical', 'B.Sc. PCM', '9470811221'),
(N'Swati Priya', 'Female', 'Pre-Primary Teacher', 'Early Childhood Nursery', 'B.A., Montessori Trained', '9470811222'),
(N'Arun Kumar Chaubey', 'Male', 'TGT Teacher', 'Economics', 'M.A. Economics', '9470811223');

DECLARE @tIdx INT = 1;
DECLARE @tCount INT = (SELECT COUNT(*) FROM @TeacherNames);

WHILE @tIdx <= @tCount
BEGIN
    DECLARE @tName NVARCHAR(100), @tGender NVARCHAR(20), @tDesig NVARCHAR(100), @tSub NVARCHAR(100), @tQual NVARCHAR(150), @tMob NVARCHAR(20), @tEmpId NVARCHAR(50);
    SELECT @tName = Name, @tGender = Gender, @tDesig = Designation, @tSub = Subject, @tQual = Qualification, @tMob = Mobile
    FROM @TeacherNames WHERE Id = @tIdx;

    SET @tEmpId = 'RNB-T' + RIGHT('00' + CAST(@tIdx AS VARCHAR(5)), 3);

    IF NOT EXISTS (SELECT 1 FROM Teachers WHERE SchoolId = @SchoolId AND EmployeeId = @tEmpId)
    BEGIN
        INSERT INTO Teachers (SchoolId, EmployeeId, Name, Gender, DateOfBirth, Qualification, Experience, Subject, Designation, Mobile, Email, Address, JoiningDate, Status, IsActive, CreatedDate)
        VALUES (@SchoolId, @tEmpId, @tName, @tGender, DATEADD(YEAR, -35 - (@tIdx % 10), '2026-01-01'), @tQual, CAST(5 + (@tIdx % 15) AS VARCHAR(5)) + ' Years', @tSub, @tDesig, @tMob, LOWER(REPLACE(@tEmpId, '-', '')) + '@rnbpublicschool.com', N'Station Road, Piro, Bhojpur, Bihar', '2019-04-01', 1, 1, SYSUTCDATETIME());
    END
    SET @tIdx = @tIdx + 1;
END;

-- 3. Ensure All Classes (Nursery to Class 10)
PRINT 'Ensuring Classes & Sections...';

DECLARE @ClassDefs TABLE (
    OrderNum INT,
    ClassName NVARCHAR(50)
);

INSERT INTO @ClassDefs VALUES 
(1, 'Nursery'), (2, 'LKG'), (3, 'UKG'),
(4, 'Class 1'), (5, 'Class 2'), (6, 'Class 3'),
(7, 'Class 4'), (8, 'Class 5'), (9, 'Class 6'),
(10, 'Class 7'), (11, 'Class 8'), (12, 'Class 9'), (13, 'Class 10');

DECLARE @cIdx INT = 1;
WHILE @cIdx <= 13
BEGIN
    DECLARE @cName NVARCHAR(50);
    SELECT @cName = ClassName FROM @ClassDefs WHERE OrderNum = @cIdx;

    DECLARE @CurrentClassId INT;
    SELECT @CurrentClassId = Id FROM Classes WHERE SchoolId = @SchoolId AND Name = @cName;

    IF @CurrentClassId IS NULL
    BEGIN
        INSERT INTO Classes (SchoolId, Name, DisplayOrder, IsActive, CreatedDate)
        VALUES (@SchoolId, @cName, @cIdx, 1, SYSUTCDATETIME());
        SET @CurrentClassId = SCOPE_IDENTITY();
    END

    -- Ensure Section A and Section B for each class
    DECLARE @SecNames TABLE (SecName NVARCHAR(10));
    DELETE FROM @SecNames;
    INSERT INTO @SecNames VALUES ('A'), ('B');

    DECLARE @SecName NVARCHAR(10);
    DECLARE sec_cursor CURSOR FOR SELECT SecName FROM @SecNames;
    OPEN sec_cursor;
    FETCH NEXT FROM sec_cursor INTO @SecName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Sections WHERE SchoolId = @SchoolId AND ClassId = @CurrentClassId AND Name = @SecName)
        BEGIN
            -- Pick a teacher
            DECLARE @AssignedTeacherId INT;
            SELECT TOP 1 @AssignedTeacherId = Id FROM Teachers WHERE SchoolId = @SchoolId ORDER BY NEWID();

            INSERT INTO Sections (SchoolId, ClassId, Name, RoomNumber, Capacity, ClassTeacherId, IsActive, CreatedDate)
            VALUES (@SchoolId, @CurrentClassId, @SecName, 'Room ' + CAST((@cIdx * 10 + CASE WHEN @SecName = 'A' THEN 1 ELSE 2 END) AS VARCHAR(10)), 65, @AssignedTeacherId, 1, SYSUTCDATETIME());
        END
        FETCH NEXT FROM sec_cursor INTO @SecName;
    END
    CLOSE sec_cursor;
    DEALLOCATE sec_cursor;

    SET @cIdx = @cIdx + 1;
END;

-- 4. Seed 60 Students per Section
PRINT 'Populating 60 Students per Section across all classes...';

DECLARE @FirstNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50), Gender NVARCHAR(10));
INSERT INTO @FirstNames (Name, Gender) VALUES
(N'Aarav', 'Male'), (N'Vivaan', 'Male'), (N'Aditya', 'Male'), (N'Vihaan', 'Male'), (N'Arjun', 'Male'),
(N'Sai', 'Male'), (N'Reyansh', 'Male'), (N'Ayaan', 'Male'), (N'Krishna', 'Male'), (N'Ishaan', 'Male'),
(N'Shaurya', 'Male'), (N'Atharv', 'Male'), (N'Advik', 'Male'), (N'Pranav', 'Male'), (N'Advaith', 'Male'),
(N'Aayush', 'Male'), (N'Dhruv', 'Male'), (N'Kabir', 'Male'), (N'Ritik', 'Male'), (N'Shivam', 'Male'),
(N'Ananya', 'Female'), (N'Diya', 'Female'), (N'Gauri', 'Female'), (N'Isha', 'Female'), (N'Kavya', 'Female'),
(N'Khushi', 'Female'), (N'Pari', 'Female'), (N'Riya', 'Female'), (N'Saanvi', 'Female'), (N'Sneha', 'Female'),
(N'Tanvi', 'Female'), (N'Pooja', 'Female'), (N'Shreya', 'Female'), (N'Kritika', 'Female'), (N'Aaradhya', 'Female'),
(N'Deepika', 'Female'), (N'Simran', 'Female'), (N'Neha', 'Female'), (N'Muskan', 'Female'), (N'Anjali', 'Female'),
(N'Rohan', 'Male'), (N'Alok', 'Male'), (N'Manish', 'Male'), (N'Rakesh', 'Male'), (N'Saurabh', 'Male'),
(N'Vikash', 'Male'), (N'Pankaj', 'Male'), (N'Sandeep', 'Male'), (N'Rajeev', 'Male'), (N'Chandan', 'Male'),
(N'Suman', 'Female'), (N'Rani', 'Female'), (N'Shweta', 'Female'), (N'Sunidhi', 'Female'), (N'Archana', 'Female'),
(N'Payal', 'Female'), (N'Komal', 'Female'), (N'Divya', 'Female'), (N'Megha', 'Female'), (N'Rashmi', 'Female');

DECLARE @LastNames TABLE (Id INT IDENTITY(1,1), Name NVARCHAR(50));
INSERT INTO @LastNames (Name) VALUES
(N'Singh'), (N'Pandey'), (N'Kumar'), (N'Mishra'), (N'Tiwari'), (N'Choudhary'), (N'Yadav'), (N'Verma'), (N'Gupta'),
(N'Sharma'), (N'Dubey'), (N'Tripathi'), (N'Jha'), (N'Thakur'), (N'Rai'), (N'Upadhyay'), (N'Srivastava'), (N'Pathak'), (N'Ojha'), (N'Choubey');

DECLARE @SectionsToSeed TABLE (SectionId INT, ClassId INT, ClassName NVARCHAR(50), SecName NVARCHAR(10));
INSERT INTO @SectionsToSeed
SELECT s.Id, c.Id, c.Name, s.Name
FROM Sections s
JOIN Classes c ON s.ClassId = c.Id
WHERE s.SchoolId = @SchoolId;

DECLARE @CurrentSecId INT, @CurClassId INT, @CurClassName NVARCHAR(50), @CurSecName NVARCHAR(10);
DECLARE sec_loop CURSOR FOR SELECT SectionId, ClassId, ClassName, SecName FROM @SectionsToSeed;

OPEN sec_loop;
FETCH NEXT FROM sec_loop INTO @CurrentSecId, @CurClassId, @CurClassName, @CurSecName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @ExistingStudentCount INT;
    SELECT @ExistingStudentCount = COUNT(*) FROM Students WHERE SchoolId = @SchoolId AND SectionId = @CurrentSecId;

    DECLARE @TargetRoll INT = 1;
    WHILE @TargetRoll <= 60
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM Students WHERE SchoolId = @SchoolId AND SectionId = @CurrentSecId AND RollNumber = @TargetRoll)
        BEGIN
            DECLARE @fnId INT = ((@CurrentSecId * 7 + @TargetRoll) % 60) + 1;
            DECLARE @lnId INT = ((@CurrentSecId * 3 + @TargetRoll) % 20) + 1;

            DECLARE @sFName NVARCHAR(50), @sGen NVARCHAR(10), @sLName NVARCHAR(50);
            SELECT @sFName = Name, @sGen = Gender FROM @FirstNames WHERE Id = @fnId;
            SELECT @sLName = Name FROM @LastNames WHERE Id = @lnId;

            DECLARE @sFullName NVARCHAR(100) = @sFName + ' ' + @sLName;
            DECLARE @sAdmNo NVARCHAR(50) = 'RNB-' + CAST(@CurClassId AS VARCHAR(5)) + CAST(@CurrentSecId AS VARCHAR(5)) + RIGHT('00' + CAST(@TargetRoll AS VARCHAR(5)), 3);
            DECLARE @sFather NVARCHAR(100) = 'Shri ' + (SELECT TOP 1 Name FROM @FirstNames WHERE Gender = 'Male' ORDER BY NEWID()) + ' ' + @sLName;
            DECLARE @sMother NVARCHAR(100) = 'Smt. ' + (SELECT TOP 1 Name FROM @FirstNames WHERE Gender = 'Female' ORDER BY NEWID()) + ' Devi';
            DECLARE @sMobile NVARCHAR(20) = '94708' + RIGHT('00000' + CAST((@CurrentSecId * 100 + @TargetRoll) AS VARCHAR(10)), 5);

            INSERT INTO Students (
                SchoolId, AcademicYearId, ClassId, SectionId, AdmissionNumber, RollNumber,
                Name, DateOfBirth, Gender, BloodGroup, FatherName, MotherName, FatherMobile,
                Address, City, State, PinCode, AdmissionDate, Status, IsActive, CreatedDate
            ) VALUES (
                @SchoolId, @AcademicYearId, @CurClassId, @CurrentSecId, @sAdmNo, @TargetRoll,
                @sFullName, DATEADD(YEAR, -5 - (@CurClassId % 10), '2026-05-15'), @sGen, 'B+',
                @sFather, @sMother, @sMobile, N'Station Road, Piro', N'Piro, Bhojpur', N'Bihar', '802207',
                '2024-04-01', 1, 1, SYSUTCDATETIME()
            );
        END
        SET @TargetRoll = @TargetRoll + 1;
    END

    FETCH NEXT FROM sec_loop INTO @CurrentSecId, @CurClassId, @CurClassName, @CurSecName;
END;
CLOSE sec_loop;
DEALLOCATE sec_loop;

-- 5. Seed Attendance for Past 30 Days
PRINT 'Seeding 30 Days of realistic Student & Faculty Attendance...';

DECLARE @DatesTable TABLE (AttDate DATE);
DECLARE @dOffset INT = 0;
WHILE @dOffset < 30
BEGIN
    DECLARE @chkDate DATE = DATEADD(DAY, -@dOffset, CAST(GETDATE() AS DATE));
    -- Skip Sundays
    IF DATEPART(WEEKDAY, @chkDate) <> 1
    BEGIN
        INSERT INTO @DatesTable VALUES (@chkDate);
    END
    SET @dOffset = @dOffset + 1;
END;

-- Student Attendance Insert
INSERT INTO StudentAttendances (SchoolId, AcademicYearId, ClassId, SectionId, StudentId, AttendanceDate, Status, Remarks, RecordedDate, RecordedBy)
SELECT 
    st.SchoolId,
    @AcademicYearId,
    st.ClassId,
    st.SectionId,
    st.Id,
    d.AttDate,
    CASE 
        WHEN (st.RollNumber + DAY(d.AttDate)) % 17 = 0 THEN 2 -- Absent
        WHEN (st.RollNumber + DAY(d.AttDate)) % 29 = 0 THEN 3 -- Leave
        WHEN (st.RollNumber + DAY(d.AttDate)) % 37 = 0 THEN 4 -- Late
        ELSE 1 -- Present
    END AS Status,
    CASE 
        WHEN (st.RollNumber + DAY(d.AttDate)) % 17 = 0 THEN 'Uninformed'
        WHEN (st.RollNumber + DAY(d.AttDate)) % 29 = 0 THEN 'Medical Leave'
        ELSE NULL
    END AS Remarks,
    SYSUTCDATETIME(),
    'AutoSeed'
FROM Students st
CROSS JOIN @DatesTable d
WHERE st.SchoolId = @SchoolId
  AND NOT EXISTS (
      SELECT 1 FROM StudentAttendances sa
      WHERE sa.StudentId = st.Id 
        AND sa.AttendanceDate = d.AttDate 
        AND sa.AcademicYearId = @AcademicYearId
  );

-- Teacher Attendance Insert
INSERT INTO TeacherAttendances (SchoolId, AcademicYearId, TeacherId, AttendanceDate, Status, Remarks, RecordedDate, RecordedBy)
SELECT 
    t.SchoolId,
    @AcademicYearId,
    t.Id,
    d.AttDate,
    CASE 
        WHEN (t.Id + DAY(d.AttDate)) % 19 = 0 THEN 3 -- Leave
        WHEN (t.Id + DAY(d.AttDate)) % 31 = 0 THEN 2 -- Absent
        ELSE 1 -- Present
    END AS Status,
    NULL,
    SYSUTCDATETIME(),
    'AutoSeed'
FROM Teachers t
CROSS JOIN @DatesTable d
WHERE t.SchoolId = @SchoolId
  AND NOT EXISTS (
      SELECT 1 FROM TeacherAttendances ta
      WHERE ta.TeacherId = t.Id 
        AND ta.AttendanceDate = d.AttDate 
        AND ta.AcademicYearId = @AcademicYearId
  );

PRINT 'Heavy Data Seeding Completed Successfully!';
SELECT COUNT(*) AS TotalTeachers FROM Teachers WHERE SchoolId = @SchoolId;
SELECT COUNT(*) AS TotalStudents FROM Students WHERE SchoolId = @SchoolId;
SELECT COUNT(*) AS TotalStudentAttendanceRecords FROM StudentAttendances WHERE SchoolId = @SchoolId;
SELECT COUNT(*) AS TotalTeacherAttendanceRecords FROM TeacherAttendances WHERE SchoolId = @SchoolId;
GO
