using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyDotnet.Common.Cache;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.System;
using System.Collections.Generic;

namespace MyDotnet.Controllers.System
{
    /// <summary>
    /// 字典管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DicController : BaseApiController
    {
        DicService _dicService;
        BaseServices<DicType> _dicType;
        BaseServices<DicData> _dicData;
        ICaching _caching;

        UnitOfWorkManage _unitOfWorkManage;

        public DicController(DicService dicService
            , BaseServices<DicType> dicType
            , BaseServices<DicData> dicData
            , ICaching caching
            , UnitOfWorkManage unitOfWorkManage)
        {
            _dicService = dicService;
            _dicType = dicType;
            _dicData = dicData;
            _caching = caching;
            _unitOfWorkManage = unitOfWorkManage;
        }
        /// <summary>
        /// 获取字典类型(缓存用)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<DicType>> GetDic(string code)
        {
            var data = await _dicService.GetDic(code);
            return Success(data);
        }
        /// <summary>
        /// 获取字典类型列表(缓存用)
        /// </summary>
        /// <param name="code"></param>
        /// <param name="key">子集</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<List<DicData>>> GetDicData(string code,string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                var data = await _dicService.GetDicData(code);
                return Success(data);
            }
            else
            {
                List<DicData> ls = new List<DicData>();
                var data = await _dicService.GetDicData(code,key);
                ls.Add(data);
                return Success(ls);
            }
            
        }

        /// <summary>
        /// 获取某个字典列表中的值(缓存用)
        /// </summary>
        /// <param name="pCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<DicData>> GetDicDataOne(string pCode, string code)
        {
            var data = await _dicService.GetDicDataOne(pCode, code);
            return Success(data);
        }










        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<PageModel<DicType>>> Get(int page = 1, int size = 10, string key = "")
        {
            var whereFind = LinqHelper.True<DicType>();

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.code.Contains(key) 
                || t.name.Contains(key)  
                || t.content.Contains(key)
                || t.content2.Contains(key)
                || t.content3.Contains(key)
                || t.description.Contains(key));
            }
            var data = await _dicType.Dal.QueryPage(whereFind, page, size, " Id desc ");
            return Success(data, "获取成功");
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Post([FromBody] DicType data)
        {
            var id = await _dicService.AddDicType(data);
            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed();

        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Put([FromBody] DicType data)
        {
            if (data == null || data.Id <= 0)
                return Failed("缺少参数");
            await _dicService.PutDicType(data);
            return Success("更新成功");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Delete(long id)
        {
            if (id <= 0)
                return Failed("缺少参数");
            var data = await _dicType.Dal.QueryById(id);
            if(data == null)
                return Failed("数据不存在");
            var childData = await _dicData.Dal.Query(t => t.pCode == data.code);
            if (childData.Count > 0)
            {
                return Failed($"{data.code}存在子项数据,请先删除子项数据");
            }
            var isOk = await _dicType.Dal.DeleteById(id);
            await _caching.RemoveAsync(data.code);
            if (isOk)
                return Success("","删除成功");
            return Failed();
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> Deletes([FromBody] object[] ids)
        {

            var ls = await _dicType.Dal.QueryByIDs(ids);
            foreach (var data in ls)
            {
                var childData = await _dicData.Dal.Query(t => t.pCode == data.code);
                if (childData.Count > 0)
                {
                    return Failed($"{data.code}存在子项数据,请先删除子项数据");
                }
            }
            
            var isOk = await _dicType.Dal.DeleteByIds(ids);
            if (isOk)
                return Success("", "删除成功");
            foreach (var data in ls)
            {
                await _caching.RemoveAsync(data.code);
            }
            return Failed();
        }






        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<PageModel<DicData>>> DicDataGet(string code,int page = 1, int size = 10, string key = "")
        {
            var whereFind = LinqHelper.True<DicData>();
            if (string.IsNullOrEmpty(code))
            {
                return MessageModel<PageModel<DicData>>.Fail("请选择一个要查询的字典");
            }
            else
            {
                whereFind = whereFind.And(t => t.pCode.Equals(code));
            }

            if (!string.IsNullOrEmpty(key))
            {
                whereFind = whereFind.And(t => t.name.Contains(key)
                || t.code.Contains(key)
                || t.content.Contains(key)
                || t.content2.Contains(key)
                || t.content3.Contains(key)
                || t.description.Contains(key));
            }
            var data = await _dicData.Dal.QueryPage(whereFind, page, size, "codeOrder asc");
            return Success(data, "获取成功");
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> DicDataPost([FromBody] DicData data)
        {
            var id = await _dicService.AddDicData(data); 
            return id > 0 ? Success(id.ObjToString(), "添加成功") : Failed();

        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> DicDataPut([FromBody] DicData data)
        {
            if (data == null || data.Id <= 0)
                return Failed("缺少参数");
            var isOk = await _dicService.PutDicData(data);
            return isOk ? Success(data.Id.ObjToString(), "更新成功") : Failed();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> DicDataDelete(long id)
        {
            if (id <= 0)
                return Failed("缺少参数");
            var data = await _dicData.Dal.QueryById(id);
            var isOk = await _dicData.Dal.DeleteById(id);
            if (isOk)
            {
                await _caching.RemoveAsync($"{data.pCode}_list");
                return Success("", "删除成功");
            }
            return Failed();
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Permissions.Name)]
        public async Task<MessageModel<string>> DicDataDeletes([FromBody] object[] ids)
        {

            var data = await _dicData.Dal.QueryByIDs(ids);
            if(data.Count == 0) return Failed("没有要删除的数据");
            var isOk = await _dicData.Dal.DeleteByIds(ids);
            if (isOk)
            {
                await _caching.RemoveAsync($"{data[0].pCode}_list");
                return Success("", "删除成功");
            }
            return Failed();
        }



    }
}
