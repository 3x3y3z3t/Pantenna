// ;
using Sandbox.ModAPI;
using VRageMath;
using static Draygo.API.HudAPIv2;

namespace Pantenna
{
    public partial class Session_PantennaClient
    {
        internal const string c_DefaultValueColorTag = "<color=128,128,128,72>";
        internal const string c_ReadOnlyValueColorTag = "<color=32,223,223,200>";
        internal const string c_NumberColorTag = "<color=223,223,32>";
        internal const string c_BoolTrueColorTag = "<color=32,223,32>";
        internal const string c_BoolFalseColorTag = "<color=223,32,32>";
        internal const float c_SensitivityClamp = 1.0f;
        internal const float c_PanelWidthMaxClamp = 1024.0f;
        internal const float c_PanelWidthMinClamp = 64.0f;
        internal const float c_PaddingClamp = 10.0f;

        #region Labels
        internal string RootItemString
        {
            get
            {
                if (ConfigManager.ClientConfig.ModEnabled)
                    return Constants.LOG_PREFIX;
                else
                    return c_DefaultValueColorTag + Constants.LOG_PREFIX + c_BoolFalseColorTag + " (Disabled)";
            }
        }

        internal string ConfigVersionString
        {
            get { return "Config Version " + c_ReadOnlyValueColorTag + ConfigManager.ClientConfig.ConfigVersion; }
        }

        internal string LogLevelString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s = "Log Level: ";
                if (config.LogLevel < 0)
                    s += (c_BoolFalseColorTag + "Off");
                else
                    s += (c_NumberColorTag + config.LogLevel);
                AppendDefaultString(ref s, config.LogLevel, Constants.CLIENT_LOG_LEVEL);

