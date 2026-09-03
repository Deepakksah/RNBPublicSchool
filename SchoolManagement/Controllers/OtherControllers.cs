using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Data;
using SchoolManagement.Extensions;
using SchoolManagement.Models;
using SchoolManagement.Services;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public GalleryController(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;
            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            return sessionSchoolId ?? _context.Schools.FirstOrDefault()?.Id ?? 1;
        }

        public async Task<IActionResult> Index(string? category)
        {
            var schoolId = GetEffectiveSchoolId();
            var query = _context.SchoolImages.Where(i => i.SchoolId == schoolId);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(i => i.Category == category);

            var images = await query.OrderByDescending(i => i.IsCoverImage).ThenByDescending(i => i.UploadDate).ToListAsync();
            var categories = await _context.SchoolImages.Where(i => i.SchoolId == schoolId).Select(i => i.Category).Distinct().ToListAsync();

            return View(new GalleryViewModel
            {
                SchoolId = schoolId,
                SelectedCategory = category,
                Images = images,
                Categories = categories
            });
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public IActionResult Create()
        {
            var schoolId = GetEffectiveSchoolId();
            var categories = new[] { "Campus", "Building", "Classroom", "Library", "Laboratory", "Computer Lab", "Playground", "Sports", "Events", "Cultural Activities", "Other" };
            return View(new GalleryUploadViewModel
            {
                SchoolId = schoolId,
                Categories = new SelectList(categories)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryUploadViewModel model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (model.ImageFiles == null || !model.ImageFiles.Any())
            {
                ModelState.AddModelError("ImageFiles", "Please select at least one image to upload.");
            }

            if (!ModelState.IsValid)
            {
                var categories = new[] { "Campus", "Building", "Classroom", "Library", "Laboratory", "Computer Lab", "Playground", "Sports", "Events", "Cultural Activities", "Other" };
                model.Categories = new SelectList(categories);
                return View(model);
            }

            int count = 0;
            foreach (var file in model.ImageFiles)
            {
                var path = await _fileService.UploadFileAsync(file, "gallery");
                if (path != null)
                {
                    _context.SchoolImages.Add(new SchoolImage
                    {
                        SchoolId = schoolId,
                        Title = model.Title,
                        Description = model.Description,
                        Category = model.Category,
                        ImagePath = path,
                        IsCoverImage = model.SetAsCover && count == 0,
                        UploadDate = DateTime.UtcNow,
                        UploadedBy = User.Identity?.Name
                    });
                    count++;
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Upload", "SchoolImage", null, $"Uploaded {count} images to gallery.", schoolId);

            TempData["SuccessMessage"] = $"{count} image(s) uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var img = await _context.SchoolImages.FirstOrDefaultAsync(i => i.Id == id && i.SchoolId == schoolId);
            if (img != null)
            {
                _fileService.DeleteFile(img.ImagePath);
                _context.SchoolImages.Remove(img);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Image deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public NotificationController(ApplicationDbContext context, IFileService fileService, IAuditService auditService)
        {
            _context = context;
            _fileService = fileService;
            _auditService = auditService;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;
            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            return sessionSchoolId ?? _context.Schools.FirstOrDefault()?.Id ?? 1;
        }

        public async Task<IActionResult> Index()
        {
            var schoolId = GetEffectiveSchoolId();
            var notices = await _context.Notifications
                .Where(n => (n.SchoolId == schoolId || n.SchoolId == null) && n.IsActive)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();

            return View(notices);
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public IActionResult Create()
        {
            var schoolId = GetEffectiveSchoolId();
            return View(new Notification { SchoolId = schoolId });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = User.IsSuperAdmin() ? model.SchoolId : schoolId;
            model.CreatedBy = User.Identity?.Name;
            model.CreatedDate = DateTime.UtcNow;

            if (!ModelState.IsValid) return View(model);

            _context.Notifications.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Create", "Notification", model.Id.ToString(), $"Created notice: {model.Title}", model.SchoolId);

            TempData["SuccessMessage"] = "Notice published successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var notice = await _context.Notifications.FindAsync(id);
            if (notice != null)
            {
                notice.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Notice deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class HolidayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public HolidayController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;
            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            return sessionSchoolId ?? _context.Schools.FirstOrDefault()?.Id ?? 1;
        }

        public async Task<IActionResult> Index()
        {
            var schoolId = GetEffectiveSchoolId();
            var holidays = await _context.Holidays
                .Include(h => h.AcademicYear)
                .Where(h => (h.SchoolId == schoolId || h.SchoolId == null) && h.IsActive)
                .OrderBy(h => h.HolidayDate)
                .ToListAsync();

            return View(holidays);
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Create()
        {
            var schoolId = GetEffectiveSchoolId();
            var academicYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync();
            ViewBag.AcademicYears = new SelectList(academicYears, "Id", "Name");
            return View(new Holiday { SchoolId = schoolId, HolidayDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Holiday model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid)
            {
                var academicYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId && a.IsActive).ToListAsync();
                ViewBag.AcademicYears = new SelectList(academicYears, "Id", "Name", model.AcademicYearId);
                return View(model);
            }

            _context.Holidays.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Create", "Holiday", model.Id.ToString(), $"Added holiday: {model.Name}", schoolId);

            TempData["SuccessMessage"] = "Holiday added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday != null)
            {
                holiday.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Holiday deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize]
    public class AcademicYearController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public AcademicYearController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        private int GetEffectiveSchoolId()
        {
            var userSchoolId = User.GetSchoolId();
            if (userSchoolId.HasValue) return userSchoolId.Value;
            var sessionSchoolId = HttpContext.Session.GetInt32("SelectedSchoolId");
            return sessionSchoolId ?? _context.Schools.FirstOrDefault()?.Id ?? 1;
        }

        public async Task<IActionResult> Index()
        {
            var schoolId = GetEffectiveSchoolId();
            var years = await _context.AcademicYears
                .Where(a => a.SchoolId == schoolId && a.IsActive)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return View(years);
        }

        [Authorize(Roles = "Super Admin,School Admin")]
        public IActionResult Create()
        {
            var schoolId = GetEffectiveSchoolId();
            var year = DateTime.Today.Year;
            return View(new AcademicYear
            {
                SchoolId = schoolId,
                Name = $"{year}-{year + 1}",
                StartDate = new DateTime(year, 4, 1),
                EndDate = new DateTime(year + 1, 3, 31),
                IsCurrent = true
            });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicYear model)
        {
            var schoolId = GetEffectiveSchoolId();
            model.SchoolId = schoolId;

            if (!ModelState.IsValid) return View(model);

            if (model.IsCurrent)
            {
                var currentYears = await _context.AcademicYears.Where(a => a.SchoolId == schoolId).ToListAsync();
                foreach (var y in currentYears) y.IsCurrent = false;
            }

            _context.AcademicYears.Add(model);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Create", "AcademicYear", model.Id.ToString(), $"Created academic year: {model.Name}", schoolId);

            TempData["SuccessMessage"] = "Academic Year created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCurrent(int id)
        {
            var schoolId = GetEffectiveSchoolId();
            var years = await _context.AcademicYears.Where(a => a.SchoolId == schoolId).ToListAsync();
            foreach (var y in years)
            {
                y.IsCurrent = (y.Id == id);
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Active Academic Year updated.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "Super Admin,School Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IAuditService auditService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var isSuper = User.IsSuperAdmin();
            var userSchoolId = User.GetSchoolId();

            var query = _userManager.Users.Include(u => u.School).AsQueryable();
            if (!isSuper && userSchoolId.HasValue)
                query = query.Where(u => u.SchoolId == userSchoolId.Value);

            var users = await query.OrderByDescending(u => u.CreatedDate).ToListAsync();
            var userItems = new List<UserItemViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userItems.Add(new UserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    SchoolName = u.School?.Name,
                    SchoolId = u.SchoolId,
                    Roles = roles.ToList(),
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate
                });
            }

            return View(new UserListViewModel { Users = userItems });
        }

        public async Task<IActionResult> Create()
        {
            var vm = new UserCreateEditViewModel();
            await PopulateUserDropDownsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateEditViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new user.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateUserDropDownsAsync(model);
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                SchoolId = model.SchoolId,
                EmailConfirmed = true,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", user.FullName));
                if (user.SchoolId.HasValue)
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("SchoolId", user.SchoolId.Value.ToString()));
                }

                await _auditService.LogAsync("Create", "User", user.Id, $"Created user: {user.UserName} ({model.Role})", user.SchoolId);
                TempData["SuccessMessage"] = $"User '{user.UserName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }

            await PopulateUserDropDownsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"User '{user.UserName}' status updated to {(user.IsActive ? "Active" : "Disabled")}.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateUserDropDownsAsync(UserCreateEditViewModel model)
        {
            var schools = await _context.Schools.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            model.Schools = new SelectList(schools, "Id", "Name", model.SchoolId);

            var roles = await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToListAsync();
            model.Roles = new SelectList(roles, model.Role);
        }
    }

    [Authorize(Roles = "Super Admin,School Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public SettingsController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.ToListAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                var s = await _context.SystemSettings.FirstOrDefaultAsync(x => x.SettingKey == kvp.Key);
                if (s != null)
                {
                    s.SettingValue = kvp.Value;
                    s.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        SettingKey = kvp.Key,
                        SettingValue = kvp.Value,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Update", "SystemSettings", null, "Updated system configuration settings.");
            TempData["SuccessMessage"] = "Settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }

    [Authorize(Roles = "Super Admin,School Admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var isSuper = User.IsSuperAdmin();
            var userSchoolId = User.GetSchoolId();

            var query = _context.AuditLogs.AsQueryable();
            if (!isSuper && userSchoolId.HasValue)
                query = query.Where(a => a.SchoolId == userSchoolId.Value);

            int pageSize = 25;
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            int pageIndex = Math.Max(1, page);

            var logs = await query.OrderByDescending(a => a.DateTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = Math.Max(1, totalPages);
            ViewBag.TotalCount = totalCount;

            return View(logs);
        }
    }
}
