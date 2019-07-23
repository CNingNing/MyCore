using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DBModels.Hr
{
    public partial class HrContext:DbContext
    {
        public HrContext()
        {

        }
        public  HrContext(DbContextOptions<HrContext>options):base(options)
        {

        }
        public virtual DbSet<User>User { get; set; }
        protected override  void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var dbcontext = DbConnnectString.GetDatabase("Hr");
                optionsBuilder.UseSqlServer(dbcontext);
            }
        }

        protected override void  OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("t_hr_user0");
            });
        }
    }
}
