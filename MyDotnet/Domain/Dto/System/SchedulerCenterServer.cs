﻿using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;
using Quartz.Spi;
using SqlSugar;
using System.Collections.Specialized;
using System.Reflection;

namespace MyDotnet.Domain.Dto.System
{
    /// <summary>
    /// 任务调度管理中心
    /// </summary>
    public class SchedulerCenterServer
    {
        private IScheduler _scheduler;
        private readonly IJobFactory _iocjobFactory;
        public  SchedulerCenterServer(IJobFactory jobFactory)
        {
            _iocjobFactory = jobFactory;
            _scheduler = GetSchedulerAsync().Result;
        }
        private async Task<IScheduler> GetSchedulerAsync()
        {
            if (_scheduler != null)
                return _scheduler;
            else
            {
                // 从Factory中获取Scheduler实例
                NameValueCollection collection = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" },
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(collection);
                return _scheduler = await  factory.GetScheduler();
            }
        }

        /// <summary>
        /// 开启任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<string>> StartScheduleAsync()
        {
            var result = new MessageModel<string>();
            try
            {
                _scheduler.JobFactory = _iocjobFactory;
                if (!_scheduler.IsStarted)
                {
                    //等待任务运行完成
                    await _scheduler.Start();
                    await Console.Out.WriteLineAsync("任务调度开启！");
                    result.success = true;
                    result.msg = $"任务调度开启成功";
                    return result;
                }
                else
                {
                    result.success = false;
                    result.msg = $"任务调度已经开启";
                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<string>> StopScheduleAsync()
        {
            var result = new MessageModel<string>();
            try
            {
                if (!_scheduler.IsShutdown)
                {
                    //等待任务运行完成
                    await _scheduler.Shutdown();
                    await Console.Out.WriteLineAsync("任务调度停止！");
                    result.success = true;
                    result.msg = $"任务调度停止成功";
                    return result;
                }
                else
                {
                    result.success = false;
                    result.msg = $"任务调度已经停止";
                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 添加一个计划任务（映射程序集指定IJob实现类）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> AddScheduleJobAsync(TasksQz tasksQz)
        {
            var result = new MessageModel<string>();

            if (tasksQz != null)
            {
                try
                {
                    JobKey jobKey = new JobKey(tasksQz.Id.ToString(), tasksQz.JobGroup);
                    if (await _scheduler.CheckExists(jobKey))
                    {
                        result.success = false;
                        result.msg = $"该任务计划已经在执行:【{tasksQz.Name}】,请勿重复启动！";
                        return result;
                    }
                    if (tasksQz.TriggerType == 0 && tasksQz.CycleRunTimes != 0 && tasksQz.CycleHasRunTimes >= tasksQz.CycleRunTimes)
                    {
                        result.success = false;
                        result.msg = $"该任务计划已完成:【{tasksQz.Name}】,无需重复启动,如需启动请修改已循环次数再提交";
                        return result;
                    }
                    #region 设置开始时间和结束时间

                    if (tasksQz.BeginTime == null)
                    {
                        tasksQz.BeginTime = DateTime.Now;
                    }
                    DateTimeOffset starRunTime = DateBuilder.NextGivenSecondDate(tasksQz.BeginTime, 1);//设置开始时间
                    if (tasksQz.EndTime == null)
                    {
                        tasksQz.EndTime = DateTime.MaxValue.AddDays(-1);
                    }
                    DateTimeOffset endRunTime = DateBuilder.NextGivenSecondDate(tasksQz.EndTime, 1);//设置暂停时间

                    #endregion

                    #region 通过反射获取程序集类型和类   

                    //var jobType = Assembly.GetEntryAssembly().GetTypes()
                    //     .Where(t => t.Namespace == tasksQz.AssemblyName && t.Name == tasksQz.ClassName)
                    //     .FirstOrDefault();
                    //Assembly assembly = Assembly.Load(new AssemblyName(tasksQz.AssemblyName));
                    //Type jobType = assembly.GetType(tasksQz.AssemblyName + "." + tasksQz.ClassName);

                    string className = $"{tasksQz.AssemblyName}.{tasksQz.ClassName}";
                    Type jobType = Type.GetType(className);

                    #endregion
                    //判断任务调度是否开启
                    if (!_scheduler.IsStarted)
                    {
                        await StartScheduleAsync();
                    }

                    //传入反射出来的执行程序集
                    IJobDetail job = new JobDetailImpl(tasksQz.Id.ToString(), tasksQz.JobGroup, jobType);
                    job.JobDataMap.Add("JobParam", tasksQz.JobParams);
                    job.JobDataMap.Add("Now", false);
                    ITrigger trigger;

                    #region 泛型传递
                    //IJobDetail job = JobBuilder.Create<T>()
                    //    .WithIdentity(sysSchedule.Name, sysSchedule.JobGroup)
                    //    .Build();
                    #endregion

                    if (tasksQz.Cron != null && CronExpression.IsValidExpression(tasksQz.Cron) && tasksQz.TriggerType > 0)
                    {
                        trigger = CreateCronTrigger(tasksQz);

                        ((CronTriggerImpl)trigger).MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing;
                    }
                    else
                    {
                        trigger = CreateSimpleTrigger(tasksQz);
                    }

                    // 告诉Quartz使用我们的触发器来安排作业
                    await _scheduler.ScheduleJob(job, trigger);
                    //await Task.Delay(TimeSpan.FromSeconds(120));
                    //await Console.Out.WriteLineAsync("关闭了调度器！");
                    //await _scheduler.Result.Shutdown();
                    result.success = true;
                    result.msg = $"【{tasksQz.Name}】成功";
                    return result;
                }
                catch (Exception ex)
                {
                    result.success = false;
                    result.msg = $"任务计划异常:【{ex.Message}】";
                    return result;
                }
            }
            else
            {
                result.success = false;
                result.msg = $"任务计划不存在:【{tasksQz?.Name}】";
                return result;
            }
        }

        /// <summary>
        /// 任务是否存在?
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsExistScheduleJobAsync(TasksQz sysSchedule)
        {
            JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
            if (await _scheduler.CheckExists(jobKey))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 停止一个指定的计划任务
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<string>> StopScheduleJobAsync(TasksQz sysSchedule)
        {
            var result = new MessageModel<string>();
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.CheckExists(jobKey))
                {
                    result.success = false;
                    result.msg = $"未找到要暂停的任务:【{sysSchedule.Name}】";
                    return result;
                }
                else
                {
                    await _scheduler.DeleteJob(jobKey);
                    result.success = true;
                    result.msg = $"【{sysSchedule.Name}】成功";
                    return result;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 恢复指定的计划任务
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> ResumeJob(TasksQz sysSchedule)
        {
            var result = new MessageModel<string>();
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.CheckExists(jobKey))
                {
                    result.success = false;
                    result.msg = $"未找到要恢复的任务:【{sysSchedule.Name}】";
                    return result;
                }
                await _scheduler.ResumeJob(jobKey);
                result.success = true;
                result.msg = $"【{sysSchedule.Name}】成功";
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 暂停指定的计划任务
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> PauseJob(TasksQz sysSchedule)
        {
            var result = new MessageModel<string>();
            try
            {
                JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
                if (!await _scheduler.CheckExists(jobKey))
                {
                    result.success = false;
                    result.msg = $"未找到要暂停的任务:【{sysSchedule.Name}】";
                    return result;
                }
                await _scheduler.PauseJob(jobKey);
                result.success = true;
                result.msg = $"【{sysSchedule.Name}】成功";
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #region 状态状态帮助方法
        public async Task<List<TaskInfoDto>> GetTaskStaus(TasksQz sysSchedule)
        {

            var ls = new List<TaskInfoDto>();
            var noTask = new List<TaskInfoDto>{ new TaskInfoDto {
                jobId = sysSchedule.Id.ObjToString(),
                jobGroup = sysSchedule.JobGroup,
                triggerId = "",
                triggerGroup = "",
                triggerStatus = "不存在"
            } };
            JobKey jobKey = new JobKey(sysSchedule.Id.ToString(), sysSchedule.JobGroup);
            IJobDetail job = await _scheduler.GetJobDetail(jobKey);
            if (job == null)
            {
                return noTask;
            }
            //info.Append(string.Format("任务ID:{0}\r\n任务名称:{1}\r\n", job.Key.Name, job.Description)); 
            var triggers = await _scheduler.GetTriggersOfJob(jobKey);
            if (triggers == null || triggers.Count == 0)
            {
                return noTask;
            }
            foreach (var trigger in triggers)
            {
                var triggerStaus = await _scheduler.GetTriggerState(trigger.Key);
                string state = GetTriggerState(triggerStaus.ObjToString());
                ls.Add(new TaskInfoDto
                {
                    jobId = job.Key.Name,
                    jobGroup = job.Key.Group,
                    triggerId = trigger.Key.Name,
                    triggerGroup = trigger.Key.Group,
                    triggerStatus = state
                });
                //info.Append(string.Format("触发器ID:{0}\r\n触发器名称:{1}\r\n状态:{2}\r\n", item.Key.Name, item.Description, state));

            }
            return ls;
        }
        
        public async Task<DateTimeOffset?> RescheduleJob(TriggerKey key, ITrigger trigger)
        {
            return await _scheduler.RescheduleJob(key, trigger);
        }
        public async Task<TriggerState> GetTriggerState(TriggerKey key)
        {
            return await _scheduler.GetTriggerState(key);
        }
        public string GetTriggerState(string key)
        {
            string state = null;
            if (key != null)
                key = key.ToUpper();
            switch (key)
            {
                case "1":
                    state = "暂停";
                    break;
                case "2":
                    state = "完成";
                    break;
                case "3":
                    state = "出错";
                    break;
                case "4":
                    state = "阻塞";
                    break;
                case "0":
                    state = "正常";
                    break;
                case "-1":
                    state = "不存在";
                    break;
                case "BLOCKED":
                    state = "阻塞";
                    break;
                case "COMPLETE":
                    state = "完成";
                    break;
                case "ERROR":
                    state = "出错";
                    break;
                case "NONE":
                    state = "不存在";
                    break;
                case "NORMAL":
                    state = "正常";
                    break;
                case "PAUSED":
                    state = "暂停";
                    break;
            }
            return state;
        }
        #endregion
        #region 创建触发器帮助方法

        /// <summary>
        /// 创建SimpleTrigger触发器（简单触发器）
        /// </summary>
        /// <param name="sysSchedule"></param>
        /// <param name="starRunTime"></param>
        /// <param name="endRunTime"></param>
        /// <returns></returns>
        private ITrigger CreateSimpleTrigger(TasksQz sysSchedule)
        {
            if (sysSchedule.CycleRunTimes > 0)
            {
                ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(sysSchedule.Id.ToString(), sysSchedule.JobGroup)
                .StartAt(sysSchedule.BeginTime.Value)
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(sysSchedule.IntervalSecond)
                    .WithRepeatCount(sysSchedule.CycleRunTimes - 1))
                .EndAt(sysSchedule.EndTime.Value)
                .Build();
                return trigger;
            }
            else
            {
                ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(sysSchedule.Id.ToString(), sysSchedule.JobGroup)
                .StartAt(sysSchedule.BeginTime.Value)
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(sysSchedule.IntervalSecond)
                    .RepeatForever()
                )
                .EndAt(sysSchedule.EndTime.Value)
                .Build();
                return trigger;
            }
            // 触发作业立即运行，然后每10秒重复一次，无限循环

        }
        /// <summary>
        /// 创建类型Cron的触发器
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private ITrigger CreateCronTrigger(TasksQz sysSchedule)
        {
            // 作业触发器
            return TriggerBuilder.Create()
                   .WithIdentity(sysSchedule.Id.ToString(), sysSchedule.JobGroup)
                   .StartAt(sysSchedule.BeginTime.Value)//开始时间
                   .EndAt(sysSchedule.EndTime.Value)//结束数据
                   .WithCronSchedule(sysSchedule.Cron)//指定cron表达式
                   .ForJob(sysSchedule.Id.ToString(), sysSchedule.JobGroup)//作业名称
                   .Build();
        }
        #endregion


        /// <summary>
        /// 立即执行 一个任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> ExecuteJobAsync(TasksQz tasksQz)
        {
            var result = new MessageModel<string>();
            try
            {
                //判断任务调度是否开启
                if (!_scheduler.IsStarted)
                {
                    await StartScheduleAsync();
                }

                string className = $"{tasksQz.AssemblyName}.{tasksQz.ClassName}";
                Type jobType = Type.GetType(className);

                //传入反射出来的执行程序集
                IJobDetail job = new JobDetailImpl(tasksQz.Id.ToString(), tasksQz.JobGroup+"-"+ StringHelper.GetGUID(), jobType);
                job.JobDataMap.Add("JobParam", tasksQz.JobParams);
                job.JobDataMap.Add("Now", true);

                // 定义一个立即触发的触发器
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity(tasksQz.Id.ToString(), tasksQz.JobGroup + "-" + StringHelper.GetGUID())
                    .StartNow()
                    .Build();
                await _scheduler.ScheduleJob(job, trigger);
                result.success = true;
                result.msg = "执行成功";
            }
            catch (Exception ex)
            {
                result.success = false;
                result.msg = $"立即执行计划任务失败:【{ex.Message}】";
            }

            return result;
        }


    }
}
