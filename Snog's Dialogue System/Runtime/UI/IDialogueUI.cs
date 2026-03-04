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

        public LineUIOptions(float speedMultiplier)
        {
            SpeedMultiplier = speedMultiplier;
        }

        public static LineUIOptions Default
        {
            get
            {
                return new LineUIOptions(1f);
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