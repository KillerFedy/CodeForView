using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Entities
{
    public abstract class BaseEntity
    {
        public long Id { get; protected set; }

        public BaseEntity(long id) => Id = id;
    }
}
