using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WaMediaWeb.Controllers
{
    public class JobsController : BaseMediaController
    {
        //
        // GET: /Jobs/

        public ActionResult Index()
        {
            return View(this.JobService.Jobs.OrderByDescending( j => j.StartTime));
        }

        public ActionResult JobDetails(string jobId)
        {
            var job = this.JobService.Jobs.FirstOrDefault(j => j.Id.Equals(jobId));
            return View(job);
        }

        public ActionResult DeleteJob(string jobId)
        {
            this.JobService.DeleteJob(jobId);
            return RedirectToAction("Index");
        }
        
    }
}
