using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using ColossalFramework.UI;
using UnityEngine;
using AnotherTerrain.Services;

namespace AnotherTerrain.Services
{
    public delegate void SettingsPanelEventHandler(float x, float y);

    /// <summary>
    /// I am the paint brush types
    /// </summary>
    public enum Patterns
    {
        Square = 0,
        Ellipses,
        Triangle,
        Star,
        Empty
    }

    public enum ValueType
    {
        Textual,
        Real,
        Whole
    }

    public class SettingsPanel : UIPanel
    {
        Vector2 m_Resolution = new Vector2(Screen.currentResolution.height, Screen.currentResolution.width);
        public Vector2 Resolution
        {
            get {  return new Vector2(Screen.currentResolution.height, Screen.currentResolution.width); }
        }

        UILabel titleLabel;
        UILabel lbInformationLabel;
        UILabel lbTerrainLabel;
        UITextField tfTerrainHeight;
        UILabel lbTerrainHeight;
        UICheckboxDropDown cbTerrainPattern;
        UILabel lbTerrainPattern;
        UITextField tfMaxArea;
        UILabel lbMaxArea;
        UITextField tfMaxHeight;
        UILabel lbMaxHeight;
        UITextField tfMaxWidth;
        UILabel lbMaxWidth;
        UILabel lbSettingsLabel;
        UILabel lbSettingsTop;
        UITextField tfSettingsTop;
        UILabel lbSettingsLeft;
        UITextField tfSettingsLeft;
        UILabel lbSettingsHeight;
        UITextField tfSettingsHeight;
        UILabel lbSettingsWidth;
        UITextField tfSettingsWidth;
        UIButton btUndoButton;
        UILabel lbUndoButton;

        private bool mouseDown;
        private string log;
        private float bottomToolbar = 120;

        UserPreferences up = new UserPreferences();
        
        //private Patterns m_TerrainPattern = Patterns.Empty;
        //public Patterns TerrainPattern
        //{
        //    get { return m_TerrainPattern; }
        //    set { m_TerrainPattern = value; }
        //}

        //private double m_TerrainHeight = 50;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public double TerrainHeight
        //{
        //    get { return m_TerrainHeight; }
        //    set { m_TerrainHeight = value; }
        //}

        //private int m_MaxArea = 8000;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int MaxArea
        //{
        //    get { return m_MaxArea; }
        //    set { m_MaxArea = value; }
        //}

        //private int m_MaxHeight = 800;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int MaxHeight
        //{
        //    get { return m_MaxHeight; }
        //    set { m_MaxHeight = value; }
        //}

        //private int m_MaxWidth = 800;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int MaxWidth
        //{
        //    get { return m_MaxWidth; }
        //    set { m_MaxWidth = value; }
        //}

        //private int m_SettingsTop = 1;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int SettingsTop
        //{
        //    get { return m_SettingsTop; }
        //    set { m_SettingsTop = value; }
        //}

        //private int m_SettingsLeft = 1;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int SettingsLeft
        //{
        //    get { return m_SettingsLeft; }
        //    set { m_SettingsLeft = value; }
        //}

        //private int m_SettingsHeight = 350;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int SettingsHeight
        //{
        //    get { return m_SettingsHeight; }
        //    set { m_SettingsHeight = value; }
        //}
        
        //private int m_SettingsWidth = 250;
        ///// <summary>
        ///// Min and Max set in UITextField
        ///// </summary>
        //public int SettingsWidth
        //{
        //    get { return m_SettingsWidth; }
        //    set { m_SettingsWidth = value; }
        //}

        public event SettingsPanelEventHandler MoveCompleted;

