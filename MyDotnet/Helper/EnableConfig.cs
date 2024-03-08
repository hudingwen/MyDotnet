namespace MyDotnet.Helper
{
    public static class EnableConfig
    {
        /// <summary>
        /// 是否开启数据库日志写入
        /// </summary>
        public static readonly bool sqlLogEnable = ConfigHelper.GetValue(new string[] { "EnableConfig", "sqlLogEnable" }).ObjToBool();
        /// <summary>
        /// 是否初始化数据库(第一建库用)
        /// </summary>
        public static readonly bool initDataBase = ConfigHelper.GetValue(new string[] { "EnableConfig", "initDataBase" }).ObjToBool();
        /// <summary>
        /// 是否启用调度服务
        /// </summary>
        public static readonly bool quartzEnable = ConfigHelper.GetValue(new string[] { "EnableConfig", "quartzEnable" }).ObjToBool();

    }
}
