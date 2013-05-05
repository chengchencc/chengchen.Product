using SubSonic.Oracle.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BlackMamba.Framework.SubSonic.Oracle
{
    public static class DbContextExtension
    {
        public static IList<T> All<T>(this IDbContext dbContext)
            where T : EntityBase, new()
        {
            return dbContext.DbContext.All<T>().ToList();
        }

        public static IList<T> All<T>(this IDbContext dbContext, Expression<Func<T, bool>> expression) where T : EntityBase, new()
        {
            return dbContext.DbContext.Find<T>(expression);
        }

        public static T Single<T>(this IDbContext dbContext, int id) where T : EntityBase, new()
        {
            return dbContext.DbContext.Single<T>(id);
        }

        public static T Single<T>(this IDbContext dbContext, Expression<Func<T, bool>> expression) where T : EntityBase, new()
        {
            return dbContext.DbContext.Single<T>(expression);
        }

        public static object Add<T>(this IDbContext dbContext, T model) where T : EntityBase, new()
        {
            if (model != null)
            {
                model.CreatedDate = DateTime.Now;
                var modelId = Convert.ToInt32(dbContext.DbContext.NewAdd<T>(model).ToString());
                model.Id = modelId;

                return model;
            }

            return default(T);
        }

        public static object AddLog<T>(this IDbContext dbContext, T model) where T : LogBase, new()
        {
            if (model != null)
            {
                model.CreatedDate = DateTime.Now;
                model.ID = dbContext.DbContext.NewAdd<T>(model);

                return model;
            }

            return default(T);
        }

        public static void Add<T>(this IDbContext dbContext, IEnumerable<T> models) where T : EntityBase, new()
        {
            foreach (var item in models)
            {
                dbContext.Add<T>(item);
            }
        }

        public static int Update<T>(this IDbContext dbContext, T model, bool getOrigin = true) where T : EntityBase, new()
        {
            if (model != null)
            {
                if (getOrigin)
                {
                    var origin = dbContext.Single<T>(model.Id);
                    if (origin != null)
                    {
                        model.LastModifiedDate = DateTime.Now;
                        model.CreatedDate = origin.CreatedDate;
                        return dbContext.DbContext.Update<T>(model);
                    }
                }
                else
                {
                    model.LastModifiedDate = DateTime.Now;
                    return dbContext.DbContext.Update<T>(model);
                }
            }
            return -1;
        }

        public static int Update<T>(this IDbContext dbContext, IEnumerable<T> models) where T : EntityBase, new()
        {
            if (models != null && models.Any())
            {
                foreach (var model in models)
                {
                    Update<T>(dbContext, model);
                }

                return models.Count();
            }

            return -1;
        }

        public static int Delete<T>(this IDbContext dbContext, int id) where T : EntityBase, new()
        {
            return dbContext.DbContext.Delete<T>(id);
        }

        public static int Truncate<T>(this IDbContext context) where T : EntityBase, new()
        {
            return context.DbContext.DeleteMany<T>(x => x.Id > 0);
        }

        public static int Delete<T>(this IDbContext dbContext, IEnumerable<T> models) where T : EntityBase, new()
        {
            return dbContext.DbContext.DeleteMany<T>(models);
        }

        public static int Delete<T>(this IDbContext dbContext, Expression<Func<T, bool>> expression) where T : EntityBase, new()
        {
            return dbContext.DbContext.DeleteMany<T>(expression);
        }

        public static T Save<T>(this IDbContext dbContext, Expression<Func<T, bool>> expression, T model)
            where T : EntityBase, new()
        {
            var existed = dbContext.DbContext.Single<T>(expression);
            if (existed == null)
            {
                dbContext.Add<T>(model);
            }
            else
            {
                model.LastModifiedDate = DateTime.Now;
                model.Id = existed.Id;
                dbContext.DbContext.Update<T>(model);
            }

            return model;
        }

        public static bool Exists<T>(this IDbContext context, Expression<Func<T, bool>> expression)
            where T : EntityBase, new()
        {
            return context.DbContext.Exists<T>(expression);
        }


        public static IList<T> Find<T>(this IDbContext context, Expression<Func<T, bool>> expression)
            where T : EntityBase, new()
        {
            return context.DbContext.Find<T>(expression);
        }

        public static PagedList<T> GetPaged<T>(this IDbContext context, int pageIndex, int pageSize)
            where T : EntityBase, new()
        {
            return context.DbContext.GetPaged<T>(pageIndex, pageSize);
        }

        public static PagedList<T> GetPaged<T>(this IDbContext context, string sortBy, int pageIndex, int pageSize)
            where T : EntityBase, new()
        {
            return context.DbContext.GetPaged<T>(sortBy, pageIndex, pageSize);
        }
        public static PagedList<T> GetPaged<T>(this IDbContext context, int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool isDesc = false)
            where T : EntityBase, new()
        {
            return context.DbContext.GetPaged<T>(pageIndex, pageSize, orderBy, isDesc);
        }

        public static PagedList<T> GetPaged<T>(this IDbContext context, int pageIndex, int pageSize,Expression<Func<T,bool>> expression, Expression<Func<T, object>> orderBy, bool isDesc = false)
            where T : EntityBase, new()
        {
            return context.DbContext.GetPaged<T>(pageIndex, pageSize,expression,orderBy, isDesc);
        }

    }
}
