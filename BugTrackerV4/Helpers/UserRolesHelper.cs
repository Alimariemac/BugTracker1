using BugTrackerV4.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerV4.Helpers
{
    public class UserRolesHelper
    {
        private UserManager<ApplicationUser> manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

        public bool IsUserInRole(string userId, string roleName)
        {
            return manager.IsInRole(userId, roleName);
        }

        public ICollection<string> ListUserRoles(string userId)
        {
            return manager.GetRoles(userId);
        }

        public bool AddUserToRole(string userId, string roleName)
        {
            var result = manager.AddToRole(userId, roleName);
            return result.Succeeded;
        }
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = manager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            //var List = manager.Users.ToList();
            //foreach (var user in List)
            //{
            //    if (IsUserInRole(user.Id, roleName))
            //        resultList.Add(user);
            //}


            //var userIDs = System.Web.Security.Roles.GetUsersInRole(roleName);
            //return manager.Users.Where(u => userIDs.Contains(u.Id)).Select(u => new UserDropDownViewModel { UserId = u.Id, Name = u.DisplayName }).ToList();

            var userIDs = roleManager.Roles.FirstOrDefault(r => r.Name == roleName).Users.Select(u => u.UserId).ToList();
            return manager.Users.Where(u => userIDs.Contains(u.Id)).ToList();
            //var roleId = db.Roles.FirstOrDefault(r => r.Name == roleName).Id;
            //resultList = manager.Users.Where(u => u.Roles.Any(r => r.RoleId == roleId)).ToList();
            //return resultList;
        }

        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            //var List = manager.Users.ToList();
            //foreach (var user in List)
            //{
            //    if (!IsUserInRole(user.Id, roleName))
            //        resultList.Add(user);
            //}

            //var userIDs = roleManager.Roles.FirstOrDefault(r => r.Name == roleName).Users.Select(u => u.UserId).ToList();
            //return manager.Users.Where(u => !userIDs.Contains(u.Id)).ToList();

            //var userIDs = System.Web.Security.Roles.GetUsersInRole(roleName);
            //return manager.Users.Where(u => !userIDs.Contains(u.Id)).Select(u => new UserDropDownViewModel { UserId = u.Id, Name = u.DisplayName }).ToList();

            var roleId = db.Roles.FirstOrDefault(r => r.Name == roleName).Id;
            resultList = manager.Users.Where(u => u.Roles.Any(r => r.RoleId != roleId)).ToList();
            return resultList;
        }

    }
}