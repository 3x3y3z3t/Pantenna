//// ;
//using Draygo.API;
//using Sandbox.ModAPI;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using VRage.Game;
//using VRage.Game.Components;
//using VRage.Utils;
//using VRageMath;

//using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

//namespace Pantenna
//{
//    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
//    public class Session_PocketShieldClientV2 : MySessionComponentBase
//    {

//        public bool IsServer { get; private set; }
//        public bool IsDedicated { get; private set; }
//        public bool IsSetupDone { get; private set; }

//        private PlayerShieldSyncData m_MyPlayerShieldData;

//        private HudAPIv2 m_TextHudAPI = null;
//        private bool IsTextHudApiInitDone = false;
//        private bool m_IsHudDirty = false;

//        private LoggerV2 m_Logger = null;
//        private int m_Ticks = 0;

//        public override void LoadData()
//        {
//            m_Logger = new LoggerV2(LoggerSide.CLIENT);
//            m_Logger.Init("debug_client.log");

//            m_MyPlayerShieldData = ShieldEmitterV2.GenerateSyncData(null);
//        }

//        protected override void UnloadData()
//        {
//            Shutdown();

//            m_Logger.DeInit();
//        }

//        public override void UpdateBeforeSimulation()
//        {
//            if (MyAPIGateway.Session == null)
//                return;
//            ClientConfig config = ClientConfig.GetInstance();

//            if (m_Ticks % config.ClientUpdateInterval == 0)
//            {
//                if (!IsSetupDone)
//                {
//                    Setup();
//                    if (!IsServer)
//                    {
//                        m_Logger.Log(0, "  Sending Initial Shield Value request to server.");
//                        MyAPIGateway.Multiplayer.SendMessageToServer(Constants.MSG_HANDLER_ID_INITIAL_SYNC, Encoding.Unicode.GetBytes(Constants.MSG_REQ_INITIAL_VAL));
//                    }
//                }

//                if (!IsTextHudApiInitDone)
//                {
//                    InitTextHud();
//                }
//            }

//            if (m_Ticks % config.ShieldFakeUpdateInterval == 0)
//            {
//                // "simulate" shield charging, this value will be overwritten on next sync;
//                if (m_MyPlayerShieldData.Energy < m_MyPlayerShieldData.MaxEnergy && m_MyPlayerShieldData.ChargeRate > 0.0f)
//                {
//                    m_MyPlayerShieldData.Energy += m_MyPlayerShieldData.ChargeRate / 60.0f * config.ShieldFakeUpdateInterval;
//                    m_IsHudDirty = true;
//                }

//                if (m_IsHudDirty)
//                {
//                    UpdateTextHud();
//                }
//            }

//            // clear ticks count;
//            if (m_Ticks >= 2000000000)
//                m_Ticks = 0;

//            ++m_Ticks;
//            // end of methods;
//            return;
//        }

//        public void HandleSyncShieldResponse(ushort _handlerId, byte[] _package, ulong _playerId, bool _sentMsg)
//        {
//            string decodedPackage = Encoding.Unicode.GetString(_package);
//            //m_Logger.Log("  _handlerId = " + _handlerId + ", _package = " + decodedPackage + ", _playerId = " + _playerId + ", _sentMsg = " + _sentMsg);
//            m_Logger.Log(0, "Recieved message from server <" + _playerId + ">: " + decodedPackage);
//            try
//            {
//                m_MyPlayerShieldData = MyAPIGateway.Utilities.SerializeFromXML<PlayerShieldSyncData>(decodedPackage);

//                m_IsHudDirty = true;
//                //m_Logger.Log("  Recieved PlayerShieldData for player [" + m_MyPlayerShieldData.SteamUserId + "].");

//                // do update;
//                UpdateTextHud();
//            }
//            catch (Exception _e)
//            {
//                m_Logger.Log(0, "Failed to deserialize package.");
//            }
//        }

//        public void Setup()
//        {
//            IsServer = (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer);
//            IsDedicated = IsServer && MyAPIGateway.Utilities.IsDedicated;
//            if (IsDedicated)
//                return;

//            //m_Logger.Log("  Initializing Text HUD API v2...");
//            m_TextHudAPI = new HudAPIv2();

//            //m_Logger.Log("  Registering Message Handler (id " + Constants.MSG_HANDLER_ID + ")...");
//            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Constants.MSG_HANDLER_ID_SYNC, HandleSyncShieldResponse);

