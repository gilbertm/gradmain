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
using GradDisplayMain.Models.QueueViewModels;
using System.Collections.Generic;
using GradDisplayMain.Models.GraduateViewModels;

namespace GradDisplayMain.Controllers
{

    [Authorize(Policy = "AdministratorRequirement")]
    public class ScanController : Controller
    {
        private QueueDbContext _contextQueue;
        private GraduateDbContext _contextGraduate;
        private TelepromptDbContext _contextTeleprompt;
        private readonly IHostingEnvironment _hostEnvironment;
        private GradConfigDbContext _gradConfiguration;
        private readonly UserManager<ApplicationUser> _userManager;

        // mimic the way startup is getting
        // custom configuration variables
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextQueue"></param>
        /// <param name="gradConfiguration"></param>
        /// <param name="appEnvironment"></param>
        /// <param name="hostEnvironment"></param>
        public ScanController(QueueDbContext contextQueue, GraduateDbContext contextGraduate, TelepromptDbContext contextTeleprompt, GradConfigDbContext gradConfiguration, IHostingEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _contextQueue = contextQueue;
            _contextGraduate = contextGraduate;
            _contextTeleprompt = contextTeleprompt;
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

            var graduatesIdString = String.Empty;
            var graduates = new List<QueueViewModel>();

            // even with preload
            // just get the first five
            // to minimize ajax queries
            var first = String.Empty;
            var ictr = 0;
            foreach (var q in _contextQueue.Queue.OrderBy(x => x.Created))
            {
                if (q != null && ictr == 0)
                {
                    first = q.GraduateId.ToString();
                    ictr++;
                }

                var grad = _contextGraduate.Graduate.FirstOrDefault(g => g.GraduateId == q.GraduateId);

                if (grad != null)
                {
                    graduates.Add(new QueueViewModel()
                    {
                        GraduateId = grad.GraduateId,
                        GraduateScannerId = grad.GraduateScannerId,
                        Status = grad.Status,
                        Arabic = grad.Arabic,
                        School = grad.School,
                        Program = grad.Program,
                        Major = grad.Major,
                        Merit = grad.Merit,
                        FirstName = grad.FirstName,
                        ArabicFullname = grad.ArabicFullname,
                        Fullname = grad.Fullname,
                        LastName = grad.LastName,
                        MiddleName = grad.MiddleName,
                        Created = q.Created
                    });
                }
            }

            ViewBag.TotalGraduates = _contextQueue.Queue.Count();

            return View(graduates.OrderByDescending(o => o.Created).ToList());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create(string searchString)
        {
            if (searchString != null)
            {
                Teleprompt teleprompt = _contextTeleprompt.Teleprompt.SingleOrDefault(m => m.GraduateId == searchString);

                // not in teleprompt
                if (teleprompt == null)
                {
                    Queue queue = _contextQueue.Queue.SingleOrDefault(m => m.GraduateId == searchString);

                    // not in queue
                    if (queue == null)
                    {
                        // add
                        _contextQueue.Queue.Add(new Queue() { GraduateId = searchString, Created = System.DateTime.Now });
                        _contextQueue.SaveChanges();

                    }
                }
            }


            return RedirectToAction("Index", "Scan");
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

            Queue queue = _contextQueue.Queue.Single(m => m.GraduateId == id);
            if (queue == null)
            {
                return NotFound();
            }

            return View(queue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            Queue queue = _contextQueue.Queue.SingleOrDefault(m => m.GraduateId == id);

            if (queue != null)
            {
                _contextQueue.Queue.Remove(queue);
                _contextQueue.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Scan");
        }


        // GET: scan/graduates
        [HttpGet]
        public IActionResult Graduates()
        {
            /* get the teleprompts */
            var graduates = _contextGraduate.Graduate.ToList();

            ICollection<SimpleGraduateViewModel> dict = new List<SimpleGraduateViewModel>();

            if (graduates != null)
            {
                foreach (var item in graduates)
                {
                    dict.Add(new SimpleGraduateViewModel
                    {
                        Text = item.GraduateId + " " + item.Fullname,
                        Value = item.GraduateId
                    });

                }
            }

            return Json(dict);
        }
    }
}
