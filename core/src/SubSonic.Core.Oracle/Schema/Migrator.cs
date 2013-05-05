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
using System.Reflection;
using SubSonic.Oracle.Extensions;
using SubSonic.Oracle.DataProviders;
using System.Data;
using SubSonic.Oracle.SqlGeneration.Schema;
using System.Text;

namespace SubSonic.Oracle.Schema
{
    public class Migrator
    {
        private readonly Assembly _modelAssembly;

        public Migrator(Assembly modelAssembly)
        {
            _modelAssembly = modelAssembly;
        }

        ///<summary>
        /// Creates a set of SQL commands for synchronizing your database with your object set
        ///</summary>
        public string[] CreateColumnMigrationSql(ITable source)
        {
            var result = new List<string>();
            var existing = source.Provider.GetTableFromDB(source.Name);
            //remove columns not found
            foreach(var c in existing.Columns)
            {
                var colFound = source.GetColumn(c.Name);
                if(colFound == null)
                {
                    //remove it 
                    result.Add(source.Provider.SchemaGenerator.BuildDropIndexStatement(c));
                    result.Add(source.DropColumnSql(c.Name));
                }
            }
            //loop the existing table and add columns not found, update columns found...
            foreach(var col in source.Columns)
            {
                var colFound = existing.GetColumn(col.Name);
                if(colFound == null)
                {
                    //add it
                    string addSql = col.CreateSql;
                    //when adding a column, and UPDATE it appended
                    //need to split that out into its own command
                    var sqlCommands = addSql.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in sqlCommands)
                    {
                        result.Add(s);
                        if (col.IsIndexed)
                            result.Add(source.Provider.SchemaGenerator.BuildAddIndexStatement(col));
                    }
                }
                else
                {
                    if (IsColumnChanged(colFound, col) & !col.IsPrimaryKey)
                    {
                        if (!String.IsNullOrEmpty(col.AlterSql))
                        {
                            string alterStatement = col.AlterSql;
                            alterStatement = alterStatement.Replace("NOT NULL", string.Empty);
                            if (!colFound.IsNullable && col.IsNullable)
                            {
                                alterStatement += " NULL";
                            }
                            else if (colFound.IsNullable && !col.IsNullable)
                            {
                                alterStatement += " NOT NULL";
                            }
                            
                            result.Add(alterStatement);
                        }
                        
                    }

                    result.Add(source.Provider.SchemaGenerator.BuildAlterIndexStatement(col));
                }
            }

            return result.ToArray();
        }

        private bool IsColumnChanged(IColumn existedCol, IColumn currentCol)
        {
            bool stringMaxLengthCheck = true;
            if (existedCol.IsString)
            {
                stringMaxLengthCheck = existedCol.MaxLength.Equals(currentCol.MaxLength);
            }

            

             bool columnMatch = existedCol.DataType.Equals(currentCol.DataType)
                && existedCol.Name.ToUpper().Equals(currentCol.Name.ToUpper())
                //&& existedCol.NumberScale.Equals(currentCol.NumberScale)
                //&& existedCol.NumericPrecision.Equals(currentCol.NumericPrecision)
                && stringMaxLengthCheck
                && existedCol.IsDateTime.Equals(currentCol.IsDateTime)
                && existedCol.IsForeignKey.Equals(currentCol.IsForeignKey)
                && existedCol.IsNullable.Equals(currentCol.IsNullable)
                && existedCol.IsNumeric.Equals(currentCol.IsNumeric)
                && existedCol.IsReadOnly.Equals(currentCol.IsReadOnly)
                && existedCol.IsString.Equals(currentCol.IsString)
                && existedCol.QualifiedName.ToUpper().Equals(currentCol.QualifiedName.ToUpper());

            // we need check precision for decimal data
             if (columnMatch && existedCol.DataType == DbType.Decimal)
             {
                 columnMatch &= existedCol.NumberScale.Equals(currentCol.NumberScale);
                 columnMatch &= existedCol.NumericPrecision.Equals(currentCol.NumericPrecision);
             }

             return !columnMatch;

        }

