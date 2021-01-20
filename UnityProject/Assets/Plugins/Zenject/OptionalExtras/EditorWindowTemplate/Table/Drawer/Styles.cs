using UnityEditor;
using UnityEngine;

namespace Zenject.EditorWindowTemplate
{
    public sealed class Styles
    {
        public Vector2 triangleOffset = new Vector2(2.0f, 5.0f);
        
        public float scrollSpeed = 10.0f;
        public float normalColumnWidth = 128.0f;
        
        public float headerHeight = 26.0f;
        public float headerTop = 54.0f;// headerHeight + filterHeight;
        
        public float filterHeight = 28.0f;
        public float filterInputHeight = 18.0f;
        public float filterWidth = 300.0f;
        public float filterPaddingLeft = 42.0f;
        public float filterPaddingTop = 5.0f;

        public float splitterWidth = 1.0f;
        public float rowHeight = 24.0f;
        
        public Texture2D rowBackground1;
        public Texture2D rowBackground2;
        public Texture2D rowBackgroundHighlighted;
        public Texture2D rowBackgroundSelected;
        public Texture2D lineTexture;
        public Texture2D triangleUp;
        public Texture2D triangleDown;
        
        public GUIStyle filterTextStyle;
        
        public GUIStyle headerTextLeft;
        public GUIStyle headerTextCenter;
        public GUIStyle contentTextLeft;
        public GUIStyle contentTextCenter;

        private bool init;
        private bool isDarkSkin;
        
        public void Check()
        {
            if (init && EditorGUIUtility.isProSkin == isDarkSkin && lineTexture != null)
                return;

            init = true;
            isDarkSkin = EditorGUIUtility.isProSkin;

            Color colorTriangle;
            Color colorRowBackground1;
            Color colorRowBackground2;
            Color colorLine;

            if (isDarkSkin)
            {
                colorTriangle = Color.white;
                colorRowBackground1 = new Color(0.23f, 0.23f, 0.23f, 1.0f);
                colorRowBackground2 = new Color(0.21f, 0.21f, 0.21f, 1.0f);
                colorLine = new Color(0.125f, 0.125f, 0.125f, 1.0f);
            }
            else
            {
                colorTriangle = Color.black;
                colorRowBackground1 = new Color(0.85f, 0.85f, 0.85f, 1.0f);
                colorRowBackground2 = new Color(0.83f, 0.83f, 0.83f, 1.0f);
                colorLine = new Color(0.67f, 0.67f, 0.67f, 1.0f);
            }

            Texture2D texTriangleUp = LoadTexture("TriangleUp");
            Texture2D texTriangleDown = LoadTexture("TriangleDown");
            
            triangleUp = CreateColorTexture(texTriangleUp, colorTriangle);
            triangleDown = CreateColorTexture(texTriangleDown, colorTriangle);
            rowBackground1 = CreateColorTexture(colorRowBackground1);
            rowBackground2 = CreateColorTexture(colorRowBackground2);
            rowBackgroundHighlighted = CreateColorTexture(new Color(0, 0, 0, 0));
            rowBackgroundSelected = CreateColorTexture(new Color(0, 0, 0, 0));
            lineTexture = CreateColorTexture(colorLine);

            GUISkin skin = GUI.skin;
            filterTextStyle = new GUIStyle(skin.label);
            
            headerTextLeft = new GUIStyle(skin.label);
            headerTextLeft.alignment = TextAnchor.MiddleLeft;
            headerTextLeft.fontStyle = FontStyle.Bold;
            headerTextLeft.contentOffset = new Vector2(16.0f, 0.0f);
            
            headerTextCenter = new GUIStyle(skin.label);
            headerTextCenter.alignment = TextAnchor.MiddleCenter;
            headerTextCenter.fontStyle = FontStyle.Bold;

            contentTextLeft = new GUIStyle(skin.label);
            contentTextLeft.alignment = TextAnchor.MiddleLeft;
            
            contentTextCenter = new GUIStyle(skin.label);
            contentTextCenter.alignment = TextAnchor.MiddleCenter;
        }

        private static Texture2D LoadTexture(string name)
        {
            var searchInFolders = new[] { "Assets/Plugins/Zenject" };
            string[] guids = AssetDatabase.FindAssets("t:texture " + name, searchInFolders);

            for (var i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                if (texture != null)
                    return texture;
            }

            return null;
        }

        private static Texture2D CreateColorTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(1, 1, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateColorTexture(Texture2D original, Color color)
        {
            Color[] pixels = original.GetPixels();

            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] *= color;
            }
            
            var texture = new Texture2D(original.width, original.height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
