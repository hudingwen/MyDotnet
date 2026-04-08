using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Core.Servers;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using SixLabors.ImageSharp.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Numerics;
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




        /// <summary>
        /// 注册机1
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="needTime"></param>
        /// <param name="times"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <returns></returns>
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
                if (times > 0)
                {
                    hexStringBuilder.Append(times);
                }
                else
                {
                     times = ((long)day) * 60 * 60 * 24 * 1000 + ((long)hour) * 60 * 60 * 1000 + ((long)min) * 60 * 1000 + ((long)sec) * 1000;
                     hexStringBuilder.Append(times);
                }
               

                hexString = hexStringBuilder.ToString();
            }
            var auth_code = Encrypt(hexString);

            TCode code = new TCode();

            code.pass_type = "auth001";
            code.id = StringHelper.GetGUID().ToLower();
            code.record_id = id;
            code.record_key = key;
            code.auth_code = auth_code;
            code.create_date = DateTime.Now;
            if (needTime)
            {
                code.expiry_date = DateTime.Now.AddMilliseconds(times);
            }
            else
            {
                code.expiry_date = DateTime.MaxValue;
            }
            
            code.create_day = day;
            code.create_hour = hour;
            code.create_min = min;
            code.create_sec = sec;

            await baseServices.Dal.Db.Insertable(code).ExecuteCommandAsync();

            var codetemp  = new TCode();
            codetemp.auth_code = code.auth_code;
            codetemp.expiry_date = code.expiry_date;

            return MessageModel<TCode>.Success("添加成功", codetemp);

        }


        /// <summary>
        /// 注册机2
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]

        public async Task<MessageModel<TCode>> CreateKey2(string id)
        {
             

            TCode code = new TCode();

            code.pass_type = "auth002";
            string auth_code;
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(id);
                byte[] var6 = md5.ComputeHash(inputBytes);
                //BigInteger bigInt = new BigInteger(hashBytes.Reverse().ToArray()); // Reverse to handle endianness
                BigInteger var3;
                // 检查 var6 的首字节是否会影响符号（如果最高位为 1，它会被解释为负数）
                if ((var6[0] & 0x80) != 0)  // 检查最高位是否为1
                {
                    // 创建一个新的数组，长度比 var6 多 1，并在开头添加 0x00
                    byte[] var6WithZero = new byte[var6.Length + 1];
                    var6WithZero[0] = 0x00;  // 确保正数
                    Array.Copy(var6, 0, var6WithZero, 1, var6.Length);

                    // 使用修改后的数组创建 BigInteger
                    var3 = new BigInteger(var6WithZero, isBigEndian: true);
                }
                else
                {
                    // 如果不需要添加 0x00，直接创建 BigInteger
                    var3 = new BigInteger(var6, isBigEndian: true);

                }

                auth_code = var3.ToString();
                if (auth_code.Length > 6)
                {
                    auth_code = auth_code.Substring(0, 6);
                }
            }

            code.id = StringHelper.GetGUID().ToLower();
            code.record_id = id;
            code.record_key = "";
            code.auth_code = auth_code;
            code.create_date = DateTime.Now;
            code.expiry_date = DateTime.MaxValue;

            await baseServices.Dal.Db.Insertable(code).ExecuteCommandAsync();

            var codetemp = new TCode();
            codetemp.auth_code = code.auth_code;
            codetemp.expiry_date = code.expiry_date;

            return MessageModel<TCode>.Success("添加成功", codetemp);

             
        }


        /// <summary>
        /// 注册机3
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]

        public async Task<MessageModel<TCode>> CreateKey3(string id)
        {


            TCode code = new TCode();
            code.pass_type = "auth003";
            string auth_code;


            string md5Hex = Md5String(id + "xc122989779");
            var bigInt = BigInteger.Parse("0" + md5Hex, NumberStyles.HexNumber); // 加个"0"避免负数
            auth_code = bigInt.ToString().Substring(0, 8);
             



            code.id = StringHelper.GetGUID().ToLower();
            code.record_id = id;
            code.record_key = "";
            code.auth_code = auth_code;
            code.create_date = DateTime.Now;
            code.expiry_date = DateTime.MaxValue;

            await baseServices.Dal.Db.Insertable(code).ExecuteCommandAsync();

            var codetemp = new TCode();
            codetemp.auth_code = code.auth_code;
            codetemp.expiry_date = code.expiry_date;

            return MessageModel<TCode>.Success("添加成功", codetemp);


        }
        private static string Md5String(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";

            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(bytes);
                string hex = ToHexString(hash);
                return hex.Length == 32 ? hex.Substring(8, 16) : hex;
            }
        }
        private static string ToHexString(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append((b >> 4).ToString("x")); // 高4位
                sb.Append((b & 0x0F).ToString("x")); // 低4位
            }
            return sb.ToString();
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
