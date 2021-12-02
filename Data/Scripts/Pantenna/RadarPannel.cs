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
    static class Utils
    {
        public static string FormatDistanceAsString(float _distance)
        {
            if (_distance > 1000.0f)
            {
                _distance /= 1000.0f;
                return string.Format("{0:F1}km", _distance);
            }

            return string.Format("{0:F1}m", _distance);
        }
    }

    public partial class RadarPanel
    {
        public bool Visible { get; set; }
        public float BackgroundOpacity { get; set; }

        private Color m_HudBGColor = Color.White;
        private List<ItemCard> m_ItemCards = null; /* This keeps track of all 5 displayed item cards. */
        
        private int m_TextureSlot = 3;
        private float RadarRangeIconHeight
        {
            get { return 30.0f * ConfigManager.ClientConfig.ItemScale; }
        }

        public static float LabelScale
        {
            get { return 16.0f * ConfigManager.ClientConfig.ItemScale; }
        }

        private StringBuilder m_RadarRangeSB = null;
        //private StringBuilder m_DummySB = null;

        private HudAPIv2.BillBoardHUDMessage m_Background = null;
        private HudAPIv2.BillBoardHUDMessage m_RadarRangeIcon = null;
        private HudAPIv2.HUDMessage m_RadarRangeLabel = null;

        public RadarPanel()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            Visible = false;
            BackgroundOpacity = 1.0f;
            m_ItemCards = new List<ItemCard>(5);
            //s_CharacterSize = new Dictionary<char, float>();

            m_RadarRangeSB = new StringBuilder(Utils.FormatDistanceAsString(config.RadarMaxRange));
            //m_DummySB = new StringBuilder();

            m_Background = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_BG"),
                Origin = config.PanelPosition,
                Width = config.PanelWidth,
                Height = 0.0f,
                Visible = Visible,
                BillBoardColor = CalculateBGColor(),
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            m_TextureSlot = Constants.TEXTURE_ANTENNA;
            m_RadarRangeIcon = new HudAPIv2.BillBoardHUDMessage()
            {
                Material = MyStringId.GetOrCompute("Pantenna_ShipIcons"),
                Origin = config.PanelPosition + new Vector2D(config.Padding, config.Padding),
                Width = RadarRangeIconHeight,
                Height = RadarRangeIconHeight,
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
                //Origin = config.PanelPosition + new Vector2D(s_RadarRangeIconSize + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0 * config.ItemScale),
                //Font = MyFontEnum.Red,
                Scale = RadarPanel.LabelScale,
                //InitialColor = color,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel | HudAPIv2.Options.Shadowing
            };

            Color color = Color.Darken(Color.FromNonPremultiplied(218, 62, 62, 255), 0.2);
            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, RadarRangeIconHeight + config.Padding * 2.0f);
            //Color color = new Color(218, 62, 62);
            //Color color = Color.White;

            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = new ItemCard(cursorPos, color);
                cursorPos = item.NextItemPosition;
                m_ItemCards.Add(item);
            }
            
            UpdatePanelConfig();
        }

        ~RadarPanel()
        {
            m_ItemCards.Clear();
            m_ItemCards = null;

            //s_CharacterSize.Clear();
            //s_CharacterSize = null;

            m_RadarRangeSB = null;
            //m_DummySB = null;
        }

        public void UpdatePanel(List<SignalData> _signals)
        {
            ClientConfig config = ConfigManager.ClientConfig;

            m_RadarRangeSB.Clear();
            m_RadarRangeSB.Append(Utils.FormatDistanceAsString(config.RadarMaxRange));

            Logger.Log("  signal count: " + _signals.Count, 5);
            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = m_ItemCards[i];
                if (i >= _signals.Count || i >= config.DisplayItemsCount)
                {
                    Logger.Log("  updating item card " + i + ": i > _signal.Count, hide item", 5);
                    item.Visible = false;
                    item.UpdateItemCard();
                }
                else
                {
                    Logger.Log("  updating item card " + i + ": updating item", 5);
                    SignalData signal = _signals[i];

                    item.Visible = Visible && config.ShowPanel;
                    item.SignalType = signal.SignalType;
                    item.RelativeVelocity = signal.Velocity;
                    item.Distance = signal.Distance;

                    item.DisplayNameRawString = signal.DisplayName;

                    item.UpdateItemCard();
                }
            }
        }

        public void UpdatePanelConfig()
        {
            Logger.Log("UpdatePanelConfig()...", 5);
            ClientConfig config = ConfigManager.ClientConfig;
            
            float bgWidth = config.PanelWidth;
            if (!config.ShowSignalName || bgWidth < (ItemCard.ItemCardMinWidth + config.Padding * 2.0f))
            {
                bgWidth = ItemCard.ItemCardMinWidth + config.Padding * 2.0f;
            }
            float bgHeight = config.Padding * 2.0f + config.DisplayItemsCount * ItemCard.ItemCardHeight + config.SpaceBetweenItems * (config.DisplayItemsCount - 1);

            float cursorPosY = config.Padding;

            if (config.ShowMaxRangeIcon)
            {
                bgHeight += RadarRangeIconHeight + config.Padding;
                cursorPosY += RadarRangeIconHeight + config.Padding;
            }

            Logger.Log(">>   ItemCardSize = (" + ItemCard.ItemCardWidth + ", " + ItemCard.ItemCardHeight + ")", 5);
            Logger.Log(">>   bgWidth = " + bgWidth, 5);
            Logger.Log(string.Format(">>   bgHeight = {0:0} * 2.0f + {1:0} * {2:0} + {3:0} * ({4:0} - 1) = {5:0}",
                config.Padding, config.DisplayItemsCount, ItemCard.ItemCardHeight, config.SpaceBetweenItems, config.DisplayItemsCount, bgHeight), 5);
            Logger.Log(string.Format(">>   bgHeight = {0:0} + {1:0} + {2:0} = {3:0}",
                config.Padding * 2.0f, config.DisplayItemsCount * ItemCard.ItemCardHeight, config.SpaceBetweenItems * (config.DisplayItemsCount - 1), bgHeight), 5);

            Logger.Log(">>   Visible = " + Visible + ", ShowPanelBG = " + config.ShowPanelBackground + ", ShowPanel = " + config.ShowPanel, 5);
            m_Background.Visible = Visible && config.ShowPanelBackground && config.ShowPanel;
            Logger.Log(">>     m_Background.Visible = " + m_Background.Visible, 5);
            m_Background.Origin = config.PanelPosition;
            m_Background.Width = bgWidth;
            m_Background.Height = bgHeight;
            m_Background.BillBoardColor = CalculateBGColor();

            m_RadarRangeIcon.Visible = Visible && config.ShowMaxRangeIcon && config.ShowPanel;
            m_RadarRangeIcon.Origin = config.PanelPosition;
            m_RadarRangeIcon.Offset = new Vector2D(config.Padding, config.Padding);
            m_RadarRangeIcon.Width = RadarRangeIconHeight;
            m_RadarRangeIcon.Height = RadarRangeIconHeight;

            float offsY = config.Padding + RadarRangeIconHeight * 0.5f - Constants.MAGIC_LABEL_HEIGHT_16 * 0.5f * config.ItemScale;
            m_RadarRangeLabel.Visible = Visible && config.ShowPanel;
            m_RadarRangeLabel.Origin = config.PanelPosition;
            m_RadarRangeLabel.Offset = new Vector2D(RadarRangeIconHeight + config.Padding + config.SpaceBetweenItems, config.Padding + 8.0 * config.ItemScale);
            m_RadarRangeLabel.Scale = RadarPanel.LabelScale;
            Logger.Log(">>   Scale = " + config.ItemScale + ", Label Y = " + m_RadarRangeLabel.GetTextLength().Y, 5);
            
            Vector2D cursorPos = config.PanelPosition + new Vector2D(config.Padding, cursorPosY);
            for (int i = 0; i < Constants.DISPLAY_ITEMS_COUNT; ++i)
            {
                ItemCard item = m_ItemCards[i];
                item.Visible = Visible && config.ShowPanel;
                item.Position = cursorPos;
                item.UpdateItemCardConfig();
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
