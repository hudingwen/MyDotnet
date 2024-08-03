using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{

    [SugarTable("t_code", "激活码表")]
    public class TCode
    {
        /// <summary>
        /// ID主键
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IsPrimaryKey = true)]
        public string id { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string user_id { get; set; }
        /// <summary>
        /// 记录id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string record_id { get; set; }
        /// <summary>
        /// 记录key
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string record_key { get; set; }
        /// <summary>
        /// 授权码
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string auth_code { get; set; }
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime create_date { get; set; }
        /// <summary>
        /// 天
        /// </summary>
        public int create_day { get; set; }
        /// <summary>
        /// 时
        /// </summary>
        public int create_hour { get; set; }
        /// <summary>
        /// 分
        /// </summary>
        public int create_min { get; set; }
        /// <summary>
        /// 秒
        /// </summary>
        public int create_sec { get; set; }
        /// <summary>
        /// 激活时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? user_time { get; set; }
        /// <summary>
        /// 到期时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? expiry_date { get; set; }
        /// <summary>
        /// 设备码
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string uuid { get; set; }
        /// <summary>
        /// 验证状态
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        public int is_delete { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string comment { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string code_type { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? reg_time { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int? expire_time { get; set; }
        /// <summary>
        /// 删除(冻结)时间
        /// </summary>
        public DateTime delete_time { get; set; }

        /// <summary>
        /// 创建数量
        /// </summary>

        [SugarColumn(IsIgnore = true)]
        public int createCount {  get; set; }

        /// <summary>
        /// 创建类型0-英文 1-中文 
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int createType { get; set; }
    }
}