        public override void Awake()
        {
            log = "entering SettingsPanel Awake";
            LoadingExtension.WriteLog(log);

            isInteractive = true;
            enabled = true;

            if (up.SettingsWidth > 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;

            width = up.SettingsWidth;
            height = up.SettingsHeight;

            titleLabel = AddUIComponent<UILabel>();
            lbInformationLabel = AddUIComponent<UILabel>();
            lbTerrainLabel = AddUIComponent<UILabel>();
            lbTerrainHeight = AddUIComponent<UILabel>();
            tfTerrainHeight = AddUIComponent<UITextField>();
            cbTerrainPattern = AddUIComponent<UICheckboxDropDown>();
            lbTerrainPattern = AddUIComponent<UILabel>();
            tfMaxArea = AddUIComponent<UITextField>();
            lbMaxArea = AddUIComponent<UILabel>();
            tfMaxHeight = AddUIComponent<UITextField>();
            lbMaxHeight = AddUIComponent<UILabel>();
            tfMaxWidth = AddUIComponent<UITextField>();
            lbMaxWidth = AddUIComponent<UILabel>();
            lbSettingsLabel = AddUIComponent<UILabel>();
            tfSettingsTop = AddUIComponent<UITextField>();
            lbSettingsTop = AddUIComponent<UILabel>();
            tfSettingsLeft = AddUIComponent<UITextField>();
            lbSettingsLeft = AddUIComponent<UILabel>();
            lbSettingsHeight = AddUIComponent<UILabel>();
            tfSettingsHeight = AddUIComponent<UITextField>();
            lbSettingsWidth = AddUIComponent<UILabel>();
            tfSettingsWidth = AddUIComponent<UITextField>();
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

            if (up.SettingsWidth > 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;

            width = up.SettingsWidth;
            height = up.SettingsHeight;

            //log = string.Format("leaving SettingsPanel Start, up.SettingsWidth, up.SettingsHeight {0}, {1}",width.ToString(), height.ToString());
            //LoadingExtension.WriteLog(log);
        }

        public override void OnEnable()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            FileStream myFileStream = new FileStream("c:/prefs.xml", FileMode.Open);

            up = (UserPreferences)mySerializer.Deserialize(myFileStream);

            //Make sure we have the minimum values
            if (up.MaxArea < 400)
                up.MaxArea = 400;
            if (up.MaxHeight < 400)
                up.MaxHeight = 400;
            if (up.MaxWidth < 400)
                up.MaxWidth = 400;
            if (up.TerrainHeight < 50)
                up.TerrainHeight = 50;
            if (up.SettingsTop < 1)
                up.SettingsTop = 1;
            if (up.SettingsLeft < 1)
                up.SettingsLeft = 1;
            if (up.SettingsWidth < 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;

            log = string.Format("eventMouseDown up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.MaxArea, up.MaxHeight, up.MaxWidth {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} , {8}", up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.MaxArea, up.MaxHeight, up.MaxWidth);
            LoadingExtension.WriteLog(log);

            base.OnEnable();
        }

        public override void OnDisable()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            StreamWriter myWriter = new StreamWriter("UserPreferences.xml");
            mySerializer.Serialize(myWriter, up);
            myWriter.Close();

            log = string.Format("OnDisable up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.MaxArea, up.MaxHeight, up.MaxWidth {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7} , {8}", up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.MaxArea, up.MaxHeight, up.MaxWidth);
            LoadingExtension.WriteLog(log);

            base.OnDisable();
        }

        private void SettingsPanel_eventSizeChanged(UIComponent component, Vector2 value)
        {
            //set minimum values
            if (up.SettingsWidth < 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;
            //set the values
            if (width > up.SettingsWidth)
                width = up.SettingsWidth;
            if (height > up.SettingsHeight)
                height = up.SettingsHeight;
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
                    relativePosition = pos;
                }
                this.tfSettingsHeight.text = Convert.ToInt32(height).ToString();
                this.tfSettingsLeft.text = Convert.ToInt32(pos.x).ToString();
                this.tfSettingsTop.text = Convert.ToInt32(pos.y).ToString();
                this.tfSettingsWidth.text = Convert.ToInt32(width).ToString();

                //log = "Moved panel to (x, y): " + relativePosition.x + ", " + relativePosition.y;
                //LoadingExtension.WriteLog(log, false);
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

                var vertPadding = 30;
                var x = 15;
                var y = 50;
                int l = 120;
                int w = 100;
                int h = 20;
                
                //Now set the location and size
                SetLabel(lbTerrainLabel, "Terrain options", x, y);
                y += vertPadding;

                double val = up.TerrainHeight;

                SetLabel(lbTerrainHeight, "New Terrain Height", x, y);
                SetTextBox(tfTerrainHeight, "TerrainHeight", up.TerrainHeight.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(50f, 2000f), ValueType.Real);
                y += vertPadding;

                string[] vals = { "Square", "Ellipses", "Triangle", "Star", "Empty" };

                SetLabel(lbTerrainPattern, "New Terrain Pattern", x, y);
                SetCheckBoxDropDown(cbTerrainPattern, vals, x + l, y, w, h, "Use values between 24 and 0");
                y += vertPadding;

                int vl = up.MaxArea;
                SetLabel(lbMaxArea, "Selectable Area", x, y);
                SetTextBox(tfMaxArea, "MaxArea", up.MaxArea.ToString(), x + l, y, w, h, "Use values between 400 and 9999", new Vector2(400f, 9999f), ValueType.Whole);
                y += vertPadding;

                vl = up.MaxHeight;
                SetLabel(lbMaxHeight, "Selectable Height", x, y);
                SetTextBox(tfMaxHeight, "MaxHeight", up.MaxHeight.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(400f, 9999f), ValueType.Whole);
                y += vertPadding;

                vl = up.MaxWidth;
                SetLabel(lbMaxWidth, "Selectable Width", x, y);
                SetTextBox(tfMaxWidth, "MaxWidth", up.MaxWidth.ToString(), x + l, y, w, h, "Use values between 2000 and 0.01", new Vector2(400f, 9999f), ValueType.Whole);
                y += vertPadding;
                                
                //Now set the location and size
                SetLabel(lbSettingsLabel, "Screen position", x, y);
                y += vertPadding;

                vl = up.SettingsTop;
                SetLabel(lbSettingsTop, "Top position", x, y);
                SetTextBox(tfSettingsTop, "SettingsTop", up.SettingsTop.ToString(), x + l, y, w, h, "Use values between 1 and 1620", new Vector2(1f, 1620f), ValueType.Whole);
                y += vertPadding;

                vl = up.SettingsLeft;
                SetLabel(lbSettingsLeft, "Left position", x, y);
                SetTextBox(tfSettingsLeft, "SettingsLeft", up.SettingsLeft.ToString(), x + l, y, w, h, "Use values between 1 and 1080", new Vector2(1f, 1080f), ValueType.Whole);
                y += vertPadding;

                vl = up.SettingsHeight;
                SetLabel(lbSettingsHeight, "Height size", x, y);
                SetTextBox(tfSettingsHeight, "SettingsHeight", up.SettingsHeight.ToString(), x + l, y, w, h, "Use values between 250 and 1080", new Vector2(1f, 1080f), ValueType.Whole);
                y += vertPadding;

                vl = up.SettingsWidth;
                SetLabel(lbSettingsWidth, "Width position", x, y);
                SetTextBox(tfSettingsWidth, "SettingsWidth", up.SettingsWidth.ToString(), x + l, y, w, h, "Use values between 250 and 1620", new Vector2(1f, 1620f), ValueType.Whole);
                y += vertPadding;

                SetLabel(lbUndoButton, "Undo changes", x, y);
                SetButton(btUndoButton, "Undo", x + l, y, w, "Undo last update.");
                btUndoButton.eventClick += undoMapButton_eventClick;
                y += vertPadding;
                
                SetLabel(lbUndoButton, "Look here for information", x, y);
                y += (vertPadding + 30);

                up.SettingsHeight = y + 0;
            }
            catch (Exception e)
            {
                LoadingExtension.WriteLog(e.Message);
            }

            //log = "leaving SettingsPanel SetControl " + MadeitHere;
            //LoadingExtension.WriteLog(log);
        }

