using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GradDisplayMain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace GradDisplayMain.Controllers
{

    [Authorize(Policy = "AdministratorRequirement")]
    public class GraduateController : Controller
    {
        private GraduateDbContext _contextGraduate;
        private readonly IHostingEnvironment _hostEnvironment;
        private GradConfigDbContext _gradConfiguration;
        private readonly UserManager<ApplicationUser> _userManager;

        // mimic the way startup is getting
        // custom configuration variables
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextGraduate"></param>
        /// <param name="gradConfiguration"></param>
        /// <param name="appEnvironment"></param>
        /// <param name="hostEnvironment"></param>
        public GraduateController(GraduateDbContext contextGraduate, GradConfigDbContext gradConfiguration, IHostingEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _contextGraduate = contextGraduate;
            _hostEnvironment = hostEnvironment;
            _gradConfiguration = gradConfiguration;
            _userManager = userManager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(hostEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            ViewBag.isFullName = false;
            ViewBag.isShowGraduates = false;

            var configShowGraduates = _gradConfiguration.GradConfig.SingleOrDefault(c => c.UserId == _userManager.GetUserId(HttpContext.User) && c.Name == "ShowGraduates");

            if (configShowGraduates != null)
            {
                ViewBag.isShowGraduates = configShowGraduates.Value == "1" ? true : false;
            }

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            return View(_contextGraduate.Graduate.ToList());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult IndexSearch(string searchString)
        {
            ViewBag.root = _hostEnvironment.ContentRootPath;
            ViewBag.wwwroot = _hostEnvironment.WebRootPath;
            ViewBag.hosting = HttpContext.Request.Host;

            ViewBag.voicePerson = null;
            ViewBag.voiceExtra = null;
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            if (!String.IsNullOrEmpty(Configuration["Custom:Voice:person"]))
            {
                ViewBag.voicePerson = Configuration["Custom:Voice:person"];
            }

            if (!String.IsNullOrEmpty(Configuration["Custom:Voice:extra"]))
            {
                ViewBag.voiceExtra = Configuration["Custom:Voice:extra"];
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                // all status 0
                // not yet queued/prompted
                ViewBag.TotalGraduates = _contextGraduate.Graduate.Count(g => g.Status == 0);

                return PartialView("_GraduateFilterSinglePartial", _contextGraduate.Graduate.Where(s => s.GraduateId.Contains(searchString)).OrderBy(sort => sort.Status).ToList());
            }
            else
            {
                // all status 0
                // not yet queued/prompted
                ViewBag.TotalGraduates = _contextGraduate.Graduate.Count(g => g.Status == 0);

                // limit to five
                // too much records can cause
                // delay on display/refresh
                return PartialView("_GraduateFilterSinglePartial", _contextGraduate.Graduate.OrderBy(sort => sort.Status).ToList().Take(15));
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="searchString"></param>
        /// <returns></returns>
        [HttpPost]
        public string Index(FormCollection fc, string searchString)
        {
            return searchString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Details(string id)
        {
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            if (id == null)
            {
                return NotFound();
            }

            Graduate graduate = _contextGraduate.Graduate.Single(m => m.GraduateId == id);
            if (graduate == null)
            {
                return NotFound();
            }



            return View(graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }


            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graduate"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Graduate graduate)
        {
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            if (ModelState.IsValid)
            {
                _contextGraduate.Graduate.Add(graduate);
                await _contextGraduate.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(string id)
        {
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            if (id == null)
            {
                return NotFound();
            }

            Graduate graduate = _contextGraduate.Graduate.Single(m => m.GraduateId == id);
            if (graduate == null)
            {
                return NotFound();
            }
            return View(graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> ResetStatus()
        {
            foreach (var item in _contextGraduate.Graduate)
            {
                item.Status = 0;
            }

            await _contextGraduate.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graduate"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Graduate graduate)
        {
            ViewBag.isFullName = false;

            if (!String.IsNullOrEmpty(Configuration["Custom:Fields:fullname"]))
            {
                ViewBag.isFullName = Configuration["Custom:Fields:fullname"] == "yes" ? true : false;
            }

            if (ModelState.IsValid)
            {
                _contextGraduate.Update(graduate);
                await _contextGraduate.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ActionName("Delete")]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Graduate graduate = _contextGraduate.Graduate.Single(m => m.GraduateId == id);
            if (graduate == null)
            {
                return NotFound();
            }

            return View(graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            Graduate graduate = _contextGraduate.Graduate.Single(m => m.GraduateId == id);
            _contextGraduate.Graduate.Remove(graduate);

            await _contextGraduate.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
