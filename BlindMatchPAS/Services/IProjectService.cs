using BlindMatchPAS.Models;

namespace BlindMatchPAS.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetStudentProjectsAsync(string studentId);
        Task<Project?> GetProjectByIdAsync(int id);
        Task<List<Project>> GetAvailableProjectsAsync(string? area);
        Task<List<Project>> GetSupervisorProjectsAsync(string supervisorId);
        Task<List<Project>> GetAllProjectsAsync();
        Task<bool> CreateProjectAsync(Project project);
        Task<bool> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        Task<bool> ExpressInterestAsync(int projectId, string supervisorId);
        Task<bool> ConfirmMatchAsync(int projectId, string supervisorId);
    }
}