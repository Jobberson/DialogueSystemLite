using System.Collections;
using UnityEngine;

namespace SnogDialogue.Runtime
{
    public sealed class DialogueRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonoBehaviour uiBehaviour;

        [SerializeField] private GlobalVariablesAsset globalVariablesAsset;

        private IDialogueUI ui;
        private DialogueContext context;
        private DialogueRuntime runtime;

        private Coroutine playCoroutine;

        public void Play(DialogueGraph graph)
        {
            if (graph == null)
            {
                return;
            }

            EnsureInitialized();

            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
            }

            playCoroutine = StartCoroutine(PlayRoutine(graph));
        }

        private void EnsureInitialized()
        {
            if (ui == null)
            {
                ui = uiBehaviour as IDialogueUI;
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
            runtime.ClearNext();

            DialogueNode current = FindStartNode(graph);

            while (current != null)
            {
                runtime.ClearNext();

                yield return current.Execute(runtime);

                current = runtime.NextNode;
            }

            ui?.Hide();
        }

        private static DialogueNode FindStartNode(DialogueGraph graph)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                DialogueNode node = graph.nodes[i] as DialogueNode;

                if (node is StartNode)
                {
                    return node;
                }
            }

            return null;
        }
    }
}