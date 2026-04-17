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

                // Save user profile
                var profile = new UserProfile
                {
                    UserId = user.Id,
                    FullName = fullName,
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