using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackMamba.Framework.Core
{
    public static class TypeExtensions
    {
        public static Dictionary<Type, bool> s_enumerableTypes = new Dictionary<Type, bool>();
        public static Dictionary<Type, bool> s_serializableTypes = new Dictionary<Type, bool>();

        public static bool IsEnumerableType(this Type type)
        {
            if (type == null) return false;

            if (s_enumerableTypes.ContainsKey(type))
            {
                return s_enumerableTypes[type];
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && interfaceType.GetGenericArguments().Length == 1 && !interfaceType.GetGenericArguments()[0].Equals(typeof(char)))
                {
                    s_enumerableTypes[type] = true;

                    return true;
                }
            }

            s_enumerableTypes[type] = false;

            return false;
        }

        public static bool IsSerializableEnumerableType(this Type type)
        {
            if (type == null) return false;

            if (s_serializableTypes.ContainsKey(type))
            {
                return s_serializableTypes[type];
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    && interfaceType.GetGenericArguments().Length == 1 && !interfaceType.GetGenericArguments()[0].Equals(typeof(char)))
                {
                    if (interfaceType.GetGenericArguments()[0].IsSerializable)
                    {
                        s_serializableTypes[type] = true;
                        return true;
                    }
                }
            }
            s_serializableTypes[type] = false;

            return false;
        }
    }
}
