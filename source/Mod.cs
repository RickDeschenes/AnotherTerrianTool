using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Drawing;
using UnityEngine;

namespace AnotherTerrain
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get { return "Another Terrain Tool SoftwareByRAD"; }
        }

        public string Name
        {
            get { return "Another Terrain Tool"; }
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public AnotherTerrainTool buildTool;
        public UITextureAtlas terraform_atlas;

        public static ICities.LoadMode mode;

        public void LoadResources()
        {
            string[] spriteNames = {
                "Square",
                "SquareDisabled",
                "SquareFocused",
                "SquareHovered",
                "SquarePressed",
                "Ellipses",
                "EllipsesDisabled",
                "EllipsesFocused",
                "EllipsesHovered",
                "EllipsesPressed",
                "Triangle",
                "TriangleDisabled",
                "TriangleFocused",
                "TriangleHovered",
                "TrianglePressed",
                "Star",
                "StarDisabled",
                "StarFocused",
                "StarHovered",
                "StarPressed"
            };

            terraform_atlas = CreateTextureAtlas("spritesheet.png", "TerraformUI", UIView.GetAView().defaultAtlas.material, 60, 41, spriteNames);
        }

        static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, Material baseMaterial, int spriteWidth, int spriteHeight, string[] spriteNames)
        {
            var tex = new Texture2D(spriteWidth * spriteNames.Length, spriteHeight, TextureFormat.ARGB32, false);
            tex.filterMode = FilterMode.Bilinear;

            WriteLog("About to load loadTextureFromAssembly.");
            tex = loadTextureFromAssembly(textureFile, false);
            WriteLog("Loaded loadTextureFromAssembly.");

            UITextureAtlas atlas = ScriptableObject.CreateInstance<UITextureAtlas>();

            { // Setup atlas
                Material material = UnityEngine.Object.Instantiate(baseMaterial);
                material.mainTexture = tex;

                atlas.material = material;
                atlas.name = atlasName;
            }

            // Add sprites
            for (int i = 0; i < spriteNames.Length; ++i)
            {
                float uw = 1.0f / spriteNames.Length;

                var spriteInfo = new UITextureAtlas.SpriteInfo
                {
                    name = spriteNames[i],
                    texture = tex,
                    region = new Rect(i * uw, 0, uw, 1),
                };

                atlas.AddSprite(spriteInfo);
            }
            return atlas;
        }

        static Texture2D loadTextureFromAssembly(string textureFile, bool readOnly = true)
        {
            WriteLog("entering loadTextureFromAssembly.");
            //Bitmap img = Properties.Resources.spritesheet;
            //byte[] buf = ImageToByte(img);
            //WriteLog("Converted to byte Array");

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            WriteLog("loaded Assembly." + assembly.GetName().Name);
            System.IO.Stream textureStream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + textureFile);

            var buf = new byte[textureStream.Length];  //declare arraysize
            textureStream.Read(buf, 0, buf.Length); // read from stream to byte array

            WriteLog("loaded Image.");
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(buf);
            tex.Apply(false, readOnly);
            return tex;
        }

        private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset || mode == LoadMode.NewMap || mode == LoadMode.LoadMap)
            {
                try
                {
                    LoadResources();
                    if (buildTool == null)
                    {
                        File.Delete("AnotherTerrainTool.Log");
                        GameObject gameController = GameObject.FindWithTag("GameController");
                        buildTool = gameController.AddComponent<AnotherTerrainTool>();
                        buildTool.m_atlas = terraform_atlas;
                        buildTool.InitGui(mode);
                        buildTool.enabled = false;
                    }
                }
                catch (Exception e)
                {
                    WriteLog(e.ToString());
                }
            }
        }

        public static void WriteLog(string log)
        {
            WriteLog(log, false);
        }

        internal static void WriteLog(string log, bool DebugOnly)
        {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, log);
            if (DebugOnly)
                return;
            using (StreamWriter w = File.AppendText("AnotherTerrainTool.Log"))
            {
                w.WriteLine(log);
            }
        }
    }
}
