using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using SubSonic.Oracle.DataProviders;
using SubSonic.Oracle.Extensions;
using SubSonic.Oracle.Schema;
using SubSonic.Oracle.SqlGeneration.Schema;
using System.Text.RegularExpressions;

namespace SubSonic.Oracle.DataProviders.Oracle
{
    public class OracleSchema : ANSISchemaGenerator
    {

        public OracleSchema()
        {
            ADD_COLUMN = "ALTER TABLE {0} ADD {1}{2}";
            ALTER_COLUMN = "ALTER TABLE {0} MODIFY {1}{2}";
            CREATE_TABLE = "CREATE TABLE {0} ({1}\r\n)";
            DROP_COLUMN = "ALTER TABLE {0} DROP COLUMN {1}";
            DROP_TABLE = "DROP TABLE {0} CASCADE CONSTRAINTS PURGE";
            DROP_INDEX = "DROP INDEX {0}";
            // i'm assuming people don't want to use oracle's stupid "recycle bin"-esque thing, and including the 'purge'.
            // we need also to cascade constraints or you will get an ORA-02449 exception
        }

        public override string BuildCreateTableStatement(ITable table)
        {
            var columnSql = GenerateColumns(table);
            var tableSql = string.Format(CREATE_TABLE, table.Name, columnSql);
            tableSql += BuildTablePartitionStatement(table);
            // set up sequences and triggers for any auto-incrementing columns.
            var sequenceSql = GenerateSequences(table);
            var insertProSql = GenerateInsertProcedure(table);
            return String.IsNullOrEmpty(sequenceSql)
                    ? tableSql
                    : String.Format("DECLARE BEGIN EXECUTE IMMEDIATE '{0}'; {1}  EXECUTE IMMEDIATE '{2}'; END; ", tableSql, sequenceSql, insertProSql);
        }

        private string BuildTablePartitionStatement(ITable table)
        {
            var sql = new StringBuilder();
            if (table.Partition != null)
            {
                OraclePartition partition = new OraclePartition(table.Partition);
                sql.Append(partition.GetPartitionSql(table));
            }
            return sql.ToString();
        }

        /// <summary>
        /// Builds a DROP TABLE statement.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        /*public override string BuildDropTableStatement(string tableName)
        {

        }*/

        public override string BuildDropTableStatement(ITable table)
        {
            StringBuilder sqlBulder = new StringBuilder();

            sqlBulder.Append(String.Format("DECLARE BEGIN EXECUTE IMMEDIATE '{0}';", GetDropTableSql(table)));

            foreach (IColumn col in table.Columns)
            {
                if (col.AutoIncrement && col.IsNumeric)
                {
                    sqlBulder.Append(GetDropSequenceSqlForColumn(table, col));
                }
            }
            sqlBulder.AppendFormat(" EXECUTE IMMEDIATE '{0}';", GetDropInsertProcedureSql(table));

            return sqlBulder.Append("END;").ToString();
        }

        private string GetDropSequenceSqlForColumn(ITable table, IColumn column)
        {
            // check for and drop each sequence that was associated with an auto-incrementing numeric PK column.
            string seqName = GetSequenceName(table, column);
            string dropSequenceSql = String.Format(@"DECLARE seq_cnt INT; BEGIN SELECT COUNT(*) INTO seq_cnt FROM ALL_SEQUENCES WHERE SEQUENCE_NAME = '{0}'", seqName);
            if (HasSchemaName(table))
            {
                dropSequenceSql += String.Format(" AND SEQUENCE_OWNER = '{0}'", table.SchemaName);
                seqName = String.Format("{0}.{1}", table.SchemaName, seqName);
            }
            dropSequenceSql += String.Format(@"; IF (seq_cnt > 0) THEN EXECUTE IMMEDIATE 'DROP SEQUENCE {0}'; END IF; END;", seqName);
            return dropSequenceSql;
        }

        private string GetDropInsertProcedureSql(ITable table)
        {
            string procName = GetInsertProcedureName(table);
            return " DROP PROCEDURE " + procName;
        }