//            m_Logger.Log(0, "  IsServer = " + IsServer);
//            m_Logger.Log(0, "  IsDedicated = " + IsDedicated);

//            m_Logger.Log(0, "  Setup Done.");
//            IsSetupDone = true;
//        }

//        private void Shutdown()
//        {
//            try
//            {
//                //m_Logger.Log("    Shutting down Text HUD API v2...");
//                m_TextHudAPI.Close();

//                //m_Logger.Log("    UnRegistering Message Handler (id " + Constants.MSG_HANDLER_ID + ")...");
//                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Constants.MSG_HANDLER_ID_SYNC, HandleSyncShieldResponse);
//            }
//            catch (Exception _e)
//            { }
//        }

//        #region HUD Elements
//        private HudAPIv2.BillBoardHUDMessage m_PSIconBGTop = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconBGMiddle = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconBGBottom = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconMain = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconMainOvercharge = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconBar = null;
//        private HudAPIv2.BillBoardHUDMessage m_PSIconBarBG = null;

//        private StringBuilder m_PSSBLabelMain = null;

//        private HudAPIv2.HUDMessage m_PSLabelMain = null;








//        private readonly MyStringId s_PSIconBGStringId = MyStringId.GetOrCompute("PocketShield_BG");
//        private readonly MyStringId s_PSIconShieldBarStringId = MyStringId.GetOrCompute("PocketShield_ShieldBar");
//        private readonly MyStringId s_PSIconShieldMainIconStringId = MyStringId.GetOrCompute("PocketShield_ShieldMainIcon");
//        private readonly MyStringId s_PSIconShieldBonusIcon0StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon0");
//        private readonly MyStringId s_PSIconShieldBonusIcon1StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon1");
//        private readonly MyStringId s_PSIconShieldBonusIcon2StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon2");
//        private readonly MyStringId s_PSIconShieldBonusIcon3StringId = MyStringId.GetOrCompute("PocketShield_ShieldBonusIcon3");


//        private float m_PsIconBar_Width = 150.0f;

//        private const float s_MainStringScale = 13.0f;
//        private const float s_BonusStringScale = 13.0f;
//        #endregion

//        struct ItemCard
//        {
//            public int Slot;
//            //public MyStringId IconStringId;
//            public StringBuilder LabelSB;

//            public HudAPIv2.BillBoardHUDMessage IconBillboard;
//            public HudAPIv2.HUDMessage Label;
//        }

//        List<ItemCard> m_HudIcons = new List<ItemCard>();

//        List<ItemCard> m_FixedHudIcons = new List<ItemCard>();
//        private enum FixedItemSlot
//        {
//            BGTop = 0,
//        }

//        private void InitTextHud()
//        {
//            m_Logger.Log(0, "Starting InitTextHud()");
//            ClientConfig config = ClientConfig.GetInstance();

//            if (!m_TextHudAPI.Heartbeat)
//            {
//                m_Logger.Log(0, "  Text Hud API hasn't recieved heartbeat.");
//                if (m_Ticks % config.ClientUpdateInterval == 0)
//                {
//                    MyAPIGateway.Utilities.ShowNotification("Text HUD API mod is missing. HUD will not be displayed.", Constants.WAIT_TICKS_CLIENT / 2, MyFontEnum.Red);
//                    m_Logger.Log(0, "  Text Hud API mod is missing.");
//                }

//                return;
//            }

//            #region Fixed Icons Initializations
//            m_PSIconBGTop = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_BGTop"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconBGMiddle = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_BGMiddle"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconBGBottom = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_BGBottom"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconMain = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_IconMain"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconMainOvercharge = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_IconMainOvercharge"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconBar = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_IconBar"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            m_PSIconBarBG = new HudAPIv2.BillBoardHUDMessage()
//            {
//                Material = MyStringId.GetOrCompute("PocketShield_IconBarBG"),
//                uvEnabled = true,
//                uvSize = new Vector2(1.0f, 1.0f),
//                uvOffset = new Vector2(),
//                Options = HudAPIv2.Options.Pixel,
//                Width = 260.0f,
//                Height = 120.0f,
//                Blend = BlendTypeEnum.PostPP,
//            };

//            m_PSSBLabelMain = new StringBuilder();
//            m_PSLabelMain = new HudAPIv2.HUDMessage()
//            {
//                Message = m_PSSBLabelMain,
//                Scale = s_MainStringScale,
//                Options = HudAPIv2.Options.Pixel,
//                Blend = BlendTypeEnum.PostPP,
//            };
//            #endregion

