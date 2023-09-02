using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.System;

/// <summary>
/// 博客文章 评论
/// </summary>
public class BlogArticleComment : RootEntityTkey<long>
{
    public long bID { get; set; }

    public string Comment { get; set; }


    public string UserId { get; set; }

    [Navigate(NavigateType.OneToOne, nameof(UserId))]
    public SysUserInfo User { get; set; }
}