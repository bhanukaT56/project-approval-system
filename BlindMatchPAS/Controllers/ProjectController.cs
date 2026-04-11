using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Student: View their own projects
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.StudentId == userId)
                .ToListAsync();
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
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // Student: Edit proposal
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
            if (project == null) return NotFound();
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
            var existing = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
            if (existing == null) return NotFound();
            if (existing.Status != "Pending")
            {
                TempData["Error"] = "You can only edit pending proposals.";
                return RedirectToAction(nameof(Index));
            }
            existing.Title = project.Title;
            existing.Abstract = project.Abstract;
            existing.TechnicalStack = project.TechnicalStack;
            existing.ResearchArea = project.ResearchArea;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Student: Withdraw proposal
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Withdraw(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
            if (project == null) return NotFound();
            if (project.Status != "Pending")
            {
                TempData["Error"] = "You can only withdraw pending proposals.";
                return RedirectToAction(nameof(Index));
            }
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Student: View matched supervisor details after reveal
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.StudentId == userId);
            if (project == null) return NotFound();
            return View(project);
        }
    }
}