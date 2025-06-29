using System;

namespace GravityAI.Client
{
    internal class GaiApiHelper
    {
        public static TimeSpan GetPollDelay(long count)
        {
            if (count < 3) { return TimeSpan.FromMilliseconds(500); }
            if (count < 6) { return TimeSpan.FromMilliseconds(1000); }
            if (count < 10) { return TimeSpan.FromMilliseconds(2000); }
            return TimeSpan.FromMilliseconds(5000);
        }
    }



}
