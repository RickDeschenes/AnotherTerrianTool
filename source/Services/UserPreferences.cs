using System;

namespace AnotherTerrain.Services
{
    [Serializable]
    public class UserPreferences
    {
        private double m_TerrainHeight = 50;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public double TerrainHeight
        {
            get { return m_TerrainHeight; }
            set { m_TerrainHeight = value; }
        }

        private int m_MaxArea = 8000;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int MaxArea
        {
            get { return m_MaxArea; }
            set { m_MaxArea = value; }
        }

        private int m_MaxHeight = 800;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int MaxHeight
        {
            get { return m_MaxHeight; }
            set { m_MaxHeight = value; }
        }
        private int m_MaxWidth = 800;
        /// <summary>
        /// Min and Max set in UITextBox
        /// </summary>
        public int MaxWidth
        {
            get { return m_MaxWidth; }
            set { m_MaxWidth = value; }
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
