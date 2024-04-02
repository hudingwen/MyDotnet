using Microsoft.AspNetCore.Http;
using MyDotnet.Domain.Dto.ExceptionDomain;
using MyDotnet.Domain.Dto.Ns;
using MyDotnet.Domain.Dto.System;
using MyDotnet.Domain.Dto.WeChat;
using MyDotnet.Domain.Entity.Ns;
using MyDotnet.Domain.Entity.WeChat;
using MyDotnet.Helper;
using MyDotnet.Repository;
using MyDotnet.Services.Ns;
using MyDotnet.Services.System;

namespace MyDotnet.Services.WeChat
{
    /// <summary>
	/// 微信公众号管理服务
	/// </summary>
    public class WeChatConfigServices : BaseServices<WeChatConfig>
    {

        public BaseServices<WeChatSub> _weChatSubServices { get; set; }
        public BaseServices<WeChatKeyword> _weChatKeywordServices { get; set; }
        public IHttpContextAccessor _accessor { get; set; }
        public NightscoutServices _nightscoutServices { get; set; }
        public DicService _dictService { get; set; }
        public BaseServices<NightscoutServer> _nightscoutServerServices;

        public WeChatConfigServices(BaseRepository<WeChatConfig> baseRepository
            , BaseServices<WeChatSub> weChatSubServices
            , BaseServices<WeChatKeyword> weChatKeywordServices
            , IHttpContextAccessor accessor
            , NightscoutServices nightscoutServices
            , DicService dictService
            , BaseServices<NightscoutServer> nightscoutServerServices
            ) : base(baseRepository)
        {
            _weChatSubServices = weChatSubServices;
            _weChatKeywordServices = weChatKeywordServices;
            _accessor = accessor;
            _nightscoutServices = nightscoutServices;
            _dictService = dictService;
            _nightscoutServerServices = nightscoutServerServices;
        }


        public async Task<MessageModel<WeChatApiDto>> GetToken(string publicAccount)
        {
            var config = await Dal.QueryById(publicAccount);
            if (config == null)
                throw new ServiceException($"公众号{publicAccount}未维护至系统");
            var data = await WeChatHelper.GetToken(config.appid, config.appsecret);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");
            config.token = data.access_token;
            config.tokenExpiration = DateTime.Now.AddSeconds(data.expires_in);
            await Dal.Update(config);
            return MessageModel<WeChatApiDto>.Success("获取token成功", data);
        }
        public async Task<MessageModel<WeChatApiDto>> RefreshToken(string publicAccount)
        {
            var config = await Dal.QueryById(publicAccount);
            if (config == null)
                throw new ServiceException($"公众号{publicAccount}未维护至系统");
            //过期了,重新获取
            var data = await WeChatHelper.GetToken(config.appid, config.appsecret);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");

            config.token = data.access_token;
            config.tokenExpiration = DateTime.Now.AddSeconds(data.expires_in);
            await Dal.Update(config);
            return MessageModel<WeChatApiDto>.Success("刷新token成功", data);

        }
        public async Task<MessageModel<WeChatApiDto>> GetTemplate(string publicAccount)
        {
            var res = await GetToken(publicAccount);
            var data = await WeChatHelper.GetTemplate(res.response.access_token);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");

            return MessageModel<WeChatApiDto>.Success("获取模板成功", data);
        }
        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> GetMenu(string publicAccount)
        {
            var res = await GetToken(publicAccount);

            var data = await WeChatHelper.GetMenu(res.response.access_token);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");
            return MessageModel<WeChatApiDto>.Success("获取菜单成功", data);

        }
        /// <summary>
        /// 获取订阅用户 - 慎用
        /// </summary>
        /// <param name="publicAccount"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> GetSubUsers(string publicAccount)
        {
            var res = await GetToken(publicAccount);
            var data = await WeChatHelper.GetUsers(res.response.access_token);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");

            //data.users = new List<WeChatApiDto>();

            //foreach (var openid in data.data.openid)
            //{
            //    data.users.Add(await WeChatHelper.GetUserInfo(res.response.access_token, openid));
            //}
            return MessageModel<WeChatApiDto>.Success("获取订阅用户成功", data);

        }
        /// <summary>
        /// 获取单个订阅用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="openid"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> GetSubUser(string id, string openid)
        {
            var res = await GetToken(id);

            var data = await WeChatHelper.GetUserInfo(res.response.access_token, openid);
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");
            return MessageModel<WeChatApiDto>.Success("获取订阅用户成功", data);
        }
        public WeChatXMLDto weChat = null;
        /// <summary>
        /// 微信入口
        /// </summary>
        /// <param name="validDto"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<string> Valid(WeChatValidDto validDto, string body)
        {
            
            string objReturn = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(validDto.publicAccount))
                    throw new ServiceException("没有微信公众号唯一标识id数据");
                var config = await Dal.QueryById(validDto.publicAccount);
                if (config == null)
                    throw new ServiceException($"公众号不存在=>{validDto.publicAccount}");


