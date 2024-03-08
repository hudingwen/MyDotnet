using MyDotnet.Domain.Dto.System;
using MyDotnet.Helper;
using SqlSugar;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace MyDotnet.Repository
{
    /// <summary>
    /// 泛型仓储实现类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class BaseRepository<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// 全局事务
        /// </summary>
        public UnitOfWorkManage _unitOfWorkManage { get; set; }
        /// <summary>
        /// 当前数据库链接
        /// </summary>
        public ISqlSugarClient Db { get; set; }

        public BaseRepository(UnitOfWorkManage unitOfWorkManage)
        {
            _unitOfWorkManage = unitOfWorkManage;
            ISqlSugarClient _Db = _unitOfWorkManage.db;

            //多库判断
            if (DbConfigHelper.MutiDBEnabled)
            {
                var tenantAttr = typeof(TEntity).GetCustomAttribute<TenantAttribute>();
                if (tenantAttr != null)
                {
                    //开启多库且实体是多库标识
                    _Db = _unitOfWorkManage.db.GetConnectionScope(tenantAttr.configId);
                }
            }
            Db = _Db;
        }


        /// <summary>
        /// 根据id主键查询
        /// 温馨提示: 联合主键请用where条件
        /// </summary>
        /// <param name="objId">id</param>
        /// <param name="useCache">是否缓存 默认:否</param>
        /// <param name="cacheSecond">缓存时间 默认:10秒</param>
        /// <returns></returns>
        public async Task<TEntity> QueryById(object objId, bool useCache = false, int cacheSecond = 10)
        {
            return await Db.Queryable<TEntity>().WithCacheIF(useCache, cacheSecond).In(objId).SingleAsync();
        }
        /// <summary>
        /// 根据多个id查询
        /// </summary>
        /// <param name="lstIds">ids</param>
        /// <param name="useCache">是否缓存 默认:否</param>
        /// <param name="cacheSecond">缓存时间 默认:10秒</param>
        /// <returns></returns>
        public async Task<List<TEntity>> QueryByIDs(object[] lstIds, bool useCache = false, int cacheSecond = 10)
        {
            return await Db.Queryable<TEntity>().WithCacheIF(useCache, cacheSecond).In(lstIds).ToListAsync();
        }
        /// <summary>
        /// 写入单个实体数据
        /// </summary>
        /// <param name="entity">要写入的数据</param>
        /// <param name="insertColumns">要写入的字段[可选]</param>
        /// <returns></returns>
        public async Task<long> Add(TEntity entity, Expression<Func<TEntity, object>> insertColumns = null)
        {
            //默认雪花id
            if (insertColumns == null)
                return await Db.Insertable(entity).ExecuteReturnSnowflakeIdAsync();
            else
                return await Db.Insertable(entity).InsertColumns(insertColumns).ExecuteReturnSnowflakeIdAsync();

            //这里你可以返回TEntity，这样的话就可以获取id值，无论主键是什么类型
            //var return3 = await insert.ExecuteReturnEntityAsync();
        }
        /// <summary>
        /// 写入多个实体数据 - 速度快
        /// </summary>
        /// <param name="listEntity">实体列表</param>
        /// <param name="insertColumns">要写入的字段[可选]</param>
        /// <returns></returns>
        public async Task<List<long>> Add(TEntity[] listEntity, Expression<Func<TEntity, object>> insertColumns = null)
        {
            return await Add(listEntity.ToList(), insertColumns);
        }
        /// <summary>
        /// 写入多个实体数据 - 速度快
        /// </summary>
        /// <param name="listEntity">实体列表</param>
        /// <param name="insertColumns">要写入的字段[可选]</param>
        /// <returns></returns>
        public async Task<List<long>> Add(List<TEntity> listEntity, Expression<Func<TEntity, object>> insertColumns = null)
        {
            if (insertColumns == null)
                return await Db.Insertable(listEntity).ExecuteReturnSnowflakeIdListAsync();
            else
                return await Db.Insertable(listEntity).InsertColumns(insertColumns).ExecuteReturnSnowflakeIdListAsync();
        }
        /// <summary>
        /// 更新实体数据
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <param name="updateColumns">要更新的字段[可选]</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity entity, Expression<Func<TEntity, object>> updateColumns = null)
        {
            if (updateColumns == null)
                return await Db.Updateable(entity).ExecuteCommandHasChangeAsync();
            else
                return await Db.Updateable(entity).UpdateColumns(updateColumns).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 更新多个实体数据 - 返回成功与否
        /// </summary>
        /// <param name="entity">实体列表</param>
        /// <param name="updateColumns">要更新的字段[可选]</param>
        /// <returns></returns>
        public async Task<bool> Update(List<TEntity> entity, Expression<Func<TEntity, object>> updateColumns = null)
        {
            if (updateColumns == null)
                return await Db.Updateable(entity).ExecuteCommandHasChangeAsync();
            else
                return await Db.Updateable(entity).UpdateColumns(updateColumns).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 更新多个实体数据 - 返回成功与否
        /// </summary>
        /// <param name="entity">实体列表</param>
        /// <param name="updateColumns">要更新的字段[可选]</param>
        /// <returns></returns>
        public async Task<bool> Update(TEntity[] entity, Expression<Func<TEntity, object>> updateColumns = null)
        {
            return await Update(entity.ToList(), updateColumns);
        }

        /// <summary>
        /// 更新多个实体数据 - 返回影响行数
        /// </summary>
        /// <param name="entity">实体列表</param>
        /// <param name="updateColumns">要更新的字段[可选]</param>
        /// <returns></returns>
        public async Task<int> UpdateCount(List<TEntity> entity, Expression<Func<TEntity, object>> updateColumns = null)
        {
            if (updateColumns == null)
                return await Db.Updateable(entity).ExecuteCommandAsync();
            else
                return await Db.Updateable(entity).UpdateColumns(updateColumns).ExecuteCommandAsync();
        }
        /// <summary>
        /// 更新多个实体数据 - 返回影响行数
        /// </summary>
        /// <param name="entity">实体列表</param>
        /// <param name="updateColumns">要更新的字段[可选]</param>
        /// <returns></returns>
        public async Task<int> UpdateCount(TEntity[] entity, Expression<Func<TEntity, object>> updateColumns = null)
        {
            return await UpdateCount(entity.ToList(), updateColumns);
        }
        /// <summary>
        /// 删除一条实体数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Delete(TEntity entity)
        {
            return await Db.Deleteable(entity).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 返回成功与否
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Delete(List<TEntity> entity)
        {
            return await Db.Deleteable(entity).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 返回成功与否
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<bool> Delete(TEntity[] entity)
        {
            return await Delete(entity.ToList());
        }
        /// <summary>
        /// 删除多条实体数据 - 返回影响行数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> DeleteCount(List<TEntity> entity)
        {
            return await Db.Deleteable(entity).ExecuteCommandAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 返回影响行数
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> DelDeleteCountete(TEntity[] entity)
        {
            return await DeleteCount(entity.ToList());
        }
        /// <summary>
        /// 删除一条实体数据 - 根据id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteById(object id)
        {
            return await Db.Deleteable<TEntity>().In(id).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 根据ids - 返回成功与否
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByIds(object[] ids)
        {
            return await Db.Deleteable<TEntity>().In(ids).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 根据ids - 返回成功与否
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> DeleteByIds(List<object> ids)
        {
            return await DeleteByIds(ids.ToArray());
        }
        /// <summary>
        /// 删除多条实体数据 - 根据ids - 返回影响行数
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<int> DeleteByIdsCount(object[] ids)
        {
            return await Db.Deleteable<TEntity>().In(ids).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除多条实体数据 - 根据条件 - 返回成功与否
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<bool> Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommandHasChangeAsync();
        }
        /// <summary>
        /// 删除多条实体数据 - 根据条件 - 返回成功与否
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<int> DeleteCount(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Db.Deleteable<TEntity>().Where(whereExpression).ExecuteCommandAsync();
        }


        /// <summary>
        /// 查询实体列表  - 并返回特定实体
        /// </summary>
        /// <typeparam name="TResult">特定实体</typeparam>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="expression">要查询的字段[可选]</param>
        /// <param name="orderByFields">排序条件[可选]</param>
        /// <returns></returns>
        public async Task<List<TResult>> QueryMap<TResult>(Expression<Func<TEntity, bool>> whereExpression
            , Expression<Func<TEntity, TResult>> expression)
        {
            return await QueryMap(whereExpression, expression, string.Empty);
        }

        /// <summary>
        /// 查询实体列表  - 并返回特定实体
        /// </summary>
        /// <typeparam name="TResult">特定实体</typeparam>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="expression">要查询的字段</param>
        /// <param name="orderByFields">排序条件</param>
        /// <returns></returns>
        public async Task<List<TResult>> QueryMap<TResult>(Expression<Func<TEntity, bool>> whereExpression
            , Expression<Func<TEntity, TResult>> expression
            , string orderByFields)
        {
            return await Db.Queryable<TEntity>().Where(whereExpression).Select(expression)
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .ToListAsync();
        }




        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<TEntity>> Query()
        {
            return await Query(string.Empty, string.Empty);
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="orderByFields">排序条件</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(string where, string orderByFields)
        {
            return await Db.Queryable<TEntity>().WhereIF(!string.IsNullOrEmpty(where), where)
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .ToListAsync();
        }



        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression)
        {
            return await Query(whereExpression, string.Empty);
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="orderByFields">排序条件</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression, string orderByFields)
        {
            return await Db.Queryable<TEntity>().WhereIF(whereExpression != null, whereExpression)
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .ToListAsync();
        }

        /// <summary>
        /// 查询实体列表
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="orderExpression">排序字段</param>
        /// <param name="orderByType">排序方式</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression
            , Expression<Func<TEntity, object>> orderExpression, OrderByType orderByType)
        {
            return await Db.Queryable<TEntity>().WhereIF(whereExpression != null, whereExpression)
                .OrderBy(orderExpression, orderByType)
                .ToListAsync();
        }


        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="top">前N条</param>
        /// <param name="orderByFields">排序条件[可选] 例如: name asc,age desc</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(
            string where,
            int top,
            string orderByFields)
        {
            return await Db.Queryable<TEntity>().OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(!string.IsNullOrEmpty(where), where)
                .Take(top)
                .ToListAsync();
        }


        /// <summary>
        /// 查询前N条数据
        /// </summary>
        /// <param name="whereExpression">查询条件</param>
        /// <param name="top">前N条</param>
        /// <param name="orderExpression">排序字段</param>
        /// <param name="orderByType">排序方式</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(
            Expression<Func<TEntity, bool>> whereExpression,
            int top,
            Expression<Func<TEntity, object>> orderExpression,
            OrderByType orderByType)
        {
            return await Db.Queryable<TEntity>().OrderBy(orderExpression, orderByType)
                .WhereIF(whereExpression != null, whereExpression)
                .Take(top)
                .ToListAsync();
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">完整的sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public async Task<List<TEntity>> QuerySqlList(string sql, SugarParameter[] parameters = null)
        {
            return await Db.Ado.SqlQueryAsync<TEntity>(sql, parameters);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">完整的sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public async Task<DataTable> QuerySqlTable(string sql, SugarParameter[] parameters = null)
        {
            return await Db.Ado.GetDataTableAsync(sql, parameters);
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(
            string where,
            int pageIndex,
            int pageSize,
            string orderByFields)
        {
            return await Db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(!string.IsNullOrEmpty(where), where)
                .ToPageListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where">条件</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <param name="total">返回总数</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(
            string where,
            int pageIndex,
            int pageSize,
            string orderByFields,
            RefAsync<int> total)
        {
            return await Db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(!string.IsNullOrEmpty(where), where)
                .ToPageListAsync(pageIndex, pageSize, total);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderExpression">排序字段</param>
        /// <param name="orderByType">排序方式</param>
        /// <param name="total">返回总数</param>
        /// <returns></returns>
        public async Task<List<TEntity>> Query(
            Expression<Func<TEntity, bool>> whereExpression,
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, object>> orderExpression,
            OrderByType orderByType,
            RefAsync<int> total)
        {
            return await Db.Queryable<TEntity>()
                .OrderBy(orderExpression, orderByType)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageListAsync(pageIndex, pageSize, total);
        }


        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<PageModel<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression, int pageIndex = 1, int pageSize = 10, string orderByFields = null)
        {
            RefAsync<int> totalCount = 0;
            var list = await Db.Queryable<TEntity>()
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageListAsync(pageIndex, pageSize, totalCount);

            return new PageModel<TEntity>(pageIndex, totalCount, pageSize, list);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<PageModel<TEntity>> QueryPage(Expression<Func<TEntity, bool>> whereExpression,
            int pageIndex,
            int pageSize,
            Expression<Func<TEntity, object>> orderExpression,
            OrderByType orderByType)
        {
            RefAsync<int> totalCount = 0;
            var list = await Db.Queryable<TEntity>()
                .OrderBy(orderExpression, orderByType)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageListAsync(pageIndex, pageSize, totalCount);
            return new PageModel<TEntity>(pageIndex, totalCount, pageSize, list);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="pageIndex">页码（下标0）</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc</param>
        /// <returns></returns>
        public async Task<PageModel<TEntity>> QueryPage(PaginationModel pagination)
        {
            var express = DynamicLinqFactory.CreateLambda<TEntity>(pagination.conditions);
            return await QueryPage(express, pagination.page, pagination.size, pagination.orderByFileds);
        }


        /// <summary> 
        ///多表查询
        /// </summary> 
        /// <typeparam name="T">实体1</typeparam> 
        /// <typeparam name="T2">实体2</typeparam> 
        /// <typeparam name="T3">实体3</typeparam>
        /// <typeparam name="TResult">返回对象</typeparam>
        /// <param name="joinExpression">关联表达式 (join1,join2) => new object[] {JoinType.Left,join1.UserNo==join2.UserNo}</param> 
        /// <param name="selectExpression">返回表达式 (s1, s2) => new { Id =s1.UserNo, Id1 = s2.UserNo}</param>
        /// <param name="whereLambda">查询表达式 (w1, w2) =>w1.UserNo == "")</param> 
        /// <returns>值</returns>
        public async Task<List<TResult>> QueryMuch<T, T2, T3, TResult>(
            Expression<Func<T, T2, T3, object[]>> joinExpression,
            Expression<Func<T, T2, T3, TResult>> selectExpression,
            Expression<Func<T, T2, T3, bool>> whereLambda) where T : class, new()
        {
            return await Db.Queryable(joinExpression).WhereIF(whereLambda != null, whereLambda).Select(selectExpression).ToListAsync();
        }


        /// <summary> 
        ///多表查询
        /// </summary> 
        /// <typeparam name="T">实体1</typeparam> 
        /// <typeparam name="T2">实体2</typeparam> 
        /// <typeparam name="T3">实体3</typeparam>
        /// <typeparam name="TResult">返回对象</typeparam>
        /// <param name="joinExpression">关联表达式 (join1,join2) => new object[] {JoinType.Left,join1.UserNo==join2.UserNo}</param> 
        /// <param name="selectExpression">返回表达式 (s1, s2) => new { Id =s1.UserNo, Id1 = s2.UserNo}</param>
        /// <param name="whereLambda">查询表达式 (w1, w2) =>w1.UserNo == "")</param> 
        /// <returns>值</returns>
        public async Task<List<TResult>> QueryMuch<T, T2, T3, TResult>(
            Expression<Func<T, T2, T3, object[]>> joinExpression,
            Expression<Func<T, T2, T3, TResult>> selectExpression) where T : class, new()
        {
            return await QueryMuch(joinExpression, selectExpression, null);
        }


        /// <summary>
        /// 两表联合查询 - 分页
        /// </summary>
        /// <typeparam name="T">实体1</typeparam>
        /// <typeparam name="T2">实体1</typeparam>
        /// <typeparam name="TResult">返回对象</typeparam>
        /// <param name="joinExpression">关联表达式</param>
        /// <param name="selectExpression">返回表达式</param>
        /// <param name="whereExpression">查询表达式</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段</param>
        /// <returns></returns>
        public async Task<PageModel<TResult>> QueryTabsPage<T, T2, TResult>(
            Expression<Func<T, T2, object[]>> joinExpression,
            Expression<Func<T, T2, TResult>> selectExpression,
            Expression<Func<TResult, bool>> whereExpression,
            int pageIndex = 1,
            int pageSize = 10,
            string orderByFields = null)
        {
            RefAsync<int> totalCount = 0;
            var list = await Db.Queryable(joinExpression)
                .Select(selectExpression)
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageListAsync(pageIndex, pageSize, totalCount);
            return new PageModel<TResult>(pageIndex, totalCount, pageSize, list);
        }

        /// <summary>
        /// 两表联合查询-分页 - 分组
        /// </summary>
        /// <typeparam name="T">实体1</typeparam>
        /// <typeparam name="T2">实体1</typeparam>
        /// <typeparam name="TResult">返回对象</typeparam>
        /// <param name="joinExpression">关联表达式</param>
        /// <param name="selectExpression">返回表达式</param>
        /// <param name="whereExpression">查询表达式</param>
        /// <param name="groupExpression">group表达式</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="orderByFields">排序字段</param>
        /// <returns></returns>
        public async Task<PageModel<TResult>> QueryTabsPage<T, T2, TResult>(
            Expression<Func<T, T2, object[]>> joinExpression,
            Expression<Func<T, T2, TResult>> selectExpression,
            Expression<Func<TResult, bool>> whereExpression,
            Expression<Func<T, object>> groupExpression,
            int pageIndex = 1,
            int pageSize = 10,
            string orderByFields = null)
        {
            RefAsync<int> totalCount = 0;
            var list = await Db.Queryable(joinExpression).GroupBy(groupExpression)
                .Select(selectExpression)
                .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                .WhereIF(whereExpression != null, whereExpression)
                .ToPageListAsync(pageIndex, pageSize, totalCount);
            return new PageModel<TResult>(pageIndex, totalCount, pageSize, list);
        }
    }
}
