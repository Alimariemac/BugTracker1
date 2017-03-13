using BugTrackerV4.Helpers;
using BugTrackerV4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerV4.Controllers
{
    public class ProjectUsersController : Controller
        
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult AssignUsers(string id)
        //{
        //    var user = db.Users.Find(id);
        //    var projectList = db.Projects.Select(p=> new ProjectUsersViewModel {projectName= p.Name, s })

        //}
    }
}