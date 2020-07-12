using DominosLocationMap.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace DominosLocationMap.DataAccess.Concrete.EntityFramework.DatbaseContext
{
    public class DominosLocationMapDbContext : DbContext
    {
        public DominosLocationMapDbContext()
        {
        }

        public DominosLocationMapDbContext(DbContextOptions<DominosLocationMapDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;;Database=DominosLocationDatabase;Trusted_Connection=true");
        }

        public DbSet<DominosLocation> DominosLocation { get; set; }
    }
}