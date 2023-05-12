using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TSAIdentity.Data;
using TSAIdentity.Models;
using TSAIdentity.Services;
using TSAIdentity.ViewModels;

namespace TSAIdentity.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.User.Identity.Name;
            var organizationId = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();
            var applicationDbContext = _context.Projects.Include(p => p.Organization).Where(p => p.OrganizationId == organizationId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Organization)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }
            var tasks = await _context.Tasks
            .Include(t => t.AssignedEmployee)
            .Include(t => t.Organization)
            .Include(t => t.Project)
            .Include(t => t.Skill)
            .Where(t => t.ProjectId == id)
            .ToListAsync();
            if(tasks == null)
            {
                return NotFound();
            }
            var viewModel = new ProjectTaskViewModel
            {
                Project = project,
                Tasks = tasks
            };
            return View(viewModel);
        }

        // GET: Projects/Create
        public async Task<IActionResult> CreateAsync()
        {
            var userEmail = HttpContext.User.Identity?.Name;
            ViewData["OrganizationId"] = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,ProjectName,ProjectDescription,ProjectDeadline,OrganizationId")] Project project)
        {
            var userEmail = HttpContext.User.Identity.Name;
            var organizationId = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();
            bool projectExists = await _context.Projects.AnyAsync(p => p.ProjectName == project.ProjectName && p.OrganizationId== organizationId);

            if (projectExists)
            {
                ModelState.AddModelError("ProjectName", "A project with the same name already exists.");
                return View(project);
            }
            if (ModelState.IsValid)
            {
                project.ProjectId = Guid.NewGuid();
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ProjectId,ProjectName,ProjectDescription,ProjectDeadline,OrganizationId")] Project project)
        {
            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrganizationId"] = new SelectList(_context.Organizations, "OrganizationId", "OrganizationEmail", project.OrganizationId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Organization)
                .FirstOrDefaultAsync(m => m.ProjectId == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(Guid id)
        {
          return (_context.Projects?.Any(e => e.ProjectId == id)).GetValueOrDefault();
        }







        public async Task<IActionResult> UseAlgorithm(Guid? id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var tasks = _context.Tasks.Where(t => t.ProjectId == project.ProjectId && !t.isassigned).ToList();
            var employees = _context.Employees.Where(e => e.OrganizationId == project.OrganizationId && !e.IsBusy && e.IsActive).ToList();

            if (tasks == null || employees == null)
            {
                return NotFound();
            }

            BGraph g = new BGraph(employees, tasks, _context);

            foreach(var match in g.matchings)
            {
                var TskId=match.Key;
                var EmpId = match.Value;

                var emp = await _context.Employees.FindAsync(EmpId);
                var tsk = await _context.Tasks.FindAsync(TskId);

                if (emp == null && tsk==null)
                {
                    return NotFound();
                }
                emp.IsBusy = true;
                _context.Update(emp);

                tsk.isassigned = true;
                tsk.AssignedEmployeeId = emp.EmployeeId;
                tsk.TaskStatus= Models.TaskStatus.Assigned;

                _context.Update(tsk);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Projects", new { id = project.ProjectId });
        }

    }
}
