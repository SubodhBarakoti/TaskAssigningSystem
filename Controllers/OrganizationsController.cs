using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class OrganizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public OrganizationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // GET: Organizations
        //public async Task<IActionResult> Index()
        //{
        //      return _context.Organizations != null ? 
        //                  View(await _context.Organizations.ToListAsync()) :
        //                  Problem("Entity set 'ApplicationDbContext.Organizations'  is null.");
        //}

        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Details()
        {
            string userEmail = HttpContext.User.Identity.Name;

            if (userEmail == null || _context.Organizations == null)
            {
                return NotFound();
            }
            
            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.OrganizationEmail == userEmail);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // GET: Organizations/Create
        [Route("Register")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Register")]
        public async Task<IActionResult> Create([Bind("OrganizationId,OrganizationName,OrganizationEmail,OrganizationPassword,ConfirmPassword")] Organization organization)
        {
            if (ModelState.IsValid)
            {
                var existingOrganization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationName == organization.OrganizationName);
                if (existingOrganization != null)
                {
                    ModelState.AddModelError("OrganizationName", "An organization with the same name already exists.");
                    return View(organization);
                }
                var user = new IdentityUser() { UserName = organization.OrganizationEmail, Email = organization.OrganizationEmail };
                var result = await _userManager.CreateAsync(user, organization.OrganizationPassword);
                if (result.Succeeded)
                {
                    var roleExists = await _roleManager.RoleExistsAsync("Admin");
                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                _context.Add(organization);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index","Home");
            }
            return View(organization);
        }

        // Cannot Edit the organization
        //// GET: Organizations/Edit/5
        //public async Task<IActionResult> Edit(Guid? id)
        //{
        //    if (id == null || _context.Organizations == null)
        //    {
        //        return NotFound();
        //    }

        //    var organization = await _context.Organizations.FindAsync(id);
        //    if (organization == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(organization);
        //}

        //// POST: Organizations/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(Guid id, [Bind("OrganizationId,OrganizationName,OrganizationEmail,OrganizationPassword,ConfirmPassword")] Organization organization)
        //{
            //    var existingOrganization = await _context.Organizations.FirstOrDefaultAsync(o => o.OrganizationName == organization.OrganizationName);
            //            if (existingOrganization != null)
            //            {
            //                ModelState.AddModelError("OrganizationName", "An organization with the same name already exists.");
            //                return View(organization);
            //}
    //    if (id != organization.OrganizationId)
    //    {
    //        return NotFound();
    //    }

    //    if (ModelState.IsValid)
    //    {
    //        try
    //        {
    //            _context.Update(organization);
    //            await _context.SaveChangesAsync();
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            if (!OrganizationExists(organization.OrganizationId))
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
    //    return View(organization);
    //}



    // GET: Organizations/Delete/5

    public async Task<IActionResult> ChangePassword(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            ViewData["OrganizationId"] = id;
            ViewData["OrganizationEmail"] = await _context.Organizations.Where(e => e.OrganizationId == id).Select(e => e.OrganizationEmail).FirstOrDefaultAsync();
            return View();
        }


    // POST: Organizations/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        ViewData["OrganizationId"] = model.Id;
        ViewData["OrganizationEmail"] = model.Email;
        
         
        if (ModelState.IsValid)
        {
                
                // Update organization password
                //var passwordHasher = new PasswordHasher<Organization>();
                //organization.OrganizationPassword = passwordHasher.HashPassword(organization, model.NewPassword);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // User not found
                    return NotFound();
                }
                // Update associated IdentityUser password
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (changePasswordResult.Succeeded)
                {
                    var organization = await _context.Organizations.FindAsync(model.Id);
                    if (organization == null)
                    {
                        return NotFound();
                    }
                    organization.OrganizationPassword = model.NewPassword;
                    _context.Update(organization);
                    await _context.SaveChangesAsync();

                    // Password changed successfully
                    return RedirectToAction(nameof(Details));
                }
                if (!changePasswordResult.Succeeded)
                {
                    ModelState.AddModelError("", "Failed to reset password for IdentityUser.");
                    return View(model);
                }

                await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details));
        }

        return View(model);
    }

    public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Organizations == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.OrganizationId == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Organizations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Organizations'  is null.");
            }
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            // Find the IdentityUser with the same email as the organization
            var user = await _userManager.FindByEmailAsync(organization.OrganizationEmail);
            if (user != null)
            {
                // Delete the user's role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles != null && roles.Count > 0)
                {
                    var result = await _userManager.RemoveFromRolesAsync(user, roles);

                }

                // Delete the user
                await _userManager.DeleteAsync(user);
            }

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details));
        }

        private bool OrganizationExists(Guid id)
        {
          return (_context.Organizations?.Any(e => e.OrganizationId == id)).GetValueOrDefault();
        }
    }
}
