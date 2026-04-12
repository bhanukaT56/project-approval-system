using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Microsoft.EntityFrameworkCore;

namespace BlindMatchPAS.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all projects for a specific student
        public async Task<List<Project>> GetStudentProjectsAsync(string studentId)
        {
            return await _context.Projects
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get a single project by ID
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        // Get all available projects for supervisor blind browse
        public async Task<List<Project>> GetAvailableProjectsAsync(string? area)
        {
            var query = _context.Projects
                .Where(p => p.Status == "Pending" ||
                            p.Status == "Under Review");

            if (!string.IsNullOrEmpty(area))
                query = query.Where(p => p.ResearchArea == area);

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get all projects for a specific supervisor
        public async Task<List<Project>> GetSupervisorProjectsAsync(string supervisorId)
        {
            return await _context.Projects
                .Where(p => p.SupervisorId == supervisorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get all projects for admin dashboard
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Create a new project
        public async Task<bool> CreateProjectAsync(Project project)
        {
            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Update an existing project
        public async Task<bool> UpdateProjectAsync(Project project)
        {
            try
            {
                var existing = await _context.Projects.FindAsync(project.Id);
                if (existing == null) return false;

                existing.Title = project.Title;
                existing.Abstract = project.Abstract;
                existing.TechnicalStack = project.TechnicalStack;
                existing.ResearchArea = project.ResearchArea;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Delete a project
        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                var project = await _context.Projects.FindAsync(id);
                if (project == null) return false;

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Supervisor expresses interest in a project
        public async Task<bool> ExpressInterestAsync(int projectId, string supervisorId)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null || project.Status != "Pending")
                    return false;

                project.SupervisorId = supervisorId;
                project.Status = "Under Review";
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Supervisor confirms match and triggers identity reveal
        public async Task<bool> ConfirmMatchAsync(int projectId, string supervisorId)
        {
            try
            {
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == projectId &&
                                             p.SupervisorId == supervisorId);
                if (project == null) return false;

                project.Status = "Matched";
                project.IsRevealed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}