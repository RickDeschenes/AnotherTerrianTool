using System;
using ColossalFramework.UI;
using UnityEngine;
using System.Text.RegularExpressions;
using AnotherTerrain.Services;

namespace AnotherTerrain.Services
{
    public delegate void SettingsPanelEventHandler(float x, float y);

    public class SettingsPanel : UIPanel
    {
        public enum Patterns
        {
            Square,
            Ellipses,
            Triangle,
            Star,
            Empty
        }

        Vector2 m_Resolution = new Vector2(Screen.currentResolution.height, Screen.currentResolution.width);
        public Vector2 Resolution
        {
            get {  return new Vector2(Screen.currentResolution.height, Screen.currentResolution.width); }
        }

        Patterns m_Patterns;
        private int m_TerrainPattern = 0;
        public int TerrainPattern
        {
            get { return m_TerrainPattern; }
            set
            {
                m_TerrainPattern = value;
                if (value < 0)
                    m_TerrainPattern = 0;
                if (value > 4)
                    m_TerrainPattern = 4;

                switch (m_TerrainPattern)
                    {
                    case 0:
                        m_Patterns = Patterns.Square;
                        break;
                    case 1:
                        m_Patterns = Patterns.Ellipses;
                        break;
                    case 2:
                        m_Patterns = Patterns.Triangle;
                        break;
                    case 3:
                        m_Patterns = Patterns.Star;
                        break;
                    case 4:
                        m_Patterns = Patterns.Empty;
                        break;
                    case 5:
                        m_Patterns = Patterns.Empty;
                        break;
                    case 6:
                        m_Patterns = Patterns.Empty;
                        break;
                    case 7:
                        m_Patterns = Patterns.Empty;
                        break;
                    default:
                        m_Patterns = Patterns.Empty;
                        break;
                }
            }
        }

        UILabel titleLabel;
        UILabel lbTerrainLabel;
        UITextBox tfTerrainHeight;
        UILabel lbTerrainHeight;
        UITextBox tfTerrainPattern;
        UILabel lbTerrainPattern;
        UITextBox tfMaxArea;
        UILabel lbMaxArea;
        UITextBox tfMaxHeight;
        UILabel lbMaxHeight;
        UITextBox tfMaxWidth;
        UILabel lbMaxWidth;
        UILabel lbSettingsLabel;
        UILabel lbSettingsTop;
        UITextBox tfSettingsTop;
        UILabel lbSettingsLeft;
        UITextBox tfSettingsLeft;
        UILabel lbSettingsHeight;
        UITextBox tfSettingsHeight;
        UILabel lbSettingsWidth;
        UITextBox tfSettingsWidth;
        UIButton btUndoButton;
        UILabel lbUndoButton;

        private bool mouseDown;
        private string log;

        private double m_TerrainHeight = 80;
        public double TerrainHeight
        {
            get { return m_TerrainHeight; }
            set
            {
                m_TerrainHeight = value;
                if (value < 0.01)
                    m_TerrainHeight = 0.01;
                if (value > 2000)
                    m_TerrainHeight = 2000;
            }
        }

        private int m_MaxArea = 8000;
        public int MaxArea
        {
            get { return m_MaxArea; }
            set
            {
                m_MaxArea = value;
                if (value < 400)
                    m_MaxArea = 400;
                if (value > 9999)
                    m_MaxArea = 9999;
            }
        }

        private int m_MaxHeight = 800;
        public int MaxHeight
        {
            get { return m_MaxHeight; }
            set
            {
                m_MaxHeight = value;
                if (value < 400)
                    m_MaxHeight = 400;
                if (value > 1080)
                    m_MaxHeight = 9999;
            }
        }
        private int m_MaxWidth = 800;
        public int MaxWidth
        {
            get { return m_MaxWidth; }
            set
            {
                m_MaxWidth = value;
                if (value < 400)
                    m_MaxWidth = 400;
                if (value > 1080)
                    m_MaxWidth = 9999;
            }
        }

        private int m_SettingsTop = 1;
        public int SettingsTop
        {
            get { return m_SettingsTop; }
            set
            {
                m_SettingsTop = value;
                if (value < 1)
                    m_SettingsTop = 1;
                if (value > 1080)
                    m_SettingsTop = 1080;
            }
        }