                var token = config.interactiveToken;//验证用的token 和access_token不一样
                string[] arrTmp = { token, validDto.timestamp, validDto.nonce };
                Array.Sort(arrTmp);
                string combineString = string.Join("", arrTmp);
                string encryption = MD5Helper.Sha1(combineString).ToLower();

                if (encryption == validDto.signature)
                {
                    //判断是首次验证还是交互?
                    if (string.IsNullOrEmpty(validDto.echoStr))
                    {
                        //非首次验证 
                        weChat = XmlHelper.ParseFormByXml<WeChatXMLDto>(body, "xml");
                        weChat.publicAccount = validDto.publicAccount;
                        try
                        {
                            //进入事件
                            objReturn = await HandleWeChat();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.logApp.Error("公众号处理失败", ex);
                            objReturn = @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[{ex.Message}]]></Content></xml>";
                        }

                    }
                    else
                    {
                        //首次接口地址验证 
                        objReturn = validDto.echoStr;
                    }
                }
                else
                {
                    LogHelper.logApp.Error("公众号验签失败");
                    LogHelper.logApp.Error(@$"
                        来自公众号:{validDto.publicAccount}\r\n
                        微信signature:{validDto.signature}\r\n
                        微信timestamp:{validDto.timestamp}\r\n
                        微信nonce:{validDto.nonce}\r\n
                        合并字符串:{combineString}\r\n
                        微信服务器signature:{validDto.signature}\r\n
                        本地服务器signature:{encryption}
                    ");
                }

            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error("公众号异常失败");
                LogHelper.logApp.Error(ex);
                objReturn = "公众号异常失败";
            }
            return objReturn;
        }
        /// <summary>
        /// 获取绑定二维码
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatResponseUserInfo>> GetQRBind(WeChatUserInfo info)
        {
            var res = await GetToken(info.id);
            var push = new WeChatQRDto
            {
                expire_seconds = 604800,
                action_name = "QR_STR_SCENE",
                action_info = new WeChatQRActionDto
                {
                    scene = new WeChatQRActionInfoDto
                    {
                        scene_str = $"bind_{info?.id}"
                    }
                }
            };
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo();
            reData.companyCode = info.companyCode;
            reData.id = info.id;
            var pushJosn = JsonHelper.ObjToJson(push);
            var data = await WeChatHelper.GetQRCode(res.response.access_token, pushJosn);
            WeChatQR weChatQR = new WeChatQR
            {
                QRbindCompanyID = info.companyCode,
                QRbindJobID = info.userID,
                QRbindJobNick = info.userNick,
                QRcrateTime = DateTime.Now,
                QRpublicAccount = info.id,
                QRticket = data.ticket
            };
            data.id = info.userID;
            await Dal.Db.Insertable<WeChatQR>(weChatQR).ExecuteCommandAsync();
            reData.usersData = data;
            return MessageModel<WeChatResponseUserInfo>.Success("获取二维码成功", reData);
        }
        /// <summary>
        /// 推送卡片消息(绑定用户)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatResponseUserInfo>> PushCardMsg(WeChatCardMsgDataDto msg)
        {
            
            var ip = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            var bindUser = await Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == msg.info.id && t.CompanyID == msg.info.companyCode && t.IsUnBind == false && t.SubJobID == msg.info.userID).SingleAsync();
            if (bindUser == null)
                return MessageModel<WeChatResponseUserInfo>.Fail($"用户不存在或者已经解绑,公众号:{msg.info.id} 公司:{msg.info.companyCode} 员工号:{msg.info.userID}");
            var res = await GetToken(msg.info.id);
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo();
            reData.companyCode = msg.info.companyCode;
            reData.id = msg.info.id;


            string pushJson;
            if (msg.isNewVersion)
            {
                //新版

                //老版本
                var pushData = new WeChatPushCardMsgNewDto
                {
                    template_id = msg.cardMsg.template_id,
                    url = msg.cardMsg.url,
                    touser = bindUser.SubUserOpenID,
                    miniprogram = new WeChatCardMsgMiniprogram
                    {
                        appid = msg.cardMsg.miniprogram?.appid,
                        pagepath = msg.cardMsg.miniprogram?.pagepath
                    },
                    data = msg.newMsg
                };
                pushJson = JsonHelper.ObjToJson(pushData);
            }
            else
            {
                //老版本
                var pushData = new WeChatPushCardMsgDto
                {
                    template_id = msg.cardMsg.template_id,
                    url = msg.cardMsg.url,
                    touser = bindUser.SubUserOpenID,
                    miniprogram = new WeChatCardMsgMiniprogram
                    {
                        appid = msg.cardMsg.miniprogram?.appid,
                        pagepath = msg.cardMsg.miniprogram?.pagepath
                    },
                    data = new WeChatPushCardMsgDetailDto
                    {
                        first = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.first,
                            color = msg.cardMsg.color1
                        },
                        keyword1 = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.keyword1,
                            color = msg.cardMsg.color1
                        },
                        keyword2 = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.keyword2,
                            color = msg.cardMsg.color2
                        },
                        keyword3 = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.keyword3,
                            color = msg.cardMsg.color3
                        },
                        keyword4 = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.keyword4,
                            color = msg.cardMsg.color4
                        },
                        keyword5 = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.keyword5,
                            color = msg.cardMsg.color5
                        },
                        remark = new WeChatPushCardMsgValueColorDto
                        {
                            value = msg.cardMsg.remark,
                            color = msg.cardMsg.colorRemark
                        }
                    }
                };
                pushJson = JsonHelper.ObjToJson(pushData);
            }
            var data = await WeChatHelper.SendCardMsg(res.response.access_token, pushJson);
            reData.usersData = data;
            try
            {
                var pushLog = new WeChatPushLog
                {
                    PushLogCompanyID = msg.info.companyCode,
                    PushLogPublicAccount = msg.info.id,
                    PushLogContent = pushJson,
                    PushLogOpenid = bindUser.SubUserOpenID,
                    PushLogToUserID = bindUser.SubJobID,
                    PushLogStatus = data.errcode == 0 ? "Y" : "N",
                    PushLogRemark = data.errmsg,
                    PushLogTime = DateTime.Now,
                    PushLogTemplateID = msg.cardMsg.template_id,
                    PushLogIP = ip
                };
                await Dal.Db.Insertable<WeChatPushLog>(pushLog).ExecuteReturnSnowflakeIdAsync();
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error("推送失败");
                LogHelper.logApp.Error(ex);
            }
            if (reData.usersData.errcode.Equals(0))
            {
                return MessageModel<WeChatResponseUserInfo>.Success("卡片消息推送成功", reData);
            }
            else
            {
                return MessageModel<WeChatResponseUserInfo>.Fail($"卡片消息推送失败=>{reData.usersData?.errmsg}", reData);
            }
        }
        /// <summary>
        /// 推送文本消息(绑定或订阅)
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> PushTxtMsg(WeChatPushTestDto msg)
        {
            var res = await GetToken(msg.selectWeChat);
            var token = res.response.access_token;
            if (msg.selectBindOrSub.Equals("sub"))
            {
                return await PushText(token, msg);
            }
            else
            {
                MessageModel<WeChatApiDto> messageModel = new MessageModel<WeChatApiDto>();
                messageModel.success = true;
                //绑定用户
                if (msg.selectOperate.Equals("one"))
                {
                    //发送单个 
                    var usrs = Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount.Equals(msg.selectWeChat) && t.CompanyID.Equals(msg.selectCompany) && t.SubJobID.Equals(msg.selectUser)).ToList();
                    foreach (var item in usrs)
                    {
                        msg.selectUser = item.SubUserOpenID;
                        var info = await PushText(token, msg);
                        if (!info.success)
                        {
                            messageModel.success = false;
                        }
                        messageModel.msg += info.msg;
                    }
                }
                else
                {
                    //发送所有
                    var usrs = Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount.Equals(msg.selectWeChat) && t.CompanyID.Equals(msg.selectCompany)).ToList();
                    foreach (var item in usrs)
                    {
                        msg.selectUser = item.SubUserOpenID;
                        var info = await PushText(token, msg);
                        if (!info.success)
                        {
                            messageModel.success = false;
                        }
                        messageModel.msg += info.msg;
                    }
                }
                return messageModel;
            }

        }
        /// <summary>
        /// 推送消息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task<MessageModel<WeChatApiDto>> PushText(string token, WeChatPushTestDto msg)
        {
            object data;
            WeChatApiDto pushres;
            //订阅用户  
            switch (msg.selectMsgType)
            {
                case "text":
                    //发送文本 
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.selectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.selectUser,
                        msgtype = msg.selectMsgType,
                        text = new
                        {
                            content = msg.textContent.text
                        }
                    };

                    if (msg.selectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "image":
                    //发送图片 
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.selectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.selectUser,
                        msgtype = msg.selectMsgType,
                        images = new
                        {
                            media_ids = new List<string> {
                                msg.pictureContent.pictureMediaID
                            },
                            recommend = "xxx",
                            need_open_comment = 1,
                            only_fans_can_comment = 0
                        }
                    };
                    if (msg.selectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "voice":
                    //发送音频
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.selectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.selectUser,
                        msgtype = msg.selectMsgType,
                        voice = new
                        {
                            media_id = msg.voiceContent.voiceMediaID
                        }
                    };
                    if (msg.selectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                case "mpvideo":
                    //发送视频
                    data = new
                    {
                        filter = new
                        {
                            is_to_all = msg.selectOperate.Equals("one") ? false : true,
                            tag_id = 0,
                        },
                        touser = msg.selectUser,
                        msgtype = msg.selectMsgType,
                        mpvideo = new
                        {
                            media_id = msg.videoContent.videoMediaID,
                        }
                    };
                    if (msg.selectOperate.Equals("one"))
                    {
                        pushres = await WeChatHelper.SendMsg(token, JsonHelper.ObjToJson(data));
                    }
                    else
                    {
                        pushres = await WeChatHelper.SendMsgToAll(token, JsonHelper.ObjToJson(data));
                    }
                    break;
                default:
                    pushres = new WeChatApiDto() { errcode = -1, errmsg = $"未找到推送类型{msg.selectMsgType}" };
                    break;
            }
            if (!pushres.errcode.Equals(0))
                throw new ServiceException($"错误代码:{pushres.errcode} 错误信息:{pushres.errmsg}");
            return MessageModel<WeChatApiDto>.Success("推送成功", pushres);
        }
        /// <summary>
        /// 更新菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<MessageModel<WeChatApiDto>> UpdateMenu(WeChatApiDto menu)
        {
            WeChatHelper.ConverMenuButtonForEvent(menu);
            var res = await GetToken(menu.id);
            var data = await WeChatHelper.SetMenu(res.response.access_token, JsonHelper.ObjToJson(menu.menu));
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");
            return MessageModel<WeChatApiDto>.Success("更新成功", data);
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<MessageModel<WeChatResponseUserInfo>> GetBindUserInfo(WeChatUserInfo info)
        {
            var bindUser = await Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == info.id && t.CompanyID == info.companyCode && info.userID.Equals(t.SubJobID)).SingleAsync();
            if (bindUser == null || bindUser.IsUnBind)
                throw new ServiceException("用户不存在或者用户已解绑");
            var res = await GetToken(info.id);
            var token = res.response.access_token;
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo();
            reData.companyCode = info.companyCode;
            reData.id = info.id;
            var data = await WeChatHelper.GetUserInfo(token, bindUser.SubUserOpenID);
            reData.usersData = data;
            if (!data.errcode.Equals(0))
                throw new ServiceException($"错误代码:{data.errcode} 错误信息:{data.errmsg}");
            return MessageModel<WeChatResponseUserInfo>.Success("用户信息获取成功", reData);
        }
        /// <summary>
        /// 解绑用户
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        public async Task<MessageModel<WeChatResponseUserInfo>> UnBind(WeChatUserInfo info)
        {
            var bindUser = await Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == info.id && t.CompanyID == info.companyCode && info.userID.Equals(t.SubJobID)).FirstAsync();
            if (bindUser == null || bindUser.IsUnBind)
                throw new ServiceException("用户不存在或者用户已解绑");
            WeChatResponseUserInfo reData = new WeChatResponseUserInfo();
            reData.companyCode = info.companyCode;
            reData.id = info.id;
            bindUser.IsUnBind = true;
            bindUser.SubUserRefTime = DateTime.Now;
            await Dal.Db.Updateable<WeChatSub>(bindUser).UpdateColumns(t => new { t.IsUnBind, t.SubUserRefTime }).ExecuteCommandAsync();
            return MessageModel<WeChatResponseUserInfo>.Success("用户解绑成功", reData);
        }
        /// <summary>
        /// 处理微信公众号事件
        /// </summary>
        /// <returns></returns>
        public async Task<string> HandleWeChat()
        {
            switch (weChat.MsgType)
            {
                case "text":
                    return await HandText();
                case "image":
                    return await HandImage();
                case "voice":
                    return await HandVoice();
                case "shortvideo":
                    return await HandShortvideo();
                case "location":
                    return await HandLocation();
                case "link":
                    return await HandLink();
                case "event":
                    return await HandEvent();
                default:
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[处理失败,没有找到消息类型=>{weChat.MsgType}]]></Content></xml>";
            }
        }
        /// <summary>
        /// 处理文本
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandText()
        {
            return await HandleKeyword();

        }


        /// <summary>
        /// 处理关键词
        /// </summary>
        /// <param name="isEvent"></param>
        /// <returns></returns>
        private async Task<string> HandleKeyword(bool isEvent = false)
        {
            var key = isEvent ? weChat.EventKey : weChat.Content.ObjToString().Trim();


            var weChatLaunchNsKey = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.weChatLaunchNsKey);
            //启动关键词捕获
            if (weChatLaunchNsKey.content.Equals(key))
            {
                


                 
                //推送消息
                var toekn = await GetToken(weChat.publicAccount);
                var sendWechat = new WeChatPushTestDto();
                sendWechat.selectMsgType = "text";
                sendWechat.selectOperate = "one";
                sendWechat.selectUser = weChat.FromUserName;
                sendWechat.textContent = new WeChatPushTextContentDto();
                sendWechat.textContent.text = "您的实例正在启动请稍等";
                var sendWechatRes = await PushText(toekn.response.access_token, sendWechat); 

                Task.Run(async () =>
                {
                    //当前实例
                    Nightscout curNs = null;
                    try
                    {
                        var pushWechatID = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.pushWechatID);
                        var pushCompanyCode = await _dictService.GetDicDataOne(NsInfo.KEY, NsInfo.pushCompanyCode);
                        //推送ns的微信公众号
                        if (weChat.publicAccount.Equals(pushWechatID.content))
                        {
                            //有哪些绑定的ns
                            var ls = await Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == pushWechatID.content && t.CompanyID == pushCompanyCode.content && t.SubUserOpenID == weChat.FromUserName && t.IsUnBind == false).Select(t => t.SubJobID).ToListAsync();
                            //查找ns实例
                            var nsList = await _nightscoutServices.Dal.QueryByIDs(ls.ToArray());
                            nsList = nsList.Where(t => t.isStop == true).ToList();
                            if (ls.Count > 0 && nsList.Count>0)
                            {
                                foreach (var ns in nsList)
                                {
                                    curNs = ns;
                                    //启动实例
                                    if(DateTime.Now > ns.endTime)
                                    {

                                        //推送消息
                                        var toekn = await GetToken(weChat.publicAccount);
                                        var sendWechat = new WeChatPushTestDto();
                                        sendWechat.selectMsgType = "text";
                                        sendWechat.selectOperate = "one";
                                        sendWechat.selectUser = weChat.FromUserName;
                                        sendWechat.textContent = new WeChatPushTextContentDto();
                                        sendWechat.textContent.text = $"{ns.name},您好!您的Ns已经到期,无法启动!";
                                        var sendWechatRes = await PushText(toekn.response.access_token, sendWechat);
                                    }
                                    else
                                    {
                                        var nsServer = await _nightscoutServerServices.Dal.QueryById(ns.serverId);
                                        await _nightscoutServices.Refresh(ns, nsServer);
                                        //推送消息
                                        var toekn = await GetToken(weChat.publicAccount);
                                        var sendWechat = new WeChatPushTestDto();
                                        sendWechat.selectMsgType = "text";
                                        sendWechat.selectOperate = "one";
                                        sendWechat.selectUser = weChat.FromUserName;
                                        sendWechat.textContent = new WeChatPushTextContentDto();
                                        sendWechat.textContent.text = $"{ns.name},您好!您的Ns启动成功了!";
                                        var sendWechatRes = await PushText(toekn.response.access_token, sendWechat);
                                    }
                                }
                            }
                            else
                            {
                                //推送消息
                                var toekn = await GetToken(weChat.publicAccount);
                                var sendWechat = new WeChatPushTestDto();
                                sendWechat.selectMsgType = "text";
                                sendWechat.selectOperate = "one";
                                sendWechat.selectUser = weChat.FromUserName;
                                sendWechat.textContent = new WeChatPushTextContentDto();
                                sendWechat.textContent.text = $"您好!您当前没有可以启动的实例";
                                var sendWechatRes = await PushText(toekn.response.access_token, sendWechat);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if(curNs != null)
                        {
                            LogHelper.logApp.Error($"{curNs.name}的启动恢复异常,id:{curNs.Id}", ex);
                        }
                        else
                        {
                            LogHelper.logApp.Error($"启动恢复异常", ex);
                        }
                        try
                        {
                            //推送消息
                            var toekn = await GetToken(weChat.publicAccount);
                            var sendWechat = new WeChatPushTestDto();
                            sendWechat.selectMsgType = "text";
                            sendWechat.selectOperate = "one";
                            sendWechat.selectUser = weChat.FromUserName;
                            sendWechat.textContent = new WeChatPushTextContentDto();
                            if (curNs != null)
                            {
                                sendWechat.textContent.text = $"{curNs.name},您好!您的Ns启动失败了!请联系管路员!";
                            }
                            else
                            {
                                sendWechat.textContent.text = $"您好!您的Ns启动失败了!请联系管路员!";
                            }
                            var sendWechatRes = await PushText(toekn.response.access_token, sendWechat);
                        }
                        catch (Exception ex2)
                        {
                            LogHelper.logApp.Error("推送启动恢复消息异常", ex2);
                        }
                    }
                });

            }
            var findKeys = await _weChatKeywordServices.Dal.Query(t => t.publicAccount.Equals(weChat.publicAccount) && t.key.Equals(key));
            if (findKeys != null && findKeys.Count > 0)
            {
                var findKey = findKeys[0];
                switch (findKey.media_type)
                {
                    case "text":
                        return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findKey.media_type}]]></MsgType>
                                <Content><![CDATA[{findKey.media_desc}]]></Content>
                                </xml>";
                    case "image":
                        return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findKey.media_type}]]></MsgType>
                                <Image><MediaId><![CDATA[{findKey.media_id}]]></MediaId></Image>
                                </xml>";
                    case "voice":
                        return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findKey.media_type}]]></MsgType>
                                <Voice><MediaId><![CDATA[{findKey.media_id}]]></MediaId></Voice>
                                </xml>";
                    case "video":
                        return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findKey.media_type}]]></MsgType>
                                <Video>
                                     <MediaId><![CDATA[{findKey.media_id}]]></MediaId>
                                     <Title><![CDATA[{findKey.title}]]></Title>
                                     <Description><![CDATA[{findKey.description}]]></Description>
                                </Video>
                                </xml>";
                    default:
                        return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[匹配错误:media_type:{findKey.media_type}]]></Content>
                                </xml>";
                }
            }
            else
            {
                return @$"<xml>
                        <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                        <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                        <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                        <MsgType><![CDATA[text]]></MsgType>
                        <Content><![CDATA[我收到了消息=>{(isEvent ? weChat.EventKey : weChat.Content)}]]></Content>
                        </xml>";
            }
        }
        /// <summary>
        /// 处理图片
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandImage()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了图片=>{weChat.PicUrl}]]></Content></xml>";
        }
        /// <summary>
        /// 处理声音
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandVoice()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了声音=>{weChat.MediaId}]]></Content></xml>";
        }
        /// <summary>
        /// 处理小视频
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandShortvideo()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了小视频=>{weChat.MediaId}]]></Content></xml>";
        }
        /// <summary>
        /// 处理地理位置
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandLocation()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了地址位置=>{weChat.Label}]]></Content></xml>";
        }
        /// <summary>
        /// 处理链接消息
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandLink()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了链接=>{weChat.Url}]]></Content></xml>";
        }
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandEvent()
        {
            switch (weChat.Event)
            {
                case "subscribe":
                    return await EventSubscribe();
                case "unsubscribe":
                    return await EventUnsubscribe();
                case "SCAN":
                    return await EventSCAN();
                case "LOCATION":
                    return await EventLOCATION();
                case "CLICK":
                    return await EventCLICK();
                case "VIEW":
                    return await EventVIEW();
                case "TEMPLATESENDJOBFINISH":
                    //模板消息回执
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[模板回执:{weChat.Statuss}]]></Content></xml>";
                default:
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[处理失败,没有找到事件类型=>{weChat.Event}]]></Content></xml>";
            }
        }
        /// <summary>
        /// 关注事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventSubscribe()
        {
            if (weChat.EventKey != null && weChat.EventKey.Contains("bind"))
            {
                return await QRBind();
            }
            else
            {
                var findWechat = await Dal.QueryById(weChat.publicAccount);
                if (findWechat != null && findWechat.isFocusReply)
                {
                    switch (findWechat.replyType)
                    {
                        case "text":
                            return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findWechat.replyType}]]></MsgType>
                                <Content><![CDATA[{findWechat.replyText}]]></Content>
                                </xml>";
                        case "image":
                            return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findWechat.replyType}]]></MsgType>
                                <Image><MediaId><![CDATA[{findWechat.replyID}]]></MediaId></Image>
                                </xml>";
                        case "voice":
                            return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findWechat.replyType}]]></MsgType>
                                <Voice><MediaId><![CDATA[{findWechat.replyID}]]></MediaId></Voice>
                                </xml>";
                        case "video":
                            return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[{findWechat.replyType}]]></MsgType>
                                <Video>
                                     <MediaId><![CDATA[{findWechat.replyID}]]></MediaId>
                                     <Title><![CDATA[{findWechat.replyTitle}]]></Title>
                                     <Description><![CDATA[{findWechat.replyDescription}]]></Description>
                                </Video>
                                </xml>";
                        default:
                            return @$"<xml>
                                <ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[匹配错误:media_type:{findWechat.replyType}]]></Content>
                                </xml>";
                    }
                }
                else
                {
                    return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[感谢您的关注!]]></Content></xml>";
                }


            }
        }
        /// <summary>
        /// 取消关注事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventUnsubscribe()
        {
            var data = await _weChatSubServices.Dal.Query(t => t.SubFromPublicAccount == weChat.publicAccount && t.SubUserOpenID == weChat.FromUserName && t.IsUnBind == false);
            foreach (var item in data)
            {
                item.IsUnBind = true;
                item.SubUserRefTime = DateTime.Now;
            }
            await Dal.Db.Updateable<WeChatSub>(data).UpdateColumns(t => new { t.IsUnBind, t.SubUserRefTime }).ExecuteCommandAsync();
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了取消关注事件=>{weChat.Event}]]></Content></xml>";
        }
        /// <summary>
        /// 已关注扫码事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventSCAN()
        {
            if (weChat.EventKey != null && weChat.EventKey.Contains("bind"))
            {
                return await QRBind();
            }
            else
            {
                return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了已关注扫码事件=>key:{weChat.EventKey}=>ticket:{weChat.Ticket}]]></Content></xml>";
            }

        }
        /// <summary>
        /// 扫码绑定
        /// </summary>
        /// <returns></returns>

        private async Task<string> QRBind()
        {
            var ticket = await Dal.Db.Queryable<WeChatQR>().InSingleAsync(weChat.Ticket);
            if (ticket == null || ticket.QRisUsed || !ticket.QRpublicAccount.Equals(weChat.publicAccount))
                throw new ServiceException("无效的绑定信息，请勿重复扫码");

            var bindUser = await Dal.Db.Queryable<WeChatSub>().Where(t => t.SubFromPublicAccount == ticket.QRpublicAccount && t.CompanyID == ticket.QRbindCompanyID && t.SubJobID == ticket.QRbindJobID).SingleAsync();
            bool isNewBind = false;
            if (bindUser == null)
            {
                isNewBind = true;
                bindUser = new WeChatSub
                {
                    SubFromPublicAccount = ticket.QRpublicAccount,
                    CompanyID = ticket.QRbindCompanyID,
                    SubJobID = ticket.QRbindJobID,
                    SubUserOpenID = weChat.FromUserName,
                    SubUserRegTime = DateTime.Now,
                };
            }
            else
            {
                isNewBind = false;
                //订阅过的就更新
                if (bindUser.SubUserOpenID != weChat.FromUserName)
                {
                    //记录上一次的订阅此工号的微信号
                    bindUser.LastSubUserOpenID = bindUser.SubUserOpenID;
                }
                bindUser.SubUserOpenID = weChat.FromUserName;
                bindUser.SubUserRefTime = DateTime.Now;
                bindUser.IsUnBind = false;
            }
            ticket.QRisUsed = true;
            ticket.QRuseTime = DateTime.Now;
            ticket.QRuseOpenid = weChat.FromUserName;

            try
            {
                Dal._unitOfWorkManage.BeginTran();
                await Dal.Db.Updateable<WeChatQR>(ticket).ExecuteCommandAsync();
                if (isNewBind)
                    await Dal.Db.Insertable<WeChatSub>(bindUser).ExecuteCommandAsync();
                else
                    await Dal.Db.Updateable<WeChatSub>(bindUser).ExecuteCommandAsync();
                Dal._unitOfWorkManage.CommitTran();
            }
            catch
            {
                Dal._unitOfWorkManage.RollbackTran();
                throw;
            }
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[恭喜您！{(string.IsNullOrEmpty(ticket.QRbindJobNick) ? ticket.QRbindJobID : ticket.QRbindJobNick)}，绑定成功！请勿重复扫码绑定！]]></Content></xml>";
        }
        /// <summary>
        /// 上报位置地理事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventLOCATION()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了地理位置事件=>维度:{weChat.Latitude}经度:{weChat.Longitude}位置精度:{weChat.Precision}]]></Content></xml>";
        }
        /// <summary>
        /// 点击菜单按钮事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventCLICK()
        {
            return await HandleKeyword(true);
        }
        /// <summary>
        /// 点击菜单网址事件
        /// </summary>
        /// <returns></returns>
        private async Task<string> EventVIEW()
        {
            await Task.CompletedTask;
            return @$"<xml><ToUserName><![CDATA[{weChat.FromUserName}]]></ToUserName>
                                <FromUserName><![CDATA[{weChat.ToUserName}]]></FromUserName>
                                <CreateTime>{DateTime.Now.Ticks.ToString()}</CreateTime>
                                <MsgType><![CDATA[text]]></MsgType>
                                <Content><![CDATA[我收到了菜单点击网址事件=>{weChat.EventKey}]]></Content></xml>";
        }

    }
}