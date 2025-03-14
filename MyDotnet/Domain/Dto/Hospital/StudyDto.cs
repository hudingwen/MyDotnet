using MyDotnet.Domain.Entity.Hospital;

namespace MyDotnet.Domain.Dto.Hospital
{
    public class StudyDto
    {
        /// <summary>
        /// 目录描述
        /// </summary>
        public StudyCategory categoryInfo { get; set; }
        /// <summary>
        /// 子目录列表
        /// </summary>
        public List<StudyCategory> categoryChildList { get; set; }
        /// <summary>
        /// 目录详情
        /// </summary>
        public List<StudyParameter> parameterList { get; set; }
    }
}