        private int m_SettingsLeft = 1;
        public int SettingsLeft
        {
            get { return m_SettingsLeft; }
            set
            {
                m_SettingsLeft = value;
                if (value < 1)
                    m_SettingsLeft = 1;
                if (value > 1620)
                    m_SettingsLeft = 1620;
            }
        }

        private int m_SettingsHeight = 350;
        public int SettingsHeight
        {
            get { return m_SettingsHeight; }
            set
            {
                m_SettingsHeight = value;
                if (value < 350)
                    m_SettingsHeight = 350;
                if (value > 1080)
                    m_SettingsHeight = 1080;
            }
        }
        
        private int m_SettingsWidth = 250;
        private float bottomToolbar = 120;

        public int SettingsWidth
        {
            get { return m_SettingsWidth; }
            set
            {
                m_SettingsWidth = value;
                if (value < 250)
                    m_SettingsWidth = 250;
                if (value > 1080)
                    m_SettingsWidth = 1080;
            }
        }


        public event SettingsPanelEventHandler MoveCompleted;

        public override void Awake()
        {
            log = "entering SettingsPanel Awake";
            LoadingExtension.WriteLog(log);

            isInteractive = true;
            enabled = true;
            width = m_SettingsWidth;
            height = m_SettingsHeight;

            titleLabel = AddUIComponent<UILabel>();
            lbTerrainLabel = AddUIComponent<UILabel>();
            lbTerrainHeight = AddUIComponent<UILabel>();
            tfTerrainHeight = AddUIComponent<UITextBox>();
            tfTerrainPattern = AddUIComponent<UITextBox>();
            lbTerrainPattern = AddUIComponent<UILabel>();
            tfMaxArea = AddUIComponent<UITextBox>();
            lbMaxArea = AddUIComponent<UILabel>();
            tfMaxHeight = AddUIComponent<UITextBox>();
            lbMaxHeight = AddUIComponent<UILabel>();
            tfMaxWidth = AddUIComponent<UITextBox>();
            lbMaxWidth = AddUIComponent<UILabel>();
            lbSettingsLabel = AddUIComponent<UILabel>();
            tfSettingsTop = AddUIComponent<UITextBox>();
            lbSettingsTop = AddUIComponent<UILabel>();
            tfSettingsLeft = AddUIComponent<UITextBox>();
            lbSettingsLeft = AddUIComponent<UILabel>();
            lbSettingsHeight = AddUIComponent<UILabel>();
            tfSettingsHeight = AddUIComponent<UITextBox>();
            lbSettingsWidth = AddUIComponent<UILabel>();
            tfSettingsWidth = AddUIComponent<UITextBox>();
            btUndoButton = AddUIComponent<UIButton>();
            lbUndoButton = AddUIComponent<UILabel>();

            base.Awake();

            log = "leaving SettingsPanel Awake";
            LoadingExtension.WriteLog(log);
        }

        public override void Start()
        {
            base.Start();

            log = "entering SettingsPanel Start";
            LoadingExtension.WriteLog(log);

            relativePosition = new Vector3(1550f, 550f);
            backgroundSprite = "MenuPanel2";
            isInteractive = true;
            titleLabel.eventMouseDown += titleLabel_eventMouseDown;
            titleLabel.eventMouseMove += titleLabel_eventMouseMove;
            titleLabel.eventMouseUp += titleLabel_eventMouseUp;
            
            eventSizeChanged += SettingsPanel_eventSizeChanged;

            SetControl("Another Terrain Tool Options");

            width = m_SettingsWidth;
            height = m_SettingsHeight;

            log = string.Format("leaving SettingsPanel Start, m_SettingsWidth, m_SettingsHeight {0}, {1}",width.ToString(), height.ToString());
            LoadingExtension.WriteLog(log);
        }

        public override void OnEnable()
        {
            MaxArea = Properties.Settings.Default.MaxArea;
            MaxHeight = Properties.Settings.Default.MaxHeight;
            MaxWidth = Properties.Settings.Default.MaxWidth;
            SettingsTop = Properties.Settings.Default.SettingsTop;
            SettingsLeft = Properties.Settings.Default.SettingsLeft;
            SettingsHeight = Properties.Settings.Default.SettingsHeight;
            SettingsWidth = Properties.Settings.Default.SettingsWidth;
            TerrainHeight = Properties.Settings.Default.TerrainHeight;
            TerrainPattern = Properties.Settings.Default.TerrainPattern;
            log = string.Format("eventMouseDown m_TerrainHeight, m_TerrainPattern, m_SettingsTop, m_SettingsLeft, m_SettingsWidth, m_SettingsHeight, m_MaxArea, m_MaxHeight, m_MaxWidth {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} , {8}", m_TerrainHeight, m_TerrainPattern, m_SettingsTop, m_SettingsLeft, m_SettingsWidth, m_SettingsHeight, m_MaxArea, m_MaxHeight, m_MaxWidth);
            LoadingExtension.WriteLog(log);

            base.OnEnable();
        }

