using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [NodeTint("#FF9800")]
    [NodeWidth(250)]
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Choice")]
    public sealed class ChoiceNode : DialogueNode
    {
        [Input(connectionType = ConnectionType.Override, backingValue = ShowBackingValue.Never)]
        public int input;

        [SerializeField] private LockedChoiceDisplay lockedChoiceDisplay = LockedChoiceDisplay.Hide;

        [SerializeField] private string lockedSuffix = " (Locked)";

        // Data for each choice option (text, conditions, etc.)
        [SerializeField] private List<ChoiceOption> options = new List<ChoiceOption>();

        // One output port per option — kept the same size as options.
        // Uses int so ports are compatible with the int input ports on all other nodes.
        [Output(dynamicPortList = true)] private List<int> outputs = new List<int>();

        [Output] public int fallback;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            List<ChoiceUIEntry> entries = new List<ChoiceUIEntry>(options.Count);
            List<int> optionIndexMap = new List<int>(options.Count);

            bool anyInteractable = false;

            for (int i = 0; i < options.Count; i++)
            {
                ChoiceOption option = options[i];

                bool allowed = ConditionEvaluator.EvaluateAll(option.Conditions, runtime.Context);

                if (!allowed && lockedChoiceDisplay == LockedChoiceDisplay.Hide)
                {
                    continue;
                }

                string label = option.InlineText;

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = $"Choice {i + 1}";
                }

                bool interactable = allowed;

                if (!allowed && lockedChoiceDisplay == LockedChoiceDisplay.Disable)
                {
                    label += lockedSuffix;
                }

                entries.Add(new ChoiceUIEntry(label, interactable));
                optionIndexMap.Add(i);

                if (interactable)
                {
                    anyInteractable = true;
                }
            }

            if (entries.Count == 0 || !anyInteractable)
            {
                runtime.SetNext(GetNextFromPort("fallback"));
                yield break;
            }

            int selectedIndex = -1;

            runtime.UI.ShowChoices(entries, (index) =>
            {
                selectedIndex = index;
            });

            while (selectedIndex < 0)
            {
                yield return null;
            }

            int originalIndex = optionIndexMap[selectedIndex];

            DialogueNode next = GetNextFromDynamicPort("outputs", originalIndex);
            runtime.SetNext(next);
        }

        public int OptionCount => options.Count;

        public string GetOptionLabel(int index)
        {
            if (index < 0 || index >= options.Count)
            {
                return $"Option {index + 1}";
            }

            string text = options[index].InlineText;
            return string.IsNullOrWhiteSpace(text) ? $"Choice {index + 1}" : text;
        }

        private DialogueNode GetNextFromDynamicPort(string fieldName, int index)
        {
            string portName = $"{fieldName} {index}";
            NodePort port = GetOutputPort(portName);

            if (port == null)
            {
                return null;
            }

            if (!port.IsConnected)
            {
                return null;
            }

            return port.Connection.node as DialogueNode;
        }

        public enum LockedChoiceDisplay
        {
            Hide,
            Disable
        }

        [Serializable]
        public sealed class ChoiceOption
        {
            public string InlineText;
            public string LocalizationKey;
            public string[] Tags;

            public Condition[] Conditions;
        }
    }
}