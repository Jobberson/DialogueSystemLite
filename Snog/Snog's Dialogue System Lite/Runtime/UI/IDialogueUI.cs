using System;
using System.Collections.Generic;

namespace SnogDialogue.Runtime
{
    public interface IDialogueUI
    {
        void ShowLine(string text, LineUIOptions options, Action onContinueRequested);
        void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected);
        void Hide();
    }

    public readonly struct LineUIOptions
    {
        public float SpeedMultiplier
        {
            get;
        }

        public string SpeakerName
        {
            get;
        }

        public LineUIOptions(float speedMultiplier, string speakerName = "")
        {
            SpeedMultiplier = speedMultiplier;
            SpeakerName = speakerName ?? string.Empty;
        }

        public static LineUIOptions Default
        {
            get
            {
                return new LineUIOptions(1f, string.Empty);
            }
        }
    }

    public readonly struct ChoiceUIEntry
    {
        public string Text
        {
            get;
        }

        public bool Interactable
        {
            get;
        }

        public ChoiceUIEntry(string text, bool interactable)
        {
            Text = text;
            Interactable = interactable;
        }
    }
}