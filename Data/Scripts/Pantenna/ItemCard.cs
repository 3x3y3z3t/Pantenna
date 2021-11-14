// ;
using Draygo.API;
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

            //ShipIconOffsX = ConfigManager.ClientConfig.ShipIconOffsX;
            //TrajectoryIconOffsX = ConfigManager.ClientConfig.TrajectoryIconOffsX;
            //DistanceIconOffsX = ConfigManager.ClientConfig.DistanceIconOffsX;
            //DisplayNameIconOffsX = ConfigManager.ClientConfig.DisplayNameIconOffsX;

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
                uvOffset = new Vector2(0.0f, 0.0f),
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
                uvOffset = new Vector2(0.0f, 0.0f),
                TextureSize = 1.0f,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            DistanceLabel = new HudAPIv2.HUDMessage()
            {
                Message = DistanceSB,
                Origin = Position,
                Offset = new Vector2D(DistanceIconOffsX, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
                Visible = Visible,
                Blend = BlendTypeEnum.PostPP,
                Options = HudAPIv2.Options.Pixel
            };
            DisplayNameLabel = new HudAPIv2.HUDMessage()
            {
                Message = DisplayNameSB,
                Origin = Position,
                Offset = new Vector2D(DisplayNameIconOffsX, 9.0),
                //Font = MyFontEnum.Red,
                Scale = 16.0f,
                InitialColor = LabelColor,
                ShadowColor = Color.Black,
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
                _distance /= 1000.0f;
            return string.Format("{0:F1}km", _distance);
            }

            return string.Format("{0:F1} m", _distance);
        }
    }
}
