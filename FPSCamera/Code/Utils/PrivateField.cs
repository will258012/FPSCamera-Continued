using System;
using System.Reflection;

namespace FPSCamera.Utils
{
    public static class PrivateField
    {
        public static T GetValue<T>(object obj, string fieldName)
        {
            var type = obj.GetType();
            var fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentException($"Field '{fieldName}' not found in type '{type.FullName}'.");
            return (T)fieldInfo.GetValue(obj);
        }

        public static void SetValue(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            var fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentException($"Field '{fieldName}' not found in type '{type.FullName}'.");
            fieldInfo.SetValue(obj, value);
        }
    }
}
