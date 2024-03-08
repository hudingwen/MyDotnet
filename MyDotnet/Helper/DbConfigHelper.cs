using MyDotnet.Domain.Dto.System;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 数据库全局配置访问类
    /// </summary>
    public static class DbConfigHelper
    {
        /// <summary>
        /// 数据库可用列表
        /// </summary>
        public static List<MutiDBOperate> listdatabase = new List<MutiDBOperate>();
        /// <summary>
        /// 主数据库id
        /// </summary>
        public static string MainDB = ConfigHelper.GetValue(new string[] { "Database", "MainDB" }).ObjToString();
        /// <summary>
        /// 是否多库
        /// </summary>
        public static bool MutiDBEnabled = ConfigHelper.GetValue(new string[] { "Database", "MutiDBEnabled" }).ObjToBool();
    }
}
