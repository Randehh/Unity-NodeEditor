using UnityEngine;
using System.Collections;

namespace Rondo.NodeEditor.Base {
    public class NodeConnection {

        private BaseNode fromNode;
        private BaseNode toNode;

        public NodeConnection(BaseNode fromNode, BaseNode toNode) {
            this.fromNode = fromNode;
            this.toNode = toNode;
        }

        public BaseNode GetFrom() {
            return fromNode;
        }

        public BaseNode GetTo() {
            return toNode;
        }
    }
}
