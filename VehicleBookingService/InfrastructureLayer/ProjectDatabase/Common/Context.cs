using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using ProjectDatabase.Models.Country;
using ProjectDatabase.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public class Context(DbContextOptions<Context> options) : DbContext(options)
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ClientInfoModel> Clients { get; set; }
        public DbSet<RentalInfoModel> RentalInfos { get; set; }
        public DbSet<UserVerificationTokenModel> UserVerificationTokens { get; set; }
        public DbSet<CountryModel> Countries { get; set; }
        public DbSet<CountryLocationModel> CountryLocationModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Context).Assembly);
        }
    }
}
