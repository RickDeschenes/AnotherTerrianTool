using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.UI;
using UnityEngine;

namespace AnotherTerrain.Services
{
    /// <summary>
    /// Simple override class to add minimum and maximum value properties
    /// </summary>
    class UITextBox : UITextField
    {
        public enum ValueType
        {
            Textual,
            Real,
            Whole
        }
        private ValueType m_Type;

        public double minimum { get; set; }
        public double maximum { get; set; }

        private double m_Value = 80;
        public double Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                if (value < minimum)
                    m_Value = 0.01;
                if (value > maximum)
                    m_Value = 2000;
                text = m_Value.ToString();
            }
        }

        public override void Awake()
        {
            minimum = double.MinValue + 1;
            maximum = double.MinValue - 1;

            eventTextChanged += UITextBox_eventTextChanged;
            base.Awake();
        }

        private void UITextBox_eventTextChanged(UIComponent component, string value)
        {
            if (numericalOnly == true)
            {
                if (m_Type == ValueType.Real)
                {
                    //Floatee
                }
                if (m_Type == ValueType.Whole)
                {
                    //Integer
                }
            }
        }

        public void SetTextBox(string text, int top, int left, int width, int height, string tooltip, Vector2 minmax, ValueType valueType)
        {
            this.relativePosition = new Vector3(top, left);
            this.size = new Vector3(100, 20);
            this.text = text;
            this.width = width;
            this.height = height;
            this.tooltip = tooltip;
            this.numericalOnly = (valueType != ValueType.Textual);
            m_Type = valueType;
            this.minimum = minmax.x;
            this.minimum = minmax.y;

            this.textScale = 0.8f;
            this.color = Color.black;
            this.cursorBlinkTime = 0.45f;
            this.cursorWidth = 1;
            this.horizontalAlignment = UIHorizontalAlignment.Left;
            this.selectionBackgroundColor = new Color(233, 201, 148, 255);
            this.selectionSprite = "EmptySprite";
            this.verticalAlignment = UIVerticalAlignment.Middle;
            this.padding = new RectOffset(5, 0, 5, 0);
            this.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            this.normalBgSprite = "TextFieldPanel";
            this.hoveredBgSprite = "TextFieldPanelHovered";
            this.focusedBgSprite = "TextFieldPanel";
            this.isInteractive = true;
            this.enabled = true;
            this.readOnly = false;
            this.builtinKeyNavigation = true;
        }
    }
}
