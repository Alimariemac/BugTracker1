using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Models.CodeFirst
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        public virtual ApplicationUser Users { get; set; }
        public virtual Project Project { get; set; }
    }
}