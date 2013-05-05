using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace SubSonic.Oracle.Extensions
{
    /// <summary>
    /// This class contains general extension methods for the System.Linq.Expressions.Expression class.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Evaluates an Expression and builts a for-display string representation of the expression tree.
        /// This is primarily used to debug and visualize an expression, to help understand how it will be parsed.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <remarks>
        /// this is reflection heavy, try not to use it too terribly much.
        /// was too lazy to build out full dynamic methods for a basic utility/support/debug-helper
        /// function, since it shouldn't be called all that much.
        /// </remarks>
        public static string PrintExpressionTree(this Expression expr)
        {
            var sb = new StringBuilder();
            int nodeNumber = 1;
            VisitNode(expr, "Root", sb, 0, ref nodeNumber);
            return sb.ToString();
        }

        private static void AddTreeLines(StringBuilder sb, int depth)
        {
            if (depth == 0)
                return;

            AddBlankTreeLines(sb, depth);
            sb.Append("|-- ");
        }
        private static void AddBlankTreeLines(StringBuilder sb, int depth)
        {
            for (int i = 0; i < depth - 1; i++)
                sb.Append("|   ");
        }

        private static void VisitNode(Expression expr, string desc, StringBuilder sb, int depth, ref int nodeNumber)
        {
           AddTreeLines(sb, depth);
           if (expr == null)
            {
                sb.AppendFormat("[{1}]{0}: null",
                    desc,
                    nodeNumber);
                sb.AppendLine("");
                return;
            }

            var additionalChildren = new List<KeyValuePair<string, Expression>>();

            sb.AppendFormat("[{3}]{0}: {1} (evaluates to {2})",
                desc,
                expr.GetType().Name,
                expr.Type == null ? "null" : expr.Type.Name,
                nodeNumber);

            var t = expr.GetType().FullName;
            switch (t)
            {
                case "System.Linq.Expressions.BinaryExpression":
                    var e1 = (BinaryExpression)expr;
                    break;
                case "System.Linq.Expressions.ConditionalExpression":
                    var e3 = (ConditionalExpression)expr;
                    break;
                case "System.Linq.Expressions.ConstantExpression":
                    var e4 = (ConstantExpression)expr;
                    sb.AppendFormat(" Value=({0})", e4.Value);
                    break;
                case "System.Linq.Expressions.InvocationExpression":
                    var e10 = (InvocationExpression)expr;
                    if (e10.Arguments != null)
                    {
                        sb.AppendFormat(" ArgumentCount=({0})", e10.Arguments.Count);
                        for (int i = 0; i < e10.Arguments.Count; i++)
                            additionalChildren.Add(new KeyValuePair<string, Expression>("Argument" + (i+1).ToString(), e10.Arguments[i]));
                    }
                    break;
                case "System.Linq.Expressions.LambdaExpression":
                    var e12 = (LambdaExpression)expr;
                    break;
                case "System.Linq.Expressions.ListInitExpression":
                    var e13 = (ListInitExpression)expr;
                    break;
                case "System.Linq.Expressions.MemberExpression":
                    var e15 = (MemberExpression)expr;
                    sb.AppendFormat(" MemberName=({0})", e15.Member.Name);
                    break;
                case "System.Linq.Expressions.MemberInitExpression":
                    var e16 = (MemberInitExpression)expr;
                    break;
                case "System.Linq.Expressions.MethodCallExpression":
                    var e17 = (MethodCallExpression)expr;
                    sb.AppendFormat(" MethodName=({0})", e17.Method.Name);
                    if (e17.Arguments != null)
                    {
                        sb.AppendFormat(" ArgumentCount=({0})", e17.Arguments.Count);
                        for (int i = 0; i < e17.Arguments.Count; i++)
                            additionalChildren.Add(new KeyValuePair<string, Expression>("Argument" + (i + 1).ToString(), e17.Arguments[i]));
                    }
                    break;
                case "System.Linq.Expressions.NewArrayExpression":
                    var e18 = (NewArrayExpression)expr;
                    break;
                case "System.Linq.Expressions.NewExpression":
                    var e19 = (NewExpression)expr;
                    break;
                case "System.Linq.Expressions.ParameterExpression":
                    var e20 = (ParameterExpression)expr;
                    sb.AppendFormat(" ParameterName=({0})", e20.Name);
                    break;
                case "System.Linq.Expressions.TypeBinaryExpression":
                    var e24 = (TypeBinaryExpression)expr;
                    break;
                case "System.Linq.Expressions.UnaryExpression":
                    var e25 = (UnaryExpression)expr;
                    if(e25.Method != null)
                        sb.AppendFormat(" MethodName=({0})", e25.Method.Name);
                    break;
            }
            sb.AppendLine("");

            var hadChild = false;
            // Loop thoruhg child nodes
            foreach (var child in additionalChildren)
            {
                nodeNumber++;
                VisitNode(child.Value, child.Key, sb, depth + 1, ref nodeNumber);
            }
            // Loop through each public property that is an Expression, and visit it as a child node.
            foreach (var prop in expr.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(Expression) || p.PropertyType.IsSubclassOf(typeof(Expression))))
            {
                nodeNumber++;
                VisitNode((Expression)prop.GetValue(expr, null), prop.Name, sb, depth + 1, ref nodeNumber);
            }
        }

    }
}
