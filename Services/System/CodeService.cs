using MyDotnet.Common.Cache;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Base;
using MyDotnet.Helper;
using MyDotnet.Repository;

namespace MyDotnet.Services.System
{
    /// <summary>
    /// 验证码服务类
    /// </summary>
    public class CodeService : BaseServices<BaseEntity>
    {

        public ICaching _caching;
        public CodeService(BaseRepository<BaseEntity> baseRepository, ICaching caching) : base(baseRepository)
        {
            _caching = caching;
        }
        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public CodeDto CreateCode()
        {
            CodeDto code = new CodeDto();
            var codeTxt = CodeHelper.GetRandomEnDigitalText(5);
            var codeArr = CodeHelper.GetBubbleCodeByte(codeTxt);
            var txt = Convert.ToBase64String(codeArr);
            code.key = StringHelper.GetGUID();
            code.code = txt;
            code.expireTime = DateTime.Now.AddMinutes(2);
            _caching.Set(code.key, codeTxt, code.expireTime - DateTime.Now);
            return code;
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool ValidCode(string key, string code)
        {
            var resourceCode = _caching.Get<string>(key);
            _caching.Remove(key);
            if (string.IsNullOrEmpty(resourceCode) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(code))

                return false;

            if (!resourceCode.Equals(code, StringComparison.CurrentCultureIgnoreCase))
                return false;


            return true;
        }


    }
}