        public void SetTextBox(UITextField tf, string name, string text, int top, int left, int width, int height, string tooltip, Vector2 minmax, ValueType valueType)
        {
            tf.relativePosition = new Vector3(top, left);
            tf.size = new Vector3(100, 20);
            tf.name = name;
            tf.text = text;
            tf.width = width;
            tf.height = height;
            tf.tooltip = tooltip;
            tf.numericalOnly = (valueType != ValueType.Textual);
            tf.textScale = 0.8f;
            tf.color = Color.black;
            tf.cursorBlinkTime = 0.45f;
            tf.cursorWidth = 1;
            tf.horizontalAlignment = UIHorizontalAlignment.Left;
            tf.selectionBackgroundColor = new Color(233, 201, 148, 255);
            tf.selectionSprite = "EmptySprite";
            tf.verticalAlignment = UIVerticalAlignment.Middle;
            tf.padding = new RectOffset(5, 0, 5, 0);
            tf.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            tf.normalBgSprite = "TextFieldPanel";
            tf.hoveredBgSprite = "TextFieldPanelHovered";
            tf.focusedBgSprite = "TextFieldPanel";
            tf.isInteractive = true;
            tf.enabled = true;
            tf.readOnly = false;
            tf.builtinKeyNavigation = true;
            //used to validate a setting
            //log = "(valueType != ValueType.Textual: " + (valueType != ValueType.Textual);
            //LoadingExtension.WriteLog(log);
        }

