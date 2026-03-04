using SnogDialogue.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomPropertyDrawer(typeof(DialogueValue))]
    public sealed class DialogueValueDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new VisualElement();

            SerializedProperty typeProp = property.FindPropertyRelative("Type");
            SerializedProperty intProp = property.FindPropertyRelative("IntValue");
            SerializedProperty floatProp = property.FindPropertyRelative("FloatValue");
            SerializedProperty boolProp = property.FindPropertyRelative("BoolValue");
            SerializedProperty stringProp = property.FindPropertyRelative("StringValue");

            PropertyField typeField = new PropertyField(typeProp, "Type");
            PropertyField intField = new PropertyField(intProp, "Value");
            PropertyField floatField = new PropertyField(floatProp, "Value");
            PropertyField boolField = new PropertyField(boolProp, "Value");
            PropertyField stringField = new PropertyField(stringProp, "Value");

            root.Add(typeField);
            root.Add(intField);
            root.Add(floatField);
            root.Add(boolField);
            root.Add(stringField);

            void RefreshVisibility()
            {
                DialogueValueType type = (DialogueValueType)typeProp.enumValueIndex;

                intField.style.display = type == DialogueValueType.Int ? DisplayStyle.Flex : DisplayStyle.None;
                floatField.style.display = type == DialogueValueType.Float ? DisplayStyle.Flex : DisplayStyle.None;
                boolField.style.display = type == DialogueValueType.Bool ? DisplayStyle.Flex : DisplayStyle.None;
                stringField.style.display = type == DialogueValueType.String ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RefreshVisibility();

            typeField.RegisterValueChangeCallback(_ =>
            {
                RefreshVisibility();
            });

            return root;
        }
    }
}