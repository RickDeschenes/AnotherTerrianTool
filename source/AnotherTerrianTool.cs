using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Threading;
using ColossalFramework.Math;
using ICities;

namespace AnotherTerrain
{
    /// <summary>
    /// We will allow the user to select an area
    /// we will update that area to the desires changes
    /// We will handle one back off
    /// We will have a menu form for the options
    /// </summary>
    public class AnotherTerrainTool : DefaultTool
    {
        public enum Mode
        {
            Shift,
            Level,
            Slope,
            Point,
            Square
        }

        private Mode m_mode;

        private object m_dataLock = new object();

        private bool m_active;

        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;

        private Quad3 m_quad;

        readonly ushort[] m_undoBuffer = Singleton<TerrainManager>.instance.UndoBuffer;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        public float m_maxArea = 400f;

        public UIButton mainButton;
        public UITextureAtlas m_atlas;

        private UIPanel marqueeTerrianPanel;
        private UIButton btSquare;
        private UITextField ipHeight;
        //private UICheckBox cbSolid;
        private UIButton btUndo;
        //private UICheckboxDropDown cbHeights;

        protected override void Awake()
        {
            //this.m_dataLock = new object();
            m_active = false;
            base.Awake();
        }

        void InitGui()
        {
            marqueeTerrianPanel = UIView.GetAView().FindUIComponent("TSBar").AddUIComponent<UIPanel>();
            marqueeTerrianPanel.backgroundSprite = "SubcategoriesPanel";
            marqueeTerrianPanel.isVisible = false;
            marqueeTerrianPanel.name = "MarqueeTerrianPanel";
            marqueeTerrianPanel.size = new Vector2(250, 250);

            UIComponent refButton = UIView.GetAView().FindUIComponent("BulldozerButton");

            marqueeTerrianPanel.relativePosition = new Vector2
            (
                refButton.relativePosition.x + refButton.width / 2.0f - marqueeTerrianPanel.width,
                refButton.relativePosition.y - marqueeTerrianPanel.height
            );
            marqueeTerrianPanel.isVisible = true;

            int top = 1;
            addUILabel(marqueeTerrianPanel, top, "Lines");
            ipHeight = addTextField(marqueeTerrianPanel, 20, "Height");
            addUILabel(marqueeTerrianPanel, top, "UnDo");
            btUndo = addButton(marqueeTerrianPanel, 20, "UnDo");

        }

        void InitButton(UIButton button, string Name, int position, string tooltip)
        {
            button.width = 60;
            button.height = 41;

            button.cachedName = Name;
            button.tooltip = tooltip;
            button.playAudioEvents = true;

            button.normalBgSprite = Name;
            button.disabledBgSprite = Name + "Disabled";
            button.hoveredBgSprite = Name + "Hovered";
            button.focusedBgSprite = Name + "Focused";
            button.pressedBgSprite = Name + "Pressed";

            button.atlas = m_atlas;
            button.eventClick += ToggleTerraform;

            UIComponent refButton = UIView.GetAView().FindUIComponent("BulldozerButton");

            button.relativePosition = new Vector2
                (
                    refButton.relativePosition.x + refButton.width / 2.0f - button.width * position - refButton.width - 8.0f,
                    refButton.relativePosition.y + refButton.height / 2.0f - button.height / 2.0f
                );
        }

        public void CreateButtons()
        {
            UIComponent tsBar = UIView.GetAView().FindUIComponent("TSBar");
            if (btSquare == null)
            {
                btSquare = tsBar.AddUIComponent<UIButton>();
                InitButton(btSquare, "Square", 1, "Square Tool");
            }
        }

        private void ToggleTerraform(UIComponent component, UIMouseEventParameter eventParam)
        {
            enabled = false;
            //WriteLog("entering select mode");
            switch (component.cachedName)
            {
                case "Square":
                    m_mode = AnotherTerrainTool.Mode.Square;
                    enabled = true;
                    break;
                    //case "Ellipses":
                    //    m_myMode = AnotherTerrainTool.MyMode.Ellipses;
                    //    break;
                    //case "Triangle":
                    //    m_myMode = AnotherTerrainTool.MyMode.Triangle;
                    //    break;
                    //case "Star":
                    //    m_myMode = AnotherTerrainTool.MyMode.Star;
                    //    break;
            }
        }

        void buttonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            marqueeTerrianPanel.isVisible = true;
            this.enabled = true;
        }

        private void addUILabel(UIPanel panel, int yPos, string text)
        {
            UILabel label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(1, yPos);
            label.height = 0;
            label.width = 80;
            label.text = text;
        }

        private UIButton addButton(UIPanel panel, int yPos, string text)
        {
            UIButton label = panel.AddUIComponent<UIButton>();
            label.relativePosition = new Vector3(1, yPos);
            label.height = 0;
            label.width = 80;
            label.text = text;
            return label;
        }

