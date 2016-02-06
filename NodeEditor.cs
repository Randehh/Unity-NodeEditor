using Rondo.NodeEditor.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Rondo.NodeEditor.Base {

    /// <summary>
    /// The base of the node editor
    /// </summary>
    public class NodeEditor : EditorWindow {

        private List<BaseNode> nodes = new List<BaseNode>();
        private NodeEditorState state = new NodeEditorState();

        private GUIStyle selectionStyle = new GUIStyle();

        void OnGUI() {

            Event e = Event.current;
            state.SetMousePos(e.mousePosition);
            state.mouseOverNode = GetMouseOverNode();

            if (e.button == 0) {
                HandleLeftClick(e);
            }

            if (e.button == 1) {
                HandleRightClick(e);
            }

            BeginWindows();

            DrawWindow();

            EndWindows();
        }

        public virtual void DrawWindow() {
            DrawSelectionRectangle();
            DrawNodes();
        }

        private void DrawNodes() {
            for (int i = 0; i < nodes.Count; i++) {
                BaseNode node = nodes[i];
                node.rect = GUI.Window(i, node.rect, DrawNode, node.windowTag);

                foreach (NodeConnection c in node.GetConnections()) {
                    if (c.GetTo() == node) continue;
                    NodeUtils.DrawNodeCurve(c.GetFrom().rect, c.GetTo().rect);
                }
            }
            Repaint();
        }

        private void DrawNode(int id) {
            nodes[id].DrawNode();

            if (state.selectedNodes.Count != 0) {
                foreach(BaseNode n in state.selectedNodes) {
                    if (!n.isDraggable) continue;
                    GUI.DragWindow(n.rect);
                }
            }
        }

        private void DrawSelectionRectangle() {
            Rect r = new Rect(Vector2.zero, Vector2.zero);
            if (state.isSelecting) {
                r = new Rect(state.startSelection, state.mousePos - state.startSelection);
            }
            GUILayout.BeginArea(r, "", selectionStyle);
            GUILayout.EndArea();
            Repaint();
        }

        #region Input
        private void HandleLeftClick(Event e) {
            if (e.type == EventType.MouseDown) {
                if (state.mouseOverNode == null) {
                    state.isSelecting = true;
                    state.selectedNodes.Clear();
                    state.startSelection = e.mousePosition;
                } else {
                    state.isDraggingNodes = true;
                    if (!state.selectedNodes.Contains(state.mouseOverNode)) {
                        //Start dragging on new node
                        state.selectedNodes.Clear();
                        state.selectedNodes.Add(state.mouseOverNode);
                    }
                }
            } else if (e.type == EventType.MouseDrag) {
                if (state.isDraggingNodes) {
                    //Drag nodes
                    foreach (BaseNode n in state.selectedNodes) {
                        n.Move(e.delta, e.shift);
                    }
                    Repaint();
                }
            } else if (e.type == EventType.MouseUp) {
                if (state.isSelecting) {

                    //Grab selection
                    state.selectedNodes.Clear();
                    Rect selectionRect = new Rect(state.startSelection, e.mousePosition - state.startSelection);
                    foreach (BaseNode n in nodes) {
                        if (selectionRect.Overlaps(n.rect, true)) {
                            state.selectedNodes.Add(n);
                            //Debug.Log("Intersection with " + n.windowTag);
                        }
                    }
                }

                //Reset
                state.isSelecting = false;
                state.isDraggingNodes = false;
            }
        }

        private void HandleRightClick(Event e) {
            BaseNode mouseOverNode = GetMouseOverNode();

            if (e.type == EventType.MouseDown) {
                if (!state.isDraggingWindow) {
                    state.startRightClick = e.mousePosition;
                }
            } else if (e.type == EventType.MouseDrag) {
                if (state.isDraggingWindow) {
                    OffsetNodes(e.delta);
                } else if (Vector2.Distance(state.startRightClick, e.mousePosition) >= 10) {
                    state.isDraggingWindow = true;
                }
            } else if (e.type == EventType.MouseUp) {
                if (!state.isDraggingWindow) {
                    if (state.selectedNodes.Count == 1) {
                        //Single node right click
                        GetContextMenu(state.selectedNodes[0]).ShowAsContext();
                    } else if (state.selectedNodes.Count >= 2) {
                        //Multi select, handle as single for now...
                        GetContextMenu(GetMouseOverNode()).ShowAsContext();
                    }
                }

                //Reset
                state.isDraggingWindow = false;
            }
        }
        #endregion Input

        /// <summary>
        /// Gets the context menu of the current mouse pos. If no node is moused over, parameter will be null
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual GenericMenu GetContextMenu(BaseNode n) {
            GenericMenu menu = new GenericMenu();
            return menu;
        }

        /// <summary>
        /// Add a node to the editor
        /// </summary>
        /// <param name="node"></param>
        public virtual void AddNode(BaseNode node) {
            if (nodes.Contains(node)) return;
            nodes.Add(node);
        }

        /// <summary>
        /// Remove a node from the editor
        /// </summary>
        /// <param name="node"></param>
        public virtual void RemoveNode(BaseNode node) {
            if (!nodes.Contains(node)) return;
            nodes.Remove(node);
        }

        /// <summary>
        /// Removes all nodes from the screen
        /// </summary>
        public virtual void ClearNodes() {
            nodes.Clear();
        }

        public virtual NodeConnection ConnectNodes(BaseNode from, BaseNode to) {
            if (from == null ||
                to == null)
                return null;
            NodeConnection c = new NodeConnection(from, to);
            from.AddConnection(c);
            to.AddConnection(c);
            return c;
        }

        /// <summary>
        /// Gets the node over which the mouse currently is
        /// </summary>
        /// <returns></returns>
        public virtual BaseNode GetMouseOverNode() {
            foreach (BaseNode n in nodes) {
                if (n.rect.Contains(state.mousePos)) {
                    return n;
                }
            }
            return null;
        }

        /// <summary>
        /// Offsets all nodes on the screen
        /// </summary>
        /// <param name="v"></param>
        public virtual void OffsetNodes(Vector2 v) {
            foreach (BaseNode n in nodes) {
                n.Move(v, false);
            }
            Repaint();
        }

        /// <summary>
        /// Gets a node by tag. Returns null if no node is found
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual BaseNode GetNode(string tag) {
            foreach (BaseNode node in nodes) {
                if (node.windowTag == tag) return node;
            }
            return null;
        }

        /// <summary>
        /// Attempts to organize all nodes on the screen by their heirarchy
        /// </summary>
        public virtual void OrganizeNodes() {
            int heightOffset = 0;
            foreach (BaseNode node in state.startNodes) {
                if (node == null) continue;
                node.rect.position += new Vector2(0, heightOffset + 150);
                heightOffset = node.Organize();
            }
        }

        /// <summary>
        /// Calculates the starting nodes based on connections, which is accessable via GetState()
        /// </summary>
        public void CalculateStartNodes() {
            Debug.Log("Getting start nodes from " + nodes.Count + " nodes");
            state.startNodes.Clear();
            foreach (BaseNode n in nodes) {
                int inConnections = 0;
                foreach (NodeConnection c in n.GetConnections()) {
                    if (c.GetTo() == n) {
                        inConnections++;
                    }
                }
                Debug.Log("In connections to " + n.windowTag + "= " + inConnections);
                if (inConnections == 0) {
                    Debug.Log("Start node: " + n.windowTag);
                    state.startNodes.Add(n);
                }
            }
        }

        /// <summary>
        /// Get the editor state which contains most frame by frame information
        /// </summary>
        /// <returns></returns>
        public virtual NodeEditorState GetEditorState() {
            return state;
        }

        /// <summary>
        /// Called when the window is opened. Load any information the node editor requires here
        /// </summary>
        public void PrepareWindow() {
            selectionStyle = new GUIStyle();
            selectionStyle.normal.background = NodeUtils.ColorToTex(new Color(1, 1, 1, 0.5f));
        }
    }
}