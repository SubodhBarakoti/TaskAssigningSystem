using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TSAIdentity.Data;
using TSAIdentity.Models;


namespace TSAIdentity.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DesignationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DesignationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Designations
        public async Task<IActionResult> Index()
        {
            var userEmail = HttpContext.User.Identity.Name;
            var organizationId = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();

            var applicationDbContext = _context.Designations.Include(d => d.Organization).Where(d => d.OrganizationId == organizationId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Designations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Designations == null)
            {
                return NotFound();
            }

            var designation = await _context.Designations
                .Include(d => d.Organization)
                .FirstOrDefaultAsync(m => m.DesignationId == id);
            if (designation == null)
            {
                return NotFound();
            }

            return View(designation);
        }

        // GET: Designations/Create
        public async Task<IActionResult> CreateAsync()
        {
            string userEmail = HttpContext.User.Identity.Name;
            ViewData["OrganizationId"] = await _context.Organizations
                          .Where(o => o.OrganizationEmail == userEmail)
                          .Select(o => o.OrganizationId)
                          .FirstOrDefaultAsync();

            return View();
        }

        // POST: Designations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DesignationId,DesignationName,OrganizationId")] Designation designation)
        {
            if (ModelState.IsValid)
            {
                var existingDesignation = await _context.Designations.FirstOrDefaultAsync(d => d.DesignationName == designation.DesignationName && d.OrganizationId == designation.OrganizationId);
                if (existingDesignation != null)
                {
                    ModelState.AddModelError("DesignationName", "A designation with the same name already exists.");
                    return View(designation);
                }
                designation.DesignationId = Guid.NewGuid();
                _context.Add(designation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(designation);
        }

        // GET: Designations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Designations == null)
            {
                return NotFound();
            }

            var designation = await _context.Designations.FindAsync(id);
            if (designation == null)
            {
                return NotFound();
            }
            return View(designation);
        }

        // POST: Designations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("DesignationId,DesignationName,OrganizationId")] Designation designation)
        {
            if (id != designation.DesignationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(designation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DesignationExists(designation.DesignationId))
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
            return View(designation);
        }

        // GET: Designations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Designations == null)
            {
                return NotFound();
            }

            var designation = await _context.Designations
                .Include(d => d.Organization)
                .FirstOrDefaultAsync(m => m.DesignationId == id);
            if (designation == null)
            {
                return NotFound();
            }

            return View(designation);
        }

        // POST: Designations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Designations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Designations'  is null.");
            }
            var designation = await _context.Designations.FindAsync(id);
            if (designation != null)
            {
                _context.Designations.Remove(designation);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DesignationExists(Guid id)
        {
          return (_context.Designations?.Any(e => e.DesignationId == id)).GetValueOrDefault();
        }
    }
}
