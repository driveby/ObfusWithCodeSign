using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObfusWithSignTool
{
    public static class StringExtension
    {
        /// <summary>
        /// true : 문자열이 비어있는 경우, false : 문자열이 비어있지 않은 경우 
        /// Indicates whether the specified string is null or an System.String.Empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true : 문자열이 비어있는 경우, false : 문자열이 비어있지 않은 경우</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
                return true;
            else return false;
        }

        /// <summary>
        /// true : 문자열이 비어있지 않은 경우, false : 문자열이 비어있는 경우 
        /// Indicates whether the specified string is NotNull
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true : 문자열이 비어있지 않은 경우, false : 문자열이 비어있는 경우</returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !IsNullOrEmpty(value);
        }
    }
}
