using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectDatabase.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Configurations
{
    public class UserVerificationTokenConfiguration : IEntityTypeConfiguration<UserVerificationTokenModel>
    {
        public void Configure(EntityTypeBuilder<UserVerificationTokenModel> builder)
        {
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(e => e.ExpireDateUtc)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
        }
    }
}
