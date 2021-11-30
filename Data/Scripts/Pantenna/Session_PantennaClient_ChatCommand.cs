// ;
using ExSharedCore;
using Sandbox.ModAPI;
using System;

namespace Pantenna
{
    public partial class Session_PantennaClient
    {
        private bool ProcessCommands(string _commands)
        {
            string[] commands = _commands.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (commands.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowNotification("[Pantenna] You didn't specify any command", 2000);
                return false;
            }

            for (int i = 1; i < commands.Length; ++i)
            {
                string cmd = commands[i].Trim();
                ProcessSingleCommand(cmd);
            }

            return true;
        }

        private bool ProcessSingleCommand(string _command)
        {
            ClientConfig config = ConfigManager.ClientConfig;

            if (_command == "Enable")
            {
                Logger.Log("  Execute Enable command");
                config.ModEnabled = true;
                MyAPIGateway.Utilities.ShowNotification("[Pantenna] Mod enabled (effective this session only)", 3000);
                return true;
            }

            if (_command.StartsWith("ShowPanel"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(10), out flag))
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute ShowPanel command with argument '" + flag + "'");
                config.ShowPanel = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("ShowPanelBG"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(12), out flag))
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute ShowPanelBG command with argument '" + flag + "'");
                config.ShowPanelBackground = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("MinimalMode"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(12), out flag))
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute MinimalMode command with argument '" + flag + "'");
                config.ShowSignalName = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("Scale"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                float scale = 1.0f;
                if (!float.TryParse(_command.Substring(6), out scale) || scale < 0.0f)
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute Scale command with argument '" + scale + "'");
                config.ItemScale = scale;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("PanelPos"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }
                if (!_command.Contains(","))
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                //int index0 = _command.IndexOf('=');
                int index1 = _command.IndexOf(',');

                float x = 0.0f;
                float y = 0.0f;
                if (!float.TryParse(_command.Substring(9, index1 - 9), out x) || !float.TryParse(_command.Substring(index1 + 1), out y)
                    || (x < 0.0f || y < 0.0f))
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute PanelPos command with arguments (" + x + ", " + y + ")");
                config.PanelPosition = new VRageMath.Vector2D(x, y);
                UpdatePanelConfig();
                return true;
            }
            
            if (_command.StartsWith("PanelWidth"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                float w = 0.0f;
                if (!float.TryParse(_command.Substring(11), out w) || w < 0.0f)
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute PanelWidth command with arguments '" + w + "'");
                config.PanelWidth = w;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("RadarRange"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("  No argument");
                    return false;
                }

                float range = 0.0f;
                if (!float.TryParse(_command.Substring(11), out range) || range < 0.0f)
                {
                    Logger.Log("  Bad argument");
                    return false;
                }

                Logger.Log("  Execute RadarRange command with arguments '" + range + "'");
                config.RadarMaxRange = range;
                return true;
            }

            if (_command == "ReloadCfg")
            {
                Logger.Log("  Executing reload command");
                if (ConfigManager.ClientConfig.LoadConfigFile())
                {
                    MyAPIGateway.Utilities.ShowNotification("[Pantenna] Config reloaded", 3000);
                    m_RadarPanel.UpdatePanelConfig();
                    m_IsHudDirty = true;
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[Pantenna] Config reload failed", 3000);
                }
                return true;
            }
            
            #region Debug
            if (_command == "LoadedCfg")
            {
                Logger.Log("  Executing LoadedCfg command");
                string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ClientConfig);
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Loaded Configs",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }

            if (_command == "PeekCfg")
            {
                Logger.Log("  Executing PeekCfg command");
                //MyAPIGateway.Utilities.ShowNotification("[Pantenna] PeekCfg Command", 3000);
                string configs = ConfigManager.ClientConfig.PeekConfigFile();
                MyAPIGateway.Utilities.ShowMissionScreen(
                    screenTitle: "Raw Config File",
                    currentObjectivePrefix: "",
                    currentObjective: "ClientConfig.xml",
                    screenDescription: configs,
                    okButtonCaption: "Close"
                );
                return true;
            }
            #endregion

            MyAPIGateway.Utilities.ShowNotification("[Pantenna] Unknown Command [" + _command + "]", 3000);
            Logger.Log("Unknown argument [" + _command + "]");
            return false;


            //else if (arg.StartsWith("opacity="))
            //{
            //    float.TryParse(arg.Remove(0, 8), out magicNum);
            //    MyAPIGateway.Utilities.ShowNotification("[Pantenna] Opacity coeff. changed to " + magicNum, 3000);
            //    UpdateHudConfigs();
            //}
        }
    }
}
