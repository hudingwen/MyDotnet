﻿using SqlSugar;

namespace MyDotnet.Domain.Entity.Base;

/// <summary>
/// 部门表
/// </summary>
public class DepartmentRoot<Tkey> : RootEntityTkey<Tkey> where Tkey : IEquatable<Tkey>
{
    /// <summary>
    /// 上一级（0表示无上一级）
    /// </summary>
    public Tkey Pid { get; set; }

}
