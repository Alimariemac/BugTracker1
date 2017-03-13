using BugTrackerV4.Helpers;
using BugTrackerV4.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerV4.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public UserRolesHelper ur = new UserRolesHelper();
        public ProjectHelper ph = new ProjectHelper();
        // GET: Admin
        public ActionResult Users()
        {
            var users = new List<UsersViewModel>();
            UserRolesHelper helper = new UserRolesHelper();
            foreach (var user in db.Users)
            {
                var uservm = new UsersViewModel();
                uservm.User = user;
                uservm.Roles = helper.ListUserRoles(user.Id).ToList();
                users.Add(uservm);
            }
            return View(users);
        }

        public ActionResult EditUser(string id)
        {
            var user = db.Users.Find(id);
            var roleList = db.Roles.Select(r => new UserRoleViewModel { Name = r.Name, UserId = id, IsInRole = r.Users.Any(u => u.UserId == id) });
            var selected = roleList.Where(r => r.IsInRole).Select(n => n.Name).ToArray();
            var selectList = new MultiSelectList(roleList, "Name", "Name", selected);
            var model = new AdminUserViewModel
                {
                    User = user,
                    Roles = selectList,
                    SelectedRoles = selected
                };
            return View(model); 
        }

        [HttpPost]
        public ActionResult EditUser(AdminUserViewModel model)
        {
            model.User = db.Users.Find(model.User.Id);
            var um = Request.GetOwinContext().Get<ApplicationUserManager>();
            string[] sel = { };
            var SelRoles = model.SelectedRoles != null ? model.SelectedRoles : sel;
            foreach (var role in db.Roles.ToList())
            {
                if (SelRoles.Contains(role.Name))
                    um.AddToRole(model.User.Id, role.Name);
               /* else
                    if (!(role.Name == "Admin" && model.User.UserName == "rmanglani@coderfoundry.com"))
                    um.RemoveFromRole(model.User.Id, role.Name);*/
            }
            /*return RedirectToAction("EditUser", new { Id = model.User.Id });
            return RedirectToAction("DetailsUserRoles", new { Id = model.User.Id });*/
            return RedirectToAction("Users");
        }
    }
}