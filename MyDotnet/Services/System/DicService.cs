using MyDotnet.Common.Cache;
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
        private UnitOfWorkManage _unitOfWorkManage;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="baseRepository"></param>
        /// <param name="dicBaseRepository"></param>
        /// <param name="caching"></param>
        /// <param name="unitOfWorkManage"></param>
        public DicService(BaseRepository<DicType>  baseRepository
            , BaseRepository<DicData> dicBaseRepository
            , ICaching caching
            , UnitOfWorkManage unitOfWorkManage
            ) : base(baseRepository)
        {
            _dicType = baseRepository;
            _dicData = dicBaseRepository;
            _caching = caching;
            _unitOfWorkManage = unitOfWorkManage;
        }
        /// <summary>
        /// 获取一个字典类型值(缓存用)
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
        /// 获取一个字典类型列表值(缓存用)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<List<DicData>> GetDicData(string code)
        {

            var data = await _caching.GetAsync<List<DicData>>(code);
            if(data == null)
            {
                data = await _dicData.Query(t => t.pCode == code && t.Enabled == true, "codeOrder asc");
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
        /// 获取一个字典类型列表值(缓存用)
        /// </summary>
        /// <param name="pCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<DicData> GetDicDataOne(string pCode,string code)
        {

            var data = await _caching.GetAsync<List<DicData>>(pCode);
            if (data == null)
            {
                data = await _dicData.Query(t => t.pCode == pCode && t.Enabled == true, "codeOrder asc");
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

        /// <summary>
        /// 添加一个字典类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        public async Task<long> AddDicType(DicType data)
        {
            var id = await _dicType.Add(data);
            await _caching.RemoveAsync(data.code);
            return id;
        }


        /// <summary>
        /// 更新一个字典类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        public async Task<bool> PutDicType(DicType data)
        {
            try
            {
                _unitOfWorkManage.BeginTran();

                var oldData = await _dicType.QueryById(data.Id);

                await _dicType.Update(data);
                if (!oldData.code.Equals(data.code))
                {
                    //修改code后同步修改子集
                    await _dicData.Db.Updateable<DicData>().SetColumns(t => t.pCode, data.code).Where(t => t.pCode == oldData.code).ExecuteCommandAsync();

                    
                }
                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            await _caching.RemoveAsync(data.code);
            await _caching.RemoveAsync($"{data.code}_list");
            return true;
        }


        /// <summary>
        /// 添加一个字典子集类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        public async Task<long> AddDicData(DicData data)
        {
            var id = await _dicData.Add(data);
            await _caching.RemoveAsync($"{data.pCode}_list");
            return id;
        }


        /// <summary>
        /// 更新一个字典子集类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        public async Task<bool> PutDicData(DicData data)
        {
            var isOk = await _dicData.Update(data);
            await _caching.RemoveAsync($"{data.pCode}_list");
            return isOk;
        }





    }
}
