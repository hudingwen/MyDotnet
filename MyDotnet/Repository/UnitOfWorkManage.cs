

using SqlSugar;

namespace MyDotnet.Repository
{

    /// <summary>
    /// 事务管理
    /// </summary>
    public class UnitOfWorkManage
    {
        /// <summary>
        /// single全局实例
        /// </summary>
        private ISqlSugarClient _db;
        /// <summary>
        /// scope用户实例
        /// </summary>
        public SqlSugarScope db;
        private bool isBegin = false;
        public UnitOfWorkManage(ISqlSugarClient sqlSugarClient)
        {
            _db = sqlSugarClient;
            db = sqlSugarClient as SqlSugarScope;
        }
        /// <summary>
        /// 开启事务
        /// </summary>
        public void BeginTran()
        {
            lock (this)
            {
                if (!isBegin)
                {
                    db.BeginTran();
                    isBegin = true;
                }
            }
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTran()
        {
            lock (this)
            {
                if (isBegin)
                {
                    db.CommitTran();
                }
                isBegin = false;
            }
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTran()
        {
            lock (this)
            {
                if (isBegin)
                {
                    db.RollbackTran();
                }
                isBegin = false;
            }
        }

    }
}