                return s;
            }
        }
        internal float LogLevelInitialPercent
        {
            get { return (ConfigManager.ClientConfig.LogLevel + 1.0f) / 6.0f; }
        }

        internal string ClientUpdateIntervalString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                float ups = 60.0f / config.ClientUpdateInterval;
                string s = string.Format("Client Update Interval: {0:0}{1:0}<reset> ({2:0.#}<reset>ups)", c_NumberColorTag, config.ClientUpdateInterval, ups);
                AppendDefaultString(ref s, config.ClientUpdateInterval, Constants.CLIENT_UPDATE_INTERVAL);

                return s;
            }
        }

        internal string ItemsCountString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s = string.Format("Display Items Count: {0:0}{1:0}", c_NumberColorTag, config.DisplayItemsCount);
                AppendDefaultString(ref s, config.DisplayItemsCount, Constants.DISPLAY_ITEMS_COUNT);

                return s;
            }
        }
        internal float ItemsCountInitialPercent
        {
            get { return (ConfigManager.ClientConfig.DisplayItemsCount - 1.0f) / 4.0f; }
        }

        internal string MaxRangeString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s = string.Format("Radar Max Range: {0:0}{1:0}", c_NumberColorTag, Utils.FormatDistanceAsString(config.RadarMaxRange));
                AppendDefaultString(ref s, config.RadarMaxRange, Constants.RADAR_MAX_RANGE);

                return s;
            }
        }

        internal string SensitivityString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s = string.Format("Sensitivity: {0:0}{1:F2} m/s", c_NumberColorTag, config.TrajectorySensitivity * (60.0f / config.ClientUpdateInterval));
                if (config.TrajectorySensitivity != Constants.TRAJECTORY_SENSITIVITY)
                    s += string.Format("{0:0} default: {1:F2}", c_DefaultValueColorTag, Constants.TRAJECTORY_SENSITIVITY * (60.0f / config.ClientUpdateInterval));

                return s;
            }
        }
        internal float SensitivityInitialPercent
        {
            get { return ConfigManager.ClientConfig.TrajectorySensitivity / c_SensitivityClamp; }
        }

        internal string ShowPanelString
        {
            get { return "Show Panel: " + FormatBoolString(ConfigManager.ClientConfig.ShowPanel); }
        }

        internal string ShowPanelBGString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Show Panel Background: " + FormatBoolString(config.ShowPanelBackground);
                else
                    s = c_DefaultValueColorTag + "Show Panel Background: " + config.ShowPanelBackground;
                AppendDefaultString(ref s, config.ShowPanelBackground, Constants.SHOW_PANEL_BG);

                return s;
            }
        }

        internal string ShowMaxRangeString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Show Max Range: " + FormatBoolString(config.ShowMaxRangeIcon);
                else
                    s = c_DefaultValueColorTag + "Show Max Range: " + config.ShowMaxRangeIcon;
                AppendDefaultString(ref s, config.ShowMaxRangeIcon, Constants.SHOW_MAX_RANGE_ICON);

                return s;
            }
        }

        internal string ShowDisplayNameString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Show Signal Name: " + FormatBoolString(config.ShowSignalName);
                else
                    s = c_DefaultValueColorTag + "Show Signal Name: " + config.ShowSignalName;
                AppendDefaultString(ref s, config.ShowSignalName, Constants.SHOW_SIGNAL_NAME);

                return s;
            }
        }

        internal string PanelPositionString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = string.Format("Panel Position: ({0:0}{1:0}<reset>, {0:0}{2:0}<reset>)", c_NumberColorTag, (int)config.PanelPosition.X, (int)config.PanelPosition.Y);
                else
                    s = c_DefaultValueColorTag + "Panel Position: (" + (int)config.PanelPosition.X + ", " + (int)config.PanelPosition.X + ")";
                if (config.PanelPosition.X != Constants.PANEL_POS_X || config.PanelPosition.Y != Constants.PANEL_POS_Y)
                    s += c_DefaultValueColorTag + " default: (" + Constants.PANEL_POS_X + ", " + Constants.PANEL_POS_Y + ")";

                return s;
            }
        }
        internal Vector2D PanelPositionItemPos
        {
            get {
                return new Vector2D(
                    +(ConfigManager.ClientConfig.PanelPosition.X - s_ViewportSize.X * 0.5) / (s_ViewportSize.X * 0.5),
                    -(ConfigManager.ClientConfig.PanelPosition.Y - s_ViewportSize.Y * 0.5) / (s_ViewportSize.Y * 0.5) - (RadarPanel.PanelSize.Y) / (s_ViewportSize.Y * 0.5));
            }
        }
        internal Vector2D PanelPositionItemSize
        {
            get
            {
                return new Vector2D(
                    +(RadarPanel.PanelSize.X) / (s_ViewportSize.X * 0.5),
                    +(RadarPanel.PanelSize.Y) / (s_ViewportSize.Y * 0.5));
            }
        }

        internal string PanelWidthString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Panel Width: " + c_NumberColorTag + (int)config.PanelWidth;
                else
                    s = c_DefaultValueColorTag + "Panel Width: " + (int)config.PanelWidth;
                AppendDefaultString(ref s, config.PanelWidth, Constants.PANEL_WIDTH);

                return s;
            }
        }
        internal float PanelWidthInitialPercent
        {
            get { return (ConfigManager.ClientConfig.PanelWidth - RadarPanel.PanelMinWidth - c_PanelWidthMinClamp) / (c_PanelWidthMaxClamp - RadarPanel.PanelMinWidth - c_PanelWidthMinClamp); }
        }

        internal string PaddingString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Padding: " + c_NumberColorTag + (int)config.Padding;
                else
                    s = c_DefaultValueColorTag + "Padding: " + (int)config.Padding;
                AppendDefaultString(ref s, config.Padding, Constants.PADDING);

                return s;
            }
        }
        internal float PaddingInitialPercent
        {
            get { return ConfigManager.ClientConfig.Padding / c_PaddingClamp; }

        }

        internal string MarginString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = "Margin: " + c_NumberColorTag + (int)config.Margin;
                else
                    s = c_DefaultValueColorTag + "Margin: " + (int)config.Margin;
                AppendDefaultString(ref s, config.Margin, Constants.MARGIN);

                return s;
            }
        }
        internal float MarginInitialPercent
        {
            get { return ConfigManager.ClientConfig.Margin / c_PaddingClamp; }
        }

        internal string ScaleString
        {
            get
            {
                ClientConfig config = ConfigManager.ClientConfig;
                string s;
                if (config.ShowPanel)
                    s = string.Format("Scale: {0:0}{1:F1}", c_NumberColorTag, config.ItemScale);
                else
                    s = string.Format("{0:0}Scale: {1:F1}", c_DefaultValueColorTag, config.ItemScale);
                AppendDefaultString(ref s, config.ItemScale, Constants.ITEM_SCALE);

                return s;
            }
        }
        internal float ScaleInitialPercent
        {
            get { return (ConfigManager.ClientConfig.ItemScale - 0.5f) / 1.0f; }
        }
        #endregion
        
        internal MenuRootCategory m_RootCategory = null;

        internal MenuItem m_ConfigVersionItem = null;
        internal MenuSliderInput m_LogLevelItem = null;
        internal MenuTextInput m_ClientUpdateIntervalItem = null;
        internal MenuSliderInput m_ItemsCountItem = null;
        internal MenuTextInput m_MaxRangeItem = null;
        internal MenuSliderInput m_SensitivityItem = null;

        internal MenuSubCategory m_HudConfigSubCategory = null;

        internal MenuItem m_ShowPanelItem = null;
        internal MenuItem m_ShowPanelBGItem = null;
        internal MenuItem m_ShowMaxRangeItem = null;
        internal MenuItem m_ShowDisplayNameItem = null;
        internal MenuScreenInput m_PanelPositionItem = null;
        internal MenuSliderInput m_PanelWidthItem = null;
        internal MenuSliderInput m_PaddingItem = null;
        internal MenuSliderInput m_MarginItem = null;
        internal MenuSliderInput m_ScaleItem = null;

        internal static float s_CachedPanelWidth = ConfigManager.ClientConfig.PanelWidth;
        internal static float s_CachedPadding = ConfigManager.ClientConfig.Padding;
        internal static float s_CachedMargin = ConfigManager.ClientConfig.Margin;
        internal static float s_CachedScale = ConfigManager.ClientConfig.ItemScale;
        internal static Vector2D s_CachedPanelPositionItemPos = ConfigManager.ClientConfig.PanelPosition;

        private bool InitModSettingsMenu()
        {
            ClientConfig config = ConfigManager.ClientConfig;
            m_RootCategory = new MenuRootCategory(RootItemString, MenuRootCategory.MenuFlag.PlayerMenu, Constants.LOG_PREFIX + " Settings");

            m_ConfigVersionItem = new MenuItem(ConfigVersionString, m_RootCategory, Interactable: false);
            m_LogLevelItem = new MenuSliderInput(LogLevelString, m_RootCategory, LogLevelInitialPercent, "Adjust Slider to modify Log Level", LogLevelOnSubmit, ConstructLogLevelHelperString);
            m_ClientUpdateIntervalItem = new MenuTextInput(ClientUpdateIntervalString, m_RootCategory, "Enter an integer for Client Update Interval", ClientUpdateIntervalItemOnSubmit);
            m_ItemsCountItem = new MenuSliderInput(ItemsCountString, m_RootCategory, ItemsCountInitialPercent, "Adjust Slider to modify Items Count", ItemsCountOnSubmit, ConstructItemsCountHelperString);
            m_MaxRangeItem = new MenuTextInput(MaxRangeString, m_RootCategory, "Enter a decimal for Max Range", MaxRangeOnSubmit);
            m_SensitivityItem = new MenuSliderInput(SensitivityString, m_RootCategory, SensitivityInitialPercent, "Adjust Slider to modify Sensitivity (long slider for better control)", SensitivityOnSubmit, ConstructSensitivityHelperString);
            
            m_HudConfigSubCategory = new MenuSubCategory("Hud " + c_ReadOnlyValueColorTag + ">>>", m_RootCategory, "Hud Settings");

            m_ShowPanelItem = new MenuItem(ShowPanelString, m_HudConfigSubCategory, ShowPanelOnClick);
            m_ShowPanelBGItem = new MenuItem(ShowPanelBGString, m_HudConfigSubCategory, ShowPanelBGOnClick);
            m_ShowMaxRangeItem = new MenuItem(ShowMaxRangeString, m_HudConfigSubCategory, ShowMaxRangeOnClick);
            m_ShowDisplayNameItem = new MenuItem(ShowDisplayNameString, m_HudConfigSubCategory, ShowDisplayNameOnClick);
            m_PanelPositionItem = new MenuScreenInput(PanelPositionString, m_HudConfigSubCategory, PanelPositionItemPos, PanelPositionItemSize, PanelPositionHelperString, PanelPositionOnSubmit, PanelPositionUpdate);
            m_PanelWidthItem = new MenuSliderInput(PanelWidthString, m_HudConfigSubCategory, PanelWidthInitialPercent, "Adjust Slider to modify Panel Width", PanelWidthOnSubmit, ConstructPanelWidthHelperString, PanelWidthOnCancel);
            m_PaddingItem = new MenuSliderInput(PaddingString, m_HudConfigSubCategory, PaddingInitialPercent, "Adjust Slider to modify Padding", PaddingOnSubmit, ConstructPaddingHelperString, PaddingOnCancel);
            m_MarginItem = new MenuSliderInput(MarginString, m_HudConfigSubCategory, MarginInitialPercent, "Adjust Slider to modify Margin", MarginOnSubmit, ConstructMarginHelperString, MarginOnCancel);
            m_ScaleItem = new MenuSliderInput(ScaleString, m_HudConfigSubCategory, ScaleInitialPercent, "Adjust Slider to modify Scale", ScaleOnSubmit, ConstructScaleHelperString, ScaleOnCancel);

            RefreshHudSettingsInteractability();
            
            new MenuItem("", m_RootCategory, null, false);
            new MenuItem("——— Load Config ———", m_RootCategory, LoadConfig);
            new MenuItem("——— Save Config ———", m_RootCategory, SaveConfig);

            return true;
        }

        public void RefreshSettingsMenu()
        {
            RefreshSettingsMenuRootItem();

            m_ConfigVersionItem.Text = ConfigVersionString;

            m_LogLevelItem.Text = LogLevelString;
            m_LogLevelItem.InitialPercent = LogLevelInitialPercent;

            m_ClientUpdateIntervalItem.Text = ClientUpdateIntervalString;

            m_ItemsCountItem.Text = ItemsCountString;
            m_ItemsCountItem.InitialPercent = ItemsCountInitialPercent;

            m_MaxRangeItem.Text = MaxRangeString;

            m_SensitivityItem.Text = SensitivityString;
            m_SensitivityItem.InitialPercent = SensitivityInitialPercent;

            // HUD settings;
            m_ShowPanelItem.Text = ShowPanelString;
            RefreshHudSettingsInteractability();

            m_ShowPanelBGItem.Text = ShowPanelBGString;

            m_ShowMaxRangeItem.Text = ShowMaxRangeString;

            m_ShowDisplayNameItem.Text = ShowDisplayNameString;

            m_PanelPositionItem.Text = PanelPositionString;

            m_PanelWidthItem.Text = PanelWidthString;
            m_PanelWidthItem.InitialPercent = PanelWidthInitialPercent;

            m_PaddingItem.Text = PaddingString;
            m_PaddingItem.InitialPercent = PaddingInitialPercent;

            m_MarginItem.Text = MarginString;
            m_MarginItem.InitialPercent = MarginInitialPercent;

            m_ScaleItem.Text = ScaleString;
            m_ScaleItem.InitialPercent = ScaleInitialPercent;
        }

        #region Callback Methods
        public void RefreshHudSettingsInteractability()
        {
            ClientConfig config = ConfigManager.ClientConfig;

            // HACK! user will never get here when mod is disabled, so there is no need to check for .ModEnabled;
            m_ShowPanelBGItem.Interactable = config.ShowPanel;
            m_ShowPanelBGItem.Text = ShowPanelBGString;

            m_ShowMaxRangeItem.Interactable = config.ShowPanel;
            m_ShowMaxRangeItem.Text = ShowMaxRangeString;

            m_ShowDisplayNameItem.Interactable = config.ShowPanel;
            m_ShowDisplayNameItem.Text = ShowDisplayNameString;

            m_PanelPositionItem.Interactable = config.ShowPanel;
            m_PanelPositionItem.Text = PanelPositionString;

            m_PanelWidthItem.Interactable = config.ShowPanel;
            m_PanelWidthItem.Text = PanelWidthString;

            m_PaddingItem.Interactable = config.ShowPanel;
            m_PaddingItem.Text = PaddingString;

            m_MarginItem.Interactable = config.ShowPanel;
            m_MarginItem.Text = MarginString;

            m_ScaleItem.Interactable = config.ShowPanel;
            m_ScaleItem.Text = ScaleString;
        }

        public void RefreshSettingsMenuRootItem()
        {
            m_RootCategory.Text = RootItemString;
            m_RootCategory.Interactable = ConfigManager.ClientConfig.ModEnabled;
        }

        internal void LogLevelOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.LogLevel = MathHelper.RoundToInt(MathHelper.Lerp(-1.0f, 5.0f, _value));

            m_LogLevelItem.Text = LogLevelString;
            m_LogLevelItem.InitialPercent = LogLevelInitialPercent;
        }

        internal string ConstructLogLevelHelperString(float _value)
        {
            int intValue = MathHelper.RoundToInt(MathHelper.Lerp(-1.0f, 5.0f, _value));
            string s = "Log Level: ";
            if (intValue < 0)
                s += string.Format("{0:0}Off", c_BoolFalseColorTag);
            else
                s += string.Format("{0:0}{1:0}", c_NumberColorTag, intValue);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.Lerp(-1.0f, 5.0f, _value);

            return s;
        }

        internal void ClientUpdateIntervalItemOnSubmit(string _string)
        {
            int value = 0;
            if (int.TryParse(_string, out value))
            {
                if (value > 0)
                {
                    ConfigManager.ClientConfig.ClientUpdateInterval = value;
                    m_ClientUpdateIntervalItem.Text = ClientUpdateIntervalString;
                }
            }
        }

        internal void ItemsCountOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.DisplayItemsCount = MathHelper.RoundToInt(MathHelper.Lerp(1.0f, 5.0f, _value));
            UpdateHudConfigs();

            m_ItemsCountItem.Text = ItemsCountString;
            m_ItemsCountItem.InitialPercent = ItemsCountInitialPercent;
        }

        internal string ConstructItemsCountHelperString(float _value)
        {
            int intValue = MathHelper.RoundToInt(MathHelper.Lerp(1.0f, 5.0f, _value));
            string s = string.Format("Display Items Count: {0:0}{1:0}", c_NumberColorTag, intValue);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.Lerp(1.0f, 5.0f, _value);

            return s;
        }

        internal void MaxRangeOnSubmit(string _string)
        {
            float value = 0;
            if (float.TryParse(_string, out value))
            {
                if (value >= 0.0f)
                {
                    ConfigManager.ClientConfig.RadarMaxRange = value;
                    m_MaxRangeItem.Text = MaxRangeString;
                }
            }
        }

        internal void SensitivityOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.TrajectorySensitivity = MathHelper.Lerp(0.0f, c_SensitivityClamp, _value);

            m_SensitivityItem.Text = SensitivityString;
            m_SensitivityItem.InitialPercent = SensitivityInitialPercent;
        }

        internal string ConstructSensitivityHelperString(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            string s = string.Format("Sensitivity: {0:0}{1:F2}", c_NumberColorTag, MathHelper.Lerp(0.0f, c_SensitivityClamp, _value) * (60.0f / config.ClientUpdateInterval));
            return s;
        }

        internal void ShowPanelOnClick()
        {
            ConfigManager.ClientConfig.ShowPanel = !ConfigManager.ClientConfig.ShowPanel;
            UpdateHudConfigs();

            m_ShowPanelItem.Text = ShowPanelString;
            RefreshHudSettingsInteractability();
        }

        internal void ShowPanelBGOnClick()
        {
            ConfigManager.ClientConfig.ShowPanelBackground = !ConfigManager.ClientConfig.ShowPanelBackground;
            UpdateHudConfigs();
            m_ShowPanelBGItem.Text = ShowPanelBGString;
        }

        internal void ShowMaxRangeOnClick()
        {
            ConfigManager.ClientConfig.ShowMaxRangeIcon = !ConfigManager.ClientConfig.ShowMaxRangeIcon;
            UpdateHudConfigs();
            m_ShowMaxRangeItem.Text = ShowMaxRangeString;
        }

        internal void ShowDisplayNameOnClick()
        {
            ConfigManager.ClientConfig.ShowSignalName = !ConfigManager.ClientConfig.ShowSignalName;
            UpdateHudConfigs();
            m_ShowDisplayNameItem.Text = ShowDisplayNameString;
        }
        
        private string PanelPositionHelperString
        {
            get
            {
                string s = string.Format("New Panel Position: ({0:0}{1:0}<reset>, {0:0}{2:0}<reset>) (Drag to reposition)", 
                    c_NumberColorTag, s_CachedPanelPositionItemPos.X, s_CachedPanelPositionItemPos.Y);
                return s;
            }
        }

        private void PanelPositionOnSubmit(Vector2D _vector)
        {
            ConfigManager.ClientConfig.PanelPosition = s_CachedPanelPositionItemPos;
            UpdateHudConfigs();

            m_PanelPositionItem.Text = PanelPositionString;
            m_PanelPositionItem.Origin = PanelPositionItemPos;
        }
        
        private void PanelPositionUpdate(Vector2D _vector)
        {
            s_CachedPanelPositionItemPos = new Vector2D(
                (int)(+(_vector.X + 1.0) * s_ViewportSize.X * 0.5), 
                (int)(-(_vector.Y - 1.0) * s_ViewportSize.Y * 0.5 - RadarPanel.PanelSize.Y));

            m_PanelPositionItem.InputDialogTitle = PanelPositionHelperString;
            //MyAPIGateway.Utilities.ShowNotification("Current pos = " + offsX, config.ClientUpdateInterval * (int)(100.0f / 6.0f));
        }
        
        internal void PanelWidthOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.PanelWidth = MathHelper.RoundToInt(MathHelper.Lerp(RadarPanel.PanelMinWidth + c_PanelWidthMinClamp, c_PanelWidthMaxClamp, _value));
            UpdateHudConfigs();

            m_PanelWidthItem.Text = PanelWidthString;
            m_PanelWidthItem.InitialPercent = PanelWidthInitialPercent;
            m_PanelPositionItem.Size = PanelPositionItemSize;
            s_CachedPanelWidth = config.PanelWidth;
        }

        internal string ConstructPanelWidthHelperString(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.PanelWidth = MathHelper.RoundToInt(MathHelper.Lerp(RadarPanel.PanelMinWidth + c_PanelWidthMinClamp, c_PanelWidthMaxClamp, _value));
            string s = "Panel Width: " + c_NumberColorTag + MathHelper.RoundToInt(config.PanelWidth);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.Lerp(RadarPanel.PanelMinWidth, c_PanelWidthClamp, _value);
            //s += " Cached = " + s_CachedPanelWidth;
            UpdateHudConfigs(); // HACK!!;

            return s;
        }

        internal void PanelWidthOnCancel()
        {
            ConfigManager.ClientConfig.PanelWidth = s_CachedPanelWidth;
            UpdateHudConfigs();
        }

        internal void PaddingOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.Padding = MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            UpdateHudConfigs();

            m_PaddingItem.Text = PaddingString;
            m_PaddingItem.InitialPercent = PaddingInitialPercent;
            s_CachedPadding = config.Padding;
        }

        internal string ConstructPaddingHelperString(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.Padding = MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            string s = "Padding: " + c_NumberColorTag + MathHelper.RoundToInt(config.Padding);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            UpdateHudConfigs(); // HACK!!;

            return s;
        }

        internal void PaddingOnCancel()
        {
            ConfigManager.ClientConfig.Padding = s_CachedPadding;
            UpdateHudConfigs();
        }

        internal void MarginOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.Margin = MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            UpdateHudConfigs();

            m_MarginItem.Text = MarginString;
            m_MarginItem.InitialPercent = MarginInitialPercent;
            s_CachedMargin = config.Margin;
        }

        internal string ConstructMarginHelperString(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.Margin = MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            string s = "Margin: " + c_NumberColorTag + MathHelper.RoundToInt(config.Margin);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.RoundToInt(MathHelper.Lerp(0.0f, c_PaddingClamp, _value));
            UpdateHudConfigs(); // HACK!!;

            return s;
        }

        internal void MarginOnCancel()
        {
            ConfigManager.ClientConfig.Margin = s_CachedMargin;
            UpdateHudConfigs();
        }

        internal void ScaleOnSubmit(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.ItemScale = MathHelper.Lerp(0.5f, 1.5f, _value);
            UpdateHudConfigs();

            m_ScaleItem.Text = ScaleString;
            m_ScaleItem.InitialPercent = ScaleInitialPercent;
            s_CachedScale = config.ItemScale;
        }

        internal string ConstructScaleHelperString(float _value)
        {
            ClientConfig config = ConfigManager.ClientConfig;
            config.ItemScale = MathHelper.Lerp(0.5f, 1.5f, _value);
            string s = string.Format("Scale: {0:0}{1:F1}", c_NumberColorTag, config.ItemScale);
            //s += c_ReadOnlyValueColorTag + " Raw value: " + MathHelper.Lerp(0.5f, 1.5f, _value));
            UpdateHudConfigs(); // HACK!!;

            return s;
        }

        internal void ScaleOnCancel()
        {
            ConfigManager.ClientConfig.ItemScale = s_CachedScale;
            UpdateHudConfigs();

            //MyAPIGateway.Utilities.ShowNotification("Scale = " + ConfigManager.ClientConfig.ItemScale, 3000);
        }
        #endregion

        public void LoadConfig()
        {
            if (ConfigManager.ClientConfig.LoadConfigFile())
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reloaded", 2000);
                UpdatePanelConfig();
                RefreshSettingsMenu();
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config reload failed", 2000);
            }
        }

        public void SaveConfig()
        {
            if (ConfigManager.ClientConfig.SaveConfigFile())
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saved", 2000);
            }
            else
            {
                MyAPIGateway.Utilities.ShowNotification("[" + Constants.LOG_PREFIX + "] Config saving failed", 2000);
            }
        }

        #region Helper Methods
        internal static string FormatBoolString(bool _value)
        {
            return (_value ? c_BoolTrueColorTag : c_BoolFalseColorTag) + _value;
        }

        internal static void AppendDefaultString(ref string _string, bool _value, bool _defaultValue)
        {
            if (_value != _defaultValue)
                _string += (c_DefaultValueColorTag + " default: " + _defaultValue);
        }

        internal static void AppendDefaultString(ref string _string, int _value, int _defaultValue)
        {
            if (_value != _defaultValue)
                _string += (c_DefaultValueColorTag + " default: " + _defaultValue);
        }

        internal static void AppendDefaultString(ref string _string, float _value, float _defaultValue)
        {
            if (_value != _defaultValue)
                _string += (c_DefaultValueColorTag + " default: " + _defaultValue);
        }
        #endregion
    }
}
