using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using GradDisplayMain.Models;
using Microsoft.AspNetCore.Hosting;
using GradDisplayMain.Models.QueueViewModels;

namespace GradDisplayMain.Controllers
{
    [Authorize(Policy = "AdministratorRequirement")]
    public class QueueController : Controller
    {
        private QueueDbContext _contextQueue;
        private TelepromptDbContext _contextTeleprompt;
        private GraduateDbContext _contextGraduate;

        private readonly IHostingEnvironment _hostEnvironment;

        // mimic the way startup is getting
        // custom configuration variables
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextQueue"></param>
        /// <param name="contextGraduate"></param>
        /// <param name="contextTeleprompt"></param>
        /// <param name="appEnvironment"></param>
        /// <param name="hostEnvironment"></param>
        public QueueController(QueueDbContext contextQueue, GraduateDbContext contextGraduate, TelepromptDbContext contextTeleprompt, IHostingEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;

            _contextQueue = contextQueue;
            _contextGraduate = contextGraduate;
            _contextTeleprompt = contextTeleprompt;

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
        [HttpPost]
        public IActionResult IndexList()
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

            var graduates = new List<QueueViewModel>();

            // even with preload
            // just get the first five
            // to minimize ajax queries
            foreach (var q in _contextQueue.Queue.OrderBy(x => x.Created).Take(5))
            {
                foreach (var g in _contextGraduate.Graduate)
                {
                    if (q.GraduateId == g.GraduateId)
                    {
                        graduates.Add(new QueueViewModel()
                        {
                            GraduateId = g.GraduateId,
                            GraduateScannerId = g.GraduateScannerId,
                            Status = g.Status,
                            Arabic = g.Arabic,
                            School = g.School,
                            Program = g.Program,
                            Major = g.Major,
                            Merit = g.Merit,
                            FirstName = g.FirstName,
                            ArabicFullname = g.ArabicFullname,
                            Fullname = g.Fullname,
                            LastName = g.LastName,
                            MiddleName = g.MiddleName,
                            Created = q.Created
                        });
                    }
                }
            }

            // get the total number in the queues table
            // the the take(5)
            ViewBag.TotalGraduates = _contextQueue.Queue.Count();

            // take must be of same
            // number as the queue limit above
            return PartialView("_QueueListPartial", graduates.ToList().Take(5));
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
                        _contextQueue.Queue.Add(new Queue() { GraduateId = searchString, Created = System.DateTime.Now});
                        _contextQueue.SaveChanges();

                    }
                }
            }


            return RedirectToAction("Index", "Graduate");
        }


        [HttpGet]
        public IActionResult Preload()
        {
            return View();
        }

        [HttpPost, ActionName("PreloadConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult PreloadConfirmed()
        {
            // remove all teleprompts
            {
                var all = from c in _contextTeleprompt.Teleprompt select c;
                _contextTeleprompt.Teleprompt.RemoveRange(all);
                _contextTeleprompt.SaveChanges();
            }

            // remove all queues
            {
                var all = from c in _contextQueue.Queue select c;
                _contextQueue.Queue.RemoveRange(all);
                _contextQueue.SaveChanges();
            }

            foreach (var g in _contextGraduate.Graduate.OrderBy(o => o.GraduateId))
            {
                Queue queue = _contextQueue.Queue.SingleOrDefault(q => q.GraduateId == g.GraduateId);

                // not in queue
                if (queue == null)
                {
                    // add
                    _contextQueue.Queue.Add(new Queue() { GraduateId = g.GraduateId, Created = System.DateTime.Now });
                    _contextQueue.SaveChanges();

                }
            }

           
            return RedirectToAction("Index", "Graduate");
        }

        [HttpGet]
        public IActionResult Clean()
        {
            return View();
        }

        [HttpPost, ActionName("CleanConfirmed")]
        [ValidateAntiForgeryToken]
        public IActionResult CleanConfirmed()
        {
            // remove all teleprompts
            {
                var all = from c in _contextTeleprompt.Teleprompt select c;
                _contextTeleprompt.Teleprompt.RemoveRange(all);
                _contextTeleprompt.SaveChanges();
            }

            // remove all queues
            {
                var all = from c in _contextQueue.Queue select c;
                _contextQueue.Queue.RemoveRange(all);
                _contextQueue.SaveChanges();
            }

           
            return RedirectToAction("Index", "Graduate");
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

            Queue queue =  _contextQueue.Queue.Single(m => m.GraduateId == id);
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
            Queue queue = _contextQueue.Queue.Single(m => m.GraduateId == id);
            _contextQueue.Queue.Remove(queue);

            _contextQueue.SaveChangesAsync();

            return RedirectToAction("Index", "Graduate");
        }
    }
}
