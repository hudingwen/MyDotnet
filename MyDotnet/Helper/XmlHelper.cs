using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MyDotnet.Helper
{
    /// <summary>
    /// xml序列化帮助类
    /// </summary>
    public class XmlHelper
    {
        /// <summary>
        /// 转换对象为JSON格式数据
        /// </summary>
        /// <typeparam name="T">类</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>字符格式的JSON数据</returns>
        public static string GetXML<T>(object obj)
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            using (TextWriter tw = new StringWriter())
            {
                xs.Serialize(tw, obj);
                return tw.ObjToString();
            }
        }

        /// <summary>
        /// Xml格式字符转换为T类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T ParseFormByXml<T>(string xml, string rootName = "root")
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(rootName));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
