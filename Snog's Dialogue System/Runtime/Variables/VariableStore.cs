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
    }
}