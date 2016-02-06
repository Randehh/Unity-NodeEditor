using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rondo.NodeEditor.System {
    public static class NodeUtils {

        /// <summary>
        /// Draws a curve on the screen from one rectangle to the other
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static void DrawNodeCurve(Rect start, Rect end) {
            Vector3 startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
            Vector3 endPos = new Vector3(end.x, end.y + end.height / 2, 0);
            Vector3 startTan = startPos + Vector3.right * 100;
            Vector3 endTan = endPos + Vector3.left * 100;
            Color shadowCol = new Color(0, 0, 0, 0.06f);

            for (int i = 0; i < 3; i++) {// Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
        }

        /// <summary>
        /// Creates a texture with just a single color
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Texture2D ColorToTex(Color col) {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(1, 1, col);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Snaps a rectangle to another rectangle
        /// </summary>
        /// <param name="rect">The rectangle to snap</param>
        /// <param name="snapTo">The rectangle to snap to</param>
        public static void SnapRectToRect(Rect rect, Rect snapTo) {

        }
    }
}
