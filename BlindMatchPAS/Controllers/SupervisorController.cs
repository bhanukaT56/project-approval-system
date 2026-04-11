using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupervisorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Blind browse - supervisor sees projects without student identity
        public async Task<IActionResult> Index(string? area)
        {
            var projects = _context.Projects
                .Where(p => p.Status == "Pending" || p.Status == "Under Review");

            if (!string.IsNullOrEmpty(area))
                projects = projects.Where(p => p.ResearchArea == area);

            ViewBag.SelectedArea = area;
            return View(await projects.ToListAsync());
        }

        // View single project anonymously
        public async Task<IActionResult> ViewProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }

        // Express interest in a project
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            if (project.Status != "Pending")
            {
                TempData["Error"] = "This project is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            var supervisorId = _userManager.GetUserId(User);
            project.Status = "Under Review";
            project.SupervisorId = supervisorId;
            await _context.SaveChangesAsync();

            TempData["Success"] = "You have expressed interest. You can now confirm the match.";
            return RedirectToAction(nameof(MyInterests));
        }

        // View projects supervisor expressed interest in
        public async Task<IActionResult> MyInterests()
        {
            var supervisorId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.SupervisorId == supervisorId)
                .ToListAsync();
            return View(projects);
        }

        // Confirm match and trigger identity reveal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmMatch(int id)
        {
            var supervisorId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.SupervisorId == supervisorId);

            if (project == null) return NotFound();

            project.Status = "Matched";
            project.IsRevealed = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Match confirmed! Student identity has been revealed.";
            return RedirectToAction(nameof(MyInterests));
        }

        // View revealed student details after match
        public async Task<IActionResult> RevealedDetails(int id)
        {
            var supervisorId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.SupervisorId == supervisorId);

            if (project == null) return NotFound();
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