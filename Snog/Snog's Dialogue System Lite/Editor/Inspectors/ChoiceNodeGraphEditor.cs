using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace SnogDialogue.Editor
{
    [CustomNodeEditor(typeof(ChoiceNode))]
    public sealed class ChoiceNodeGraphEditor : NodeEditor
    {
        private static readonly GUIStyle _labelStyle = new GUIStyle();
        private static bool _stylesInitialized;

        public override void OnBodyGUI()
        {
            EnsureStyles();

            ChoiceNode node = (ChoiceNode)target;
            serializedObject.Update();

            // Input port
            NodeEditorGUILayout.PortField(new GUIContent("In"), node.GetInputPort("input"));

            EditorGUILayout.Space(2);

            // One labelled output port per choice option
            for (int i = 0; i < node.OptionCount; i++)
            {
                string label = Truncate(node.GetOptionLabel(i), 28);
                NodePort port = node.GetOutputPort($"outputs {i}");

                if (port != null)
                {
                    NodeEditorGUILayout.PortField(new GUIContent(label), port);
                }
            }

            if (node.OptionCount == 0)
            {
                EditorGUILayout.HelpBox("No options. Add some in the Inspector.", MessageType.Warning);
            }

            EditorGUILayout.Space(2);

            // Fallback port
            NodeEditorGUILayout.PortField(new GUIContent("Fallback"), node.GetOutputPort("fallback"));

            serializedObject.ApplyModifiedProperties();
        }

        private static void EnsureStyles()
        {
            if (_stylesInitialized)
            {
                return;
            }

            _stylesInitialized = true;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }

            return value.Substring(0, max - 1) + "…";
        }
    }
}
