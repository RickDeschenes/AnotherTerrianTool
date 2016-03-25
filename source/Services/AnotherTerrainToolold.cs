using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;

namespace AnotherTerrain
{
    public class AnotherTerrainToolOld : DefaultTool
    {
        public enum MyMode
        {
            Shift,
            Level,
            Slope,
            Point,
            Square
        }

        struct ToolSettings
        {
            public ToolSettings(float brushSize, float strength)
            {
                m_strength = strength;
                m_brushSize = brushSize;
            }
            public float m_brushSize;
            public float m_strength;
        }

        public UITextureAtlas m_atlas;

        Dictionary<AnotherTerrainToolOld.MyMode, AnotherTerrainToolOld.ToolSettings> ModeSettings;
        float m_brushSize = 1f;
        float m_strength = 1f;

        private MyMode m_myMode = new MyMode();

        private object m_dataLock = new object();
        
        private bool m_active;
        //private bool m_strokeInProgress;

        private Vector3 m_startPosition;
        private Vector3 m_startDirection;
        //private Vector3 m_endPosition;
        //private Vector3 m_endDirection;
        private new Vector3 m_mousePosition;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;

        public List<ushort> segmentsToDelete;

        public Texture2D m_brush_circular;
        public Texture2D m_brush_square;
        public float m_maxArea = 400f;

        public UIButton mainButton;
        public UIPanel marqueeAnotherTerrainPanel;

        private UIButton btSquare;
        
        public string[] Modes = { "Ellipses", "Squares", "Triangle", "Star" };
        
        protected override void Awake()
        {
            base.Awake();
            ModeSettings = new Dictionary<AnotherTerrainToolOld.MyMode, AnotherTerrainToolOld.ToolSettings>();
            ModeSettings[AnotherTerrainToolOld.MyMode.Square] = new AnotherTerrainToolOld.ToolSettings(24, 0.1f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TerrainManager.instance.TransparentWater = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Singleton<TerrainManager>.instance.TransparentWater = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        void GetBrushBounds(out int minX, out int minZ, out int maxX, out int maxZ, bool screenPos = false)
        {
            float brushRadius = m_brushSize / 2;
            minX = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_mousePosition.x - brushRadius)), 1);
            minZ = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_mousePosition.z - brushRadius)), 1);
            maxX = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.x + brushRadius)), 1080 - 1);
            maxZ = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.z + brushRadius)), 1080 - 1);
            if (screenPos)
            {
                minX = (int)ConvertCoords(minX, false);
                minZ = (int)ConvertCoords(minZ, false);
                maxX = (int)ConvertCoords(maxX, false);
                maxZ = (int)ConvertCoords(maxZ, false);
            }
        }
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
            ToolManager toolManager = ToolManager.instance;
            toolManager.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, -1f, 1025f, false, true);
            base.RenderOverlay(cameraInfo);

            string log = "We are leaving RenderOverlay.";
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, log);

            return;
        }

        private void ProcessTerrainUpdate()
        {
            DebugOutputPanel.Show();

            try
            {
                int minX = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_startPosition.x)), 1);
                int minZ = Mathf.Max(Mathf.CeilToInt(ConvertCoords(m_startPosition.y)), 1);
                int maxX = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.x)), 1080 - 1);
                int maxZ = Mathf.Min(Mathf.FloorToInt(ConvertCoords(m_mousePosition.y)), 1080 - 1);

                string log = "About to update area " + minX + "x" + minZ + " by " + maxX + "x" + maxZ;

                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, log);
                WriteLog(log);

                ushort[] heights = Singleton<TerrainManager>.instance.RawHeights;
                for (int i = minX; i < minZ; ++i)
                {
                    for (int j = maxX; j < maxZ; ++j)
                    {
                        int index = i * 1081 + j;
                        heights[index] = 0;
                    }
                }

                //write the update
                TerrainModify.UpdateArea(minX, minZ, maxX, maxZ, true, true, false);

            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, e.Message);
                WriteLog("Error in ProcessTerrainUpdate: " + e.Message);
            }
        }

        private IEnumerator LoadHeightMap(byte[] map)
        {
            Singleton<TerrainManager>.instance.SetRawHeightMap(map);
            yield return null;
        }

        private void ToggleTerraform(UIComponent component, UIMouseEventParameter eventParam)
        {
            //WriteLog("entering select mode");
            switch (component.cachedName)
            {
                case "Square":
                    m_myMode = AnotherTerrainToolOld.MyMode.Square;
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
            enabled = true;
            ApplySettings();
        }

        public void ApplySettings()
        {
            m_strength = ModeSettings[m_myMode].m_strength;
            m_brushSize = ModeSettings[m_myMode].m_brushSize;
        }

        void UpdateSettings()
        {
            //What are the setting
            WriteLog("Brush settings brush and strenght are:" + m_brushSize + ":" + m_strength);
            ModeSettings[m_myMode] = new AnotherTerrainToolOld.ToolSettings(m_brushSize, m_strength);
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

        protected override void OnToolGUI(Event e)
        {
            m_mousePosition = Input.mousePosition;
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                {
                    m_active = true;
                    this.m_startPosition = Input.mousePosition;
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
                    string tm = Modes[(int)m_myMode];
                    WriteLog("We are now ready to make the updates. We are in mode " + tm);
                    if (m_myMode == MyMode.Square)
                    {
                        ProcessTerrainUpdate();
                    }
                    m_active = false;
                }
            }
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
