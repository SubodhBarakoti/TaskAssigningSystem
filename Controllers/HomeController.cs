using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager ,ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to the Dashboard action of your desired controller
                return RedirectToAction("Dashboard", "Home");
            }
            return View();
        }
        public async Task<IActionResult> DashBoardAsync()
        {
            string userEmail = HttpContext.User.Identity.Name;

            if (userEmail == null || _context.Organizations == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Admin"))
            {
                var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.OrganizationEmail == userEmail);
                if (organization == null)
                {
                    return NotFound();
                }
                var OrganizationId = organization.OrganizationId;

                ViewData["Designation_Count"] = _context.Designations.Count(d => d.OrganizationId == OrganizationId);
                ViewData["Skill_Count"] = _context.Skills.Count(s => s.OrganizationId == OrganizationId);
                ViewData["Active_Employees"] = _context.Employees.Count(e => e.OrganizationId == OrganizationId && e.IsActive);
                ViewData["Busy_Employees"] = _context.Employees.Count(e => e.OrganizationId == OrganizationId && e.IsActive && e.IsBusy);
                ViewData["Available_Employees"] = _context.Employees.Count(e => e.OrganizationId == OrganizationId && e.IsActive && !e.IsBusy);
                ViewData["Total_Project"] = _context.Projects.Count(p => p.OrganizationId == OrganizationId);
                ViewData["Pending_Project"] = _context.Projects.Count(p => p.OrganizationId == OrganizationId && p.ProjectStatus == Models.ProjectStatus.Pending);
                ViewData["Ongoing_Project"] = _context.Projects.Count(p => p.OrganizationId == OrganizationId && p.ProjectStatus == Models.ProjectStatus.InProgress);
                ViewData["Completed_Project"] = _context.Projects.Count(p => p.OrganizationId == OrganizationId && p.ProjectStatus == Models.ProjectStatus.Completed);
            }   
            else if (User.IsInRole("Employee"))
            {
                var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeEmail == userEmail);
                if (employee == null)
                {
                    return NotFound();
                }
                ViewData["TotalAssigned"]= _context.Tasks.Count(t=>t.AssignedEmployeeId== employee.EmployeeId);
                ViewData["Completed"] = _context.Tasks.Count(t => t.AssignedEmployeeId == employee.EmployeeId && t.TaskStatus==Models.TaskStatus.Completed);
                ViewData["InProgress"] = _context.Tasks.Count(t => t.AssignedEmployeeId == employee.EmployeeId && (t.TaskStatus == Models.TaskStatus.Assigned|| t.TaskStatus==Models.TaskStatus.InProgress ) );
                ViewData["Assigned"] = await _context.Tasks.AnyAsync(t => t.AssignedEmployeeId == employee.EmployeeId && t.TaskStatus == Models.TaskStatus.Assigned);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
