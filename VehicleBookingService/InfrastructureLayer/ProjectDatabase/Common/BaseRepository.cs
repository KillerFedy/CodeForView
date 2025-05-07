using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDatabase.Common
{
    public abstract class BaseRepository<T>(Context context) where T : BaseModel, new()
    {
        protected async Task<List<T>> GetListByQueryAsync(Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
            bool asNoTracking = false)
        {
            IQueryable<T> query = context.Set<T>();
            if (asNoTracking) query = query.AsNoTracking();
            if (predicate != null) query = query.Where(predicate);
            query = orderBy?.Invoke(query) ?? query;
            query = include?.Invoke(query) ?? query;
            return await query.ToListAsync();
        }

        protected async Task<T?> GetItemByQueryAsync(Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object?>>? include = null,
            bool asNoTracking = false)
        {
            IQueryable<T> query = context.Set<T>();
            if (asNoTracking) query = query.AsNoTracking();
            if (predicate != null) query = query.Where(predicate);
            query = orderBy?.Invoke(query) ?? query;
            query = include?.Invoke(query) ?? query;
            return await query.FirstOrDefaultAsync();
        }

        protected async Task<List<T>> GetAllAsync()
        {
            var result = context.Set<T>();
            return await result.AsNoTracking().ToListAsync();
        }

        protected async Task<T?> GetByIdAsync(long id, bool asNoTracking = false)
        {
            IQueryable<T> query = context.Set<T>();
            query = asNoTracking ? query.AsNoTracking() : query;
            return await query.SingleOrDefaultAsync(x => x.Id == id);
        }

        protected virtual async Task<T> InsertAsync(T model)
        {
            var set = context.Set<T>();
            set.Add(model);
            await context.SaveChangesAsync();
            return model;
        }

        protected async Task DeleteItemAsync(long id)
        {
            var objectToDelete = new T { Id = id };
            context.Set<T>().Remove(objectToDelete);
            await context.SaveChangesAsync();
        }

        protected virtual async Task<T> UpdateAsync(T model)
        {
            if (model.Id == 0) return await InsertAsync(model);
            context.Entry(model).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return model;
        }
    }
}
