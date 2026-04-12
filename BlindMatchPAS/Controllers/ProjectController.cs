using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Models;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(IProjectService projectService,
            UserManager<IdentityUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }

        // Student: View their own projects
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _projectService.GetStudentProjectsAsync(userId!);
            return View(projects);
        }

        // Student: Submit new proposal form
        [Authorize(Roles = "Student")]
        public IActionResult Create()
        {
            return View();
        }

        // Student: Submit new proposal
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            if (ModelState.IsValid)
            {
                project.StudentId = _userManager.GetUserId(User)!;
                project.Status = "Pending";
                project.IsRevealed = false;
                project.CreatedAt = DateTime.Now;
                await _projectService.CreateProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // Student: Edit proposal
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null || project.StudentId != userId)
                return NotFound();
            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending proposals.";
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // Student: Save edited proposal
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            var userId = _userManager.GetUserId(User);
            var existing = await _projectService.GetProjectByIdAsync(id);
            if (existing == null || existing.StudentId != userId)
                return NotFound();
            if (existing.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending proposals.";
                return RedirectToAction(nameof(Index));
            }
            project.Id = id;
            await _projectService.UpdateProjectAsync(project);
            return RedirectToAction(nameof(Index));
        }

        // Student: Withdraw proposal
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null || project.StudentId != userId)
                return NotFound();
            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only withdraw pending proposals.";
                return RedirectToAction(nameof(Index));
            }
            await _projectService.DeleteProjectAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Student: View project details
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null || project.StudentId != userId)
                return NotFound();
            return View(project);
        }
    }
}