using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Oracle.Schema
{
    public class DBPartition : IPartition
    {
        private string columnName = string.Empty;
        public DBPartition()
        {
        }

        public DBPartition(string column, string partPrefix, int spaceLength, string spacePrefix, object minValue, int partValue, bool isMonthly, bool useDateFormat = false)
        {
            this.columnName = column;
            this.SpaceLength = spaceLength;
            this.SpacePrefix = spacePrefix;
            this.MinValue = minValue;
            this.PartValue = partValue;
            this.PartPrefix = partPrefix;
            this.IsMonthly = isMonthly;
            this.UseDateFormat = useDateFormat;
        }

        public bool UseDateFormat
        {
            get;
            set;
        }

        public string Column
        {
            get
            {
                return columnName;
            }
            set
            {
                columnName = value;
            }
        }

        public int SpaceLength { get; set; }

        public string SpacePrefix { get; set; }

        public object MinValue { get; set; }

        public int PartValue { get; set; }

        public virtual string GetPartitionSql(ITable table)
        {
            throw new NotImplementedException();
        }

        public string PartPrefix { get; set; }


        public bool IsMonthly { get; set; }
    }
}
