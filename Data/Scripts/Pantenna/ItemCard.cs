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
        //private const float c_DefaultItemHeight = 32.0f;

        public Vector2D Position { get; set; } /* Position is Top-Left. */
        public Vector2D NextItemPosition
        {
            get
            {
                return new Vector2D(Position.X, Position.Y + ItemCardHeight + ConfigManager.ClientConfig.SpaceBetweenItems);
            }
        }
        public float DisplayNameLabelMaxWidth
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                return config.PanelWidth - s_DistanceLabelMaxWidth - config.Padding * 2.0f - config.SpaceBetweenItems * 2.0f;
            }
        }

        public Color LabelColor { get; set; }
        public bool Visible { get; set; }

        public SignalType SignalType { get; set; }
        public float RelativeVelocity { get; set; }
        public float Distance { get; set; }
        public string DisplayNameString { get; set; }
        
        private int m_ShipIconTextureSlot = Constants.TEXTURE_BLANK;
        private int m_TrajectoryTextureSlot = Constants.TEXTURE_BLANK;

        public static float ItemCardWidth
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                if (config.ShowSignalName)
                    return config.PanelWidth - config.Padding * 2.0f;

                return ItemCardHeight * 2.0f + s_DistanceLabelMaxWidth + config.SpaceBetweenItems;
            }
        }
        public static float ItemCardHeight
        {
            get
            {
                return 32.0f * ConfigManager.ClientConfig.ItemScale;
            }
        }
        private static float s_DistanceLabelMaxWidth = ItemCardHeight * 4.0f;

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
                Width = ItemCardHeight,
                Height = ItemCardHeight,
                uvEnabled = true,
                uvSize = new Vector2(64.0f / 256.0f, 64.0f / 128.0f),
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TrajectoryIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = Position,
                Offset = new Vector2D(ItemCardHeight, 0.0),
                Width = ItemCardHeight,
                Height = ItemCardHeight,
                uvEnabled = true,
                uvSize = new Vector2(64.0f / 256.0f, 64.0f / 128.0f),
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_DistanceLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_DistanceSB,
                Origin = Position,
                //Offset = new Vector2D(m_ItemHeight * 2.0f + config.SpaceBetweenItems, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f * config.ItemScale,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };
            m_DisplayNameLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_DisplayNameSB,
                Origin = Position,
                //Offset = new Vector2D(m_DistancLabelMaxWidth + config.SpaceBetweenItems * 2.0f, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f * config.ItemScale,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };
            #endregion

            UpdateItemCardConfig();
        }

        ~ItemCard()
        {
            m_DistanceSB = null;
            m_DisplayNameSB = null;
        }

        public void UpdateItemCard()
        {
            ClientConfig config = ConfigManager.ClientConfig;
            switch (SignalType)
            {
                case SignalType.LargeGrid:
                    m_ShipIconTextureSlot = Constants.TEXTURE_LARGE_SHIP;
                    break;
                case SignalType.SmallGrid:
                    m_ShipIconTextureSlot = Constants.TEXTURE_SMALL_SHIP;
                    break;
                case SignalType.Character:
                    m_ShipIconTextureSlot = Constants.TEXTURE_CHARCTER;
                    break;
                default:
                    m_ShipIconTextureSlot = Constants.TEXTURE_BLANK;
                    break;
            }
            m_ShipIcon.uvOffset = new Vector2((m_ShipIconTextureSlot % 4) * 0.25f, (m_ShipIconTextureSlot / 4) * 0.5f);

            if (RelativeVelocity > config.TrajectorySensitivity)
            {
                //Logger.Log(string.Format("Vel = {0:F2}, Sens = {1:0}", RelativeVelocity, config.TrajectorySensitivity), 5);
                m_TrajectoryTextureSlot = Constants.TEXTURE_GO_AWAY;
            }
            else if (RelativeVelocity < -config.TrajectorySensitivity)
            {
                //Logger.Log(string.Format("Vel = {0:F2}, Sens = {1:0}", RelativeVelocity, -config.TrajectorySensitivity), 5);
                m_TrajectoryTextureSlot = Constants.TEXTURE_APPROACH;
            }
            else
            {
                m_TrajectoryTextureSlot = Constants.TEXTURE_STAY;
            }
            m_TrajectoryIcon.uvOffset = new Vector2((m_TrajectoryTextureSlot % 4) * 0.25f, (m_TrajectoryTextureSlot / 4) * 0.5f);

            m_DistanceSB.Clear();
            m_DistanceSB.Append(Utils.FormatDistanceAsString(Distance));

            if (config.ShowSignalName)
            {
                m_DisplayNameSB.Clear();
                m_DisplayNameSB.Append(DisplayNameString);
                //m_DisplayNameSB.Append(RelativeVelocity);
            }
        }

        public void UpdateItemCardConfig()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            m_ShipIcon.Visible = Visible;
            m_ShipIcon.Origin = Position;

            m_TrajectoryIcon.Visible = Visible;
            m_TrajectoryIcon.Origin = Position;

            float labelHeight = (float)m_DistanceLabel.GetTextLength().Y;
            float offsY = ItemCardHeight * 0.5f - labelHeight * 0.5f;

            m_DistanceLabel.Visible = Visible;
            m_DistanceLabel.Origin = Position;
            m_DistanceLabel.Offset = new Vector2D(ItemCardHeight * 2.0f + config.SpaceBetweenItems, offsY);

            m_DisplayNameLabel.Visible = Visible && ConfigManager.ClientConfig.ShowSignalName;
            m_DisplayNameLabel.Origin = Position;
            m_DisplayNameLabel.Offset = new Vector2D(s_DistanceLabelMaxWidth + config.SpaceBetweenItems * 2.0f, offsY);
        }
    }
}
