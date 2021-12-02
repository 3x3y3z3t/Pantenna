// ;
using ExShared;
using Sandbox.ModAPI;
using System;
using System.IO;
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
        public const bool SHOW_PANEL = true;
        public const bool SHOW_PANEL_BG = true;
        public const bool SHOW_MAX_RANGE_ICON = true;
        public const bool SHOW_SIGNAL_NAME = true;

        public const float PANEL_POS_X = 1475.0f;
        public const float PANEL_POS_Y = 590.0f;
        public const float PANEL_WIDTH = 420.0f;
        //public const float PANEL_HEIGHT = 240.0f;
        public const float PADDING = 10.0f;
        public const float SPACE_BETWEEN_ITEMS = 5.0f;
        public const int DISPLAY_ITEMS_COUNT = 5;
        public const float ITEM_SCALE = 1.0f;

        public const int TEXTURE_BLANK = 0;
        public const int TEXTURE_CHARCTER = 1;
        public const int TEXTURE_SMALL_SHIP = 2;
        public const int TEXTURE_LARGE_SHIP = 3;
        public const int TEXTURE_GO_AWAY = 4;
        public const int TEXTURE_APPROACH = 5;
        public const int TEXTURE_STAY = 6;
        public const int TEXTURE_ANTENNA = 7;
        
        /* 
        BG: 80 92 103
        FG: 187 233 246
        AnimatedSegment: 212 251 254 0.7
        */
        #endregion

        public const bool ENABLE_MOD = true;
        public const float RADAR_MAX_RANGE = 5000.0f;
        public const float TRAJECTORY_SENSITIVITY = 0.2f;

        public const float MAGIC_LABEL_HEIGHT_16 = 17.1428571408f;


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

        public override bool SaveConfigFile()
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
                Logger.Log("    Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CLIENT_CONFIG_VERSION);

                //Logger.Log("  Updating config...");
                // TODO: Updating new config here;
                return false;
            }

            ConfigVersion = config.ConfigVersion;
            return true;
            
        }
    }

}
