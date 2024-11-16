namespace MyDotnet.Domain.Dto.Ns
{
    public class NsBloodErrorStatic
    {
        /// <summary>
        /// 统计异常次数
        /// </summary>
        public static Dictionary<long,int> errStatic = new Dictionary<long,int>();
        public static DateTime checkErrStatic(long uid,DateTime nextTime,bool isTouchTime)
        {
            if(!errStatic.ContainsKey(uid))
            {
                errStatic.Add(uid, 1);
            }
            else
            {
                errStatic[uid] = errStatic[uid] + 1;
            }

            if (errStatic[uid] > 3)
            {
                if(isTouchTime)
                {
                    //刷新血糖后重置计数器
                    errStatic[uid] = 0;
                    //异常血糖5分钟后再执行
                    return DateTime.Now.AddMinutes(5); 
                }
                else
                {
                    if (errStatic[uid] > 10)
                    {

                        return DateTime.Now.AddMinutes(60);
                    }
                    else
                    {
                        return DateTime.Now.AddMinutes(10);
                    }
                }
            }
            else
            {
                return nextTime;
            }
        }
    }
}
