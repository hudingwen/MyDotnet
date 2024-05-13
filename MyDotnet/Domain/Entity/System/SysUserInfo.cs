using MyDotnet.Domain.Entity.Base;
using SqlSugar;
using System.Reflection.Metadata;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    //[SugarTable("SysUserInfo")]
    [SugarTable("SysUserInfo", "用户表")] //('数据库表名'，'数据库表备注')
    public class SysUserInfo : BaseEntity
    {
        public SysUserInfo()
        {
        }

        public SysUserInfo(string loginName, string loginPWD)
        {
            LoginName = loginName;
            LoginPWD = loginPWD;
            RealName = LoginName;
            Status = 0; 
            ErrorCount = 0;
            Name = "";
        }

        /// <summary>
        /// 登录账号
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "登录账号")]
        //:eg model 根据sqlsugar的完整定义可以如下定义，ColumnDescription可定义表字段备注
        //[SugarColumn(IsNullable = false, ColumnDescription = "登录账号", IsPrimaryKey = false, IsIdentity = false, Length = 50)]
        //ColumnDescription 表字段备注，  已在MSSQL测试，配合 [SugarTable("SysUserInfo", "用户表")]//('数据库表名'，'数据库表备注')
        //可以完整生成 表备注和各个字段的中文备注
        //2022/10/11
        //测试mssql 发现 不写ColumnDescription，写好注释在mssql下也能生成表字段备注
        public string LoginName { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string LoginPWD { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string RealName { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int DepartmentId { get; set; } = -1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Remark { get; set; } 

        /// <summary>
        /// 关键业务修改时间
        /// </summary>
        public DateTime CriticalModifyTime { get; set; } = DateTime.Now;

        /// <summary>
        ///最后异常时间 
        /// </summary>
        public DateTime LastErrorTime { get; set; } = DateTime.Now;

        /// <summary>
        ///错误次数 
        /// </summary>
        public int ErrorCount { get; set; }


        /// <summary>
        /// 登录账号
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Name { get; set; }

        // 性别
        [SugarColumn(IsNullable = true)]
        public int Sex { get; set; } = 0;

        // 年龄
        [SugarColumn(IsNullable = true)]
        public int Age { get; set; }

        // 生日
        [SugarColumn(IsNullable = true)]
        public DateTime Birth { get; set; } = DateTime.Now;



        // 头像
        [SugarColumn(IsNullable = true)]
        public string logo { get; set; }

        /// <summary>
        /// 是否开启双因子认证
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool auth2faEnable { get; set; }
        /// <summary>
        /// 双因子认证id
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long auth2faId { get; set; }



        // 地址
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Address { get; set; } 

        [SugarColumn(IsNullable = true)]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 租户Id
        /// </summary>
        [SugarColumn(IsNullable = false, DefaultValue = "0")]
        public long TenantId { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<string> RoleNames { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<long> Dids { get; set; }

        [SugarColumn(IsIgnore = true)]
        public string DepartmentName { get; set; }

        [SugarColumn(IsIgnore = true)]
        public List<long> RIDs { get; set; }
    }
}