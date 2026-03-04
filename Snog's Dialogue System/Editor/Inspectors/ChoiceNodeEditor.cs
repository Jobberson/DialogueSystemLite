using System.Text;
using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeEditor : UnityEditor.Editor
    {
        private Label summaryLabel;

        public override VisualElement CreateInspectorGUI()
        {
            SerializedProperty lockedDisplayProp = serializedObject.FindProperty("lockedChoiceDisplay");
            SerializedProperty lockedSuffixProp = serializedObject.FindProperty("lockedSuffix");
            SerializedProperty optionsProp = serializedObject.FindProperty("options");
            SerializedProperty fallbackProp = serializedObject.FindProperty("fallback");

            VisualElement root = new VisualElement();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 4;

            Label header = new Label("Choice Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            root.Add(header);

            HelpBox info = new HelpBox(
                "Lite: Options support conditions (Global + Graph-local). Locked options can be hidden or shown disabled.",
                HelpBoxMessageType.Info
            );

            root.Add(info);

            HelpBox warning = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warning.style.display = DisplayStyle.None;
            root.Add(warning);

            Foldout behaviorFoldout = new Foldout
            {
                text = "Behavior",
                value = true
            };

            PropertyField lockedDisplayField = new PropertyField(lockedDisplayProp, "Locked Choice Display");
            PropertyField lockedSuffixField = new PropertyField(lockedSuffixProp, "Locked Suffix");

            behaviorFoldout.Add(lockedDisplayField);
            behaviorFoldout.Add(lockedSuffixField);
            root.Add(behaviorFoldout);

            Foldout summaryFoldout = new Foldout
            {
                text = "Options Summary",
                value = true
            };

            summaryLabel = new Label();
            summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryLabel.style.unityTextAlign = TextAnchor.UpperLeft;

            summaryFoldout.Add(summaryLabel);
            root.Add(summaryFoldout);

            Foldout optionsFoldout = new Foldout
            {
                text = "Options",
                value = true
            };

            PropertyField optionsField = new PropertyField(optionsProp, "Options");
            optionsField.Bind(serializedObject);
            optionsFoldout.Add(optionsField);
            root.Add(optionsFoldout);

            VisualElement buttonsRow = new VisualElement();
            buttonsRow.style.flexDirection = FlexDirection.Row;
            buttonsRow.style.marginTop = 6;

            Button addOptionButton = new Button(() =>
            {
                serializedObject.Update();

                int newIndex = optionsProp.arraySize;
                optionsProp.InsertArrayElementAtIndex(newIndex);

                SerializedProperty newElement = optionsProp.GetArrayElementAtIndex(newIndex);
                SerializedProperty inlineText = newElement.FindPropertyRelative("InlineText");

                inlineText.stringValue = $"Choice {newIndex + 1}";

                serializedObject.ApplyModifiedProperties();

                RefreshWarningsAndSummary(optionsProp, warning);
            })
            {
                text = "Add Option"
            };

            Button removeLastButton = new Button(() =>
            {
                serializedObject.Update();

                if (optionsProp.arraySize > 0)
                {
                    optionsProp.DeleteArrayElementAtIndex(optionsProp.arraySize - 1);
                }

                serializedObject.ApplyModifiedProperties();

                RefreshWarningsAndSummary(optionsProp, warning);
            })
            {
                text = "Remove Last"
            };

            addOptionButton.style.flexGrow = 1;
            removeLastButton.style.flexGrow = 1;
            removeLastButton.style.marginLeft = 6;

            buttonsRow.Add(addOptionButton);
            buttonsRow.Add(removeLastButton);
            root.Add(buttonsRow);

            Foldout fallbackFoldout = new Foldout
            {
                text = "Fallback",
                value = false
            };

            PropertyField fallbackField = new PropertyField(fallbackProp, "Fallback (Port Data)");
            fallbackField.SetEnabled(false);
            fallbackFoldout.Add(fallbackField);
            root.Add(fallbackFoldout);

            RefreshWarningsAndSummary(optionsProp, warning);

            optionsField.RegisterValueChangeCallback(_ =>
            {
                RefreshWarningsAndSummary(optionsProp, warning);
            });

            return root;
        }

        private void RefreshWarningsAndSummary(SerializedProperty optionsProp, HelpBox warning)
        {
            serializedObject.Update();

            if (optionsProp.arraySize == 0)
            {
                warning.text = "This Choice node has 0 options. It will follow the fallback output if connected.";
                warning.style.display = DisplayStyle.Flex;
            }
            else
            {
                warning.style.display = DisplayStyle.None;
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = BuildSummaryText(optionsProp);
            }
        }

        private string BuildSummaryText(SerializedProperty optionsProp)
        {
            int total = optionsProp.arraySize;
            int lockedByConditions = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Total Options: {total}");

            for (int i = 0; i < total; i++)
            {
                SerializedProperty option = optionsProp.GetArrayElementAtIndex(i);
                SerializedProperty textProp = option.FindPropertyRelative("InlineText");
                SerializedProperty conditionsProp = option.FindPropertyRelative("Conditions");

                string text = textProp != null ? textProp.stringValue : string.Empty;
                text = string.IsNullOrWhiteSpace(text) ? $"Choice {i + 1}" : Truncate(text, 42);

                int conditionCount = conditionsProp != null && conditionsProp.isArray ? conditionsProp.arraySize : 0;

                if (conditionCount > 0)
                {
                    lockedByConditions++;
                }

                sb.AppendLine($"{i + 1}) \"{text}\"  | Conditions: {conditionCount}");
            }

            sb.AppendLine();
            sb.AppendLine($"Options With Conditions: {lockedByConditions}");

            return sb.ToString();
        }

        private string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.Length <= max)
            {
                return value;
            }

            return value.Substring(0, max - 1) + "…";
        }
    }
}