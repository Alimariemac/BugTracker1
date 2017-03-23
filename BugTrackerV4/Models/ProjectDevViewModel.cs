using BugTrackerV4.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerV4.Models
{
    public class ProjectDevViewModel
    {
        public Project project { get; set; }
        
        public MultiSelectList DevUsers { get; set; }

        public List<string> SelectedUsers { get; set; }
    }
}