        public override string BuildAlterIndexStatement(IColumn column)
        {
            column.IndexName = GetDefaultIndexName(column);
            if (!column.IsIndexed)
                return this.BuildDropIndexStatement(column);
            else
                return string.Format("DECLARE BEGIN EXECUTE IMMEDIATE 'DECLARE ls_count number; BEGIN SELECT COUNT(1) INTO ls_count FROM ALL_INDEXES WHERE INDEX_NAME= ''{0}''; IF ls_count = 0 THEN Execute immediate ''{1}''; END IF; END;'; END;", column.IndexName, this.BuildAddIndexStatement(column));
        }

        public override string BuildAddIndexStatement(IColumn column)
        {
            column.IndexName = GetDefaultIndexName(column);
            StringBuilder sql = new StringBuilder("CREATE INDEX ");
            sql.Append(column.IndexName.ToUpper());
            sql.Append(" ON ");
            sql.Append(column.Table.Name.ToUpper());
            sql.Append("(");
            sql.Append(column.Name);
            sql.Append(")");
            if (column.IsPartitionIndex)
                sql.Append(" local");

            return sql.ToString();
        }

        public override string BuildDropIndexStatement(IColumn column)
        {
            column.IndexName = GetDefaultIndexName(column);
            return string.Format("DECLARE BEGIN EXECUTE IMMEDIATE 'DECLARE ls_count number; BEGIN SELECT COUNT(1) INTO ls_count FROM ALL_INDEXES WHERE INDEX_NAME= ''{0}''; IF ls_count > 0 THEN Execute immediate ''DROP INDEX {0}''; END IF; END;'; END;", column.IndexName);
        }

        private string GetDropTableSql(ITable table)
        {
            return String.Format(DROP_TABLE, table.QualifiedName);
        }

        private bool HasSchemaName(ITable table)
        {
            return !(string.IsNullOrEmpty(table.SchemaName));
        }

        private string GetSequenceName(ITable table, IColumn col)
        {
            string sequenceName = String.Format("{0}_{1}", table.Name.ToUpper(), col.Name.ToUpper());
            if (sequenceName.Length > (26)) // 30 char max name length in oracle.
            {
                sequenceName = sequenceName.Substring(0, 26);
            }
            sequenceName += "_SEQ";
            return sequenceName;
        }

