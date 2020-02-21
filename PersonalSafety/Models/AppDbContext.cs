using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

            //-----------------------------------
            //Seed the database with initial data
            builder.Entity<ApplicationUser>()
                   .HasData(
                        new ApplicationUser
                        {
                            //Id = 1,
                            FullName = "Test User",
                            NationalId = "29700000000",
                            PhoneNumber = "01010101010",
                            Birthday = Convert.ToDateTime("1/1/2020"),
                            Email = "user@user.com",
                            BloodType = (int)BloodTypesEnum.A,
                            MedicalHistoryNotes = "none...",
                            CurrentAddress = "unknown...",
                            CurrentOngoingEvent = 0, // doesn't currently need help
                            CurrentInvolvement = 0 // isn't currently involved in any events
                        }
                    );
        }

        public DbSet<ApplicationUser> UserInfos { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<SOSRequest> SOSRequests { get; set; }
    }
}
