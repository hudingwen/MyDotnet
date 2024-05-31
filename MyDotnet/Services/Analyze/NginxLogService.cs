using MyDotnet.Domain.Entity.Base;
using MyDotnet.Repository;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net;
using MyDotnet.Helper;
using MyDotnet.Domain.Entity.Nginx;
using SharpCompress.Common;

namespace MyDotnet.Services.Analyze
{
    /// <summary>
    /// Nginx日志服务类
    /// </summary>
    public class NginxLogService : BaseServices<BaseEntity>
    {
        public NginxLogService(BaseRepository<BaseEntity> baseRepository) : base(baseRepository)
        {

        }
        /// <summary>
        /// 分析Nginxlog日志
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public async Task<List<NginxHostRequest>> AnalyzeLog(string filePath)
        {

            List<NginxHostRequest> requests = new List<NginxHostRequest>();
            //string filePath = "D:\\360Downloads\\desktop\\github\\Nginx日志\\access.log";

            // 检查文件是否存在
            if (!File.Exists(filePath))
            {
                return requests;
            }
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;

                    string pattern = "\"([^\"]*)\"";
                    Regex regex = new Regex(pattern);

                    while ((line = reader.ReadLine()) != null)
                    {
                        MatchCollection matches = regex.Matches(line);

                        if(matches.Count == 5)
                        {


                            //时间
                            var requestTimeStr = matches[0].Groups[1].Value;
                            string format = "dd/MMM/yyyy:HH:mm:ss zzz";
                            DateTime dateTime = DateTime.ParseExact(requestTimeStr, format, CultureInfo.InvariantCulture);

                            //host
                            var host = matches[2].Groups[1].Value;
                            var findHost = requests.Find(t => t.host.Equals(host));
                            if (findHost == null)
                            {
                                findHost = new NginxHostRequest();
                                findHost.host = host;
                                requests.Add(findHost);
                            }
                            findHost.date = dateTime;
                            //url
                            var urlStr = matches[3].Groups[1].Value;

                            var url = urlStr;
                            if (urlStr.IndexOf("?") > 0)
                            {
                                url = urlStr.Substring(0, urlStr.IndexOf("?"));
                            }
                            var urlSp = url.Split(" ");
                            if (urlSp.Length > 1)
                            {
                                url = urlSp[1];
                            }
                            else
                            {
                                if ("-".Equals(host))
                                {
                                    url = "";
                                }
                            } 
                            url = WebUtility.UrlDecode(url);
                            //var urlSp = urlStr.Split(" ");
                            //var url = WebUtility.UrlDecode(urlSp[1]);

                            //url统计
                            var findUrl = findHost.urls.Find(t=>t.url.Equals(url));
                            if(findUrl == null)
                            {
                                findUrl= new NginxHostUrlRequest();
                                findUrl.url = url;
                                findHost.urls.Add(findUrl);
                            }

                            findHost.requestCount += 1;
                            findUrl.requestCount+= 1;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.logApp.Error("分析发生错误", ex);
                throw;
            }
            return requests.OrderByDescending(t=>t.requestCount).ToList();
        }

    }
}
