using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace tellick_admin.Repository {
    public class GenericRepository<TEntity> where TEntity : class {
        internal TellickAdminContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(TellickAdminContext context) {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> filter = null) {
            IQueryable<TEntity> query = dbSet;
            if (filter != null) {
                query = query.Where(filter);
            }
            return query.Count();
        }

        public virtual IList<TEntity> SearchFor(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "") {
            IQueryable<TEntity> query = dbSet;
            if (filter != null) {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                query = query.Include(includeProperty);
            }

            if (orderBy != null) {
                var result = orderBy(query).ToList();
                return result;
            } else {
                var result = query.ToList();
                return result;
            }
        }

        public virtual TEntity GetByID(object id) {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity) {
            dbSet.Add(entity);
        }

        public virtual void Delete(object id) {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete) {
            if (context.Entry(entityToDelete).State == EntityState.Detached) {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate) {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void Save() {
            context.SaveChanges();
        }
    }
}