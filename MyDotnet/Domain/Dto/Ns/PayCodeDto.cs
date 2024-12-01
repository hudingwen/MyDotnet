namespace MyDotnet.Domain.Dto.Ns
{
    public class PayCodeDto
    {
        /// <summary>
        /// 应用密钥的 AK，即是 AccessKey
        /// </summary>
        public string access_key { get; set; }
        /// <summary>
        /// 商品名称 例如：QQ 会员充值
        /// </summary>
        public string body { get; set; }
        /// <summary>
        /// 商户订单号（字母、数字，至少 18 位长度）
        /// </summary>
        public string out_order_no { get; set; }
        /// <summary>
        /// 支付类型 1-微信；2-支付宝；3-云闪付
        /// </summary>
        public int pay_type { get; set; }
        /// <summary>
        /// 价格单位：分
        /// </summary>
        public int total_fee { get; set; }
        /// <summary>
        /// 你平台的接收回调通知的地址
        /// </summary>
        public string notify_url { get; set; }
        /// <summary>
        /// 订单过期分钟，默认 30，最大值 60；例如：10，即是 10 分钟
        /// </summary>
        //public int expire { get; set; }
        /// <summary>
        /// 附加信息，回调时原样传回 例如：{test:'test1_params'}
        /// </summary>
        //public string attach { get; set; }
        /// <summary>
        /// 分账接收方，jsonArray 格式 例如：'[{"amount":1,"sub_mch_no":"xxxxxxxx"}]'
        /// </summary>
        //public string ledger { get; set; }
        /// <summary>
        /// 签名结果（大写），详见 签名算法
        /// </summary>
        public string sign { get; set; }
    }
}
