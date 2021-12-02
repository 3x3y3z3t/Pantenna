// ;
using Sandbox.ModAPI;
using System;
using System.IO;
using VRage.Utils;

namespace ExShared
{
    public enum LoggerSide
    {
        Custom = 0,
        Server,
        Client,
    }

    public class Logger
    {
        private const string c_LogPrefix = "[Pantenna]";

        //private MyTimeSpan m_LocalUtcOffset;
        
        private TextWriter m_TextWriter;
        private TextWriter m_CustomTextWriter;
        private int m_LogLevel = 5;
        private bool m_IsSuppressed = false;

        public static string CustomLogFilename { get; set; }
        private static Logger s_Instance = null;

        /// <summary> Initializes Logger instance. Unless spacified, Logger Side is Common by default. </summary>
        /// <param name="_loggerSide">indicates which side, client or server, this Logger instance is working on</param>
        /// <returns>true if the initialization success, otherwise false</returns>
        public static bool Init(LoggerSide _loggerSide)
        {
            if (s_Instance != null)
                return false;

            MyLog.Default.WriteLine(c_LogPrefix + " Logger.Init() called");
            TimeSpan offs;
            TimeSpan.TryParse(DateTime.Now.ToString("zzz"), out offs);

            try
            {
                switch (_loggerSide)
                {
                    case LoggerSide.Server:
                        s_Instance = new Logger
                        {
                            m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage("debug_server.log", typeof(ExShared.Logger))
                        };
                        break;
                    case LoggerSide.Client:
                        s_Instance = new Logger
                        {
                            m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage("debug_client.log", typeof(ExShared.Logger))
                        };
                        break;
                    default:
                        s_Instance = new Logger
                        {
                            m_CustomTextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomLogFilename, typeof(ExShared.Logger))
                        };
                        break;
                }
            }
            catch (Exception _e)
            {
                MyLog.Default.WriteLine(c_LogPrefix + "   Problem encountered while initializing logger: " + _e.Message);
                //throw _e;
                return false;
            }
            //s_Instance.m_LocalUtcOffset = new MyTimeSpan(offs.Ticks);
            
            MyLog.Default.WriteLine(c_LogPrefix + "   Logger init done");
            Log(">>> Log Begin <<<");

            return true;
        }

        public static bool InitCustom(string _customLogFilename = "debug_custom.log")
        {
            CustomLogFilename = _customLogFilename;

            if (s_Instance == null)
            {
                return Init(LoggerSide.Custom);
            }
            if (s_Instance.m_CustomTextWriter != null)
                return false;

            Log(">>> Initializing Custom Logger...");
            try
            {
                s_Instance.m_CustomTextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(CustomLogFilename, typeof(ExShared.Logger));
            }
            catch (Exception _e)
            {
                Log(">>>   Problem encountered while initializing logger: " + _e.Message);
                return false;
            }
            Log(">>>   Custom Logger init done");
            Log(">>> Log begin <<<", 0, true);

            return true;
        }

        public static bool DeInit()
        {
            MyLog.Default.WriteLine(c_LogPrefix + " Logger.DeInit() called");
            if (s_Instance == null)
            {
                MyLog.Default.WriteLine(c_LogPrefix + "   Logger instance is null, there is no need to deinit");
                return true;
            }

            if (s_Instance.m_CustomTextWriter != null)
            {
                Log(">>> Log End <<<", 0, true);
                s_Instance.m_CustomTextWriter.Close();
            }

            Log(">>> Custom Logger deinit done");
            Log(">>> Log End <<<");

            if (s_Instance.m_TextWriter != null)
                s_Instance.m_TextWriter.Close();
            s_Instance = null;
            MyLog.Default.WriteLine(c_LogPrefix + "   Logger deinit done");

            return true;
        }

        public static void SetLogLevel(int _level)
        {
            if (s_Instance == null)
                InitCustom();

            s_Instance.m_LogLevel = _level;
        }

        public static void SuppressLogger(bool _enable)
        {
            if (s_Instance == null)
                InitCustom();

            s_Instance.m_IsSuppressed = _enable;
        }

        public static void Log(string _message, int _level = 0, bool _writeToCustomStream = false)
        {
            if (s_Instance == null)
                InitCustom();

            if (s_Instance.m_IsSuppressed)
                return;
         
            // custom logging bypass all log level settings;
            if (_writeToCustomStream)
            {
                if (s_Instance.m_CustomTextWriter == null)
                    InitCustom();

                try
                {
                    s_Instance.m_CustomTextWriter.WriteLine("[{0:0}]: {1:0}", s_Instance.GetDateTimeAsString(), _message);
                    s_Instance.m_CustomTextWriter.Flush();
                }
                catch (Exception _e)
                { }

                return;
            }

            if (_level > s_Instance.m_LogLevel)
                return;
            
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
            DateTime datetime = DateTime.Now;
            //DateTime datetime = DateTime.UtcNow + m_LocalUtcOffset.TimeSpan;
            return datetime.ToString("yy.MM.dd HH:mm:ss.ff");
        }
    }
}
