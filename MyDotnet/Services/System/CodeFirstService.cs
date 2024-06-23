using Microsoft.IdentityModel.Tokens;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Domain.Entity.Nginx;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Domain.Entity.Trojan;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Repository.System;
using SqlSugar;
using System.Reflection.Metadata;
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
        /// <summary>
        /// 通过实体创建数据库表
        /// </summary>
        /// <param name="codeFirstDTO"></param>
        /// <returns></returns>
        public async Task CreateTables(CodeFirstDTO codeFirstDTO)
        {
            var db = _unitOfWorkManage.db.GetConnectionScope(codeFirstDTO.connId);

            var dbInfo = DbConfigHelper.listdatabase.Find(t => t.ConnId == codeFirstDTO.connId);

            if (!(dbInfo.DbType == DataBaseType.Oracle || dbInfo.DbType == DataBaseType.Dm))
            {
                // 建库：如果不存在创建数据库存在不会重复创建 createdb
                // 注意 ：Oracle和个别国产库需不支持该方法，需要手动建库 
                db.DbMaintenance.CreateDatabase();
            }

            //创建表：根据实体类CodeFirstTable1  (所有数据库都支持)
            Type type = typeof(NightscoutGuardAccount);
            var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
            var attrTable = string.Empty;
            if (tempAttr != null)
            {
                attrTable = ((SugarTable)tempAttr).TableName;
            }

            if (!string.IsNullOrEmpty(attrTable))
            {
                //查特性
                if (!db.DbMaintenance.IsAnyTable(attrTable))
                {
                    db.CodeFirst.InitTables<NightscoutGuardAccount>();
                }
            }
            else if (!db.DbMaintenance.IsAnyTable(type.Name))
            {
                //直接查反射名
                db.CodeFirst.InitTables<NightscoutGuardAccount>();
            }

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

            //部门表
            {
                Console.WriteLine($"创建-Department");
                Type type = typeof(Department);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable) )
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<Department>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<Department>();
                }
            }

            //字典表
            {
                Console.WriteLine($"创建-DicType");
                Type type = typeof(DicType);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<DicType>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<DicType>();
                }
            }

            //字典列表
            {
                Console.WriteLine($"创建-DicData");
                Type type = typeof(DicData);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<DicData>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<DicData>();
                }
            }

            //接口表
            {
                Console.WriteLine($"创建-Modules");
                Type type = typeof(Modules);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<Modules>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<Modules>();
                }
            }

            //菜单表
            {
                Console.WriteLine($"创建-Permission");
                Type type = typeof(Permission);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<Permission>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<Permission>();
                }
            }

            //角色表
            {
                Console.WriteLine($"创建-Role");
                Type type = typeof(Role);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<Role>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<Role>();
                }
            }

            //角色菜单关联表
            {
                Console.WriteLine($"创建-RoleModulePermission");
                Type type = typeof(RoleModulePermission);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<RoleModulePermission>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<RoleModulePermission>();
                }
            }

            //用户表
            {
                Console.WriteLine($"创建-SysUserInfo");
                Type type = typeof(SysUserInfo);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<SysUserInfo>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<SysUserInfo>();
                }
            }

            //调度日志表
            {
                Console.WriteLine($"创建-TasksLog");
                Type type = typeof(TasksLog);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<TasksLog>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<TasksLog>();
                }
            }

            //调度任务表
            {
                Console.WriteLine($"创建-TasksQz");
                Type type = typeof(TasksQz);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<TasksQz>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<TasksQz>();
                }
            }

            //用户角色表
            {
                Console.WriteLine($"创建-UserRole");
                Type type = typeof(UserRole);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<UserRole>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<UserRole>();
                }
            }

            //用户2FA关联表
            {
                Console.WriteLine($"创建-UserGoogleAuthenticator");
                Type type = typeof(UserGoogleAuthenticator);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<UserGoogleAuthenticator>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<UserGoogleAuthenticator>();
                }
            }

            //微信公众号配置表
            {
                Console.WriteLine($"创建-WeChatConfig");
                Type type = typeof(WeChatConfig);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatConfig>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatConfig>();
                }
            }

            //微信公司关联表
            {
                Console.WriteLine($"创建-WeChatCompany");
                Type type = typeof(WeChatCompany);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatCompany>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatCompany>();
                }
            }

            //微信关键词表
            {
                Console.WriteLine($"创建-WeChatKeyword");
                Type type = typeof(WeChatKeyword);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatKeyword>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatKeyword>();
                }
            }

            //微信推送表
            {
                Console.WriteLine($"创建-WeChatPushLog");
                Type type = typeof(WeChatPushLog);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatPushLog>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatPushLog>();
                }
            }

            //微信二维码绑定记录表
            {
                Console.WriteLine($"创建-WeChatQR");
                Type type = typeof(WeChatQR);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatQR>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatQR>();
                }
            }

            //微信订阅绑定表
            {
                Console.WriteLine($"创建-WeChatSub");
                Type type = typeof(WeChatSub);
                var tempAttr = Attribute.GetCustomAttribute(type, typeof(SugarTable));
                var attrTable = string.Empty;
                if (tempAttr != null)
                {
                    attrTable = ((SugarTable)tempAttr).TableName;
                }

                if (!string.IsNullOrEmpty(attrTable))
                {
                    //查特性
                    if (!db.DbMaintenance.IsAnyTable(attrTable))
                    {
                        db.CodeFirst.InitTables<WeChatSub>();
                    }
                }
                else if (!db.DbMaintenance.IsAnyTable(type.Name))
                {
                    //直接查反射名
                    db.CodeFirst.InitTables<WeChatSub>();
                }
            } 


            //db.CodeFirst.InitTables<Nightscout>();
            //db.CodeFirst.InitTables<NightscoutBanner>();
            //db.CodeFirst.InitTables<NightscoutCustomer>();
            //db.CodeFirst.InitTables<NightscoutLog>();
            //db.CodeFirst.InitTables<NightscoutServer>();
            //db.CodeFirst.InitTables<TCode>();


            //db.CodeFirst.InitTables<TrojanCusServers>();
            //db.CodeFirst.InitTables<TrojanCusServersUsers>();
            //db.CodeFirst.InitTables<TrojanDetails>();
            //db.CodeFirst.InitTables<TrojanServers>();
            //db.CodeFirst.InitTables<TrojanServersUsers>();
            //db.CodeFirst.InitTables<TrojanServersUsersExclude>();
            //db.CodeFirst.InitTables<TrojanUrlServers>();
            //db.CodeFirst.InitTables<TrojanUrlServersUsers>();
            //db.CodeFirst.InitTables<TrojanUsers>();


            //初始化数据
            var dicPath = Path.Combine(Directory.GetCurrentDirectory(), "ComFile", "SeedData"); 

            if (!await db.Queryable<DicData>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-WeChatSub");
                db.Insertable<DicData>(JsonHelper.JsonToObj<List<DicData>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(DicData).Name}.json")))).ExecuteCommand();
            }
                
            if (!await db.Queryable<DicType>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-DicType");
                db.Insertable<DicType>(JsonHelper.JsonToObj<List<DicType>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(DicType).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<Modules>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-Modules");
                db.Insertable<Modules>(JsonHelper.JsonToObj<List<Modules>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Modules).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<Permission>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-Permission");
                db.Insertable<Permission>(JsonHelper.JsonToObj<List<Permission>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Permission).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<Role>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-Role");
                db.Insertable<Role>(JsonHelper.JsonToObj<List<Role>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(Role).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<RoleModulePermission>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-RoleModulePermission");
                db.Insertable<RoleModulePermission>(JsonHelper.JsonToObj<List<RoleModulePermission>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(RoleModulePermission).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<UserRole>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-UserRole");
                db.Insertable<UserRole>(JsonHelper.JsonToObj<List<UserRole>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(UserRole).Name}.json")))).ExecuteCommand();
            }
            if (!await db.Queryable<SysUserInfo>().AnyAsync())
            {
                Console.WriteLine($"生成种子数据-SysUserInfo");
                db.Insertable<SysUserInfo>(JsonHelper.JsonToObj<List<SysUserInfo>>(FileHelper.ReadFile(Path.Combine(dicPath, $"{typeof(SysUserInfo).Name}.json")))).ExecuteCommand();
            }

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