        private UITextField addTextField(UIPanel panel, int yPos, string text)
        {
            UITextField label = panel.AddUIComponent<UITextField>();
            label.relativePosition = new Vector3(1, yPos);
            label.height = 0;
            label.width = 80;
            label.text = text;
            return label;
        }

        private UICheckBox addCheckbox(UIPanel panel, int yPos, string text)
        {
            var checkBox = marqueeTerrianPanel.AddUIComponent<UICheckBox>();
            checkBox.relativePosition = new Vector3(20, yPos);
            checkBox.height = 0;
            checkBox.width = 80;

            var label = marqueeTerrianPanel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(45, yPos + 3);
            checkBox.label = label;
            checkBox.text = text;
            UISprite uncheckSprite = checkBox.AddUIComponent<UISprite>();
            uncheckSprite.height = 20;
            uncheckSprite.width = 20;
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";
            uncheckSprite.isVisible = true;

            UISprite checkSprite = checkBox.AddUIComponent<UISprite>();
            checkSprite.height = 20;
            checkSprite.width = 20;
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            checkBox.checkedBoxObject = checkSprite;
            //make sure the test is gramatically correct
            string txt = " will be deleted.";
            if (text == "Bridge" || text == "Ground" || text == "Tunnel")
                txt = " items" + txt;
            checkBox.tooltip = String.Format("If checked {0} {1}", text.ToLower(), txt);
            label.tooltip = checkBox.tooltip;
            checkBox.isChecked = true;

            return checkBox;
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            DebugOutputPanel.Show();
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            DebugOutputPanel.Hide();
            base.OnDisable();
        }

        #region "Selection"
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Vector3 startPosition;
            Vector3 mousePosition;
            Vector3 startDirection;
            Vector3 mouseDirection;
            bool active;

