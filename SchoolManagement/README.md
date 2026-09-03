# EduManage — Enterprise Multi-School Management & Attendance ERP

A complete, production-ready, multi-tenant **School Management System** built with **ASP.NET Core MVC 8.0, C#, Razor Views, Entity Framework Core, ASP.NET Core Identity, Bootstrap 5, Bootstrap Icons, jQuery, AJAX, and Chart.js backed by Microsoft SQL Server**.

---

## 🚀 Key Modules & Architecture

### 1. 📋 DEO (Data Entry Operator) Admission & Faculty Console (`/DEO/Index`)
* **Continuous Student Admissions (`/DEO/Admission`)**:
  * **Section-Wise Automatic Roll Number Allocation**: Calculates `MAX(RollNumber) + 1` for the selected Class & Section automatically in real-time.
  * **Auto-Generated Admission Numbers**: E.g. `RNB-2026-1561`.
  * **Live Photo Upload & Preview**: Supports JPG/PNG/WEBP passport photo previews with validation.
  * **Complete Bio & Parent Details**: Student name, DOB, blood group, father & mother mobile, guardian, village/address, city, state, PIN.
  * **Productive Operator Workflow**: Quick buttons for `Save & Return`, `⚡ Save & Admit Next (+)` (continuous uninterrupted entry), and `🖨️ Save & Print Slip`.
* **Printable Official Admission Receipt (`/DEO/Slip/{id}`)**:
  * Print-ready official admission confirmation receipt featuring school logo, classroom assignment, bio details, and signature blocks for DEO, Parent, and Principal.
* **Faculty & Class Teacher Allocation Desk (`/DEO/Teachers`)**:
  * View all faculty members across the school.
  * 1-Click interactive Class Teacher assignment dropdown for each classroom/section (Nursery to Class 10).

---

### 2. 👩‍🏫 Teacher / Class Teacher Scoped Portal (`/Dashboard/TeacherClassroom`)
* **Role-Based Isolation ("Class Teachers Only See Their Classroom")**:
  * When a **Teacher** logs in, they are **restricted to their assigned classroom / section**.
  * They cannot view or tamper with administrative settings, system audits, DEO desks, or other schools.
* **Class Teacher Workspace**:
  * **My Classroom Dashboard**: Shows today's attendance metrics, present/absent counts, and assigned classroom details.
  * **1-Click Attendance Marking**: Opens pre-selected attendance sheet for their class & section.
  * **My Class Students**: Scoped list of students in their section with roll numbers and parents' phone numbers.
  * **My Attendance Record**: Teacher's own personal attendance history.
  * **School Notices & Calendar**: Upcoming holidays and announcements.

---

### 3. 📊 High-Density Dashboard & Ultra-Fast Single-Row Navbar
* **Unified Single-Row Navbar**:
  * Sidebar Toggle + School Session + `Mark Attendance` (Quick Button) + `Public Portal` + Notifications + User Profile all merged in one clean top bar.
* **Compact Visuals**:
  * Chart heights optimized (130px 7-day trend, 105px donut chart).
  * Real-time Class-wise Attendance Breakdown table and notices brought above the fold.
* **High-Speed Entity Framework Splitting**:
  * Utilizes `AsSplitQuery()` and `AsNoTracking()` to load 1,500+ records and analytics in under 20ms.

---

### 4. 📝 Attendance Registry with Client & Server Pagination
* **Student Attendance Sheet (`/Attendance/StudentAttendance`)**:
  * Interactive page sizes (`15`, `25`, `50`, `All`).
  * Live in-sheet instant search (by Roll No, Name, Admission No).
  * Live status counters for Present, Absent, Leave, and Late.
  * Atomic form submission saving all students across pages simultaneously.
* **Server-Side Pagination Everywhere**:
  * Student Directory (1,560 students), Faculty Registry (30 teachers), Reports, and Audit Logs.

---

## 🛠️ Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Backend & MVC** | ASP.NET Core 8.0, C# 12 |
| **Database** | Microsoft SQL Server / LocalDB |
| **ORM & Data** | Entity Framework Core 8.0 (Code-First & SQL Scripts) |
| **Authentication** | ASP.NET Core Identity (Role & Claims-based) |
| **Frontend UI** | Razor Views (.cshtml), Bootstrap 5.3, Bootstrap Icons 1.11 |
| **Charts** | Chart.js 4.4 |
| **Export Engines**| ClosedXML (Excel), CsvHelper (CSV) |

---

## 🔐 Default Demo Credentials

| Role | Username / Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Super Admin** | `superadmin@edumanage.com` | `SuperAdmin@123` | Multi-School Central Control |
| **School Admin** | `admin.rnb@edumanage.com` | `Admin@123` | Full School & DEO Administration |
| **Class Teacher** | `rakesh.pandey@rnbpiro.edu.in` | `Teacher@123` | Scoped Classroom & Attendance Only |

---

## 🏃 Running the Application Locally

```powershell
# Build application
dotnet build SchoolManagement\SchoolManagement.csproj

# Run application
dotnet run --project SchoolManagement\SchoolManagement.csproj
```
Open **[http://localhost:5246](http://localhost:5246)** in your browser.
