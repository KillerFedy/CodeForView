using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectDatabase.Configurations.Common;
using ProjectDatabase.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Configurations
{
    public class ClientInfoConfiguration : IEntityTypeConfiguration<ClientInfoModel>
    {
        public void Configure(EntityTypeBuilder<ClientInfoModel> builder)
        {

            builder.Property(e => e.BirthDate)
                .HasColumnType("timestamp without time zone");
        }
    }
}
