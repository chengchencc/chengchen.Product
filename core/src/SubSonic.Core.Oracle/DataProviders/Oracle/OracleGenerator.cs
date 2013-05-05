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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Extensions;
using SubSonic.Oracle.Query;
using SubSonic.Oracle.Schema;
using SubSonic.Oracle.DataProviders;
using Constraint = SubSonic.Oracle.Query.Constraint;
using SubSonic.Oracle.SqlGeneration;

namespace SubSonic.Oracle.DataProviders.Oracle
{
	/// <summary>
	/// 
	/// </summary>
	public class OracleGenerator : ANSISqlGenerator {
		private const string PAGING_SQL =
			@"
SELECT *
FROM     (SELECT ROW_NUMBER() OVER ({1}) AS RowNumber, 
{0} 
{2}
{3}
{4}
) PagedResults
 WHERE  RowNumber >= {5} AND RowNumber <= {6}";

        

		/// <summary>
		/// Initializes a new instance of the <see cref="OracleGenerator"/> class.
		/// </summary>
		/// <param name="query">The query.</param>
		public OracleGenerator(SqlQuery query)
			: base(query) { }

		/// <summary>
		/// Builds the paged select statement.
		/// </summary>
		/// <returns></returns>
		public override string BuildPagedSelectStatement() {
			Select qry = (Select)query;

			string idColumn = GetSelectColumns()[0];

			string select = GenerateCommandLine();
			string columnList = select.Replace("SELECT", String.Empty);
			string fromLine = GenerateFromList();
			string joins = GenerateJoins();
			string wheres = GenerateConstraints();
			string orderby = GenerateOrderBy();

			if (String.IsNullOrEmpty(orderby.Trim()))
				orderby = String.Concat(this.sqlFragment.ORDER_BY, idColumn);

			if (qry.Aggregates.Count > 0)
				joins = String.Concat(joins, GenerateGroupBy());

			int pageStart = (qry.CurrentPage - 1) * qry.PageSize + 1;
			int pageEnd = qry.CurrentPage * qry.PageSize;

			string sql = string.Format(PAGING_SQL, columnList, orderby, fromLine, joins, wheres, pageStart, pageEnd);
			return sql;
		}

		private string stripBraces(string toStrip) {
			return toStrip.Replace("[", "").Replace("]", "");
		}

		/// <summary>
		/// Generates the constraints.
		/// </summary>
		/// <returns></returns>
		public override string GenerateConstraints() {
			string whereOperator = this.sqlFragment.WHERE;

			if (query.Aggregates.Count > 0 && query.Aggregates.Any(x => x.AggregateType == AggregateFunction.GroupBy))
				whereOperator = this.sqlFragment.HAVING;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			bool isFirst = true;

			//int paramCount;
			bool expressionIsOpen = false;
			int indexer = 0;
			foreach (Constraint c in query.Constraints) {
				string columnName = String.Empty;
				bool foundColumn = false;
				if (c.ConstructionFragment == c.ColumnName && c.ConstructionFragment != "##") {
					IColumn col = FindColumn(c.ColumnName);

					if (col != null) {
						columnName = col.QualifiedName;
						c.ParameterName = string.Format("{0}{1}", GetParameterPrefix(), indexer);
						c.DbType = col.DataType;
						foundColumn = true;
					}
				}

				if (!foundColumn && c.ConstructionFragment != "##") {
					bool isAggregate = false;
					//this could be an expression
					//string rawColumnName = c.ConstructionFragment;
					if (c.ConstructionFragment.StartsWith("(")) {
						//rawColumnName = c.ConstructionFragment.Replace("(", String.Empty);
						expressionIsOpen = true;
					}
					//this could be an aggregate function
					else if (c.IsAggregate ||
							(c.ConstructionFragment.Contains("(") && c.ConstructionFragment.Contains(")"))) {
						//rawColumnName = c.ConstructionFragment.Replace("(", String.Empty).Replace(")", String.Empty);
						isAggregate = true;
					}

					IColumn col = FindColumn(c.ColumnName);
					if (!isAggregate && col != null) {
						columnName = c.ConstructionFragment.Replace(col.Name, col.QualifiedName);
						c.ParameterName = String.Concat(col.ParameterName, indexer.ToString());
						c.DbType = col.DataType;
					}
					else {
						c.ParameterName = query.FromTables[0].Provider.ParameterPrefix + indexer;
						columnName = c.ConstructionFragment;
					}

				}
				columnName = stripBraces(columnName);
				//paramCount++;

				if (!isFirst) {
					whereOperator = Enum.GetName(typeof(ConstraintType), c.Condition);
					whereOperator = String.Concat(" ", whereOperator.ToUpper(), " ");
				}

				if (c.Comparison != Comparison.OpenParentheses && c.Comparison != Comparison.CloseParentheses)
					sb.Append(whereOperator);

				if (c.Comparison == Comparison.BetweenAnd) {
					sb.Append(columnName);
					sb.Append(this.sqlFragment.BETWEEN);
					sb.Append(c.ParameterName + "_start");
					sb.Append(this.sqlFragment.AND);
					sb.Append(c.ParameterName + "_end");
				}
				else if (c.Comparison == Comparison.In || c.Comparison == Comparison.NotIn) {
					sb.Append(columnName);
					if (c.Comparison == Comparison.In)
						sb.Append(this.sqlFragment.IN);
					else
						sb.Append(this.sqlFragment.NOT_IN);

					sb.Append("(");

					if (c.InSelect != null) {
						//create a sql statement from the passed-in select
						string sql = c.InSelect.BuildSqlStatement();
						sb.Append(sql);
					}
					else {
						//enumerate INs
						IEnumerator en = c.InValues.GetEnumerator();
						StringBuilder sbIn = new StringBuilder();
						bool first = true;
						int i = 1;
						while (en.MoveNext()) {
							if (!first)
								sbIn.Append(",");
							else
								first = false;

							sbIn.Append(String.Concat(c.ParameterName, "In", i));
							i++;
						}

						string inList = sbIn.ToString();
						//inList = Sugar.Strings.Chop(inList);
						sb.Append(inList);
					}

					sb.Append(")");
				}
				else if (c.Comparison == Comparison.OpenParentheses) {
					expressionIsOpen = true;
					sb.Append("(");
				}
				else if (c.Comparison == Comparison.CloseParentheses) {
					expressionIsOpen = false;
					sb.Append(")");
				}
				else {
					if (columnName.StartsWith("("))
						expressionIsOpen = true;
					if (c.ConstructionFragment != "##") {
						sb.Append(columnName);
						sb.Append(Constraint.GetComparisonOperator(c.Comparison));
						if (c.Comparison == Comparison.Is || c.Comparison == Comparison.IsNot) {
							if (c.ParameterValue == null || c.ParameterValue == DBNull.Value)
								sb.Append("NULL");
						}
						else
							sb.Append(c.ParameterName);
					}
				}
				indexer++;

				isFirst = false;
			}

			string result = sb.ToString();
			//a little help...
			if (expressionIsOpen & !result.EndsWith(")"))
				result = String.Concat(result, ")");

			return result;
		}

