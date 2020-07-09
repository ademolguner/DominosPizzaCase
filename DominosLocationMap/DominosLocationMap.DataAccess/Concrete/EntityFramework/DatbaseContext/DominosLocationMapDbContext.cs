using DominosLocationMap.Entities.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.DataAccess.Concrete.EntityFramework.DatbaseContext
{
   public class DominosLocationMapDbContext:DbContext
    {
        public DominosLocationMapDbContext()
        {
        }

        public DominosLocationMapDbContext(DbContextOptions<DominosLocationMapDbContext> options): base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=DominosLocationDb;Trusted_Connection=true");
        }

        public DbSet<LocationInfo> LocationInfos { get; set; } 
    }
}
