using System.Collections;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#F44336")]
    [NodeWidth(200)]
    [CreateNodeMenu("Snog/DialogueSystem/Lite/End")]
    public sealed class EndNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never)]
        public int input;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            runtime.UI.Hide();
            runtime.SetNext(null);
            yield break;
        }
    }
}