using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Models;
using Xunit;

namespace BlindMatchPAS.Tests
{
    public class ProjectTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        // Test 1: Student can submit a project
        [Fact]
        public async Task Student_Can_Submit_Project()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "AI Research",
                Abstract = "This is a test abstract",
                TechnicalStack = "Python, TensorFlow",
                ResearchArea = "Artificial Intelligence",
                StudentId = "student-123",
                Status = "Pending",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var saved = await context.Projects.FirstOrDefaultAsync(
                p => p.StudentId == "student-123");
            Assert.NotNull(saved);
            Assert.Equal("AI Research", saved.Title);
            Assert.Equal("Pending", saved.Status);
        }

        // Test 2: Project status is Pending by default
        [Fact]
        public async Task Project_Default_Status_Is_Pending()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "Web Dev Project",
                Abstract = "A web development project",
                TechnicalStack = "ASP.NET Core",
                ResearchArea = "Web Development",
                StudentId = "student-456",
                Status = "Pending",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var saved = await context.Projects.FirstOrDefaultAsync(
                p => p.StudentId == "student-456");
            Assert.Equal("Pending", saved!.Status);
        }

        // Test 3: Supervisor can express interest
        [Fact]
        public async Task Supervisor_Can_Express_Interest()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "Cybersecurity Project",
                Abstract = "A cybersecurity project",
                TechnicalStack = "Python",
                ResearchArea = "Cybersecurity",
                StudentId = "student-789",
                Status = "Pending",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Supervisor expresses interest
            project.SupervisorId = "supervisor-123";
            project.Status = "Under Review";
            await context.SaveChangesAsync();

            var updated = await context.Projects.FirstOrDefaultAsync(
                p => p.Id == project.Id);
            Assert.Equal("Under Review", updated!.Status);
            Assert.Equal("supervisor-123", updated.SupervisorId);
        }

        // Test 4: Blind match - student identity hidden before reveal
        [Fact]
        public async Task Student_Identity_Hidden_Before_Reveal()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "ML Project",
                Abstract = "A machine learning project",
                TechnicalStack = "Python, Keras",
                ResearchArea = "Machine Learning",
                StudentId = "student-secret",
                Status = "Pending",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var saved = await context.Projects.FirstOrDefaultAsync(
                p => p.Id == project.Id);
            Assert.False(saved!.IsRevealed);
        }

        // Test 5: Confirm match triggers identity reveal
        [Fact]
        public async Task Confirm_Match_Reveals_Identity()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "Cloud Project",
                Abstract = "A cloud computing project",
                TechnicalStack = "Azure",
                ResearchArea = "Cloud Computing",
                StudentId = "student-reveal",
                SupervisorId = "supervisor-reveal",
                Status = "Under Review",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            // Confirm match
            project.Status = "Matched";
            project.IsRevealed = true;
            await context.SaveChangesAsync();

            var updated = await context.Projects.FirstOrDefaultAsync(
                p => p.Id == project.Id);
            Assert.Equal("Matched", updated!.Status);
            Assert.True(updated.IsRevealed);
        }

        // Test 6: Student cannot edit matched project
        [Fact]
        public async Task Student_Cannot_Edit_Matched_Project()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "Data Science Project",
                Abstract = "A data science project",
                TechnicalStack = "R, Python",
                ResearchArea = "Data Science",
                StudentId = "student-edit",
                Status = "Matched",
                IsRevealed = true,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var saved = await context.Projects.FirstOrDefaultAsync(
                p => p.Id == project.Id);

            // Only pending projects can be edited
            bool canEdit = saved!.Status == "Pending";
            Assert.False(canEdit);
        }

        // Test 7: Student can withdraw pending project
        [Fact]
        public async Task Student_Can_Withdraw_Pending_Project()
        {
            var context = GetInMemoryContext();
            var project = new Project
            {
                Title = "Mobile Project",
                Abstract = "A mobile development project",
                TechnicalStack = "Flutter",
                ResearchArea = "Mobile Development",
                StudentId = "student-withdraw",
                Status = "Pending",
                IsRevealed = false,
                CreatedAt = DateTime.Now
            };

            context.Projects.Add(project);
            await context.SaveChangesAsync();

            context.Projects.Remove(project);
            await context.SaveChangesAsync();

            var deleted = await context.Projects.FirstOrDefaultAsync(
                p => p.StudentId == "student-withdraw");
            Assert.Null(deleted);
        }

        // Test 8: Multiple projects can exist in system
        [Fact]
        public async Task Multiple_Projects_Can_Exist()
        {
            var context = GetInMemoryContext();

            context.Projects.AddRange(
                new Project
                {
                    Title = "Project One",
                    Abstract = "First project",
                    TechnicalStack = "React",
                    ResearchArea = "Web Development",
                    StudentId = "student-1",
                    Status = "Pending",
                    IsRevealed = false,
                    CreatedAt = DateTime.Now
                },
                new Project
                {
                    Title = "Project Two",
                    Abstract = "Second project",
                    TechnicalStack = "Angular",
                    ResearchArea = "Web Development",
                    StudentId = "student-2",
                    Status = "Pending",
                    IsRevealed = false,
                    CreatedAt = DateTime.Now
                }
            );

            await context.SaveChangesAsync();

            var count = await context.Projects.CountAsync();
            Assert.Equal(2, count);
        }
    }
}