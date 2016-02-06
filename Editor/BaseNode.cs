using Assets.NodeEditor.Editor;
using Rondo.NodeEditor.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rondo.NodeEditor.Base {
    public class BaseNode {
        public Rect rect;

        //Connections
        private List<NodeConnection> connections = new List<NodeConnection>();
        private bool isDirty = true;

        public string windowTag = "Node Name";

        //Size
        protected int height = 0;
        protected int width = 450;

        //Others
        public bool isDraggable = true;

        public BaseNode(Rect rect) {
            this.rect = rect;
            Resize();
        }

        public virtual void DrawNode() {
            windowTag = EditorGUILayout.TextField("Tag", windowTag);
            height = 35;
        }

        public virtual void DrawHandles() {
            foreach (NodeConnection c in GetConnections()) {
                if (c.GetFromNode() == this) continue;
                DrawConnectionCurve(c);
                DrawHandle(c.GetFromHandle());
                DrawHandle(c.GetToHandle());
            }
        }

        private void DrawConnectionCurve(NodeConnection c) {
            Rect r1;
            Rect r2;

            if(c.GetFromHandle() != null) {
                r1 = c.GetFromHandle().rect;
            } else {
                r1 = c.GetFromNode().rect;
            }

            if(c.GetToHandle() != null) {
                r2 = c.GetToHandle().rect;
            } else {
                r2 = c.GetToNode().rect;
            }

            NodeUtils.DrawNodeCurve(r1, r2);
        }

        private void DrawHandle(NodeHandle handle) {
            if (handle == null) return;
            handle.DrawHandle();
        }

        public void Move(Vector2 v, bool moveChildren) {
            rect.position += v;

            if (moveChildren) {
                MoveChildren(v);
            }
        }

        public void Resize() {
            rect.height = height;
            rect.width = width;
        }

        public void AddConnection(NodeConnection connection) {
            if (connections.Contains(connection)) return;
            connections.Add(connection);
        }

        public void RemoveConnection(NodeConnection connection) {
            if (!connections.Contains(connection)) return;
            connections.Remove(connection);
        }

        public List<NodeConnection> GetConnections() {
            return connections;
        }

        public void MoveChildren(Vector2 v) {
            foreach (BaseNode c in GetChildNodes()) {
                c.Move(v, true);
            }
        }

        public int Organize() {
            if (!isDirty) return 0;

            isDirty = false;

            List<BaseNode> cNodes = GetChildNodes();

            //Total height first
            int totalHeight = 0;
            foreach (BaseNode n in cNodes) {
                if (n == cNodes[0]) continue;
                totalHeight += (int)n.rect.height;
            }

            totalHeight = (int)(totalHeight * 1.5f);
            int currentHeight = 0;
            int lowestGlobalPoint = int.MinValue;
            int lowestBranchPoint = 0;
            foreach (BaseNode n in cNodes) {
                n.rect.position = rect.position + new Vector2(width * 1.5f, currentHeight - (totalHeight / 2) + lowestBranchPoint);
                currentHeight += (int)(n.height * 1.5f);

                if (n.rect.position.y + n.rect.height > lowestGlobalPoint) {
                    lowestGlobalPoint = (int)(n.rect.position.y + n.rect.height);
                }

                int lPoint = (int)n.rect.position.y - n.Organize();
                if (lPoint > lowestBranchPoint) {
                    lPoint = lowestBranchPoint;
                }
            }
            return lowestGlobalPoint;
        }

        public List<BaseNode> GetChildNodes() {
            List<BaseNode> cNodes = new List<BaseNode>();
            foreach (NodeConnection c in GetConnections()) {
                if (c.GetToNode() == null ||
                    c.GetToNode() == this)
                    continue;
                cNodes.Add(c.GetToNode());
            }
            return cNodes;
        }
    }
}
