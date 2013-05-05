using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubSonic.Oracle.Linq.Structure;

namespace SubSonic.Oracle.Extensions
{
    /// <summary>
    /// Contains general extension methods for IQueryable
    /// </summary>
    public static class IQueryableExtenstions
    {
        public static string GetQueryText(this IQueryable query)
        {
            try
            {
                return (query.Provider as IQueryText).GetQueryText(query.Expression);
            }
            catch
            {
                return null;
            }
        }

        public static string PrintDebugInfo(this IQueryable query)
        {
            return string.Format(@"IQueryable.ToString
-------------------
{0}

IQueryable.Expression.ToString
------------------------------
{1}

Expression Tree
---------------
{2}

SQL Query
---------
{3}", query.ToString(), query.Expression.ToString(), query.Expression.PrintExpressionTree(), query.GetQueryText() ?? "Unable to build query text.");
        }
    }
}
