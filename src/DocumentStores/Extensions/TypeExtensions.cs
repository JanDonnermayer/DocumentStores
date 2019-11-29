using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    internal static class TypeExtensions
    {

        /// <summary>
        /// Returns an C#-style string representation of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="t"></param>
        /// <param name="trimArgCount"></param>
        /// <returns></returns>
        public static string ShortName(this Type t, bool trimArgCount)
        {
            if (t.IsGenericType)
            {
                var genericArgs = t.GetGenericArguments();

                return ShortName(t, trimArgCount, genericArgs);
            }

            return t.Name;
        }

        private static string ShortName(Type t, bool trimArgCount, IEnumerable<Type> availableArguments)
        {
            if (t.IsGenericType)
            {
                string value = t.Name;
                if (trimArgCount && value.IndexOf("`") > -1)
                    value = value.Substring(0, value.IndexOf("`"));

                if (t.DeclaringType != null)
                    // This is a nested type, build the nesting type first
                    value = ShortName(t.DeclaringType, trimArgCount, availableArguments) + "+" + value;

                // Build the type arguments (if any)
                string argString = "";
                var thisTypeArgs = t.GetGenericArguments();
                for (int i = 0; i < thisTypeArgs.Length && availableArguments.Any(); i++)
                {
                    if (i != 0) argString += ", ";

                    argString += availableArguments.First().ShortName(trimArgCount);
                    availableArguments = availableArguments.Skip(1);
                }

                // If there are type arguments, add them with < >
                if (argString.Length > 0)
                    value += "<" + argString + ">";

                return value;
            }

            return t.Name;
        }

    }
}


