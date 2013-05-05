using System;
using System.Collections.Generic;
using System.Text;

namespace SubSonic.Linq.Structure
{
    /// <summary>
    /// This class is here to provide a common trace point for what expression nodes are visited by the various ExpressionVisitor classes.
    /// This can be used to provide some debugging insight as to why expressions are parsed the way they are.
    /// 
    /// Caveot: this assumes that expression processing is done on a single thread, as this uses Thread Local Storage (TLS) to collect data.
    /// </summary>
    internal static class ExpressionVisitorTrace
    {
        [ThreadStatic]
        private static List<string> traceData;
        
        /// <summary>
        /// Indicates that expression visitor tracing is enabled.
        /// If this method is not called, then no tracing will be available.
        /// </summary>
        internal static void Init()
        {
            traceData = new List<string>();
        }

        internal static void Trace(string nodeTypeName, object visitor)
        {
            if(traceData != null)
                traceData.Add(nodeTypeName + " visited by " + visitor.GetType().Name);
        }

        internal static string GetTraceString()
        {
            var sb = new StringBuilder();
            if (traceData != null)
                traceData.ForEach(x => sb.Append(x).Append("\r\n"));
            return sb.ToString();
        }

        /// <summary>
        /// Call this when all Expression Visitors have been run to clean up memory.
        /// </summary>
        internal static void Deinit()
        {
            traceData.Clear();
            traceData = null;
        }
    }
}
