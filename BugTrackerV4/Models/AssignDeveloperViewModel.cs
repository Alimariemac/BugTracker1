using BugTrackerV4.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerV4.Models
{
    public class AssignDeveloperViewModel
    {
        public Ticket ticket { get; set; }
        public SelectList DevUsers { get; set; }
        public string SelectedUser { get; set; }
    }
}