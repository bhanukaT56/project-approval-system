using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;
using BlindMatchPAS.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed Roles and Default Admin
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider
        .GetRequiredService<UserManager<IdentityUser>>();

    // Seed Roles
    string[] roles = { "Student", "Supervisor", "ModuleLeader", "Admin" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed Default Admin User
    string adminEmail = "admin@blindmatch.com";
    string adminPassword = "Admin@1234";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(newAdmin, "Admin");
    }

    // Seed Default Supervisor
    string supervisorEmail = "supervisor@blindmatch.com";
    string supervisorPassword = "Supervisor@1234";

    var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);
    if (supervisorUser == null)
    {
        var newSupervisor = new IdentityUser
        {
            UserName = supervisorEmail,
            Email = supervisorEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(
            newSupervisor, supervisorPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(newSupervisor, "Supervisor");
    }

    // Seed Default Student
    string studentEmail = "student@blindmatch.com";
    string studentPassword = "Student@1234";

    var studentUser = await userManager.FindByEmailAsync(studentEmail);
    if (studentUser == null)
    {
        var newStudent = new IdentityUser
        {
            UserName = studentEmail,
            Email = studentEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(newStudent, studentPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(newStudent, "Student");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();