using AppStoreConnect.Model;
using System;

namespace MyDotnet.Domain.Dto.Ns
{
    public class OrderNotifyMsg
    {
        public string sign { get; set; }
        public OrderNotifyMsgData content { get; set; }
    }
    public class OrderNotifyMsgData
    {


        /// <summary>
        /// 平台返回的订单号
        /// </summary>
        public string mch_trade_no { get; set; }
        /// <summary>
        /// 商户订单号（字母、数字）
        /// </summary>
        public string out_order_no { get; set; }
        /// <summary>
        /// 微信/支付宝订单显示的流水号 例如: 123456
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 价格单位：分
        /// </summary>
        public int total_fee { get; set; }
        /// <summary>
        /// 附加信息，回调时原样传回 例如：{test:'test1_params'}
        /// </summary>
        public string attach { get; set; }
        /// <summary>
        /// 支付类型：1-刷卡支付；2-扫码支付；3-公众号支付；4-App 支付；6-手机网站支付；9-小程序支付
        /// </summary>
        public int trade_type { get; set; }
        /// <summary>
        /// 支付渠道：1-微信支付；2-支付宝
        /// </summary>
        public int pay_type { get; set; }
        /// <summary>
        /// 付款银行卡类型
        /// </summary>
        public string bank_type { get; set; }
        /// <summary>
        /// 用户支付时间，例如：2021-05-11 11:31:08
        /// </summary>
        public string pay_time { get; set; }
        /// <summary>
        /// 订单状态：1-待支付；2-成功；3-关闭
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonce_str { get; set; }
    }
}
