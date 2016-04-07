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
        private ushort[] m_originalHeights;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        SavedInputKey m_UndoKey = new SavedInputKey(Settings.mapEditorTerrainUndo, Settings.inputSettingsFile, DefaultSettings.mapEditorTerrainUndo, true);
        
        private int m_minX = 0;
        private int m_maxX = 0;
        private int m_minZ = 0;
        private int m_maxZ = 0;

        private string log;

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
            ////only show this while not released
            //DebugOutputPanel.Show();

            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;

            //setting up our backup
            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;
            m_originalHeights = new ushort[m_rawHeights.Length];

            for (int i = 0; i <= 1080; i++)
            {
                for (int j = 0; j <= 1080; j++)
                {
                    int num = i * 1081 + j;
                    m_backupHeights[num] = m_rawHeights[num];
                    m_originalHeights[num] = m_rawHeights[num];
                }
            }
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

            if (!m_active && m_UndoKey.IsPressed(current) && sp.ListCount())
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
                
                this.sp.transform.parent = view.transform;
                this.sp.isVisible = true;
                this.sp.canFocus = true;
                this.sp.isInteractive = true;
                this.sp.MoveCompleted += MoveCompleted;
                this.sp.ApplyUndo += Sp_ApplyUndo;
                this.sp.Hide();
            }
        }

        private void Sp_ApplyUndo()
        {
            if (!m_active && sp.ListCount())
            {
                ApplyUndo();
            }
        }

        #endregion

        #region "Private Procedures"

        /// <summary>
        /// Used to log that the SettingPanel move completed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void MoveCompleted(float x, float y)
        {
            //we want to store the panels possition;
            log = "fired Settings Panel MoveCompleted: " + x + ", " + y;
           // LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// Set up the button
        /// </summary>
        /// <param name="button"></param>
        /// <param name="Name"></param>
        /// <param name="position"></param>
        /// <param name="tooltip"></param>
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

        /// <summary>
        /// Toggle the tool on and off
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventParam"></param>
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

        /// <summary>
        /// once we update the shape shifter
        /// this will allow for different shapes to be updated
        /// </summary>
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

        /// <summary>
        /// Add the button
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="yPos"></param>
        /// <param name="text"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Call to call a call
        /// check if we are in the mode
        /// </summary>
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
        /// Used to make sure we can have our selected area
        /// and the be any direction, backwards, forwards, up, or down
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        private void GetMinMax(out int minX, out int minZ, out int maxX, out int maxZ)
        {
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

            //Get the smaller X into startx and larger into endx
            //Also not less than 0 or more then 1080
            if (startx > endx)
            {
                minX = (int)Mathf.Min(Mathf.Max(endx, min),max);
                maxX = (int)Mathf.Max(Mathf.Min(startx, max),min);
            }
            else
            {
                minX = (int)Mathf.Min(Mathf.Max(startx, min),max);
                maxX = (int)Mathf.Max(Mathf.Min(endx, max), min);
            }
            //Get the smaller Z into startz and larger into endz
            if (startz > endz)
            {
                minZ = (int)Mathf.Min(Mathf.Max(endz, min),max);
                maxZ = (int)Mathf.Max(Mathf.Min(startz, max), min);
            }
            else
            {
                minZ = (int)Mathf.Min(Mathf.Max(startz, min),max);
                maxZ = (int)Mathf.Max(Mathf.Min(endz, max), min);
            }
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

            //log = "GetMinMax = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            //LoadingExtension.WriteLog(log);

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
                ////Apply Update
                //TerrainModify.UpdateArea(minX, minZ, maxX, maxZ, true, false, false);
                log = "(minX, minZ) : ( maxX, maxZ): (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
                LoadingExtension.WriteLog("ApplyBrush: " + log);

                //we need to update the area in 120 point sections
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        int x1 = j;
                        int x2 = Math.Max(i + 119, maxX);
                        int z1 = i;
                        int z2 = Math.Max(j + 119, maxZ);
                        TerrainModify.UpdateArea(x1, z1, x2, z2, true, false, false);

                        log = "(x1, z1) : ( x2, z2): (" + x1 + ", " + z1 + ") : (" + x2 + ", " + z2 + ")";
                        LoadingExtension.WriteLog("ApplyBrush: " + log);
                        //make sure we exit the loop
                        if (j + 1 >= maxX)
                            break;
                        j += 119;
                        if (j > maxX)
                            j = maxX - 1;
                    }
                    //make sure we exit the loop
                    if (i + 1 >= maxZ)
                        break;
                    i += 119;
                    if (i > maxZ)
                        i = maxZ - 1;
                }

                m_minX = minX;
                m_maxX = maxX;
                m_minZ = minZ;
                m_maxZ = maxZ;

                //Store the change
                EndStroke();

                //does this redraw the screen
                transform.Translate(new Vector3(0, 0, 0));

                //string coords = minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") diff = (" + (maxX - minX) + ", " + (maxZ - minZ) + ")";
                //log = "Exiting ApplyBrush: (minX, minZ) : (maxX, maxZ) = (" + coords;
                //LoadingExtension.WriteLog(log);
            }
        }

        private void EndStroke()
        {
            log = "Entering EndStroke";
           // LoadingExtension.WriteLog(log);
            //creating the undo stroke
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

            log = "Exiting EndStroke";
           // LoadingExtension.WriteLog(log);
        }

        /// <summary>
        /// used to apply the undo
        /// revert the last terrain change
        /// restore the back up to the prior back up
        /// otherwise we overwrite the changes
        /// </summary>
        private void ApplyUndo()
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

            //log = "ApplyUndo = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            //LoadingExtension.WriteLog(log);

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

            ////Apply Undo
            //TerrainModify.UpdateArea(minX, minZ, maxX, maxZ, true, false, false);
            log = "(minX, minZ) : ( maxX, maxZ): (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            LoadingExtension.WriteLog("ApplyBrush: " + log);

            //we need to update the area in 120 point sections
            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int x1 = j;
                    int x2 = Math.Max(i + 119, maxX);
                    int z1 = i;
                    int z2 = Math.Max(j + 119, maxZ);
                    TerrainModify.UpdateArea(x1, z1, x2, z2, true, false, false);
                    log = "(x1, z1) : ( x2, z2): (" + x1 + ", " + z1 + ") : (" + x2 + ", " + z2 + ")";
                    LoadingExtension.WriteLog("ApplyUndo: " + log);
                    //make sure we exit the loop
                    if (j + 1 >= maxX)
                        break;
                    j += 119;
                    if (j > maxX)
                        j = maxX - 1;
                }
                //make sure we exit the loop
                if (i + 1 >= maxZ)
                    break;
                i += 119;
                if (i > maxZ)
                    i = maxZ - 1;
            }

            //does this redraw the screen
            transform.Translate(new Vector3(0, 0, 0));
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
                        this.m_mousePosition = raycastOutput.m_hitPos;
                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }
                }

            }
        }
        #endregion

    }
}