using DominosLocationMap.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DominosLocationMap.Core.DataAccess
{
    public interface IEntityRepositoryBase<T> where T : class, IEntity, new()
    {
        T Get(Expression<Func<T, bool>> filter);

        IList<T> GetList(Expression<Func<T, bool>> filter = null);

        T Add(T entity);

        Task<T> AddAsync(T entity);

        T Update(T entity);

        void Delete(T entity);
    }
}