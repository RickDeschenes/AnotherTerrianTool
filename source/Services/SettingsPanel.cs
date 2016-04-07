using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using ColossalFramework.UI;
using UnityEngine;
using AnotherTerrain.Services;
using System.ComponentModel;

namespace AnotherTerrain.Services
{
    public delegate void SettingsPanelEventHandler(float x, float y);
    public delegate void ApplyUndoEventHandler();

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

    /// <summary>
    /// Value types for the text box types
    /// </summary>
    public enum ValueType
    {
        Textual,
        Real,
        Whole
    }

    /// <summary>
    /// this stores the changes
    /// will allow complete backout of all changes
    /// </summary>
    public struct UndoStroke
    {
        public string name;
        public int minX;
        public int maxX;
        public int minZ;
        public int maxZ;
        public ushort[] originalHeights;
        public ushort[] backupHeights;
        public ushort[] rawHeights;
        public int pointer;
    }

    /// <summary>
    /// The settings used to help the user
    /// use the test boxes to paint an area or select one with you mouse
    /// </summary>
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
        //UICheckboxDropDown cbTerrainPattern;
        //UILabel lbTerrainPattern;
        //UILabel lbRenderLabel;
        //UICheckBox lbRenderCheckBox;
        //UITextField tfStartX;
        //UILabel lbStartX;
        //UITextField tfStartZ;
        //UILabel lbStartZ;
        //UITextField tfEndX;
        //UILabel lbEndX;
        //UITextField tfEndZ;
        //UILabel lbEndZ;
        //UIButton btRenderButton;
        //UILabel lbSettingsLabel;
        //UILabel lbSettingsTop;
        //UITextField tfSettingsTop;
        //UILabel lbSettingsLeft;
        //UITextField tfSettingsLeft;
        //UILabel lbSettingsHeight;
        //UITextField tfSettingsHeight;
        //UILabel lbSettingsWidth;
        //UITextField tfSettingsWidth;
        UILabel lbUndoButton;
        UIButton btUndoButton;
        //UILabel lbInfoLabel;

        public BindingList<UndoStroke> UndoList;

        private bool mouseDown;
        private string log;
        private float bottomToolbar = 120;
        private int min = 0;
        private int max = 1080;

        public UserPreferences up = new UserPreferences();
        
        public event SettingsPanelEventHandler MoveCompleted;
        public event ApplyUndoEventHandler ApplyUndo;

