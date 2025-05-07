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
    public class UserConfiguration : BaseModelWithStateDatesConfiguration<UserModel>
    {
        public override void Configure(EntityTypeBuilder<UserModel> builder)
        {
            base.Configure(builder);

            builder.HasOne(x => x.RentalInfo).WithOne(x => x.User)
                .HasForeignKey<RentalInfoModel>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ClientInfo).WithOne(x => x.User)
                .HasForeignKey<ClientInfoModel>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
