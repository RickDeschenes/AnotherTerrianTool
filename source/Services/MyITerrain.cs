using ICities;
using System;
using UnityEngine;

namespace AnotherTerrain.Services
{
    public class MyITerrain : TerrainExtensionBase
    {
        private static ITerrain mTerrain;

        public override void OnCreated(ITerrain terrain)
        {
            mTerrain = terrain;
            base.OnCreated(terrain);
        }

        public float cellSize
        {
            get
            {
                return 0;
            }
        }

        public int heightMapResolution
        {
            get
            {
                return 0;
            }
        }

        public void GetHeights(int heightX, int heightZ, int heightWidth, int heightLength, ushort[] rawHeights)
        {
            mTerrain.GetHeights(heightX, heightZ, heightWidth, heightLength, rawHeights);
        }

        public void HeightMapCoordToPosition(int heightX, int heightZ, out float x, out float z)
        {
            mTerrain.HeightMapCoordToPosition(heightX, heightZ, out x, out z);
        }

        public ushort HeightToRaw(float height)
        {
            return mTerrain.HeightToRaw(height);
        }

        public void PositionToHeightMapCoord(float x, float z, out int heightX, out int heightZ)
        {
            mTerrain.PositionToHeightMapCoord(x, z, out heightX, out heightZ);
        }

        public float RawToHeight(ushort rawHeight)
        {
            return mTerrain.RawToHeight(rawHeight);
        }

        public float SampleTerrainHeight(float x, float z)
        {
            return mTerrain.SampleTerrainHeight(x, z);
        }

        public float SampleWaterHeight(float x, float z)
        {
            return mTerrain.SampleWaterHeight(x,  z);
        }

        public void SetHeights(int heightX, int heightZ, int heightWidth, int heightLength, ushort[] rawHeights)
        {
            mTerrain.SetHeights(heightX,  heightZ,  heightWidth,  heightLength,  rawHeights);
        }
    }
}
