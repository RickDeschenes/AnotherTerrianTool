using System;
using ColossalFramework.UI;
using UnityEngine;

namespace AnotherTerrain.Services
{

    public delegate void SettingsPanelEventHandler(float x, float y);

    public class SettingsPanel : UIPanel
    {
        UILabel titleLabel;
        UITextField heightTextBox;
        UILabel heightTextBoxLabel;
        UIButton undoMapButton;

        private bool mouseDown;
        private string log;

        public event SettingsPanelEventHandler MoveCompleted;

        public override void Awake()
        {
            log = "entering SettingsPanel Awake";
            LoadingExtension.WriteLog(log);

            this.isInteractive = true;
            this.enabled = true;
            width = 300f;
            height = 300f;

            titleLabel = AddUIComponent<UILabel>();
            heightTextBox = AddUIComponent<UITextField>();
            heightTextBoxLabel = AddUIComponent<UILabel>();
            undoMapButton = AddUIComponent<UIButton>();
            base.Awake();
        }

        public override void Start()
        {
            base.Start();

            log = "entering SettingsPanel Start";
            LoadingExtension.WriteLog(log);

            relativePosition = new Vector3(1500f, 750f);
            backgroundSprite = "MenuPanel2";
            isInteractive = true;
            eventMouseDown += SettingsPanel_eventMouseDown;
            eventMouseMove += SettingsPanel_eventMouseMove;
            eventMouseUp += SettingsPanel_eventMouseUp;

            SetControl("Another Terrain Tool Options");
        }

        private void SettingsPanel_eventMouseDown(UIComponent component, UIMouseEventParameter eventParam)
        {
            log = "eventMouseDown";
            LoadingExtension.WriteLog(log);

            mouseDown = true;
        }

        private void SettingsPanel_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                //log = "eventMouseMove: " + eventParam.moveDelta.x + " X " + eventParam.moveDelta.y;
                //LoadingExtension.WriteLog(log);
                try
                {
                    // Move the top and left according to the delta amount
                    Vector3 delta = new Vector3(eventParam.moveDelta.x, eventParam.moveDelta.y);
                    //Just move the screen
                    position += delta;
                }
                catch (Exception e)
                {
                    LoadingExtension.WriteLog(e.Message);
                }
            }
        }

        private void SettingsPanel_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                mouseDown = false;
                MoveCompleted(position.x, position.y);
            }
        }

        private static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            return ScreenToTerrain ? coords / 16f + 1080 / 2 : (coords - 1080 / 2) * 16f;
        }

        private void SetControl(string title)
        {
            titleLabel.text = title;
            titleLabel.relativePosition = new Vector3(15, 15);
            titleLabel.textScale = 0.9f;
            titleLabel.size = size;

            var vertPadding = 30;
            var x = 15;
            var y = 50;

            SetLabel(heightTextBoxLabel, "New Terrain Height", x, y);
            SetTextBox(heightTextBox, "1", x + 120, y);
            y += vertPadding;

            SetButton(undoMapButton, "Undo", x, y);
            undoMapButton.tooltip = "Undo last update.";
            undoMapButton.eventClick += undoMapButton_eventClick;
            y += vertPadding + 5;
        }

        private void undoMapButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            throw new NotImplementedException();
        }

        private void SetButton(UIButton okButton, string p1, int x, int y)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(50, 18);
            okButton.relativePosition = new Vector3(x, y - 3);
            okButton.textScale = 0.8f;
        }

        private void SetCheckBox(UICheckBox pedestriansCheck, int x, int y)
        {
            pedestriansCheck.isChecked = true;
            pedestriansCheck.relativePosition = new Vector3(x, y);
            pedestriansCheck.size = new Vector2(13, 13);
            pedestriansCheck.Show();
            pedestriansCheck.color = new Color32(185, 221, 254, 255);
            pedestriansCheck.enabled = true;
        }

        private void SetTextBox(UITextField scaleTextBox, string p, int x, int y)
        {
            scaleTextBox.relativePosition = new Vector3(x, y - 4);
            scaleTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
            scaleTextBox.text = p;
            scaleTextBox.textScale = 0.8f;
            scaleTextBox.color = Color.black;
            scaleTextBox.cursorBlinkTime = 0.45f;
            scaleTextBox.cursorWidth = 1;
            scaleTextBox.selectionBackgroundColor = new Color(233, 201, 148, 255);
            scaleTextBox.selectionSprite = "EmptySprite";
            scaleTextBox.verticalAlignment = UIVerticalAlignment.Middle;
            scaleTextBox.padding = new RectOffset(5, 0, 5, 0);
            scaleTextBox.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            scaleTextBox.normalBgSprite = "TextFieldPanel";
            scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            scaleTextBox.focusedBgSprite = "TextFieldPanel";
            scaleTextBox.size = new Vector3(100, 20);
            scaleTextBox.isInteractive = true;
            scaleTextBox.enabled = true;
            scaleTextBox.readOnly = false;
            scaleTextBox.builtinKeyNavigation = true;
        }

        private void SetLabel(UILabel pedestrianLabel, string p, int x, int y)
        {
            pedestrianLabel.relativePosition = new Vector3(x, y);
            pedestrianLabel.text = p;
            pedestrianLabel.textScale = 0.8f;
            pedestrianLabel.size = new Vector3(120, 20);
        }
    }
}