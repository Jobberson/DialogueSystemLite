using System.Collections;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#4CAF50")]
    [NodeWidth(200)]
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Start")]
    public sealed class StartNode : DialogueNode
    {
        [Output]
        public int next;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            runtime.SetNext(GetNextFromPort("next"));
            yield break;
        }
    }
}