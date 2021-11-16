// ;
using Draygo.API;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRageMath;

using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace Pantenna
{
    static class Utils
    {
        public static string FormatDistanceAsString(float _distance)
        {
            if (_distance > 1000.0f)
            {
                _distance /= 1000.0f;
                return string.Format("{0:F1}km", _distance);
            }

            return string.Format("{0:F1} m", _distance);
        }
    }

    public class RadarPannel
    {
        public bool Visible { get; set; }
        public float BackgroundOpacity { get; set; }

        private Color m_HudBGColor = Color.White;
        private List<ItemCard> m_ItemCards = null; /* This keeps track of all 5 displayed item cards. */
        private Dictionary<char, float> m_CharacterSize;

        private int m_TextureSlot = 3;
        private const float s_RadarRangeIconSize = 30.0f;

        private StringBuilder m_RadarRangeSB = null;
        private StringBuilder m_DummySB = null;

        private HudAPIv2.BillBoardHUDMessage m_Background = null;
        private HudAPIv2.BillBoardHUDMessage m_RadarRangeIcon = null;
        private HudAPIv2.HUDMessage m_RadarRangeLabel = null;

        public RadarPannel()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            Visible = false;
            BackgroundOpacity = 1.0f;
            m_ItemCards = new List<ItemCard>(5);
            m_CharacterSize = new Dictionary<char, float>();

            m_RadarRangeSB = new StringBuilder(Utils.FormatDistanceAsString(config.RadarMaxRange));
            m_DummySB = new StringBuilder();

            m_Background = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_BG"),
                Origin = config.PanelPosition,
                Width = (float)config.PanelSize.X,
                Height = (float)config.PanelSize.Y,
                Visible = Visible,
                BillBoardColor = CalculateBGColor(),
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TextureSlot = 7;
            m_RadarRangeIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = config.PanelPosition + new Vector2D(config.Padding, config.Padding),
                Width = s_RadarRangeIconSize,
                Height = s_RadarRangeIconSize,
                uvEnabled = true,
                uvSize = new Vector2(0.25f, 0.5f),
                uvOffset = new Vector2((m_TextureSlot % 4) * 0.25f, (m_TextureSlot / 4) * 0.5f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_RadarRangeLabel = new HudAPIv2.HUDMessage()
            {
                Message = m_RadarRangeSB,
                Origin = config.PanelPosition + new Vector2D(s_RadarRangeIconSize + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f,
                //InitialColor = color,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };

            Color color = Color.Darken(Color.FromNonPremultiplied(218, 62, 62, 255), 0.2);
            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, s_RadarRangeIconSize + config.Padding * 2.0f);
            //Color color = new Color(218, 62, 62);
            //Color color = Color.White;

            for (int i = 0; i < 5; ++i)
            {
                ItemCard item = new ItemCard(cursorPos, color);
                cursorPos = item.NextItemPosition;
                m_ItemCards.Add(item);
            }
        }

        ~RadarPannel()
        {
            m_ItemCards.Clear();
            m_ItemCards = null;

            m_CharacterSize.Clear();
            m_CharacterSize = null;

            m_RadarRangeSB = null;
            m_DummySB = null;
        }

        public void UpdatePanel(List<SignalData> _signals)
        {
            ClientConfig config = ConfigManager.ClientConfig;

            m_RadarRangeSB.Clear();
            m_RadarRangeSB.Append(Utils.FormatDistanceAsString(config.RadarMaxRange));

            for (int i = 0; i < 5; ++i)
            {
                ItemCard item = m_ItemCards[i];
                if (i >= _signals.Count)
                {
                    item.Visible = false;
                    item.UpdateItemCard();
                }
                else
                {
                    SignalData signal = _signals[i];

                    item.Visible = Visible;
                    item.SignalType = signal.SignalType;
                    item.RelativeVelocity = signal.Velocity;
                    item.Distance = signal.Distance;

                    string signalDisplayName = ConstructLabelString(signal.DisplayName, item.DisplayNameLabelWidth);
                    item.DisplayNameString = signalDisplayName;
                    
                    item.UpdateItemCard();
                }
            }
        }

        public void UpdatePanelConfig()
        {
            ClientConfig config = ConfigManager.ClientConfig;
            
            m_Background.Visible = Visible;
            m_Background.Origin = config.PanelPosition;
            m_Background.Width = (float)config.PanelSize.X;
            m_Background.Height = (float)config.PanelSize.Y;
            m_Background.BillBoardColor = CalculateBGColor();

            m_RadarRangeIcon.Visible = Visible;
            m_RadarRangeIcon.Origin = config.PanelPosition;
            m_RadarRangeIcon.Offset = new Vector2D(config.Padding, config.Padding);

            m_RadarRangeLabel.Visible = Visible;
            m_RadarRangeLabel.Origin = config.PanelPosition;
            m_RadarRangeLabel.Offset = new Vector2D(s_RadarRangeIconSize + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0);

            

            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, s_RadarRangeIconSize + config.Padding * 2.0f);
            for (int i = 0; i < 5; ++i)
            {
                ItemCard item = m_ItemCards[i];
                item.Position = cursorPos;
                item.TrajectorySensitivity = config.TrajectorySensitivity;
                item.UpdateItemConfig();
                cursorPos = item.NextItemPosition;
            }
        }

        private Color CalculateBGColor()
        {
            Color color = Color.White * BackgroundOpacity * BackgroundOpacity * 0.85f;
            color.A = (byte)(BackgroundOpacity * 255);

            return color;
        }

        private string ConstructLabelString(string _originalString, float _maxWidth)
        {
            m_DummySB.Clear();
            m_DummySB.Append(_originalString);
            m_RadarRangeLabel.Message = m_DummySB;
            float orgWidth = (float)m_RadarRangeLabel.GetTextLength().X;
            m_RadarRangeLabel.Message = m_RadarRangeSB;

            if (orgWidth <= _maxWidth)
            {
                return _originalString;
            }
            else
            {
                float width = GetOrCalculateCharacterSize('.') * 3.0f;
                for (int i = 0; i < _originalString.Length; ++i)
                {
                    width += GetOrCalculateCharacterSize(_originalString[i]);
                    if (width >= _maxWidth)
                    {
                        if (i == 0)
                            return "";
                        return _originalString.Substring(0, i - 1) + "...";
                    }
                }

                return _originalString;
            }
        }

        private float GetOrCalculateCharacterSize(char _character)
        {
            if (m_CharacterSize.ContainsKey(_character))
                return m_CharacterSize[_character];
            
            m_DummySB.Clear();
            m_DummySB.Append(_character);
            m_RadarRangeLabel.Message = m_DummySB;

            float length = (float)m_RadarRangeLabel.GetTextLength().X;
            m_CharacterSize.Add(_character, length);

            m_RadarRangeLabel.Message = m_RadarRangeSB;
            
            return length;
        }
    }
}
