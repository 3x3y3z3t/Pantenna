// ;
using Draygo.API;
using ExShared;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace Pantenna
{
    //internal class SignalComparer : IComparer<SignalData>
    //{
    //    public int Compare(SignalData _x, SignalData _y)
    //    {
    //        // sort by distance, nearer signal will be higher;
    //        if (_x.Distance < _y.Distance)
    //            return 1;
    //        if (_x.Distance > _y.Distance)
    //            return -1;

    //        // sort by display name, alphabetically;
    //        int diff = _x.DisplayName.CompareTo(_y.DisplayName);
    //        if (diff > 0)
    //            return 1;
    //        if (diff < 0)
    //            return -1;

    //        // sort by entity id;
    //        if (_x.EntityId > _y.EntityId)
    //            return 1;
    //        if (_x.EntityId < _y.EntityId)
    //            return -1;

    //        // same object, this should not happen;
    //        return 0;
    //    }
    //}

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public partial class Session_PantennaClient : MySessionComponentBase
    {
        public bool IsServer { get; private set; }
        public bool IsDedicated { get; private set; }
        public bool IsSetupDone { get; private set; }

        private List<SignalData> m_Signals = null; /* This keeps track of all non-relayed visible enemy signals. */
        private List<SignalData> m_Signals_Last = null; /* This keeps track of all non-relayed visible enemy signals IN PREVIOUS UPDATE. */

        private RadarPanel m_RadarPanel = null;
                
        private HudAPIv2 m_TextHudAPI = null;
        //private bool IsTextHudApiInitDone = false;
        private bool m_IsHudDirty = false;
        private bool m_IsTextHudModMissingConfirmed = false;

        private bool m_IsHudVisible = false;
        private float m_HudBGOpacity = 1.0f;

        private int m_Ticks = 0;

        private static Vector2 s_ViewportSize = Vector2.Zero;

        public override void LoadData()
        {
            Logger.Init(LoggerSide.Client);
            ConfigManager.ForceInit();
            
            m_Signals = new List<SignalData>();
            m_Signals_Last = new List<SignalData>();

            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            MyAPIGateway.Gui.GuiControlRemoved += Gui_GuiControlRemoved;
        }

        protected override void UnloadData()
        {
            Shutdown();
            MyAPIGateway.Gui.GuiControlRemoved -= Gui_GuiControlRemoved;
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;

            m_Signals.Clear();
            m_Signals = null;

            m_Signals_Last.Clear();
            m_Signals_Last = null;

            Logger.DeInit();
        }

        public override void UpdateAfterSimulation()
        {
            ++m_Ticks;
            // clear ticks count;
            if (m_Ticks >= 2000000000)
                m_Ticks -= 2000000000;

            ClientConfig config = ConfigManager.ClientConfig;
            if (m_Ticks % config.ClientUpdateInterval == 0)
            {
                if (MyAPIGateway.Session == null)
                    return;
                
                if (!IsSetupDone)
                {
                    Setup();
                    return;
                }

                if (!m_IsTextHudModMissingConfirmed && !m_TextHudAPI.Heartbeat && m_Ticks >= 300)
                {
                    Logger.Log("Text Hud API still hasn't recieved heartbeat.", 3);
                    //MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", (config.ClientUpdateInterval * (int)(100.0f / 6.0f)), MyFontEnum.Red);
                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", 3000, MyFontEnum.Red);
                    Logger.Log("  Text Hud API mod is missing.", 3);
                    m_IsTextHudModMissingConfirmed = true;
                }

                if (!config.ModEnabled)
                    return;

                Logger.Log("Doing Main Job...", 3);
                DoMainJob();
            }

            // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L52...
            // Stealing In Progress...
            if (MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.TOGGLE_HUD) ||
               MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.PAUSE_GAME))
            {
                UpdateHudConfigs();
            }

            if (m_IsHudDirty)
            {
                UpdateTextHud();
            }
            
            // end of method;
            return;
        }

        private void Utilities_MessageEntered(string _messageText, ref bool _sendToOthers)
        {
            Logger.Log(">>> Ultilities_MessageEntered triggered <<<", 5);
            if (MyAPIGateway.Session.Player == null)
                return;

            const string prefix = "/Pantenna";
            if (!_messageText.StartsWith(prefix))
                return;

            Logger.Log("  Chat Command captured: " + _messageText, 1);
            ProcessCommands(_messageText);
                        
            _sendToOthers = false;
        }
        
        private void Gui_GuiControlRemoved(object _obj)
        {            
            // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L58...
            // Stealing In Progress...
            try
            {
                if (_obj.ToString().EndsWith("ScreenOptionsSpace")) // closing options menu just assumes you changed something so it'll re-check config settings
                {
                    UpdateHudConfigs();
                }
            }
            catch (Exception _e)
            {
                Logger.Log(_e.Message);
            }
        }

        private void InitTextHudCallback()
        {
            Logger.Log("Starting InitTextHudCallback()", 5);
            ClientConfig config = ConfigManager.ClientConfig;
            
            m_RadarPanel = new RadarPanel()
            {
                Visible = m_IsHudVisible,
                BackgroundOpacity = m_HudBGOpacity
            };
            //m_RadarPanel.UpdatePanelConfig();
            UpdateHudConfigs();

            m_IsHudDirty = true;
            //IsTextHudApiInitDone = true;

            InitModSettingsMenu();

            Logger.Log("InitTextHudCallback() done", 5);
        }

        public void Setup()
        {
            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (IsDedicated)
                return;

            Logger.SetLogLevel(ConfigManager.ClientConfig.LogLevel);

            //m_Logger.Log("  Initializing Text HUD API v2...");
            m_TextHudAPI = new HudAPIv2(InitTextHudCallback);
            
            UpdateHudConfigs();

            Logger.Log("  IsServer = " + IsServer);
            Logger.Log("  IsDedicated = " + IsDedicated);

            Logger.Log("  Setup Done.");
            IsSetupDone = true;
        }

        private void Shutdown()
        {
            try
            {
                Logger.Log("Shutting down Text HUD API v2...");
                m_TextHudAPI.Close();
            }
            catch (Exception _e)
            { }
        }

        private bool DoMainJob()
        {
            m_Signals_Last.Clear();
            m_Signals_Last.AddRange(m_Signals);
            m_Signals.Clear();

            if (MyAPIGateway.Session.Player == null)
                return false;

            Logger.Log("  Getting Player...", 4);
            IMyPlayer player = MyAPIGateway.Session.Player;
            if (player == null)
            {
                Logger.Log("    You don't have a Player.", 4);
                return false;
            }

            Logger.Log("  Getting Character...", 4);
            IMyCharacter character = player.Character;
            if (character == null)
            {
                Logger.Log("    Your Player <" + player.SteamUserId + "> doesn't have a Character", 4);
                return false;
            }

            Logger.Log("  Getting Character signal reciever...", 4);
            MyDataReceiver receiver = character.Components.Get<MyDataReceiver>();
            if (receiver == null)
            {
                Logger.Log("    Your Character [" + character + "] doesn't have receiver.", 4);
                m_Signals.Clear();
                m_Signals_Last.Clear();
                m_IsHudDirty = true;
                return false;
            }

            if (!PopulateSignals(receiver))
            {
                // TODO: Log something here;
                return false;
            }

            m_Signals.Sort((SignalData _x, SignalData _y) =>
            {
                // sort by distance, nearer signal will be higher;
                if (_x.Distance > _y.Distance)
                    return 1;
                if (_x.Distance < _y.Distance)
                    return -1;

                // sort by display name, alphabetically;
                int diff = _x.DisplayName.CompareTo(_y.DisplayName);
                if (diff > 0)
                    return 1;
                if (diff < 0)
                    return -1;

                // sort by entity id;
                if (_x.EntityId > _y.EntityId)
                    return 1;
                if (_x.EntityId < _y.EntityId)
                    return -1;

                // same object, this should not happen;
                return 0;
            });

            m_IsHudDirty = true;

            return true;
        }

        private bool PopulateSignals(MyDataReceiver _receiver)
        {
            foreach (MyDataBroadcaster broadcaster in _receiver.BroadcastersInRange)
            {
                if (broadcaster == null)
                {
                    Logger.Log("  Signal broadcaster is null", 4);
                    continue;
                }
                if (broadcaster.Entity == null)
                {
                    Logger.Log("  Signal with no Entity", 4);
                    continue;
                }
                
                IMyPlayer player = MyAPIGateway.Session.Player;
                IMyCharacter character = player.Character;

                double distance = Vector3D.Distance(player.Character.GetPosition(), broadcaster.BroadcastPosition);
                if (distance > ConfigManager.ClientConfig.RadarMaxRange)
                {
                    Logger.Log("  (" + distance + "  m) Signal Out Of Range", 4);
                    continue;
                }

                if (TryInterpretSignalAsCharacter(broadcaster.Entity, distance))
                {
                    //Logger.Log("  (" + distance + "  m) Signal is Character [" + broadcaster.Entity.DisplayName + "]", 4);
                    continue;
                }

                if (TryInterpretSignalAsCubeGrid(broadcaster.Entity, distance))
                {
                    //Logger.Log("  (" + distance + "  m) Signal is Grid [" + broadcaster.Entity.DisplayName + "]", 4);
                    continue;
                }

                // TODO: failure (unknown signal);
                Logger.Log("  (" + distance + "  m) Unidentified Signal [" + broadcaster.Entity.DisplayName + "]", 4);
            }
            
            return true;
        }

        private bool TryInterpretSignalAsCharacter(VRage.ModAPI.IMyEntity _entity, double _distance)
        {
            if (IsSignalAdded(_entity.EntityId))
            {
                Logger.Log("  (" + _distance + "  m) Signal [" + _entity.DisplayName + "] has already been added", 4);
                return true;
            }

            IMyCharacter charSignal = _entity as IMyCharacter;
            if (charSignal == null)
            {
                return false;
            }

            if (charSignal.EntityId == MyAPIGateway.Session.Player.Character.EntityId)
            {
                Logger.Log("  (" + _distance + "  m) Signal is Me [" + charSignal.DisplayName + "]", 4); // distance should be 0;
                return true;
            }

            MyRelationsBetweenPlayerAndBlock relation = MyAPIGateway.Session.Player.GetRelationTo(charSignal.ControllerInfo.ControllingIdentityId);
            
            string displayName = "";
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(charSignal.ControllerInfo.ControllingIdentityId);
            if (faction != null)
            {
                displayName = faction.Tag + "." + charSignal.DisplayName;
            }
            else
            {
                //factionTag = "";
                displayName = charSignal.DisplayName;
                Logger.Log("  Faction for <" + charSignal.ControllerInfo.ControllingIdentityId + ">[" + charSignal.DisplayName + "] is null", 5);
            }

            float velocity = 0.0f;
            SignalData lastData = TryGetLastSignalData(charSignal.EntityId);
            if (lastData.EntityId == charSignal.EntityId)
            {
                velocity = (float)_distance - lastData.Distance;
            }

            Logger.Log("  (" + _distance + "  m) Signal is Character [" + displayName + "] (" + relation + ")", 4);
            if (relation == MyRelationsBetweenPlayerAndBlock.Enemies)
            {
                Logger.Log("  (" + _distance + "  m) Signal is Character [" + displayName + "] (" + relation + ")", 4);
                m_Signals.Add(new SignalData(charSignal.EntityId, SignalType.Character, relation, (float)_distance, velocity, displayName));
            }
            else
            {
                Logger.Log("  (" + _distance + "  m) Signal is Character [" + displayName + "] (" + relation + ") (ignored)", 4);
            }

            return true;
        }

        private bool TryInterpretSignalAsCubeGrid(VRage.ModAPI.IMyEntity _entity, double _distance)
        {
                MyCubeBlock block = _entity as MyCubeBlock;
            if (block == null)
            {
                //Logger.Log("  (" + _distance + "  m) Signal that is not a CubeBlock: " + _entity.DisplayName, 4);
                return false;
            }
                MyCubeGrid grid = _entity.GetTopMostParent(null) as MyCubeGrid;
            if (grid == null || grid.IsPreview)
            {
                //Logger.Log("  (" + _distance + "  m) Signal that is not a CubeGrid: " + _entity.DisplayName, 4);
                return false;
            }

            if (IsSignalAdded(grid.EntityId))
            {
                Logger.Log("  (" + _distance + "  m) Signal [" + block.DisplayNameText + "] belongs to grid [" + grid.DisplayNameText + "] that has already been added", 4);
                return true;
            }

            MyRelationsBetweenPlayerAndBlock relation = MyRelationsBetweenPlayerAndBlock.NoOwnership;
            if (block.OwnerId != 0)
                relation = MyAPIGateway.Session.Player.GetRelationTo(block.OwnerId);
            
            SignalType signalType = SignalType.Unknown;
            switch (grid.GridSizeEnum)
            {
                case MyCubeSize.Large:
                    signalType = SignalType.LargeGrid;
                    break;
                case MyCubeSize.Small:
                    signalType = SignalType.SmallGrid;
                    break;
            }

            string displayName = "";
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            if (faction != null)
            {
                displayName = faction.Tag + "." + block.DisplayNameText;
            }
            else
            {
                //factionTag = "";
                displayName = block.DisplayNameText;
                Logger.Log("  Faction for <" + block.OwnerId + "> is null", 5);
            }

            float velocity = 0.0f;
            SignalData lastData = TryGetLastSignalData(grid.EntityId);
            if (lastData.EntityId == grid.EntityId)
            {
                velocity = (float)_distance - lastData.Distance;
            }
            
            if (relation == MyRelationsBetweenPlayerAndBlock.Enemies)
            {
                Logger.Log("  (" + _distance + "  m) Signal is CubeGrid (" + grid.GridSizeEnum + ") [" + displayName + "] (" + relation + ")", 4);
                m_Signals.Add(new SignalData(grid.EntityId, signalType, relation, (float)_distance, velocity, displayName));
            }
            else
            {
                Logger.Log("  (" + _distance + "  m) Signal is CubeGrid (" + grid.GridSizeEnum + ") [" + displayName + "] (" + relation + ") (ignored)", 4);
            }

            return true;
        }
        
        private SignalData TryGetLastSignalData(long _entityId)
        {
            foreach (SignalData signal in m_Signals_Last)
            {
                if (signal.EntityId == _entityId)
                    return signal;
            }

            return new SignalData();
        }

        private bool IsSignalAdded(long _entityId)
        {
            foreach (SignalData signal in m_Signals)
            {
                if (signal.EntityId == _entityId)
                    return true;
            }

            return false;
        }

        private void UpdateHudConfigs()
        {
            if (MyAPIGateway.Session.Config != null)
            {
                m_HudBGOpacity = MyAPIGateway.Session.Config.HUDBkOpacity;
                m_IsHudVisible = MyAPIGateway.Session.Config.HudState != 0; // 0 = Off, 1 = Hints, 2 = Basic;
            }
            if (MyAPIGateway.Session.Camera != null)
            {
                s_ViewportSize = MyAPIGateway.Session.Camera.ViewportSize;
            }

            //// https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Utilities/Utils.cs#L256-L263
            //// SK: Stolen stuff
            //m_HudBGColor = Color.FromNonPremultiplied(41, 54, 62, 255) * m_HudBGOpacity * m_HudBGOpacity * 1.075f;
            //m_HudBGColor = Color.White * m_HudBGOpacity * m_HudBGOpacity * magicNum;
            //m_HudBGColor.A = (byte)(m_HudBGOpacity * 255.0f);
            
            UpdatePanelConfig();
        }

        private void UpdatePanelConfig()
        {
            Logger.SuppressLogger(true);
            if (ConfigManager.ClientConfig.ModEnabled)
            {
                if (m_RadarPanel != null)
                {
                    m_RadarPanel.Visible = m_IsHudVisible;
                    m_RadarPanel.BackgroundOpacity = m_HudBGOpacity;
                    m_RadarPanel.UpdatePanelConfig();
                    m_IsHudDirty = true;
                }
            }
            Logger.SuppressLogger(false);
        }
                
        private void UpdateTextHud()
        {
            ClientConfig config = ConfigManager.ClientConfig;
            if (!m_TextHudAPI.Heartbeat)
                return;
            if (!config.ShowPanel)
                return;
            
            Logger.Log("Starting UpdateTextHud()", 5);

            //Logger.Log("  Signal Count = " + m_Signals.Count);
            m_RadarPanel.UpdatePanel(m_Signals);
            
            m_IsHudDirty = false;
            Logger.Log("UpdateTextHud() done", 5);
        }


    }
}
