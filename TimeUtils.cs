using System;
using System.Collections.Generic;
using System.Text;

namespace VSTSTimeTrackerAddin
{
    public class TimeUtils
    {
        /// <summary>
        /// Gets a DateTime from a time string in the format hh:mm:ss. This static member is useful
        /// for creating DateTime objects for time counts.
        /// </summary>
        /// <param name="textTime"></param>
        /// <param name="timeCountStart"></param>
        /// <returns></returns>
        public static DateTime GetStartDateTimeFromText(string textTime, DateTime timeCountStart)
        {
            string[] timePiecesArr = textTime.Split(":".ToCharArray());

            if (timePiecesArr.Length != 3)
                return new DateTime(0);

            TimeSpan ts = new TimeSpan(int.Parse(timePiecesArr[0]), int.Parse(timePiecesArr[1]), int.Parse(timePiecesArr[2]));

            timeCountStart = timeCountStart.Subtract(ts);

            return timeCountStart;
        }

        /// <summary>
        /// Gets a TimeSpan from a Decimal time (in hours).
        /// </summary>
        /// <param name="decTime"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpanFromDecimal(Decimal decTime)
        {
            int h = Decimal.ToInt32(decTime);

            Decimal aux = decTime - h;

            int m = Decimal.ToInt32(aux * 60);

            aux = aux * 60 - m;

            int s = Decimal.ToInt32(aux * 60);

            return new TimeSpan(h, m, s);
        }
        /// <summary>
        /// Gets a Decimal time (in hours) from a TimeSpan object.
        /// </summary>
        /// <param name="tsTime"></param>
        /// <returns></returns>
        public static Decimal GetDecimalFromTimeSpan(TimeSpan tsTime)
        {
            return new Decimal(tsTime.TotalHours);
        }

        /// <summary>
        /// Converts a TimeSpan object to a time string in the format hh:mm:ss.
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string GetTimeStringFromTimeSpan(TimeSpan ts)
	    {
		    double aux = ts.TotalHours;
            int h = (int)Math.Truncate(aux);

            aux = (aux - h)*60;

            int m = (int)Math.Truncate(aux);

            aux = (aux - m) * 60;

            int s = (int)Math.Truncate(aux);

    		return h.ToString("00")+":"+m.ToString("00")+":"+s.ToString("00");    	
	    }
        /// <summary>
        /// Converts a time string in the format hh:mm:ss to a TimeSpan object.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeSpanFromTimeString(string time)
        {
            string[] timePiecesArr = time.Split(":".ToCharArray());

            if (timePiecesArr.Length != 3)
                return TimeSpan.Zero;

            return new TimeSpan(int.Parse(timePiecesArr[0]), int.Parse(timePiecesArr[1]), int.Parse(timePiecesArr[2]));
        }
    }
}
