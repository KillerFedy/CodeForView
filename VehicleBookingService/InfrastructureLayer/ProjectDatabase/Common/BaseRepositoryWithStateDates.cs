using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public abstract class BaseRepositoryWithStateDates<T>(Context context) : 
        BaseRepositoryWithDeletedProperty<T>(context) where T : BaseModelWithStateDates, new()
    {
        protected async override Task<T> InsertAsync(T entity)
        {
            entity.CreatedDateUtc = DateTime.UtcNow;
            entity.UpdatedDateUtc = entity.CreatedDateUtc;
            return await base.InsertAsync(entity);
        }

        protected async override Task<T> UpdateAsync(T entity)
        {
            entity.UpdatedDateUtc = DateTime.UtcNow;
            return await base.UpdateAsync(entity);
        }
    }
}