//            #region ItemCard Icons Initializations
//            {
//                ItemCard item;
//                item.Slot = config.ShieldCapacityBonusIconPosition;
//                item.IconBillboard = new HudAPIv2.BillBoardHUDMessage()
//                {
//                    Material = MyStringId.GetOrCompute("PocketShield_IconBonusEnergy"),
//                    BillBoardColor = Color.White,
//                    uvEnabled = true,
//                    uvSize = new Vector2(278.0f / 512.0f, 1.0f),
//                    uvOffset = new Vector2(117.0f / 512.0f, 0.0f),
//                    Options = HudAPIv2.Options.Pixel,
//                    Width = 260.0f,
//                    Height = 120.0f,
//                    Blend = BlendTypeEnum.PostPP,
//                };
//                item.LabelSB = new StringBuilder();
//                item.Label = new HudAPIv2.HUDMessage()
//                {
//                    Message = item.LabelSB,
//                    Scale = s_BonusStringScale,
//                    Options = HudAPIv2.Options.Pixel,
//                    Blend = BlendTypeEnum.PostPP,
//                };

//                m_HudIcons.Add(item);
//            }
//            {
//                ItemCard item;
//                item.Slot = config.ShieldChargeRateIconPosition;
//                item.IconBillboard = new HudAPIv2.BillBoardHUDMessage()
//                {
//                    Material = MyStringId.GetOrCompute("PocketShield_IconChargeRate"),
//                    BillBoardColor = Color.White,
//                    uvEnabled = true,
//                    uvSize = new Vector2(278.0f / 512.0f, 1.0f),
//                    uvOffset = new Vector2(117.0f / 512.0f, 0.0f),
//                    Options = HudAPIv2.Options.Pixel,
//                    Width = 260.0f,
//                    Height = 120.0f,
//                    Blend = BlendTypeEnum.PostPP,
//                };
//                item.LabelSB = new StringBuilder();
//                item.Label = new HudAPIv2.HUDMessage()
//                {
//                    Message = item.LabelSB,
//                    Scale = s_BonusStringScale,
//                    Options = HudAPIv2.Options.Pixel,
//                    Blend = BlendTypeEnum.PostPP,
//                };
//                m_HudIcons.Add(item);
//            }

//            #endregion


//            IsTextHudApiInitDone = true;
//        }

//        private void UpdateTextHud()
//        {
//            //m_Logger.Log("Starting UpdateTextHud()");
//            if (!IsTextHudApiInitDone)
//                return;

//            if (m_MyPlayerShieldData == null)
//                return;

//            if (!m_IsHudDirty)
//                return;

//            float hpPercent = m_MyPlayerShieldData.Health / m_MyPlayerShieldData.MaxHealth;
//            //m_Logger.Log("  Shield = " + m_MyPlayerShieldData.Health + " (" + hpPercent * 100.0f + "%)");

//            m_PSIconBar.uvSize = new Vector2(MathHelper.Lerp(0.0f, 230.0f / 256.0f, hpPercent), 1.0f);
//            m_PSIconBar.Width = m_PsIconBar_Width * hpPercent;

//            m_PSShieldCapSB.Clear();
//            if (m_MyPlayerShieldData.Health <= 1000.0f)
//            {
//                m_PSShieldCapSB.AppendFormat("{0:F0}", m_MyPlayerShieldData.Health);
//            }
//            else if (m_MyPlayerShieldData.Health < 1000000.0f)
//            {
//                m_PSShieldCapSB.AppendFormat("{0:F2}k", m_MyPlayerShieldData.Health / 1000.0f);
//            }
//            else
//            {
//                m_PSShieldCapSB.Append("9999.9k+");
//            }

//            m_PSShieldBonus0SB.Clear();
//            //if (m_PlayerShieldData.HealthBonus > 0.0f)
//            m_PSShieldBonus0SB.Append('+');
//            m_PSShieldBonus0SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.HealthBonus * 100.0f);
//            m_PSShieldBonus1SB.Clear();
//            m_PSShieldBonus1SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.Defense * 100.0f);
//            m_PSShieldBonus2SB.Clear();
//            m_PSShieldBonus2SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.BulletDamageResistance * 100.0f);
//            m_PSShieldBonus3SB.Clear();
//            m_PSShieldBonus3SB.AppendFormat("{0:F1}%", m_MyPlayerShieldData.ExplosiveDamageResistance * 100.0f);

//            m_IsHudDirty = false;
//        }

















//    }
//}
