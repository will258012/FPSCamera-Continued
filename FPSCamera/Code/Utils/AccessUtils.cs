using HarmonyLib;
using System;

namespace FPSCamera.Utils
{
    public static class AccessUtils
    {
        /// <summary>
        /// Indicates that the attributed code element is accessed via reflection.
        /// </summary>
        /// <remarks>
        /// Reflection has a performance cost.
        /// Mark code with this attribute when reflection is used.
        /// <strong>Remember to verify compatibility after game or mod updates, 
        /// or consider using alternative ways to invoke the target code.</strong>
        /// </remarks>
        [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
        [Obsolete("This code uses reflection. Remember to verify compatibility after game/mod updates.")]
        public sealed class UsedReflectionAttribute : Attribute;
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            var fieldInfo = AccessTools.Field(obj.GetType(), fieldName) ?? throw new ArgumentException($"Field '{fieldName}' not found in type '{obj.GetType().FullName}'.");
            return (T)fieldInfo.GetValue(obj);
        }
        public static T GetStaticFieldValue<T>(Type type, string fieldName)
        {
            var fieldInfo = AccessTools.Field(type, fieldName) ?? throw new ArgumentException($"Field '{fieldName}' not found in type '{type.FullName}'.");
            return (T)fieldInfo.GetValue(null);
        }
        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            var fieldInfo = AccessTools.Field(obj.GetType(), fieldName) ?? throw new ArgumentException($"Field '{fieldName}' not found in type '{obj.GetType().FullName}'.");
            fieldInfo.SetValue(obj, value);
        }
        public static T GetPropertyValue<T>(object obj, string propertyName, object[] index = null)
        {
            var propertyInfo = AccessTools.Property(obj.GetType(), propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found in type '{obj.GetType().FullName}'.");
            return (T)propertyInfo.GetValue(obj, index);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value, object[] index = null)
        {
            var propertyInfo = AccessTools.Property(obj.GetType(), propertyName) ?? throw new ArgumentException($"Property '{propertyName}' not found in type '{obj.GetType().FullName}'.");
            propertyInfo.SetValue(obj, value, index);
        }
        public static object InvokeMethod(string typeName, string methodName, object[] parameters, Type[] paramTypes = null, object obj = null)
        {
            var type = Type.GetType(typeName) ?? throw new ArgumentException($"Class '{typeName}' not found.");
            var methodInfo = AccessTools.Method(type, methodName, paramTypes) ?? throw new ArgumentException($"Method '{methodName}' not found.");
            return methodInfo.Invoke(obj, parameters);
        }
    }
}
