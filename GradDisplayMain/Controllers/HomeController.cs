using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GradDisplayMain.Models;


namespace GradDisplayMain.Controllers
{
    public class HomeController : Controller
    {
        private GradConfigDbContext _gradConfiguration;

        public HomeController(GradConfigDbContext gradConfiguration)
        {
            _gradConfiguration = gradConfiguration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ShowInitialScreen()
        {
            return View();
        }

       
        [HttpPost, ActionName("SetAndShowInitialScreenConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult SetAndShowInitialScreenConfirmed()
        {

            var configShowInitialScreen = _gradConfiguration.GradConfig.SingleOrDefault(c => c.UserId == "Global" && c.Name == "ShowInitialScreen");

            if (configShowInitialScreen != null)
            {
                configShowInitialScreen.Value = "1";
                _gradConfiguration.Update(configShowInitialScreen);

                _gradConfiguration.SaveChanges();

            }

            return RedirectToAction("Index", "Graduate");
        }
    }
}
