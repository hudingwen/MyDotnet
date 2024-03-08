using MyDotnet.Helper;

namespace MyDotnet.Domain.Dto.Trojan
{
    public static class TrojanInfo
    {
        public static string KEY = "trojan";
        public static string KEY_normalApi = "normalApi";
        public static string KEY_clashApi = "clashApi";
        public static string KEY_clashApiBackup = "clashApiBackup";

        //public static string normalApi = ConfigHelper.GetValue(new string[] { "trojan", "normalApi" });
        //public static string clashApi = ConfigHelper.GetValue(new string[] { "trojan", "clashApi" });
        //public static string clashApiBackup = ConfigHelper.GetValue(new string[] { "trojan", "clashApiBackup" });
    }
}
