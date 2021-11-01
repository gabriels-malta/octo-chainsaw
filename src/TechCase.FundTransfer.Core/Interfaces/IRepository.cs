using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TechCase.FundTransfer.Core.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Insert(TEntity entity);
        void Update(TEntity entity);
        IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> expression);
        TEntity GetFirstBy(Expression<Func<TEntity, bool>> expression);
        bool Any(Expression<Func<TEntity, bool>> expression);
    }
}
