using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            IProjectService projectService,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

       
        // Dashboard - view all projects and matches
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var userProfiles = new Dictionary<string, UserProfile>();

            foreach (var project in projects)
            {
                if (!userProfiles.ContainsKey(project.StudentId))
                {
                    var studentProfile = await _context.UserProfiles
                        .FirstOrDefaultAsync(p => p.UserId == project.StudentId);
                    if (studentProfile != null)
                        userProfiles[project.StudentId] = studentProfile;
                }

                if (project.SupervisorId != null &&
                    !userProfiles.ContainsKey(project.SupervisorId))
                {
                    var supervisorProfile = await _context.UserProfiles
                        .FirstOrDefaultAsync(p => p.UserId == project.SupervisorId);
                    if (supervisorProfile != null)
                        userProfiles[project.SupervisorId] = supervisorProfile;
                }
            }

            ViewBag.UserProfiles = userProfiles;
            return View(projects);
        }

        // View all users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            var userProfiles = new Dictionary<string, UserProfile>();

            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
                var profile = await _context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (profile != null)
                    userProfiles[user.Id] = profile;
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.UserProfiles = userProfiles;
            return View(users);
        }

        // Create new user form
        public IActionResult CreateUser()
        {
            return View();
        }

        // Create new user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string email,
      string password, string role, string fullName,
      string? studentNumber, string? supervisorNumber,
      string? batch, string? degreeProgram, string? department)
        {
            if (string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(role) ||
                string.IsNullOrEmpty(fullName))
            {
                TempData["Error"] = "All required fields must be filled.";
                return View();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);

                var profile = new UserProfile
                {
                    UserId = user.Id,
                    FullName = fullName,
                    StudentNumber = studentNumber,
                    SupervisorNumber = supervisorNumber,
                    Batch = batch,
                    DegreeProgram = degreeProgram,
                    Department = department
                };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"User {email} created successfully as {role}.";
                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

        // Edit user form
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == id);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "";

            ViewBag.UserId = id;
            ViewBag.Email = user.Email;
            ViewBag.Role = role;

            return View(profile ?? new UserProfile { UserId = id });
        }

        // Save edited user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string userId,
            string fullName, string? studentNumber,
            string? supervisorNumber, string? batch,
            string? degreeProgram, string? department)
        {
            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile { UserId = userId };
                _context.UserProfiles.Add(profile);
            }

            profile.FullName = fullName;
            profile.StudentNumber = studentNumber;
            profile.SupervisorNumber = supervisorNumber;
            profile.Batch = batch;
            profile.DegreeProgram = degreeProgram;
            profile.Department = department;

            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Users));
        }

        // Delete user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            await _userManager.DeleteAsync(user);
            TempData["Success"] = "User deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // Reassign project to different supervisor
        public async Task<IActionResult> Reassign(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            var supervisors = await _userManager.GetUsersInRoleAsync("Supervisor");
            ViewBag.Supervisors = supervisors;
            return View(project);
        }

        // Save reassignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reassign(int id, string supervisorId)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();

            project.SupervisorId = supervisorId;
            project.Status = "Under Review";
            project.IsRevealed = false;
            await _projectService.UpdateProjectAsync(project);

            TempData["Success"] = "Project reassigned successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}