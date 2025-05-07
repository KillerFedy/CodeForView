using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProjectDatabase.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Configurations.Common
{
    public abstract class BaseModelWithStateDatesConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseModelWithStateDates
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Настройка свойств CreatedDateUtc и UpdatedDateUtc
            builder.Property(e => e.CreatedDateUtc)
                   .IsRequired()
                   .HasColumnType("timestamp without time zone"); // Без временной зоны

            builder.Property(e => e.UpdatedDateUtc)
                   .IsRequired()
                   .HasColumnType("timestamp without time zone"); // Без временной зоны
        }
    }
}
