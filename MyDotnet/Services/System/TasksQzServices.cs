using Microsoft.AspNetCore.Mvc;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using SqlSugar;
using System.Linq.Expressions;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// TasksQzServices
    /// </summary>
    public partial class TasksQzServices : BaseServices<TasksQz>
    {

        public UnitOfWorkManage _unitOfWorkManage;
        public SchedulerCenterServer _schedulerCenter;
        public TasksQzServices(BaseRepository<TasksQz> baseRepository
            , UnitOfWorkManage unitOfWorkManage
            , SchedulerCenterServer schedulerCenter
            ) : base(baseRepository)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _schedulerCenter = schedulerCenter;
        }
        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="tasksQz"></param>
        /// <returns></returns>
        public async Task<MessageModel<string>> AddTask(TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            _unitOfWorkManage.BeginTran();
            var id = await Dal.Add(tasksQz);
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
        public async Task<MessageModel<string>> EditTast(TasksQz tasksQz)
        {
            var data = new MessageModel<string>();
            if (tasksQz != null && tasksQz.Id > 0)
            {
                _unitOfWorkManage.BeginTran();
                data.success = await Dal.Update(tasksQz);
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
        public async Task<MessageModel<string>> DeleteTask(long jobId)
        {
            var data = new MessageModel<string>();

            var model = await Dal.QueryById(jobId);
            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                data.success = await Dal.Delete(model);
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
    }
}
