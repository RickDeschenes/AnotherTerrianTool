using ICities;
using System;

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
                throw new NotImplementedException();
            }
        }

        public int heightMapResolution
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        //public IManagers managers
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public void GetHeights(int heightX, int heightZ, int heightWidth, int heightLength, ushort[] rawHeights)
        {
            throw new NotImplementedException();
        }

        public void HeightMapCoordToPosition(int heightX, int heightZ, out float x, out float z)
        {
            throw new NotImplementedException();
        }

        public ushort HeightToRaw(float height)
        {
            return mTerrain.HeightToRaw(height);
        }

        public void PositionToHeightMapCoord(float x, float z, out int heightX, out int heightZ)
        {
            throw new NotImplementedException();
        }

        public float RawToHeight(ushort rawHeight)
        {
            return mTerrain.RawToHeight(rawHeight);
        }

        public float SampleTerrainHeight(float x, float z)
        {
            throw new NotImplementedException();
        }

        public float SampleWaterHeight(float x, float z)
        {
            throw new NotImplementedException();
        }

        public void SetHeights(int heightX, int heightZ, int heightWidth, int heightLength, ushort[] rawHeights)
        {
            throw new NotImplementedException();
        }
    }
}
