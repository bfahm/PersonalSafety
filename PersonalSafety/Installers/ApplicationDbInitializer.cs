using Microsoft.AspNetCore.Identity;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Installers
{
    public static class ApplicationDbInitializer
    {
        public static void SeedUsers(UserManager<ApplicationUser> userManager)
        {
            if (userManager.FindByEmailAsync("admin@admin.com").Result == null)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    FullName = "Adminstrator",
                    EmailConfirmed = true
                };

                IdentityResult result = userManager.CreateAsync(user, "Admin@123").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Admin").Wait();
                }
            }
        }
    }
}
