namespace MyDotnet.Domain.Dto.System
{
    public class DbFirstDTO
    {
        /// <summary>
        /// 数据库id
        /// </summary>
        public string connId { get; set; }
        /// <summary>
        /// 生成的路径
        /// </summary>
        public string strPath { get; set; }
        /// <summary>
        /// 生成的命名空间
        /// </summary>
        public string strNameSpace { get; set; }
        /// <summary>
        /// 需要生成的表名列表
        /// </summary>
        public string[] lsTableNames { get; set; }
        /// <summary>
        /// 继承接口名称
        /// </summary>
        public string strInterface { get; set; }
        /// <summary>
        /// 是否序列化
        /// </summary>
        public bool isSerializable { get; set; } = false;

    }
}
