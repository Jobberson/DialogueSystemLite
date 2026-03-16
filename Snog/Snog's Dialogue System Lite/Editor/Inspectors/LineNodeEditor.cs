using SnogDialogue.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SnogDialogue.Editor
{
    [CustomEditor(typeof(LineNode))]
    public sealed class LineNodeEditor : UnityEditor.Editor
    {
        private Label estimateLabel;

        public override VisualElement CreateInspectorGUI()
        {
            SerializedProperty speakerNameProp = serializedObject.FindProperty("speakerName");
            SerializedProperty inlineTextProp = serializedObject.FindProperty("inlineText");
            SerializedProperty localizationKeyProp = serializedObject.FindProperty("localizationKey");
            SerializedProperty tagsProp = serializedObject.FindProperty("tags");
            SerializedProperty speedMultiplierProp = serializedObject.FindProperty("speedMultiplier");
            SerializedProperty nextProp = serializedObject.FindProperty("next");

            VisualElement root = new VisualElement();
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 4;

            Label header = new Label("Line Node");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 6;
            root.Add(header);

            HelpBox warningBox = new HelpBox(string.Empty, HelpBoxMessageType.Warning);
            warningBox.style.display = DisplayStyle.None;
            root.Add(warningBox);

            // Speaker name sits above the text foldout so it's always visible at a glance
            PropertyField speakerNameField = new PropertyField(speakerNameProp, "Speaker Name");
            speakerNameField.style.marginBottom = 4;
            root.Add(speakerNameField);

            Foldout textFoldout = new Foldout
            {
                text = "Text",
                value = true
            };

            PropertyField inlineTextField = new PropertyField(inlineTextProp, "Inline Text");
            PropertyField localizationKeyField = new PropertyField(localizationKeyProp, "Localization Key (Lite: not used)");

            textFoldout.Add(inlineTextField);
            textFoldout.Add(localizationKeyField);
            root.Add(textFoldout);

            Foldout typewriterFoldout = new Foldout
            {
                text = "Typewriter",
                value = true
            };

            Slider speedSlider = new Slider("Speed Multiplier", 0.05f, 4f);
            speedSlider.showInputField = true;
            speedSlider.BindProperty(speedMultiplierProp);

            estimateLabel = new Label();
            estimateLabel.style.marginTop = 6;
            estimateLabel.style.whiteSpace = WhiteSpace.Normal;

            VisualElement presetsRow = new VisualElement();
            presetsRow.style.flexDirection = FlexDirection.Row;
            presetsRow.style.marginTop = 6;

            Button slowButton = new Button(() =>
            {
                SetSpeed(speedMultiplierProp, 0.6f);
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);
            })
            {
                text = "Slow"
            };

            Button normalButton = new Button(() =>
            {
                SetSpeed(speedMultiplierProp, 1f);
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);
            })
            {
                text = "Normal"
            };

            Button fastButton = new Button(() =>
            {
                SetSpeed(speedMultiplierProp, 1.8f);
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);
            })
            {
                text = "Fast"
            };

            slowButton.style.flexGrow = 1;
            normalButton.style.flexGrow = 1;
            fastButton.style.flexGrow = 1;

            normalButton.style.marginLeft = 6;
            fastButton.style.marginLeft = 6;

            presetsRow.Add(slowButton);
            presetsRow.Add(normalButton);
            presetsRow.Add(fastButton);

            typewriterFoldout.Add(speedSlider);
            typewriterFoldout.Add(estimateLabel);
            typewriterFoldout.Add(presetsRow);

            root.Add(typewriterFoldout);

            Foldout metaFoldout = new Foldout
            {
                text = "Metadata",
                value = false
            };

            PropertyField tagsField = new PropertyField(tagsProp, "Tags");
            metaFoldout.Add(tagsField);

            root.Add(metaFoldout);

            PropertyField nextField = new PropertyField(nextProp, "Next (Port Data)");
            nextField.SetEnabled(false);
            nextField.style.marginTop = 8;
            root.Add(nextField);

            RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);

            inlineTextField.RegisterValueChangeCallback(_ =>
            {
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);
            });

            speedSlider.RegisterValueChangedCallback(_ =>
            {
                RefreshAll(inlineTextProp, speedMultiplierProp, warningBox);
            });

            return root;
        }

        private void SetSpeed(SerializedProperty speedProp, float value)
        {
            serializedObject.Update();
            speedProp.floatValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshAll(
            SerializedProperty inlineTextProp,
            SerializedProperty speedMultiplierProp,
            HelpBox warningBox
        )
        {
            serializedObject.Update();

            RefreshWarnings(inlineTextProp, speedMultiplierProp, warningBox);
            RefreshEstimate(inlineTextProp, speedMultiplierProp);
        }

        private void RefreshWarnings(
            SerializedProperty inlineTextProp,
            SerializedProperty speedMultiplierProp,
            HelpBox warningBox
        )
        {
            string text = inlineTextProp.stringValue;

            if (string.IsNullOrWhiteSpace(text))
            {
                warningBox.text = "Inline Text is empty. In Lite, this will display as [Missing line text].";
                warningBox.style.display = DisplayStyle.Flex;
                return;
            }

            float speed = speedMultiplierProp.floatValue;

            if (speed < 0.05f)
            {
                warningBox.text = "Speed Multiplier is too low. Recommended minimum is 0.05.";
                warningBox.style.display = DisplayStyle.Flex;
                return;
            }

            warningBox.style.display = DisplayStyle.None;
        }

        private void RefreshEstimate(SerializedProperty inlineTextProp, SerializedProperty speedMultiplierProp)
        {
            if (estimateLabel == null)
            {
                return;
            }

            string text = inlineTextProp.stringValue ?? string.Empty;
            int charCount = text.Length;

            float multiplier = Mathf.Max(0.05f, speedMultiplierProp.floatValue);

            // Assumption for estimate only: base speed ~40 cps (matches typical defaults).
            float baseCps = 40f;
            float effectiveCps = baseCps * multiplier;

            float seconds = effectiveCps <= 0f ? 0f : charCount / effectiveCps;

            estimateLabel.text = $"Estimate: {seconds:0.0}s to type ({charCount} chars @ {effectiveCps:0.#} cps).";
        }
    }
}