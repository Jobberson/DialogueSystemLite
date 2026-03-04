using System.Collections;

namespace SnogDialogue.Runtime
{
    [CreateNodeMenu("Snog/DialogueSystem/Lite/End")]
    public sealed class EndNode : DialogueNode
    {
        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            runtime.UI.Hide();
            runtime.SetNext(null);
            yield break;
        }
    }
}