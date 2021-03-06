﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Models.CodeFirst
{

    public class TicketNotification
    {
        public int Id { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}