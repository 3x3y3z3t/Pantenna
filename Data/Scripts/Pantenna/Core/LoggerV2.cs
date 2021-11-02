// ;
using Sandbox.ModAPI;
using System;
using System.IO;

namespace SharedCore
{
    public enum LoggerSide
    {
        SERVER,
        CLIENT,
    }

    class Logger
    {
        private string m_LoggerName = "";
        private uint m_LogLevel = 0;
        private LoggerSide m_LoggerSide = LoggerSide.SERVER;
        private string m_Filename = "";
        private TextWriter m_TextWriter = null;

        public Logger(string _name, LoggerSide _side)
        {
            m_LoggerName = _name;
            m_LoggerSide = _side;
        }

        public void Init(string _filename)
        {
            m_Filename = _filename;

            try
            {
                if (m_TextWriter != null)
                {
                    m_TextWriter.Close();
                }
                if (m_LoggerSide == LoggerSide.SERVER)
                {
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(_filename, typeof(SharedCore.Logger));
                    m_TextWriter.WriteLine("PocketShieldV2 LoggerV2 (Server-side) init done.");
                }
                else if (m_LoggerSide == LoggerSide.CLIENT)
                {
                    m_TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(_filename, typeof(SharedCore.Logger));
                    m_TextWriter.WriteLine("PocketShieldV2 LoggerV2 (Client-side) init done.");
                }
                //else if (m_LoggerSide == LoggerSide.SHIELD)
                //{
                //    m_TextWriter = MyAPIGateway.Utilities.WriteFileInWorldStorage(_filename, typeof(SharedCore.Logger));
                //    m_TextWriter.WriteLine("PocketShieldV2 ShieldLoggerV2 init done.");
                //}

                m_TextWriter.Flush();
            }
            catch (Exception _e)
            { }
        }

        public void DeInit()
        {
            if (m_TextWriter == null)
                return;

            try
            {
                if (m_LoggerSide == LoggerSide.SERVER)
                {
                    m_TextWriter.WriteLine("PocketShield Logger (Server-side) deinit done.");
                    m_TextWriter.Flush();
                }
                else if (m_LoggerSide == LoggerSide.CLIENT)
                {
                    m_TextWriter.WriteLine("PocketShield Logger (Client-side) deinit done.");
                    m_TextWriter.Flush();
                }
                //else if (m_LoggerSide == LoggerSide.SHIELD)
                //{
                //    m_TextWriter.WriteLine("PocketShieldV2 ShieldLoggerV2 init done.");
                //    m_TextWriter.Flush();
                //}

                m_TextWriter.Close();
                m_TextWriter = null;
            }
            catch (Exception _e)
            { }
        }

        public void Log(uint _level, string _message)
        {
            if (m_TextWriter == null)
                return;

            try
            {
                m_TextWriter.WriteLine(_message);
                m_TextWriter.Flush();
            }
            catch (Exception _e)
            { }
        }

    }
}
