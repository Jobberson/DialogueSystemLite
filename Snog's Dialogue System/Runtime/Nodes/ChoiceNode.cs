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
        [SerializeField]
        [Output(dynamicPortList = true)]
        private List<ChoiceOption> options = new List<ChoiceOption>();

        public override IEnumerator Execute(DialogueRuntime runtime)
        {
            List<string> labels = new List<string>(options.Count);

            for (int i = 0; i < options.Count; i++)
            {
                string label = options[i].InlineText;

                if (string.IsNullOrWhiteSpace(label))
                {
                    label = $"Choice {i + 1}";
                }

                labels.Add(label);
            }

            int selectedIndex = -1;

            runtime.UI.ShowChoices(labels, (index) =>
            {
                selectedIndex = index;
            });

            while (selectedIndex < 0)
            {
                yield return null;
            }

            DialogueNode next = GetNextFromDynamicPort("options", selectedIndex);
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
        }
    }
}