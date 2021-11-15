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

        private int m_TextureSlot = 3;
        private const float s_RadarRangeIconSize = 30.0f;

        private StringBuilder m_RadarRangeSB = null;

        private HudAPIv2.BillBoardHUDMessage m_Background = null;
        private HudAPIv2.BillBoardHUDMessage m_RadarRangeIcon = null;
        private HudAPIv2.HUDMessage m_RadarRangeLabel = null;


        public RadarPannel()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            Visible = false;
            BackgroundOpacity = 1.0f;
            m_ItemCards = new List<ItemCard>(5);

            m_RadarRangeSB = new StringBuilder(Utils.FormatDistanceAsString(config.RadarMaxRange));

            m_Background = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_BG"),
                Origin = config.PanelPosition,
                Offset = new Vector2D(0.0, 0.0),
                Width = 420,
                Height = (float)config.PanelSize.Y,
                //uvEnabled = true,
                //uvSize = new Vector2(1.0f, 1.0f),
                //uvOffset = new Vector2(0.0f, 0.0f),
                //TextureSize = 1.0f,
                Visible = Visible,
                BillBoardColor = CalculateBGColor(),
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TextureSlot = 7;
            m_RadarRangeIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = config.PanelPosition,
                Offset = new Vector2D(config.Padding, config.Padding),
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
                Origin = config.PanelPosition,
                Offset = new Vector2D(s_RadarRangeIconSize + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0),
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
                    item.UpdateItemCard(signal);
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
    }
}
