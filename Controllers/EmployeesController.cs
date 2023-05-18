using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TSAIdentity.Data;
using TSAIdentity.Models;
using TSAIdentity.ViewModels;

namespace TSAIdentity.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // GET: Employees
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.User.Identity.Name;
            var organizationId = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();
            var applicationDbContext = _context.Employees.Include(e => e.Designation).Include(e => e.Organization).Where(e=>e.OrganizationId==organizationId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Employees/Details/5
        [Route("Profile")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Designation)
                .Include(e => e.Organization)
                .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
                .Include(e => e.EmployeeProjects).ThenInclude(es => es.Project)
                .Include(e => e.EmployeeTasks).ThenInclude(es => es.Task)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [Authorize(Roles = "Employee")]
        public IActionResult Profile()
        {
            var userEmail = HttpContext.User.Identity?.Name;
            if (userEmail == null)
            {
                return NotFound();
            }
            var EmployeeId = _context.Employees.Where(e => e.EmployeeEmail.Equals(userEmail)).Select(e => e.EmployeeId).FirstOrDefault();

            return RedirectToAction("Details", "Employees", new { id = EmployeeId });
        }
        [Authorize(Roles = "Admin")]
        // GET: Employees/Create
        public async Task<IActionResult> CreateAsync()
        {
            var userEmail = HttpContext.User.Identity?.Name;
            var organization = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .FirstOrDefaultAsync();
            ViewData["OrganizationId"]= organization?.OrganizationId;
            ViewData["OrganizationName"] = organization?.OrganizationName.ToLower();
            ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d=>d.OrganizationId==organization.OrganizationId), "DesignationId", "DesignationName");
            ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(s => s.OrganizationId == organization.OrganizationId), "SkillId", "SkillName");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,EmployeeName,EmployeeContact,EmployeeEmail,EmployeePassword,ConfirmPassword,DesignationId,OrganizationId, SkillIds,IsActive")] Employee employee)
        {
            ViewData["OrganizationId"] = employee.OrganizationId;
            var organizationName = await _context.Organizations.Where(o => o.OrganizationId == employee.OrganizationId).Select(o=>o.OrganizationName).FirstOrDefaultAsync();
            ViewData["OrganizationName"] = organizationName.ToLower();
            ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d => d.OrganizationId == employee.OrganizationId), "DesignationId", "DesignationName", employee.DesignationId);
            ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(s => s.OrganizationId == employee.OrganizationId), "SkillId", "SkillName");
            if (ModelState.IsValid)
            {
                bool rerror = false;
                if (await _context.Employees.AnyAsync(e => e.EmployeeEmail == employee.EmployeeEmail))
                {
                    ModelState.AddModelError("EmployeeEmail", "This email address is already registered.");
                    rerror = true;
                }
                if (await _context.Employees.AnyAsync(e => e.EmployeeContact == employee.EmployeeContact))
                {
                    ModelState.AddModelError("EmployeeContact", "This Employee Contact is already registered.");
                    rerror = true;
                }
                // check if the employee email already exists in the IdentityUser table
                if (await _userManager.FindByEmailAsync(employee.EmployeeEmail) != null)
                {
                    ModelState.AddModelError("EmployeeEmail", "This email address is already registered.");
                    rerror = true;
                }
                if (rerror) {
                    return View(employee);
                }

                var user = new IdentityUser
                {
                    UserName = employee.EmployeeEmail,
                    Email = employee.EmployeeEmail
                };

                var result = await _userManager.CreateAsync(user, employee.EmployeePassword);
                if (result.Succeeded)
                {
                    var roleExists = await _roleManager.RoleExistsAsync("Employee");
                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Employee"));
                    }
                    await _userManager.AddToRoleAsync(user, "Employee");
                }
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                employee.EmployeeId = Guid.NewGuid();
                _context.Add(employee);
                EmployeeSkill employeeSkill = new EmployeeSkill();
                employeeSkill.EmployeeId = employee.EmployeeId;
                if (employee.SkillIds != null && employee.SkillIds.Count > 0)
                {
                    foreach (var skills in employee.SkillIds)
                    {
                        employeeSkill.SkillId = skills;
                        _context.EmployeeSkills.Add(employeeSkill);
                        await _context.SaveChangesAsync();
                    }
                }
                employee.IsActive = true;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            return View(employee);
        }

        // GET: Employees/Edit/5
        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null || _context.Employees == null)
        //    {
        //        return NotFound();
        //    }

        //    var employee = await _context.Employees
        //    .Include(e => e.Designation)
        //    .Include(e => e.Organization)
        //    .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
        //    .FirstOrDefaultAsync(e => e.EmployeeId == id);

        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["OrganizationName"]= await _context.Organizations.Where(o=>o.OrganizationId==employee.OrganizationId).Select(o=>o.OrganizationName).FirstOrDefaultAsync();
        //    ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d => d.OrganizationId == employee.OrganizationId), "DesignationId", "DesignationName", employee.DesignationId);
        //    ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(d => d.OrganizationId == employee.OrganizationId), "SkillId", "SkillName", employee.EmployeeSkills.Select(es => es.SkillId));
        //    return View(employee);
        //}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
            .Include(e => e.Designation)
            .Include(e => e.Organization)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }
            ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(d => d.OrganizationId == employee.OrganizationId), "SkillId", "SkillName", employee.EmployeeSkills.Select(es => es.SkillId));
            var model = new EmployeeEditViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.EmployeeName,
                EmployeeContact = employee.EmployeeContact,
                OrganizationId = employee.OrganizationId,
                SkillIds = employee.EmployeeSkills.Select(es => es.SkillId).ToList()
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("EmployeeId,EmployeeName,EmployeeContact,OrganizationId,SkillIds")] EmployeeEditViewModel employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }
            ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(s => s.OrganizationId == employee.OrganizationId), "SkillId", "SkillName");

            if (ModelState.IsValid)
            {
                bool rerror = false;
                if (await _context.Employees.AnyAsync(e => e.EmployeeContact == employee.EmployeeContact && e.EmployeeId != employee.EmployeeId))
                {
                    ModelState.AddModelError("EmployeeContact", "This employee contact is already registered.");
                    rerror = true;
                }

                if (rerror)
                {
                    return View(employee);
                }
                try
                {
                    // First, retrieve the existing EmployeeSkills for the employee being edited
                    var existingEmployeeSkills = await _context.EmployeeSkills.Where(es => es.EmployeeId == employee.EmployeeId).ToListAsync();

                    // Then, remove any EmployeeSkills that are no longer selected
                    foreach (var existing in existingEmployeeSkills)
                    {
                        if (!employee.SkillIds.Contains(existing.SkillId))
                        {
                            _context.EmployeeSkills.Remove(existing);
                        }
                    }

                    // Finally, add any new EmployeeSkills that have been selected
                    foreach (var skillId in employee.SkillIds)
                    {
                        if (!existingEmployeeSkills.Any(es => es.SkillId == skillId))
                        {
                            var newEmployeeSkill = new EmployeeSkill
                            {
                                EmployeeId = employee.EmployeeId,
                                SkillId = skillId
                            };
                            _context.EmployeeSkills.Add(newEmployeeSkill);
                        }
                    }
                    var employeeupdate = await _context.Employees.FindAsync(employee.EmployeeId);
                    employeeupdate.EmployeeName=employee.EmployeeName;
                    employeeupdate.EmployeeContact = employee.EmployeeContact;

                    _context.Update(employeeupdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (User.IsInRole("Employee"))
                {
                    return RedirectToAction("Profile", "Employees");
                }
                return RedirectToAction(nameof(Index));
            }

            return View(employee);
        }
        public async Task<IActionResult> Promote(Guid? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }
            var employee = await _context.Employees
                .Include(e => e.Designation)
                .Include(e => e.Organization)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d => d.OrganizationId == employee.OrganizationId), "DesignationId", "DesignationName", employee.DesignationId);
            var promotion = new PromoteViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName= employee.EmployeeName,
                DesignationId = employee.DesignationId
            };
            return View(promotion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promote(Guid id, [Bind("EmployeeId,EmployeeName,DesignationId")] PromoteViewModel promote)
        {
            if (id != promote.EmployeeId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var employee = await _context.Employees.FindAsync(promote.EmployeeId);
                employee.DesignationId = promote.DesignationId;
                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Employees", new {id=employee.EmployeeId});
            }
            return View(promote);
        }

            // POST: Employees/Edit/5
            // To protect from overposting attacks, enable the specific properties you want to bind to.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            //[HttpPost]
            //[ValidateAntiForgeryToken]
            //public async Task<IActionResult> Edit(Guid id, [Bind("EmployeeId,EmployeeName,EmployeeContact,EmployeeEmail,EmployeePassword,ConfirmPassword,DesignationId,OrganizationId,IsActive,SkillIds")] Employee employee)
            //{
            //    if (id != employee.EmployeeId)
            //    {
            //        return NotFound();
            //    }
            //    var organizationName = await _context.Organizations.Where(o => o.OrganizationId == employee.OrganizationId).Select(o => o.OrganizationName).FirstOrDefaultAsync();
            //    ViewData["OrganizationName"] = organizationName.ToLower();
            //    ViewData["DesignationId"] = new SelectList(_context.Designations.Where(d => d.OrganizationId == employee.OrganizationId), "DesignationId", "DesignationName", employee.DesignationId);
            //    ViewData["SkillId"] = new MultiSelectList(_context.Skills.Where(s => s.OrganizationId == employee.OrganizationId), "SkillId", "SkillName");

            //    if (ModelState.IsValid)
            //    {
            //        bool rerror = false;
            //        if (await _context.Employees.AnyAsync(e => e.EmployeeEmail == employee.EmployeeEmail && e.EmployeeId !=employee.EmployeeId))
            //        {
            //            ModelState.AddModelError("EmployeeEmail", "This email address is already registered.");
            //            rerror = true;
            //        }
            //        if (await _context.Employees.AnyAsync(e => e.EmployeeContact == employee.EmployeeContact && e.EmployeeId != employee.EmployeeId))
            //        {
            //            ModelState.AddModelError("EmployeeContact", "This employee contact is already registered.");
            //            rerror = true;
            //        }

            //        if (rerror)
            //        {
            //            return View(employee);
            //        }
            //        try
            //        {
            //            // First, retrieve the existing EmployeeSkills for the employee being edited
            //            var existingEmployeeSkills = await _context.EmployeeSkills.Where(es => es.EmployeeId == employee.EmployeeId).ToListAsync();

            //            // Then, remove any EmployeeSkills that are no longer selected
            //            foreach (var existing in existingEmployeeSkills)
            //            {
            //                if (!employee.SkillIds.Contains(existing.SkillId))
            //                {
            //                    _context.EmployeeSkills.Remove(existing);
            //                }
            //            }

            //            // Finally, add any new EmployeeSkills that have been selected
            //            foreach (var skillId in employee.SkillIds)
            //            {
            //                if (!existingEmployeeSkills.Any(es => es.SkillId == skillId))
            //                {
            //                    var newEmployeeSkill = new EmployeeSkill
            //                    {
            //                        EmployeeId = employee.EmployeeId,
            //                        SkillId = skillId
            //                    };
            //                    _context.EmployeeSkills.Add(newEmployeeSkill);
            //                }
            //            }
            //            employee.IsActive = true;
            //            _context.Update(employee);
            //            await _context.SaveChangesAsync();
            //        }
            //        catch (DbUpdateConcurrencyException)
            //        {
            //            if (!EmployeeExists(employee.EmployeeId))
            //            {
            //                return NotFound();
            //            }
            //            else
            //            {
            //                throw;
            //            }
            //        }
            //        return RedirectToAction(nameof(Index));
            //    }

            //    return View(employee);
            //}

            // GET: Employees/Delete/5
            public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Employees == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Designation)
                .Include(e => e.Organization)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Employees == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Employees'  is null.");
            }
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByEmailAsync(employee.EmployeeEmail);

            if (user != null)
            {
                // Delete the user's role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles != null && roles.Count > 0)
                {
                    var result1 = await _userManager.RemoveFromRolesAsync(user, roles);
                    if (!result1.Succeeded)
                    {
                        throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");
                    }
                }
                // Remove the user from the user manager.
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");
                }
                
            }
            // Remove employee skills from EmployeeSkills table
            var employeeSkills = await _context.EmployeeSkills.Where(e => e.EmployeeId == id).ToListAsync();
            if (employeeSkills.Any())
            {
                _context.EmployeeSkills.RemoveRange(employeeSkills);
            }
            //Remove employee Projects from EmployeeProjects table
            var employeeProjects = await _context.EmployeeProjects.Where(e => e.EmployeeId == id).ToListAsync();
            if (employeeProjects.Any())
            {
                _context.EmployeeProjects.RemoveRange(employeeProjects);
            }

            //Remove employee Tasks from EmployeeTasks table
            var employeeTasks = await _context.EmployeeTasks.Where(e => e.EmployeeId == id).ToListAsync();
            if (employeeTasks.Any())
            {
                _context.EmployeeTasks.RemoveRange(employeeTasks);
            }
            //Finally remove employee from Employees table
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(Guid id)
        {
          return (_context.Employees?.Any(e => e.EmployeeId == id)).GetValueOrDefault();
        }
        public async Task<IActionResult> ActivateDeactivate(Guid id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.AssignedEmployeeId == employee.EmployeeId && t.TaskStatus==Models.TaskStatus.Assigned);
            if (task != null)
            {
                task.TaskStatus = Models.TaskStatus.Pending;
                task.isassigned = false;
                task.AssignedEmployeeId = null;
                _context.Update(task);
            }
            employee.IsActive = !employee.IsActive;
            employee.IsBusy = false;
            _context.Update(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Employees/ChangePassword
        public async Task<IActionResult> ChangePassword(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"]=id;
            ViewData["EmployeeEmail"] = await _context.Employees.Where(e => e.EmployeeId == id).Select(e => e.EmployeeEmail).FirstOrDefaultAsync();
            return View();
        }
        // POST: Employees/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            ViewData["EmployeeId"] = model.Id;
            ViewData["EmployeeEmail"] = model.Email;
            if (ModelState.IsValid)
            {
                
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    // User not found
                    return NotFound();
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (changePasswordResult.Succeeded)
                {
                    var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == model.Id);
                    employee.EmployeePassword = model.NewPassword;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();

                    if (User.IsInRole("Employee"))
                    {
                        return RedirectToAction("Profile","Employees");
                    }
                    // Password changed successfully
                    return RedirectToAction(nameof(Index));
                }
                if(!changePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to reset password for IdentityUser.");
                    return View(model);
                }
            }
            
            // If the model is invalid or password change fails, return the view with the model
            return View(model);
        }
    }

}