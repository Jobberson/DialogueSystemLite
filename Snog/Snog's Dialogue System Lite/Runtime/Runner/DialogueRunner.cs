using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SnogDialogue.Runtime
{
    [DisallowMultipleComponent]
    public sealed class DialogueRunner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The MonoBehaviour that implements IDialogueUI. " +
                 "Assign your SimpleDialogueUI component here, or any custom UI that implements IDialogueUI.")]
        [SerializeField] private MonoBehaviour uiBehaviour;

        [Tooltip("Optional. Pre-populates global variables at runtime. " +
                 "Leave empty if you are not using global variables.")]
        [SerializeField] private GlobalVariablesAsset globalVariablesAsset;

        [Header("Events")]
        [Tooltip("Fired when a dialogue graph begins playing.")]
        public UnityEvent OnDialogueStarted;

        [Tooltip("Fired when a dialogue graph finishes playing.")]
        public UnityEvent OnDialogueFinished;

        // ── Public state ──────────────────────────────────────────────────────

        /// <summary>True while a dialogue graph is currently playing.</summary>
        public bool IsPlaying => playCoroutine != null;

        /// <summary>
        /// Runtime global variable store. Read or write from other scripts (e.g. a quest system).
        /// Null before the first Play() call.
        /// </summary>
        public VariableStore GlobalVariables => context?.GlobalVariables;

        /// <summary>
        /// Runtime graph-local variable store. Cleared at the start of every Play() call.
        /// Null before the first Play() call.
        /// </summary>
        public VariableStore GraphVariables => context?.GraphVariables;

        // ── Private ───────────────────────────────────────────────────────────

        private IDialogueUI ui;
        private DialogueContext context;
        private DialogueRuntime runtime;
        private Coroutine playCoroutine;

        // ── Public API ────────────────────────────────────────────────────────

        public void Play(DialogueGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("[DialogueRunner] Play() called with a null graph.", this);
                return;
            }

            if (!EnsureInitialized())
            {
                return;
            }

            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }

            playCoroutine = StartCoroutine(PlayRoutine(graph));
        }

        /// <summary>Immediately stops the current dialogue and fires OnDialogueFinished.</summary>
        public void Stop()
        {
            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
                playCoroutine = null;
            }

            ui?.Hide();
            OnDialogueFinished?.Invoke();
        }

        // ── Private ───────────────────────────────────────────────────────────

        private bool EnsureInitialized()
        {
            if (ui == null)
            {
                ui = uiBehaviour as IDialogueUI;

                if (ui == null)
                {
                    Debug.LogError(
                        "[DialogueRunner] uiBehaviour does not implement IDialogueUI. " +
                        "Assign a component that implements IDialogueUI (e.g. SimpleDialogueUI) " +
                        "to the UI Behaviour field on this DialogueRunner.",
                        this
                    );
                    return false;
                }
            }

            if (context == null)
            {
                VariableStore globalStore = CreateGlobalStore();
                VariableStore graphStore = new VariableStore();
                context = new DialogueContext(globalStore, graphStore);
            }

            if (runtime == null)
            {
                runtime = new DialogueRuntime(context, ui);
            }

            return true;
        }

        private VariableStore CreateGlobalStore()
        {
            if (globalVariablesAsset == null)
            {
                return new VariableStore();
            }

            return globalVariablesAsset.CreateRuntimeStore();
        }

        private IEnumerator PlayRoutine(DialogueGraph graph)
        {
            context.GraphVariables.Clear();
            runtime.ClearNext();

            OnDialogueStarted?.Invoke();

            DialogueNode current = FindStartNode(graph);

            if (current == null)
            {
                Debug.LogWarning(
                    $"[DialogueRunner] No StartNode found in graph '{graph.name}'. " +
                    "Make sure the graph contains a Start node.",
                    this
                );

                playCoroutine = null;
                OnDialogueFinished?.Invoke();
                yield break;
            }

            while (current != null)
            {
                runtime.ClearNext();
                yield return current.Execute(runtime);
                current = runtime.NextNode;
            }

            ui?.Hide();

            playCoroutine = null;
            OnDialogueFinished?.Invoke();
        }

        private static DialogueNode FindStartNode(DialogueGraph graph)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i] is StartNode startNode)
                {
                    return startNode;
                }
            }

            return null;
        }
    }
}