        public string[] MigrateFromModel<T>(IDataProvider provider)
        {
            return MigrateFromModel(typeof(T), provider);
        }
        private string AlterTableSpace(Type type, string tableName)
        {

            var typeAttributes = type.GetCustomAttributes(typeof(IClassMappingAttribute), false);

            foreach (IClassMappingAttribute attr in typeAttributes)
            {
                MethodInfo tableSpace = attr.GetType().GetMethod("GetTableSpace");
                if (tableSpace != null)
                {
                    string sql = "Alter table " + tableName + " move  tablespace " + tableSpace.Invoke(attr, null).ToString();
                    return String.Format("DECLARE BEGIN EXECUTE IMMEDIATE '\r\n {0}' \r\n ;END; ", sql);
                }
            }

            return string.Empty;
        }
        public string[] MigrateFromModel(Type type, IDataProvider provider)
        {
            var result = new List<string>();

            var table = type.ToSchemaTable(provider);
            var existing = provider.GetTableFromDB(table.Name);

            if (existing != null)
            {
                //if the tables exist, reconcile the columns
                result.AddRange(CreateColumnMigrationSql(table));
                //columns changed
                if (result.Count > 0)
                {
                    result.Add(table.InsertProcedureSql);
                }
            }
            else
            {
                //create tables for them
                result.Add(table.CreateSql);
                //create table Index
                foreach (IColumn col in table.Columns.Where(c => c.IsIndexed == true))
                {
                    result.Add(table.Provider.SchemaGenerator.BuildAddIndexStatement(col));
                }
                

            }


            string isAlterTableSpace = AlterTableSpace(type, table.Name);
            if (!string.IsNullOrEmpty(isAlterTableSpace))
            {
                result.Add(isAlterTableSpace);
            }
            foreach (var r in result)
            {
                Console.WriteLine(r);
            }

            return result.ToArray();
        }

        public string[] GenUpdateTriggerFromModel(Type type, IDataProvider provider)
        {
            var result = new List<string>();

            var table = type.ToSchemaTable(provider);

            if (table.HasUpdateTrigger)
            {
                result.AddRange(GenUpdateTriggerSql(table));
            }
            
            return result.ToArray();
        }

        private List<string> GenUpdateTriggerSql(ITable table)
        {

            var sqlList = new List<string>();
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(string.Format("CREATE OR REPLACE package {0}_pkg", table.Name));
            sql.AppendLine(" as ");
            sql.AppendLine("   type ridArray is table of rowid index by binary_integer;  ");
            sql.AppendLine(" newRows ridArray;  ");
            sql.AppendLine(" empty   ridArray;  ");
            sql.AppendLine("  end;  ");
            sqlList.Add(sql.ToString());

            sql = new StringBuilder();
            sql.AppendLine(string.Format(" CREATE OR REPLACE TRIGGER {0}_init ", table.Name));
            sql.AppendLine(string.Format(" before insert  on {0} ", table.Name));
            sql.AppendLine(" BEGIN ");
            sql.AppendLine(string.Format(" {0}_pkg.newRows := {1}_pkg.empty;", table.Name, table.Name));
            sql.AppendLine(" END; ");
            sqlList.Add(sql.ToString());


            sql = new StringBuilder();
            sql.AppendLine(string.Format("CREATE OR REPLACE TRIGGER {0}_do ", table.Name));
            sql.AppendLine(" AFTER INSERT ");
            sql.AppendLine(string.Format(" ON {0} ", table.Name));
            sql.AppendLine(" REFERENCING NEW AS NEW OLD AS OLD ");
            sql.AppendLine(" FOR EACH ROW ");
            sql.AppendLine(" DECLARE ");
            sql.AppendLine(" BEGIN ");
            sql.AppendLine(string.Format(" update {0} ", table.UpdateTableName));
            sql.AppendLine(" set ");
            int i = 0;
            var updateColumns = table.Columns.ToList().FindAll(c=>c.IsForUpdateTrigger == true);
            foreach (var col in updateColumns)
            {
                sql.AppendLine(string.Format(" {0}=:new.{1}{2}", col.Name, col.Name, (i == updateColumns.Count - 1) ? string.Empty : ","));
                i++;
            }
            sql.AppendLine(" where Id=:new.Id;");
            sql.AppendLine(string.Format("  {0}_pkg.newRows( {1}_pkg.newRows.count+1 ) := :new.rowid;  ", table.Name, table.Name));
            sql.AppendLine(" END; ");
            sqlList.Add(sql.ToString());


            sql = new StringBuilder();
            sql.AppendLine(string.Format("CREATE OR REPLACE TRIGGER {0}_cln ", table.Name));
            sql.AppendLine(" AFTER INSERT ");
            sql.AppendLine(string.Format(" ON {0} ", table.Name));
            sql.AppendLine(" BEGIN ");
            sql.AppendLine(string.Format(" for i in 1 .. {0}_pkg.newRows.count loop", table.Name));
            sql.AppendLine(string.Format(" delete from {0}  where rowid = {1}_pkg.newRows(i); ", table.Name, table.Name));
            sql.AppendLine("  end loop;  ");
            sql.AppendLine(" END; ");
            sqlList.Add(sql.ToString());

            return sqlList;

        }

        public string[] MigrateFromModel(string baseNameSpace, IDataProvider provider)
        {
            var result = new List<string>();

            //pull all the objects out of the namespace
            var modelTypes = _modelAssembly.GetTypes().Where(x => x.Namespace != null 
                && x.Namespace.Equals(baseNameSpace)
                && !x.IsAbstract
                && x.IsPublic);

            foreach (var type in modelTypes)
            {
                result.AddRange(MigrateFromModel(type, provider));
            }

            foreach (var type in modelTypes)
            {
                result.AddRange(GenUpdateTriggerFromModel(type, provider));
            }

            return result.ToArray();
        }
    }
}