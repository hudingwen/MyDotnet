using MyDotnet.Aop.Database;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using Quartz.Impl.AdoJobStore.Common;
using SqlSugar;
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
            //获取数据库链接
            List<MutiDBOperate> listdatabase = ConfigHelper.GetList<MutiDBOperate>(new string[] { "Database", "DBS" });
            listdatabase = listdatabase.Where(t => t.Enabled).OrderBy(t=>t.HitRate).ToList();
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

            List<ConnectionConfig> connectionConfigs = new List<ConnectionConfig>();
            DbConfigHelper.listdatabase.ForEach(m =>
            {
                var config = new ConnectionConfig()
                {
                    ConfigId = m.ConnId,
                    ConnectionString = m.Connection,
                    DbType = (DbType)m.DbType,
                    IsAutoCloseConnection = true,
                    //IsShardSameThread = false,
                    MoreSettings = new ConnMoreSettings()
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
                    InitKeyType = InitKeyType.Attribute
                };

                if(config.DbType == DbType.Sqlite)
                {
                    config.ConnectionString = $"DataSource=" + Path.Combine(Environment.CurrentDirectory, config.ConnectionString);
                }

               
                connectionConfigs.Add(config);
            });
            //数据库链接注入
            builder.Services.AddSingleton<ISqlSugarClient>(o =>
            {
                return new SqlSugarScope(connectionConfigs, db =>
                {
                    connectionConfigs.ForEach(config =>
                    {
                        var dbProvider = db.GetConnectionScope(config.ConfigId);

                        if(EnableConfig.sqlLogEnable)
                        {
                            // 打印SQL语句 
                            dbProvider.Aop.OnLogExecuting = (sql, pars) => SqlSugarAop.OnLogExecuting(config, sql, pars);
                        }

                        // 数据审计
                        dbProvider.Aop.DataExecuting = SqlSugarAop.DataExecuting;

                        // 软删除
                        dbProvider.QueryFilter.AddTableFilter<IDeleteFilter>(it => it.IsDeleted == false); 

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
                {
                    builder.Services.AddScoped(item);
                    continue;
                }
                //自定服务层储注入 
                if (item.BaseType != null && item.BaseType.IsGenericType && item.BaseType.GetGenericTypeDefinition() == baseTypeServices.GetGenericTypeDefinition())
                {
                    builder.Services.AddScoped(item);
                    continue;
                }
            }

        }
    }
    
}
