using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Hospital
{
    /// <summary>
    /// 学习参数
    /// </summary>
    public class StudyParameter : RootEntityTkey<long>
    {

        /// <summary>
        /// 父级目录id
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public long categoryId { get; set; }
        /// <summary>
        /// 参数类型(10-文字 20-图片 30-音频 40-视频 50-pdf 60-文件)
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int parameterType { get; set; }
        /// <summary>
        /// 参数排序
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public int parameterSort { get; set; }
        /// <summary>
        /// 参数内容
        /// </summary>
        [SugarColumn(Length = 2000,IsNullable = true)]
        public string parameterContent { get; set; }

    }
}
