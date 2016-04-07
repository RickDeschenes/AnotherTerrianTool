using System;

namespace AnotherTerrain.Services
{
    [Serializable]
    public class UserPreferences
    {
        const int min = 0;
        const int max = 1080;

        private double m_TerrainHeight = 50;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public double TerrainHeight
        {
            get { return m_TerrainHeight; }
            set { m_TerrainHeight = value; }
        }

        private int m_StartX = 0;
        /// <summary>
        /// Min and Max set to 0, 1080
        /// </summary>
        public int StartX
        {
            get { return m_StartX; }
            set { m_StartX = Math.Max(Math.Min(value,max),min); }
        }

        private int m_StartZ = 0;
        /// <summary>
        /// Min and Max set to 0, 1080
        /// </summary>
        public int StartZ
        {
            get { return m_StartZ; }
            set { m_StartZ = Math.Max(Math.Min(value, max), min); }
        }
        private int m_EndX = 1080;
        /// <summary>
        /// Min and Max set to 0, 1080
        /// </summary>
        public int EndX
        {
            get { return m_EndX; }
            set { m_EndX = Math.Max(Math.Min(value, max), min); }
        }
        private int m_EndZ = 1080;
        /// <summary>
        /// Min and Max set to 0, 1080
        /// </summary>
        public int EndZ
        {
            get { return m_EndZ; }
            set { m_EndZ = Math.Max(Math.Min(value, max), min); }
        }

        private int m_SettingsTop = 1;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int SettingsTop
        {
            get { return m_SettingsTop; }
            set { m_SettingsTop = value; }
        }

        private int m_SettingsLeft = 1;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int SettingsLeft
        {
            get { return m_SettingsLeft; }
            set { m_SettingsLeft = value; }
        }

        private int m_SettingsHeight = 350;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int SettingsHeight
        {
            get { return m_SettingsHeight; }
            set { m_SettingsHeight = value; }
        }

        private int m_SettingsWidth = 250;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int SettingsWidth
        {
            get { return m_SettingsWidth; }
            set { m_SettingsWidth = value; }
        }

        private Patterns m_Pattern = Patterns.Empty;
        /// <summary>
        /// Last selected Pattern
        /// </summary>
        public Patterns TerrainPattern
        {
            get { return m_Pattern; }
            set { m_Pattern = value; }
        }
    }
}
