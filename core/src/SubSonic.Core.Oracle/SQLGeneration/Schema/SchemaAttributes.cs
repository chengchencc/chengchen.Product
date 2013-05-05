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
using System.Data;
using SubSonic.Oracle.Schema;

namespace SubSonic.Oracle.SqlGeneration.Schema
{
    public interface IClassMappingAttribute
    {
        bool Accept(ITable table);
        void Apply(ITable table);
    }

    public interface IPropertyMappingAttribute
    {
        bool Accept(IColumn column);
        void Apply(IColumn column);
    }

    public interface IPartitionMappingAttribute
    {
        bool Accept(ITable table);
        void Apply(ITable table);
        IPartition Partition { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SubSonicTableNameOverrideAttribute : Attribute, IClassMappingAttribute
    {
        public string TableName { get; set; }

        public SubSonicTableNameOverrideAttribute(string tableName)
        {
            TableName = tableName;
        }

        public bool Accept(ITable table)
        {
            return true;
        }

        public void Apply(ITable table)
        {
            table.Name = this.TableName;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SubSonicTableSpaceOverrideAttribute : Attribute, IClassMappingAttribute
    {
        public string TableSpace { get; set; }

        public SubSonicTableSpaceOverrideAttribute(string tableSpace)
        {
            TableSpace = tableSpace;
        }

        public bool Accept(ITable table)
        {
            return true;
        }

        public void Apply(ITable table)
        {
            //table.TableSpace = this.TableSpace;
        }
        public string GetTableSpace()
        {
            return TableSpace;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicColumnNameOverrideAttribute : Attribute, IPropertyMappingAttribute
    {
        public string ColumnName { get; set; }

        public SubSonicColumnNameOverrideAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public bool Accept(IColumn column)
        {
            return true;
        }

        public void Apply(IColumn column)
        {
            column.NameInClass = column.Name;
            column.Name = ColumnName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class UpdateColumnAttribute : Attribute, IPropertyMappingAttribute
    {
        public bool Accept(IColumn column)
        {
            return true;
        }

        public void Apply(IColumn column)
        {
            column.IsForUpdateTrigger = true;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicNullStringAttribute : Attribute, IPropertyMappingAttribute
    {
        public bool Accept(IColumn column)
        {
            return DbType.String == column.DataType;
        }

        public void Apply(IColumn column)
        {
            column.IsNullable = true;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicLongStringAttribute : Attribute, IPropertyMappingAttribute
    {
        public bool Accept(IColumn column)
        {
            return DbType.String == column.DataType;
        }

        public void Apply(IColumn column)
        {
            column.MaxLength = 4000;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicIgnoreAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicPrimaryKeyAttribute : Attribute, IPropertyMappingAttribute
    {
        public bool AutoIncrement { get; set; }

        public bool CancelPrimaryKey { get; set; }

        public SubSonicPrimaryKeyAttribute() : this(true) { }

        public SubSonicPrimaryKeyAttribute(bool autoIncrement)
        {
            AutoIncrement = autoIncrement;
        }

        public bool Accept(IColumn column)
        {
            return true;
        }

        public void Apply(IColumn column)
        {
            if (!CancelPrimaryKey)
            {
                column.IsPrimaryKey = true;

                column.IsNullable = false;
                if (column.IsNumeric)
                    column.AutoIncrement = AutoIncrement;
                else if (column.IsString && column.MaxLength == 0)
                    column.MaxLength = 255;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicStringLengthAttribute : Attribute, IPropertyMappingAttribute
    {
        public int Length { get; set; }

        public SubSonicStringLengthAttribute(int length)
        {
            Length = length;
        }

        public bool Accept(IColumn column)
        {
            return DbType.String == column.DataType;
        }

        public void Apply(IColumn column)
        {
            column.MaxLength = Length;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicNumericPrecisionAttribute : Attribute, IPropertyMappingAttribute
    {
        public int Precision { get; set; }
        public int Scale { get; set; }

        public SubSonicNumericPrecisionAttribute(int precision, int scale)
        {
            Scale = scale;
            Precision = precision;
        }

        public bool Accept(IColumn column)
        {
            return column.DataType == DbType.Decimal || column.DataType == DbType.Double;
        }

        public void Apply(IColumn column)
        {
            column.NumberScale = Scale;
            column.NumericPrecision = Precision;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicDefaultSettingAttribute : Attribute, IPropertyMappingAttribute
    {
        public object DefaultSetting { get; set; }

        public SubSonicDefaultSettingAttribute(object defaultSetting)
        {
            DefaultSetting = defaultSetting;
        }

        public bool Accept(IColumn column)
        {
            return true;
        }

        public void Apply(IColumn column)
        {
            column.DefaultSetting = DefaultSetting;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MonthPartitionSettingAttribute : Attribute, IPartitionMappingAttribute
    {
        public IPartition Partition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column">字段名</param>
        /// <param name="partPrefix">分区前缀</param>
        /// <param name="spacePrefix">空间前缀</param>
        public MonthPartitionSettingAttribute(string column, string partPrefix, string spacePrefix)
        {
            DateTime nextMonth = DateTime.Today.AddMonths(1);
            this.Partition = new DBPartition(column, partPrefix, 12, spacePrefix, new DateTime(nextMonth.Year, nextMonth.Month, 1), 1, true);
        }
        public bool Accept(ITable table)
        {
            return true;
        }

        public void Apply(ITable table)
        {
            table.Partition = Partition;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DailyPartitionSettingAttribute : Attribute, IPartitionMappingAttribute
    {
        public IPartition Partition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="column">字段名</param>
        /// <param name="partPrefix">分区前缀</param>
        /// <param name="spacePrefix">空间前缀</param>
        public DailyPartitionSettingAttribute(string column, string partPrefix, string spacePrefix, bool userDateFormat = false)
        {
            DateTime today = DateTime.Today;
            this.Partition = new DBPartition(column, partPrefix, 31, spacePrefix, DateTime.Parse(today.Year + "/" + today.Month + "/2"), 1, false, userDateFormat);
        }
        public bool Accept(ITable table)
        {
            return true;
        }

        public void Apply(ITable table)
        {
            table.Partition = Partition;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class HasUpdateTriggerAttribute : Attribute, IClassMappingAttribute
    {
        private string _updateTableName;

        public HasUpdateTriggerAttribute(string updateTableName)
        {
            _updateTableName = updateTableName;
        }

        public bool Accept(ITable table)
        {
            return true;
        }

        public void Apply(ITable table)
        {
            table.HasUpdateTrigger = true;
            table.UpdateTableName = _updateTableName;
        }

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SubSonicIndexMappingAttribute : Attribute, IPropertyMappingAttribute
    {
        private string _groupName = string.Empty;
        private bool _isPartitionIndex = false;
        public SubSonicIndexMappingAttribute()
            : this(string.Empty, false)
        {

        }
        public SubSonicIndexMappingAttribute(bool isPartitionIndex)
            : this(string.Empty, isPartitionIndex)
        {

        }
        public SubSonicIndexMappingAttribute(string groupName)
            : this(groupName, false)
        {

        }
        public SubSonicIndexMappingAttribute(string groupName, bool isPartitionIndex)
        {
            this._groupName = groupName;
            this._isPartitionIndex = isPartitionIndex;

        }
        public bool Accept(IColumn column)
        {
            return true;
        }

        public void Apply(IColumn column)
        {
            column.IsPartitionIndex = this._isPartitionIndex;
            column.IsIndexed = true;
            if (!string.IsNullOrEmpty(_groupName))
                column.IndexName = column.Table.Name.ToUpper() + "_" + _groupName + "_IX";
        }
    }
}