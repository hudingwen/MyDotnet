
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Helper;
using SqlSugar;
namespace MyDotnet.Aop.Database;

public static class SqlSugarAop
{
    public static void OnLogExecuting(ConnectionConfig config, string sql, SugarParameter[] pars)
    {
        LogHelper.logSql.Info($"数据库:{config.ConfigId}\r\nsql语句:{sql}\r\nsql参数:{GetParas(pars)}");
    }

    public static void DataExecuting(object oldValue, DataFilterModel entityInfo)
    {
       

        if (entityInfo.EntityValue is BaseEntity baseEntity)
        {
            
            // 新增操作
            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                AspNetUser curUser;
                using (var serviceScope = AppHelper.appService.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    curUser = services.GetRequiredService<AspNetUser>();
                }
                //新增自动添加主键
                if (entityInfo.EntityValue is RootEntityTkey<long> rootEntity)
                {
                    if (rootEntity.Id == 0)
                    {
                        rootEntity.Id = SnowFlakeSingle.Instance.NextId();
                    }
                }
                //审计记录
                baseEntity.CreateTime = DateTime.Now;
                if(curUser!=null)
                {
                    baseEntity.CreateId = curUser.ID;
                    baseEntity.CreateBy = curUser.Name;
                }

            }
            //修改操作
            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                AspNetUser curUser;
                using (var serviceScope = AppHelper.appService.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;
                    curUser = services.GetRequiredService<AspNetUser>();
                }
                //审计记录
                baseEntity.ModifyTime = DateTime.Now;
                if (curUser != null)
                {
                    baseEntity.ModifyId = curUser.ID;
                    baseEntity.ModifyBy = curUser.Name;
                }

            }

        }
    } 

    private static string GetParas(SugarParameter[] pars)
    {
        string key = "";
        foreach (var param in pars)
        {
            key += $"{param.ParameterName}:{param.Value}\n";
        }
        return key;
    }

   
}
