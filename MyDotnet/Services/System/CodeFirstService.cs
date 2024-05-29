using Microsoft.IdentityModel.Tokens;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Repository.System;
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
            db.CodeFirst.InitTables(typeof(UserGoogleAuthenticator)); 

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
                // 注意 ：Oracle和个别国产库不支持该方法，需要手动建库 
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
            db.CodeFirst.InitTables<UserGoogleAuthenticator>();

            

            db.CodeFirst.InitTables<WeChatCompany>();
            db.CodeFirst.InitTables<WeChatConfig>();
            db.CodeFirst.InitTables<WeChatKeyword>();
            db.CodeFirst.InitTables<WeChatPushLog>();
            db.CodeFirst.InitTables<WeChatQR>();
            db.CodeFirst.InitTables<WeChatSub>();


            db.CodeFirst.InitTables<Nightscout>();
            db.CodeFirst.InitTables<NightscoutBanner>();
            db.CodeFirst.InitTables<NightscoutCustomer>();
            db.CodeFirst.InitTables<NightscoutLog>();
            db.CodeFirst.InitTables<NightscoutServer>();
            db.CodeFirst.InitTables<TCode>();


            db.CodeFirst.InitTables<TrojanCusServers>();
            db.CodeFirst.InitTables<TrojanCusServersUsers>();
            db.CodeFirst.InitTables<TrojanDetails>();
            db.CodeFirst.InitTables<TrojanServers>();
            db.CodeFirst.InitTables<TrojanServersUsers>();
            db.CodeFirst.InitTables<TrojanServersUsersExclude>();
            db.CodeFirst.InitTables<TrojanUrlServers>();
            db.CodeFirst.InitTables<TrojanUrlServersUsers>();
            db.CodeFirst.InitTables<TrojanUsers>();


            //初始化数据
            var dicPath = Path.Combine(Directory.GetCurrentDirectory(), "ComFile", "SeedData"); 
            db.Insertable<DicData>(JsonHelper.JsonToObj<List<DicData>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(DicData).Name}.json")))).ExecuteCommand();
            db.Insertable<DicType>(JsonHelper.JsonToObj<List<DicType>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(DicType).Name}.json")))).ExecuteCommand();
            db.Insertable<Modules>(JsonHelper.JsonToObj<List<Modules>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Modules).Name}.json")))).ExecuteCommand();
            db.Insertable<Permission>(JsonHelper.JsonToObj<List<Permission>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Permission).Name}.json")))).ExecuteCommand();
            db.Insertable<Role>(JsonHelper.JsonToObj<List<Role>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Role).Name}.json")))).ExecuteCommand();
            db.Insertable<RoleModulePermission>(JsonHelper.JsonToObj<List<RoleModulePermission>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(RoleModulePermission).Name}.json")))).ExecuteCommand();
            db.Insertable<UserRole>(JsonHelper.JsonToObj<List<UserRole>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(UserRole).Name}.json")))).ExecuteCommand();
            db.Insertable<SysUserInfo>(JsonHelper.JsonToObj<List<SysUserInfo>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(SysUserInfo).Name}.json")))).ExecuteCommand(); 

            await Task.CompletedTask;
        }
        /// <summary>
        /// 保存种子数据
        /// </summary>
        /// <returns></returns>
        public async Task SaveSeedData()
        {
            var db = _unitOfWorkManage.db.GetConnectionScope(DbConfigHelper.MainDB);
             
            var dicPath = Path.Combine(Directory.GetCurrentDirectory(), "ComFile", "SeedData");
            if (!Directory.Exists(dicPath))
                Directory.CreateDirectory(dicPath);
            //保存种子数据
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(DicData).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<DicData>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(DicType).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<DicType>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(Modules).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<Modules>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(Permission).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<Permission>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(Role).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<Role>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(RoleModulePermission).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<RoleModulePermission>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(UserRole).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<UserRole>().ToListAsync()));
            FileHelper.WriteFile(Path.Combine(dicPath, $"{typeof(SysUserInfo).Name}.json"), JsonHelper.ObjToJson(await db.Queryable<SysUserInfo>().ToListAsync()));

        }



    }
}
