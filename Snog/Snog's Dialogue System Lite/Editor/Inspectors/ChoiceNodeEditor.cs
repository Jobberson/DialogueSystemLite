using System.Collections.Generic;
using System.Text;
using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using XNode;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeEditor : UnityEditor.Editor
    {
        private Label summaryLabel;
        private ListView listView;

        public override VisualElement CreateInspectorGUI()
        {
            ChoiceNode choiceNode = (ChoiceNode)target;

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
                "Lite: Options support conditions. Use Up/Down to reorder while preserving xNode port connections.",
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

            behaviorFoldout.Add(new PropertyField(lockedDisplayProp, "Locked Choice Display"));
            behaviorFoldout.Add(new PropertyField(lockedSuffixProp, "Locked Suffix"));
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
                text = "Options (Reorderable)",
                value = true
            };

            listView = CreateOptionsListView(choiceNode, optionsProp);
            optionsFoldout.Add(listView);

            VisualElement buttonsRow = new VisualElement();
            buttonsRow.style.flexDirection = FlexDirection.Row;
            buttonsRow.style.marginTop = 6;

            Button addOptionButton = new Button(() =>
            {
                Undo.RecordObject(choiceNode, "Add Choice Option");

                serializedObject.Update();

                int newIndex = optionsProp.arraySize;
                optionsProp.InsertArrayElementAtIndex(newIndex);

                SerializedProperty newElement = optionsProp.GetArrayElementAtIndex(newIndex);
                SerializedProperty inlineText = newElement.FindPropertyRelative("InlineText");

                inlineText.stringValue = $"Choice {newIndex + 1}";

                // Keep outputs list in sync so XNode creates the matching port.
                SerializedProperty outputsProp = serializedObject.FindProperty("outputs");
                if (outputsProp != null)
                {
                    outputsProp.InsertArrayElementAtIndex(newIndex);
                }

                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(choiceNode);

                RefreshAll(choiceNode, optionsProp, warning);
            })
            {
                text = "Add Option"
            };

            Button removeLastButton = new Button(() =>
            {
                if (optionsProp.arraySize <= 0)
                {
                    return;
                }

                Undo.RecordObject(choiceNode, "Remove Choice Option");

                serializedObject.Update();

                int indexToRemove = optionsProp.arraySize - 1;

                ClearPortConnections(choiceNode, $"outputs {indexToRemove}");
                optionsProp.DeleteArrayElementAtIndex(indexToRemove);

                // Keep outputs list in sync.
                SerializedProperty outputsProp = serializedObject.FindProperty("outputs");
                if (outputsProp != null && indexToRemove < outputsProp.arraySize)
                {
                    outputsProp.DeleteArrayElementAtIndex(indexToRemove);
                }

                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(choiceNode);

                RefreshAll(choiceNode, optionsProp, warning);
            })
            {
                text = "Remove Last"
            };

            addOptionButton.style.flexGrow = 1;
            removeLastButton.style.flexGrow = 1;
            removeLastButton.style.marginLeft = 6;

            buttonsRow.Add(addOptionButton);
            buttonsRow.Add(removeLastButton);

            optionsFoldout.Add(buttonsRow);
            root.Add(optionsFoldout);

            Foldout fallbackFoldout = new Foldout
            {
                text = "Fallback",
                value = false
            };

            PropertyField fallbackField = new PropertyField(fallbackProp, "Fallback (Port Data)");
            fallbackField.SetEnabled(false);

            fallbackFoldout.Add(fallbackField);
            root.Add(fallbackFoldout);

            RefreshAll(choiceNode, optionsProp, warning);

            return root;
        }

        private ListView CreateOptionsListView(ChoiceNode node, SerializedProperty optionsProp)
        {
            ListView lv = new ListView();
            lv.style.marginTop = 6;
            lv.style.borderTopWidth = 1;
            lv.style.borderBottomWidth = 1;
            lv.style.borderLeftWidth = 1;
            lv.style.borderRightWidth = 1;

            lv.itemsSource = new List<int>();
            lv.fixedItemHeight = 58;
            lv.selectionType = SelectionType.None;

            lv.makeItem = () =>
            {
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingLeft = 6;
                row.style.paddingRight = 6;

                VisualElement left = new VisualElement();
                left.style.flexGrow = 1;
                left.style.flexDirection = FlexDirection.Column;

                Label title = new Label();
                title.name = "title";
                title.style.unityFontStyleAndWeight = FontStyle.Bold;

                Label meta = new Label();
                meta.name = "meta";
                meta.style.opacity = 0.75f;

                TextField textField = new TextField("Text");
                textField.name = "inlineText";
                textField.style.marginTop = 4;

                left.Add(title);
                left.Add(meta);
                left.Add(textField);

                VisualElement right = new VisualElement();
                right.style.flexDirection = FlexDirection.Column;
                right.style.marginLeft = 8;

                Button upButton = new Button(() =>
                {
                    int index = (int)row.userData;
                    MoveOption(node, index, index - 1, optionsProp);
                })
                {
                    text = "▲"
                };

                Button downButton = new Button(() =>
                {
                    int index = (int)row.userData;
                    MoveOption(node, index, index + 1, optionsProp);
                })
                {
                    text = "▼"
                };

                upButton.style.width = 32;
                downButton.style.width = 32;

                right.Add(upButton);
                right.Add(downButton);

                row.Add(left);
                row.Add(right);

                return row;
            };

            lv.bindItem = (element, i) =>
            {
                serializedObject.Update();

                int count = optionsProp.arraySize;

                if (i < 0 || i >= count)
                {
                    return;
                }

                element.userData = i;

                SerializedProperty optionProp = optionsProp.GetArrayElementAtIndex(i);
                SerializedProperty inlineTextProp = optionProp.FindPropertyRelative("InlineText");
                SerializedProperty conditionsProp = optionProp.FindPropertyRelative("Conditions");

                Label title = element.Q<Label>("title");
                Label meta = element.Q<Label>("meta");
                TextField textField = element.Q<TextField>("inlineText");

                int conditionsCount = conditionsProp != null && conditionsProp.isArray ? conditionsProp.arraySize : 0;

                string labelText = inlineTextProp != null ? inlineTextProp.stringValue : string.Empty;
                labelText = string.IsNullOrWhiteSpace(labelText) ? $"Choice {i + 1}" : labelText;

                title.text = $"{i + 1}) {Truncate(labelText, 60)}";
                meta.text = $"Conditions: {conditionsCount}";

                if (textField != null && inlineTextProp != null)
                {
                    textField.Unbind();
                    textField.bindingPath = inlineTextProp.propertyPath;
                    textField.Bind(serializedObject);

                    textField.RegisterValueChangedCallback(_ =>
                    {
                        RefreshSummary(node, optionsProp);
                    });
                }

                Button upButton = element.Q<Button>(null, "unity-button");
                List<Button> buttons = element.Query<Button>().ToList();

                if (buttons.Count >= 2)
                {
                    Button up = buttons[0];
                    Button down = buttons[1];

                    up.SetEnabled(i > 0);
                    down.SetEnabled(i < count - 1);
                }
            };

            RefreshListViewItems(lv, optionsProp);

            return lv;
        }

        private void MoveOption(ChoiceNode node, int fromIndex, int toIndex, SerializedProperty optionsProp)
        {
            serializedObject.Update();

            int count = optionsProp.arraySize;

            if (fromIndex < 0 || fromIndex >= count)
            {
                return;
            }

            if (toIndex < 0 || toIndex >= count)
            {
                return;
            }

            if (fromIndex == toIndex)
            {
                return;
            }

            // Keep option-to-connection mapping correct by swapping the connections of the involved ports.
            // Then move the array element (so the data order matches the visual order).
            Undo.RecordObject(node, "Reorder Choice Options");

            SwapPortConnections(node, $"outputs {fromIndex}", $"outputs {toIndex}");

            optionsProp.MoveArrayElement(fromIndex, toIndex);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(node);

            RefreshAll(node, optionsProp, null);
        }

        private void SwapPortConnections(Node node, string portAName, string portBName)
        {
            NodePort portA = node.GetOutputPort(portAName);
            NodePort portB = node.GetOutputPort(portBName);

            if (portA == null || portB == null)
            {
                return;
            }

            List<NodePort> aConnections = new List<NodePort>(portA.GetConnections());
            List<NodePort> bConnections = new List<NodePort>(portB.GetConnections());

            portA.ClearConnections();
            portB.ClearConnections();

            for (int i = 0; i < bConnections.Count; i++)
            {
                portA.Connect(bConnections[i]);
            }

            for (int i = 0; i < aConnections.Count; i++)
            {
                portB.Connect(aConnections[i]);
            }
        }

        private void ClearPortConnections(Node node, string portName)
        {
            NodePort port = node.GetOutputPort(portName);

            if (port == null)
            {
                return;
            }

            port.ClearConnections();
        }

        private void RefreshAll(ChoiceNode node, SerializedProperty optionsProp, HelpBox warning)
        {
            if (warning != null)
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
            }

            RefreshSummary(node, optionsProp);

            if (listView != null)
            {
                RefreshListViewItems(listView, optionsProp);
                listView.Rebuild();
            }
        }

        private void RefreshSummary(ChoiceNode node, SerializedProperty optionsProp)
        {
            if (summaryLabel == null)
            {
                return;
            }

            summaryLabel.text = BuildSummaryText(optionsProp);
        }

        private void RefreshListViewItems(ListView lv, SerializedProperty optionsProp)
        {
            int count = optionsProp.arraySize;

            List<int> items = lv.itemsSource as List<int>;

            if (items == null)
            {
                items = new List<int>();
                lv.itemsSource = items;
            }

            items.Clear();

            for (int i = 0; i < count; i++)
            {
                items.Add(i);
            }
        }

        private string BuildSummaryText(SerializedProperty optionsProp)
        {
            int total = optionsProp.arraySize;
            int withConditions = 0;

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
                    withConditions++;
                }

                sb.AppendLine($"{i + 1}) \"{text}\"  | Conditions: {conditionCount}");
            }

            sb.AppendLine();
            sb.AppendLine($"Options With Conditions: {withConditions}");

            return sb.ToString();
        }

        private static string Truncate(string value, int max)
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