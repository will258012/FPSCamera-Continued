﻿using HarmonyLib;
using System;

namespace FPSCamera.Utils
{
    public static class AccessUtils
    {
        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            var fieldInfo = AccessTools.Field(obj.GetType(), fieldName);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in type '{obj.GetType().FullName}'.");
            }
            return (T)fieldInfo.GetValue(obj);
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            var fieldInfo = AccessTools.Field(obj.GetType(), fieldName);
            if (fieldInfo == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in type '{obj.GetType().FullName}'.");
            }
            fieldInfo.SetValue(obj, value);
        }
        public static T GetPropertyValue<T>(object obj, string propertyName, object[] index = null)
        {
            var propertyInfo = AccessTools.Property(obj.GetType(), propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found in type '{obj.GetType().FullName}'.");
            }
            return (T)propertyInfo.GetValue(obj, index);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value, object[] index = null)
        {
            var propertyInfo = AccessTools.Property(obj.GetType(), propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found in type '{obj.GetType().FullName}'.");
            }
            propertyInfo.SetValue(obj, value, index);
        }
        public static object InvokeMethod(string typeName, string methodName, object[] parameters, Type[] paramTypes = null)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                var method = AccessTools.Method(type, methodName, paramTypes);
                if (method != null)
                {
                    return method.Invoke(null, parameters);
                }
                else
                {
                    throw new ArgumentException($"Method '{methodName}' not found.");
                }
            }
            else
            {
                throw new ArgumentException($"Class '{typeName}' not found.");
            }
        }
    }
}
