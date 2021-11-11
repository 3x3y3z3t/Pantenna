// ;
using ExSharedCore;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace Pantenna
{
    internal class Constants
    {
        public const ushort MSG_HANDLER_ID_SYNC = 1249;
        public const ushort MSG_HANDLER_ID_INITIAL_SYNC = 1250;

        #region Server/Client Default Config
        public const string CLIENT_CONFIG_VERSION = "1";
        public const int SERVER_UPDATE_INTERVAL = 30; // Server will update 2ups;
        public const int CLIENT_UPDATE_INTERVAL = 6; // Client will update 10ups;
        #endregion

        #region Hud Config
        public const float PANEL_POS_X = 0.0f;
        public const float PANEL_POS_Y = 0.0f;
        public const float PANEL_WIDTH = 0.0f;
        public const float PANEL_HEIGHT = 0.0f;
        public const float PADDING = 10.0f;

        public const float SHIP_ICON_OFFS_X = 0.0f;
        public const float TRAJECTORY_ICON_OFFS_X = 30.0f;
        public const float DISTANCE_ICON_OFFS_X = 60.0f;
        public const float DISPLAY_NAME_ICON_OFFS_X = 120.0f;

        public const int DISPLAY_ITEMS_COUNT = 5;
        public const float SPACE_BETWEEN_ITEMS = 5.0f;
        public const float ITEM_0_SCALE = 1.0f;
        public const float ITEM_1_SCALE = 1.0f;
        public const float ITEM_2_SCALE = 1.0f;
        public const float ITEM_3_SCALE = 1.0f;
        public const float ITEM_4_SCALE = 1.0f;
        #endregion
        



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

        protected string m_ConfigFileName = "";

        internal bool Init()
        {
            if (LoadConfigFile())
                return true;

            Logger.Log("Config file not found, init with default values");
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

        public virtual bool LoadConfigFile()
        {
            //throw new Exception("LoadConfigFile is not implemented");
            
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(m_ConfigFileName, GetType()))
            {
                Logger.Log("Config file found: " + m_ConfigFileName);
                string data = null;
                Config config = null;
                try
                {
                    Logger.Log("Reading config file...");
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(m_ConfigFileName, GetType());
                    data = reader.ReadToEnd();
                    reader.Close();
                    Logger.Log("  Read content: \n" + data);

                    Logger.Log("Deserializing data...");
                    config = DeserializeData(data);
                    Logger.Log("Invalidating data...");
                    if (!InvalidateConfig(config))
                    {
                        Logger.Log("  Invalidate failed, saving legacy data...");
                        SaveLegacyConfigFile(data);
                        Logger.Log("  Saving new config...");
                        SaveConfigFile();
                    }

                    Logger.Log("Init done");
                    return true;
                }
                catch (Exception _e)
                {
                    Logger.Log(">>> Exception <<< " + _e.Message);
                    Logger.Log("  Parse failed, saving legacy data..");
                    SaveLegacyConfigFile(data);
                }
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
                Logger.Log("    TThe fallback method failed...");
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

        #region Server/Client Updates Config
        public int ClientUpdateInterval { get; set; }
        #endregion

        #region Hud Config
        public Vector2D PanelPosition { get; set; }
        public Vector2D PanelSize { get; set; }
        public float Padding { get; set; }

        public float ShipIconOffsX { get; set; }
        public float TrajectoryIconOffsX { get; set; }
        public float DistanceIconOffsX { get; set; }
        public float DisplayNameIconOffsX { get; set; }

        public int DisplayItemsCount { get; set; }
        public float SpaceBetweenItems { get; set; }
        public float ItemScale0 { get; set; }
        public float ItemScale1 { get; set; }
        public float ItemScale2 { get; set; }
        public float ItemScale3 { get; set; }
        public float ItemScale4 { get; set; }
        #endregion
        
        public ClientConfig()
        {
            m_ConfigFileName = "ClientConfig.xml";
            InitDefault();
        }

        protected override bool InitDefault()
        {
            // init with default;
            ConfigVersion = Constants.CLIENT_CONFIG_VERSION;

            ClientUpdateInterval = Constants.CLIENT_UPDATE_INTERVAL;
            
            PanelPosition = new Vector2D(Constants.PANEL_POS_X, Constants.PANEL_POS_Y);
            PanelSize = new Vector2D(Constants.PANEL_WIDTH, Constants.PANEL_HEIGHT);
            Padding = Constants.PADDING;
            
            ShipIconOffsX = Constants.SHIP_ICON_OFFS_X;
            TrajectoryIconOffsX = Constants.TRAJECTORY_ICON_OFFS_X;
            DistanceIconOffsX = Constants.DISTANCE_ICON_OFFS_X;
            DisplayNameIconOffsX = Constants.DISPLAY_NAME_ICON_OFFS_X;
            
            DisplayItemsCount = Constants.DISPLAY_ITEMS_COUNT;
            SpaceBetweenItems = Constants.SPACE_BETWEEN_ITEMS;
            ItemScale0 = Constants.ITEM_0_SCALE;
            ItemScale1 = Constants.ITEM_1_SCALE;
            ItemScale2 = Constants.ITEM_2_SCALE;
            ItemScale3 = Constants.ITEM_3_SCALE;
            ItemScale4 = Constants.ITEM_4_SCALE;

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
            if (_config.ConfigVersion != Constants.CLIENT_CONFIG_VERSION)
            {
                // config version mismatch;
                Logger.Log("  Config version mismatch: read " + _config.ConfigVersion + ", newest version " + Constants.CLIENT_CONFIG_VERSION);

                Logger.Log("  Updating config...");
                return false;
            }

            return true;
        }
    }

}
