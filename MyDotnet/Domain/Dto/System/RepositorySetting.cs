using SqlSugar;

namespace MyDotnet.Domain.Dto.System;

/// <summary>
/// 仓库中间件
/// </summary>
public class RepositorySetting
{
    /// <summary>
    /// 配置实体软删除过滤器<br/>
    /// 统一过滤 软删除 无需自己写条件
    /// </summary>
    public static void SetDeletedEntityFilter(SqlSugarScopeProvider db)
    {
        db.QueryFilter.AddTableFilter<IDeleteFilter>(it => it.IsDeleted == false);
    }
}