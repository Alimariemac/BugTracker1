using BugTrackerV4.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Models
{
    public class ProjectPMViewModel
    {
        public Project project { get; set; }
        public ApplicationUser ProjectManager { get; set; }
    }
}