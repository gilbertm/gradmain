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
    [Authorize(Policy = "AdministratorRequirement")]
    public class ConfigController : Controller
    {
        private readonly GradConfigDbContext _gradConfiguration;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfigController(GradConfigDbContext gradConfiguration, UserManager<ApplicationUser> userManager)
        {
            _gradConfiguration = gradConfiguration;
            _userManager = userManager;
        }


        [HttpPost]
        public async Task<IActionResult> SetShowGraduates(int setShowGraduates = 0)
        {

            if (ModelState.IsValid)
            {

                try
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);

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
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

            }
            return RedirectToAction("Index", "Graduate");
        }

        [Route("/Config/SetShowInitialScreen/{initialScreen}")]
        public void SetShowInitialScreen(string initialScreen = "0")
        {
            try
            {
                var configInitialScreen = _gradConfiguration.GradConfig.SingleOrDefault(c => c.UserId == "Global" && c.Name == "ShowInitialScreen");

                if (configInitialScreen != null)
                {
                    configInitialScreen.Value = initialScreen;
                    _gradConfiguration.Update(configInitialScreen);

                    _gradConfiguration.SaveChanges();
                }

               
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
        }

    }
}
