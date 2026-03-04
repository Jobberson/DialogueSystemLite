using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Line")]
    public sealed class LineNode : DialogueNode
    {
        [TextArea(2, 6)] public string inlineText;

        public string localizationKey;

        public string[] tags;

        [Header("Typewriter")]
        [Min(0.05f)] public float speedMultiplier = 1f;

        [Output]
        public int next;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            string textToShow = inlineText;

            if (string.IsNullOrWhiteSpace(textToShow))
            {
                textToShow = "[Missing line text]";
            }

            bool done = false;

            LineUIOptions options = new LineUIOptions(speedMultiplier);

            runtime.UI.ShowLine(textToShow, options, () =>
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