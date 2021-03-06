// ;
using Draygo.API;
using ExShared;
using System.Collections.Generic;
using System.Text;
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
                return new Vector2D(Position.X, Position.Y + ItemCardHeight + ConfigManager.ClientConfig.Margin * 2.0f);
            }
        }
        public float DisplayNameLabelMaxWidth
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                //Logger.Log(">> panelWidth = " + config.PanelWidth + ", distlabelmaxwidth = " + DistanceLabelMaxWidth);
                return config.PanelWidth - DistanceLabelMaxWidth - config.Padding * 2.0f - config.Margin * 2.0f;
            }
        }

        public Color LabelColor { get; set; }
        public bool Visible { get; set; }

        public SignalType SignalType { get; set; }
        public float RelativeVelocity { get; set; }
        public float Distance { get; set; }
        public string DisplayNameString { get; set; }
        public string DisplayNameRawString { get; set; }
        
        private int m_ShipIconTextureSlot = Constants.TEXTURE_BLANK;
        private int m_TrajectoryTextureSlot = Constants.TEXTURE_BLANK;

        public static float ItemCardWidth
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                if (config.ShowSignalName)
                    return config.PanelWidth - config.Padding * 2.0f;

                return ItemCardMinWidth;
            }
        }

        public static float ItemCardMinWidth
        {
            get { return DistanceLabelMaxWidth + ConfigManager.ClientConfig.Margin * 2.0f; }
        }

        public static float ItemCardHeight
        {
            get { return 32.0f * ConfigManager.ClientConfig.ItemScale; }
        }

        private static float DistanceLabelMaxWidth
        {
            get { return ItemCardHeight * 4.2f + ConfigManager.ClientConfig.Margin * 2.0f; }
        }

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
                Scale = RadarPanel.LabelScale,
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
                Scale = RadarPanel.LabelScale,
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

            m_ShipIcon.Visible = Visible;
            m_TrajectoryIcon.Visible = Visible;
            m_DistanceLabel.Visible = Visible;
            m_DisplayNameLabel.Visible = Visible && config.ShowSignalName;

            if (!Visible)
                return;

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

            Logger.Log(string.Format("Vel = {0:F2}, Sens = {1:F2}", RelativeVelocity, config.TrajectorySensitivity), 5);
            if (RelativeVelocity > config.TrajectorySensitivity)
            {
                m_TrajectoryTextureSlot = Constants.TEXTURE_GO_AWAY;
            }
            else if (RelativeVelocity < -config.TrajectorySensitivity)
            {
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
                ReconstructLabelString();
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
            m_ShipIcon.Width = ItemCardHeight;
            m_ShipIcon.Height = ItemCardHeight;

            m_TrajectoryIcon.Visible = Visible;
            m_TrajectoryIcon.Origin = Position;
            m_TrajectoryIcon.Offset = new Vector2D(ItemCardHeight, 0.0);
            m_TrajectoryIcon.Width = ItemCardHeight;
            m_TrajectoryIcon.Height = ItemCardHeight;

            //float labelHeight = (float)m_DistanceLabel.GetTextLength().Y;
            float offsY = ItemCardHeight * 0.5f - Constants.MAGIC_LABEL_HEIGHT_16 * 0.5f * config.ItemScale;

            m_DistanceLabel.Visible = Visible;
            m_DistanceLabel.Origin = Position;
            m_DistanceLabel.Offset = new Vector2D(ItemCardHeight * 2.0f + config.Margin * 2.0f, offsY);
            m_DistanceLabel.Scale = RadarPanel.LabelScale;

            m_DisplayNameLabel.Visible = Visible && config.ShowSignalName;
            m_DisplayNameLabel.Origin = Position;
            m_DisplayNameLabel.Offset = new Vector2D(DistanceLabelMaxWidth + config.Margin * 2.0f, offsY);
            m_DisplayNameLabel.Scale = RadarPanel.LabelScale;

            ReconstructLabelString();
        }
        
        private void ReconstructLabelString()
        {
            m_DisplayNameSB.Clear();
            m_DisplayNameSB.Append(DisplayNameRawString);

            if (string.IsNullOrEmpty(DisplayNameRawString))
                return;

            float orgWidth = (float)m_DisplayNameLabel.GetTextLength().X;
            if (orgWidth <= DisplayNameLabelMaxWidth)
            {
                DisplayNameString = DisplayNameRawString;
            }
            else
            {
                float width = GetOrCalculateCharacterSize('.') * 3.0f;
                for (int i = 0; i < DisplayNameRawString.Length; ++i)
                {
                    width += GetOrCalculateCharacterSize(DisplayNameRawString[i]);
                    if (width > DisplayNameLabelMaxWidth)
                    {
                        if (i == 0)
                            DisplayNameString = "";
                        DisplayNameString = DisplayNameRawString.Substring(0, i - 1) + "...";
                        break;
                    }
                }
            }

            m_DisplayNameSB.Clear();
            m_DisplayNameSB.Append(DisplayNameString);
        }

        private float GetOrCalculateCharacterSize(char _character)
        {
            if (RadarPanel.s_CachedScale != ConfigManager.ClientConfig.ItemScale)
            {
                RadarPanel.s_CachedScale = ConfigManager.ClientConfig.ItemScale;
                RadarPanel.s_CharacterSize.Clear();
            }

            if (RadarPanel.s_CharacterSize.ContainsKey(_character))
                return RadarPanel.s_CharacterSize[_character];

            m_DisplayNameSB.Clear();
            m_DisplayNameSB.Append(_character);

            float length = (float)m_DisplayNameLabel.GetTextLength().X;
            RadarPanel.s_CharacterSize.Add(_character, length);
            
            return length;
        }

    }

    public partial class RadarPanel
    {
        internal static float s_CachedScale = 0.0f;
        internal static Dictionary<char, float> s_CharacterSize = new Dictionary<char, float>();
    }
}
