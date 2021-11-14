﻿// ;
using Draygo.API;
using ExSharedCore;
using Sandbox.Game.Entities;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Groups;
using VRage.Utils;
using VRageMath;

using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Pantenna
{
    internal class SignalComparer : IComparer<SignalData>
    {
        public int Compare(SignalData _x, SignalData _y)
        {
            // sort by distance, nearer signal will be higher;
            if (_x.Distance < _y.Distance)
                return 1;
            if (_x.Distance > _y.Distance)
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
        }
    }

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class Session_PantennaClientV2 : MySessionComponentBase
    {
        public bool IsServer { get; private set; }
        public bool IsDedicated { get; private set; }
        public bool IsSetupDone { get; private set; }

        private List<SignalData> m_Signals = null; /* This keeps track of all non-relayed visible enemy signals. */
        private List<SignalData> m_Signals_Last = null; /* This keeps track of all non-relayed visible enemy signals IN PREVIOUS UPDATE. */
        private List<ItemCard> m_ItemCards = null; /* This keeps track of all 5 displayed item cards. */

        private HudAPIv2.BillBoardHUDMessage m_Background = null;
        
        private HudAPIv2 m_TextHudAPI = null;
        private bool IsTextHudApiInitDone = false;
        private bool m_IsHudDirty = false;

        private bool m_IsHudVisible = false;
        private float m_HudBGOpacity = 1.0f;
        private Color m_HudBGColor = Color.White;
        private static float magicNum = 0.75f;

        private int m_Ticks = 0;

        public override void LoadData()
        {
            ConfigManager.ForceInit();

            //m_Signals = new SortedSet<SignalData>(new SignalComparer());
            m_Signals = new List<SignalData>();
            m_Signals_Last = new List<SignalData>();
            m_ItemCards = new List<ItemCard>();

            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
            MyAPIGateway.Gui.GuiControlRemoved += Gui_GuiControlRemoved;
        }

        protected override void UnloadData()
        {
            Shutdown();
            MyAPIGateway.Gui.GuiControlRemoved -= Gui_GuiControlRemoved;
            MyAPIGateway.Utilities.MessageEntered -= Utilities_MessageEntered;

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

                if (!IsTextHudApiInitDone)
                {
                    InitTextHud();
                    return;
                }
                
                Logger.Log("Doing Main Job...");
                DoMainJob();

            }

            // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L52...
            // Stealing In Progress...
            if (MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.TOGGLE_HUD) ||
               MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.PAUSE_GAME))
            {
                // 0 = Off, 1 = Hints, 2 = Basic;
                if (MyAPIGateway.Session.Config != null)
                {
                    m_IsHudVisible = MyAPIGateway.Session.Config.HudState != 0;
                }
                m_IsHudDirty = true;
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
            Logger.Log(">>> Ultilities_MessageEntered triggered <<<");
            if (MyAPIGateway.Session.Player == null)
                return;

            const string prefix = "/Pantenna";
            if (!_messageText.StartsWith(prefix))
                return;

            Logger.Log("  Command captured: " + _messageText);
            string[] arguments = _messageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (arguments.Length <= 1)
            {
                MyAPIGateway.Utilities.ShowNotification("[Pantenna] You didn't specify any arguments", 3000);
                return;
            }

            for (int i = 1; i < arguments.Length; ++i)
            {
                string arg = arguments[i].Trim();
                if (IsArgumentValid(arg))
                {
                    Logger.Log("    Argument " + i + " (valid):   " + arg);
                }
                else
                {
                    Logger.Log("    Argument " + i + " (invalid): " + arg);
                }
            }

            // TODO: clean up this mess;
            for (int i = 1; i < arguments.Length; ++i)
            {
                string arg = arguments[i].Trim();

                if (arg == "ReloadCfg")
                {
                    Logger.Log("  Executing reload command");
                    if (ConfigManager.ClientConfig.LoadConfigFile())
                    {
                        MyAPIGateway.Utilities.ShowNotification("[Pantenna] Config reloaded", 3000);
                        UpdateTextHudPosition();
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowNotification("[Pantenna] Config reload failed", 3000);
                    }
                }
                else if (arg == "LoadedCfg")
                {
                    Logger.Log("  Executing LoadedCfg command");
                    //MyAPIGateway.Utilities.ShowNotification("[Pantenna] LoadedCfg Command", 3000);
                    string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ClientConfig);
                    MyAPIGateway.Utilities.ShowMissionScreen(
                        screenTitle: "Loaded Configs",
                        currentObjectivePrefix: "",
                        currentObjective: "ClientConfig.xml",
                        screenDescription: configs,
                        okButtonCaption: "Close"
                    );
                }
                else if (arg == "PeekCfg")
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
                }
                else if (arg.StartsWith("opacity="))
                {
                    float.TryParse(arg.Remove(0, 8), out magicNum);
                    MyAPIGateway.Utilities.ShowNotification("[Pantenna] Opacity coeff. changed to " + magicNum, 3000);
                    UpdateHudConfigs();
                }
                else
                {
                    MyAPIGateway.Utilities.ShowNotification("[Pantenna] Unknown Command [" + arg + "] (argument " + i + ")", 3000);
                    Logger.Log("Unknown argument [" + arg + "] (argument " + i + ")");
                }
            }
            
            MyAPIGateway.Utilities.SendMessage("[chat] Sent Command: " + _messageText);
            
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

        public void Setup()
        {
            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (IsDedicated)
                return;

            //m_Logger.Log("  Initializing Text HUD API v2...");
            m_TextHudAPI = new HudAPIv2();
            
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
            m_Signals.AddRange(m_Signals);
            m_Signals.Clear();

            if (MyAPIGateway.Session.Player == null)
                return false;

            Logger.Log("  Getting Player...");
            IMyPlayer player = MyAPIGateway.Session.Player;
            if (player == null)
            {
                Logger.Log("    You don't have a Player.");
                return false;
            }

            Logger.Log("  Getting Character...");
            IMyCharacter character = player.Character;
            if (character == null)
            {
                Logger.Log("    Your Player <" + player.SteamUserId + "> doesn't have a Character");
                return false;
            }

            Logger.Log("  Getting Character signal reciever...");
            MyDataReceiver receiver = player.Character.Components.Get<MyDataReceiver>();
            if (receiver == null)
            {
                Logger.Log("    Your Character [" + player.Character + "] doesn't have receiver.");
                //SignalData data = new SignalData(0L, SignalType.Unknown, SignalRelation.Unknown, -1.0f, 0.0f, "Error! Character doesn't have receiver.");
                //m_Signals.Add(data);
                //m_IsHudDirty = true;
                return false;
            }

            foreach (MyDataBroadcaster broadcaster in receiver.BroadcastersInRange)
            {
                double distance = Vector3D.Distance(player.Character.GetPosition(), broadcaster.BroadcastPosition);

                if (broadcaster.Entity == null)
                {
                    Logger.Log("  (" + distance + "  m) Signal with no Entity");
                    continue;
                }

                string factionTag = "";

                IMyCharacter charSignal = broadcaster.Entity as IMyCharacter;
                if (charSignal != null)
                {
                    if (charSignal.EntityId == character.EntityId)
                    {
                        Logger.Log("  (" + distance + "  m) Signal is Me [" + charSignal.DisplayName + "]");
                        continue;
                    }

                    SignalRelation relation = SignalRelation.Unknown;
#if true
                    switch (player.GetRelationTo(charSignal.ControllerInfo.ControllingIdentityId))
#else
                    switch (GetRelationsBetweenPlayers(_player.IdentityId, charEnt.ControllerInfo.ControllingIdentityId))
#endif
                    {
                        case MyRelationsBetweenPlayerAndBlock.Friends:
                            relation = SignalRelation.Ally;
                            break;
                        case MyRelationsBetweenPlayerAndBlock.Neutral:
                            relation = SignalRelation.Neutral;
                            break;
                        case MyRelationsBetweenPlayerAndBlock.Enemies:
                            relation = SignalRelation.Enemy;
                            break;
                    }
                    
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(charSignal.ControllerInfo.ControllingIdentityId);
                    if (faction != null)
                    {
                        factionTag = faction.Tag + ".";
                        //Logger.Log("  Tag = " + factionTag);
                    }
                    else
                    {
                        factionTag = "";
                        Logger.Log("  Faction for <" + charSignal.ControllerInfo.ControllingIdentityId + "> is null");
                    }

                    SignalData oldData = TryGetSignal(charSignal.EntityId);
                    if (oldData.EntityId == charSignal.EntityId)
                    {
                        //m_Signals.Remove(oldData);

                        if (relation == SignalRelation.Enemy)
                        {
                            oldData.Relation = relation;
                            oldData.Distance = (float)distance;
                            oldData.Velocity = (float)distance - oldData.Distance;
                            oldData.DisplayName = charSignal.DisplayName;

                            m_Signals.Add(oldData);
                        }
                    }
                    else
                    {
                        if (relation == SignalRelation.Enemy)
                        {
                            SignalData data = new SignalData(charSignal.EntityId, SignalType.Character, relation, (float)distance, 0.0f, factionTag + charSignal.DisplayName);
                            m_Signals.Add(data);
                        }
                    }
                    Logger.Log("  (" + distance + "  m) Signal as Character [" + factionTag + charSignal.DisplayName + "] (" + relation + ")");

                    continue;
                } // endif (charEnt != null)

                {
                    MyCubeBlock block = broadcaster.Entity as MyCubeBlock;
                    if (block == null)
                    {
                        Logger.Log("  (" + distance + "  m) Signal that is not a CubeBlock: " + broadcaster.Entity.DisplayName);
                        continue;
                    }

                    MyCubeGrid grid = broadcaster.Entity.GetTopMostParent(null) as MyCubeGrid;
                    if (grid == null || grid.IsPreview)
                    {
                        Logger.Log("  (" + distance + "  m) Signal that is not a CubeGrid: " + broadcaster.Entity.DisplayName);
                        continue;
                    }

                    SignalRelation relation = SignalRelation.Unknown;
                    switch (player.GetRelationTo(block.OwnerId))
                    {
                        case MyRelationsBetweenPlayerAndBlock.Friends:
                            relation = SignalRelation.Ally;
                            break;
                        case MyRelationsBetweenPlayerAndBlock.Neutral:
                            relation = SignalRelation.Neutral;
                            break;
                        case MyRelationsBetweenPlayerAndBlock.Enemies:
                            relation = SignalRelation.Enemy;
                            break;
                    }

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

                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                    if (faction != null)
                    {
                        factionTag = faction.Tag + ".";
                        //Logger.Log("  Tag = " + factionTag);
                    }
                    else
                    {
                        Logger.Log("  Faction for <" + block.OwnerId + "> is null");
                    }
                    
                    SignalData oldData = TryGetSignal(grid.EntityId);
                    if (oldData.EntityId == grid.EntityId)
                    {
                        //m_Signals.Remove(oldData);

                        oldData.Relation = relation;
                        oldData.Distance = (float)distance;
                        oldData.Velocity = (float)distance - oldData.Distance;
                        oldData.DisplayName = block.DisplayNameText;

                        if (relation == SignalRelation.Enemy)
                        {
                            m_Signals.Add(oldData);
                        }
                    }
                    else
                    {
                        if (relation == SignalRelation.Enemy)
                        {
                            SignalData data = new SignalData(grid.EntityId, signalType, relation, (float)distance, 0.0f, factionTag + block.DisplayNameText);
                            m_Signals.Add(data);
                        }
                    }
                    
                    Logger.Log("  (" + distance + "  m) Signal as CubeGrid (" + grid.GridSizeEnum + ") [" + factionTag + block.DisplayNameText + "]");

                    continue;
                } // end 'broadcasterEntity as MyCubeGrid;
            }
            
            //m_Signals.Sort(new SignalComparer());
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

        private SignalData TryGetSignal(long _entityId)
        {
            foreach (SignalData signal in m_Signals_Last)
            {
                if (signal.EntityId == _entityId)
                    return signal;
            }

            return new SignalData();
        }

        private bool IsArgumentValid(string _argument)
        {
            if (string.IsNullOrEmpty(_argument))
                return false;

            if (_argument == "reload")
                return true;
            else if (_argument == "LoadedCfg")
                return true;
            else if (_argument == "PeekCfg")
                return true;

            return false;
        }

        private void UpdateHudConfigs()
        {
            if (MyAPIGateway.Session.Config != null)
            {
                m_HudBGOpacity = MyAPIGateway.Session.Config.HUDBkOpacity;
                m_IsHudVisible = MyAPIGateway.Session.Config.HudState != 0;
            }

            //// https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Utilities/Utils.cs#L256-L263
            //// SK: Stolen stuff
            //m_HudBGColor = Color.FromNonPremultiplied(41, 54, 62, 255) * m_HudBGOpacity * m_HudBGOpacity * 1.075f;
            m_HudBGColor = Color.White * m_HudBGOpacity * m_HudBGOpacity * magicNum;
            m_HudBGColor.A = (byte)(m_HudBGOpacity * 255.0f);

            m_IsHudDirty = true;
        }

        //public static MyRelationsBetweenPlayers GetRelationsBetweenPlayers(long _playerId1, long _playerId2)
        //{
        //    if (_playerId1 == _playerId2)
        //        return MyRelationsBetweenPlayers.Self;

        //    IMyFaction faction1 = MyAPIGateway.Session.Factions.TryGetPlayerFaction(_playerId1);
        //    IMyFaction faction2 = MyAPIGateway.Session.Factions.TryGetPlayerFaction(_playerId2);

        //    if (faction1 == null || faction2 == null)
        //        return MyRelationsBetweenPlayers.Enemies;

        //    if (faction1 == faction2)
        //        return MyRelationsBetweenPlayers.Self;

        //    if (MyAPIGateway.Session.Factions.GetRelationBetweenFactions(faction1.FactionId, faction2.FactionId) == MyRelationsBetweenFactions.Neutral)
        //        return MyRelationsBetweenPlayers.Neutral;

        //    return MyRelationsBetweenPlayers.Enemies;
        //}

        private void InitTextHud()
        {
            Logger.Log("Starting InitTextHud()");
            ClientConfig config = ConfigManager.ClientConfig;

            if (!m_TextHudAPI.Heartbeat)
            {
                Logger.Log("  Text Hud API hasn't recieved heartbeat.");
                if (m_Ticks % config.ClientUpdateInterval == 0)
                {
                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", (config.ClientUpdateInterval * (int)(100.0f / 6.0f)), MyFontEnum.Red);
                    Logger.Log("  Text Hud API mod is missing.");
                }

                return;
            }

            m_Background = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_BG"),
                Origin = config.PanelPosition,
                Offset = new Vector2D(0.0, 0.0),
                Width = (float)config.PanelSize.X,
                Height = (float)config.PanelSize.Y,
                uvEnabled = true,
                uvSize = new Vector2(1.0f, 1.0f),
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Visible = false,
                BillBoardColor = m_HudBGColor,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };

            Vector2D cursorPos = config.PanelPosition + config.Padding;
            Color color = Color.Darken(Color.FromNonPremultiplied(218, 62, 62, 255), 0.2);
            //Color color = new Color(218, 62, 62);
            //Color color = Color.White;

            ItemCard item0 = new ItemCard(cursorPos, color);
            ItemCard item1 = new ItemCard(item0.NextItemPosition, color);
            ItemCard item2 = new ItemCard(item1.NextItemPosition, color);
            ItemCard item3 = new ItemCard(item2.NextItemPosition, color);
            ItemCard item4 = new ItemCard(item3.NextItemPosition, color);

            m_ItemCards.Add(item0);
            m_ItemCards.Add(item1);
            m_ItemCards.Add(item2);
            m_ItemCards.Add(item3);
            m_ItemCards.Add(item4);

            m_IsHudDirty = true;
            IsTextHudApiInitDone = true;
            Logger.Log("InitTextHud() done");
        }

        private void UpdateTextHud()
        {
            Logger.Log("Starting UpdateTextHud()");
            if (!IsTextHudApiInitDone)
                return;

            ClientConfig config = ConfigManager.ClientConfig;

            m_Background.Visible = m_IsHudVisible;
            m_Background.BillBoardColor = m_HudBGColor;

            //Logger.Log("  Signal count: " + m_Signals.Count);
            //for (int i = 0; i < config.DisplayItemsCount; ++i)
            for (int i = 0; i < 5; ++i)
            {
                if (i >= m_Signals.Count)
                    break;

                SignalData signal = m_Signals[i];
                ItemCard item = m_ItemCards[i];

                item.Visible = m_IsHudVisible;
                item.UpdateItemCard(signal);
            }

            m_IsHudDirty = false;
            Logger.Log("UpdateTextHud() done");
        }

        private void UpdateTextHudPosition()
        {
            Logger.Log("Starting UpdateTextHudPosition()");
            if (!IsTextHudApiInitDone)
                return;

            ClientConfig config = ConfigManager.ClientConfig;

            m_Background.Origin = config.PanelPosition;
            m_Background.Width = (float)config.PanelSize.X;
            m_Background.Height = (float)config.PanelSize.Y;

            Vector2D cursorPos = config.PanelPosition + config.Padding;
            //for (int i = 0; i < config.DisplayItemsCount; ++i)
            for (int i = 0; i < 5; ++i)
            {
                ItemCard item = m_ItemCards[i];

                item.Position = cursorPos;
                cursorPos = item.NextItemPosition;


                
            }

            


            m_IsHudDirty = true;
        }

    }
}
