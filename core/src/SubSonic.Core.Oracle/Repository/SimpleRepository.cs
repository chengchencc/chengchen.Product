// 
//   SubSonic - http://subsonicproject.com
// 
//   The contents of this file are subject to the New BSD
//   License (the "License"); you may not use this file
//   except in compliance with the License. You may obtain a copy of
//   the License at http://www.opensource.org/licenses/bsd-license.php
//  
//   Software distributed under the License is distributed on an 
//   "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
//   implied. See the License for the specific language governing
//   rights and limitations under the License.
//  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SubSonic.Oracle.Extensions;
using SubSonic.Oracle.DataProviders;
using SubSonic.Oracle.Query;
using SubSonic.Oracle.Schema;
using SubSonic.Oracle.Linq.Structure;
using SubSonic.Oracle.SqlGeneration.Schema;

namespace SubSonic.Oracle.Repository
{
    public class SimpleRepository : IRepository
    {
        private readonly IDataProvider _provider;
        private readonly List<Type> migrated;
        private readonly SimpleRepositoryOptions _options=SimpleRepositoryOptions.Default;
        
        public SimpleRepository() : this(ProviderFactory.GetProvider(),SimpleRepositoryOptions.Default) {}

        public SimpleRepository(string connectionStringName)
            : this(connectionStringName,SimpleRepositoryOptions.Default) { }

        public SimpleRepository(string connectionStringName, SimpleRepositoryOptions options)
            : this(ProviderFactory.GetProvider(connectionStringName), options) { }


        public SimpleRepository(SimpleRepositoryOptions options) : this(ProviderFactory.GetProvider(), options) { }

        public SimpleRepository(IDataProvider provider) : this(provider, SimpleRepositoryOptions.Default) {}

        public SimpleRepository(IDataProvider provider, SimpleRepositoryOptions options)
        {
            _provider = provider;
            _options = options;
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                migrated = new List<Type>();
        }


        #region IRepository Members


        public bool Exists<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            //return All<T>().Any(expression);
            var tbl = _provider.FindOrCreateTable<T>();

            var qry = new Select(_provider).From(tbl);
            var constraints = expression.ParseConstraints().ToList();
            #region Support SubSonicColumnNameOverrideAttribute
            foreach (Constraint ct in constraints)
            {
                PropertyInfo pi = typeof(T).GetProperties().SingleOrDefault(p => p.Name == ct.ColumnName);
                if (pi != null)
                {
                    var columnAttributes = pi.GetCustomAttributes(typeof(SubSonicColumnNameOverrideAttribute), false);
                    if (columnAttributes.Length > 0)
                    {
                        ct.ConstructionFragment = (columnAttributes[0] as SubSonicColumnNameOverrideAttribute).ColumnName;
                    }
                }

            }
            #endregion

            qry.Constraints = constraints;
            var list = qry.ToList<T>();
            if (list.Count > 0)
                return true;
            return false;
        }

