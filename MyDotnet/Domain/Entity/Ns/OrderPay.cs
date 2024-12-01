using AppStoreConnect.Model;
using MyDotnet.Domain.Entity.Base;
using SqlSugar;

namespace MyDotnet.Domain.Entity.Ns
{
    /// <summary>
    /// 订单支付表
    /// </summary>
    public class OrderPay  
    {
        /// <summary>
        /// 订单id
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = false, IsPrimaryKey = true)]
        public string id { get; set; }
        /// <summary>
        /// 用户网址
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
        public string url { get; set; }
        /// <summary>
        /// ns主键
        /// </summary>
        public long nsid { get; set; }
        /// <summary>
        /// 费用/单位/分
        /// </summary>
        public int cost { get; set; }
        /// <summary>
        /// 订单描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string orderDescription { get; set; }
        /// <summary>
        /// 续费多少年
        /// </summary>
        public int years { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary> 
        public DateTime createTime { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? payTime { get; set; }
        /// <summary>
        /// 支付状态：1-待支付；2-成功；3-失败；
        /// </summary>
        public int payStatus { get; set; }
        /// <summary>
        /// 支付方式： 1-刷卡支付；2-扫码支付；3-公众号支付；4-App 支付； 6-手机网站支付；9-小程序支付
        /// </summary>
        public int tradeType { get; set; }
        /// <summary>
        /// 支付渠道：1-微信支付；2-支付宝
        /// </summary>
        public int payType { get; set; }
        /// <summary>
        /// 平台返回的订单号
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string tradeNo { get; set; }
        /// <summary>
        /// 支付链接
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string payUrl { get; set; }
        /// <summary>
        /// 微信/支付宝订单显示的流水号 例如: 123456
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string transactionId { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true)]
        public string attach { get; set; }
        /// <summary>
        /// 付款银行卡类型
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string bankType { get; set; }
        /// <summary>
        /// 随机字符串
        /// </summary>
        [SugarColumn(Length = 64, IsNullable = true)]
        public string nonceStr { get; set; }



    }
}
