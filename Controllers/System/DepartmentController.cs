
using AppStoreConnect.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Services;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Text;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 部门管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class DepartmentController : BaseApiController
    {
        public BaseServices<Department> _departmentServices;
        public DepartmentController(BaseServices<Department> departmentServices)
        {
            _departmentServices = departmentServices;
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<Department>>> Get(int page = 1, string key = "", int size = 50)
        {


            Expression<Func<Department, bool>> whereExpression = a => true; 
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrWhiteSpace(key))
            {
                whereExpression = whereExpression.And(t => t.Name.Contains(key.Trim()));
            }
            return new MessageModel<PageModel<Department>>()
            {
                msg = "获取成功",
                success = true,
                response = await _departmentServices.Dal.QueryPage(whereExpression, page, size)
            };

        }

        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Department request)
        {
            var data = new MessageModel<string>();

            var id = await _departmentServices.Dal.Add(request);
            data.success = id > 0;
            if (data.success)
            {
                data.response = id.ObjToString();
                data.msg = "添加成功";
            }

            return data;
        }

        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Department request)
        {
            var data = new MessageModel<string>();
            data.success = await _departmentServices.Dal.Update(request);
            if (data.success)
            {
                data.msg = "更新成功";
                data.response = request?.Id.ObjToString();
            }

            return data;
        }

        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            var model = await _departmentServices.Dal.QueryById(id);
            model.IsDeleted = true;
            data.success = await _departmentServices.Dal.Update(model);
            if (data.success)
            {
                data.msg = "删除成功";
                data.response = model?.Id.ObjToString();
            }
            return data;
        }


        /// <summary>
        /// 查询树形 Table
        /// </summary>
        /// <param name="pid">父节点</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<Department>>> GetTreeTable(long pid = 0, string key = "")
        {
            List<Department> rootDepartments = new List<Department>();
            var departmentList = await _departmentServices.Dal.Query();
            key = key.ObjToString().Trim();
             
            if (string.IsNullOrEmpty(key))
            {
                rootDepartments = departmentList.Where(a => a.Pid == pid).OrderBy(a => a.OrderSort).ToList();
            }
            else
            {
                rootDepartments = departmentList.Where(a => a.Name.Contains(key)).OrderBy(a => a.OrderSort).ToList();
            }
            HandleDepartment(rootDepartments, departmentList);
            return Success(rootDepartments, "获取成功");
        }
        private void HandleDepartment(List<Department> rootDepartments, List<Department> departmentList)
        {
            foreach (var item in rootDepartments)
            {
                departmentList.Remove(item);
                if (item.Id >0) {
                    var parent = departmentList.Find(t => t.Id == item.Pid);
                    if (parent != null)  item.PidName = parent.Name;
                }

                var childList = departmentList.FindAll(t => t.Pid == item.Id).OrderBy(t=>t.OrderSort).ToList();
                if (childList.Count > 0)
                {
                    //子节点
                    //item.hasChildren = true;
                    item.children = childList;
                    item.children.ForEach(child => { child.PidName = item.Name; });
                    HandleDepartment(item.children, departmentList);
                }
                else
                {
                    //叶子结点
                    //item.hasChildren = false;
                }
            }
        }

        /// <summary>
        /// 获取部门树
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<DepartmentTree>> GetDepartmentTree(long pid = 0)
        {
            var departments = await _departmentServices.Dal.Query();
            var departmentTrees = (from child in departments
                                   orderby child.OrderSort
                                   select new DepartmentTree
                                   {
                                       value = child.Id,
                                       label = child.Name,
                                       Pid = child.Pid,
                                       order = child.OrderSort,
                                   }).ToList();
            DepartmentTree rootRoot = new DepartmentTree
            {
                value = 0,
                Pid = 0,
                label = "根节点"
            };

            departmentTrees = departmentTrees.OrderBy(d => d.order).ToList();


            RecursionHelper.LoopToAppendChildren(departmentTrees, rootRoot, pid);

            return Success(rootRoot, "获取成功");
        }
    }
}