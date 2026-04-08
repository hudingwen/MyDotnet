namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 使用的字典集
    /// </summary>
    public static class DicTypeList
    {
        /// <summary>
        /// 默认cdn
        /// </summary>
        public static string defaultCDN = "defaultCDN";
        /// <summary>
        /// Nightscout默认版本
        /// </summary>
        public static string defaultNsVersion = "defaultNsVersion";
        /// <summary>
        /// Nightscout默认内存/单位/M
        /// </summary>
        public static string defaultNsMemory = "defaultNsMemory";
        /// <summary>
        /// Nightscout服务名称当前序列号 每次加+1 并更新字典
        /// </summary>
        public static string NsServiceNameCurSerial = "NsServiceNameCurSerial";
    }
}
