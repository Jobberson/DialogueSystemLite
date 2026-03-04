using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [CreateNodeMenu("Snog/DialogueSystem/Lite/Choice")]
    public sealed class ChoiceNode : DialogueNode
    {
        [SerializeField] private LockedChoiceDisplay lockedChoiceDisplay = LockedChoiceDisplay.Hide;

        [SerializeField] private string lockedSuffix = " (Locked)";

        [SerializeField][Output(dynamicPortList = true)] private List<ChoiceOption> options = new List<ChoiceOption>();

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

            DialogueNode next = GetNextFromDynamicPort("options", originalIndex);
            runtime.SetNext(next);
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