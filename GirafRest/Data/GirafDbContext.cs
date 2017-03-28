﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GirafRest.Models;

namespace GirafRest.Data
{
    public class GirafDbContext : IdentityDbContext<GirafUser>
    {
        public DbSet<Department> Departments { get; set; }
        public DbSet<Pictogram> Pictograms { get; set; }
        public DbSet<PictoFrame> PictoFrames { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Weekday> Weekdays { get; set; }
        public DbSet<Frame> Frames {get; set;}

        public DbSet<UserResource> UserResources { get; set; }

        public DbSet<DepartmentResource> DeparmentResources { get; set; }

        public GirafDbContext(DbContextOptions<GirafDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Frame>().ToTable("Frames");
            builder.Entity<Department>().ToTable("Departments");
            builder.Entity<Pictogram>().ToTable("Pictograms");
            builder.Entity<PictoFrame>().ToTable("PictoFrames");
            builder.Entity<Choice>().ToTable("Choices");
            builder.Entity<Weekday>().ToTable("Weekdays");

            //asp.net does not support many-to-many in its' current release. Here is a work around.
            //The work around is similar to the one taught in the DBS course, where a relationship called
            //DeparmentResource is used to map between departments and resources
            builder.Entity<DepartmentResource>()
                .HasOne(dr => dr.Department)
                .WithMany(d => d.Resources)
                .HasForeignKey(dr => dr.DepartmentKey);
            builder.Entity<DepartmentResource>()
                .HasOne(dr => dr.Resource)
                .WithMany(r => r.Departments)
                .HasForeignKey(dr => dr.PictoFrameKey);
            //The same goes for user and resources
            builder.Entity<UserResource>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.Resources)
                .HasForeignKey(u => u.UserId);
            builder.Entity<UserResource>()
                .HasOne(ur => ur.Resource)
                .WithMany(r => r.Users)
                .HasForeignKey(dr => dr.PictoFrameKey);

            builder.Entity<GirafUser>()
                .HasOne<Department>(u => u.Department)
                .WithMany(d => d.Members);
            
            builder.Entity<UserResource>().ToTable("UserResources");
            builder.Entity<DepartmentResource>().ToTable("DeparmentResources");
            
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
