using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#2196F3")]
    [NodeWidth(230)]
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Line")]
    public sealed class LineNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never)]
        public int input;

        public string speakerName;

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

            LineUIOptions options = new LineUIOptions(speedMultiplier, speakerName);

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