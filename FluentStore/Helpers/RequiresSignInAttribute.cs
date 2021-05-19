using System;

namespace FluentStore.Helpers
{
    /// <summary>
    /// Marks the target as requiring user authentication
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class RequiresSignInAttribute : Attribute
    {
        public static bool IsPresent(object obj) => IsPresent(obj.GetType());
        public static bool IsPresent(Type type)
        {
            return type.GetCustomAttributes(typeof(RequiresSignInAttribute), false).Length > 0;
        }
    }
}
