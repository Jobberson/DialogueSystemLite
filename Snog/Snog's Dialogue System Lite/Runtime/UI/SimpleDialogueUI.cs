using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SnogDialogue.Runtime
{
    public sealed class SimpleDialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("Panels")]
        [SerializeField] private GameObject linePanel;

        [SerializeField] private GameObject choicesPanel;

        [Header("Line UI")]
        [SerializeField] private TMP_Text lineText;

        [SerializeField] private TMP_Text speakerText;

        [SerializeField] private Button continueButton;

        [Header("Choices UI")]
        [SerializeField] private Transform choicesContainer;

        [SerializeField] private Button choiceButtonPrefab;

        [Header("Typewriter Settings")]
        [SerializeField] private Typewriter typewriter;

        private Action continueRequested;
        private Action<int> choiceSelected;

        private readonly List<Button> spawnedChoiceButtons = new List<Button>();

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            Hide();
        }

        public void ShowLine(string text, LineUIOptions options, Action onContinueRequested)
        {
            continueRequested = onContinueRequested;

            linePanel.SetActive(true);
            choicesPanel.SetActive(false);

            ClearChoices();

            string safeText = text ?? string.Empty;

            if (speakerText != null)
            {
                speakerText.text = options.SpeakerName ?? string.Empty;
                speakerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(options.SpeakerName));
            }

            if (typewriter != null)
            {
                typewriter.Play(safeText, options.SpeedMultiplier, null);
            }
            else
            {
                if (lineText != null)
                {
                    lineText.text = safeText;
                }
            }
        }

        public void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected)
        {
            choiceSelected = onChoiceSelected;

            linePanel.SetActive(false);
            choicesPanel.SetActive(true);

            ClearChoices();

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;

                Button btn = Instantiate(choiceButtonPrefab, choicesContainer);
                spawnedChoiceButtons.Add(btn);

                ChoiceUIEntry entry = choices[i];

                btn.interactable = entry.Interactable;

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = entry.Text;

                    if (!entry.Interactable)
                    {
                        label.alpha = 0.5f;
                    }
                    else
                    {
                        label.alpha = 1f;
                    }
                }

                if (entry.Interactable)
                {
                    btn.onClick.AddListener(() =>
                    {
                        choiceSelected?.Invoke(index);
                    });
                }
            }
        }

        public void Hide()
        {
            continueRequested = null;
            choiceSelected = null;

            ClearChoices();

            if (linePanel != null)
            {
                linePanel.SetActive(false);
            }

            if (choicesPanel != null)
            {
                choicesPanel.SetActive(false);
            }

            if (typewriter != null)
            {
                typewriter.StopTyping();
            }
        }

        private void OnContinueClicked()
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.SkipToEnd();
                return;
            }

            continueRequested?.Invoke();
        }

        private void ClearChoices()
        {
            for (int i = 0; i < spawnedChoiceButtons.Count; i++)
            {
                if (spawnedChoiceButtons[i] != null)
                {
                    Destroy(spawnedChoiceButtons[i].gameObject);
                }
            }

            spawnedChoiceButtons.Clear();
        }
    }
}
