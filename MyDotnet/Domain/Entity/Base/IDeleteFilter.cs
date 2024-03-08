using SqlSugar;

namespace MyDotnet.Domain.Entity.Base;

/// <summary>
/// 软删除 过滤器
/// </summary>
public interface IDeleteFilter
{

    /// <summary>
    ///获取或设置是否禁用，逻辑上的删除，非物理删除
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public bool IsDeleted { get; set; }
}