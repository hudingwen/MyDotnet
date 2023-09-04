using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using System.Reflection;

namespace MyDotnet.Config
{
    /// <summary>
    /// Sqlsugar配置类
    /// </summary>
    public static class SqlsugarConfig
    {
        /// <summary>
        /// 数据配置
        /// </summary>
        /// <param name="builder"></param>
        public static void SetSqlsugar(this WebApplicationBuilder builder)
        {

            //获取主库
            DbConfigHelper.MainDB = ConfigHelper.GetValue(new string[] { "Database", "MainDB" }).ObjToString();
            //是否多库
            DbConfigHelper.MutiDBEnabled = ConfigHelper.GetValue(new string[] { "Database", "MutiDBEnabled" }).ObjToBool();
            //获取数据库链接
            List<MutiDBOperate> listdatabase = ConfigHelper.GetList<MutiDBOperate>(new string[] { "Database", "DBS" });
            listdatabase = listdatabase.Where(t => t.Enabled).ToList();
            var mainDb = listdatabase.Find(t => t.ConnId.Equals(DbConfigHelper.MainDB));
            if (mainDb == null)
                throw new ArgumentNullException("没有配置主库数据库链接");
            //默认主库为第一个链接
            listdatabase.Remove(mainDb);
            listdatabase.Insert(0, mainDb);

            //如果不是多库操作就只能剩下一个库
            if (!DbConfigHelper.MutiDBEnabled)
            {
                listdatabase.Clear();
                listdatabase.Add(mainDb);
            }
            DbConfigHelper.listdatabase = listdatabase;

            List<SqlSugar.ConnectionConfig> connectionConfigs = new List<SqlSugar.ConnectionConfig>();
            DbConfigHelper.listdatabase.ForEach(m =>
            {
                var config = new SqlSugar.ConnectionConfig()
                {
                    ConfigId = m.ConnId,
                    ConnectionString = m.Connection,
                    DbType = (SqlSugar.DbType)m.DbType,
                    IsAutoCloseConnection = true,
                    //IsShardSameThread = false,
                    MoreSettings = new SqlSugar.ConnMoreSettings()
                    {
                        //IsWithNoLockQuery = true,
                        IsAutoRemoveDataCache = true,
                        SqlServerCodeFirstNvarchar = true,
                    },
                    // 从库
                    //SlaveConnectionConfigs = BaseDBConfig.AllSlaveConfigs,
                    // 自定义特性
                    //ConfigureExternalServices = new ConfigureExternalServices()
                    //{
                    //    DataInfoCacheService = new SqlSugarCacheService(),
                    //    EntityService = (property, column) =>
                    //    {
                    //        if (column.IsPrimarykey && property.PropertyType == typeof(int))
                    //        {
                    //            column.IsIdentity = true;
                    //        }
                    //    }
                    //},
                    InitKeyType = SqlSugar.InitKeyType.Attribute
                };

                connectionConfigs.Add(config);
            });
            //数据库链接注入
            builder.Services.AddSingleton<SqlSugar.ISqlSugarClient>(o =>
            {
                return new SqlSugar.SqlSugarScope(connectionConfigs, db =>
                {
                    DbConfigHelper.listdatabase.ForEach(config =>
                    {
                        var dbProvider = db.GetConnectionScope(config.ConnId);

                        // 打印SQL语句
                        dbProvider.Aop.OnLogExecuting = (sql, pars) =>
                        {
                            //var user = InternalHelper.RootServices.GetService<AspNetUser>();

                            //我可以在这里面写逻辑
                            //LogHelper.logApp.Info($"【用户ID】:{user.ID}【数据库】：{config.ConnId} 【SQL语句】：{sql} 【SQL参数】：{GetParas(pars)}");
                            //技巧：AOP中获取IOC对象
                            //var serviceBuilder = services.BuildServiceProvider();
                            //var log= serviceBuilder.GetService<ILogger<WeatherForecastController>>();
                        };
                        //// 数据审计
                        //dbProvider.Aop.DataExecuting = SqlSugarAop.DataExecuting;

                        //// 配置实体假删除过滤器
                        RepositorySetting.SetDeletedEntityFilter(dbProvider);
                        //// 配置实体数据权限
                        //RepositorySetting.SetTenantEntityFilter(dbProvider);
                    });
                });
            });


            //全局事务注入
            builder.Services.AddScoped(typeof(UnitOfWorkManage));

            //获取所有类型
            var types = Assembly.GetEntryAssembly().GetExportedTypes();

            //泛型仓储层注入
            builder.Services.AddScoped(typeof(BaseRepository<>));

            //泛型服务层注册
            builder.Services.AddScoped(typeof(BaseServices<>));

            //遍历
            Type baseTypeRepository = typeof(BaseRepository<>);
            Type baseTypeServices = typeof(BaseServices<>);
            foreach (var item in types)
            {
                //自定义仓层储注入 
                if (item.BaseType != null && item.BaseType.IsGenericType && item.BaseType.GetGenericTypeDefinition() == baseTypeRepository.GetGenericTypeDefinition())
                    builder.Services.AddScoped(item);
                //自定服务层储注入 
                if (item.BaseType != null && item.BaseType.IsGenericType && item.BaseType.GetGenericTypeDefinition() == baseTypeServices.GetGenericTypeDefinition())
                    builder.Services.AddScoped(item);
            }

        }
    }
}
