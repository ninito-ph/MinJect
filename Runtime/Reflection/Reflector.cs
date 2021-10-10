using System;
using System.Collections.Generic;
using System.Reflection;
using Ninito.MinJect.Injection;
using UnityEngine.Assertions;

namespace Ninito.MinJect.Reflection
{
    /// <summary>
    /// A reflector that obtains fields marked for injection
    /// </summary>
    public static class Reflector
    {
        #region Private Fields

        private static readonly Type _injectAttributeType = typeof(InjectField);

        private static readonly Dictionary<Type, FieldInfo[]> cachedFieldInfos =
            new Dictionary<Type, FieldInfo[]>();

        private static readonly List<FieldInfo> _reusableList = new List<FieldInfo>(1024);

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the InjectField fields of the given type
        /// </summary>
        /// <param name="type">The type to get injected fields of</param>
        /// <returns>The fields marked for injection in the given type</returns>
        public static FieldInfo[] GetInjectableFieldsOf(Type type)
        {
            Assert.AreEqual(0, _reusableList.Count, "Reusable list in Reflector was not empty!");

            if (IsThereCacheFor(type, out FieldInfo[] cachedResult))
            {
                return cachedResult;
            }

            FieldInfo[] fields = GetRelevantFieldsOf(type);

            AddInjectableFields(fields);

            FieldInfo[] resultAsArray = _reusableList.ToArray();
            CacheResults(type, resultAsArray);
            return resultAsArray;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Caches the given results to the given key
        /// </summary>
        /// <param name="cacheKey">The key to store the cache in</param>
        /// <param name="resultAsArray">The results to cache</param>
        private static void CacheResults(Type cacheKey, FieldInfo[] resultAsArray)
        {
            _reusableList.Clear();
            cachedFieldInfos[cacheKey] = resultAsArray;
        }

        /// <summary>
        /// Gets relevant fields out of a type
        /// </summary>
        /// <param name="type">The type to extract fields from</param>
        /// <returns>The relevant field from the given type</returns>
        private static FieldInfo[] GetRelevantFieldsOf(IReflect type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                  BindingFlags.FlattenHierarchy);
        }

        /// <summary>
        /// Adds injectable fields to the reusable list
        /// </summary>
        /// <param name="fields">The list of fields to add</param>
        private static void AddInjectableFields(IReadOnlyList<FieldInfo> fields)
        {
            for (int fieldIndex = 0, fieldLenght = fields.Count; fieldIndex < fieldLenght; fieldIndex++)
            {
                if (!HasInjectAttribute(fields[fieldIndex])) continue;
                _reusableList.Add(fields[fieldIndex]);
            }
        }

        /// <summary>
        /// Checks whether the given field has the InjectField attribute
        /// </summary>
        /// <param name="field">The field to check</param>
        /// <returns>Whether the given field has the InjectField attribute</returns>
        private static bool HasInjectAttribute(ICustomAttributeProvider field)
        {
            return field.IsDefined(_injectAttributeType, false);
        }

        /// <summary>
        /// Checks whether a cache exists for a given type, and returns it if it does
        /// </summary>
        /// <param name="type">The type to check for a cache</param>
        /// <param name="cachedResult">The cache for the given type</param>
        /// <returns>Whether a cache exists for a given type</returns>
        private static bool IsThereCacheFor(Type type, out FieldInfo[] cachedResult)
        {
            return cachedFieldInfos.TryGetValue(type, out cachedResult);
        }

        #endregion
    }
}