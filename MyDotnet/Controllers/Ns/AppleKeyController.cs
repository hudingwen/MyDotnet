using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Core.Servers;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace MyDotnet.Controllers.Ns
{

    /// <summary>
    /// 苹果验证
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class AppleKeyController : BaseApiController
    {
        private BaseServices<TCode> baseServices;
        public AppleKeyController(BaseServices<TCode> _baseServices)
        {
            baseServices = _baseServices;
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<TCode>>> Get(int page = 1, string key = "", int size = 10,int? status = null)
        {
            Expression<Func<TCode, bool>> whereExpression = a => true;

            key = key.ObjToString().Trim();
            if(!string.IsNullOrEmpty(key) )
            {
                whereExpression = whereExpression.And(t => t.auth_code.Contains(key) || t.record_id.Contains(key) || t.record_key.Contains(key));
            }
            if(status != null )
            {
                whereExpression = whereExpression.And(t=>t.status ==  status);
            }
            var data = await baseServices.Dal.QueryPage(whereExpression, page, size, "create_date desc");
            return MessageModel<PageModel<TCode>>.Success("获取成功", data);
        }
        [HttpPost]
        public async Task<MessageModel<List<TCode>>> Post([FromBody] TCode data)
        {
            if (data.createCount > 0 && data.createCount <=100)
            {
                List<TCode> ls = new List<TCode>();
                int i = 1; 
                while (i<= data.createCount)
                {
                    TCode code = new TCode();
                    if(data.createType == 0)
                    {
                        code.auth_code = "Q" + StringHelper.GetGUID().ToUpper().Substring(0, 17);
                    }else if (data.createType == 1)
                    {
                        code.auth_code = StringHelper.GenerateRandomChinese(12);
                    }
                    else
                    {
                        return Failed(new List<TCode>(),"激活码类型错误");
                    }
                    code.user_id = "d480cea86cda0272bc7e8e20f5e3eea6"; 
                    code.id = StringHelper.GetGUID().ToLower();
                    code.record_id = code.id;
                    code.status = 0;
                    code.is_delete = 0;
                    code.create_date = DateTime.Now;

                    code.expiry_date = null;
                    code.user_time = null;
                    code.reg_time = null;
                    code.expire_time = null;

                    code.comment = "注册机";
                    code.code_type = "year";
                    ls.Add(code);
                    i++;
                }
                await baseServices.Dal.Db.Insertable(ls).ExecuteCommandAsync();
                return MessageModel<List<TCode>>.Success("添加成功",ls);
            }
            else
            {
                return MessageModel<List<TCode>>.Fail("请输入正确的数量");
            }
        }
        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] TCode data)
        {
            var isOk = await baseServices.Dal.Update(data);
            if (isOk)
            {
                return MessageModel<string>.Success("更新成功");
            }
            else
            {
                return MessageModel<string>.Success("更新失败");
            }
        }

        [HttpDelete]
        public async Task<MessageModel<string>> Delete(string id)
        {
            var isOk = await baseServices.Dal.DeleteById(id);
            if (isOk)
            {
                return MessageModel<string>.Success("删除成功");
            }
            else
            {
                return MessageModel<string>.Success("删除失败");
            }
        }

        [HttpPost]
        public async Task<MessageModel<string>> Deletes(string[] ids)
        {
            var isOk = await baseServices.Dal.DeleteByIds(ids);
            if (isOk)
            {
                return MessageModel<string>.Success("删除成功");
            }
            else
            {
                return MessageModel<string>.Success("删除失败");
            }
        }

        [HttpGet]
        public async Task<MessageModel<string>> GetCalcKey(string[] ids)
        {
            var isOk = await baseServices.Dal.DeleteByIds(ids);
            if (isOk)
            {
                return MessageModel<string>.Success("删除成功");
            }
            else
            {
                return MessageModel<string>.Success("删除失败");
            }
        }



        [HttpGet] 
        public async Task<MessageModel<TCode>> CreateKey(string id, string key, bool needTime, long times,int day, int hour, int min, int sec)
        {
            int intId = int.Parse(id);
            int intKey = int.Parse(key);
            string hexString = ((intId / 2) + intKey).ToString("X").ToLower();

            if (needTime)
            {
                StringBuilder hexStringBuilder = new StringBuilder();
                hexStringBuilder.Append(hexString);
                hexStringBuilder.Append("z");
                hexStringBuilder.Append(times);
                hexString = hexStringBuilder.ToString();
            }
            var auth_code = Encrypt(hexString);

            TCode code = new TCode();
              
            code.id = StringHelper.GetGUID().ToLower();
            code.record_id = id;
            code.record_key = key;
            code.auth_code = auth_code;
            code.create_date = DateTime.Now;
            code.expiry_date = DateTime.Now.AddMilliseconds(times);
            code.create_day = day;
            code.create_hour = hour;
            code.create_min = min;
            code.create_sec = sec;

            await baseServices.Dal.Db.Insertable(code).ExecuteCommandAsync();


            return MessageModel<TCode>.Success("添加成功", code);

        }

        private static string Byte2Hex(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString().ToUpper();
        }

        private static string Encrypt(string plainText)
        {
            string key = "Format2044153997";
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Byte2Hex(encryptedBytes).ToLower();
                }
            }
        }

    }
}
