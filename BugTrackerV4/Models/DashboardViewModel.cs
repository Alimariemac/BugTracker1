using BugTrackerV4.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Models
{
    public class DashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
        public List<Ticket> AdminTickets { get; set; }
        public List<Ticket> PMTickets { get; set; }
        public List<Ticket> DevTickets { get; set; }
        public List<Ticket> SubTickets { get; set; }
        public Project project { get; set; }
    }
}