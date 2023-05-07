using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using TSAIdentity.Data;
using TSAIdentity.Models;

namespace TSAIdentity.Controllers
{
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tasks
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tasks.Include(t => t.AssignedEmployee).Include(t => t.Organization).Include(t => t.Project).Include(t => t.Skill);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Organization)
                .Include(t => t.Project)
                .Include(t => t.Skill)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // GET: Tasks/Create
        public async Task<IActionResult> CreateAsync(Guid ?id)
        {
            var project = await _context.Projects.FindAsync(id);    
            ViewData["OrganizationId"] = project.OrganizationId;
            ViewData["ProjectId"] = id;
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName");
            return View();
        }

        // POST: Tasks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaskId,TaskName,TaskDescription,SkillId,ProjectId,OrganizationId")] Tasks tasks)
        {
            bool taskExists = await _context.Tasks.AnyAsync(t => t.TaskName == tasks.TaskName);

            if (taskExists)
            {
                ModelState.AddModelError("TaskName", "A task with the same name already exists.");
                return View(tasks);
            }
            if (ModelState.IsValid)
            {
                tasks.TaskId = Guid.NewGuid();
                _context.Add(tasks);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details","Projects", new { id = tasks.ProjectId });
            }
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName", tasks.SkillId);
            return View(tasks);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks == null)
            {
                return NotFound();
            }
            
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName", tasks.SkillId);
            return View(tasks);
        }

        // POST: Tasks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("TaskId,TaskName,TaskDescription,TaskStatus,isassigned,SkillId,AssignedEmployeeId,ProjectId,OrganizationId")] Tasks tasks)
        {
            if (id != tasks.TaskId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool taskExists = await _context.Tasks.AnyAsync(t => t.TaskName == tasks.TaskName && t.TaskId != id);

                if (taskExists)
                {
                    ModelState.AddModelError("TaskName", "A task with the same name already exists.");
                    return View(tasks);
                }
                try
                {
                    _context.Update(tasks);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TasksExists(tasks.TaskId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Projects", new { id = tasks.ProjectId });
            }
            ViewData["SkillId"] = new SelectList(_context.Skills, "SkillId", "SkillName", tasks.SkillId);
            return View(tasks);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Tasks == null)
            {
                return NotFound();
            }

            var tasks = await _context.Tasks
                .Include(t => t.AssignedEmployee)
                .Include(t => t.Organization)
                .Include(t => t.Project)
                .Include(t => t.Skill)
                .FirstOrDefaultAsync(m => m.TaskId == id);
            if (tasks == null)
            {
                return NotFound();
            }

            return View(tasks);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Tasks == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tasks'  is null.");
            }
            var tasks = await _context.Tasks.FindAsync(id);
            if (tasks != null)
            {
                _context.Tasks.Remove(tasks);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Projects", new { id = tasks.ProjectId });
        }

        private bool TasksExists(Guid id)
        {
          return (_context.Tasks?.Any(e => e.TaskId == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> AssignTask(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var viewModel = new AssignTaskViewModel
            {
                TaskId = task.TaskId,
                TaskName = task.TaskName,
            };
            var userEmail = HttpContext.User.Identity?.Name;
            var organization = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .FirstOrDefaultAsync();
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.OrganizationId == organization.OrganizationId), "EmployeeId", "EmployeeName");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTask(AssignTaskViewModel viewModel)
        {
            //return Content(viewModel.TaskId.ToString()+","+viewModel.TaskName+","+viewModel.SelectedEmployeeId.ToString());
            if (ModelState.IsValid)
            {
                var task = await _context.Tasks.FindAsync(viewModel.TaskId);
                if (task == null)
                {
                    return NotFound();
                }

                task.AssignedEmployeeId = viewModel.SelectedEmployeeId;
                task.isassigned = true;
                _context.Update(task);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details","Projects", new { id = task.ProjectId });
            }

            // If ModelState is invalid, redisplay the form with validation errors
            var userEmail = HttpContext.User.Identity?.Name;
            var organization = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .FirstOrDefaultAsync();
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(e => e.OrganizationId == organization.OrganizationId), "EmployeeId", "EmployeeName",viewModel.SelectedEmployeeId);

            return View(viewModel);
        }

    }
}
