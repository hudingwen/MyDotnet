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
        public static List<MutiDBOperate> listdatabase;
        /// <summary>
        /// 主数据库id
        /// </summary>
        public static string MainDB;
        /// <summary>
        /// 是否多库
        /// </summary>
        public static bool MutiDBEnabled;
    }
}