		/// <summary>
		/// Builds the update statement.
		/// </summary>
		/// <returns></returns>
		public override string BuildUpdateStatement() {
			StringBuilder sb = new StringBuilder();

			//cast it

			sb.Append(this.sqlFragment.UPDATE);
			sb.Append(stripBraces(query.FromTables[0].QualifiedName));

			for (int i = 0; i < query.SetStatements.Count; i++) {
				if (i == 0) {
					sb.AppendLine(" ");
					sb.Append(this.sqlFragment.SET);
				}
				else
					sb.AppendLine(", ");

				sb.Append(stripBraces(query.SetStatements[i].ColumnName));

				sb.Append("=");

				if (!query.SetStatements[i].IsExpression)
					sb.Append(query.SetStatements[i].ParameterName);
				else
					sb.Append(query.SetStatements[i].Value.ToString());
			}

			//wheres
			sb.Append(GenerateConstraints());

			return sb.ToString();
		}

		public override string GetParameterPrefix() {
			return ":t";
		}

		/// <summary>
		/// Builds the insert statement.
		/// </summary>
		/// <returns></returns>
		public override string BuildInsertStatement() {
			StringBuilder sb = new StringBuilder();

			//cast it
			Insert i = insert;
            //sb.Append("begin ");
			sb.Append(this.sqlFragment.INSERT_INTO);
			sb.Append(stripBraces(i.Table.QualifiedName));
			sb.Append("(");
			sb.Append(stripBraces(i.SelectColumns));
            sb.Append(")");

			//if the values list is set, use that
            InsertSetting keySetting = null;
			if (i.Inserts.Count > 0) {
				sb.Append(" VALUES (");
				bool isFirst = true;
				foreach (InsertSetting s in i.Inserts) {
					if (!isFirst)
						sb.Append(",");

					if (!s.IsExpression && !s.IsPrimaryKey)
						sb.Append(s.ParameterName);
					else
						sb.Append(s.Value);
					isFirst = false;

                    if (s.IsPrimaryKey)
                    {
                        keySetting = s;
                        
                    }
				}
				sb.Append(")");
			}
			else {
				if (i.SelectValues != null) {
					string selectSql = i.SelectValues.BuildSqlStatement();
					sb.AppendLine(selectSql);
				}
				else {
					throw new InvalidOperationException(
						"Need to specify Values or a Select query to insert - can't go on!");
				}
			}
            //if (keySetting != null)
            //{

            //    return String.Format("DECLARE BEGIN EXECUTE IMMEDIATE '{0}'; {1} END;", sb.ToString(), "select " + keySetting.Value.ToString().Replace("Nextval", "CURRVAL") + " from DUAL");

            //}
            //sb.Append("end;");

           
			return sb.ToString();
		}
	}
}