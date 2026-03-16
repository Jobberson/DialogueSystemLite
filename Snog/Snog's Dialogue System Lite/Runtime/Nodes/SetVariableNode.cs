using System.Collections;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#9C27B0")]
    [NodeWidth(230)]
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Set Variable")]
    public sealed class SetVariableNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never)]
        public int input;

        public VariableScope scope;

        public string key;

        public DialogueValue value;

        [Output]
        public int next;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                VariableStore store = GetTargetStore(runtime);

                store.Set(key, value);
            }

            runtime.SetNext(GetNextFromPort("next"));
            yield break;
        }

        private VariableStore GetTargetStore(DialogueRuntime runtime)
        {
            if (scope == VariableScope.Global)
            {
                return runtime.Context.GlobalVariables;
            }

            return runtime.Context.GraphVariables;
        }
    }

    public enum VariableScope
    {
        Global,
        GraphLocal
    }
}