using System.Collections.Generic;

namespace SnogDialogue.Runtime
{
    public sealed class VariableStore
    {
        private readonly Dictionary<string, DialogueValue> values;

        public VariableStore()
        {
            values = new Dictionary<string, DialogueValue>();
        }

        public bool Has(string key)
        {
            return values.ContainsKey(key);
        }

        public bool TryGet(string key, out DialogueValue value)
        {
            return values.TryGetValue(key, out value);
        }

        public void Set(string key, DialogueValue value)
        {
            values[key] = value;
        }

        public void Clear()
        {
            values.Clear();
        }

        /// <summary>
        /// Prints all current variables to the Unity console. Useful for debugging
        /// conditions that aren't behaving as expected.
        /// </summary>
        public void DebugDump(string label = "VariableStore")
        {
            if (values.Count == 0)
            {
                UnityEngine.Debug.Log($"[{label}] (empty)");
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{label}] {values.Count} variable(s):");

            foreach (System.Collections.Generic.KeyValuePair<string, DialogueValue> kvp in values)
            {
                string displayValue;

                switch (kvp.Value.Type)
                {
                    case DialogueValueType.Int:    displayValue = kvp.Value.IntValue.ToString();    break;
                    case DialogueValueType.Float:  displayValue = kvp.Value.FloatValue.ToString("G"); break;
                    case DialogueValueType.Bool:   displayValue = kvp.Value.BoolValue.ToString();   break;
                    case DialogueValueType.String: displayValue = $"\"{kvp.Value.StringValue}\"";   break;
                    default:                       displayValue = "?";                               break;
                }

                sb.AppendLine($"  {kvp.Key} ({kvp.Value.Type}) = {displayValue}");
            }

            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}