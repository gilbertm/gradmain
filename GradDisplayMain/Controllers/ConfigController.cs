using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using GradDisplayMain.Models;
using GradDisplayMain.Services;
using GradDisplayMain.Models.ManageViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GradDisplayMain.Controllers
{
    [Authorize]
    public class ConfigController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GradConfigDbContext _gradConfiguration;

        public ConfigController(
        UserManager<ApplicationUser> userManager,
        GradConfigDbContext gradConfiguration)
        {
            _userManager = userManager;
            _gradConfiguration = gradConfiguration;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.FindByIdAsync(_userManager.GetUserId(HttpContext.User));
        }


        [HttpPost]
        public async Task<IActionResult> SetShowGraduates(int setShowGraduates = 0)
        {

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    try {
                        var configShowGraduates = _gradConfiguration.GradConfig.SingleOrDefault(c => c.UserId == user.Id && c.Name == "ShowGraduates");

                        if (configShowGraduates != null)
                        {
                            configShowGraduates.Value = setShowGraduates.ToString();
                            _gradConfiguration.Update(configShowGraduates);
                        }
                        else
                        {
                            _gradConfiguration.Add(new GradConfig { Name = "ShowGraduates", UserId = user.Id, Value = setShowGraduates.ToString() });
                        }


                        _gradConfiguration.SaveChanges();
                    } catch (Exception e)
                    {
                        Console.Write(e.Message);
                        // System.Diagnostics.Debug.Write("Error DB Write.");
                    }
                }
            }
            return RedirectToAction("Index", "Graduate");
        }

    }
}
