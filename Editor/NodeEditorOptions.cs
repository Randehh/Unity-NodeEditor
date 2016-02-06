using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rondo.NodeEditor.System {
    public class NodeEditorOptions {
        public NodeHandleMode nodeHandleMode = NodeHandleMode.IN_OUT;
    }

    public enum NodeHandleMode {
        IN_OUT,
        IN,
        OUT,
        NONE
    }
}
