using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlindMatchPAS.Services;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;

        public SupervisorController(IProjectService projectService,
            UserManager<IdentityUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }

        // Blind browse - supervisor sees projects without student identity
        public async Task<IActionResult> Index(string? area)
        {
            var projects = await _projectService.GetAvailableProjectsAsync(area);
            ViewBag.SelectedArea = area;
            return View(projects);
        }

        // View single project anonymously
        public async Task<IActionResult> ViewProject(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }

        // Express interest in a project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int id)
        {
            var supervisorId = _userManager.GetUserId(User)!;
            var result = await _projectService.ExpressInterestAsync(id, supervisorId);

            if (!result)
            {
                TempData["Error"] = "This project is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "You have expressed interest. You can now confirm the match.";
            return RedirectToAction(nameof(MyInterests));
        }

        // View projects supervisor expressed interest in
        public async Task<IActionResult> MyInterests()
        {
            var supervisorId = _userManager.GetUserId(User)!;
            var projects = await _projectService.GetSupervisorProjectsAsync(supervisorId);
            return View(projects);
        }

        // Confirm match and trigger identity reveal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int id)
        {
            var supervisorId = _userManager.GetUserId(User)!;
            var result = await _projectService.ConfirmMatchAsync(id, supervisorId);

            if (!result)
            {
                TempData["Error"] = "Unable to confirm match. Please try again.";
                return RedirectToAction(nameof(MyInterests));
            }

            TempData["Success"] = "Match confirmed! Student identity has been revealed.";
            return RedirectToAction(nameof(MyInterests));
        }

        // View revealed student details after match
        public async Task<IActionResult> RevealedDetails(int id)
        {
            var supervisorId = _userManager.GetUserId(User)!;
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null || project.SupervisorId != supervisorId)
                return NotFound();

            if (!project.IsRevealed)
            {
                TempData["Error"] = "Identity not yet revealed for this project.";
                return RedirectToAction(nameof(MyInterests));
            }

            var student = await _userManager.FindByIdAsync(project.StudentId);
            ViewBag.StudentEmail = student?.Email;
            ViewBag.StudentId = project.StudentId;

            return View(project);
        }
    }
}