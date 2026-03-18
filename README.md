<div align="center">

<h1> Snog's Dialogue System Lite </h1>

**A lightweight, node‑based dialogue framework for Unity.**  
Node Graphs · Conditional Choices · Variables · Typewriter · Simple UI · Open Source

---

[![Unity](https://img.shields.io/badge/Unity-6000.3.x-black?style=flat-square&logo=unity&logoColor=white)](https://unity.com)
[![License](https://img.shields.io/badge/License-MIT-blue?style=flat-square)](#license)
[![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=flat-square)](#)

</div>

---

## Why Snog's Dialogue System Lite?

Most Unity projects eventually need branching conversations, quick iteration, and a UI you can swap without touching game logic. This repo gives you exactly that: a **clean, extensible, open‑source** dialogue system built on **XNode** (for graphs), **coroutines** (for flow), and a small **UI contract** (so anyone can bring their own look). No bloat, no lock‑in.

The highlight is the **authoring experience**: reorder choices while preserving connections, tune typewriter speed with a live estimate, and pick variable keys from your global assets without memorizing strings.

---

## Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Examples](#examples)
- [Variables & Conditions](#variables--conditions)
- [Requirements](#requirements)
- [Project Structure](#project-structure)
- [Troubleshooting](#troubleshooting)
- [Roadmap](#roadmap)
- [Pro Version](#pro-version-light-mention)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)

---

## Features

<details open>
<summary><strong>🎛️ Core Runtime</strong></summary>
<br>

- **Node‑based** dialogue graph via XNode (Start, Line, Choice, Set Variable, End)
- **Coroutine execution** loop: each node exposes `IEnumerator Execute(DialogueRuntime)`
- **UI‑agnostic contract** via `IDialogueUI` (`ShowLine`, `ShowChoices`, `Hide`)
- **Global & graph‑local variables** with a compact `DialogueValue` union type (int/float/bool/string)
- **Conditional branching** with operators: Exists/NotExists/Equals/NotEquals/`>`/`>=`/`<`/`<=`

</details>

<details open>
<summary><strong>🧰 Editor Experience</strong></summary>
<br>

- **Choice Node**: reorder options while preserving xNode port connections; live summary
- **Line Node**: typewriter **speed presets** and **time estimate**
- **Set Variable Node**: **Known Keys** popup sourced from your `GlobalVariablesAsset`
- **Condition Drawer**: smart visibility (expected value only when needed)
- **Global Variables Asset** editor: clear “runtime copy” info and friendly list UI

</details>

<details open>
<summary><strong>🖼️ Built‑in UI</strong></summary>
<br>

- `SimpleDialogueUI` reference implementation (buttons, TMP text, basic layout)
- `Typewriter` effect with punctuation & newline pacing, **skip‑to‑end** support, and unscaled time option

</details>

---

## Quick Start

### 1) Install
- Clone or download this repository into your Unity project.
- Ensure **XNode** is present (either included with this repo or imported separately).
- Ensure **TextMeshPro** is available (Unity includes it by default).

### 2) Create a Dialogue Graph
**Right‑click → Create → Snog / DialogueSystem / Dialogue Graph**

### 3) Add Nodes
Drag out **Line**, **Choice**, **Set Variable**, then connect to **End**.

### 4) Add a Runner and UI
- Drop **DialogueRunner** on a GameObject.
- Assign a component that implements **`IDialogueUI`** (e.g., `SimpleDialogueUI`).
- (Optional) Create a **GlobalVariablesAsset** and assign it to the runner.

### 5) Play a Graph

```csharp
using UnityEngine;
using SnogDialogue.Runtime;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueRunner runner;
    public DialogueGraph graph;

    private void Start()
    {
        runner.Play(graph);
    }
}
```

---

## Examples

### Implementing a Minimal `IDialogueUI`

```csharp
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SnogDialogue.Runtime;

public class MyDialogueUI : MonoBehaviour, IDialogueUI
{
    [SerializeField] private GameObject linePanel;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button continueButton;

    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private Action onContinue;
    private Action<int> onChoice;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void ShowLine(string text, LineUIOptions options, Action onContinueRequested)
    {
        onContinue = onContinueRequested;
        linePanel.SetActive(true);
        choicesPanel.SetActive(false);
        lineText.text = text ?? string.Empty;
    }

    public void ShowChoices(IReadOnlyList<ChoiceUIEntry> choices, Action<int> onChoiceSelected)
    {
        onChoice = onChoiceSelected;
        linePanel.SetActive(false);
        choicesPanel.SetActive(true);

        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i;
            var btn = Instantiate(choiceButtonPrefab, choicesContainer);
            btn.interactable = choices[i].Interactable;
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = choices[i].Text;
                label.alpha = choices[i].Interactable ? 1f : 0.5f;
            }
            if (choices[i].Interactable)
            {
                btn.onClick.AddListener(() => onChoice?.Invoke(index));
            }
        }
    }

    public void Hide()
    {
        linePanel.SetActive(false);
        choicesPanel.SetActive(false);
        onContinue = null;
        onChoice = null;
    }

    private void OnContinueClicked()
    {
        onContinue?.Invoke();
    }
}
```

### Starting a Dialogue from Script

```csharp
public class StartWhenNear : MonoBehaviour
{
    public DialogueRunner runner;
    public DialogueGraph graph;
    public Transform player;
    public float triggerDistance = 2.0f;

    private bool started;

    private void Update()
    {
        if (!started && Vector3.Distance(transform.position, player.position) <= triggerDistance)
        {
            started = true;
            runner.Play(graph);
        }
    }
}
```

---

## Variables & Conditions

### Setting Variables (Set Variable Node)
- **Scope:** Global or Graph Local
- **Key:** e.g., `hasKeycard`
- **Value:** `Bool = true`

### Conditional Choices
- Use `Condition` arrays on each choice option
- Lock behavior:
  - **Hide**: remove unavailable options
  - **Disable**: show with a suffix (e.g., "(Locked)") and non‑interactable

---

## Requirements

| Requirement | Notes |
|---|---|
| **Unity 6000.3.x** | Tested baseline (newer versions likely fine) |
| **TextMeshPro** | Used by the sample UI & Typewriter |
| **XNode** | Required for node graphs (import the package or use the bundled copy) |

---

## Project Structure

```
SnogDialogue/
├─ Runtime/
│  ├─ Graph/            ← DialogueGraph, DialogueNode base
│  ├─ Nodes/            ← Start, Line, Choice, Set Variable, End
│  ├─ UI/               ← IDialogueUI, SimpleDialogueUI, Typewriter
│  └─ Variables/        ← DialogueValue, VariableStore, Condition, Evaluator
└─ Editor/
   ├─ Drawers/          ← DialogueValueDrawer, ConditionDrawer
   ├─ NodeEditors/      ← LineNodeEditor, ChoiceNodeEditor, SetVariableNodeEditor
   └─ Utility/          ← GlobalVariableKeyUtility
```

---

## Troubleshooting

<details>
<summary><strong>Dialogue doesn't start</strong></summary>
<br>

- Ensure there is a **Start** node in the graph.
- Ensure the **DialogueRunner** has a reference to a component implementing **`IDialogueUI`**.

</details>

<details>
<summary><strong>All choices are hidden or disabled</strong></summary>
<br>

- Check the **Conditions** on each choice option.
- If none are interactable, the node follows its **Fallback** output.

</details>

<details>
<summary><strong>Variables not updating</strong></summary>
<br>

- Verify the **Scope** and **Key** in your **Set Variable** node.
- If you expect fresh state per run, consider clearing or re‑creating **Graph Local** variables before `Play()`.

</details>

<details>
<summary><strong>Typewriter not skipping</strong></summary>
<br>

- Ensure your UI calls `SkipToEnd()` when the Continue button is pressed while typing.

</details>

---

## Roadmap

- Minimal **checkpoint save/load** example (graph + node IDs)
- Optional **keyboard/controller** navigation for choices
- **Play From Here** editor action for rapid iteration
- Basic **localization hook** (optional Localizer interface)
- Themeable `SimpleDialogueUI`

---

## Pro Version

A future **Pro** edition will build on this with:
- Localization pipeline
- Audio per line and advanced tags/markup
- Authoring analytics & debuggers
- Checkpoint‑based save/load out of the box
- Extra nodes and visual tools

Lite stays lean and open‑source. Upgrade only if you need the extras.

---

## License

**MIT License** — free for commercial and non‑commercial projects. See [LICENSE](LICENSE) for details.

---

## Author

**Pedro Schenegoski**  
<snogdev@gmail.com>

Made with ♥ for fellow devs.