        public override void OnDisable()
        {
            Properties.Settings.Default.MaxArea = m_MaxArea;
            Properties.Settings.Default.MaxHeight = m_MaxHeight;
            Properties.Settings.Default.MaxWidth = m_MaxWidth;
            Properties.Settings.Default.SettingsTop = m_SettingsTop;
            Properties.Settings.Default.SettingsLeft= m_SettingsLeft;
            Properties.Settings.Default.SettingsHeight = m_SettingsHeight;
            Properties.Settings.Default.SettingsWidth = m_SettingsWidth;
            Properties.Settings.Default.TerrainHeight = m_TerrainHeight;
            Properties.Settings.Default.TerrainPattern = m_TerrainPattern;
            log = string.Format("OnDisable m_TerrainHeight, m_TerrainPattern, m_SettingsTop, m_SettingsLeft, m_SettingsWidth, m_SettingsHeight, m_MaxArea, m_MaxHeight, m_MaxWidth {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} , {8}", m_TerrainHeight, m_TerrainPattern, m_SettingsTop, m_SettingsLeft, m_SettingsWidth, m_SettingsHeight, m_MaxArea, m_MaxHeight, m_MaxWidth);
            LoadingExtension.WriteLog(log);

            base.OnDisable();
        }

        private void SettingsPanel_eventSizeChanged(UIComponent component, Vector2 value)
        {
            if (width > m_SettingsWidth)
                width = m_SettingsWidth;
            if (height > m_SettingsHeight)
                height = m_SettingsHeight;
            titleLabel.width = width;
            //log = "leaving SettingsPanel_eventSizeChanged";
            //LoadingExtension.WriteLog(log);
        }

        private void titleLabel_eventMouseDown(UIComponent component, UIMouseEventParameter eventParam)
        {
            //all we need is set to left mouse click
            //(if we wanted while holding Ctrl we could check for it)
            mouseDown = Input.GetMouseButton(0);

            //I am using all this as anexample how to handle mose event
            //LoadingExtension.WriteLog("Enter Mouse Down.");
            //if (Input.GetMouseButton(0) == true)
            //{
            //    LoadingExtension.WriteLog("Left Mouse Click in Mouse Down.");
            //    mouseDown = true;
            //}
            //else
            //    LoadingExtension.WriteLog("Hit.Collider is null in Mouse Down.");
        }

