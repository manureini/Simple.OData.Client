using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Simple.OData.Client
{
    /// <copydoc cref="ITypeCache" />
    public class TypeCache : ITypeCache
    {
        private readonly ConcurrentDictionary<Type, TypeCacheResolver> _cache;

        /// <summary>
        /// Creates a new instance of the <see cref="TypeCache"/> class.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="nameMatchResolver"></param>
        public TypeCache(ITypeConverter converter, INameMatchResolver nameMatchResolver)
        {
            _cache = new ConcurrentDictionary<Type, TypeCacheResolver>();
            NameMatchResolver = nameMatchResolver ?? ODataNameMatchResolver.Strict;
            Converter = converter;
        }

        /// <copydoc cref="ITypeCache.Converter" />
        public ITypeConverter Converter { get; }

        public INameMatchResolver NameMatchResolver { get; }

        /// <copydoc cref="ITypeCache.Register{T}" />
        public void Register<T>(string dynamicContainerName = "DynamicProperties")
        {
            Register(typeof(T), dynamicContainerName);
        }

        /// <copydoc cref="ITypeCache.Register" />
        public void Register(Type type, string dynamicContainerName = "DynamicProperties")
        {
            InternalRegister(type, true, dynamicContainerName);

            foreach (var subType in GetDerivedTypes(type))
            {
                InternalRegister(subType, true, dynamicContainerName);
            }
        }

        /// <copydoc cref="ITypeCache.IsDynamicType" />
        public bool IsDynamicType(Type type)
        {
            return Resolver(type).IsDynamicType;
        }

        /// <copydoc cref="ITypeCache.DynamicContainerName" />
        public string DynamicContainerName(Type type)
        {
            return Resolver(type).DynamicPropertiesName;
        }

        public PropertyInfo GetAnnotationsProperty(Type type)
        {
            return Resolver(type).AnnotationsProperty;
        }

        public IEnumerable<Type> GetDerivedTypes(Type type)
        {
            return Resolver(type).DerivedTypes;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperties" />
        public IEnumerable<PropertyInfo> GetMappedProperties(Type type)
        {
            return Resolver(type).MappedProperties;
        }

        /// <copydoc cref="ITypeCache.GetMappedPropertiesWithNames" />
        public IEnumerable<Tuple<PropertyInfo, string>> GetMappedPropertiesWithNames(Type type)
        {
            return Resolver(type).MappedPropertiesWithNames;
        }

        /// <copydoc cref="ITypeCache.GetMappedProperty" />
        public PropertyInfo GetMappedProperty(Type type, string propertyName)
        {
            return Resolver(type).GetMappedProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, PropertyInfo)" />
        public string GetMappedName(Type type, PropertyInfo propertyInfo)
        {
            return Resolver(type).GetMappedName(propertyInfo);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type, string)" />
        public string GetMappedName(Type type, string propertyName)
        {
            return Resolver(type).GetMappedName(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllProperties" />
        public IEnumerable<PropertyInfo> GetAllProperties(Type type)
        {
            return Resolver(type).AllProperties;
        }

        /// <copydoc cref="ITypeCache.GetNamedProperty(Type, string)" />
        public PropertyInfo GetNamedProperty(Type type, string propertyName)
        {
            return Resolver(type).GetAnyProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperties" />
        public IEnumerable<PropertyInfo> GetDeclaredProperties(Type type)
        {
            return Resolver(type).DeclaredProperties;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredProperty" />
        public PropertyInfo GetDeclaredProperty(Type type, string propertyName)
        {
            return Resolver(type).GetDeclaredProperty(propertyName);
        }

        /// <copydoc cref="ITypeCache.GetAllFields" />
        public IEnumerable<FieldInfo> GetAllFields(Type type)
        {
            return Resolver(type).AllFields;
        }

        /// <copydoc cref="ITypeCache.GetAnyField" />
        public FieldInfo GetAnyField(Type type, string fieldName, bool includeNonPublic = false)
        {
            return Resolver(type).GetAnyField(fieldName, includeNonPublic);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredFields" />
        public IEnumerable<FieldInfo> GetDeclaredFields(Type type)
        {
            return Resolver(type).DeclaredFields;
        }

        /// <copydoc cref="ITypeCache.GetDeclaredField" />
        public FieldInfo GetDeclaredField(Type type, string fieldName)
        {
            return Resolver(type).GetDeclaredField(fieldName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredMethod" />
        public MethodInfo GetDeclaredMethod(Type type, string methodName)
        {
            return Resolver(type).GetDeclaredMethod(methodName);
        }

        /// <copydoc cref="ITypeCache.GetDeclaredConstructors" />
        public IEnumerable<ConstructorInfo> GetDeclaredConstructors(Type type)
        {
            return Resolver(type).GetDeclaredConstructors();
        }

        /// <copydoc cref="ITypeCache.GetDefaultConstructor" />
        public ConstructorInfo GetDefaultConstructor(Type type)
        {
            return Resolver(type).GetDefaultConstructor();
        }

        /// <copydoc cref="ITypeCache.GetTypeAttributes" />
        public TypeAttributes GetTypeAttributes(Type type)
        {
            return Resolver(type).GetTypeAttributes();
        }

        /// <copydoc cref="ITypeCache.GetGenericTypeArguments" />
        public Type[] GetGenericTypeArguments(Type type)
        {
            return Resolver(type).GetGenericTypeArguments();
        }

        /// <copydoc cref="ITypeCache.IsAnonymousType" />
        public bool IsAnonymousType(Type type)
        {
            return Resolver(type).IsAnonymousType;
        }

        /// <copydoc cref="ITypeCache.IsGeneric" />
        public bool IsGeneric(Type type)
        {
            return Resolver(type).IsGeneric;
        }

        /// <copydoc cref="ITypeCache.IsValue" />
        public bool IsValue(Type type)
        {
            return Resolver(type).IsValue;
        }

        /// <copydoc cref="ITypeCache.IsEnumType" />
        public bool IsEnumType(Type type)
        {
            return Resolver(type).IsEnumType;
        }

        /// <copydoc cref="ITypeCache.GetBaseType" />
        public Type GetBaseType(Type type)
        {
            return Resolver(type).BaseType;
        }

        /// <copydoc cref="ITypeCache.IsTypeAssignableFrom" />
        public bool IsTypeAssignableFrom(Type type, Type otherType)
        {
            return Resolver(type).IsTypeAssignableFrom(otherType);
        }

        /// <copydoc cref="ITypeCache.HasCustomAttribute" />
        public bool HasCustomAttribute(Type type, Type attributeType, bool inherit)
        {
            return Resolver(type).HasCustomAttribute(attributeType, inherit);
        }

        /// <copydoc cref="ITypeCache.GetMappedName(Type)" />
        public string GetMappedName(Type type)
        {
            return Resolver(type).MappedName;
        }

        public bool TryConvert(object value, Type targetType, out object result)
        {
            try
            {
                if (value == null)
                {
                    if (this.IsValue(targetType))
                        result = Activator.CreateInstance(targetType);
                    else
                        result = null;
                }
                else if (this.IsTypeAssignableFrom(targetType, value.GetType()))
                {
                    result = value;
                }
                else if (targetType == typeof(string))
                {
                    result = value.ToString();
                }
                else if (this.IsEnumType(targetType) && value is string)
                {
                    result = Enum.Parse(targetType, value.ToString(), true);
                }
                else if (targetType == typeof(byte[]) && value is string)
                {
                    result = System.Convert.FromBase64String(value.ToString());
                }
                else if (targetType == typeof(string) && value is byte[] bytes)
                {
                    result = System.Convert.ToBase64String(bytes);
                }
                else if ((targetType == typeof(DateTime) || targetType == typeof(DateTime?)) && value is DateTimeOffset offset)
                {
                    result = offset.Offset == default ? offset.UtcDateTime : offset.DateTime;
                }
                else if ((targetType == typeof(DateTime) || targetType == typeof(DateTime?)) && ImplicitConversionTo<DateTime>(value) is MethodInfo implicitMethod)
                {
                    result = (DateTime)implicitMethod.Invoke(value, new object[] { value });
                }
                else if ((targetType == typeof(DateTimeOffset) || targetType == typeof(DateTimeOffset?)) && value is DateTime time)
                {
                    if (time.ToUniversalTime() <= DateTimeOffset.MinValue.UtcDateTime)
                    {
                        result = DateTimeOffset.MinValue;
                    }
                    else if (time.ToUniversalTime() >= DateTimeOffset.MaxValue.UtcDateTime)
                    {
                        result = DateTimeOffset.MaxValue;
                    }
                    else
                    {
                        result = new DateTimeOffset(time);
                    }
                }
                else if (targetType == typeof(DateOnly) && value is Microsoft.OData.Edm.Date dt)
                {
                    result = new DateOnly(dt.Year, dt.Month, dt.Day);
                }
                else if (targetType == typeof(TimeOnly) && value is Microsoft.OData.Edm.TimeOfDay dt2)
                {
                    result = new TimeOnly(dt2.Hours, dt2.Minutes, dt2.Seconds, (int)dt2.Milliseconds);
                }
                else if (targetType == typeof(Microsoft.OData.Edm.Date) && value is DateOnly dateOnly)
                {
                    result = new Microsoft.OData.Edm.Date(dateOnly.Year, dateOnly.Month, dateOnly.Day);
                }
                else if (targetType == typeof(Microsoft.OData.Edm.TimeOfDay) && value is TimeOnly timeOnly)
                {
                    result = new Microsoft.OData.Edm.TimeOfDay(timeOnly.Hour, timeOnly.Minute, timeOnly.Second, timeOnly.Millisecond);
                }
                else if (this.IsEnumType(targetType))
                {
                    result = Enum.ToObject(targetType, value);
                }
                else if (targetType == typeof(Guid) && value is string)
                {
                    result = new Guid(value.ToString());
                }
                else if (Nullable.GetUnderlyingType(targetType) != null)
                {
                    result = Convert(value, Nullable.GetUnderlyingType(targetType));
                }
                else if (Converter.HasObjectConverter(targetType))
                {
                    result = Converter.Convert(value, targetType);
                }
                else
                {
                    var descriptor = TypeDescriptor.GetConverter(targetType);
                    result = descriptor != null & descriptor.CanConvertTo(targetType)
                        ? descriptor.ConvertTo(value, targetType)
                        : System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        public object Convert(object value, Type targetType)
        {
            if (value == null && !this.IsValue(targetType))
                return null;
            else if (TryConvert(value, targetType, out var result))
                return result;

            throw new FormatException($"Unable to convert value from type {value.GetType()} to type {targetType}");
        }

        private TypeCacheResolver Resolver(Type type)
        {
            var resolver = _cache.GetOrAdd(type, x => InternalRegister(x));

            return resolver;
        }

        private TypeCacheResolver InternalRegister(Type type, bool dynamicType = false, string dynamicContainerName = null)
        {
            var resolver = new TypeCacheResolver(type, NameMatchResolver, dynamicType, dynamicContainerName);

            _cache[type] = resolver;

            return resolver;
        }

        private MethodInfo ImplicitConversionTo<T>(object value)
            => value.GetType().GetMethods()
                    .FirstOrDefault(m => string.Equals(m.Name, "op_Implicit")
                                      && m.ReturnType == typeof(T));
    }
}