        private void TextBox_eventTextChanged(UIComponent component, string value)
        {
            log = "Entering Settings Panel eventTextChanged";
            LoadingExtension.WriteLog(log);

            UITextField tf = (UITextField)component;
            tf.text = Regex.Replace(tf.text, "[^0-9]", "");

            if (tf.text.Length == 0)
                return;
            if (isNumeric(value) == false)
                return;

            int tmp;
            log = "Settings Panel eventTextChanged - setting Textfield: " + tf.name;
            LoadingExtension.WriteLog(log);
            try
            {
                switch (tf.name)
                {
                    case "TerrainHeight":
                        double dtmp = ReturnDouble(value, 50, 2000);
                        up.TerrainHeight = dtmp;
                        break;
                    case "MaxArea":
                        tmp = ReturnInteger(value, 400, 9999);
                        up.MaxArea = tmp;
                        break;
                    case "MaxHeight":
                        tmp = ReturnInteger(value, 400, 9999);
                        up.MaxHeight = tmp;
                        break;
                    case "MaxWidth":
                        tmp = ReturnInteger(value, 400, 9999);
                        up.MaxWidth = tmp;
                        break;
                    case "SettingsTop":
                        tmp = ReturnInteger(value, 1, 1060);
                        up.SettingsTop = tmp;
                        break;
                    case "SettingsLeft":
                        tmp = ReturnInteger(value, 1, 1920);
                        up.SettingsLeft = tmp;
                        break;
                    case "SettingsHeight":
                        tmp = ReturnInteger(value, 350, 1020);
                        up.SettingsHeight = tmp;
                        break;
                    case "SettingsWidth":
                        tmp = ReturnInteger(value, 250, 1920);
                        up.SettingsWidth = tmp;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                LoadingExtension.WriteLog(e.Message);
            }
            log = "Leaving Settings Panel eventTextChanged - name: : " + tf.name + ", Value: " + tf.text;
            LoadingExtension.WriteLog(log);
        }

        private int ReturnInteger(string value, double min, double max)
        {
            int tmp = Convert.ToInt32(min);
            try
            {
                tmp = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                tmp = 0;
            }
            return tmp;
        }

        private double ReturnDouble(string value, int min, int max)
        {
            double tmp = min;
            try
            {
                tmp = Convert.ToDouble(value);
            }
            catch (Exception)
            {
                tmp = 0;
            }
            return tmp;
        }

        private bool isNumeric(string value)
        {
            bool results = false;
            try
            {
                System.Convert.ToInt32(value);
                results = true;
            }
            catch (FormatException e)
            {
                Debug.Log(e.Message);
                throw;
            }
            return results;
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

        private void SetCheckBoxDropDown(UICheckboxDropDown checkBox, string[] values, int top, int left, int width, int height, string tooltip)
        {
            checkBox.items = values;
            checkBox.relativePosition = new Vector3(top, left);
            checkBox.size = new Vector2(width, height);
            checkBox.tooltip = tooltip;
            checkBox.Show();
            checkBox.enabled = true;
        }

        //private void SetTextBox(UITextField tf, string name, string value, int top, int left, int width, int height, string tooltip, Vector2 minmax, UITextField.ValueType numerical)
        //{
        //    SetTextBox(tf, name, value, top, left, width, height, tooltip, minmax, numerical);
        //    tf.eventTextChanged += TextBox_eventTextChanged;
        //}

        private void SetLabel(UILabel label, string p, int x, int y)
        {
            label.relativePosition = new Vector3(x, y);
            label.text = p;
            label.textScale = 0.8f;
            label.size = new Vector3(120, 20);
        }
    }
}