        private string GenerateSequences(ITable table) // subsonic's to do: add owner account handling.
        {
            var sql = new StringBuilder();
            foreach (var col in table.Columns)
            {
                if (col.AutoIncrement && col.IsNumeric)
                {
                    // this is a bit hackey and problematic, but truncating the names to 30 chars since that is oracle's limit.
                    var seqName = string.Format("{0}_{1}", table.Name.ToUpper(), col.Name.ToUpper());
                    var trgName = string.Format("{0}_{1}", table.Name.ToUpper(), col.Name.ToUpper());
                    if (seqName.Length > (26))
                        seqName = seqName.Substring(0, 26);
                    seqName += "_SEQ";
                    if (trgName.Length > (27))
                        trgName = trgName.Substring(0, 27);
                    trgName += "_BI";

                    sql.AppendFormat(@"
  DECLARE
	seq_cnt INT;
  BEGIN
	SELECT COUNT(*) INTO seq_cnt FROM ALL_SEQUENCES WHERE SEQUENCE_NAME = '{0}';
	IF (seq_cnt = 0) THEN
	  EXECUTE IMMEDIATE 'CREATE SEQUENCE {0} START WITH 1 INCREMENT BY 1';
	END IF;
  END;
", seqName);

                    // add a trigger to use the sequence
                    /*sql.AppendFormat(@"
EXECUTE IMMEDIATE '
  CREATE TRIGGER {0}
    BEFORE INSERT ON {1}
    FOR EACH ROW
  DECLARE
  BEGIN
    SELECT {2}.NEXTVAL INTO :NEW.{3} FROM DUAL;
  END;';
", trgName, table.Name, seqName, col.Name);*/
                }
            }
            return sql.ToString();
        }

        public override string GenerateInsertProcedure(ITable table)
        {
            var sql = new StringBuilder();
            //sql.Append("EXECUTE IMMEDIATE '");
            sql.Append("CREATE OR REPLACE PROCEDURE ");
            sql.Append(GetInsertProcedureName(table));
            sql.AppendLine("(");
            bool isFirst = true;
            string outStr = string.Empty;
            foreach (var col in table.Columns)
            {
                if (!isFirst)
                {
                    sql.Append(",");
                    sql.AppendLine();
                }
                sql.Append("P_");
                sql.Append(col.Name);
                if (col.AutoIncrement)
                {
                    sql.Append(" OUT ");
                    outStr = string.Format("SELECT {0}_{1}_SEQ.NEXTVAL INTO P_{1} from DUAL;", table.Name.ToUpper(), col.Name.ToUpper());
                }
                else
                    sql.Append(" IN ");
                sql.AppendFormat("{0}.{1}%TYPE", table.Name, col.Name);


                isFirst = false;
            }
            isFirst = true;
            sql.AppendLine(") IS");
            sql.AppendLine(" BEGIN");
            sql.AppendLine(outStr);
            sql.Append(" INSERT INTO ");
            sql.Append(table.Name);
            sql.Append("(");
            foreach (var col in table.Columns)
            {
                if (!isFirst)
                    sql.Append(",");
                sql.Append(col.Name);
                isFirst = false;
            }
            sql.Append(")");
            sql.Append(" VALUES(");
            string outSql = string.Empty;
            isFirst = true;
            foreach (var col in table.Columns)
            {
                if (!isFirst)
                    sql.Append(",");

                sql.AppendFormat("P_{0}", col.Name);

                isFirst = false;
            }
            sql.AppendLine(");");
            sql.AppendLine(outSql);
            sql.AppendLine("END;");
            // sql.Append("'");

            return sql.ToString();
        }

        private string GetInsertProcedureName(ITable table)
        {
            return table.Name + "_PROC_INS";
        }


        public override string GenerateColumnAttributes(IColumn column)
        {
            StringBuilder sb = new StringBuilder(" ");

            sb.Append(GetNativeType(column.DataType, column.MaxLength, column.NumericPrecision, column.NumberScale));

            if (column.DefaultSetting != null && column.DefaultSetting.ToString().Length > 0) // In Oracle, empty string ('') is the same as NULL. Can't use an empty string as a default value.
            {
                if (column.IsNumeric)
                    sb.Append(" DEFAULT " + column.DefaultSetting); // numeric types don't get quotes
                else
                    sb.Append(" DEFAULT '" + column.DefaultSetting + "'"); // date and string types do get quotes
            }

            if (column.IsPrimaryKey || !column.IsNullable)
                sb.Append(" NOT NULL");

            return sb.ToString();
        }

        public override ITable GetTableFromDB(IDataProvider provider, string tableName)
        {
            ITable result = null;
            DataTable schema;

            using (var scope = new AutomaticConnectionScope(provider))
            {
                // get the owner
                var owner = default(string);
                var connectionString = provider.ConnectionString;
                var pattern = @"user\s+id=(?<owner>[^;]+);";
                var regex = new Regex(pattern);
                var match = regex.Match(connectionString);
                if (match.Success && match.Groups["owner"].Success)
                {
                    owner = match.Groups["owner"].Value.ToUpper();
                }

                var restrictions = new string[] { owner, tableName.ToUpper(), null }; // restrictions are {owner, table, column}
                schema = scope.Connection.GetSchema("COLUMNS", restrictions);
            }

            if (schema.Rows.Count > 0)
            {
                result = new DatabaseTable(tableName, provider);
                foreach (DataRow dr in schema.Rows)
                {
                    IColumn col = new DatabaseColumn(dr["COLUMN_NAME"].ToString(), result);

                    col.IsNullable = dr["NULLABLE"].ToString() == "Y";
                    var maxLength = dr["LENGTH"].ToString();
                    var precision = dr["PRECISION"].ToString();
                    var scale = dr["SCALE"].ToString();

                    int iMax = 0, iPrecision = 0, iScale = 0;
                    int.TryParse(maxLength, out iMax);
                    int.TryParse(precision, out iPrecision);
                    int.TryParse(scale, out iScale);
                    col.DataType = GetDbTypeByPrecisionAndScale(dr["DATATYPE"].ToString(), iPrecision, iScale);
                    col.MaxLength = iMax;
                    col.NumericPrecision = iPrecision;
                    col.NumberScale = iScale;
                    //col.

                    result.Columns.Add(col);
                }
            }

            return result;
        }

        public override string[] GetTableList(IDataProvider provider)
        {
            var result = new List<string>();
            using (DbConnection conn = provider.CreateConnection())
            {
                conn.Open();
                var schema = conn.GetSchema("TABLES");
                conn.Close();

                foreach (DataRow dr in schema.Rows)
                {
                    if (dr["TABLE_TYPE"].ToString().Equals("BASE TABLE"))
                        result.Add(dr["TABLE_NAME"].ToString());
                }
            }
            return result.ToArray();
        }

        public override string GenerateColumns(ITable table)
        {
            var createSql = new StringBuilder();
            var pkCols = new StringBuilder();
            foreach (var col in table.Columns)
            {
                createSql.AppendFormat("\r\n  {0}{1},", col.Name, GenerateColumnAttributes(col));
                if (col.IsPrimaryKey)
                {
                    if (pkCols.Length > 0)
                        pkCols.Append(", ");
                    pkCols.AppendFormat("{0}", col.Name);
                }
            }

            if (pkCols.Length > 0)
                createSql.AppendFormat("\r\n  CONSTRAINT {0}_PK PRIMARY KEY ({1}),", table.Name, pkCols);
            return createSql.ToString().Chop(",");
        }

        public override string BuildAddColumnStatement(string tableName, IColumn column)
        {
            //if we're adding a Non-null column to the DB schema, there has to be a default value
            //otherwise it will result in an error'
            if (!column.IsNullable && column.DefaultSetting == null)
                SetColumnDefaults(column);

            return string.Format(ADD_COLUMN, tableName, column.Name, GenerateColumnAttributes(column));
        }

        public override string GetNativeType(DbType dbType)
        {
            return GetNativeType(dbType, 0, 0, 0);
        }

        public string GetNativeType(DbType dbType, int maxLength, int precision, int scale)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    // max varchar2 size in ora9-11 is 4k BYTES. which basically means this is wrong if we have multi-byte utf characters in the string.
                    if (maxLength == 0)
                        return "VARCHAR2(4000)";
                    else if (maxLength > 4000)
                        return "CLOB";
                    else
                        return string.Format("VARCHAR2({0})", maxLength);

                case DbType.Binary:
                    return "BLOB";

                case DbType.Boolean:
                    // Oracle doesn't have a boolean data type. The two most common approaches are to use a
                    // VARCHAR2(1) column and use 'Y' and 'N' values, or use a NUMBER(1,0) and use 1 and 0.
                    // This is making the assumption that the user is using number(1) to represent a bool.
                    return "NUMBER(1)";

                case DbType.Byte:
                    return "CHAR(1)";

                case DbType.Date:
                    return "DATE";

                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "Date";

                case DbType.Decimal:
                case DbType.Currency:
                    if (precision == 0)
                        precision = 38;
                    if (scale == 0)
                        scale = 127;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Double:
                    if (precision == 0)
                        precision = 15;
                    if (scale == 0)
                        scale = 44;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Guid:
                    if (maxLength == 0)
                        maxLength = 36;
                    return string.Format("CHAR({0})", maxLength);

                case DbType.SByte:
                    if (precision == 0)
                        precision = 2;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Int16:
                case DbType.UInt16:
                    if (precision == 0)
                        precision = 4;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Int32:
                case DbType.UInt32:
                    if (precision == 0)
                        precision = 9;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Int64:
                case DbType.UInt64:
                    if (precision == 0)
                        precision = 18;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Object:
                    return "BLOB";

                case DbType.Single:
                    if (precision == 0)
                        precision = 7;
                    if (scale == 0)
                        scale = 44;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Time:
                    return "TIMESTAMP";

                case DbType.VarNumeric:
                    if (precision == 0)
                        precision = 38;
                    return string.Format("NUMBER({0},{1})", precision, scale);

                case DbType.Xml:
                    return "XMLTYPE";

                default:
                    return "VARCHAR2(4000)";
            }
        }

