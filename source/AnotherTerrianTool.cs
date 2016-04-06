using System;
using System.Collections.Generic;
using System.Threading;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using ColossalFramework.Math;
using ICities;
using AnotherTerrain.Services;
using System.Runtime.InteropServices;

namespace AnotherTerrain
{
    /// <summary>
    /// Show a setting panel and allow selected mouse drag areas to be updated
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
        
        private UIButton btSquare;
        private SettingsPanel sp;

        private bool m_active;

        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;

        readonly ushort[] m_undoBuffer = Singleton<TerrainManager>.instance.UndoBuffer;
        readonly ushort[] m_originalHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        SavedInputKey m_UndoKey = new SavedInputKey(Settings.mapEditorTerrainUndo, Settings.inputSettingsFile, DefaultSettings.mapEditorTerrainUndo, true);

        private int m_maxArea = 1168561;
        private int m_minX = 0;
        private int m_maxX = 0;
        private int m_minZ = 0;
        private int m_maxZ = 0;

        private string log;

        //private int m_maxheight = 800;
        //private int m_maxWith = 800;

        private static Vector3 SnapToTerrain(Vector3 mouse)
        {
            return new Vector3(Mathf.RoundToInt(mouse.x / 16f), 0f, Mathf.RoundToInt(mouse.z / 16f)) * 16f;
        }

