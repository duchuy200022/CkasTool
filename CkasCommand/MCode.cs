using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.CkasCommand
{
    public class MCode
    {
        public enum modeMoveCartesian
        {
            REPORT,
            STATIC,
            DYNAMIC,
            MIXED
        };

        public static string Move_Cartesian(modeMoveCartesian mode, int mx = 0, int my = 0, 
            int mz = 0, string mroll = "0", string mpitch = "0",
            string myaw = "0", string m_axi = "0", string m_ayi = "0", 
            string m_azi = "0", string m_axg = "0", string m_ayg = "0", string m_azg = "9.81",
            string m_wz = "0", string m_wy = "0", string m_wx = "0")
        {
            string s;

            switch (mode)
            {
                case (modeMoveCartesian.STATIC):
                    s = "~M " + mx.ToString() + " " + my.ToString() + " " + mz.ToString() + " " + mroll.ToString() + " " +
                        mpitch.ToString() + " " + myaw.ToString();
                    break;

                case (modeMoveCartesian.DYNAMIC):
                    s = "~M 0 0 0 0 0 0 " + m_axi + " " + m_ayi + " " + m_azi + " " +
                        m_axg + " " + m_ayg + " " + m_azg + " " +
                        m_wz + " " + m_wy + " " + m_wx;
                    break;

                case (modeMoveCartesian.MIXED):
                    s = "~M " + mx.ToString() + " " + my.ToString() + " " + mz.ToString() + " " +
                        mroll.ToString() + " " + mpitch.ToString() + " " + myaw.ToString() + " " +
                        m_axi.ToString() + " " + m_ayi.ToString() + " " + m_azi.ToString() + " " +
                        m_axg.ToString() + " " + m_ayg.ToString() + " " + m_azg.ToString() + " " +
                        m_wz.ToString() + " " + m_wy.ToString() + " " + m_wx.ToString();
                    break;

                default:
                    s = "~M";
                    break;
            }

            return s;
        }
    }
}
