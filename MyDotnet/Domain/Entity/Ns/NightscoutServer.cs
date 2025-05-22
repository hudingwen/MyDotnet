using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 血糖服务器
    /// </summary>
    public class NightscoutServer : BaseEntity
    {
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string serverName { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string serverIp { get; set; }
        /// <summary>
        /// 服务器host(暂未启用)
        /// </summary>
        public string serverHost { get; set; }
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int serverPort { get; set; }
        public string serverLoginName { get; set; }
        public string serverLoginPassword { get; set; }
        /// <summary>
        /// 当前实例IP(弃用)
        /// </summary>
        //public string curInstanceIp { get; set; }
        /// <summary>
        /// 当前IP序列(弃用)
        /// </summary>
        //public int curInstanceIpSerial { get; set; }
        /// <summary>
        /// 实例模板ip(弃用)
        /// </summary>
        //public string instanceIpTemplate { get; set; }
        /// <summary>
        /// 当前暴露端口(服务器IP+暴露端口)(如果为0则为实例IP+1337)
        /// </summary>
        public int curExposedPort { get; set; }
        /// <summary>
        /// 当前服务序列(弃用)
        /// </summary>
        //public int curServiceSerial { get; set; }
        /// <summary>
        /// MongoDB数据库
        /// </summary>
        public string mongoIp { get; set; }
        /// <summary>
        /// MongoDB端口
        /// </summary>
        public int mongoPort { get; set; }
        /// <summary>
        /// mongo登录账号
        /// </summary>
        public string mongoLoginName { get; set; }
        /// <summary>
        /// mongo登录密码
        /// </summary>
        public string mongoLoginPassword { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 是否nginx节点服务器
        /// </summary>
        public bool isNginx { get; set; }
        /// <summary>
        /// 是否mongo节点服务器
        /// </summary>
        public bool isMongo { get; set; }
        /// <summary>
        /// 统计数量-创建
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int count { get; set; }
        /// <summary>
        /// 统计数量-运行
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int countStart { get; set; }
        /// <summary>
        /// 统计数量-停止
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int countStop { get; set; }
        /// <summary>
        /// 理论最大ns数量
        /// </summary>
        public int holdCount { get; set; }
        /// <summary>
        /// 数据库服务器关联id(ssh用)
        /// </summary>
        public long mongoServerId { get; set; }

        /// <summary>
        /// 网关节点服务器id(ssh)(暂未启用)
        /// </summary>
        public long nginxServerId { get; set; }
        /// <summary>
        /// 网关节点服务器id(ssh)(暂未启用)
        /// </summary>
        public int nodeExportPort { get; set; } 
        /// <summary>
        /// 内存占用百分比
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public double memory { get; set; }
        


    }
}
