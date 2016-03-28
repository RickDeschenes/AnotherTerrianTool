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
using AnotherTerrain.Services;

namespace AnotherTerrain
{
    /// <summary>
    /// Show a setting and undo panel and allow selected mouse drag area to be updates
    /// </summary>
    /// <Details>
    /// Allow the user to select an area
    /// Update that area to the desires changes
    /// Handle one or more back offs (revert changes)
    /// Public Objects:
    ///     Mode, like the Map Editor Modes this is used to control the selected area 
    ///     example: (Square, future: Elipsis, Star, Triangle)
    ///     m_atlas used to control rendered area and selection
    /// Private Objects
    ///     m_mode, local instance of Mode
    ///     m_dataLock used to keep the mouse selection active during rendering
    ///     Vector3 object, stored vectors for mouse event captures
    ///     m_undoBuffer, used to store sets of heights
    ///     m_backupHeights, used to store the current height back up
    ///     m_rawHeights, used to store the new heights and apply them
    ///     m_maxArea, used to limit the selection
    ///     --m-maxHeight, used to limit the height of the selection
    ///     --m_maxWidth, used to limit the width of the selection
    ///     mainButton, the Main user button
    ///     UIButton
    /// Public procedures
    ///     InitGui - used to set up and display the User button
    ///         also to create the tools main User interface
    /// </Details>
    public class AnotherTerrainTool : DefaultTool
    {
        #region "Public Declarations"
        public enum Modes
        {
            Square,
            Ellipses,
            Triangle,
            Star,
            Empty
        }

        public UITextureAtlas m_atlas;
        #endregion

        #region "Private Declarations"
        private Modes m_mode;

        private object m_dataLock = new object();

        private GameObject buildingWindowGameObject;
        private UIButton btSquare;
        private SettingsPanel settingsPanel;

        private bool m_active;

        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;

        readonly ushort[] m_undoBuffer = Singleton<TerrainManager>.instance.UndoBuffer;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        private int m_maxArea = 8000;
        private string log;

        //private int m_maxheight = 800;
        //private int m_maxWith = 800;

        private static Vector3 SnapToTerrain(Vector3 mouse)
        {
            return new Vector3(Mathf.RoundToInt(mouse.x / 16f), 0f, Mathf.RoundToInt(mouse.z / 16f)) * 16f;
        }

        private static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            return ScreenToTerrain ? coords / 16f + 1080 / 2 : (coords - 1080 / 2) * 16f;
        }

        private Vector3 ConvertCoords(Vector3 Pos, bool ScreenToTerrain = true)
        {
            return new Vector3
            {
                x = ConvertCoords(Pos.x, ScreenToTerrain),
                z = ConvertCoords(Pos.z, ScreenToTerrain)
            };
        }
        #endregion

        #region "Class Controls"
        protected override void Awake()
        {
            m_active = false;
            base.Awake();
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            DebugOutputPanel.Show();
            this.settingsPanel.Show();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            DebugOutputPanel.Hide();
            this.settingsPanel.Hide();
            base.OnDisable();
        }

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
        #endregion

        #region "Public Procedures"
        /// <summary>
        /// Used to set up and display the User button
        ///         also to create the tools main User Interface
        /// </summary>
        /// <param name="mode"></param>
        public void InitGui(LoadMode mode)
        {
            //log = "";
            //Make the button
            UIComponent tsBar = UIView.GetAView().FindUIComponent("TSBar");

            if (btSquare == null)
            {
                //load our button
                btSquare = tsBar.AddUIComponent<UIButton>();
                InitButton(btSquare, "Square", 1, "Square Tool");

                //load our setting panel
                buildingWindowGameObject = new GameObject("buildingWindowObject");

                var view = UIView.GetAView();

                this.settingsPanel = buildingWindowGameObject.AddComponent<SettingsPanel>();
                this.settingsPanel.transform.parent = view.transform;
                this.settingsPanel.isVisible = true;
                this.settingsPanel.canFocus = true;
                this.settingsPanel.isInteractive = true;
                //this.settingsPanel.width = 250;
                //this.settingsPanel.height = 250;
                this.settingsPanel.position = new Vector3(btSquare.position.x - 300, btSquare.position.y - 300); //new Vector3(Mathf.Floor((this.settingsPanel.GetUIView().fixedWidth - this.settingsPanel.width) / 2), Mathf.Floor((this.settingsPanel.GetUIView().fixedHeight - this.settingsPanel.height) / 2));
                this.settingsPanel.MoveCompleted += SettingsPanel_MoveCompleted;

                this.settingsPanel.Hide();
            }
        }

