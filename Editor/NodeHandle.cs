using Rondo.NodeEditor.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.NodeEditor.Editor {
    public class NodeHandle {

        //Displaying
        public Rect rect;
        private Texture handleTex;
        private HandleOrientation orientation = HandleOrientation.RIGHT;
        private Vector2 position = Vector2.zero;
        private float offset = 0;

        //Info
        public BaseNode attachedTo;

        /// <summary>
        /// A node handle to which connections attach
        /// </summary>
        public NodeHandle(BaseNode attachedTo, HandleOrientation startingOrientation) {
            handleTex = AssetDatabase.LoadAssetAtPath("Assets/NodeEditor/Editor/Images/Handle.png", typeof(Texture)) as Texture;

            this.attachedTo = attachedTo;
            this.orientation = startingOrientation;

            CalculatePosition();
        }

        /// <summary>
        /// Draw the handle
        /// </summary>
        public virtual void DrawHandle() {
            rect = new Rect(attachedTo.rect.center + position, new Vector2(handleTex.width, handleTex.height));
            GUI.DrawTexture(rect, handleTex);
        }


        /// <summary>
        /// Change where the handle is drawn
        /// </summary>
        public void Move(Vector2 mousePos) {
            Vector2 spacing = mousePos - attachedTo.rect.center;
            Vector2 scaledSpacing = new Vector2(spacing.x / attachedTo.rect.width, spacing.y / attachedTo.rect.height);

            if(Mathf.Abs(scaledSpacing.x) > Mathf.Abs(scaledSpacing.y)) {
                if (spacing.x < 0) {
                    orientation = HandleOrientation.LEFT;
                } else {
                    orientation = HandleOrientation.RIGHT;
                }
                offset = spacing.y;
            } else {
                if(spacing.y < 0) {
                    orientation = HandleOrientation.TOP;
                } else {
                    orientation = HandleOrientation.BOTTOM;
                }
                offset = spacing.x;
            }

            CalculatePosition();
        }

        /// <summary>
        /// Calculate the new offset position of the handle depending on the orientation and offset
        /// </summary>
        private void CalculatePosition() {
            Rect nodeRect = attachedTo.rect;
            float widthHalf = attachedTo.rect.width / 2;
            float heightHalf = attachedTo.rect.height / 2;
            switch (orientation) {
                case HandleOrientation.TOP:
                    position = new Vector2(Mathf.Clamp(offset, -widthHalf, widthHalf - handleTex.width), -nodeRect.height / 2 - rect.height);
                    break;

                case HandleOrientation.BOTTOM:
                    position = new Vector2(Mathf.Clamp(offset, -widthHalf, widthHalf - handleTex.width), nodeRect.height / 2);
                    break;

                case HandleOrientation.LEFT:
                    position = new Vector2(-nodeRect.width / 2 - rect.width, Mathf.Clamp(offset, -heightHalf, heightHalf - handleTex.height));
                    break;

                case HandleOrientation.RIGHT:
                    position = new Vector2(nodeRect.width / 2, Mathf.Clamp(offset, -heightHalf, heightHalf - handleTex.height));
                    break;
            }
        }

        /// <summary>
        /// On which side the handle is orientated
        /// </summary>
        public enum HandleOrientation {
            LEFT,
            RIGHT,
            TOP,
            BOTTOM
        }
    }
}
