using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class AppDbContext : DbContext
    {
        // pass the parameter (the options) that will enter the constructor function to the base constructor
        // of the parent class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<SOSRequest> SOSRequests { get; set; }
    }
}
