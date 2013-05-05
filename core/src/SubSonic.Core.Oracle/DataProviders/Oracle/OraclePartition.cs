using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Schema;

namespace SubSonic.Oracle.DataProviders.Oracle
{
    public class OraclePartition : DBPartition
    {
        public OraclePartition()
        {
        }

        public OraclePartition(IPartition partition)
        {
            this.Column = partition.Column;
            this.MinValue = partition.MinValue;
            this.PartPrefix = partition.PartPrefix;
            this.PartValue = partition.PartValue;
            this.SpaceLength = partition.SpaceLength;
            this.SpacePrefix = partition.SpacePrefix;
            this.IsMonthly = partition.IsMonthly;
            this.UseDateFormat = partition.UseDateFormat;
        }
        
        public override string GetPartitionSql(ITable table)
        {
            StringBuilder sql = new StringBuilder("PARTITION BY RANGE (");
            string columnName = table.GetColumn(this.Column).Name.ToUpper();
            sql.Append(columnName);
            sql.AppendLine(")");
            sql.AppendLine("(");
            object currentValue= this.MinValue;
            for (int i = 1; i <= this.SpaceLength; i++)
            {
                if (i > 1)
                    sql.AppendLine(",");

                string item = this.IsMonthly ? this.CreatePartitionItem(table, ((DateTime)currentValue).Month, currentValue) : this.CreatePartitionItem(table, i, currentValue);
                if (this.MinValue is DateTime)
                {
                    if (this.IsMonthly)
                    {
                        currentValue = ((DateTime)currentValue).AddMonths(this.PartValue);
                    }
                    else
                    {
                        currentValue = ((DateTime)currentValue).AddDays(this.PartValue);
                    }
                }

                sql.AppendFormat(item);

            }
            sql.AppendLine(")");
            return sql.ToString();
        }

        public string CreatePartitionItem(ITable table, int index, object currentValue)
        {
            string value = string.Empty;
            if (this.MinValue is DateTime)
            {
                value = string.Format("TO_DATE(''{0}'',''DD-MM-YYYY'')", ((DateTime)currentValue).ToString("dd-MM-yyyy"));              
            }
            if (this.UseDateFormat)
            {
                var indexPostfix = ((DateTime)currentValue).AddDays(-1).ToString("yyyyMMdd");
                return string.Format("PARTITION {0} VALUES LESS THAN ({1}) TABLESPACE {2}", this.PartPrefix + indexPostfix, value, this.SpacePrefix.ToUpper() + indexPostfix);
            }
            else
            {
                return string.Format("PARTITION {0} VALUES LESS THAN ({1}) TABLESPACE {2}", this.PartPrefix + "_PT" + index, value, this.SpacePrefix.ToUpper() + index);
            }

        }

        public string DropPartitionItem(ITable table, int index)
        {
            return string.Format("ALTER TABLE {0} DROP PARTITION {1}", table.Name, this.PartPrefix + "_PT" + index);

        }
    }
}
