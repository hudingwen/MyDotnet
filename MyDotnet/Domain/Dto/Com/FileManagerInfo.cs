namespace MyDotnet.Domain.Dto.Com
{
    public class FileManagerInfo
    {
        public string fileName { get; set; }
        public string fileDisplay { get; set; }
        public bool isLeaf { get; set; }
        public bool disabled { get; set; }
        public string fileType { get; set; }
        public string fileExt { get; set; }
        public long fileSize { get; set; }
        public string fileParentDic { get; set; }
        public string filePath { get; set; }
        public DateTime fileCreateTime { get; set; }
        public DateTime fileEditTime { get; set; }


        public List<FileManagerInfo> fileList { get; set; } = new List<FileManagerInfo>();

        public List<BarDicInfo> barList = new List<BarDicInfo>();


    }
}
