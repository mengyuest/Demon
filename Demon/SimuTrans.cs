using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demon
{
    static public class SimuTrans
    {
        static public String TransIntToString( int theSimuTime)
        {
            int seconds = theSimuTime % 60;
            int minutes = theSimuTime / 60 % 60;
            int hours = theSimuTime / 3600 % 12;
            
            String sec ;
            String minu;
            String hou ;

            if(seconds < 10 )
            {
                sec = "0" + seconds.ToString();
            }

            else
            {
                sec = seconds.ToString();
            }

            if(minutes < 10 )
            {
                minu = "0" + minutes.ToString();
            }

            else
            {
                minu = minutes.ToString();
            }

            if(hours < 10 )
            {
                hou = "0" + hours.ToString();
            }

            else
            {
                hou = hours.ToString();
            }


            String simuTime = hou + ":" + minu + ":" + sec;

            return simuTime;
        }

        static public int  TransStringToInt( String theSimu)
        {
            int hour = Int32.Parse(theSimu.Substring(0,2));
            int minute = Int32.Parse(theSimu.Substring(3,2));
            int second = Int32.Parse(theSimu.Substring(6,2));
            return hour * 3600 + minute * 60 + second;
        }
    }
}
