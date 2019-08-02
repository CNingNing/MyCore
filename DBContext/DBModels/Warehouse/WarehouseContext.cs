using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace DBModels.Warehouse
{
    public partial class WarehouseContext:DbContext
    {
        public WarehouseContext():base()
        {

        }
        public  WarehouseContext(DbContextOptions<WarehouseContext>options):base(options)
        {

        }
        public virtual DbSet<Order> Order { get; set; }
      

        protected override  void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                var dbcontext = DbConnnectString.GetDatabase("Warehouse");
                optionsBuilder.UseSqlServer(dbcontext);
            }
        }
    
        
        protected override void  OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");
            //指定表的名称
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("t_warehouse_order");
            });
      
        }
    }
}
