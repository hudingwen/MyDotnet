namespace MyDotnet.Domain.Dto.Ns
{
    /// <summary>
    /// 订单返回消息
    /// </summary>
    public class PayCodeMsg
    {
        /// <summary>
        /// 1：成功   0：失败
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回描述信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 返回状态，有 success 和 fail
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// 返回订单数据
        /// </summary>
        public PayCodeMsgData data { get; set; }

    }

    public class PayCodeMsgData
    {
        /// <summary>
        /// base64 编码的二维码图片
        /// </summary>
        public string qrcode { get; set; }
        /// <summary>
        /// 付款链接，可以唤起 APP 付款
        /// </summary>
        public string code_url { get; set; }
        /// <summary>
        /// 平台返回的订单号
        /// </summary>
        public string mch_trade_no { get; set; }
        /// <summary>
        /// 商户订单号（字母、数字）
        /// </summary>
        public string out_order_no { get; set; }

    }
}
