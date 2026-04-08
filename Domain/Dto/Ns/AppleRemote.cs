namespace MyDotnet.Domain.Dto.Ns
{
    public static class AppleRemote
    {
        public static readonly string KEY = "appleRemote";
        public static readonly string apKeyID = "apKeyID";//ConfigHelper.GetValue(new string[] { "appleRemote", "apKeyID" }).ObjToString();
        public static readonly string apKey = "apKey";//ConfigHelper.GetValue(new string[] { "appleRemote", "apKey" }).ObjToString();
        public static readonly string apTeamID = "apTeamID";//ConfigHelper.GetValue(new string[] { "appleRemote", "apTeamID" }).ObjToString();
        public static readonly string apEnv = "apEnv";//ConfigHelper.GetValue(new string[] { "appleRemote", "env" }).ObjToString();
    }
}
