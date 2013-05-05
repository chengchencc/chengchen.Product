using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Schema;
using System.Data.Common;

using SubSonic.Oracle.Linq.Structure;
using SubSonic.Oracle.Query;

namespace SubSonic.Oracle.DataProviders.SqlServer
{
    public class SqlServerProvider : DbDataProvider, IDataProvider
    {
        private string _insertionIdentityFetchString = "; SELECT SCOPE_IDENTITY() as new_id";
        public override string InsertionIdentityFetchString { get { return _insertionIdentityFetchString; } }

        public SqlServerProvider(string connectionString, string providerName) : base(connectionString, providerName)
        {}

        public override string QualifyTableName(ITable table)
        {
            string qualifiedTable;


            string qualifiedFormat = String.IsNullOrEmpty(table.SchemaName) ? "[{1}]" : "[{0}].[{1}]";
            qualifiedTable = String.Format(qualifiedFormat, table.SchemaName, table.Name);


            return qualifiedTable;
        }

        public override string QualifyColumnName(IColumn column)
        {
            string qualifiedFormat;
            qualifiedFormat = String.IsNullOrEmpty(column.SchemaName) ? "[{1}].[{2}]" : "[{0}].[{1}].[{2}]";
            return String.Format(qualifiedFormat, column.Table.SchemaName, column.Table.Name, column.Name);
        }

        public override ISchemaGenerator SchemaGenerator
        {
            get { return new Sql2005Schema(); }
        }

        public override ISqlGenerator GetSqlGenerator(SqlQuery query)
        {
            return new Sql2005Generator(query);
        }

        public override IQueryLanguage QueryLanguage { get { return new TSqlLanguage(this); } }

        #region Shared connection and transaction handling

        [ThreadStatic]
        private static DbConnection __sharedConnection;
        [ThreadStatic]
        private static DbTransaction __sharedTransaction;

        /// <summary>
        /// Gets or sets the current shared connection.
        /// </summary>
        /// <value>The current shared connection.</value>
        public override DbConnection CurrentSharedConnection
        {
            get { return __sharedConnection; }

            protected set
            {
                if (value == null)
                {
                    __sharedConnection.Dispose();
                    __sharedConnection = null;
                }
                else
                {
                    __sharedConnection = value;
                    __sharedConnection.Disposed += __sharedConnection_Disposed;
                }
            }
        }

        public override DbTransaction CurrentSharedTransaction
        {
            get { return __sharedTransaction; }

            set
            {
                if (__sharedTransaction != null)
                {
                    try
                    {
                        __sharedTransaction.Dispose();
                    }
                    catch
                    {
                        // ignore errors.
                    }
                }
                __sharedTransaction = value;
            }
        }

        private static void __sharedConnection_Disposed(object sender, EventArgs e)
        {
            __sharedConnection = null;
        }

        #endregion
    }
}
