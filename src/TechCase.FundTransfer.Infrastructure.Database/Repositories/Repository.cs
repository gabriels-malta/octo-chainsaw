using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.FundTransfer.Infrastructure.Database
{
    internal class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly FundTransferContext _db;

        public Repository(FundTransferContext context) => _db = context;

        public bool Any(Expression<Func<TEntity, bool>> expression) => _db.Set<TEntity>().Any(expression);

        public IEnumerable<TEntity> GetBy(Expression<Func<TEntity, bool>> expression)
        {
            return _db.Set<TEntity>().Where(expression).AsNoTracking().ToList();
        }

        public TEntity GetFirstBy(Expression<Func<TEntity, bool>> expression) => _db.Set<TEntity>().FirstOrDefault(expression);

        public TEntity Insert(TEntity entity)
        {
            _db.Set<TEntity>().Add(entity);
            _db.SaveChanges();
            return entity;
        }

        public void Update(TEntity entity)
        {
            _db.Entry(entity).State = EntityState.Modified;
            _db.Set<TEntity>().Update(entity);
            _db.SaveChanges();
        }
    }
}
