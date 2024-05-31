namespace MyDotnet.Domain.Attr
{
    /// <summary>
    /// 自定义job描述特性
    /// </summary>
    public class JobDescriptionAttribute : Attribute
    {
        public JobDescriptionAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; }
    }
}
