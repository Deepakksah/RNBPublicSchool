# IIS Production Deployment Guide — SchoolManagement ASP.NET Core 8.0

Follow these steps to deploy the School Management System to a Windows Server with Internet Information Services (IIS).

---

## 1. Prerequisites on Windows Server

1. **Install IIS**:
   * Open Server Manager -> Add Roles and Features -> Select **Web Server (IIS)**.
   * Under Application Development, check **WebSocket Protocol**.
2. **Install .NET 8.0 Hosting Bundle**:
   * Download and install the **.NET 8.0 Windows Hosting Bundle** from Microsoft.
   * Run `iisreset` from an administrative command prompt.
3. **Microsoft SQL Server**:
   * Ensure SQL Server is accessible from IIS server with Mixed Mode Authentication enabled.

---

## 2. Publish the ASP.NET Core Project

From the project directory, run:
```bash
dotnet publish SchoolManagement/SchoolManagement.csproj -c Release -o C:\inetpub\wwwroot\SchoolManagement
```

---

## 3. Configure IIS Application Pool and Website

1. Open **IIS Manager** (`inetmgr`).
2. Create an **Application Pool**:
   * Name: `SchoolManagementAppPool`
   * .NET CLR Version: **No Managed Code**
   * Managed Pipeline Mode: **Integrated**
3. Under Application Pool **Advanced Settings**:
   * Set **Identity** to `ApplicationPoolIdentity` (or dedicated service account).
   * Ensure **Load User Profile** = `True`.
4. Create a **Website** in IIS:
   * Site name: `EduManageSchoolERP`
   * Physical path: `C:\inetpub\wwwroot\SchoolManagement`
   * Application Pool: `SchoolManagementAppPool`
   * Binding: Port `80` / `443` (HTTPS with SSL Certificate).

---

## 4. Set File System Permissions

Grant **Modify** permissions on the `uploads` directory to IIS App Pool:
```powershell
$acl = Get-Acl "C:\inetpub\wwwroot\SchoolManagement\wwwroot\uploads"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\SchoolManagementAppPool", "Modify", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.AddAccessRule($rule)
Set-Acl "C:\inetpub\wwwroot\SchoolManagement\wwwroot\uploads" $acl
```

---

## 5. Web.config

When published, ASP.NET Core automatically generates `web.config` with the `aspNetCore` module handler:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\SchoolManagement.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
    </system.webServer>
  </location>
</configuration>
```

---

## 6. Verify Deployment

Browse to `http://localhost` or configured domain in browser, verify login with `superadmin` / `Admin@123`.
