using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace DBModels.Hr
{
    public partial class HrContext:DbContext
    {
        public HrContext():base()
        {

        }
        public  HrContext(DbContextOptions<HrContext>options):base(options)
        {

        }
        public virtual DbSet<User>User { get; set; }
        public virtual DbSet<Authority> Authority { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<UserRole> UserRole { get; set; }

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
            //指定表的名称
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("t_hr_user");
            });
            modelBuilder.Entity<Authority>(entity =>
            {
                entity.ToTable("t_hr_authority");
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("t_hr_role");
            });
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("t_hr_userrole");
            });
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("t_hr_menu");
            });
        }
    }
}
