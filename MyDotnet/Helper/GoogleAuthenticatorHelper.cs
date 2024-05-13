using MyDotnet.Controllers.System;
using MyDotnet.Domain.Dto.System;
using System.Security.Cryptography;
using System.Text;

namespace MyDotnet.Helper
{
    /// <summary>
    /// 谷歌双因子验证帮助类
    /// </summary>
    public static class GoogleAuthenticatorHelper
    {
        private readonly static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly static TimeSpan DefaultClockDriftTolerance = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 生成双因子验证
        /// </summary>
        /// <param name="issuer">颁发者</param>
        /// <param name="user">用户账号</param>
        /// <param name="key">生成的key</param>
        /// <returns></returns>
        public static SetupCode GenerateSetupCode(string issuer, string user, string key)
        {
            byte[] keyArr = Encoding.UTF8.GetBytes(key);
            return GenerateSetupCode(issuer, user, keyArr);
        }
        /// <summary>
        /// 生成双因子验证
        /// </summary>
        /// <param name="issuer">颁发者</param>
        /// <param name="user">用户账号</param>
        /// <param name="keyArr">生成的key</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static SetupCode GenerateSetupCode(string issuer, string user, byte[] keyArr)
        {
            user = RemoveWhitespace(user);
            string encodedSecretKey = Base32Encoding.ToString(keyArr).Replace("=", "");
            string provisionUrl = String.Format("otpauth://totp/{2}:{0}?secret={1}&issuer={2}", user, encodedSecretKey, issuer);
            SetupCode setupCode = new SetupCode();
            setupCode.provisionUrl = provisionUrl;
            setupCode.encodedSecretKey = encodedSecretKey;
            return setupCode;
        }
        /// <summary>
        /// 清理空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string RemoveWhitespace(string str)
        {
            return new string(str.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }


        private static string GeneratePINAtInterval(string accountSecretKey, long counter, int digits = 6)
        {
            return GenerateHashedCode(accountSecretKey, counter, digits);
        }

        private static string GenerateHashedCode(string secret, long iterationNumber, int digits = 6)
        {
            byte[] key = Encoding.UTF8.GetBytes(secret);
            return GenerateHashedCode(key, iterationNumber, digits);
        }


        private static string GenerateHashedCode(byte[] key, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }

            HMACSHA1 hmac = new HMACSHA1(key);

            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[hash.Length - 1] & 0xf;

            // Convert the 4 bytes into an integer, ignoring the sign.
            int binary =
                ((hash[offset] & 0x7f) << 24)
                | (hash[offset + 1] << 16)
                | (hash[offset + 2] << 8)
                | (hash[offset + 3]);

            int password = binary % (int)Math.Pow(10, digits);
            return password.ToString(new string('0', digits));
        }

        private static long GetCurrentCounter()
        {
            return GetCurrentCounter(DateTime.UtcNow, _epoch, 30);
        }

        private static long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
        {
            return (long)(now - epoch).TotalSeconds / timeStep;
        }

        public static bool ValidateTwoFactorPIN(string accountSecretKey, string twoFactorCodeFromClient)
        {
            return ValidateTwoFactorPIN(accountSecretKey, twoFactorCodeFromClient, DefaultClockDriftTolerance);
        }

        public static bool ValidateTwoFactorPIN(string accountSecretKey, string twoFactorCodeFromClient, TimeSpan timeTolerance)
        {
            var codes = GetCurrentPINs(accountSecretKey, timeTolerance);
            return codes.Any(c => c == twoFactorCodeFromClient);
        }

        private static string[] GetCurrentPINs(string accountSecretKey, TimeSpan timeTolerance)
        {
            List<string> codes = new List<string>();
            long iterationCounter = GetCurrentCounter();
            int iterationOffset = 0;

            if (timeTolerance.TotalSeconds > 30)
            {
                iterationOffset = Convert.ToInt32(timeTolerance.TotalSeconds / 30.00);
            }

            long iterationStart = iterationCounter - iterationOffset;
            long iterationEnd = iterationCounter + iterationOffset;

            for (long counter = iterationStart; counter <= iterationEnd; counter++)
            {
                codes.Add(GeneratePINAtInterval(accountSecretKey, counter));
            }

            return codes.ToArray();
        }
    }
}
