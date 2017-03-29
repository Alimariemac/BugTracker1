﻿using System;
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
using Microsoft.AspNet.Identity;

namespace BugTrackerV4.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            TicketIndexViewModel model = new TicketIndexViewModel();
            UserRolesHelper urHelper = new UserRolesHelper();
            
                var alltickets = db.Tickets.Include(t => t.AssignedUser)
                    .Include(t => t.CreatorUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType);

                model.AdminTickets = alltickets.ToList();
            

            if (User.IsInRole("ProjectManager"))
            {
                var tickets = db.Projects.Where(p => p.PMID == userId).SelectMany(p => p.Tickets)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatorUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType);                
                model.PMTickets = tickets.ToList();
                ViewBag.PMAmount = model.PMTickets.Count();
            }

            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(t => t.AssignedUserId == userId)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatorUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType);

                    model.DevTickets = tickets.ToList();
                ViewBag.DevAmount = model.DevTickets.Count();
            }
            
            if (User.IsInRole("Submitter"))
            {
                var tickets = db.Tickets.Where(t=> t.CreatorUserId == userId)
                    .Include(t => t.AssignedUser)
                    .Include(t => t.CreatorUser)
                    .Include(t => t.Project)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType);

                model.SubTickets = tickets.ToList();
                ViewBag.SubAmount = model.SubTickets.Count();
            }           
            
           
            return View(model);
       
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        public ActionResult AssignDev(int? ticketId)
        {
            AssignDeveloperViewModel vm = new AssignDeveloperViewModel();
            ProjectHelper ph = new ProjectHelper();
            
            var tkt = db.Tickets.Find(ticketId);
            var devs = ph.ProjectUsersByRole(tkt.ProjectId, "Developer");

            vm.DevUsers = new SelectList(devs, "Id", "FullName");
            vm.ticket = tkt;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignDev(AssignDeveloperViewModel devVM)
        {
            if (ModelState.IsValid)
            {
                var tkt = db.Tickets.Find(devVM.ticket.Id);
                tkt.AssignedUserId = devVM.SelectedUser;
                db.SaveChanges();
                return RedirectToAction("Details", "Projects", new { id = tkt.ProjectId });
            }
            return View(devVM.ticket.Id);
        }


        [Authorize(Roles = "Submitter")]
        // GET: Tickets/Create
        public ActionResult Create()
        {
            UserRolesHelper urh = new UserRolesHelper();
            ViewBag.AssignedUserId = new SelectList(urh.UsersInRole("Developer"), "Id", "FirstName");
            ViewBag.CreatorUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ticket ticket)
        {          
            if (ModelState.IsValid)
            {
                ticket.CreatorUser = db.Users.Find(User.Identity.GetUserId());
                ticket.TicketPriority = db.TicketPriorities.Single(p => p.Name == "Low");
                ticket.TicketStatus = db.TicketStatuses.Single(t=>t.Name=="New");
                ticket.Created = DateTimeOffset.Now;
                ticket.TicketType = db.TicketTypes.Single(t => t.Name == "Default");          
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("Details", "Projects", new {id=ticket.ProjectId } );
            }
            UserRolesHelper urh = new UserRolesHelper();
            ViewBag.AssignedUserId = new SelectList(urh.UsersInRole("Developer"), "Id", "FirstName", ticket.AssignedUserId);
            ViewBag.CreatorUserId = new SelectList(db.Users, "Id", "FirstName", ticket.CreatorUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        //[Authorize(Roles = "ProjectManager")]
        //[HttpGet]
        //public ActionResult AssignUsers(int ticketId)
        //{
        //    UserRolesHelper urh = new UserRolesHelper();
        //    ViewBag.AssignedUserId = new SelectList(urh.UsersInRole("Developer"));

        //    return View(db.Tickets.FirstOrDefault(t => t.Id == ticketId));

        //}

        //[HttpPost]
        //public ActionResult AssignUsers(int ticketId, List<string> AssignedUserId)
        //{
        //    TicketHelper th = new TicketHelper();

        //    if (AssignedUserId != null)
        //    {
        //        return View(db.Tickets.FirstOrDefault(t => t.Id == ticketId));
        //    }

        //    foreach (var userId in AssignedUserId)
        //    {
        //        th.AddTickettoUser(userId, ticketId);             
        //    }
        //    return RedirectToAction("Index", "Tickets");
        //}

        [Authorize(Roles = "ProjectManager")]
        [HttpGet]
        public ActionResult AssignUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            UserRolesHelper urh = new UserRolesHelper();
            ViewBag.AssignedUserId = new SelectList(urh.UsersInRole("Developer"));
            return View(ticket);
        }


        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedUserId);
            ViewBag.CreatorUserId = new SelectList(db.Users, "Id", "FirstName", ticket.CreatorUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,TicketTypeId,TicketPriorityId,TicketStatusId,ProjectId,CreatorUserId,AssignedUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedUserId);
            ViewBag.CreatorUserId = new SelectList(db.Users, "Id", "FirstName", ticket.CreatorUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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