        private static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            //float a = coords / 16f + 1080 / 2;
            //float b = (coords - 1080 / 2) * 16f;
            //string log = "float (a, b): (" + a + ", " + b + ")";
            //LoadingExtension.WriteLog(log);
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
            //m_maxArea = m_rawHeights.Length;
            //log = "Awake m_maxArea: " + m_maxArea;
            //LoadingExtension.WriteLog(log);
            //if (Singleton<LoadingManager>.exists)
            //{
            //    Singleton<LoadingManager>.instance.m_levelLoaded += OnLevelLoaded;
            //}
            base.Awake();
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            DebugOutputPanel.Show();
            this.sp.Show();
            //setting up our backup
            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;
            for (int i = 0; i <= 1080; i++)
            {
                for (int j = 0; j <= 1080; j++)
                {
                    int num = i * 1081 + j;
                    m_backupHeights[num] = m_rawHeights[num];
                }
            }
            //m_maxArea = m_rawHeights.Length;
            //log = "OnEnable m_maxArea: " + m_maxArea;
            ////LoadingExtension.WriteLog(log);
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            DebugOutputPanel.Hide();
            this.sp.Hide();
            base.OnDisable();
        }

        protected override void OnToolGUI(Event e)
        {
            Event current = Event.current;

            if (!m_active && m_UndoKey.IsPressed(current) && IsUndoAvailable())
            {
                ApplyUndo();
            }
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
                GameObject go = new GameObject("buildingWindowObject");

                var view = UIView.GetAView();

                this.sp = go.AddComponent<SettingsPanel>();
                sp.UndoList = new List<UndoStroke>();

                this.sp.transform.parent = view.transform;
                this.sp.isVisible = true;
                this.sp.canFocus = true;
                this.sp.isInteractive = true;

                this.sp.MoveCompleted += MoveCompleted;
                //this.sp.ApplyUndo += ApplyUndo;

                this.sp.Hide();
            }
        }

        private void MoveCompleted(float x, float y)
        {
            //we want to store the panels possition;
            log = "fired Settings Panel MoveCompleted: " + x + ", " + y;
            LoadingExtension.WriteLog(log);
        }

        ///// <summary>
        ///// Used to reset the buffer during the leveling ingame event
        ///// </summary>
        ///// <param name="mode"></param>
        //public void OnLevelLoaded(SimulationManager.UpdateMode mode)
        //{
        //    log = "Entering OnLevelLoaded";
        //    //LoadingExtension.WriteLog(log);
        //    ResetUndoBuffer();
        //    log = "Leaving OnLevelLoaded";
        //    //LoadingExtension.WriteLog(log);
        //}
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
            string log = "Left ToggleTerraform as Mode: ";

            //Set our status
            if (enabled == true)
            {
                m_mode = Modes.Empty;
                enabled = false;
                this.sp.isVisible = true;
                this.sp.BringToFront();
                this.sp.Hide();
                log += "Disabled";
            }
            else
            {
                m_mode = Modes.Square;
                enabled = true;
                this.sp.isVisible = false;
                this.sp.Show();
                log += "Enabled";
            }
            //LoadingExtension.WriteLog(log);
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
            //Vector3 startm = m_startPosition;
            //Vector3 endm = m_endPosition;


            //get the terrain coords
            Vector3 startm = ConvertCoords(m_startPosition, true);
            Vector3 endm = ConvertCoords(m_endPosition, true);

            //Load the values
            float startx = startm.x;
            float startz = startm.z;
            float endx = endm.x;
            float endz = endm.z;

            //we need the min and max coordinates
            float min = 0;
            float max = 1080;

            //string coords = startx + " X " + endx + " : " + startz + " X " + endz;
            //log = "enter GetMinMax: startMouse.x X endMouse.x : startMouse.z X endMouse.z = " + coords;
            //LoadingExtension.WriteLog(log);

            //Get the smaller X into startx and larger into endx
            if (startx > endx)
            {
                minX = (int)Mathf.Max(endx, min);
                maxX = (int)Mathf.Min(startx, max);
            }
            else
            {
                minX = (int)Mathf.Max(startx, min);
                maxX = (int)Mathf.Min(endx, max);
            }
            //Get the smaller Z into startz and larger into endz
            if (startz > endz)
            {
                minZ = (int)Mathf.Max(endz, min);
                maxZ = (int)Mathf.Min(startz, max);
            }
            else
            {
                minZ = (int)Mathf.Max(startz, min);
                maxZ = (int)Mathf.Min(endz, max);
            }

            //coords = minX + " X " + maxX + " : " + minZ + " X " + maxZ;
            //log = "exit GetMinMax: minX X emaxX : minZ X maxZ = " + coords;
            //LoadingExtension.WriteLog(log);
        }

        private void ApplyBrush()
        {            
            ushort finalHeight = 500;
            MyITerrain mTerrain = new MyITerrain();
            finalHeight = mTerrain.HeightToRaw((float)sp.up.TerrainHeight);

            int minX;
            int minZ;
            int maxX;
            int maxZ;

            GetMinMax(out minX, out minZ, out maxX, out maxZ);

            //we need to make sure that this was not a mouse click event
            if (maxZ - minZ >= 1 && maxX - minX >= 1)
            {
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        int num = i * 1081 + j;
                        //We want the prior backup in the 'original'
                        m_originalHeights[num] = m_backupHeights[num];
                        //We want the current in the back up
                        m_backupHeights[num] = m_rawHeights[num];
                        //We want the new height in the new/raw
                        m_rawHeights[num] = finalHeight;
                    }
                }
                //TerrainModify.UpdateArea(minX - 1, minZ - 1, maxX + 1, maxZ + 1, true, false, false);

                //we need to update the area in 120 point sections
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        TerrainModify.UpdateArea(j, i, Math.Max(j + 120, maxZ), Math.Max(i + 120, maxX), true, true, false);
                        //log = j + ", " + i + ":" + Math.Max(j + 120, maxZ) + ", " + Math.Max(i + 120, maxX);
                        //LoadingExtension.WriteLog("Processing: " + log);
                        j += 119;
                    }
                    i += 119;
                }

                m_minX = minX;
                m_maxX = maxX;
                m_minZ = minZ;
                m_maxZ = maxZ;

                //Store the change
                EndStroke();

                //string coords = minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") diff = (" + (maxX - minX) + ", " + (maxZ - minZ) + ")";
                //log = "Exiting ApplyBrush: (minX, minZ) : (maxX, maxZ) = (" + coords;
                //LoadingExtension.WriteLog(log);
            }
        }

        void EndStroke()
        {
            ////creating the undo stroke
            //log = "Entering EndStroke, count: " + sp.UndoList.Count;
            //LoadingExtension.WriteLog(log);

            UndoStroke item = default(UndoStroke);
            item.name = "undo: " + sp.UndoList.Count;
            item.minX = m_minX;
            item.maxX = m_maxX;
            item.minZ = m_minZ;
            item.maxZ = m_maxZ;
            item.pointer = sp.UndoList.Count;
            item.rawHeights = m_rawHeights;
            item.backupHeights = m_backupHeights;
            item.originalHeights = m_originalHeights;

            sp.UndoList.Add(item);

            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;

            //log = "Leaving EndStroke, count: " + sp.UndoList.Count;
            //LoadingExtension.WriteLog(log);
        }

        public bool IsUndoAvailable()
        {
            return sp.UndoList != null && sp.UndoList.Count > 0;
        }

        /// <summary>
        /// used to apply the undo
        /// revert the last terrain change
        /// restore the back up to the prior back up
        /// otherwise we overwrite the changes
        /// </summary>
        public void ApplyUndo()
        {
            if (sp.UndoList.Count < 1)
            {
                return;
            }
            //remove the current changes from the list (there are none)
            UndoStroke undoStroke = sp.UndoList[sp.UndoList.Count - 1];
            sp.UndoList.RemoveAt(sp.UndoList.Count - 1);

            int minX = undoStroke.minX;
            int maxX = undoStroke.maxX;
            int minZ = undoStroke.minZ;
            int maxZ = undoStroke.maxZ;
            int pointer = undoStroke.pointer;

            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int num = i * 1081 + j;
                    //we want the new/raw to be the back up (un do)
                    m_rawHeights[num] = undoStroke.backupHeights[num];
                    //we want the prior backup to match the original (original as in one step back)
                    m_backupHeights[num] = undoStroke.originalHeights[num];
                }
            }

            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;

            //TerrainModify.UpdateArea(minX - 1, minZ - 1, maxX + 1, maxZ + 1, true, false, false);
            //we need to update the area in 120 point sections
            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    TerrainModify.UpdateArea(j, i, Math.Max(j + 120, maxZ), Math.Max(i + 120, maxX), true, true, false);
                    //log = j + ", " + i + ":" + Math.Max(j + 120, maxZ) + ", " + Math.Max(i + 120, maxX);
                    //LoadingExtension.WriteLog("Processing: " + log);
                    j += 119;
                }
                i += 119;
            }

            string coords = minX + " X " + maxX + " : " + minZ + " X " + maxZ;
            log = "Exiting ApplyUndo: minX X maxX : minZ X maxZ = " + coords;
            //LoadingExtension.WriteLog(log);
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
            //if ((SnapToTerrain(m_startPosition) - SnapToTerrain(newMousePosition)).sqrMagnitude > m_maxArea)
            //{
            //    return false;
            //}
            return true;
        }
        #endregion

    }
}