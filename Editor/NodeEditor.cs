using Assets.NodeEditor.Editor;
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

        public static NodeEditor instance;

        private List<BaseNode> nodes = new List<BaseNode>();
        private NodeEditorState state = new NodeEditorState();
        private NodeEditorOptions options = new NodeEditorOptions();

        private GUIStyle selectionStyle = new GUIStyle();

        void OnGUI() {

            Event e = Event.current;
            state.SetMousePos(e.mousePosition);

            UpdateMouseOverState();

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

                node.DrawHandles();
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
                if (state.mouseOverNode != null) {
                    state.isDraggingNodes = true;
                    if (!state.selectedNodes.Contains(state.mouseOverNode)) {
                        //Start dragging on new node
                        state.selectedNodes.Clear();
                        state.selectedNodes.Add(state.mouseOverNode);
                    }
                } else if (state.mouseOverHandle != null) {
                    state.isDraggingHandle = true;
                    state.selectedHandle = state.mouseOverHandle;
                } else {
                    state.isSelecting = true;
                    state.selectedNodes.Clear();
                    state.startSelection = e.mousePosition;
                }
            } else if (e.type == EventType.MouseDrag) {
                if (state.isDraggingNodes) {
                    //Drag nodes
                    foreach (BaseNode n in state.selectedNodes) {
                        n.Move(e.delta, e.shift);
                    }
                    Repaint();
                }else if (state.isDraggingHandle) {
                    state.selectedHandle.Move(state.mousePos);
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
                state.isDraggingHandle = false;
            }
        }

        private void HandleRightClick(Event e) {
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
                if (!state.isDraggingWindow && !state.isDraggingHandle) {
                    if (state.selectedNodes.Count == 1) {
                        //Single node right click
                        GetContextMenu(state.selectedNodes[0]).ShowAsContext();
                    } else if (state.selectedNodes.Count >= 2) {
                        //Multi select, handle as single for now...
                        GetContextMenu(state.mouseOverNode).ShowAsContext();
                    } else if(state.selectedHandle != null) {
                        GetContextMenu(state.mouseOverHandle).ShowAsContext();
                    }
                }

                //Reset
                state.isDraggingWindow = false;
            }
        }
        #endregion Input

        /// <summary>
        /// Gets the context menu of the current mouse over node. If no node is moused over, parameter will be null
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual GenericMenu GetContextMenu(BaseNode node) {
            GenericMenu menu = new GenericMenu();
            return menu;
        }

        /// <summary>
        /// Gets the context menu of the current mouse over handle. If no handle is moused over, parameter will be null
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual GenericMenu GetContextMenu(NodeHandle handle) {
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
        /// Updates the mouseover state for handles and nodes
        /// </summary>
        /// <returns></returns>
        public virtual void UpdateMouseOverState() {
            bool nodeFound = false;
            bool handleFound = false;

            foreach (BaseNode n in nodes) {
                if (n.rect.Contains(state.mousePos)) {
                    state.mouseOverNode = n;
                    nodeFound = true;
                }

                foreach(NodeConnection c in n.GetConnections()) {
                    if (c.GetFromHandle() != null &&
                        c.GetFromHandle().rect.Contains(state.mousePos)) {
                        state.mouseOverHandle = c.GetFromHandle();
                        handleFound = true;
                    }
                    if (c.GetToHandle() != null &&
                        c.GetToHandle().rect.Contains(state.mousePos)) {
                        state.mouseOverHandle = c.GetToHandle();
                        handleFound = true;
                    }
                }
            }

            if (!nodeFound) {
                state.mouseOverNode = null;
            }

            if (!handleFound) {
                state.mouseOverHandle = null;
            }
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
            state.startNodes.Clear();
            foreach (BaseNode n in nodes) {
                int inConnections = 0;
                foreach (NodeConnection c in n.GetConnections()) {
                    if (c.GetToNode() == n) {
                        inConnections++;
                    }
                }
                if (inConnections == 0) {
                    state.startNodes.Add(n);
                }
            }
        }

        /// <summary>
        /// Get the editor state which contains most frame by frame information
        /// </summary>
        /// <returns></returns>
        public NodeEditorState GetEditorState() {
            return state;
        }

        public NodeEditorOptions GetEditorOptions() {
            return options;
        }

        /// <summary>
        /// Called when the window is opened. Load any information the node editor requires here
        /// </summary>
        public void PrepareWindow() {
            instance = this;

            selectionStyle = new GUIStyle();
            selectionStyle.normal.background = NodeUtils.ColorToTex(new Color(1, 1, 1, 0.5f));
        }
    }
}