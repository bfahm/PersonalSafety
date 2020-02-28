﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalSafety.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // pass the parameter (the options) that will enter the constructor function to the base constructor
        // of the parent class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Call base class original function that we're overriding to prevent errors
            //-----------------------------------
            //Make the following columns unique in the table [User]
            builder.Entity<ApplicationUser>()
                   .HasIndex(u => u.NationalId)
                   .IsUnique();

            builder.Entity<ApplicationUser>()
                   .HasIndex(u => u.PhoneNumber)
                   .IsUnique();
        }

        public DbSet<ApplicationUser> UserInfos { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<SOSRequest> SOSRequests { get; set; }
    }
}
