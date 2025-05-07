using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public class BaseRepositoryWithDeletedProperty<T> : BaseRepository<T>
        where T : BaseModelWithDeletedProperty, new()
    {
        public BaseRepositoryWithDeletedProperty(Context context) : base(context)
        {
        }

        protected async Task<bool> SetIsDeletedAsync(T model)
        {
            model.IsDeleted = true;
            await UpdateAsync(model);
            return true;
        }
    }
}
