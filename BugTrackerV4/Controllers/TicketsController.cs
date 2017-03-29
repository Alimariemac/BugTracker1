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
                var oldTkt = db.Tickets.AsNoTracking().Where(m => m.Id == tkt.Id).FirstOrDefault();     
                tkt.AssignedUserId = devVM.SelectedUser;
                db.SaveChanges();

                if (oldTkt.AssignedUserId != tkt.AssignedUserId)
                {
                    TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Assigned User";
                    tHistory.OldValue = db.Users.Find(oldTkt.AssignedUserId).FullName;
                    tHistory.NewValue = db.Users.Find(tkt.AssignedUserId).FullName;                    
                    tHistory.TicketId = tkt.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();
                }
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

        //[Authorize(Roles = "ProjectManager")]
        //[HttpGet]
        //public ActionResult AssignUser(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    if (ticket == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    UserRolesHelper urh = new UserRolesHelper();
        //    ViewBag.AssignedUserId = new SelectList(urh.UsersInRole("Developer"));
        //    return View(ticket);
        //}


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
                //get a copy of the "old" ticket
                Ticket oldTicket = db.Tickets.AsNoTracking().Where(m => m.Id == ticket.Id).FirstOrDefault();

                ticket.Updated = DateTimeOffset.Now;
                db.Entry(ticket).State = EntityState.Modified;
                //Always save the changes user is trying to make FIRST
                db.SaveChanges();
                //Add history record
                if (oldTicket.TicketPriorityId != ticket.TicketPriorityId)
                {
                    TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Ticket Priority";
                    tHistory.OldValue = db.TicketPriorities.Find(oldTicket.TicketPriorityId).Name;
                    tHistory.NewValue = db.TicketPriorities.Find(ticket.TicketPriorityId).Name;
                    tHistory.TicketId = ticket.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();
                }

                if (oldTicket.TicketTypeId != ticket.TicketTypeId)
                {
                    TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Ticket Type";
                    tHistory.OldValue = db.TicketTypes.Find(oldTicket.TicketTypeId).Name;
                    tHistory.NewValue = db.TicketTypes.Find(ticket.TicketTypeId).Name;
                    tHistory.TicketId = ticket.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();

                }

                //if (oldTicket.AssignedUserId != ticket.AssignedUserId)
                //{
                //    TicketHistory tHistory = new TicketHistory();
                //    tHistory.ChangedDate = DateTimeOffset.Now;
                //    tHistory.Property = "Assigned User";
                //    if (oldTicket.AssignedUserId == null)
                //    {
                //        tHistory.OldValue = "Not Assigned";
                //        tHistory.NewValue = db.Users.Find(ticket.AssignedUserId).FullName;
                //    }
                //    else
                //    {
                //        tHistory.OldValue = db.Users.Find(oldTicket.AssignedUserId).FullName;
                //        tHistory.NewValue = db.Users.Find(ticket.AssignedUserId).FullName;
                //    }
                //    tHistory.TicketId = ticket.Id;
                //    tHistory.UserId = User.Identity.GetUserId();

                //    db.TicketHistories.Add(tHistory);
                //    db.SaveChanges();
                //}
                if (oldTicket.Description != ticket.Description)
                {
                    TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Description";
                    tHistory.OldValue = oldTicket.Description;
                    tHistory.NewValue = ticket.Description;
                    tHistory.TicketId = ticket.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();
                }

                if (oldTicket.Title != ticket.Title)
                {
                    TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Title";
                    tHistory.OldValue = oldTicket.Title;
                    tHistory.NewValue = ticket.Title;
                    tHistory.TicketId = ticket.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();
                }

                if (oldTicket.TicketStatusId != ticket.TicketStatusId)
                {   TicketHistory tHistory = new TicketHistory();
                    tHistory.ChangedDate = DateTimeOffset.Now;
                    tHistory.Property = "Ticket Status";
                    tHistory.OldValue = db.TicketStatuses.Find(oldTicket.TicketStatusId).Name;
                    tHistory.NewValue = db.TicketStatuses.Find(ticket.TicketStatusId).Name;
                    tHistory.TicketId = ticket.Id;
                    tHistory.UserId = User.Identity.GetUserId();

                    db.TicketHistories.Add(tHistory);
                    db.SaveChanges();
                }

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
