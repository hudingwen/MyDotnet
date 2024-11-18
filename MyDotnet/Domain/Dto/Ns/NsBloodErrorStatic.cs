namespace MyDotnet.Domain.Dto.Ns
{
    public class NsBloodErrorStatic
    {
        /// <summary>
        /// 统计异常次数
        /// </summary>
        public static Dictionary<long,int> errStatic = new Dictionary<long,int>();
        public static DateTime checkErrStatic(long jobid, DateTime nextTime,bool isTouchTime)
        {
            if(!errStatic.ContainsKey(jobid))
            {
                errStatic.Add(jobid, 1);
            }
            else
            {
                errStatic[jobid] = errStatic[jobid] + 1;
            }

            if (errStatic[jobid] > 3)
            {
                if(isTouchTime)
                {
                    //刷新血糖后重置计数器
                    errStatic[jobid] = 0;
                    //异常血糖5分钟后再执行
                    return DateTime.Now.AddMinutes(5); 
                }
                else
                {
                    if (errStatic[jobid] > 10)
                    {

                        return DateTime.Now.AddMinutes(30);
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
