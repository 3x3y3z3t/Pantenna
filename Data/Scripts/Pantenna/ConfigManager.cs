// ;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pantenna
{
    internal class Constants
    {

        public const ushort MSG_HANDLER_ID_SYNC = 1249;
        public const ushort MSG_HANDLER_ID_INITIAL_SYNC = 1250;

        public const int WAIT_TICKS_SERVER = 30; // Server will update 2ups;
        public const int WAIT_TICKS_CLIENT = 6; // Client will update 10ups;






        public const string MSG_REQ_INITIAL_VAL = "REQ_SHIELD_INITIAL_VAL";

        public const string SAVEDATA_FILENAME = "PlayerShieldData.xml";

        #region Server/Client Updates Config
        public const int SERVER_UPDATE_INTERVAL = 120;
        public const int SHIELD_UPDATE_INTERVAL = 60;

        public const int CLIENT_UPDATE_INTERVAL = 120;
        public const int SHIELD_FAKE_UPDATE_INTERVAL = 5;
        #endregion

        #region ShieldEmitter Config
        public const float BASIC_SHIELD_ENERGY = 1000.0f;
        public const float BASIC_DEFENSE = 0.25f;
        public const float BASIC_BULLET_RESISTANCE = 0.0f;
        public const float BASIC_EXPLOSION_RESISTANCE = 0.0f;
        public const float BASIC_POWER_CONSUMPTION = 0.001f;
        public const float BASIC_CHARGE_RATE = 10.0f;
        public const float BASIC_CHARGE_DELAY = 0.0f;
        public const float BASIC_OVERCHARGE_DURATION = 3.0f;
        public const float BASIC_OVERCHARGE_DEFENSE_BONUS = 1.0f;
        public const float BASIC_OVERCHARGE_RESISTANCE_BONUS = 0.5f;
        public const uint BASIC_MAX_PLUGINS_COUNT = 0;

        public const float ADVANCED_SHIELD_ENERGY = 10000.0f;
        public const float ADVANCED_DEFENSE = 0.75f;
        public const float ADVANCED_BULLET_RESISTANCE = 0.0f;
        public const float ADVANCED_EXPLOSION_RESISTANCE = 0.0f;
        public const float ADVANCED_POWER_CONSUMPTION = 0.00f;
        public const float ADVANCED_CHARGE_RATE = 100.0f;
        public const float ADVANCED_CHARGE_DELAY = 0.0f;
        public const float ADVANCED_OVERCHARGE_DURATION = 5.0f;
        public const float ADVANCED_OVERCHARGE_DEFENSE_BONUS = 0.5f;
        public const float ADVANCED_OVERCHARGE_RESISTANCE_BONUS = 0.5f;
        public const uint ADVANCED_MAX_PLUGINS_COUNT = 8;

        public const float PLUGIN_ENERGY_BONUS = 0.1f;
        public const float PLUGIN_DEFENSE_BONUS = 0.5f;
        public const float PLUGIN_BULLET_RES_BONUS = 0.1f;
        public const float PLUGIN_EXPLOSION_RES_BONUS = 0.1f;
        public const float PLUGIN_POWER_CONSUMPTION = 1.2f;
        #endregion

        #region Hud Config
        public const uint MAX_HUD_ICONS = 8;

        public const int SHIELD_CAP_BONUS_ICON_POS = 0;
        public const int SHIELD_CHARGE_RATE_ICON_POS = -1;
        #endregion




    }

    public class ServerConfig
{

    public string ConfigVersion { get; internal set; }

    #region Server/Client Updates Config
    public int ServerUpdateInterval { get; internal set; }
    #endregion

    #region ShieldEmitter Config
    #endregion

    private static ServerConfig s_Instance = null;

    static ServerConfig()
    { }

    public static ServerConfig GetInstance()
    {
        if (s_Instance == null)
        {
            Init();
        }

        return s_Instance;
    }

    /// Loads configs from file and initialize configs.
    public static bool Init()
    {
        const string filename = "ServerConfigs.xml";
        if (MyAPIGateway.Utilities.FileExistsInWorldStorage(filename, typeof(ServerConfig)))
        {
            // load
            try
            {
                TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(filename, typeof(ServerConfig));
                string data = reader.ReadToEnd();
                reader.Close();

                s_Instance = MyAPIGateway.Utilities.SerializeFromXML<ServerConfig>(data);

                return true;
            }
            catch (Exception _e)
            { }
        }

        // init with default;
        s_Instance = new ServerConfig();

        #region Server/Client Updates Config
        s_Instance.ServerUpdateInterval = Constants.SERVER_UPDATE_INTERVAL;
        #endregion








        //save;
        try
        {
            string data = MyAPIGateway.Utilities.SerializeToXML(s_Instance);

            TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(filename, typeof(ServerConfig));
            writer.Write(data);
            writer.Flush();
            writer.Close();
        }
        catch (Exception _e)
        { }

        return true;
    }
}

    class ConfigManager
    {
    }
}
