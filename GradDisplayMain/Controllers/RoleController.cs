using GradDisplayMain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GradDisplayMain.Models.RoleViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GradDisplayMain.Controllers
{
    [Authorize(Policy = "AdministratorRequirement")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

            var defaultRoles = new List<string>() { "Administrator", "Authenticated" };

            foreach (var item in defaultRoles)
            {
                var Role = _context.Roles.SingleOrDefault(m => m.Name == item);

                if (Role == null)
                {
                    ApplicationRole nRole = new ApplicationRole();

                    Guid guid = Guid.NewGuid();
                    nRole.Id = guid.ToString();
                    nRole.Name = item;
                    nRole.NormalizedName = item;

                    _context.Roles.Add(nRole);
                    _context.SaveChanges();
                }

            }

        }

        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var Roles = _context.Roles.ToList();
            return View(Roles);
        }

        /// <summary>
        /// Assign user to role
        /// </summary>
        /// <returns></returns>
        public IActionResult CreateUserRole()
        {
            var roleuser = new RoleViewModel();
            ViewBag.RoleId = new SelectList(_context.Roles.Where(r => r.Name != "Administrator").ToList(), "Id", "Name");
            ViewBag.UserId = new SelectList(_context.Users.ToList(), "Id", "UserName");

            return View(roleuser);
        }

        /// <summary>
        /// Assign user to role
        /// </summary>
        /// <param name="RoleViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateUserRole(RoleViewModel roleuser)
        {
            if (ModelState.IsValid)
            {
                var oRole = _context.Roles.SingleOrDefault(m => m.Id == roleuser.RoleId.ToString());
                var oUser = _context.Users.SingleOrDefault(m => m.Id == roleuser.UserId.ToString());

                var oUserInRole = _context.UserRoles.SingleOrDefault(m => m.UserId == roleuser.UserId.ToString() && m.RoleId == roleuser.RoleId.ToString());

                if (oUserInRole == null)
                {
                    //Assign Role to user Here 
                    await _userManager.AddToRoleAsync(oUser, oRole.Name.ToString());
                    //Ends Here
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Create  a New role
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            var Role = new ApplicationRole();
            return View(Role);
        }



        /// <summary>
        /// Create a New Role
        /// </summary>
        /// <param name="Role"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create(ApplicationRole Role)
        {
            Role.NormalizedName = Role.Name;

            _context.Roles.Add(Role);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Graduate/Delete/5
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var Role = _context.Roles.SingleOrDefault(m => m.Id == id);
            if (Role == null)
            {
                return NotFound();
            }

            return View(Role);
        }

        // POST: Graduate/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var Role = _context.Roles.SingleOrDefault(m => m.Id == id);
            _context.Roles.Remove(Role);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


    }
}
