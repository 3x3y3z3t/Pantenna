// ;
using Draygo.API;
using ExSharedCore;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.Voxels;
using VRage.Utils;
using VRageMath;

using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Pantenna
{
    public class ItemCard
    {
        private const float ItemHeight = 32;

        public Vector2D Position { get; set; } /* Position is Top-Left. */
        public Vector2D NextItemPosition
        {
            get
            {
                return new Vector2D(Position.X, Position.Y + ItemHeight + ConfigManager.ClientConfig.SpaceBetweenItems);
            }
        }

        public Color LabelColor { get; set; }
        public bool Visible { get; set; }

        public SignalType SignalType { get; set; }
        public float RelativeVelocity { get; set; }
        public float Distance { get; set; }
        public string DisplayNameString { get; set; }

        public float ShipIconOffsX;
        public float TrajectoryIconOffsX;
        public float DistanceIconOffsX;
        public float DisplayNameIconOffsX;

        #region Internal HUD Elements
        private StringBuilder DistanceSB;
        private StringBuilder DisplayNameSB;
        
        private HudAPIv2.BillBoardHUDMessage ShipIcon;
        private HudAPIv2.BillBoardHUDMessage TrajectoryIcon;
        private HudAPIv2.HUDMessage DistanceLabel;
        private HudAPIv2.HUDMessage DisplayNameLabel;
        #endregion

        public ItemCard(Vector2D _position, Color _labelColor, SignalType _signalType = SignalType.Unknown, float _relativeVelocity = 0.0f, float _distance = 0.0f, string _displayName = "")
        {
            Position = _position;
            LabelColor = _labelColor;
            Visible = false;

            ShipIconOffsX = Constants.SHIP_ICON_OFFS_X;
            TrajectoryIconOffsX = Constants.TRAJECTORY_ICON_OFFS_X;
            DistanceIconOffsX = Constants.DISTANCE_ICON_OFFS_X;
            DisplayNameIconOffsX = Constants.DISPLAY_NAME_ICON_OFFS_X;

            SignalType = _signalType;
            RelativeVelocity = _relativeVelocity;
            Distance = _distance;
            DisplayNameString = _displayName;

            #region Internal HUD Elements Initializations
            DistanceSB = new StringBuilder(FormatDistanceAsString(_distance));
            DisplayNameSB = new StringBuilder(_displayName);

            ShipIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Default_8px"),
                Origin = Position,
                Offset = new Vector2D(ShipIconOffsX, 0.0),
                Width = ItemHeight,
                Height = ItemHeight,
                uvEnabled = true,
                uvSize = new Vector2(1.0f, 1.0f),
                uvOffset = new Vector2(1.0f, 1.0f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            TrajectoryIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Default_8px"),
                Origin = Position,
                Offset = new Vector2D(TrajectoryIconOffsX, 0.0),
                Width = ItemHeight,
                Height = ItemHeight,
                uvEnabled = true,
                uvSize = new Vector2(1.0f, 1.0f),
                uvOffset = new Vector2(1.0f, 1.0f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            DistanceLabel = new HudAPIv2.HUDMessage()
            {
                Message = DistanceSB,
                Origin = Position,
                Offset = new Vector2D(DistanceIconOffsX, 0.0),
                Font = "monospace",
                InitialColor = LabelColor,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            DisplayNameLabel = new HudAPIv2.HUDMessage()
            {
                Message = DisplayNameSB,
                Origin = Position,
                Offset = new Vector2D(DisplayNameIconOffsX, 0.0),
                Font = "monospace",
                InitialColor = LabelColor,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            #endregion
        }

        public void UpdateItemCard(SignalData _signal)
        {
            SignalType = _signal.SignalType;
            RelativeVelocity = _signal.Velocity;
            Distance = _signal.Distance;
            DisplayNameString = _signal.DisplayName;

            UpdateItemCard();
        }

        public void UpdateItemCard()
        {
            switch (SignalType)
            {
                case SignalType.LargeGrid:
                    ShipIcon.Material = MyStringId.GetOrCompute("Default_8px");
                    break;
                case SignalType.SmallGrid:
                    ShipIcon.Material = MyStringId.GetOrCompute("Default_8px");
                    break;
                case SignalType.Character:
                    ShipIcon.Material = MyStringId.GetOrCompute("Default_8px");
                    break;
                default:
                    ShipIcon.Material = MyStringId.GetOrCompute("Default_8px");
                    break;
            }
            ShipIcon.Visible = Visible;

            if (RelativeVelocity > 0.0f)
            {
                TrajectoryIcon.Material = MyStringId.GetOrCompute("Default_8px");
            }
            else if (RelativeVelocity < 0.0f)
            {
                TrajectoryIcon.Material = MyStringId.GetOrCompute("Default_8px");
            }
            else
            {
                TrajectoryIcon.Material = MyStringId.GetOrCompute("Default_8px");
            }
            TrajectoryIcon.Visible = Visible;

            DistanceSB.Clear();
            DistanceSB.Append(FormatDistanceAsString(Distance));
            DistanceLabel.Visible = Visible;

            DisplayNameSB.Clear();
            DisplayNameSB.Append(DisplayNameString);
            DisplayNameLabel.Visible = Visible;
        }

        private static string FormatDistanceAsString(float _distance)
        {
            if (_distance > 1000.0f)
            {

            }

            return string.Format("{0:F2} m", _distance);
        }
    }

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
        private List<ItemCard> m_ItemCards = null; /* This keeps track of all 5 displayed item cards. */

        private ItemCard m_Item0;
        private ItemCard m_Item1;
        private ItemCard m_Item2;
        private ItemCard m_Item3;
        private ItemCard m_Item4;

        private HudAPIv2 m_TextHudAPI = null;
        private bool IsTextHudApiInitDone = false;
        private bool m_IsHudDirty = false;
        private bool m_IsHudVisible = false;

        private int m_Ticks = 0;

        public override void LoadData()
        {
            ConfigManager.ForceInit();

            //m_Signals = new SortedSet<SignalData>(new SignalComparer());
            m_Signals = new List<SignalData>();
            m_ItemCards = new List<ItemCard>();

            MyAPIGateway.Utilities.MessageEntered += Utilities_MessageEntered;
        }

        protected override void UnloadData()
        {
            Shutdown();

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
                }

                if (!IsTextHudApiInitDone)
                {
                    InitTextHud();
                }

                // Attempt to steal from https://github.com/THDigi/BuildInfo/blob/master/Data/Scripts/BuildInfo/Systems/GameConfig.cs#L52...
                // Stealing In Progress...
                if (MyAPIGateway.Input.IsNewGameControlPressed(Sandbox.Game.MyControlsSpace.TOGGLE_HUD))
                {
                    // TODO: update hud state;
                    // 0 = Off, 1 = Hints, 2 = Basic;

                    if (MyAPIGateway.Session.Config != null)
                    {
                        m_IsHudVisible = (MyAPIGateway.Session.Config.HudState == 0);
                    }
                }

                Logger.Log("Doing Main Job...");
                Logger.Log("  (fake)");
                //DoMainJob();

                if (m_IsHudDirty)
                {
                    UpdateTextHud();
                }
            }

            // end of method;
            return;
        }

        private void Utilities_MessageEntered(string _messageText, ref bool _sendToOthers)
        {
            Logger.Log(">>> Ultilities_MessageEntered triggered <<<");
            if (MyAPIGateway.Session.Player == null)
                return;

            MyAPIGateway.Utilities.ShowNotification("Sent Message: " + _messageText, 5000);
            Logger.Log("Sent Message: " + _messageText);

            MyAPIGateway.Utilities.SendMessage("[chat] Sent Message: " + _messageText);

            string configs = MyAPIGateway.Utilities.SerializeToXML(ConfigManager.ClientConfig);

            MyAPIGateway.Utilities.ShowMissionScreen(
                screenTitle: "You Chat Something",
                currentObjectivePrefix: "",
                currentObjective: "ClientConfig.xml",
                screenDescription: "Description",
                callback: null,
                okButtonCaption: "Click me to OK"
            );






            _sendToOthers = false;
        }

        public void Setup()
        {
            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
            if (IsDedicated)
                return;

            //m_Logger.Log("  Initializing Text HUD API v2...");
            m_TextHudAPI = new HudAPIv2();

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
            //m_Signals.Clear();

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
                SignalData data = new SignalData(0L, SignalType.Unknown, SignalRelation.Unknown, -1.0f, 0.0f, "Error! Character doesn't have receiver.");
                m_Signals.Add(data);
                m_IsHudDirty = true;
                return false;
            }

            foreach (MyDataBroadcaster broadcaster in receiver.BroadcastersInRange)
            {
                double distance = Vector3D.Distance(player.GetPosition(), broadcaster.BroadcastPosition);

                MyEntity broadcasterEntity = broadcaster.Entity as MyEntity;
                if (broadcasterEntity == null)
                {
                    Logger.Log("  (" + distance + "  m) Signal with no Entity");
                    continue;
                } // endif (broadcasterEntity == null)

                IMyCharacter charEnt = broadcasterEntity as IMyCharacter;
                if (charEnt != null)
                {
                    SignalRelation relation = SignalRelation.Unknown;
#if true
                    switch (player.GetRelationTo(charEnt.ControllerInfo.ControllingIdentityId))
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

                    SignalData oldData = TryGetSignal(charEnt.EntityId);
                    if (oldData.EntityId == charEnt.EntityId)
                    {
                        m_Signals.Remove(oldData);

                        oldData.Relation = relation;
                        oldData.Distance = (float)distance;
                        oldData.Velocity = (float)distance - oldData.Distance;
                        oldData.DisplayName = charEnt.DisplayName;

                        if (relation == SignalRelation.Enemy)
                        {
                            m_Signals.Add(oldData);
                        }
                    }
                    else
                    {
                        if (relation == SignalRelation.Enemy)
                        {
                            SignalData data = new SignalData(charEnt.EntityId, SignalType.Character, relation, (float)distance, 0.0f, charEnt.DisplayName);
                            m_Signals.Add(data);
                        }
                    }
                    Logger.Log("  (" + distance + "  m) Signal as Character [" + charEnt.DisplayName + "]");

                    continue;
                } // endif (charEnt != null)

                {
                    MyCubeBlock blockEnt = broadcasterEntity as MyCubeBlock;
                    if (blockEnt == null)
                    {
                        Logger.Log("  (" + distance + "  m) Signal that is not a CubeBlock: " + broadcasterEntity.DisplayName);
                        continue;
                    }

                    SignalRelation relation = SignalRelation.Unknown;
                    switch (player.GetRelationTo(blockEnt.OwnerId))
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

                    MyCubeGrid gridEnt = broadcasterEntity.GetTopMostParent() as MyCubeGrid;
                    if (gridEnt == null || gridEnt.IsPreview)
                    {
                        Logger.Log("  (" + distance + "  m) Signal that is not a CubeGrid: " + broadcasterEntity.DisplayName);
                        continue;
                    }

                    SignalType signalType = SignalType.Unknown;
                    switch (gridEnt.GridSizeEnum)
                    {
                        case MyCubeSize.Large:
                            signalType = SignalType.LargeGrid;
                            break;
                        case MyCubeSize.Small:
                            signalType = SignalType.SmallGrid;
                            break;
                    }

                    SignalData oldData = TryGetSignal(gridEnt.EntityId);
                    if (oldData.EntityId == gridEnt.EntityId)
                    {
                        m_Signals.Remove(oldData);

                        oldData.Relation = relation;
                        oldData.Distance = (float)distance;
                        oldData.Velocity = (float)distance - oldData.Distance;
                        oldData.DisplayName = charEnt.DisplayName;

                        if (relation == SignalRelation.Enemy)
                        {
                            m_Signals.Add(oldData);
                        }
                    }
                    else
                    {
                        if (relation == SignalRelation.Enemy)
                        {
                            SignalData data = new SignalData(gridEnt.EntityId, signalType, relation, (float)distance, 0.0f, blockEnt.DisplayName);
                            m_Signals.Add(data);
                        }
                    }
                    Logger.Log("  (" + distance + "  m) Signal as CubeGrid (" + gridEnt.GridSizeEnum + ") [" + charEnt.DisplayName + "]");

                    continue;
                } // end 'broadcasterEntity as MyCubeGrid;

            }

            return true;
        }

        private SignalData TryGetSignal(long _entityId)
        {
            foreach (SignalData signal in m_Signals)
            {
                if (signal.EntityId == _entityId)
                    return signal;
            }

            return new SignalData();
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
                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", config.ClientUpdateInterval, MyFontEnum.Red);
                    Logger.Log("  Text Hud API mod is missing.");
                }

                return;
            }

            Vector2D cursorPos = config.PanelPosition + config.Padding;

            ItemCard item0 = new ItemCard(cursorPos, Color.Red);
            ItemCard item1 = new ItemCard(item0.NextItemPosition, Color.Red);
            ItemCard item2 = new ItemCard(item1.NextItemPosition, Color.Red);
            ItemCard item3 = new ItemCard(item2.NextItemPosition, Color.Red);
            ItemCard item4 = new ItemCard(item3.NextItemPosition, Color.Red);

            m_ItemCards.Add(item0);
            m_ItemCards.Add(item1);
            m_ItemCards.Add(item2);
            m_ItemCards.Add(item3);
            m_ItemCards.Add(item4);

            IsTextHudApiInitDone = true;
        }

        private void UpdateTextHud()
        {
            Logger.Log("Starting UpdateTextHud()");
            if (!IsTextHudApiInitDone)
                return;
            if (MyAPIGateway.Session.Config == null)
                return;

            ClientConfig config = ConfigManager.ClientConfig;

            //for (int i = 0; i < config.DisplayItemsCount; ++i)
            for (int i = 0; i < 5; ++i)
            {
                SignalData signal = m_Signals[i];
                ItemCard item = m_ItemCards[i];

                item.Visible = m_IsHudVisible;
                item.UpdateItemCard(signal);
            }

            m_IsHudDirty = false;
        }

    }
}