        public override DbType GetDbType(string sqlType)
        {
            sqlType = sqlType.ToUpper();
            switch (sqlType)
            {
                case "CLOB":
                case "NCHAR":
                case "NCLOB":
                case "VARCHAR":
                case "NVARCHAR2":
                case "VARCHAR2":
                case "XMLTYPE":
                    return DbType.String;
                case "PLS_INTEGER":
                case "BINARY_INTEGER":
                    return DbType.Int32;
                case "LONG":
                    return DbType.Int64;
                case "DATE":
                case "TIMESTAMP":
                    return DbType.DateTime;
                case "ANYDATA":
                case "BLOB":
                case "RAW":
                case "LONG RAW":
                    return DbType.Binary;
                case "NUMBER":
                    return DbType.Decimal;
                case "FLOAT":
                    return DbType.Double;
                default:
                    return DbType.String;
            }
        }

        public DbType GetDbTypeByPrecisionAndScale(string sqlType, int precision, int scale)
        {
            sqlType = sqlType.ToUpper();
            switch (sqlType)
            {
                case "CLOB":
                case "NCHAR":
                case "NCLOB":
                case "VARCHAR":
                case "NVARCHAR2":
                case "VARCHAR2":
                case "XMLTYPE":
                    return DbType.String;
                case "PLS_INTEGER":
                case "BINARY_INTEGER":
                    return DbType.Int32;
                case "LONG":
                    return DbType.Int64;
                case "DATE":
                case "TIMESTAMP":
                case "TIMESTAMP(6)":
                    return DbType.DateTime;
                case "ANYDATA":
                case "BLOB":
                case "RAW":
                case "LONG RAW":
                    return DbType.Binary;
                case "NUMBER":
                    if (precision == 1)
                    {
                        return DbType.Boolean;
                    }
                    else if (precision == 38 && scale == 127)
                    {
                        return DbType.Currency;
                    }
                    else if (precision == 15 && scale == 44)
                    {
                        return DbType.Double;
                    }
                    else if (precision == 2)
                    {
                        return DbType.SByte;
                    }
                    else if (precision == 4)
                    {
                        return DbType.Int16;
                    }
                    else if (precision == 9)
                    {
                        return DbType.Int32;
                    }
                    else if (precision == 7 && scale == 44)
                    {
                        return DbType.Single;
                    }
                    else if (precision == 18)
                    {
                        return DbType.Int64;
                    }
                    else if (precision == 38 && scale == 0)
                    {
                        return DbType.VarNumeric;
                    }
                    else
                    {
                        return DbType.Decimal;
                    }
                case "FLOAT":
                    return DbType.Double;
                default:
                    return DbType.String;
            }
        }

        public override void SetColumnDefaults(IColumn column)
        {
            if (column.IsNumeric)
                column.DefaultSetting = 0;
            else if (column.IsDateTime)
                column.DefaultSetting = DateTime.Parse("1900/1/1");
            else if (column.IsString)
                column.DefaultSetting = " "; // I don't think there is anything reasonable we can put here. In oracle, empty string == NULL, so we can't use that.
            else if (column.DataType == DbType.Boolean)
                column.DefaultSetting = 0;
        }

        public override object ConvertDataValueForThisProvider(object input)
        {
            if (input == null)
            {
                return null;
            }
            if (input is bool)
            {
                return (bool)input ? 1 : 0;
            }
            if (input is Guid)
            {
                return input.ToString();
            }
            return base.ConvertDataValueForThisProvider(input);
        }

        public override DbType ConvertDataTypeToDbType(DbType dataType)
        {
            if (dataType == DbType.Guid)
            {
                return DbType.String;
            }
            if (dataType == DbType.Boolean)
            {
                return DbType.Int16;
            }
            return base.ConvertDataTypeToDbType(dataType);
        }
    }
}