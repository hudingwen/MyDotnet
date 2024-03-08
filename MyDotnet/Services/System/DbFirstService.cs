using Microsoft.IdentityModel.Tokens;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Domain.Entity.System;
using MyDotnet.Helper;
using MyDotnet.Repository;
using SqlSugar;
using System.Text;

namespace MyDotnet.Services.System
{
    public class DbFirstService : BaseServices<BaseEntity>
    {
        UnitOfWorkManage _unitOfWorkManage;
        public DbFirstService(BaseRepository<BaseEntity> baseRepository
            , UnitOfWorkManage unitOfWorkManage) : base(baseRepository)
        {
            _unitOfWorkManage = unitOfWorkManage;
        } 
        public async Task CreateModels(DbFirstDTO dbFirstDto)
        {
            var strPath = dbFirstDto.strPath;
            var strNameSpace = dbFirstDto.strNameSpace;
            //多库文件分离
            if (DbConfigHelper.MutiDBEnabled && !DbConfigHelper.MainDB.Equals(dbFirstDto.connId))
            {
                strPath = strPath + @"\Models\" + dbFirstDto.connId;
                strNameSpace = strNameSpace + "." + dbFirstDto.connId;
            }

            IDbFirst IDbFirst = null;
            try
            {
                //IDbFirst = _unitOfWorkManage.db.DbFirst;
                _unitOfWorkManage.db.GetConnectionScope(dbFirstDto.connId).Open();
                IDbFirst = _unitOfWorkManage.db.GetConnectionScope(dbFirstDto.connId).DbFirst;

            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error(ex);
                throw;
            }
            if (dbFirstDto.lsTableNames != null && dbFirstDto.lsTableNames.Length > 0)
            {
                IDbFirst = IDbFirst.Where(dbFirstDto.lsTableNames);
            }
            var ls = IDbFirst.IsCreateDefaultValue().IsCreateAttribute()

                  .SettingClassTemplate(p => p =
@"{using}

namespace " + strNameSpace + @"
{
{ClassDescription}
    [SugarTable( ""{ClassName}"", """ + dbFirstDto.connId + @""")]" + (dbFirstDto.isSerializable ? "\n    [Serializable]" : "") + @"
    public class {ClassName}" + (string.IsNullOrEmpty(dbFirstDto.strInterface) ? "" : (" : " + dbFirstDto.strInterface)) + @"
    {
           public {ClassName}()
           {
           }
{PropertyName}
    }
}")
                  //.SettingPropertyDescriptionTemplate(p => p = string.Empty)
                  .SettingPropertyTemplate(p => p =
@"{SugarColumn}
           public {PropertyType} {PropertyName} { get; set; }")

                   //.SettingConstructorTemplate(p => p = "              this._{PropertyName} ={DefaultValue};")

                   .ToClassStringList(strNameSpace); 
            foreach (var item in ls)
            {
                var fileName = $"{string.Format("{0}", item.Key)}.cs";
                var fileFullPath = Path.Combine(strPath, fileName);
                if (!Directory.Exists(strPath))
                {
                    Directory.CreateDirectory(strPath);
                }
                await File.WriteAllTextAsync(fileFullPath, item.Value, Encoding.UTF8);
            }
        }

    }
}
