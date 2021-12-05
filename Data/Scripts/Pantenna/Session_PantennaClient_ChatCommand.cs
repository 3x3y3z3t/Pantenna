// ;
using ExShared;
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
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] You didn't specify any command", 2000);
                return false;
            }

            for (int i = 1; i < commands.Length; ++i)
            {
                string cmd = commands[i].Trim();
                Logger.Log("    Processing command " + i + ": " + cmd, 1);
                if (ProcessSingleCommand(cmd))
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command executed.", 2000);
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Command execution failed. See log for more info.", 2000);
                }
            }

            return true;
        }

        private bool ProcessSingleCommand(string _command)
        {
            ClientConfig config = ConfigManager.ClientConfig;

            if (_command == "Enable")
            {
                Logger.Log("      Execute Enable command", 1);
                config.ModEnabled = true;
                RefreshSettingsMenuRootItem();
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Mod enabled (effective this session only)", 2000);
                return true;
            }

            if (_command.StartsWith("ShowPanelBG"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(12), out flag))
                {
                    Logger.Log("      Bad argument", 1);
                    //Logger.Log("        expect true/false, got " + _command.Substring(12), 1);
                    return false;
                }

                Logger.Log("      Execute ShowPanelBG command with argument '" + flag + "'", 1);
                config.ShowPanelBackground = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("ShowPanel"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(10), out flag))
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                Logger.Log("      Execute ShowPanel command with argument '" + flag + "'", 1);
                config.ShowPanel = flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("MinimalMode"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                bool flag = false;
                if (!bool.TryParse(_command.Substring(12), out flag))
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                Logger.Log("      Execute MinimalMode command with argument '" + flag + "'", 1);
                config.ShowSignalName = !flag;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("Scale"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                float scale = 1.0f;
                if (!float.TryParse(_command.Substring(6), out scale) || scale < 0.0f)
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                Logger.Log("      Execute Scale command with argument '" + scale + "'", 1);
                config.ItemScale = scale;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("PanelPos"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }
                if (!_command.Contains(","))
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                //int index0 = _command.IndexOf('=');
                int index1 = _command.IndexOf(',');

                float x = 0.0f;
                float y = 0.0f;
                if (!float.TryParse(_command.Substring(9, index1 - 9), out x) || !float.TryParse(_command.Substring(index1 + 1), out y)
                    || (x < 0.0f || y < 0.0f))
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                Logger.Log("      Execute PanelPos command with arguments (" + x + ", " + y + ")", 1);
                config.PanelPosition = new VRageMath.Vector2D(x, y);
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("PanelWidth"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                float w = 0.0f;
                if (!float.TryParse(_command.Substring(11), out w) || w < 0.0f)
                {
                    Logger.Log("      Bad argument");
                    return false;
                }

                Logger.Log("      Execute PanelWidth command with arguments '" + w + "'", 1);
                config.PanelWidth = w;
                UpdatePanelConfig();
                return true;
            }

            if (_command.StartsWith("RadarRange"))
            {
                if (!_command.Contains("="))
                {
                    Logger.Log("      No argument", 1);
                    return false;
                }

                float range = 0.0f;
                if (!float.TryParse(_command.Substring(11), out range) || range < 0.0f)
                {
                    Logger.Log("      Bad argument", 1);
                    return false;
                }

                Logger.Log("      Execute RadarRange command with arguments '" + range + "'", 1);
                config.RadarMaxRange = range;
                return true;
            }

            if (_command == "ReloadCfg")
            {
                Logger.Log("      Executing reload command", 1);
                LoadConfig();
                return true;
            }

            if (_command == "SaveCfg")
            {
                Logger.Log("      Executing save command", 1);
                SaveConfig();
                return true;
            }

            #region Debug
            if (_command == "LoadedCfg")
            {
                Logger.Log("  Executing LoadedCfg command");
                string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ClientConfig);
                //configs += "\nViewport Size = " + s_ViewportSize.ToString();
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

            MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Unknown Command [" + _command + "]", 2000);
            Logger.Log("      Unknown command [" + _command + "]", 1);
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
