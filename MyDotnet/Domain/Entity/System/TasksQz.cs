﻿using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace MyDotnet.Domain.Entity.System
{
    /// <summary>
    /// 任务计划表
    /// </summary>
    public class TasksQz : BaseEntity
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Name { get; set; }
        /// <summary>
        /// 任务分组
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string JobGroup { get; set; }
        /// <summary>
        /// 任务运行时间表达式
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string Cron { get; set; }
        /// <summary>
        /// 任务所在DLL对应的程序集名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string AssemblyName { get; set; }
        /// <summary>
        /// 任务所在类
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string ClassName { get; set; }
        /// <summary>
        /// 任务描述
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string Remark { get; set; }
        /// <summary>
        /// 执行次数
        /// </summary>
        public int RunTimes { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 触发器类型（0、simple 1、cron）
        /// </summary>
        public int TriggerType { get; set; }
        /// <summary>
        /// 执行间隔时间, 秒为单位
        /// </summary>
        public int IntervalSecond { get; set; }
        /// <summary>
        /// 循环执行次数
        /// </summary>
        public int CycleRunTimes { get; set; }
        /// <summary>
        /// 已循环次数
        /// </summary>
        public int CycleHasRunTimes { get; set; }
        /// <summary>
        /// 是否启动
        /// </summary>
        public bool IsStart { get; set; } = false;
        /// <summary>
        /// 执行传参
        /// </summary>
        [SugarColumn(Length = 255, IsNullable = true)]
        public string JobParams { get; set; }
        /// <summary>
        /// 任务需存储数据
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true)]
        public string StoreData { get; set; }
        /// <summary>
        /// 分布任务,不用api执行不用的调度任务
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string DistributeCode { get; set; }
        /// <summary>
        /// 源ID,是否通过其他单据发起的
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public long ResourceId { get; set; }
        /// <summary>
        /// 任务内存中的状态
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<TaskInfoDto> Triggers { get; set; }
    }
}
