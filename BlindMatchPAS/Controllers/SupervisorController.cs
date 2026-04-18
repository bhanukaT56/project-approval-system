using BlindMatchPAS.Data;
using BlindMatchPAS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProjectService _projectService;
        private readonly UserManager<IdentityUser> _userManager;

        public SupervisorController(
     ApplicationDbContext context,
     IProjectService projectService,
     UserManager<IdentityUser> userManager)
        {
            _context = context;
            _projectService = projectService;
            _userManager = userManager;
        }

        // Blind browse - supervisor sees projects without student identity
        public async Task<IActionResult> Index(string? area, string? search)
        {
            var projects = await _projectService.GetAvailableProjectsAsync(area);

            if (!string.IsNullOrEmpty(search))
                projects = projects.Where(p =>
                    p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Abstract.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            ViewBag.SelectedArea = area;
            ViewBag.SearchTerm = search;
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

        // Withdraw interest from a project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawInterest(int id)
        {
            var supervisorId = _userManager.GetUserId(User)!;
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null || project.SupervisorId != supervisorId)
                return NotFound();

            if (project.Status != "Under Review")
            {
                TempData["Error"] = "You can only withdraw interest from projects under review.";
                return RedirectToAction(nameof(MyInterests));
            }

            project.Status = "Pending";
            project.SupervisorId = null;
            await _projectService.UpdateProjectAsync(project);

            TempData["Success"] = "You have successfully withdrawn your interest. The project is back to Pending.";
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
            var studentProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == project.StudentId);

            ViewBag.StudentEmail = student?.Email;
            ViewBag.StudentFullName = studentProfile?.FullName ?? "Not provided";
            ViewBag.StudentBatch = studentProfile?.Batch ?? "Not provided";
            ViewBag.StudentDegreeProgram = studentProfile?.DegreeProgram ?? "Not provided";

            return View(project);
        }
    }
}