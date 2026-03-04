using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace SnogDialogue.Runtime
{
    [CreateNodeMenu("Snog Dialogue/Lite/Choice")]
    public sealed class ChoiceNode : DialogueNode
    {
        [SerializeField]
        [Output(dynamicPortList = true)]
        private List<ChoiceOption> options = new List<ChoiceOption>();

        [Output]
        public int fallback;

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            List<string> labels = new List<string>(options.Count);
            List<int> optionIndexMap = new List<int>(options.Count);

            for (int i = 0; i < options.Count; i++)
            {
                ChoiceOption option = options[i];

                bool allowed = ConditionEvaluator.EvaluateAll(option.Conditions, runtime.Context);

                if (!allowed)
                {
                    continue;
                }

                string label = option.InlineText;

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = $"Choice {i + 1}";
                }

                labels.Add(label);
                optionIndexMap.Add(i);
            }

            if (labels.Count == 0)
            {
                runtime.SetNext(GetNextFromPort("fallback"));
                yield break;
            }

            int selectedVisibleIndex = -1;

            runtime.UI.ShowChoices(labels, (index) =>
            {
                selectedVisibleIndex = index;
            });

            while (selectedVisibleIndex < 0)
            {
                yield return null;
            }

            int originalIndex = optionIndexMap[selectedVisibleIndex];

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