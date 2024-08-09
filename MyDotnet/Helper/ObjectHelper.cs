﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 对象扩展类
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum enumValue)
        {
            DisplayAttribute attributeOfType = enumValue.GetAttributeOfType<DisplayAttribute>();
            if (attributeOfType != null)
            {
                return attributeOfType.Name;
            }

            return enumValue.ToString();
        }

        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to retrieve.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>
        /// The attribute of the specified type or null.
        /// </returns>
        public static T GetAttributeOfType<T>(this Enum enumValue) where T : Attribute
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString()).First();
            var attributes = memInfo.GetCustomAttributes<T>(false);
            return attributes.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null) return 0;
            if (thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue"></param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            int reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        public static long ObjToLong(this object thisValue)
        {
            long reval = 0;
            if (thisValue == null) return 0;
            if (thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static double ObjToMoney(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return 0;
        }
 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool IsNotEmptyOrNull(this object thisValue)
        {
            return thisValue.ObjToString() != "" && thisValue.ObjToString() != "undefined" && thisValue.ObjToString() != "null";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue"></param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }

        public static bool IsNullOrEmpty(this object thisValue) => thisValue == null || thisValue == DBNull.Value || string.IsNullOrWhiteSpace(thisValue.ToString());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static decimal ObjToDecimal(this object thisValue)
        {
            decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue"></param>
        /// <returns></returns>
        public static decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }

            return reval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <param name="errorValue"></param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static bool ObjToBool(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }


        /// <summary>
        /// 获取当前时间的时间戳
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string DateToTimeStamp(this DateTime thisValue)
        {
            TimeSpan ts = thisValue - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        public static object ChangeType(this object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = value.ChangeType(innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }

        public static object ChangeTypeList(this object value, Type type)
        {
            if (value == null) return default;

            var gt = typeof(List<>).MakeGenericType(type);
            dynamic lis = Activator.CreateInstance(gt);

            var addMethod = gt.GetMethod("Add");
            string values = value.ToString();
            if (values != null && values.StartsWith("(") && values.EndsWith(")"))
            {
                string[] splits;
                if (values.Contains("\",\""))
                {
                    splits = values.Remove(values.Length - 2, 2)
                        .Remove(0, 2)
                        .Split("\",\"");
                }
                else
                {
                    splits = values.Remove(0, 1)
                        .Remove(values.Length - 2, 1)
                        .Split(",");
                }

                foreach (var split in splits)
                {
                    var str = split;
                    if (split.StartsWith("\"") && split.EndsWith("\""))
                    {
                        str = split.Remove(0, 1)
                            .Remove(split.Length - 2, 1);
                    }

                    addMethod.Invoke(lis, new object[] { str.ChangeType(type) });
                }
            }

            return lis;
        }
    }
}
