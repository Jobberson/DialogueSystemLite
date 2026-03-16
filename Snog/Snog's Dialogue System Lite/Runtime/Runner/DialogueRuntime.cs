using UnityEngine;

namespace SnogDialogue.Runtime
{
    public sealed class DialogueRuntime
    {
        public DialogueContext Context
        {
            get;
        }

        public IDialogueUI UI
        {
            get;
        }

        public DialogueNode NextNode
        {
            get;
            private set;
        }

        public DialogueRuntime(DialogueContext context, IDialogueUI ui)
        {
            Context = context;
            UI = ui;
        }

        public void SetNext(DialogueNode node)
        {
            NextNode = node;
        }

        public void ClearNext()
        {
            NextNode = null;
        }
    }
}