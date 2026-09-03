using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagement.Extensions;
using SchoolManagement.Models;
using SchoolManagement.Services;
using SchoolManagement.ViewModels;

namespace SchoolManagement.Controllers
{
    public class SchoolController : Controller
    {
        private readonly ISchoolService _schoolService;
        private readonly SchoolManagement.Data.ApplicationDbContext _context;

        public SchoolController(ISchoolService schoolService, SchoolManagement.Data.ApplicationDbContext context)
        {
            _schoolService = schoolService;
            _context = context;
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Index()
        {
            var schools = await _schoolService.GetAllSchoolsAsync();
            return View(schools);
        }

        [Authorize(Roles = "Super Admin")]
        public IActionResult Create()
        {
            return View(new SchoolCreateEditViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SchoolCreateEditViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var school = await _schoolService.CreateSchoolAsync(model, User.Identity?.Name ?? "Admin");
                TempData["SuccessMessage"] = $"School '{school.Name}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        public async Task<IActionResult> Edit(int? id)
        {
            int targetId;
            if (User.IsSuperAdmin())
            {
                if (!id.HasValue) return BadRequest();
                targetId = id.Value;
            }
            else
            {
                var userSchoolId = User.GetSchoolId();
                if (!userSchoolId.HasValue) return Forbid();
                targetId = userSchoolId.Value;
            }

            var school = await _schoolService.GetSchoolByIdAsync(targetId);
            if (school == null) return NotFound();

            var vm = new SchoolCreateEditViewModel
            {
                Id = school.Id,
                Name = school.Name,
                Code = school.Code,
                RegistrationNumber = school.RegistrationNumber,
                ExistingLogo = school.Logo,
                ExistingBanner = school.Banner,
                Address = school.Address,
                City = school.City,
                State = school.State,
                PinCode = school.PinCode,
                Phone = school.Phone,
                Email = school.Email,
                Website = school.Website,
                EstablishedYear = school.EstablishedYear,
                Status = school.Status,
                About = school.About,
                Vision = school.Vision,
                Mission = school.Mission,
                PrincipalName = school.Principal?.Name,
                ExistingPrincipalPhoto = school.Principal?.Photo,
                PrincipalQualification = school.Principal?.Qualification,
                PrincipalExperience = school.Principal?.Experience,
                PrincipalPhone = school.Principal?.Phone,
                PrincipalEmail = school.Principal?.Email,
                PrincipalMessage = school.Principal?.Message
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,School Admin,Principal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SchoolCreateEditViewModel model)
        {
            if (!User.IsSuperAdmin())
            {
                var userSchoolId = User.GetSchoolId();
                if (userSchoolId != model.Id) return Forbid();
            }

            if (!ModelState.IsValid) return View(model);

            var updated = await _schoolService.UpdateSchoolAsync(model, User.Identity?.Name ?? "Admin");
            if (updated == null) return NotFound();

            TempData["SuccessMessage"] = "School profile updated successfully!";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            int targetId;
            if (User.IsSuperAdmin() && id.HasValue)
            {
                targetId = id.Value;
            }
            else
            {
                var userSchoolId = User.GetSchoolId();
                targetId = userSchoolId ?? id ?? 1;
            }

            var school = await _schoolService.GetSchoolByIdAsync(targetId);
            if (school == null) return NotFound();

            return View(school);
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> Availability()
        {
            var overview = await _schoolService.GetSchoolAvailabilityOverviewAsync();
            return View(overview);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("School/Profile/{schoolCode?}")]
        [Route("School/PublicProfile/{schoolCode?}")]
        [Route("Public/{schoolCode?}")]
        public async Task<IActionResult> PublicProfile(string? schoolCode = "RNB-PIRO")
        {
            var code = string.IsNullOrWhiteSpace(schoolCode) ? "RNB-PIRO" : schoolCode;
            var profile = await _schoolService.GetPublicProfileAsync(code);
            if (profile == null)
            {
                var all = await _schoolService.GetActiveSchoolsAsync();
                if (all.Count > 0) profile = await _schoolService.GetPublicProfileAsync(all[0].Code);
            }

            if (profile == null) return NotFound();

            return View(profile);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("School/Register/{schoolCode?}")]
        [Route("School/Enquiry/{schoolCode?}")]
        public async Task<IActionResult> Register(string? schoolCode = "RNB-PIRO")
        {
            var profile = await _schoolService.GetPublicProfileAsync(schoolCode ?? "RNB-PIRO");
            if (profile == null)
            {
                var all = await _schoolService.GetActiveSchoolsAsync();
                if (all.Count > 0) profile = await _schoolService.GetPublicProfileAsync(all[0].Code);
            }

            ViewBag.School = profile?.School;
            var model = new AdmissionInquiry
            {
                SchoolId = profile?.School.Id ?? 4,
                City = "Piro",
                State = "Bihar",
                PinCode = "802207",
                DateOfBirth = DateTime.Today.AddYears(-6)
            };

            return View("Register", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInquiry(AdmissionInquiry model)
        {
            if (!ModelState.IsValid)
            {
                var school = await _schoolService.GetSchoolByIdAsync(model.SchoolId);
                ViewBag.School = school;
                return View("Register", model);
            }

            model.CreatedDate = DateTime.UtcNow;
            model.Status = "New";
            model.IsActive = true;

            _context.AdmissionInquiries.Add(model);

            // Automatically post High-Priority Notification on Dashboard!
            var notif = new Notification
            {
                SchoolId = model.SchoolId,
                Title = $"🔔 New Online Admission Inquiry: {model.StudentName} ({model.ApplyingForClass})",
                Description = $"Parent: {model.FatherName} • Mobile: {model.FatherMobile} • Applying for: {model.ApplyingForClass} • Address: {model.Address}, {model.City}. Registered via Public Portal.",
                Type = NotificationType.General,
                Audience = TargetAudience.All,
                PublishDate = DateTime.Today,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notif);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thank you {model.FatherName}! Your admission inquiry for {model.StudentName} (Class {model.ApplyingForClass}) has been submitted successfully. Our admissions desk will contact you at {model.FatherMobile} shortly.";

            return RedirectToAction("Register", new { schoolCode = "RNB-PIRO" });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _schoolService.DeleteSchoolAsync(id);
            if (success)
            {
                TempData["SuccessMessage"] = "School deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Unable to deactivate school.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
