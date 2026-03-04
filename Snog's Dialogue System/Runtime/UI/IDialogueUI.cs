using System;
using System.Collections.Generic;

namespace SnogDialogue.Runtime
{
    public interface IDialogueUI
    {
        void ShowLine(string text, Action onContinueRequested);
        void ShowChoices(IReadOnlyList<string> choices, Action<int> onChoiceSelected);
        void Hide();
    }
}