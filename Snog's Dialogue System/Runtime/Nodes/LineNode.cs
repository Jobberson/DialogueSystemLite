using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Line")]
    public sealed class LineNode : DialogueNode
    {
        [TextArea(2, 6)]
        public string inlineText;

        public string localizationKey;

        public string[] tags;

        [Output]
        public int next;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            string textToShow = inlineText;

            bool done = false;

            runtime.UI.ShowLine(textToShow, () =>
            {
                done = true;
            });

            while (!done)
            {
                yield return null;
            }

            runtime.SetNext(GetNextFromPort("next"));
        }
    }
}