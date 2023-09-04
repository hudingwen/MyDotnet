using MyDotnet.Helper;

namespace MyDotnet.Domain.Dto.Trojan
{
    public static class TrojanInfo
    {
        public static string normalApi = ConfigHelper.GetValue(new string[] { "trojan", "normalApi" });
        public static string clashApi = ConfigHelper.GetValue(new string[] { "trojan", "clashApi" });
        public static string clashApiBackup = ConfigHelper.GetValue(new string[] { "trojan", "clashApiBackup" });
}
}
