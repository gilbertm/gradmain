using System;
using System.Linq;
using GradDisplayMain.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Mvc;
using GradDisplayMain.Models.TelepromptViewModels;

namespace GradDisplayMain.Controllers
{
    [Authorize(Policy = "AdministratorRequirement")]
    public class TelepromptController : Controller
    {
        private TelepromptDbContext _contextTeleprompt;
        private GraduateDbContext _contextGraduate;
        private QueueDbContext _contextQueue;

        private readonly IHostingEnvironment _hostEnvironment;

        // mimic the way startup is getting
        // custom configuration variables
        public IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contextQueue"></param>
        /// <param name="contextGraduate"></param>
        /// <param name="appEnvironment"></param>
        /// <param name="hostEnvironment"></param>
        public TelepromptController(TelepromptDbContext contextTeleprompt, QueueDbContext contextQueue, GraduateDbContext contextGraduate, IHostingEnvironment hostEnvironment)
        {
            _contextTeleprompt = contextTeleprompt;
            _contextGraduate = contextGraduate;
            _contextQueue = contextQueue;

            _hostEnvironment = hostEnvironment;

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
        /// <param name="voicePerson"></param>
        /// <param name="voiceExtra"></param>
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

            /* teleprompt - single */
            var t = _contextTeleprompt.Teleprompt.FirstOrDefault();

            TelepromptViewModel Graduate = null;

            if (t != null)
            {
                /* graduate details from t */
                Graduate = (from g in _contextGraduate.Graduate.Where(x => x.GraduateId == t.GraduateId)
                            select new TelepromptViewModel()
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
                                Fullname = g.Fullname,
                                ArabicFullname = g.ArabicFullname,
                                LastName = g.LastName,
                                MiddleName = g.MiddleName,
                                Created = t.Created
                            }).FirstOrDefault();
            }

            return PartialView("_TelepromptListPartial", Graduate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Create()
        {

            // clean teleprompt
            foreach (var item in _contextTeleprompt.Teleprompt)
            {
                _contextTeleprompt.Teleprompt.Remove(item);
            }
            // save don't wait
            _contextTeleprompt.SaveChanges();

            // remove the top of the queue
            var itemTopQueue = _contextQueue.Queue.OrderBy(q => q.Created).FirstOrDefault();
            if (itemTopQueue != null)
            {
                _contextQueue.Remove(itemTopQueue);
                // save don't wait
                _contextQueue.SaveChanges();

                // add to teleprompt
                var respectedTime = DateTime.Now.ToString();

                var teleprompt = new Teleprompt() { GraduateId = itemTopQueue.GraduateId, Created = itemTopQueue.Created };
                _contextTeleprompt.Teleprompt.Add(teleprompt);
                _contextTeleprompt.SaveChanges();

                var graduate = _contextGraduate.Graduate.FirstOrDefault(m => m.GraduateId == itemTopQueue.GraduateId);

                // save to the disk
                try
                {
                    var mainFilename = _hostEnvironment.WebRootPath + "\\" + "resources" + "\\" + "log" + "\\" + "log.txt";
                    using (var fs = new System.IO.FileStream(mainFilename, System.IO.FileMode.Append))
                    {
                        using (var stream = new System.IO.StreamWriter(fs))
                        {
                            stream.WriteLine(respectedTime + " Id:" + graduate.GraduateId + " Name:" + graduate.FirstName + " " + graduate.LastName);

                            stream.Flush();
                        }


                    }
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                    // ignore any io error
                }

                if (graduate != null)
                {
                    graduate.Status = 1;
                }
                // save don't wait
                _contextGraduate.SaveChanges();

            }

            // handle any misc
            // write tasks
            _contextTeleprompt.SaveChanges();
            _contextGraduate.SaveChanges();
            _contextQueue.SaveChanges();


            return RedirectToAction("Index", "Graduate");

        }
    }
}
