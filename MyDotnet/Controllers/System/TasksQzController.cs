using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Attr;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Nginx;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
using Quartz;
using SqlSugar;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 调度服务管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class TasksQzController : ControllerBase
    {
        public BaseServices<TasksQz> _tasksQzServices;
        public TasksLogServices _tasksLogServices;
        public SchedulerCenterServer _schedulerCenter;
        public UnitOfWorkManage _unitOfWorkManage;

        public TasksQzController(BaseServices<TasksQz> tasksQzServices
            , TasksLogServices tasksLogServices
            , SchedulerCenterServer schedulerCenter 
            , UnitOfWorkManage unitOfWorkManage
            )
        {
            _tasksQzServices = tasksQzServices;
            _tasksLogServices = tasksLogServices;
            _schedulerCenter = schedulerCenter;
            _unitOfWorkManage = unitOfWorkManage;
        }
        /// <summary>
        /// 获取任务列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TasksQz>>> Get(int page = 1, int size = 10, string key = "")
        {


            var whereFind = LinqHelper.True<TasksQz>().And(t => t.IsDeleted == false);

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.Name.Contains(key) || t.AssemblyName.Contains(key) || t.ClassName.Contains(key));
            }

            var data = await _tasksQzServices.Dal.QueryPage(whereFind, page, size, " Id desc ");

            foreach (var item in data.data)
            {
                item.Triggers = await _schedulerCenter.GetTaskStaus(item);
            }
            return MessageModel<PageModel<TasksQz>>.Success("获取成功", data);
        }

        /// <summary>
        /// 添加计划任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            _unitOfWorkManage.BeginTran();
            var id = await _tasksQzServices.Dal.Add(tasksQz);
            data.success = id > 0;
            try
            {
                if (data.success)
                {
                    tasksQz.Id = id;
                    data.response = id.ObjToString();
                    data.msg = "添加成功";
                    if (tasksQz.IsStart)
                    {
                        //如果是启动自动
                        var ResuleModel = await _schedulerCenter.AddScheduleJobAsync(tasksQz);
                        data.success = ResuleModel.success;
                        if (ResuleModel.success)
                        {
                            data.msg = $"{data.msg}=>启动成功=>{ResuleModel.msg}";
                        }
                        else
                        {
                            data.msg = $"{data.msg}=>启动失败=>{ResuleModel.msg}";
                        }
                    }
                }
                else
                {
                    data.msg = "添加失败";
                    data.success = false;

                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (data.success)
                    _unitOfWorkManage.CommitTran();
                else
                    _unitOfWorkManage.RollbackTran();
            }
            return data;
        }


        /// <summary>
        /// 修改计划任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            if (tasksQz != null && tasksQz.Id > 0)
            {
                _unitOfWorkManage.BeginTran();
                data.success = await _tasksQzServices.Dal.Update(tasksQz);
                try
                {
                    if (data.success)
                    {
                        data.msg = "修改成功";
                        data.response = tasksQz?.Id.ObjToString();
                        if (tasksQz.IsStart)
                        {
                            var ResuleModelStop = await _schedulerCenter.StopScheduleJobAsync(tasksQz);
                            data.msg = $"{data.msg}=>停止:{ResuleModelStop.msg}";
                            var ResuleModelStar = await _schedulerCenter.AddScheduleJobAsync(tasksQz);
                            data.success = ResuleModelStar.success;
                            data.msg = $"{data.msg}=>启动:{ResuleModelStar.msg}";
                        }
                        else
                        {
                            var ResuleModelStop = await _schedulerCenter.StopScheduleJobAsync(tasksQz);
                            data.msg = $"{data.msg}=>停止:{ResuleModelStop.msg}";
                        }
                    }
                    else
                    {
                        data.msg = "修改失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            return data;
        }
        /// <summary>
        /// 删除一个任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                data.success = await _tasksQzServices.Dal.Delete(model);
                try
                {
                    data.response = jobId.ObjToString();
                    if (data.success)
                    {
                        data.msg = "删除成功";
                        var ResuleModel = await _schedulerCenter.StopScheduleJobAsync(model);
                        data.msg = $"{data.msg}=>任务状态=>{ResuleModel.msg}";
                    }
                    else
                    {
                        data.msg = "删除失败";
                    }

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;

        }
        /// <summary>
        /// 启动计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> StartJob(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.success = await _tasksQzServices.Dal.Update(model);
                    data.response = jobId.ObjToString();
                    if (data.success)
                    {
                        data.msg = "更新成功";
                        var ResuleModel = await _schedulerCenter.AddScheduleJobAsync(model);
                        data.success = ResuleModel.success;
                        if (ResuleModel.success)
                        {
                            data.msg = $"{data.msg}=>启动成功=>{ResuleModel.msg}";

                        }
                        else
                        {
                            data.msg = $"{data.msg}=>启动失败=>{ResuleModel.msg}";
                        }
                    }
                    else
                    {
                        data.msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;
        }
        /// <summary>
        /// 停止一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>        
        [HttpGet]
        public async Task<MessageModel<string>> StopJob(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                model.IsStart = false;
                data.success = await _tasksQzServices.Dal.Update(model);
                data.response = jobId.ObjToString();
                if (data.success)
                {
                    data.msg = "更新成功";
                    var ResuleModel = await _schedulerCenter.StopScheduleJobAsync(model);
                    if (ResuleModel.success)
                    {
                        data.msg = $"{data.msg}=>停止成功=>{ResuleModel.msg}";
                    }
                    else
                    {
                        data.msg = $"{data.msg}=>停止失败=>{ResuleModel.msg}";
                    }
                }
                else
                {
                    data.msg = "更新失败";
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;
        }
        /// <summary>
        /// 暂停一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>        
        [HttpGet]
        public async Task<MessageModel<string>> PauseJob(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    data.success = await _tasksQzServices.Dal.Update(model);
                    data.response = jobId.ObjToString();
                    if (data.success)
                    {
                        data.msg = "更新成功";
                        var ResuleModel = await _schedulerCenter.PauseJob(model);
                        if (ResuleModel.success)
                        {
                            data.msg = $"{data.msg}=>暂停成功=>{ResuleModel.msg}";
                        }
                        else
                        {
                            data.msg = $"{data.msg}=>暂停失败=>{ResuleModel.msg}";
                        }
                        data.success = ResuleModel.success;
                    }
                    else
                    {
                        data.msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;
        }
        /// <summary>
        /// 恢复一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>        
        [HttpGet]
        public async Task<MessageModel<string>> ResumeJob(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.success = await _tasksQzServices.Dal.Update(model);
                    data.response = jobId.ObjToString();
                    if (data.success)
                    {
                        data.msg = "更新成功";
                        var ResuleModel = await _schedulerCenter.ResumeJob(model);
                        if (ResuleModel.success)
                        {
                            data.msg = $"{data.msg}=>恢复成功=>{ResuleModel.msg}";
                        }
                        else
                        {
                            data.msg = $"{data.msg}=>恢复失败=>{ResuleModel.msg}";
                        }
                        data.success = ResuleModel.success;
                    }
                    else
                    {
                        data.msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;
        }
        /// <summary>
        /// 重启一个计划任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ReCovery(long jobId)
        {
            var data = new MessageModel<string>();
            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {

                _unitOfWorkManage.BeginTran();
                try
                {
                    model.IsStart = true;
                    data.success = await _tasksQzServices.Dal.Update(model);
                    data.response = jobId.ObjToString();
                    if (data.success)
                    {
                        data.msg = "更新成功";
                        var ResuleModelStop = await _schedulerCenter.StopScheduleJobAsync(model);
                        var ResuleModelStar = await _schedulerCenter.AddScheduleJobAsync(model);
                        if (ResuleModelStar.success)
                        {
                            data.msg = $"{data.msg}=>停止:{ResuleModelStop.msg}=>启动:{ResuleModelStar.msg}";
                            data.response = jobId.ObjToString();
                        }
                        else
                        {
                            data.msg = $"{data.msg}=>停止:{ResuleModelStop.msg}=>启动:{ResuleModelStar.msg}";
                            data.response = jobId.ObjToString();
                        }
                        data.success = ResuleModelStar.success;
                    }
                    else
                    {
                        data.msg = "更新失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;

        }
        /// <summary>
        /// 获取任务命名空间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<List<QuartzReflectionViewModel>> GetTaskNameSpace()
        {
            var baseType = typeof(IJob); 
            var types = Assembly.GetEntryAssembly()
                .GetExportedTypes()
                .Where(x => x != baseType && baseType.IsAssignableFrom(x))
                .ToArray();
            var implementTypes = types.Where(x => x.IsClass).Select(type => {
                var temp = new QuartzReflectionViewModel { nameSpace = type.Namespace, nameClass = type.Name, remark = "" };
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(JobDescriptionAttribute));
                if (tempAttr != null)
                {
                    temp.name = ((JobDescriptionAttribute)tempAttr).Name;
                    temp.description = ((JobDescriptionAttribute)tempAttr).Description;
                } 
                return temp;
            }).ToList();

            return MessageModel<List<QuartzReflectionViewModel>>.Success("获取成功", implementTypes);
        }

        /// <summary>
        /// 立即执行任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> ExecuteJob(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await _tasksQzServices.Dal.QueryById(jobId);
            if (model != null)
            {
                return await _schedulerCenter.ExecuteJobAsync(model);
            }
            else
            {
                data.msg = "任务不存在";
                data.success = false;
            }
            return data;
        }
        /// <summary>
        /// 获取任务运行日志
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<PageModel<TasksLog>>> GetTaskLogs(long jobId, int page = 1, int pageSize = 10, DateTime? runTimeStart = null, DateTime? runTimeEnd = null)
        {
            var model = await _tasksLogServices.GetTaskLogs(jobId, page, pageSize, runTimeStart, runTimeEnd);
            return MessageModel<PageModel<TasksLog>>.Message(model.dataCount >= 0, "获取成功", model);
        }
        /// <summary>
        /// 任务概况
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<object>> GetTaskOverview(long jobId, int page = 1, int pageSize = 10, DateTime? runTimeStart = null, DateTime? runTimeEnd = null, string type = "month")
        {
            var model = await _tasksLogServices.GetTaskOverview(jobId, runTimeStart, runTimeEnd, type);
            return MessageModel<object>.Message(true, "获取成功", model);
        }

    }
}
