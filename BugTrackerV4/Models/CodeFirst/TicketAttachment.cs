using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Models.CodeFirst
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string FileUrl { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser Users { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}