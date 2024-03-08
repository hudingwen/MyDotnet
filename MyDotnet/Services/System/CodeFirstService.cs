using Microsoft.IdentityModel.Tokens;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using SqlSugar;
using System.Text;

namespace MyDotnet.Services.System
{
    public class CodeFirstService : BaseServices<BaseEntity>
    {
        UnitOfWorkManage _unitOfWorkManage;
        public CodeFirstService(BaseRepository<BaseEntity> baseRepository
            , UnitOfWorkManage unitOfWorkManage) : base(baseRepository)
        {
            _unitOfWorkManage = unitOfWorkManage;
        } 
        public async Task CreateTables(DbFirstDTO dbFirstDto)
        {
            var db = _unitOfWorkManage.db.GetConnectionScope(dbFirstDto.connId);

            var dbInfo = DbConfigHelper.listdatabase.Find(t => t.ConnId == dbFirstDto.connId);

            if (!(dbInfo.DbType == DataBaseType.Oracle || dbInfo.DbType == DataBaseType.Dm))
            {
                // 建库：如果不存在创建数据库存在不会重复创建 createdb
                // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库 
                db.DbMaintenance.CreateDatabase();
            }

            //创建表：根据实体类CodeFirstTable1  (所有数据库都支持)
            db.CodeFirst.InitTables(typeof(TrojanUrlServersUsers));
            db.CodeFirst.InitTables(typeof(TrojanCusServersUsers));

            await Task.CompletedTask;
        }
        /// <summary>
        /// 初始化数据库(初始化数据库第一次用)
        /// </summary>
        /// <returns></returns>
        public async Task InitDatabase()
        {
            var db = _unitOfWorkManage.db.GetConnectionScope(DbConfigHelper.MainDB);

            var dbInfo = DbConfigHelper.listdatabase.Find(t => t.ConnId == DbConfigHelper.MainDB);
            
            if(!(dbInfo.DbType == DataBaseType.Oracle || dbInfo.DbType == DataBaseType.Dm))
            {
                // 建库：如果不存在创建数据库存在不会重复创建 createdb
                // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库 
                db.DbMaintenance.CreateDatabase(); 
            }

            //创建表：根据实体类CodeFirstTable1  (所有数据库都支持)    
            db.CodeFirst.InitTables<Department>();
            db.CodeFirst.InitTables<DicType>();
            db.CodeFirst.InitTables<DicData>();
            db.CodeFirst.InitTables<Modules>();
            db.CodeFirst.InitTables<Permission>();
            db.CodeFirst.InitTables<Role>();
            db.CodeFirst.InitTables<RoleModulePermission>();
            db.CodeFirst.InitTables<SysUserInfo>();
            db.CodeFirst.InitTables<TasksLog>();
            db.CodeFirst.InitTables<TasksQz>();
            db.CodeFirst.InitTables<UserRole>();
            db.CodeFirst.InitTables<WeChatCompany>();
            db.CodeFirst.InitTables<WeChatConfig>();
            db.CodeFirst.InitTables<WeChatKeyword>();
            db.CodeFirst.InitTables<WeChatPushLog>();
            db.CodeFirst.InitTables<WeChatQR>();
            db.CodeFirst.InitTables<WeChatSub>();

            await Task.CompletedTask;
        }

    }
}
