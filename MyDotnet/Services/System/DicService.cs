﻿using MyDotnet.Common.Cache;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// 字典服务类
    /// </summary>
    public class DicService: BaseServices<DicType>
    {
        private BaseRepository<DicType> _dicType;
        private BaseRepository<DicData> _dicData;
        private ICaching _caching;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseRepository"></param>
        /// <param name="dicBaseRepository"></param>
        /// <param name="caching"></param>
        public DicService(BaseRepository<DicType>  baseRepository
            , BaseRepository<DicData> dicBaseRepository
            , ICaching caching) : base(baseRepository)
        {
            _dicType = baseRepository;
            _dicData = dicBaseRepository;
            _caching = caching;
        }
        /// <summary>
        /// 获取一个字典类型值
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<DicType> GetDic(string code)
        {
            var data = await _caching.GetAsync<DicType>(code);
            if(data == null)
            {
                //缓存穿透
                var ls = await _dicType.Query(t => t.code == code);
                if (ls == null || ls.Count == 0)
                {
                    throw new ServiceException($"字典[{code}]不存在");
                }
                data = ls.FirstOrDefault();
                //设置缓存
                _caching.Set(code, data);
            }
            return data;
        }

        /// <summary>
        /// 获取一个字典类型列表值
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<List<DicData>> GetDicData(string code)
        {

            var data = await _caching.GetAsync<List<DicData>>(code);
            if(data == null)
            {
                data = await _dicData.Query(t => t.pCode == code, "codeOrder asc");
                if (data == null || data.Count == 0)
                {
                    throw new ServiceException($"字典[{code}]不存在");
                }
                //设置缓存
                _caching.Set($"{code}_list", data);
            }
            return data;
        }
        /// <summary>
        /// 获取一个字典类型列表值
        /// </summary>
        /// <param name="pCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<DicData> GetDicDataOne(string pCode,string code)
        {

            var data = await _caching.GetAsync<List<DicData>>(pCode);
            if (data == null)
            {
                data = await _dicData.Query(t => t.pCode == pCode, "codeOrder asc");
                if (data == null || data.Count == 0)
                {
                    throw new ServiceException($"字典[{code}]不存在");
                }

                //设置缓存
                _caching.Set($"{code}_list", data);
            }
            var one = data.Find(t => t.code == code);
            return one;
        }



    }
}
