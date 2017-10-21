using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using netCoreWepApiAuthJWT.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netCoreWepApiAuthJWT.Model
{
    public class myAppplicationContext : IdentityDbContext
    {
        public myAppplicationContext(DbContextOptions<myAppplicationContext> options) : base(options)
        {

        }

    public DbSet<JobSeeker> JobSeekers { get; set; }

        /* keys of Identity tables are mapped in OnModelCreating method of IdentityDbContext and if this method 
         * is not called, you will end up getting the error: The entity type 'IdentityUserLogin<string>' requires a primary key to be defined.*/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

