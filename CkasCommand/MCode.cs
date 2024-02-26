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

        public static string Move_Cartesian(modeMoveCartesian mode, int mx = 0, int my = 0, int mz = 0, int mroll = 0, int mpitch = 0,
            int myaw = 0, float m_axi = 0, float m_ayi = 0, float m_azi = 0, float m_axg = 0, float m_ayg = 0, float m_azg = (float)9.81,
            float m_wz = 0, float m_wy = 0, float m_wx = 0)
        {
            string s;

            switch (mode)
            {
                case (modeMoveCartesian.STATIC):
                    s = "~M " + mx.ToString() + " " + my.ToString() + " " + mz.ToString() + " " + mroll.ToString() + " " +
                        mpitch.ToString() + " " + myaw.ToString();
                    break;

                case (modeMoveCartesian.DYNAMIC):
                    s = "~M 0 0 0 0 0 0 " + m_axi.ToString() + " " + m_ayi.ToString() + " " + m_azi.ToString() + " " +
                        m_axg.ToString() + " " + m_ayg.ToString() + " " + m_azg.ToString() + " " +
                        m_wz.ToString() + " " + m_wy.ToString() + " " + m_wx.ToString();
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
