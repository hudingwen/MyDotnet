using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver.Core.Servers;
using MyDotnet.Controllers.Base;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services;
using MyDotnet.Services.Ns;
using System.Linq.Expressions;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MyDotnet.Controllers.Ns
{

    /// <summary>
    /// 支付管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class PayController : BaseApiController
    {
        public BaseServices<OrderPay> _orderService;
        public NightscoutServices _nightscoutServices;
        public UnitOfWorkManage _unitOfWorkManage;
        public PayController(BaseServices<OrderPay> orderService
            , NightscoutServices nightscoutServices
            , UnitOfWorkManage unitOfWorkManage)
        {
            _orderService = orderService;
            _nightscoutServices = nightscoutServices;
            _unitOfWorkManage = unitOfWorkManage;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<MessageModel<string>> OrderNotify()
        {
            OrderNotifyMsg data = null;
            Request.EnableBuffering(); // 允许多次读取 Body
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                data = JsonHelper.JsonToObj<OrderNotifyMsg>(body);
            }
            var contentData = JsonHelper.JsonToObj<OrderNotifyMsgData>(data.content);

            var access_key = ConfigHelper.GetValue(new string[] { "Pay", "access_key" });
            var secret_key = ConfigHelper.GetValue(new string[] { "Pay", "secret_key" });

            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret_key)))
            {
                byte[] hmacSha256Bytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data.content));


                var mySign = MD5Helper.MD5Encrypt32(hmacSha256Bytes);
                if (mySign.Equals(data.sign))
                {
                    if(contentData.status == 2)
                    {
                        //支付成功
                        //成功
                        var order = await _orderService.Dal.QueryById(contentData.out_order_no);
                        if (order == null)
                            return MessageModel<string>.Fail($"订单未找到:{contentData.out_order_no}");
                        if (order.payStatus == 2)
                            return MessageModel<string>.Fail($"订单已支付:{contentData.out_order_no}");
                        if (order.payStatus == 3)
                            return MessageModel<string>.Fail($"订单失败:{contentData.out_order_no}");

                        //续费
                        _unitOfWorkManage.BeginTran();
                        //var nightscout = await _nightscoutServices.Dal.QueryById(order.nsid);
                        //nightscout.endTime = nightscout.endTime.AddYears(order.years);
                        //await _nightscoutServices.Dal.Db.Updateable<Nightscout>(nightscout).UpdateColumns(t => new { t.endTime }).ExecuteCommandAsync();

                        order.payStatus = contentData.status;
                        order.payTime = contentData.pay_time;
                        order.payType = contentData.pay_type;
                        order.tradeType = contentData.trade_type;
                        order.transactionId = contentData.transaction_id;
                        await _orderService.Dal.Db.Updateable(order).UpdateColumns(t => new {t.payStatus, t.payTime, t.payType, t.tradeType, t.transactionId }).ExecuteCommandAsync();
                        _unitOfWorkManage.CommitTran();
                        return MessageModel<string>.Success("处理成功");
                    }
                    else
                    {
                        return MessageModel<string>.Success("其他状态暂时不做处理");
                    }
                }
                else
                {
                    return MessageModel<string>.Fail("验签失败");

                }
            }
        }
            /// <summary>
            /// 创建订单
            /// </summary>
            /// <param name="host"></param>
            /// <param name="payType">1-微信；2-支付宝；3-云闪付</param> 
            /// <param name="years">续费多少年</param> 
            /// <returns></returns>
            [AllowAnonymous]
        [HttpGet] 
        public async Task<MessageModel<PayCodeMsg>> CreateOrder(string host,int payType=1,int years=1)
        {
            //参数
            if (years <= 0) years = 1;
            // 获取当前请求的主机名（包含端口）
            if (string.IsNullOrEmpty(host))
                host = HttpContext.Request.Host.Value;

            var nightscout = await _nightscoutServices.Dal.Db.Queryable<Nightscout>().Where(t => t.url == host).Select(t => new {t.Id,t.money}).FirstAsync();
            if (nightscout == null)
            {
                return MessageModel<PayCodeMsg>.Fail($"未找到用户:{host}");
            }
            var access_key = ConfigHelper.GetValue(new string[] { "Pay", "access_key" });
            var secret_key = ConfigHelper.GetValue(new string[] { "Pay", "secret_key" });
            var notify_url = ConfigHelper.GetValue(new string[] { "Pay", "notify_url" });

            _unitOfWorkManage.BeginTran();
            try
            {
                OrderPay orderPay = new OrderPay();
                orderPay.id = StringHelper.GetGUID();
                orderPay.createTime = DateTime.Now;
                orderPay.payType = payType;
                orderPay.url = host;
                orderPay.nsid = nightscout.Id;
                orderPay.years = years;
                orderPay.cost = Convert.ToInt32(nightscout.money * years * 100);
                orderPay.orderDescription = $"用户续费({host})-{years}年";
                await _orderService.Dal.Db.Insertable<OrderPay>(orderPay).ExecuteCommandAsync();

                PayCodeDto payCodeDto = new PayCodeDto();
                payCodeDto.access_key = access_key;
                payCodeDto.body = orderPay.orderDescription;
                payCodeDto.notify_url = notify_url;
                payCodeDto.pay_type = payType;
                payCodeDto.total_fee = orderPay.cost;
                payCodeDto.out_order_no = orderPay.id;

                var sign = $"access_key={access_key}&body={payCodeDto.body}&notify_url={payCodeDto.notify_url}&out_order_no={payCodeDto.out_order_no}&pay_type={payCodeDto.pay_type}&total_fee={payCodeDto.total_fee}&secret_key={secret_key}";
                sign = MD5Helper.MD5Encrypt32(sign);
                payCodeDto.sign = sign;
                var json = JsonHelper.ObjToJson(payCodeDto);
                var dataJson = await HttpHelper.PostAsync("https://open.weidoufu.com/pay/native", json);
                var msg = JsonHelper.JsonToObj<PayCodeMsg>(dataJson);
                if (msg.code == 1)
                {
                    orderPay.tradeNo = msg.data.mch_trade_no;
                    orderPay.payUrl = msg.data.code_url;
                    await _orderService.Dal.Db.Updateable<OrderPay>(orderPay).UpdateColumns(t=>new { t.tradeNo,t.payUrl}).ExecuteCommandAsync();
                    
                    _unitOfWorkManage.CommitTran();
                    return MessageModel<PayCodeMsg>.Success("创建成功", msg);
                }
                else
                {
                    _unitOfWorkManage.RollbackTran();
                    return MessageModel<PayCodeMsg>.Fail("创建失败", msg);
                }
                
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                throw;
            }
            
        }
        

    }
}
