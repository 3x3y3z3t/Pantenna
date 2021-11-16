// ;
using Draygo.API;
using ExSharedCore;
using System.Text;
using VRage.Game;
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
        public float DisplayNameLabelWidth
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                return ((float)config.PanelSize.X - config.Padding * 2.0f - ItemHeight * 4.0f - config.SpaceBetweenItems);
            }
        }

        public Color LabelColor { get; set; }
        public bool Visible { get; set; }
        public float TrajectorySensitivity { get; set; }

        public SignalType SignalType { get; set; }
        public float RelativeVelocity { get; set; }
        public float Distance { get; set; }
        public string DisplayNameString { get; set; }

        private int m_ShipIconTextureSlot = 3; // because that slot is empty :))
        private int m_TrajectoryTextureSlot = 3; // because that slot is empty :))

        #region Internal HUD Elements
        private StringBuilder m_DistanceSB = null;
        private StringBuilder m_DisplayNameSB = null;

        private HudAPIv2.BillBoardHUDMessage m_ShipIcon = null;
        private HudAPIv2.BillBoardHUDMessage m_TrajectoryIcon = null;
        private HudAPIv2.HUDMessage m_DistanceLabel = null;
        private HudAPIv2.HUDMessage m_DisplayNameLabel = null;
        #endregion

        public ItemCard(Vector2D _position, Color _labelColor, SignalType _signalType = SignalType.Unknown, float _relativeVelocity = 0.0f, float _distance = 0.0f, string _displayName = "")
        {
            ClientConfig config = ConfigManager.ClientConfig;
            Position = _position;
            LabelColor = _labelColor;
            Visible = false;
            TrajectorySensitivity = config.TrajectorySensitivity;

            SignalType = _signalType;
            RelativeVelocity = _relativeVelocity;
            Distance = _distance;
            DisplayNameString = _displayName;

            
            #region Internal HUD Elements Initializations
            m_DistanceSB = new StringBuilder(Utils.FormatDistanceAsString(_distance));
            m_DisplayNameSB = new StringBuilder(_displayName);

            m_ShipIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = Position,
                Width = ItemHeight,
                Height = ItemHeight,
                uvEnabled = true,
                uvSize = new Vector2(64.0f / 256.0f, 64.0f / 128.0f),
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TrajectoryIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = Position,
                Offset = new Vector2D(ItemHeight, 0.0),
                Width = ItemHeight,
                Height = ItemHeight,
                uvEnabled = true,
                uvSize = new Vector2(64.0f / 256.0f, 64.0f / 128.0f),
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_DistanceLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_DistanceSB,
                Origin = Position,
                Offset = new Vector2D(ItemHeight * 2.0f + config.SpaceBetweenItems, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };
            m_DisplayNameLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_DisplayNameSB,
                Origin = Position,
                Offset = new Vector2D(ItemHeight * 4.0f + config.SpaceBetweenItems + config.Padding, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };
            #endregion
        }

        ~ItemCard()
        {
            m_DistanceSB = null;
            m_DisplayNameSB = null;
        }

        //public void UpdateItemCard(SignalData _signal)
        //{
        //    SignalType = _signal.SignalType;
        //    RelativeVelocity = _signal.Velocity;
        //    Distance = _signal.Distance;
        //    DisplayNameString = _signal.DisplayName;
        //    UpdateItemCard();
        //}

        public void UpdateItemCard()
        {
            switch (SignalType)
            {
                case SignalType.LargeGrid:
                    m_ShipIconTextureSlot = 1;
                    break;
                case SignalType.SmallGrid:
                    m_ShipIconTextureSlot = 0;
                    break;
                case SignalType.Character:
                    m_ShipIconTextureSlot = 2;
                    break;
                default:
                    m_ShipIconTextureSlot = 3;
                    break;
            }
            m_ShipIcon.uvOffset = new Vector2((m_ShipIconTextureSlot % 4) * 0.25f, (m_ShipIconTextureSlot / 4) * 0.5f);
            m_ShipIcon.Visible = Visible;

            if (RelativeVelocity > TrajectorySensitivity)
            {
                Logger.Log(string.Format("Vel = {0:F2}, Sens = {1:0}", RelativeVelocity, TrajectorySensitivity), 5);
                m_TrajectoryTextureSlot = 4;
            }
            else if (RelativeVelocity < -TrajectorySensitivity)
            {
                Logger.Log(string.Format("Vel = {0:F2}, Sens = {1:0}", RelativeVelocity, -TrajectorySensitivity), 5);
                m_TrajectoryTextureSlot = 5;
            }
            else
            {
                m_TrajectoryTextureSlot = 6;
            }
            m_TrajectoryIcon.uvOffset = new Vector2((m_TrajectoryTextureSlot % 4) * 0.25f, (m_TrajectoryTextureSlot / 4) * 0.5f);
            m_TrajectoryIcon.Visible = Visible;

            m_DistanceSB.Clear();
            m_DistanceSB.Append(Utils.FormatDistanceAsString(Distance));
            m_DistanceLabel.Visible = Visible;

            m_DisplayNameSB.Clear();
            m_DisplayNameSB.Append(DisplayNameString);
            //m_DisplayNameSB.Append(RelativeVelocity);
            m_DisplayNameLabel.Visible = Visible;
        }

        public void UpdateItemConfig()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            m_ShipIcon.Origin = Position;
            m_TrajectoryIcon.Origin = Position;
            m_DistanceLabel.Origin = Position;
            m_DisplayNameLabel.Origin = Position;
        }
    }
}