        private void titleLabel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                //log = "eventMouseMove: " + eventParam.moveDelta.x + " X " + eventParam.moveDelta.y;
                //LoadingExtension.WriteLog(log);
                try
                {
                    // Move the top and left according to the delta amount
                    Vector3 delta = new Vector3(eventParam.moveDelta.x, eventParam.moveDelta.y);
                    //Just move the Panel
                    position += delta;
                }
                catch (Exception e)
                {
                    LoadingExtension.WriteLog(e.Message);
                }
            }
        }

        private void titleLabel_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                mouseDown = false;
                MoveCompleted(position.x, position.y);

                Vector2 hw = new Vector2(width, height + bottomToolbar);
                //not sure why but the X, Y here is different from the mouse and possition x, y
                Vector2 res = new Vector2(Resolution.y, Resolution.x);
                Vector3 rel = relativePosition;
                float x = rel.x;
                float y = rel.y;
                if (rel.x < 0)
                    x = 1;
                if (rel.y < 0)
                    y = 1;
                if (rel.x + hw.x > res.x)
                    x = res.x - hw.x;
                if (rel.y + hw.y > res.y)
                    y = res.y - hw.y;
                Vector3 pos = new Vector3(x, y, rel.z);
                if (pos != rel)
                {
                    log = "Moving Panel from: " + rel + " to " + pos;
                    LoadingExtension.WriteLog(log);
                    transform.position = pos;
                    log = "Moved panel to: " + pos;
                    LoadingExtension.WriteLog(log, false);
                }
            }
            mouseDown = false;
        }

        private static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            return ScreenToTerrain ? coords / 16f + 1080 / 2 : (coords - 1080 / 2) * 16f;
        }

        private void SetControl(string title)
        {
            //log = "entering SettingsPanel SetControl";
            //LoadingExtension.WriteLog(log);
            try
            {
                titleLabel.text = title;
                titleLabel.relativePosition = new Vector3(1, 1);
                titleLabel.autoSize = false;
                titleLabel.color = Color.white;
                titleLabel.size = new Vector2(width, 40);
                titleLabel.textScale = 0.9f;
                titleLabel.textAlignment = UIHorizontalAlignment.Center;
                titleLabel.verticalAlignment = UIVerticalAlignment.Middle;
                //titleLabel.size = size;

                var vertPadding = 30;
                var x = 15;
                var y = 50;
                int l = 120;
                int w = 100;
                int h = 20;
                
                //Now set the location and size
                SetLabel(lbTerrainLabel, "Terrain options", x, y);
                y += vertPadding;

                double val = Properties.Settings.Default.TerrainHeight;

                SetLabel(lbTerrainHeight, "New Terrain Height", x, y);
                SetTextBox(tfTerrainHeight, m_TerrainHeight.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(0.01f, 2000f), UITextBox.ValueType.Real);
                y += vertPadding;

                //SetLabel(lbTerrainPattern, "New Terrain Pattern", x, y);
                //SetTextBox(tfTerrainPattern, TerrainPattern.ToString, x + l, y, w, h, "Use values between 24 and 0", new Vector2(0f, 4f), UITextBox.ValueType.Real);
                //tfTerrainPattern.tooltip = "Enter 0";
                //y += vertPadding;

                int vl = Properties.Settings.Default.MaxArea;
                SetLabel(lbMaxArea, "Selectable Area", x, y);
                SetTextBox(tfMaxArea, m_MaxArea.ToString(), x + l, y, w, h, "Use values between 400 and 9999", new Vector2(400f, 29999f), UITextBox.ValueType.Real);
                y += vertPadding;

                vl = Properties.Settings.Default.MaxHeight;
                SetLabel(lbMaxHeight, "Selectable Height", x, y);
                SetTextBox(tfMaxHeight, m_MaxHeight.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(0.01f, 2000f), UITextBox.ValueType.Real);
                y += vertPadding;

                vl = Properties.Settings.Default.MaxWidth;
                SetLabel(lbMaxWidth, "Selectable Width", x, y);
                SetTextBox(tfMaxWidth, m_MaxWidth.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(0.01f, 2000f), UITextBox.ValueType.Real);
                y += vertPadding;
                                
                //Now set the location and size
                SetLabel(lbSettingsLabel, "Screen position", x, y);
                y += vertPadding;

                vl = Properties.Settings.Default.SettingsTop;
                SetLabel(lbSettingsTop, "Top position", x, y);
                SetTextBox(tfSettingsTop, m_SettingsTop.ToString(), x + l, y, w, h, "Use values between 1 and 1620", new Vector2(1f, 1620f), UITextBox.ValueType.Real);
                y += vertPadding;

                vl = Properties.Settings.Default.SettingsLeft;
                SetLabel(lbSettingsLeft, "Left position", x, y);
                SetTextBox(tfSettingsLeft, m_SettingsLeft.ToString(), x + l, y, w, h, "Use values between 1 and 1080", new Vector2(1f, 1080f), UITextBox.ValueType.Real);
                y += vertPadding;

                vl = Properties.Settings.Default.SettingsHeight;
                SetLabel(lbSettingsHeight, "Height size", x, y);
                SetTextBox(tfSettingsHeight, m_SettingsHeight.ToString(), x + l, y, w, h, "Use values between 250 and 1080", new Vector2(1f, 1080f), UITextBox.ValueType.Real);
                y += vertPadding;

                vl = Properties.Settings.Default.SettingsWidth;
                SetLabel(lbSettingsWidth, "Width position", x, y);
                SetTextBox(tfSettingsWidth, m_SettingsWidth.ToString(), x + l, y, w, h, "Use values between 250 and 1620", new Vector2(1f, 1620f), UITextBox.ValueType.Real);
                y += vertPadding;

                SetLabel(lbUndoButton, "Undo changes", x, y);
                SetButton(btUndoButton, "Undo", x + l, y, w, "Undo last update.");
                btUndoButton.eventClick += undoMapButton_eventClick;
                y += vertPadding + 5;

                SettingsHeight = y + 0;
            }
            catch (Exception e)
            {
                LoadingExtension.WriteLog(e.Message);
            }

            //log = "leaving SettingsPanel SetControl " + MadeitHere;
            //LoadingExtension.WriteLog(log);
        }

        private void undoMapButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            throw new NotImplementedException();
        }

        private void SetButton(UIButton okButton, string p1, int x, int y, int w, string tt)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.width = w;
            okButton.tooltip = tt;
            okButton.size = new Vector2(50, 18);
            okButton.relativePosition = new Vector3(x, y - 3);
            okButton.textScale = 0.8f;
        }

        private void SetCheckBox(UICheckBox checkbox, int x, int y)
        {
            checkbox.isChecked = true;
            checkbox.relativePosition = new Vector3(x, y);
            checkbox.size = new Vector2(13, 13);
            checkbox.Show();
            checkbox.color = new Color32(185, 221, 254, 255);
            checkbox.enabled = true;
        }

        private void SetTextBox(UITextBox scaleTextBox, string value, int top, int left, int width, int height, string tooltip, Vector2 minmax, UITextBox.ValueType numerical)
        {
            scaleTextBox.SetTextBox(value, top, left, width, height, tooltip, minmax, numerical);
            //scaleTextBox.relativePosition = new Vector3(x, y - 4);
            //scaleTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
            //scaleTextBox.text = p;
            //scaleTextBox.textScale = 0.8f;
            //scaleTextBox.color = Color.black;
            //scaleTextBox.cursorBlinkTime = 0.45f;
            //scaleTextBox.cursorWidth = 1;
            //scaleTextBox.width = w;
            //scaleTextBox.tooltip = tt;
            //scaleTextBox.selectionBackgroundColor = new Color(233, 201, 148, 255);
            //scaleTextBox.selectionSprite = "EmptySprite";
            //scaleTextBox.verticalAlignment = UIVerticalAlignment.Middle;
            //scaleTextBox.padding = new RectOffset(5, 0, 5, 0);
            //scaleTextBox.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            //scaleTextBox.normalBgSprite = "TextFieldPanel";
            //scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            //scaleTextBox.focusedBgSprite = "TextFieldPanel";
            //scaleTextBox.size = new Vector3(100, 20);
            //scaleTextBox.isInteractive = true;
            //scaleTextBox.enabled = true;
            //scaleTextBox.readOnly = false;
            //scaleTextBox.builtinKeyNavigation = true;
            //scaleTextBox.eventTextChanged += ScaleTextBox_eventTextChanged;
            //scaleTextBox.numericalOnly = num;
            //scaleTextBox.minimumSize = mm;
        }

        //I was moved to textbox class
        //private void ScaleTextBox_eventTextChanged(UIComponent component, string value)
        //{
        //    UITextBox tf = (UITextBox)component;
        //    tf.text = Regex.Replace(tf.text, "[^0-9]", "");

        //    if (tf.text.Length == 0)
        //        return;

        //    double vl = Convert.ToDouble(tf.text);
        //    if (vl > tf.minimumSize.x)
        //        tf.text = tf.minimumSize.x.ToString();
        //    if (vl > tf.minimumSize.y)
        //        tf.text = tf.minimumSize.y.ToString();

        //    int vli = Convert.ToInt32(vl);
        //    switch (tf.name)
        //    {
        //        case "New Terrain Height":
        //            TerrainHeight = vl;
        //            break;
        //        case "New Terrain Pattern":
        //            TerrainPattern = vli;
        //            break;
        //        case "Selectable Area":
        //            MaxArea = vli;
        //            break;
        //        case "Selectable Height":
        //            MaxHeight = vli;
        //            break;
        //        case "Selectable Width":
        //            MaxWidth = vli;
        //            break;
        //        case "Top position":
        //            SettingsTop = vli;
        //            break;
        //        case "Left position":
        //            SettingsLeft = vli;
        //            break;
        //        case "Height size":
        //            SettingsHeight = vli;
        //            break;
        //        case "Width position":
        //            SettingsWidth = vli;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        private void SetLabel(UILabel pedestrianLabel, string p, int x, int y)
        {
            pedestrianLabel.relativePosition = new Vector3(x, y);
            pedestrianLabel.text = p;
            pedestrianLabel.textScale = 0.8f;
            pedestrianLabel.size = new Vector3(120, 20);
        }
    }
}