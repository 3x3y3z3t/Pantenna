// ;
using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Game;
using VRage.Library.Utils;

namespace ExSharedCore
{
    public enum LoggerSide
    {
        Common = 0,
        Server,
        Client,
    }

    public class Logger
    {
        private MyTimeSpan m_LocalUtcOffset;

        private TextWriter m_TextWriter;

        private static Logger s_Instance = null;

        private Logger()
        { }

        ~Logger()
        {
            DeInit();
        }

        public static bool Init(LoggerSide _loggerSide)
        {
            TimeSpan offs;
            TimeSpan.TryParse(DateTime.Now.ToString("zzz"), out offs);

            switch (_loggerSide)
            {
                case LoggerSide.Server:
                    s_Instance = new Logger
                    {
                        m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage("debug_server.log", typeof(ExSharedCore.Logger))
                    };
                    break;
                case LoggerSide.Client:
                    s_Instance = new Logger
                    {
                        m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage("debug_client.log", typeof(ExSharedCore.Logger))
                    };
                    break;
                default:
                    s_Instance = new Logger
                    {
                        m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage("debug_common.log", typeof(ExSharedCore.Logger))
                    };
                    break;
            }
            s_Instance.m_LocalUtcOffset = new MyTimeSpan(offs.Ticks);

            Log(">>> Log Begin <<<");

            return true;
        }

        public static bool DeInit()
        {
            if (s_Instance == null)
                return true;

            Log(">>> Log End <<<");

            if (s_Instance.m_TextWriter != null)
                s_Instance.m_TextWriter.Close();
            s_Instance = null;

            return true;
        }

        public static void Log(string _message, uint _level = 0)
        {
            if (s_Instance == null)
                Init(LoggerSide.Common);

            try
            {
                s_Instance.m_TextWriter.WriteLine("[{0:0}]: {1:0}", s_Instance.GetDateTimeAsString(), _message);
                s_Instance.m_TextWriter.Flush();
            }
            catch (Exception _e)
            { }
        }

        internal string GetDateTimeAsString()
        {
            DateTime datetime = DateTime.UtcNow + m_LocalUtcOffset.TimeSpan;
            datetime = DateTime.Now;
            return datetime.ToString("yy.MM.dd HH:mm:ss.ff");
        }
    }
}
