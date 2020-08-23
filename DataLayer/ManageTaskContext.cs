using Core.Abstractions;
using Domain.DataModels;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataLayer
{
    public class ManageTaskContext : DbContext
    {

        public ManageTaskContext(DbContextOptions<ManageTaskContext> options):base(options)
        {

        }

        public DbSet<Task> manageTasks { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<Tasks>(entity => {
        //        entity.HasKey(k => k.Id);
        //        entity.ToTable("Task");
        //    });
        //}
    }
}