            try
            {
                active = this.m_active;

                startPosition = this.m_startPosition;
                mousePosition = this.m_mousePosition;
                startDirection = this.m_startDirection;
                mouseDirection = this.m_mouseDirection;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            var color = Color.red;

            if (!active)
            {
                base.RenderOverlay(cameraInfo);
                return;
            }

            Vector3 a = (!active) ? mousePosition : startPosition;
            Vector3 vector = mousePosition;
            Vector3 a2 = (!active) ? mouseDirection : startDirection;
            Vector3 a3 = new Vector3(a2.z, 0f, -a2.x);

            float num = Mathf.Round(((vector.x - a.x) * a2.x + (vector.z - a.z) * a2.z) * 0.125f) * 8f;
            float num2 = Mathf.Round(((vector.x - a.x) * a3.x + (vector.z - a.z) * a3.z) * 0.125f) * 8f;

            float num3 = (num < 0f) ? -4f : 4f;
            float num4 = (num2 < 0f) ? -4f : 4f;

            Quad3 quad = default(Quad3);
            quad.a = a - a2 * num3 - a3 * num4;
            quad.b = a - a2 * num3 + a3 * (num2 + num4);
            quad.c = a + a2 * (num + num3) + a3 * (num2 + num4);
            quad.d = a + a2 * (num + num3) - a3 * num4;

            if (num3 != num4)
            {
                Vector3 b = quad.b;
                quad.b = quad.d;
                quad.d = b;
            }

            //Store the selected area
            m_quad = quad;

            ToolManager toolManager = ToolManager.instance;
            toolManager.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, -1f, 1025f, false, true);
            base.RenderOverlay(cameraInfo);
            return;
        }

        protected override void OnToolLateUpdate()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 cameraDirection = Vector3.Cross(Camera.main.transform.right, Vector3.up);
            cameraDirection.Normalize();
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                this.m_mouseRay = Camera.main.ScreenPointToRay(mousePosition);
                this.m_mouseRayLength = Camera.main.farClipPlane;
                this.m_cameraDirection = cameraDirection;
                this.m_mouseRayValid = (!this.m_toolController.IsInsideUI && Cursor.visible);
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }
        }

        public override void SimulationStep()
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Ray mouseRay;
            Vector3 cameraDirection;
            bool mouseRayValid;
            try
            {
                mouseRay = this.m_mouseRay;
                cameraDirection = this.m_cameraDirection;
                mouseRayValid = this.m_mouseRayValid;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, m_mouseRayLength);
            ToolBase.RaycastOutput raycastOutput;
            if (mouseRayValid && ToolBase.RayCast(input, out raycastOutput))
            {
                if (!m_active)
                {

                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        this.m_mouseDirection = cameraDirection;
                        this.m_mousePosition = raycastOutput.m_hitPos;

                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }

                }
                else
                {
                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        if (checkMaxArea(raycastOutput.m_hitPos))
                        {
                            this.m_mousePosition = raycastOutput.m_hitPos;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }
                }

            }
        }

        private bool checkMaxArea(Vector3 newMousePosition)
        {
            if ((m_startPosition - newMousePosition).sqrMagnitude > m_maxArea * 5000)
            {
                return false;
            }
            return true;
        }
        #endregion

        protected override void OnToolGUI(Event e)
        {
            if (e.type == EventType.MouseDown && m_mouseRayValid)
            {
                if (e.button == 0)
                {
                    m_active = true;
                    this.m_startPosition = this.m_mousePosition;
                    this.m_startDirection = Vector3.forward;
                }
                if (e.button == 1)
                {
                    m_active = false;
                }
            }
            else if (e.type == EventType.MouseUp && m_active)
            {
                if (e.button == 0)
                {
                    this.m_endPosition = this.m_mousePosition;
                    ApplyTerrainChange();
                    m_active = false;
                }
            }
        }

        private void ApplyTerrainChange()
        {
            //Figure out where to send this
            if (m_mode == AnotherTerrainTool.Mode.Square)
            {
                //Finally make the call to update the entire area with the new height
                ApplyBrush();
            }
        }

        static Vector3 SnapToTerrain(Vector3 mouse)
        {
            return new Vector3(Mathf.RoundToInt(mouse.x / 16f), 0f, Mathf.RoundToInt(mouse.z / 16f)) * 16f;
        }

        static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            return ScreenToTerrain ? coords / 16f + 1080 / 2 : (coords - 1080 / 2) * 16f;
        }

        Vector3 ConvertCoords(Vector3 Pos, bool ScreenToTerrain = true)
        {
            return new Vector3
            {
                x = ConvertCoords(Pos.x, ScreenToTerrain),
                z = ConvertCoords(Pos.z, ScreenToTerrain)
            };
        }

        //void GetBrushBounds(out int minX, out int minZ, out int maxX, out int maxZ, bool screenPos = false)
        //{
        //    minX = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_mousePosition.x)), 1);
        //    minZ = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_mousePosition.z)), 1);
        //    maxX = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.x)), 1080 - 1);
        //    maxZ = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.z)), 1080 - 1);
        //    if (screenPos)
        //    {
        //        minX = (int)ConvertCoords(minX, false);
        //        minZ = (int)ConvertCoords(minZ, false);
        //        maxX = (int)ConvertCoords(maxX, false);
        //        maxZ = (int)ConvertCoords(maxZ, false);
        //    }
        //}

        void GetMinMax(out int minX, out int minZ, out int maxX, out int maxZ)
        {
            //we need to store the mouse positions
            float startx = m_startPosition.x;
            float startz = m_startPosition.z;
            float endx = m_endPosition.x;
            float endz = m_endPosition.z;

            //we need the min and max coordinates
            float min = 1;
            float max = 1080;

            //string coords = startx + " X " + endx + " : " + startz + " X " + endz;
            //string log = "ApplyBrush: startMouse.x X endMouse.x : startMouse.z X endMouse.z = " + coords;
            //WriteLog(log);
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, log);

            //Get the smaller X into startx and larger into endx
            if (startx > endx)
            {
                minX = (int)Mathf.Max(ConvertCoords(endx), min);
                maxX = (int)Mathf.Min(ConvertCoords(startx), max);
            }
            else
            { 
                minX = (int)Mathf.Max(ConvertCoords(startx), min);
                maxX = (int)Mathf.Min(ConvertCoords(endx), max);
            }
            //Get the smaller Z into startz and larger into endz
            if (startz > endz)
            {
                minZ = (int)Mathf.Max(ConvertCoords(endz), min);
                maxZ = (int)Mathf.Min(ConvertCoords(startz), max);
            }
            else
            {
                minZ = (int)Mathf.Max(ConvertCoords(startz), min);
                maxZ = (int)Mathf.Min(ConvertCoords(endz), max);
            }

            //coords = startx + " X " + endx + " : " + startz + " X " + endz;
            //log = "ApplyBrush: startMouse.x X endMouse.x : startMouse.z X endMouse.z = " + coords;
            //WriteLog(log);
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, log);
        }

        void ApplyBrush()
        {
            ushort finalHeight = 80;
            int minX;
            int minZ;
            int maxX;
            int maxZ;

            GetMinMax(out minX, out minZ, out maxX, out maxZ);

            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    //we need to set the height
                    m_rawHeights[i * 1081 + j] = finalHeight;
                    //WriteLog("i X j = " + i + " X " + j);
                }
            }
            TerrainModify.UpdateArea(minX - 1, minZ - 1, maxX + 1, maxZ + 1, true, true, false);
            WriteLog("Exiting ApplyBrush");
        }

        public static void WriteLog(string logMessage)
        {
            using (StreamWriter w = File.AppendText("AnotherTerrainTool.txt"))
            {
                w.WriteLine(logMessage);
            }
        }
    }
}
