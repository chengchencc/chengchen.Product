using BlackMamba.Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlackMamba.Framework.Core
{
    public class ReflectionHelper
    {
        public static object GetField(object obj, string fieldName)
        {
            FieldInfo fi = obj.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            object result = fi.GetValue(obj);

            return result;
        }

        public static object GetProperty(object obj, string propertyName)
        {
            PropertyInfo fi = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            object result = fi.GetValue(obj, null);

            return result;
        }


        public static void SetProperty(object obj, string propertyName, object value)
        {
            PropertyInfo fi = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            fi.SetValue(obj, value, null);
        }

        public static T SetPropertiesByUrlParameters<T>(T obj, string urlParameters)
        {
            var items = new List<string>(urlParameters.Split('&'));
            var props = new Dictionary<string, string>();

            foreach (string item in items)
            {
                var keyValue = new List<string>(item.Split('='));

                props[keyValue[0]] = keyValue.Count > 1 ? keyValue[1].Trim() : "";
            }

            foreach (var item in props)
            {
                if (string.IsNullOrEmpty(item.Key)) continue;

                ReflectionHelper.SetProperty(obj, item.Key, item.Value);
            }

            return (T)obj;

        }

        /// <summary>
        /// Gets the type of the object from.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static object GetObjectFromType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return null;

            Type type = Type.GetType(typeName);

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Get the instance of specified assembly and type name
        /// </summary>
        /// <param name="assemblyName">The assembly name which contain the type</param>
        /// <param name="typeName">The type name to create instance</param>
        /// <returns>The instance of type</returns>
        public static object GetObjectFromType(string assemblyName, string typeName)
        {
            Type type = GetType(assemblyName, typeName);

            if (type == null)
            {
                throw new Exception(string.Format("Could not get type - {0} from assembly - {1}", typeName, assemblyName));
            }

            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static Type GetType(string assemblyName, string typeName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            Type type = assembly.GetType(typeName);

            return type;
        }


        /// <summary>
        /// Gets the method info.
        /// </summary>
        /// <remarks>
        /// This method will return the first matched method, no matter how many the methods overload
        /// </remarks>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(Type type, string methodName)
        {
            if (type == null
                || string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            MethodInfo methodInfo = type.GetMethod(methodName
                , BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            return methodInfo;
        }

        /// <summary>
        /// Examines the method match delegate.
        /// </summary>
        /// <param name="mi">The mi.</param>
        /// <param name="dgt">The DGT.</param>
        /// <returns></returns>
        public static bool ExamineMethodMatchDelegate(MethodInfo mi, Type dgt)
        {
            if (!typeof(Delegate).IsAssignableFrom(dgt))
            {
                throw new ArgumentException("dgt must be a delegate");
            }

            MethodInfo miTarget = dgt.GetMethod("Invoke");
            bool ret = miTarget.ReturnType.IsAssignableFrom(mi.ReturnType);
            ParameterInfo[] parasMi = mi.GetParameters();
            ParameterInfo[] parasTarget = miTarget.GetParameters();
            ret &= (parasMi.Length == parasTarget.Length);
            if (ret)
            {
                for (int i = 0; ret & (i < parasMi.Length); i++)
                {
                    ret = parasMi[i].ParameterType.IsAssignableFrom(parasTarget[i].ParameterType);
                }
            }

            return ret;
        }


        #region PropertyName related
        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propNameWithSeparator">The prop name with separator.</param>
        /// <param name="splitPattern">The split pattern.</param>
        /// <returns>Property value arrays</returns>
        public static object[] GetPropertyValues(object instance, string propNameWithSeparator, string splitPattern)
        {
            if (propNameWithSeparator != null && splitPattern != null)
            {
                string[] pNames = Regex.Split(propNameWithSeparator, splitPattern);
                return GetPropertyValues(instance, pNames);
            }

            return null;
        }

        /// <summary>
        /// Gets the property values.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyNames">The property names.</param>
        /// <returns>Property value arrays</returns>
        public static object[] GetPropertyValues(object instance, string[] propertyNames)
        {
            if (propertyNames != null)
            {
                object[] pValues = new object[propertyNames.Length];

                for (int i = 0; i < propertyNames.Length; i++)
                {
                    pValues[i] = GetPropertyValue(instance, propertyNames[i]);
                }

                return pValues;
            }

            return null;
        }

        /// <summary>
        /// Gets the property type from object.
        /// </summary>
        /// <param name="instance">Entity object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The Type of the property.</returns>
        /// <remarks>It support nested property.</remarks>
        public static Type GetPropertyType(object instance, string propertyName)
        {
            // If the parameter is not valid, return null directly
            if (string.IsNullOrEmpty(propertyName) || instance == null)
            {
                return null;
            }
            Type returnType = null;

            Type type = instance.GetType();
            int indexOfDot = propertyName.IndexOf(ASCII.DOT);
            if (indexOfDot == -1 && string.Compare(propertyName, type.Name, true) == 0)
            {
                returnType = type;
            }
            else
            {
                object parentInstance;

                PropertyInfo pInfo = GetPropertyInfo(instance, propertyName, out parentInstance);

                if (pInfo != null)
                    returnType = pInfo.PropertyType;
            }

            return returnType;
        }

        /// <summary>
        /// Gets the property value from object.
        /// </summary>
        /// <param name="instance">Entity object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The value of the property.</returns>
        /// <remarks>It support nested property.</remarks>
        /// <example>
        /// <code>
        /// Customer customer = new Customer();
        /// customer.Id = 30085132;
        /// customer.Address = new Address();
        /// customer.Address.Id = 888;
        /// 
        /// object objCustomerId = ReflectionHelper.GetPropertyValueFromObject(customer, "Customer.Id");
        /// object objAddressId = ReflectionHelper.GetPropertyValueFromObject(customer, "Customer.Address.Id");
        /// 
        /// Console.WriteLine("Customer Id is: " + objCustomerId.ToString());
        /// Console.WriteLine("Address Id is: " + objAddressId.ToString());
        /// </code>
        /// </example>
        public static object GetPropertyValue(object instance, string propertyName)
        {
            // If the parameter is not valid, return null directly
            if (string.IsNullOrEmpty(propertyName) || instance == null)
            {
                return null;
            }

            object value = null;

            Type type = instance.GetType();
            int indexOfDot = propertyName.IndexOf(ASCII.DOT);
            if (indexOfDot == -1 && string.Compare(propertyName, type.Name, true) == 0)
            {
                value = instance;
            }
            else
            {
                object parentInstance;

                PropertyInfo pInfo = GetPropertyInfo(instance, propertyName, out parentInstance);


                if (pInfo != null && pInfo.CanRead)
                    value = pInfo.GetValue(parentInstance, null);
            }

            return value;
        }

        /// <summary>
        /// Sets the property newValue to object.
        /// </summary>
        /// <param name="instance">Entity Object</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="newValue">The value.</param>
        ///  <remarks>It support nested property.</remarks>
        /// <example>
        /// <code>
        /// Customer customer = new Customer();
        /// customer.Id = 30085132;
        /// customer.Address = new Address();
        /// customer.Address.Id = 888;
        /// 
        /// ReflectionHelper.SetPropertyValueToObject(customer, "Customer.Id", 30085133);
        /// ReflectionHelper.SetPropertyValueToObject(customer, "Customer.Address.Id", 999);
        /// 
        /// Console.WriteLine("New Customer Id is: " + customer.Id.ToString());
        /// Console.WriteLine("New Address Id is: " + customer.Address.Id.ToString());
        /// </code>
        /// </example>
        public static void SetPropertyValueToObject(object instance, string propertyName, object newValue)
        {
            // If the parameter is not valid, exit this method
            if (string.IsNullOrEmpty(propertyName) || instance == null)
            {
                return;
            }

            if (!IsInstanceTypeName(instance, propertyName))
            {
                object parentInstance;
                PropertyInfo pInfo = GetPropertyInfo(instance, propertyName, out parentInstance);

                SetPropertyValueToObject(parentInstance, pInfo, newValue);
            }
        }

        /// <summary>
        /// Sets the property value to object.
        /// If you know the type, you can use the other overload method to specify the conversion to false;
        /// </summary>
        /// <param name="parentInstance">The parent instance.</param>
        /// <param name="pInfo">The p info.</param>
        /// <param name="newValue">The new value.</param>
        public static void SetPropertyValueToObject(object parentInstance, PropertyInfo pInfo, object newValue)
        {
            SetPropertyValueToObject(parentInstance, pInfo, newValue, true);
        }

        /// <summary>
        /// Sets the property value to object.
        /// </summary>
        /// <param name="parentInstance">The parent instance.</param>
        /// <param name="pInfo">The p info.</param>
        /// <param name="newValue">The new value.</param>
        public static void SetPropertyValueToObject(object parentInstance, PropertyInfo pInfo, object newValue, bool conversion)
        {
            if (pInfo != null && parentInstance != null)
            {
                object correctedValue = null;

                if (conversion)
                {
                    string strNewValue = newValue == null ? string.Empty : Convert.ToString(newValue);

                    if (pInfo.PropertyType == typeof(string))
                    {
                        // If the type is string, let it go. :)
                        correctedValue = strNewValue;
                    }
                    else if (strNewValue.Length != 0)
                    {
                        // Convert to corrected type according to the Property information & it can support nullable
                        // Only if the value is not a empty string
                        correctedValue = ChangeType(newValue, pInfo.PropertyType);
                    }
                }
                else
                {
                    correctedValue = newValue;
                }

                if (pInfo.CanWrite)
                {
                    // If the type of parameter info is value type, after the following code, the value will become default value of "ValueType" with the null incoming value.
                    // e.g. If the type is int, the finally value will be 0; If the type is DateTime, then will be DateTime.MinValue.
                    pInfo.SetValue(parentInstance, correctedValue, null);
                }
            }
        }

        /// <summary>
        /// Gets the property info.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="parentInstance">The parent instance.</param>
        /// <returns>The PropertyInfo</returns>
        public static PropertyInfo GetPropertyInfo(object instance, string propertyName, out object parentInstance)
        {
            parentInstance = instance;

            // If the parameter is not valid, exit this method
            if (string.IsNullOrEmpty(propertyName) || instance == null)
            {
                return null;
            }

            PropertyInfo rInfo = null;

            Type type = instance.GetType();
            propertyName = StandardizePropertyName(instance.GetType(), propertyName);
            int indexOfDot = propertyName.IndexOf(ASCII.DOT);

            if (0 < indexOfDot)
            {
                // Get the sub-property name and sub type name
                string subPropertyName = propertyName.Substring(indexOfDot).TrimStart(new char[1] { '.' }).Trim();
                string subTypeName = propertyName.Substring(0, indexOfDot);
                PropertyInfo pSub = type.GetProperty(subTypeName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);

                if (pSub != null)
                {
                    object subPropertyValue = pSub.GetValue(instance, null);
                    rInfo = GetPropertyInfo(subPropertyValue, subPropertyName, out parentInstance);
                }
            }
            else
            {
                // If it is not a nested property, get the value using reflect tech.
                rInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);
            }

            return rInfo;
        }

        /// <summary>
        /// Gets the declared only property info.
        /// </summary>
        /// <param name="baseType">Type of the base.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Property info</returns>
        /// <remarks>It does not support to retrieve the property info with the path.</remarks>
        public static PropertyInfo GetDeclaredOnlyPropertyInfo(Type baseType, string propertyName)
        {
            if (baseType == null || string.IsNullOrEmpty(propertyName)) return null;
            if (0 < propertyName.IndexOf(".")) throw new ArgumentException("There is invalid char like \".\" existed in the property name");

            PropertyInfo pInfo = null;
            while (baseType != null)
            {
                pInfo = baseType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
                if (pInfo != null) break;

                baseType = baseType.BaseType;
            }

            return pInfo;
        }

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo = null;
            string[] propertyLevels = propertyName.Split('.');
            int startIndex = 0;
            if (type.Name == propertyLevels[0])
            {
                startIndex = 1;
            }
            for (int i = startIndex; i < propertyLevels.Length; ++i)
            {
                propInfo = type.GetProperty(propertyLevels[i], BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null)
                {
                    break;
                }
                else
                {
                    type = propInfo.PropertyType;
                }
            }

            return propInfo;
        }

        /// <summary>
        /// Standardizes the name of the property.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Standardized propertyName</returns>
        /// <remarks>
        /// 1. Trim the propertyName<br>
        /// 2. Remove the type name if it is its prefix
        /// </remarks>
        private static string StandardizePropertyName(Type type, string propertyName)
        {
            if (type != null && !string.IsNullOrEmpty(propertyName))
            {
                int indexOfDot = propertyName.IndexOf(ASCII.DOT);

                // Remove the space from the property name
                propertyName = propertyName.Trim();

                // Assert the class name of the propertyName is the same as the instance's type name
                // If not, use the name then
                // else remove the class name and the dot.
                if (indexOfDot != -1)
                {
                    if (propertyName.StartsWith(type.Name, StringComparison.OrdinalIgnoreCase))
                        propertyName = propertyName.Substring(indexOfDot).TrimStart(new char[1] { '.' }).Trim();
                }
            }

            return propertyName;
        }

        #endregion

        /// <summary>
        /// Returns an Object with the specified Type and whose value is equivalent to the specified object.
        /// </summary>
        /// <param name="value">An Object that implements the IConvertible interface.</param>
        /// <param name="conversionType">The Type to which value is to be converted.</param>
        /// <returns>An object whose Type is conversionType (or conversionType's underlying type if conversionType
        /// is Nullable&lt;&gt;) and whose value is equivalent to value. -or- a null reference, if value is a null
        /// reference and conversionType is not a value type.</returns>
        /// <remarks>
        /// This method exists as a workaround to System.Convert.ChangeType(Object, Type) which does not handle
        /// nullables as of version 2.0 (2.0.50727.42) of the .NET Framework. The idea is that this method will
        /// be deleted once Convert.ChangeType is updated in a future version of the .NET Framework to handle
        /// nullable types, so we want this to behave as closely to Convert.ChangeType as possible.
        /// </remarks>
        public static object ChangeType(object value, Type conversionType)
        {
            // This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
              conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                // It's a nullable type, so instead of calling Convert.ChangeType directly which would throw a
                // InvalidCastException,
                // determine what the underlying type is
                // If it's null, it won't convert to the underlying type, but that's fine since nulls don't really
                // have a type--so just return null
                //  We only do this check if we're converting to a nullable type, since doing it outside
                // would diverge from Convert.ChangeType's behavior, which throws an InvalidCastException if
                // value is null and conversionType is a value type.
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } // end if

            //EnumType Support
            if (conversionType.IsEnum)
            {
                if (value is System.String || value is System.Int32)
                    return System.Enum.Parse(conversionType, value.ToString());
            }

            // If the value is null, return null directly
            if (conversionType.IsValueType && (value == null || string.IsNullOrEmpty(value.ToString())))
            {
                return Activator.CreateInstance(conversionType);
            }

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            object result = Convert.ChangeType(value, conversionType);

            return result;
        }

        /// <summary>
        /// Changes the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T ChangeType<T>(object value)
        {
            object returnValue = ChangeType(value, typeof(T));

            if (returnValue == null)
            {
                return default(T);
            }

            return (T)returnValue;
        }

        /// <summary>
        /// Determines whether [is valid method parameter value] [the specified method info].
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="sortedParameters">The sorted parameters.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid method parameter value] [the specified method info]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidMethodParameterValue(MethodInfo methodInfo, object[] sortedParameters)
        {
            bool isValid = true;

            if (methodInfo != null && sortedParameters != null)
            {
                ParameterInfo[] pInfos = methodInfo.GetParameters();
                if (pInfos.Length != sortedParameters.Length)
                {
                    isValid = false;
                }
                else
                {
                    for (int pIndex = 0; pIndex < pInfos.Length; pIndex++)
                    {
                        if (pInfos[pIndex].ParameterType.IsValueType && (sortedParameters[pIndex] == null || sortedParameters[pIndex].ToString().Trim().Length == 0))
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Fills the object with parameters.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        public static void FillObjectWithParameters(object obj, Type type, Dictionary<string, object> parameters)
        {
            if (obj != null && parameters != null)
            {
                if (type == null)
                    type = obj.GetType();
                foreach (string property in parameters.Keys)
                {
                    PropertyInfo pi = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (pi != null && pi.CanWrite)
                    {
                        object value = parameters[property];
                        try
                        {
                            value = ChangeType(value, pi.PropertyType);
                            pi.SetValue(obj, value, null);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts the object to param dictionary.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertObjectToParamDictionary(object entity)
        {
            if (null == entity) { return null; }
            if (entity is Dictionary<string, object>) { return entity as Dictionary<string, object>; }

            var entityType = entity.GetType();
            if (entityType.IsValueType) { return null; }

            var paramDictionary = new Dictionary<string, object>();
            var propertyInfos = entityType.GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                var value = propertyInfo.GetValue(entity, null);
                paramDictionary.Add(propertyInfo.Name, value);
            }

            return paramDictionary;
        }

        /// <summary>
        /// Converts the object to param dictionary.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Dictionary<string, object> ConvertObjectToParamDictionary(object obj, Type type)
        {
            Dictionary<string, object> paramDictionary = null;
            if (obj != null)
            {
                if (type == null)
                {
                    paramDictionary = obj as Dictionary<string, object>;
                }
                else
                {
                    paramDictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    PropertyInfo[] pis = type.GetProperties();
                    if (pis != null)
                    {
                        foreach (PropertyInfo pi in pis)
                        {
                            object value = pi.GetValue(obj, null);
                            paramDictionary.Add(pi.Name, value);
                        }
                    }
                }
            }

            return paramDictionary;
        }

        /// <summary>
        /// Determines whether [is instance type name] [the specified instance].
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// 	<c>true</c> if [is instance type name] [the specified instance]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInstanceTypeName(object instance, string propertyName)
        {
            bool returnValue = false;

            if (instance != null && !string.IsNullOrEmpty(propertyName))
            {
                Type type = instance.GetType();
                int indexOfDot = propertyName.IndexOf(".");
                if (indexOfDot == -1)
                {
                    if (string.Compare(type.Name, propertyName.Trim(), true) == 0)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the embedded resource.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResource(string assemblyName, string resourceName)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(resourceName))
                return null;

            Assembly assembly = Assembly.Load(assemblyName);

            return GetEmbeddedResource(assembly, resourceName);
        }

        /// <summary>
        /// Gets the embedded resource.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResource(Assembly assembly, string resourceName)
        {
            if (assembly == null || string.IsNullOrEmpty(resourceName))
                return null;

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }


        /// <summary>
        /// Finds a generic type in definition of <paramref name="targetBase"/> which the given <paramref name="leafType"/> inherits from.
        /// </summary>
        /// <param name="leafType">The leaf type to search from.</param>
        /// <param name="targetBase"></param>
        /// <returns></returns>
        public static Type FindBaseGenericType(Type leafType, Type targetBase)
        {
            if (!targetBase.IsGenericTypeDefinition)
            {
                throw new ArgumentException("TargetBase must be generic definition.");
            }

            Type ret = null;
            while (leafType != null)
            {
                if (TypeIsOfGenericDefinition(leafType, targetBase))
                {
                    ret = leafType;
                    break;
                }
                leafType = leafType.BaseType;
            }
            return ret;
        }

        public static Type FindInheritanceFrom(Type searchFrom, Type target)
        {
            Type ret = null;
            if ((searchFrom != null) && (target != null))
            {
                if (target.IsGenericTypeDefinition)
                {
                    if (target.IsInterface)
                    {
                        if (TypeIsOfGenericDefinition(searchFrom, target))
                        {
                            ret = searchFrom;
                        }
                        else
                        {
                            ret = FindInheritanceGenericInterfaceFrom(searchFrom, target);
                        }
                    }
                    else
                    {
                        ret = FindBaseGenericType(searchFrom, target);
                    }
                }
                else
                {
                    if (target.IsAssignableFrom(searchFrom))
                    {
                        ret = target;
                    }
                }
            }

            return ret;
        }

        private static Type FindInheritanceGenericInterfaceFrom(Type searchFrom, Type target)
        {
            Type ret = null;

            Type[] ifcs = searchFrom.GetInterfaces();
            foreach (Type ifc in ifcs)
            {
                if (TypeIsOfGenericDefinition(ifc, target))
                {
                    ret = ifc;
                    break;
                }
            }
            if ((ret == null) && !searchFrom.IsInterface)
            {
                ret = FindInheritanceFrom(searchFrom.BaseType, target);
            }
            if (ret == null)
            {
                foreach (Type ifc in ifcs)
                {
                    ret = FindInheritanceFrom(ifc, target);
                }
            }

            return ret;
        }

        private static bool TypeIsOfGenericDefinition(Type realization, Type definition)
        {
            return realization.IsGenericType && realization.GetGenericTypeDefinition().Equals(definition);
        }


        /// <summary>
        /// Copies the value with same property.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDest">The type of the dest.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static TDest CopyValueWithSameProperty<TSource, TDest>(TSource source, TDest destination)
        {
            return (TDest)CopyValueWithSameProperty(source, destination, false);
        }

        public static object CopyValueWithSameProperty(object source, object destination, bool ignoreType)
        {
            if (source == null) return destination;

            Type tpSource = source.GetType();
            Type tpDestination = destination.GetType();

            if (destination == null) destination = Activator.CreateInstance(tpDestination);

            PropertyInfo[] sourcePropertyInfos = tpSource.GetProperties();
            PropertyInfo[] destinationPropertyInfos = tpDestination.GetProperties();
            Dictionary<string, PropertyInfo> dicDestPropertyInfo = new Dictionary<string, PropertyInfo>();

            for (int destPropertyInfosIndex = 0; destPropertyInfosIndex < destinationPropertyInfos.Length; destPropertyInfosIndex++)
            {
                dicDestPropertyInfo.Add(destinationPropertyInfos[destPropertyInfosIndex].Name, destinationPropertyInfos[destPropertyInfosIndex]);
            }

            for (int sourcePropertyInfosIndex = 0; sourcePropertyInfosIndex < sourcePropertyInfos.Length; sourcePropertyInfosIndex++)
            {
                string pName = sourcePropertyInfos[sourcePropertyInfosIndex].Name;
                PropertyInfo sourcePropertyInfo = sourcePropertyInfos[sourcePropertyInfosIndex];
                PropertyInfo destPropertyInfo = dicDestPropertyInfo[pName];

                if (sourcePropertyInfo.PropertyType.IsArray && sourcePropertyInfo.PropertyType != destPropertyInfo.PropertyType) continue;

                if (dicDestPropertyInfo.ContainsKey(pName) && destPropertyInfo.CanWrite)
                {
                    if (sourcePropertyInfo.CanRead
                        && sourcePropertyInfo.GetIndexParameters().Length == 0) // Skip the property with the parameters 
                    {
                        Object newValue = sourcePropertyInfo.GetValue(source, null);

                        if (ignoreType)
                        {
                            if (sourcePropertyInfo.PropertyType != destPropertyInfo.PropertyType)
                            {
                                object tempDestPropertyValue = destPropertyInfo.GetValue(destination, null);

                                if (tempDestPropertyValue == null) tempDestPropertyValue = Activator.CreateInstance(destPropertyInfo.PropertyType);

                                newValue = CopyValueWithSameProperty(newValue, tempDestPropertyValue, ignoreType);
                            }

                            destPropertyInfo.SetValue(destination, newValue, null);
                        }
                        else
                        {
                            if (sourcePropertyInfo.PropertyType == destPropertyInfo.PropertyType)
                            {
                                destPropertyInfo.SetValue(destination, newValue, null);
                            }
                        }

                    }
                }
            }

            return destination;
        }
    }
}
