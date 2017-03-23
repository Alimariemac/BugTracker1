using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTrackerV4.Models;
using BugTrackerV4.Models.CodeFirst;
using BugTrackerV4.Helpers;

namespace BugTrackerV4.Controllers
{
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Projects
        public ActionResult Index()
        {
            var projs = db.Projects.ToList();
            List<ProjectPMViewModel> model = new List<ProjectPMViewModel>();

            foreach (var p in projs)
            {
                ProjectPMViewModel vm = new ProjectPMViewModel();
                vm.project = p;
                vm.ProjectManager = p.PMID != null ? db.Users.Find(p.PMID) : null;

                model.Add(vm);
            }
            return View(model);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }


        public ActionResult AssignPM(int id)
        {
            AdminProjectViewModel vm = new AdminProjectViewModel();
            UserRolesHelper rh = new UserRolesHelper();

            
            var pms = rh.UsersInRole("ProjectManager");
            vm.PMUsers = new SelectList(pms, "Id", "FullName");
            vm.project = db.Projects.Find(id);
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignPM(AdminProjectViewModel adminVm)
        {
            if (ModelState.IsValid)
            {
                var prj = db.Projects.Find(adminVm.project.Id);
                prj.PMID = adminVm.SelectedUser;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(adminVm.project.Id);
        }

        public ActionResult AssignDev(int id)
        {
            ProjectDevViewModel vm = new ProjectDevViewModel();
            UserRolesHelper rh = new UserRolesHelper();
            ProjectHelper ph = new ProjectHelper();

            var dev = rh.UsersInRole("Developer");
            var projectDev = ph.ProjectUsersByRole(id, "Developer").Select(u=>u.Id).ToArray();

            vm.DevUsers = new MultiSelectList(dev, "Id", "FullName", projectDev);
            vm.project = db.Projects.Find(id);
            
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignDev(ProjectDevViewModel model)
        {
            ProjectHelper ph = new ProjectHelper();
            if (ModelState.IsValid)
            {
                var prj = db.Projects.Find(model.project.Id);
                foreach (var usr in prj.Users)
                {
                    ph.RemoveUserFromProject(usr.Id, prj.Id);
                }

                foreach (var dev in model.SelectedUsers)
                {
                    ph.AddUserToProject(dev, model.project.Id);
                }
            }
            return RedirectToAction("Details", new {id = model.project.Id });
        }


        // GET: Projects/Create
        [Authorize(Roles ="Admin, Project Manager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
