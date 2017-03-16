using BugTrackerV4.Models;
using BugTrackerV4.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Helpers
{
    public class TicketHelper
    {
        ApplicationDbContext db = new ApplicationDbContext();
        public UserRolesHelper urh = new UserRolesHelper();

        public bool IsUserOnTicket(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);
            var flag = (ticket.AssignedUser.Id == userId);
            return flag;
        }

        public ICollection<Ticket> ListUserTickets(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);

            var tickets = user.AssignedTickets.ToList();
            return (tickets);

        }

        public void AddTickettoUser(string userId, int ticketId)
        {
            if (!IsUserOnTicket(userId, ticketId))
            {
                Ticket ticket = db.Tickets.Find(ticketId);
                var newTicket = db.Users.Find(userId);
                newTicket.AssignedTickets.Add(ticket);
                db.SaveChanges();
                
            }
        }

        public void RemoveTicketfromUser(string userId, int ticketId)
        {
            if (IsUserOnTicket(userId, ticketId))
            {

                Ticket ticket = db.Tickets.Find(ticketId);
                var remTicket = db.Users.Find(userId);

                remTicket.AssignedTickets.Remove(ticket);
                db.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
        }


       /*public ICollection<Ticket> TicketsAssignedtoUser(int ticketId)
        {
            
        }*/
    }
}