        public IQueryable<T> All<T>() where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            var qry = new Query<T>(_provider);
            return qry;
        }

        public IList<T> GetAll<T>() where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            var tbl = _provider.FindOrCreateTable<T>();
            var qry = new Select(_provider).From(tbl);
            return qry.ToList<T>();
        }

        /// <summary>
        /// Singles the specified expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public T Single<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            T result = default(T);
            var tbl = _provider.FindOrCreateTable<T>();

            var qry = new Select(_provider).From(tbl);
            var constraints = expression.ParseConstraints().ToList();
            #region Support SubSonicColumnNameOverrideAttribute
            var fields = Objects.ToColumnDictionary<T>();  
            foreach (Constraint ct in constraints)
            {
                PropertyInfo pi = typeof(T).GetProperties().SingleOrDefault(p => p.Name == ct.ColumnName);
                if (pi != null)
                {
                    ct.ConstructionFragment = fields[pi.Name];
                }
                
            }
            #endregion
            
            qry.Constraints = constraints;
            
            var list = qry.ToList<T>();
            if(list.Count > 0)
                result = list[0];
            return result;
        }

        /// <summary>
        /// Singles the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T Single<T>(object key) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();

            var result = new Select(_provider).From(tbl).Where(tbl.PrimaryKey).IsEqualTo(key).ExecuteSingle<T>();

            return result;
        }

        /// <summary>
        /// Retrieves subset of records from the database matching the expression
        /// </summary>
        public IList<T> Find<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();

            var qry = new Select(_provider).From(tbl);
            var constraints = expression.ParseConstraints().ToList();
            #region Support SubSonicColumnNameOverrideAttribute
            foreach (Constraint ct in constraints)
            {
                PropertyInfo pi = typeof(T).GetProperties().SingleOrDefault(p => p.Name == ct.ColumnName);
                if (pi != null)
                {
                    var columnAttributes = pi.GetCustomAttributes(typeof(SubSonicColumnNameOverrideAttribute), false);
                    if (columnAttributes.Length > 0)
                    {
                        ct.ConstructionFragment = (columnAttributes[0] as SubSonicColumnNameOverrideAttribute).ColumnName;
                    }
                }

            }
            #endregion
            qry.Constraints = constraints;
            return qry.ExecuteTypedList<T>();
        }

        /// <summary>
        /// Gets the paged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public PagedList<T> GetPaged<T>(int pageIndex, int pageSize) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            var qry = new Select(_provider).From(tbl).Paged(pageIndex + 1, pageSize).OrderAsc(tbl.PrimaryKey.Name);
            var total =
                new Select(_provider, new Aggregate(tbl.PrimaryKey, AggregateFunction.Count)).From<T>().ExecuteScalar();
            int totalRecords = 0;
            int.TryParse(total.ToString(), out totalRecords);
            return new PagedList<T>(qry.ToList<T>(), totalRecords, pageIndex, pageSize);
        }

        public PagedList<T> GetPaged<T>(int pageIndex, int pageSize, Expression<Func<T, bool>> expression) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            var qry = new Select(_provider).From(tbl).Where<T>(expression).Paged(pageIndex + 1, pageSize).OrderAsc(tbl.PrimaryKey.Name);
            var total =
                new Select(_provider, new Aggregate(tbl.PrimaryKey, AggregateFunction.Count)).From<T>().Where<T>(expression).ExecuteScalar();
            int totalRecords = 0;
            int.TryParse(total.ToString(), out totalRecords);
            return new PagedList<T>(qry.ToList<T>(), totalRecords, pageIndex, pageSize);
        }
        public PagedList<T> GetPaged<T>(int pageIndex, int pageSize, Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool isDesc) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            SqlQuery qry = null;
            #region ColumnNameOverride
            var fields = Objects.ToColumnDictionary<T>();
            string orderField = this.GetFieldName<T>(orderBy);
            #endregion
            if (!isDesc)
                qry = new Select(_provider).From(tbl).Where<T>(expression).Paged(pageIndex + 1, pageSize).OrderAsc(fields[orderField]);
            else
                qry = new Select(_provider).From(tbl).Where<T>(expression).Paged(pageIndex + 1, pageSize).OrderDesc(fields[orderField]);
            var total =
                new Select(_provider, new Aggregate(tbl.PrimaryKey, AggregateFunction.Count)).From<T>().Where<T>(expression).ExecuteScalar();
            int totalRecords = 0;
            int.TryParse(total.ToString(), out totalRecords);
            return new PagedList<T>(qry.ToList<T>(), totalRecords, pageIndex, pageSize);
        }
        
        public PagedList<T> GetPaged<T>(int pageIndex, int pageSize, Expression<Func<T, object>> orderBy, bool isDesc) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            SqlQuery qry = null;
            #region ColumnNameOverride
            var fields = Objects.ToColumnDictionary<T>();

            string orderField = this.GetFieldName<T>(orderBy);
            #endregion
            if (!isDesc)
                qry = new Select(_provider).From(tbl).Paged(pageIndex + 1, pageSize).OrderAsc(fields[orderField]);
            else
                qry = new Select(_provider).From(tbl).Paged(pageIndex + 1, pageSize).OrderDesc(fields[orderField]);
            var total =
                new Select(_provider, new Aggregate(tbl.PrimaryKey, AggregateFunction.Count)).From<T>().ExecuteScalar();
            int totalRecords = 0;
            int.TryParse(total.ToString(), out totalRecords);
            return new PagedList<T>(qry.ToList<T>(), totalRecords, pageIndex, pageSize);
        }

        private string GetPropertyName<TModel>(Expression<Func<TModel, PropertyInfo>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.Call)
            {
                var methodCallExpression = (MethodCallExpression)expression.Body;
                string name = this.GetPropertyName(methodCallExpression);
                return name.Substring(expression.Parameters[0].Name.Length + 1);

            }
            return expression.Body.ToString().Substring(expression.Parameters[0].Name.Length + 1);
        }

        private string GetPropertyName(MethodCallExpression expression)
        {
            // p => p.Foo.Bar().Baz.ToString() => p.Foo OR throw...

            var methodCallExpression = expression.Object as MethodCallExpression;
            if (methodCallExpression != null)
            {
                return GetPropertyName(methodCallExpression);
            }
            return expression.Object.ToString();
        }

        #endregion
        /// <summary>
        /// Gets the paged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public PagedList<T> GetPaged<T>(string sortBy, int pageIndex, int pageSize) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();

            var qry = new Select(_provider).From(tbl).Paged(pageIndex + 1, pageSize);

            if (!sortBy.EndsWith(" desc", StringComparison.InvariantCultureIgnoreCase))
                qry.OrderAsc(sortBy);
            else
                qry.OrderDesc(sortBy.FastReplace(" desc", ""));

            var total =
                new Select(_provider, new Aggregate(tbl.PrimaryKey, AggregateFunction.Count)).From<T>().ExecuteScalar();
            int totalRecords = 0;
            int.TryParse(total.ToString(), out totalRecords);
            return new PagedList<T>(qry.ToList<T>(), totalRecords, pageIndex, pageSize);
        }

        /// <summary>
        /// Adds the specified item, setting the key if available.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int Add<T>(T item) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            int result;
            using(var rdr = item.ToInsertQuery(_provider).ExecuteReader())
            {
                result = rdr.RecordsAffected;
            }

            return result;
        }

        public long NewAdd<T>(T item) where T : class, new()
        {
            
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            var cmd = new StoredProcedure(tbl.Name.ToUpper() +"_PROC_INS", _provider);
            var hashed = item.ToDictionary();
            int pkIndex=0;

            for (int i=0;i<tbl.Columns.Count;i++)
            {
                var col = tbl.Columns[i];
                if (col.AutoIncrement)
                {
                    cmd.Command.AddOutputParameter("P_" + col.Name.ToUpper(), System.Data.DbType.Int64);
                    pkIndex = i;
                }

                else
                    cmd.Command.AddParameter("P_" + col.Name.ToUpper(), hashed[col.Name], col.DataType);
            }

            cmd.Execute();

            long? returnId = cmd.Command.Parameters[pkIndex].ParameterValue as long?;
            return returnId.GetValueOrDefault();
        }

        

        /// <summary>
        /// Adds a lot of the items using a transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public void AddMany<T>(IEnumerable<T> items) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            BatchQuery batch = new BatchQuery(_provider);
            foreach(var item in items)
                batch.QueueForTransaction(item.ToInsertQuery(_provider));
            batch.ExecuteTransaction();
        }

        /// <summary>
        /// Updates the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int Update<T>(T item) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            return item.ToUpdateQuery(_provider).Execute();
        }

        /// <summary>
        /// Updates lots of items using a transaction.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public int UpdateMany<T>(IEnumerable<T> items) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            BatchQuery batch = new BatchQuery(_provider);
            int result = 0;
            foreach(var item in items)
            {
                batch.QueueForTransaction(item.ToUpdateQuery(_provider));
                result++;
            }
            batch.ExecuteTransaction();
            return result;
        }

        /// <summary>
        /// Deletes the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public int Delete<T>(object key) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            var tbl = _provider.FindOrCreateTable<T>();
            return new Delete<T>(_provider).From<T>().Where(tbl.PrimaryKey).IsEqualTo(key).Execute();
        }

        /// <summary>
        /// Deletes 1 or more items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public int DeleteMany<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            var tbl = _provider.FindOrCreateTable<T>();
            var qry = new Delete<T>(_provider).From<T>();

            var constraints = expression.ParseConstraints().ToList();
            qry.Constraints = constraints;

            return qry.Execute();
        }

        /// <summary>
        /// Deletes 1 or more items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public int DeleteMany<T>(IEnumerable<T> items) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();

            BatchQuery batch = new BatchQuery(_provider);
            int result = 0;
            foreach(var item in items)
            {
                batch.QueueForTransaction(item.ToDeleteQuery(_provider));
                result++;
            }
            batch.ExecuteTransaction();
            return result;
        }

       


        /// <summary>
        /// Migrates this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void Migrate<T>() where T : class, new()
        {
            Type type = typeof(T);
            if(!migrated.Contains(type))
            {
                BatchQuery batch = new BatchQuery(_provider);
                Migrator m = new Migrator(Assembly.GetExecutingAssembly());
                var commands = m.MigrateFromModel(type, _provider);
                foreach(var s in commands)
                    batch.QueueForTransaction(new QueryCommand(s, _provider));
                batch.ExecuteTransaction();
                
                migrated.Add(type);
            }
        }





        public T First<T>(Expression<Func<T, bool>> expression, Expression<Func<T, object>> orderBy, bool isDesc) where T : class, new()
        {
            if (_options.Contains(SimpleRepositoryOptions.RunMigrations))
                Migrate<T>();
            var tbl = _provider.FindOrCreateTable<T>();
            SqlQuery qry = null;
            #region ColumnNameOverride
            T t = default(T);
            var fields = Objects.ToColumnDictionary<T>();
            string orderField = this.GetFieldName<T>(orderBy);
             
            #endregion
            if (!isDesc)
                qry = new Select(_provider).From(tbl).Where<T>(expression).Paged(1, 1).OrderAsc(fields[orderField]);
            else
                qry = new Select(_provider).From(tbl).Where<T>(expression).Paged(1, 1).OrderDesc(fields[orderField]);

            var list = qry.ToList<T>();

            return list.Count > 0 ? list[0] : t;
        }

        private string GetFieldName<T>(Expression<Func<T, object>> orderBy)
        {
            string fieldName = string.Empty;
            if (orderBy.Body is BinaryExpression)
            {
                var binaryExpr = (BinaryExpression)orderBy.Body;
                var memberExp = (MemberExpression)binaryExpr.Left;
                fieldName = memberExp.Member.Name;
            }
            else
            {
                var binaryExpr = (UnaryExpression)orderBy.Body;
                var memberExp = (MemberExpression)binaryExpr.Operand;
                fieldName = memberExp.Member.Name;
            }

            return fieldName;
        }
        
    }
}