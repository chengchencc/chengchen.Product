using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Schema;
using System.Data.Common;
using SubSonic.Oracle.Linq.Structure;
using SubSonic.Oracle.Query;


namespace SubSonic.Oracle.DataProviders.SQLite
{

    public class SQLiteProvider : DbDataProvider, IDataProvider
    {
        public override string InsertionIdentityFetchString { get { return String.Empty; } }

        public SQLiteProvider(string connectionString, string providerName) : base(connectionString, providerName)
        {}

        public override string QualifyTableName(ITable table)
        {
            string qualifiedTable;

            qualifiedTable = qualifiedTable = String.Format("`{0}`", table.Name);


            return qualifiedTable;
        }

        public override string QualifyColumnName(IColumn column)
        {
            string qualifiedFormat;
            qualifiedFormat = "`{2}`";
            return String.Format(qualifiedFormat, column.Table.SchemaName, column.Table.Name, column.Name);
        }

        public override ISchemaGenerator SchemaGenerator
        {
            get { return new SQLiteSchema(); }
        }

        public override ISqlGenerator GetSqlGenerator(SqlQuery query)
        {
            return new SQLiteGenerator(query);
        }

        public override IQueryLanguage QueryLanguage { get { return new SQLiteLanguage(this); } }

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
