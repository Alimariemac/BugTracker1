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
        public ProjectHelper ph = new ProjectHelper();
        public UserRolesHelper urh = new UserRolesHelper();


        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public ActionResult AssignUsers(int projectId)
        {
            var userList = ph.UsersNotOnProject(projectId);
            ViewBag.UnassignedUsers = new MultiSelectList(userList, "Id", "FullName");

            return View(db.Projects.FirstOrDefault(p => p.Id == projectId));
        }

        [HttpPost]
        public ActionResult AssignUsers(int projectId, List<string> UnassignedUsers)
        {
            if (UnassignedUsers == null)
            {
                return RedirectToAction("Index", "Projects");
            }
            foreach (var userId in UnassignedUsers)
            {
                ph.AddUserToProject(userId, projectId);
            }
            return RedirectToAction("Index", "Projects");
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public ActionResult unassignUsers(int projectId)
        {
            var userList = ph.UsersOnProject(projectId);
            ViewBag.AssignedUsers = new MultiSelectList(userList, "Id", "FullName");

            return View(db.Projects.FirstOrDefault(p => p.Id == projectId));
        }


        [HttpPost]
        public ActionResult unassignUsers(int projectId, List<string> AssignedUsers)
        {
            if (AssignedUsers == null)
            {
                return RedirectToAction("Index", "Projects");
            }
            foreach (var userId in AssignedUsers)
            {
                ph.RemoveUserFromProject(userId, projectId);
            }
            return RedirectToAction("Index", "Projects");
        }
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public ActionResult reassignUsers(int projectId)
        {
            var userList = ph.UsersOnProject(projectId);
            ViewBag.AssignedUsers = new MultiSelectList(db.Users, "Id", "FullName", userList);

            return View(db.Projects.FirstOrDefault(p => p.Id == projectId));

        }

        [HttpPost]
        public ActionResult reassignUsers(int projectId, List<string> AssignedUsers)
        {
            foreach (var userId in AssignedUsers)
                ph.RemoveUserFromProject(userId, projectId);

                if (AssignedUsers != null)
                {
                    foreach (var userId in AssignedUsers)
                    
                        ph.AddUserToProject(userId, projectId);                                  
                 }
            return RedirectToAction("Index", "Projects");
        }

    }
}