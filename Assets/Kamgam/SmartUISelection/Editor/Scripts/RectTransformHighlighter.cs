using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kamgam.SmartUISelection
{
    public static class RectTransformHighlighter
    {
        private static IList<RectTransform> _rectsToHighlight;
        
        /// <summary>
        /// Draws an outline around each RectTransform in the list.
        /// </summary>
        /// <param name="rects"></param>
        public static void Highlight(IList<RectTransform> rects)
        {
            if (rects == null || rects.Count == 0) return;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            _rectsToHighlight = rects;
        }

        public static void Clear()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            _rectsToHighlight = null;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (_rectsToHighlight == null) return;

            Handles.color = Settings.highlightColor;

            foreach (var rectTransform in _rectsToHighlight)
            {
                if (rectTransform == null) continue;

                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                Handles.DrawAAPolyLine(Settings.highlightThickness, new Vector3[]
                {
                    corners[0], corners[1], 
                    corners[2], corners[3], 
                    corners[0]
                });
            }

            SceneView.RepaintAll();
        }
    }

}