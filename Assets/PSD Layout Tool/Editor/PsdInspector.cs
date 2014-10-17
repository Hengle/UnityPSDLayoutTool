﻿namespace PsdLayoutTool
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A custom Inspector to allow PSD files to be turned into prefabs and separate textures per layer.
    /// </summary>
    /// <remarks>
    /// Unity isn't able to draw the custom inspector for a TextureImporter (even if calling the base
    /// method or calling DrawDefaultInspector).  It comes out as just a generic, hard to use mess of GUI
    /// items.  To add in the buttons we want without disrupting the normal GUI for TextureImporter, we have
    /// to do some reflection "magic".
    /// Thanks to DeadNinja: http://forum.unity3d.com/threads/custom-textureimporterinspector.260833/
    /// </remarks>
    [CustomEditor(typeof(TextureImporter))]
    public class PsdInspector : Editor
    {
        /// <summary>
        /// The selected TextureImporter as a <see cref="SerializedObject"/>.
        /// </summary>
        private SerializedObject serializedTarget;

        /// <summary>
        /// The native Unity editor used to render the <see cref="TextureImporter"/>'s Inspector.
        /// </summary>
        private Editor nativeEditor;

        /// <summary>
        /// Called by Unity when any Texture file is first clicked on and the Inspector is populated.
        /// </summary>
        public void OnEnable()
        {
            serializedTarget = new SerializedObject(target);

            // use reflection to get the default Inspector
            Type t = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name.ToLower().Contains("textureimporterinspector"))
                    {
                        t = type;
                        break;
                    }
                }
            }

            nativeEditor = CreateEditor(serializedTarget.targetObject, t);
        }

        /// <summary>
        /// Draws the Inspector GUI for the TextureImporter.
        /// Normal Texture files should appear as they normally do, however PSD files will have additional items.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (nativeEditor != null)
            {
                // check if it is a PSD file selected
                string assetPath = ((TextureImporter)serializedTarget.targetObject).assetPath;

                if (assetPath.EndsWith(".psd"))
                {
                    PsdImporter.MaximumDepth = EditorGUILayout.FloatField("Maximum Depth", PsdImporter.MaximumDepth);

                    PsdImporter.PixelsToUnits = EditorGUILayout.FloatField("Pixels to Unity Units", PsdImporter.PixelsToUnits);

                    // draw our custom buttons for PSD files
                    if (GUILayout.Button("Export Layers as Textures"))
                    {
                        PsdImporter.LayoutInScene = false;
                        PsdImporter.CreatePrefab = false;
                        PsdImporter.Import(assetPath);
                    }

                    if (GUILayout.Button("Layout in Current Scene"))
                    {
                        PsdImporter.LayoutInScene = true;
                        PsdImporter.CreatePrefab = false;
                        PsdImporter.Import(assetPath);
                    }

                    if (GUILayout.Button("Generate Prefab"))
                    {
                        PsdImporter.LayoutInScene = false;
                        PsdImporter.CreatePrefab = true;
                        PsdImporter.Import(assetPath);
                    }

                    EditorGUILayout.Separator();

                    // draw the default Inspector for the PSD
                    nativeEditor.OnInspectorGUI();
                }
                else 
                {
                    // It is a "normal" Texture, not a PSD
                    nativeEditor.OnInspectorGUI();
                }
            }

            // Unfortunately we cant hide the ImportedObject section because the interal InspectorWindow checks via
            // "if (editor is AssetImporterEditor)" and all flags that this check sets are method local variables
            // so aside from direct patching UnityEditor.dll, reflection cannot be used here.

            // Therefore we just move the ImportedObject section out of view
            ////GUILayout.Space(2048);
        }
    }
}