        private void SettingsPanel_MoveCompleted(float x, float y)
        {
            //we want to store the panels possition;
            log = "fired Settings Panel MoveCompleted: " + x + ", " + y;
            LoadingExtension.WriteLog(log);
        }
        #endregion

        #region "Private Procedures"
        private void InitButton(UIButton button, string Name, int position, string tooltip)
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

            UIComponent refButton = UIView.GetAView().FindUIComponent<UIMultiStateButton>("BulldozerButton");
            button.relativePosition = new Vector2
                (
                    refButton.relativePosition.x + refButton.width / 2.0f - button.width * position - refButton.width - 8.0f,
                    refButton.relativePosition.y + refButton.height / 2.0f - button.height / 2.0f
                );
        }

        private void ToggleTerraform(UIComponent component, UIMouseEventParameter eventParam)
        {
            log = "Left ToggleTerraform as Mode: ";

            //Set our status
            if (enabled == true)
            {
                m_mode = Modes.Empty;
                enabled = false;
                this.settingsPanel.isVisible = true;
                this.settingsPanel.BringToFront();
                this.settingsPanel.Hide();
                log += "Disabled";
            }
            else
            {
                m_mode = Modes.Square;
                enabled = true;
                this.settingsPanel.isVisible = false;
                this.settingsPanel.Show();
                log += "Enabled";
            }
            LoadingExtension.WriteLog(log);
        }

        private void ToggleMode()
        {
            //switch (component.cachedName)
            //{
            //    case "Square":
            //        m_mode = AnotherTerrainTool.Mode.Square;
            //        break;
            //    case "Ellipses":
            //        m_myMode = AnotherTerrainTool.MyMode.Ellipses;
            //        break;
            //    case "Triangle":
            //        m_myMode = AnotherTerrainTool.MyMode.Triangle;
            //        break;
            //    case "Star":
            //        m_myMode = AnotherTerrainTool.MyMode.Star;
            //        break;
            //    default:
            //        break;
            //}
        }

        private UIButton addButton(UIPanel panel, int yPos, string text)
        {
            UIButton label = panel.AddUIComponent<UIButton>();
            label.relativePosition = new Vector3(1, yPos);
            label.height = 20;
            label.width = 80;
            label.text = text;
            return label;
        }
        #endregion

        #region "Area Selection"
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

        #region "Apply Changes"
        private void ApplyTerrainChange()
        {
            //Figure out where to send this
            if (m_mode == Modes.Square)
            {
                //Finally make the call to update the entire area with the new height
                ApplyBrush();
            }
        }

        /// <summary>
        /// Used to make sure we can have our select be any direction, backwards, forwards, up, or down
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        private void GetMinMax(out int minX, out int minZ, out int maxX, out int maxZ)
        {
            //we need to store the mouse positions
            float startx = m_startPosition.x;
            float startz = m_startPosition.z;
            float endx = m_endPosition.x;
            float endz = m_endPosition.z;

            //we need the min and max coordinates
            float min = 1;
            float max = 1080;

            string coords = startx + " X " + endx + " : " + startz + " X " + endz;
            log = "ApplyBrush: startMouse.x X endMouse.x : startMouse.z X endMouse.z = " + coords;
            LoadingExtension.WriteLog(log);

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

            coords = startx + " X " + endx + " : " + startz + " X " + endz;
            log = "ApplyBrush: startMouse.x X endMouse.x : startMouse.z X endMouse.z = " + coords;
            LoadingExtension.WriteLog(log);
        }

        private void ApplyBrush()
        {
            ushort finalHeight = 80;
            int minX;
            int minZ;
            int maxX;
            int maxZ;

            GetMinMax(out minX, out minZ, out maxX, out maxZ);

            //we need to make sure that this was not a mouse click event
            if (maxZ - minZ >= 2 && maxX - minX > 2)
            {
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        //we need to set the height
                        m_rawHeights[i * 1081 + j] = finalHeight;
                        //Turning this on kills performence
                        //LoadingExtension.WriteLog("i X j = " + i + " X " + j);
                    }
                }
                TerrainModify.UpdateArea(minX - 1, minZ - 1, maxX + 1, maxZ + 1, true, true, false);
                log = "Exiting ApplyBrush";
                LoadingExtension.WriteLog(log);
            }
        }
        #endregion
    }
}