// ;
using ExSharedCore;
using Sandbox.ModAPI;
using System;
using System.IO;
using System.Xml.Serialization;
using VRageMath;

namespace Pantenna
{
    internal class Constants
    {
        #region Server/Client Default Config
        public const string CLIENT_CONFIG_VERSION = "6";
        public const int CLIENT_LOG_LEVEL = 1;
        public const int CLIENT_UPDATE_INTERVAL = 6; // Client will update 10ups;
        #endregion

        #region Hud Config
        public const bool ALWAYS_SHOW_PANEL = false;
        public const bool SHOW_PANEL = true;
        public const bool SHOW_PANEL_BG = true;

        public const float PANEL_POS_X = 0.0f;
        public const float PANEL_POS_Y = 0.0f;
        //public const float PANEL_WIDTH = 420.0f;
        //public const float PANEL_HEIGHT = 240.0f;

        public const float ITEM_WIDTH_HEIGHT = 32.0f;

        public const float PADDING = 10.0f;
        public const float SPACE_BETWEEN_ITEMS = 5.0f;
        public const int DISPLAY_ITEMS_COUNT = 5;
        public const float ITEM_SCALE = 1.0f;

        public const int TEXTURE_BLANK = 0;
        public const int TEXTURE_BASIC_SHIELD_ICON = 0;
        public const int TEXTURE_ADVANCED_SHIELD_ICON = 0;
        public const int TEXTURE_SHIELD_DEFENSE_ICON = 0;
        public const int TEXTURE_BULLET_RESIST_ICON = 0;
        public const int TEXTURE_EXPLO_RESIST_ICON = 0;
        public const int TEXTURE_UNIVERSAL_RESIST_ICON = 0;
        public const int TEXTURE_RECHARGE_ICON = 0;
        public const int TEXTURE_POWER_COST_ICON = 0;
        public const int TEXTURE_OVERCHARGE_ICON = 0;
        
        /* 
        BG: 80 92 103
        FG: 187 233 246
        AnimatedSegment: 212 251 254 0.7
        */
        #endregion

        public const bool ENABLE_MOD = true;



    }

    public class ConfigManager
    {
        public static ClientConfig ClientConfig
        {
            get
            {
                if (s_ClientConfig == null)
                {
                    s_ClientConfig = new ClientConfig();
                    s_ClientConfig.Init();
                }

                return s_ClientConfig;
            }
        }

        public static void ForceInit()
        {
            if (s_ClientConfig == null)
            {
                s_ClientConfig = new ClientConfig();
                s_ClientConfig.Init();
            }
        }

        private static ClientConfig s_ClientConfig = null;
    }

    public class Config
    {
        public string ConfigVersion { get; set; }
        public int LogLevel { get; set; }

        protected string m_ConfigFileName = "";

        internal bool Init()
        {
            Logger.Log("Loading config file...");
            if (LoadConfigFile())
            {
                Logger.Log("  Init done");
                return true;
            }

            Logger.Log("Failed to load config file, init with default values");
            InitDefault();
            Logger.Log("  Saving config...");
            SaveConfigFile();

            Logger.Log("Init done");
            return true;
        }

        protected virtual bool InitDefault()
        {
            throw new Exception("InitDefault is not implemented");
        }

        /// <summary> Read the whole Config file. </summary>
        /// <returns> read config file content as string; or null if file is not existed or read operation fails.</returns>
        public virtual string PeekConfigFile()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(m_ConfigFileName, GetType()))
            {
                Logger.Log("  Config file found: " + m_ConfigFileName);
                try
                {
                    Logger.Log("  Reading config file...");
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(m_ConfigFileName, GetType());
                    string data = reader.ReadToEnd();
                    reader.Close();
                    Logger.Log("    Read content: \n" + data, 5);

                    return data;
                }
                catch (Exception _e)
                {
                    Logger.Log("  >>> Exception <<< " + _e.Message);
                }
            }

