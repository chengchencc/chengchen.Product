using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubSonic.Oracle.Schema
{
    public interface IPartition
    {
        string Column { get; set; }
        int SpaceLength { get; set; }
        string SpacePrefix { get; set; }
        string PartPrefix { get; set; }
        object MinValue { get; set; }
        int PartValue { get; set; }
        bool IsMonthly { get; set; }
        bool UseDateFormat { get; set; }
        string GetPartitionSql(ITable table);
    }
}