        /// <summary>
        /// yawn now what!
        /// </summary>
        public override void Awake()
        {
            log = "entering SettingsPanel Awake";
            //LoadingExtension.WriteLog(log);

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
            //cbTerrainPattern = AddUIComponent<UICheckboxDropDown>();
            //lbTerrainPattern = AddUIComponent<UILabel>();

            //lbRenderLabel = AddUIComponent<UILabel>();
            //lbRenderCheckBox = AddUIComponent<UICheckBox>();

            //tfStartX = AddUIComponent<UITextField>();
            //lbStartX = AddUIComponent<UILabel>();
            //tfStartZ = AddUIComponent<UITextField>();
            //lbStartZ = AddUIComponent<UILabel>();
            //tfEndX = AddUIComponent<UITextField>();
            //lbEndX = AddUIComponent<UILabel>();
            //tfEndZ = AddUIComponent<UITextField>();
            //lbEndZ = AddUIComponent<UILabel>();
            //btRenderButton = AddUIComponent<UIButton>();
            //lbSettingsLabel = AddUIComponent<UILabel>();
            //tfSettingsTop = AddUIComponent<UITextField>();
            //lbSettingsTop = AddUIComponent<UILabel>();
            //tfSettingsLeft = AddUIComponent<UITextField>();
            //lbSettingsLeft = AddUIComponent<UILabel>();
            //lbSettingsHeight = AddUIComponent<UILabel>();
            //tfSettingsHeight = AddUIComponent<UITextField>();
            //lbSettingsWidth = AddUIComponent<UILabel>();
            //tfSettingsWidth = AddUIComponent<UITextField>();
            lbUndoButton = AddUIComponent<UILabel>();
            btUndoButton = AddUIComponent<UIButton>();
            //lbInfoLabel = AddUIComponent<UILabel>();

            base.Awake();

            log = "leaving SettingsPanel Awake";
            //LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// I am ready!
        /// </summary>
        public override void Start()
        {
            base.Start();

            log = "entering SettingsPanel Start";
            //LoadingExtension.WriteLog(log);

            this.relativePosition = new Vector3(up.SettingsLeft, up.SettingsTop, 0);

            backgroundSprite = "MenuPanel2";
            isInteractive = true;
            titleLabel.eventMouseDown += titleLabel_eventMouseDown;
            titleLabel.eventMouseMove += titleLabel_eventMouseMove;
            titleLabel.eventMouseUp += titleLabel_eventMouseUp;
            
            eventSizeChanged += SettingsPanel_eventSizeChanged;            

            log = string.Format("Leaving SettingsPanel Start: relativePosition: {9}, up.TerrainHeight: {0}, up.TerrainPattern: {1}, up.SettingsTop: {2}, up.SettingsLeft: {3}, up.SettingsWidth: {4}, up.SettingsHeight: {5}, up.StartX: {6}, up.StartZ: {7}, up.EndX:, {8}", up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.StartX, up.StartZ, up.EndX, relativePosition);
            //LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Start me up
        /// </summary>
        public override void OnEnable()
        {
            base.Update();

            log = "Entering SettingsPanel OnEnable";
            //LoadingExtension.WriteLog(log);

            log = "SettingsPanel Start loading XML user prefs";
            //LoadingExtension.WriteLog(log);

            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            FileStream myFileStream = new FileStream("UserPreferences.xml", FileMode.Open);

            up = (UserPreferences)mySerializer.Deserialize(myFileStream);

            log = "SettingsPanel Start Validate Preferences";
            //LoadingExtension.WriteLog(log);
            //Make sure we have the minimum values
            if (up.TerrainHeight < 50)
                up.TerrainHeight = 50;
            if (up.TerrainHeight > 2000)
                up.TerrainHeight = 2000;
            if (up.StartX < min)
                up.StartX = min;
            if (up.StartX > max)
                up.StartX = max;
            if (up.StartZ < min)
                up.StartZ = min;
            if (up.StartZ > max)
                up.StartZ = max;
            if (up.EndX < min)
                up.EndX = min;
            if (up.EndX > max)
                up.EndX = max;
            if (up.EndZ < min)
                up.EndZ = min;
            if (up.EndZ > max)
                up.EndZ = max;
            if (up.SettingsTop < 1)
                up.SettingsTop = 1;
            if (up.SettingsLeft < 1)
                up.SettingsLeft = 1;
            if (up.SettingsWidth < 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;
            if (up.SettingsTop > Resolution.x - up.SettingsWidth)
                up.SettingsTop = (Convert.ToInt32(Resolution.x) - up.SettingsWidth);
            if (up.SettingsLeft > Resolution.y - up.SettingsWidth)
                up.SettingsLeft = (Convert.ToInt32(Resolution.y) - up.SettingsHeight);
            if (up.SettingsWidth < 250)
                up.SettingsWidth = 250;
            if (up.SettingsHeight < 400)
                up.SettingsHeight = 400;

            log = "SettingsPanel Start Set up controls";
            //LoadingExtension.WriteLog(log);
            Vector2 wh = SetControl("Another Terrain Tool Options");

            width = wh.x;
            height = wh.y;

            this.relativePosition = new Vector3(up.SettingsLeft, up.SettingsTop, 0);

            UndoList = new BindingList<UndoStroke>();

            log = string.Format("Leaving SettingsPanel OnEnable location: Top {0} x Left {1}: ", this.position.y, this.position.x);
            //LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Clean me up scottie
        /// </summary>
        public override void OnDisable()
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            StreamWriter myWriter = new StreamWriter("UserPreferences.xml");
            mySerializer.Serialize(myWriter, up);
            myWriter.Close();

            log = string.Format("Leaving OnDisable Start: relativePosition: {9}, up.TerrainHeight: {0}, up.TerrainPattern: {1}, up.SettingsTop: {2}, up.SettingsLeft: {3}, up.SettingsWidth: {4}, up.SettingsHeight: {5}, up.StartX: {6}, up.StartZ: {7}, up.EndX:, {8}", up.TerrainHeight, up.TerrainPattern, up.SettingsTop, up.SettingsLeft, up.SettingsWidth, up.SettingsHeight, up.StartX, up.StartZ, up.EndX, relativePosition);
            //LoadingExtension.WriteLog(log);

            base.OnDisable();
        }

        /// <summary>
        /// Return true is we have any values
        /// </summary>
        /// <returns>True if we have values</returns>
        public bool ListCount()
        {
            if (UndoList != null)
            {
                return (UndoList.Count > 0);
            }
            return false;
        }

        /// <summary>
        /// if we resize make sure we are still large enough
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
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

        /// <summary>
        /// handle the drag drop event
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventParam"></param>
        private void titleLabel_eventMouseDown(UIComponent component, UIMouseEventParameter eventParam)
        {
            mouseDown = Input.GetMouseButton(0);
        }

        /// <summary>
        /// handle the drag drop event
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventParam"></param>
        private void titleLabel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
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

        /// <summary>
        /// Handle the drag drop event
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventParam"></param>
        private void titleLabel_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                mouseDown = false;
                MoveCompleted(relativePosition.x, relativePosition.y);

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
                up.SettingsHeight = Convert.ToInt32(height);
                up.SettingsLeft = Convert.ToInt32(pos.x);
                up.SettingsTop = Convert.ToInt32(pos.y);
                up.SettingsWidth = Convert.ToInt32(width);
            }
            mouseDown = false;
        }

        /// <summary>
        /// Set up the controls for the settings panel
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private Vector2 SetControl(string title)
        {
            //log = "Entering SettingsPanel SetControl";
            //LoadingExtension.WriteLog(log);

            Vector2 result = new Vector2(550, 250);
            try
            {
                titleLabel.text = title;
                titleLabel.relativePosition = new Vector3(1, 1);
                titleLabel.autoSize = false;
                titleLabel.color = Color.white;
                titleLabel.size = new Vector2(this.width, 40);
                titleLabel.textScale = 0.9f;
                titleLabel.textAlignment = UIHorizontalAlignment.Center;
                titleLabel.verticalAlignment = UIVerticalAlignment.Middle;

                var vertPadding = 30;
                var left = 15;
                var top = 50;
                int lenght = 120;
                int width = 100;
                int height = 20;
                
                //Now set the location and size
                SetLabel(lbTerrainLabel, "Terrain options", 5, top, lenght, height);
                top += vertPadding;

                double val = up.TerrainHeight;

                SetLabel(lbTerrainHeight, "New Terrain Height", left, top + 5, lenght, height);
                SetTextBox(tfTerrainHeight, "TerrainHeight", up.TerrainHeight.ToString(), left + lenght, top, width, height, "Use values between 2000 and 0.01", new Vector2(50f, 2000f), ValueType.Real);
                top += vertPadding;

                //string[] vals = { "Square", "Ellipses", "Triangle", "Star", "Empty" };

                //SetLabel(lbTerrainPattern, "New Terrain Pattern", 5, top + 5, lenght, height);
                //SetCheckBoxDropDown(cbTerrainPattern, vals, left + lenght, top, width, height, "Use values between 24 and 0");
                //top += vertPadding;

                //string values = string.Concat("Use values between {0} and {1}.", min, max);
                //Vector2 vec = new Vector2(min, max);

                //int vl = up.StartX;
                //SetLabel(lbStartX, "Start X", left, top + 5, lenght, height);
                //SetTextBox(tfStartX, "StartX", up.StartX.ToString(), left + lenght, top, width, height, values, vec, ValueType.Whole);
                //top += vertPadding;

                //vl = up.StartZ;
                //SetLabel(lbStartZ, "Start Z", left, top + 5, lenght, height);
                //SetTextBox(tfStartZ, "StartZ", up.StartZ.ToString(), left + lenght, top, width, height, values, vec, ValueType.Whole);
                //top += vertPadding;

                //vl = up.EndX;
                //SetLabel(lbEndX, "End X", left, top + 5, lenght, height);
                //SetTextBox(tfEndX, "EndX", up.EndX.ToString(), left + lenght, top, width, height, values, vec, ValueType.Whole);
                //top += vertPadding;

                //vl = up.EndZ;
                //SetLabel(lbEndZ, "End Z", left, top + 5, lenght, height);
                //SetTextBox(tfEndZ, "EndZ", up.EndX.ToString(), left + lenght, top, width, height, values, vec, ValueType.Whole);
                //top += vertPadding;

                //SetButton(btRenderButton, "Update Rendered Area", left, top, Convert.ToInt32(this.width - 20), height + 10, "Undo last update.");
                //top += vertPadding;

                ////Now set the location and size
                //SetLabel(lbSettingsLabel, "Screen position", 5, top + 5, lenght, height);
                //top += vertPadding;

                //Vector2 vcw = new Vector2(1f, 1620f);
                //string hor = string.Concat("Use values between {0} and {1}.", 1f, 1620f);
                //Vector2 vch = new Vector2(1f, 1080f);
                //string ver = string.Concat("Use values between {0} and {1}.", 1f, 1080f);

                //vl = up.SettingsTop;
                //SetLabel(lbSettingsTop, "Top position", left, top + 5, lenght, height);
                //SetTextBox(tfSettingsTop, "SettingsTop", up.SettingsTop.ToString(), left + lenght, top, width, height, hor, vcw, ValueType.Whole);
                //top += vertPadding;

                //vl = up.SettingsLeft;
                //SetLabel(lbSettingsLeft, "Left position", left, top + 5, lenght, height);
                //SetTextBox(tfSettingsLeft, "SettingsLeft", up.SettingsLeft.ToString(), left + lenght, top, width, height, hor, vch, ValueType.Whole);
                //top += vertPadding;

                //vl = up.SettingsHeight;
                //SetLabel(lbSettingsHeight, "Height size", left, top + 5, lenght, height);
                //SetTextBox(tfSettingsHeight, "SettingsHeight", up.SettingsHeight.ToString(), left + lenght, top, width, height, hor, vch, ValueType.Whole);
                //top += vertPadding;

                //vl = up.SettingsWidth;
                //SetLabel(lbSettingsWidth, "Width position", left, top + 5, lenght, height);
                //SetTextBox(tfSettingsWidth, "SettingsWidth", up.SettingsWidth.ToString(), left + lenght, top, width, height, hor, vcw, ValueType.Whole);
                //top += vertPadding;

                //Display the List of Undoable changes
                SetLabel(lbUndoButton, "Undo changes", 5, top, lenght, height);
                top += vertPadding;

                SetButton(btUndoButton, "Undo Changes", left, top - 10, Convert.ToInt32(this.width - 20), height + 10, "Undo last update.");
                top += vertPadding;

                //SetLabel(lbInfoLabel, "Look here for information", left, top + 5, lenght, height);
                //top += vertPadding;

                result.y = top;
                result.x = 250;
            }
            catch (Exception e)
            {
                LoadingExtension.WriteLog(e.Message);
            }

            //log = "Leaving SettingsPanel SetControl results: " + result;
            //LoadingExtension.WriteLog(log);

            return result;
        }

        /// <summary>
        /// Set up out test boxes
        /// </summary>
        /// <param name="tf"></param>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tooltip"></param>
        /// <param name="minmax"></param>
        /// <param name="valueType"></param>
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
            tf.allowFloats = (valueType != ValueType.Textual) && (valueType != ValueType.Whole);
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

            tf.eventTextChanged += TextBox_eventTextChanged;
            //used to validate a setting
            //log = string.Concat("numerical: {0} floats: {1}", tf.numericalOnly, tf.allowFloats);
            //log = "(valueType != ValueType.Textual: " + (valueType != ValueType.Textual);
            //LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Lets see about handling the text changes
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        private void TextBox_eventTextChanged(UIComponent component, string value)
        {
            log = "Entering Settings Panel eventTextChanged";
           // LoadingExtension.WriteLog(log);

            UITextField tf = (UITextField)component;
            tf.text = Regex.Replace(tf.text, "[^0-9]", "");

            if (tf.text.Length == 0)
                return;
            if (isNumeric(value) == false)
                return;

            int tmp;
            log = "Settings Panel eventTextChanged - setting Textfield: " + tf.name;
           // LoadingExtension.WriteLog(log);
            try
            {
                switch (tf.name)
                {
                    case "TerrainHeight":
                        double dtmp = ReturnDouble(value, 0.01, 2000);
                        up.TerrainHeight = dtmp;
                        break;
                    case "StartX":
                        tmp = ReturnInteger(value, 0, 1080);
                        up.StartX = tmp;
                        break;
                    case "StartZ":
                        tmp = ReturnInteger(value, 0, 1080);
                        up.StartZ = tmp;
                        break;
                    case "EndX":
                        tmp = ReturnInteger(value, 0, 1080);
                        up.EndX = tmp;
                        break;
                    case "EndZ":
                        tmp = ReturnInteger(value, 0, 1080);
                        up.EndX = tmp;
                        break;
                    case "SettingsTop":
                        tmp = ReturnInteger(value, 1, 1090);
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
            log = "Leaving SettingsPanel eventTextChanged - name: : " + tf.name + ", Value: " + tf.text;
           // LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Set up our button object
        /// </summary>
        /// <param name="bt"></param>
        /// <param name="p1"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tt"></param>
        private void SetButton(UIButton bt, string p1, int top, int left, int width, int height, string tt)
        {
            bt.name = "UndoButton";
            bt.text = p1;
            bt.normalBgSprite = "ButtonMenu";
            bt.hoveredBgSprite = "ButtonMenuHovered";
            bt.disabledBgSprite = "ButtonMenuDisabled";
            bt.focusedBgSprite = "ButtonMenuFocused";
            bt.pressedBgSprite = "ButtonMenuPressed";
            bt.size = new Vector2(width, height);
            bt.relativePosition = new Vector3(top, left);
            bt.textScale = 0.8f;
            bt.tooltip = tt;

            bt.eventClick += btUndoButton_eventClick;

            try
            {
                bt.enabled = true;
            }
            catch (Exception e)
            {
                LoadingExtension.WriteLog("Error setting enabled: " + e.Message);
            }
            //log = "undoButton text, top, left, width, height, tooltip: " + p1 + ", " + top + ", " + left + ", " + width + ", " + height + ", " + tt;
            //LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Used to capture the Click Event
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventParam"></param>
        private void btUndoButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (this.UndoList != null)
            {
                //log = "Undo Changes clicked";
                //LoadingExtension.WriteLog(log);
                ApplyUndo();
            }
        }

        /// <summary>
        /// set up any check boxes
        /// </summary>
        /// <param name="checkbox"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void SetCheckBox(UICheckBox checkbox, int x, int y)
        {
            checkbox.isChecked = true;
            checkbox.relativePosition = new Vector3(x, y);
            checkbox.size = new Vector2(13, 13);
            checkbox.Show();
            checkbox.color = new Color32(185, 221, 254, 255);
            checkbox.enabled = true;
        }

        /// <summary>
        /// set up any dropdowns
        /// </summary>
        /// <param name="checkBox"></param>
        /// <param name="values"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="tooltip"></param>
        private void SetCheckBoxDropDown(UICheckboxDropDown checkBox, string[] values, int top, int left, int width, int height, string tooltip)
        {
            checkBox.items = values;
            checkBox.relativePosition = new Vector3(top, left);
            checkBox.size = new Vector2(width, height);
            checkBox.tooltip = tooltip;
            checkBox.Show();
            checkBox.enabled = true;
        }

        /// <summary>
        /// set up the lables
        /// </summary>
        /// <param name="label"></param>
        /// <param name="pl"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void SetLabel(UILabel label, string pl, int top, int left, int width, int height)
        {
            log = "undoButton text, top, left, width, height, tooltip: " + pl + ", " + top + ", " + left + ", " + width + ", " + height;
            //LoadingExtension.WriteLog(log);
            label.relativePosition = new Vector3(top, left);
            label.size = new Vector3(width, height); //120, 20);
            label.text = pl;
            label.textScale = 0.8f;
        }

        /// <summary>
        /// return the value of the string between the values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int ReturnInteger(string value, int min, int max)
        {
            int tmp = min;
            try
            {
                tmp = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                tmp = 0;
            }
            return Math.Min(Math.Max(tmp, min), max);
        }

        /// <summary>
        /// return the value of the string between the values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private double ReturnDouble(string value, double min, double max)
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
            return Math.Min(Math.Max(tmp, min), max);
        }

        /// <summary>
        /// is this a number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
            }
            return results;
        }
    }
}