using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Utils
{
    public class ConvertUtil
    {
        /// <summary>
        /// Deserializes a JSON string to an object of the specified type
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="jsonString">The JSON string to deserialize</param>
        /// <returns>Deserialized object of type T, or null if deserialization fails</returns>
        public static T? DeserializeObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        #region Safe Conversion Methods

        /// <summary>
        /// Safely converts a value to int32, returns 0 if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted int32 value or 0 if conversion fails</returns>
        public static int ToInt32Safe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (int.TryParse(value.ToString(), out int result))
                return result;

            return 0;
        }

        /// <summary>
        /// Safely converts a value to decimal, returns 0 if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted decimal value or 0 if conversion fails</returns>
        public static decimal ToDecimalSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (decimal.TryParse(value.ToString(), out decimal result))
                return result;

            return 0;
        }

        /// <summary>
        /// Safely converts a value to DateTime, returns DateTime.MinValue if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted DateTime value or DateTime.MinValue if conversion fails</returns>
        public static DateTime ToDateTimeSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return DateTime.MinValue;

            if (DateTime.TryParse(value.ToString(), out DateTime result))
                return result;

            return DateTime.MinValue;
        }

        /// <summary>
        /// Safely converts a value to boolean, returns false if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted boolean value or false if conversion fails</returns>
        public static bool ToBoolSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;

            if (bool.TryParse(value.ToString(), out bool result))
                return result;

            return false;
        }

        /// <summary>
        /// Safely converts a value to Guid, returns Guid.Empty if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted Guid value or Guid.Empty if conversion fails</returns>
        public static Guid ToGuidSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return Guid.Empty;

            if (Guid.TryParse(value.ToString(), out Guid result))
                return result;

            return Guid.Empty;
        }

        /// <summary>
        /// Safely converts a value to long, returns 0 if conversion fails
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted long value or 0 if conversion fails</returns>
        public static long ToLongSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            if (long.TryParse(value.ToString(), out long result))
                return result;

            return 0;
        }

        #endregion

        #region String Conversion Methods

        /// <summary>
        /// Converts an object to string, returns empty string if null
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>String representation of the value or empty string if null</returns>
        public static string ToStringOrEmpty(object value)
        {
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Encodes a string to Base64
        /// </summary>
        /// <param name="plainText">The string to encode</param>
        /// <returns>Base64 encoded string</returns>
        public static string ToBase64String(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a Base64 string to plain text
        /// </summary>
        /// <param name="base64EncodedData">The Base64 encoded string</param>
        /// <returns>Decoded plain text string</returns>
        public static string FromBase64String(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
                return string.Empty;

            try
            {
                var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        #region Collection Conversion Methods

        /// <summary>
        /// Converts an IEnumerable to a List
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        /// <param name="source">The source collection</param>
        /// <returns>List of type T</returns>
        public static List<T> ToList<T>(IEnumerable<T> source)
        {
            return source?.ToList() ?? new List<T>();
        }

        /// <summary>
        /// Converts an object to a dictionary
        /// </summary>
        /// <param name="obj">The object to convert</param>
        /// <returns>Dictionary representation of the object</returns>
        public static Dictionary<string, object> ToDictionary(object obj)
        {
            var dictionary = new Dictionary<string, object>();
            if (obj == null)
                return dictionary;

            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                dictionary[property.Name] = property.GetValue(obj, null);
            }

            return dictionary;
        }

        #endregion

        #region DateTime Format Methods

        /// <summary>
        /// Formats a DateTime to string with specified pattern
        /// </summary>
        /// <param name="dateTime">The DateTime value</param>
        /// <param name="format">The format pattern (default: dd/MM/yyyy)</param>
        /// <returns>Formatted date string</returns>
        public static string ToDateTimeString(DateTime dateTime, string format = "dd/MM/yyyy")
        {
            if (dateTime == DateTime.MinValue)
                return string.Empty;

            return dateTime.ToString(format);
        }

        /// <summary>
        /// Parses a string to DateTime with multiple format support
        /// </summary>
        /// <param name="dateString">The date string to parse</param>
        /// <param name="formats">Array of acceptable date formats</param>
        /// <returns>Parsed DateTime or DateTime.MinValue if parsing fails</returns>
        public static DateTime ParseDateTime(string dateString, string[] formats = null)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            if (formats == null)
            {
                formats = new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };
            }

            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            if (DateTime.TryParse(dateString, out result))
                return result;

            return DateTime.MinValue;
        }
        #endregion
    }
}