            return null;
        }

        public virtual bool LoadConfigFile()
        {
            string data = PeekConfigFile();
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }

            Config config = null;
            try
            {
                Logger.Log("  Deserializing data...");
                config = DeserializeData(data);
                Logger.Log("  Invalidating data...");
                if (!InvalidateConfig(config))
                {
                    Logger.Log("    Invalidate failed, saving legacy data...");
                    SaveLegacyConfigFile(data);
                    Logger.Log("    Saving new config...");
                    SaveConfigFile();
                }

                return true;
            }
            catch (Exception _e)
            {
                Logger.Log("  >>> Exception <<< " + _e.Message);
                Logger.Log("    Parse failed, saving legacy data..");
                SaveLegacyConfigFile(data);
            }

            return false;
        }

        protected virtual bool SaveConfigFile()
        {
            throw new Exception("SaveConfigFile is not implemented");
        }

        protected virtual bool SaveLegacyConfigFile(string _data)
        {
            try
            {
                string filename = m_ConfigFileName.Insert(m_ConfigFileName.Length - 4, "_legacy");
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, GetType());
                writer.Write(_data);
                writer.Flush();
                writer.Close();
                return true;
            }
            catch (Exception _e)
            {
                Logger.Log("    Fallback method failed...");
            }
            return false;
        }

        protected virtual Config DeserializeData(string _data)
        {
            throw new Exception("DeserializeData is not implemented");
        }

        protected virtual bool InvalidateConfig(Config _config)
        {
            throw new Exception("InvalidateConfig is not implemented");
        }


    }

    public class ClientConfig : Config
    {
        public int ClientUpdateInterval { get; set; }
        
        public bool ShowPanel { get; set; }
        public bool ShowPanelBackground { get; set; }
        public bool ShowMaxRangeIcon { get; set; }
        public bool ShowSignalName { get; set; }

        public Vector2D PanelPosition { get; set; }
        public float PanelWidth { get; set; }
        public float Padding { get; set; }
        public float SpaceBetweenItems { get; set; }
        public int DisplayItemsCount { get; set; }
        public float ItemScale { get; set; }
        
        public bool ModEnabled { get; set; }
        public float RadarMaxRange { get; set; }
        public float TrajectorySensitivity { get; set; }

        public ClientConfig()
        {
            m_ConfigFileName = "ClientConfig.xml";
            InitDefault();
        }

        protected override bool InitDefault()
        {
            // init with default;
            ConfigVersion = Constants.CLIENT_CONFIG_VERSION;
            LogLevel = Constants.CLIENT_LOG_LEVEL;

            ClientUpdateInterval = Constants.CLIENT_UPDATE_INTERVAL;

            ShowPanel = Constants.SHOW_PANEL;
            ShowPanelBackground = Constants.SHOW_PANEL_BG;
            ShowMaxRangeIcon = Constants.SHOW_MAX_RANGE_ICON;
            ShowSignalName = Constants.SHOW_SIGNAL_NAME;

            PanelPosition = new Vector2D(Constants.PANEL_POS_X, Constants.PANEL_POS_Y);
            PanelWidth = Constants.PANEL_WIDTH;
            Padding = Constants.PADDING;
            SpaceBetweenItems = Constants.SPACE_BETWEEN_ITEMS;
            DisplayItemsCount = Constants.DISPLAY_ITEMS_COUNT;
            ItemScale = Constants.ITEM_SCALE;

            ModEnabled = Constants.ENABLE_MOD;
            RadarMaxRange = Constants.RADAR_MAX_RANGE;
            TrajectorySensitivity = Constants.TRAJECTORY_SENSITIVITY;

            return true;
        }

        protected override bool SaveConfigFile()
        {
            try
            {
                string data = MyAPIGateway.Utilities.SerializeToXML(this);
                TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(m_ConfigFileName, GetType());
                writer.Write(data);
                writer.Flush();
                writer.Close();

                return true;
            }
            catch (Exception _e)
            {
                Logger.Log("    Failed to save config file");
                Logger.Log(_e.Message);
            }

            return false;
        }

        protected override Config DeserializeData(string _data)
        {
            ClientConfig config = MyAPIGateway.Utilities.SerializeFromXML<ClientConfig>(_data);
            return config;
        }

        protected override bool InvalidateConfig(Config _config)
        {
            bool versionMatch = (_config.ConfigVersion == Constants.CLIENT_CONFIG_VERSION);
                
            ClientConfig config = _config as ClientConfig;
            if (config == null)
            {
                Logger.Log("  This Config is not ClientConfig (this should not happen)");
                return false;
            }

            LogLevel = config.LogLevel;

            ClientUpdateInterval = config.ClientUpdateInterval;

            ShowPanel = config.ShowPanel;
            ShowPanelBackground = config.ShowPanelBackground;
            ShowMaxRangeIcon = config.ShowMaxRangeIcon;
            ShowSignalName = config.ShowSignalName;

            PanelPosition = config.PanelPosition;
            PanelWidth = config.PanelWidth;
            Padding = config.Padding;
            SpaceBetweenItems = config.SpaceBetweenItems;
            DisplayItemsCount = config.DisplayItemsCount;
            ItemScale = config.ItemScale;

            ModEnabled = config.ModEnabled;
            RadarMaxRange = config.RadarMaxRange;
            TrajectorySensitivity = config.TrajectorySensitivity;

            if (PanelWidth < 0.0f)
                PanelWidth = 0.0f;
            if (DisplayItemsCount < 1)
                DisplayItemsCount = 1;
            if (DisplayItemsCount > 5)
                DisplayItemsCount = 5;
            if (ItemScale < 0.0f)
                ItemScale = 0.0f;

            if (!versionMatch)
            {
                // config version mismatch;
                Logger.Log("  Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CLIENT_CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;
            
        }
    }

}
