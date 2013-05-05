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
using System.Data;
using SubSonic.Oracle.DataProviders;
using SubSonic.Oracle.Schema;


namespace SubSonic.Oracle.DataProviders
{
    public interface ISchemaGenerator
    {
        /// <summary>
        /// Builds a CREATE TABLE statement.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        string BuildCreateTableStatement(ITable table);

        /// <summary>
        /// Builds a DROP TABLE statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        //string BuildDropTableStatement(string tableName);

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="column">The column.</param>
        string BuildAddColumnStatement(string tableName, IColumn column);

        /// <summary>
        /// Alters the column.
        /// </summary>
        /// <param name="column">The column.</param>
        string BuildAlterColumnStatement(IColumn column);

        /// <summary>
        /// Builds a DROP TABLE statement.
        /// </summary>
        /// <param name="table">Name of the table.</param>
        /// <returns></returns>
        string BuildDropTableStatement(ITable table);

        /// <summary>
        /// Removes the column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        string BuildDropColumnStatement(string tableName, string columnName);

        /// <summary>
        /// Adds the Index.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="column">Name of the column.</param>
        /// <returns></returns>
        string BuildAddIndexStatement(IColumn column);

        /// <summary>
        /// Alters the Index.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        string BuildAlterIndexStatement(IColumn column);

        /// <summary>
        /// Builds a DROP Index statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        string BuildDropIndexStatement(IColumn column);

        /// <summary>
        /// Gets the type of the native.
        /// </summary>
        /// <param name="dbType">Type of the db.</param>
        /// <returns></returns>
        string GetNativeType(DbType dbType);

        /// <summary>
        /// Generates the columns.
        /// </summary>
        /// <param name="table">Table containing the columns.</param>
        /// <returns>
        /// SQL fragment representing the supplied columns.
        /// </returns>
        string GenerateColumns(ITable table);

        /// <summary>
        /// Sets the column attributes.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        string GenerateColumnAttributes(IColumn column);

        ITable GetTableFromDB(IDataProvider provider, string tableName);
        string[] GetTableList(IDataProvider provider);
        DbType GetDbType(string sqlType);

        /// <summary>
        /// This method is a place to convert data types before they are added to a DbParameter's Value.
        /// This can be used to convert object to a different data type incase a DB provider doesn't natively
        /// support that type. For example, Oracle doesn't support 'bool' so it needs to be converted to a
        /// string or number instead.
        /// </summary>
        /// <param name="input">The original data.</param>
        /// <returns>The object to set the parameter's value to.</returns>
        object ConvertDataValueForThisProvider(object input);

        DbType ConvertDataTypeToDbType(DbType dataType);

        string ClientName { get; set; }

        string GenerateInsertProcedure(ITable table);
    }
}