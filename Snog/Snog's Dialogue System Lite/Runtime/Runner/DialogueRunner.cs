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
            if (playCoroutine == null) return;

            StopCoroutine(playCoroutine);
            playCoroutine = null;

            ui?.Hide();
            OnDialogueFinished?.Invoke();
        }

        // ── Convenience variable API ──────────────────────────────────────────

        /// <summary>Sets a global bool. Safe to call before the first Play().</summary>
        public void SetGlobalBool(string key, bool value) =>
            GlobalVariables?.Set(key, DialogueValue.FromBool(value));

        /// <summary>Gets a global bool, returning <paramref name="fallback"/> if the key doesn't exist.</summary>
        public bool GetGlobalBool(string key, bool fallback = false)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v))
                return v.BoolValue;
            return fallback;
        }

        /// <summary>Sets a global int. Safe to call before the first Play().</summary>
        public void SetGlobalInt(string key, int value) =>
            GlobalVariables?.Set(key, DialogueValue.FromInt(value));

        /// <summary>Gets a global int, returning <paramref name="fallback"/> if the key doesn't exist.</summary>
        public int GetGlobalInt(string key, int fallback = 0)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v))
                return v.IntValue;
            return fallback;
        }

        /// <summary>Sets a global float. Safe to call before the first Play().</summary>
        public void SetGlobalFloat(string key, float value) =>
            GlobalVariables?.Set(key, DialogueValue.FromFloat(value));

        /// <summary>Gets a global float, returning <paramref name="fallback"/> if the key doesn't exist.</summary>
        public float GetGlobalFloat(string key, float fallback = 0f)
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v))
                return v.FloatValue;
            return fallback;
        }

        /// <summary>Sets a global string. Safe to call before the first Play().</summary>
        public void SetGlobalString(string key, string value) =>
            GlobalVariables?.Set(key, DialogueValue.FromString(value));

        /// <summary>Gets a global string, returning <paramref name="fallback"/> if the key doesn't exist.</summary>
        public string GetGlobalString(string key, string fallback = "")
        {
            if (GlobalVariables != null && GlobalVariables.TryGet(key, out DialogueValue v))
                return v.StringValue;
            return fallback;
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

            int safetyCounter = 0;

            while (current != null)
            {
                if (++safetyCounter > 2000)
                {
                    Debug.LogError(
                        $"[DialogueRunner] Possible infinite loop detected in graph '{graph.name}' " +
                        "after 2000 node executions. Stopping dialogue to prevent a hang. " +
                        "Check for cycles in your graph.",
                        this
                    );
                    break;
                }

                DialogueNode previous = current;
                runtime.ClearNext();
                yield return current.Execute(runtime);
                current = runtime.NextNode;

                if (current == null && !(previous is EndNode))
                {
                    Debug.LogWarning(
                        $"[DialogueRunner] Dialogue in graph '{graph.name}' reached node " +
                        $"'{previous.GetType().Name}' (ID: {previous.NodeId}) with no outgoing connection. " +
                        "The dialogue will end here. Connect the output port to continue or add an End node.",
                        this
                    );
                }
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
