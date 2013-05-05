using System;

namespace SubSonic
{
    /// <summary>
    /// Contains general information about a Linq query that can be helpful for debugging.
    /// </summary>
    public class QueryDebugInfo
    {
        public string ExpressionString { get; internal set; }
        public string ExpressionTree { get; internal set; }
        public string SQLStatement { get; internal set; }

        public override string ToString()
        {
            return string.Format(@"Expression String
-----------------
{0}

Expression Tree
---------------
{1}

SQL Query
---------
{2}", ExpressionString, ExpressionTree, SQLStatement);
        }
    }
}
