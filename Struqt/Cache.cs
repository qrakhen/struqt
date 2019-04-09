using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qrakhen.Struqt.Models
{
    public static class Cache
    {
        private static Dictionary<Type, Dictionary<object, Model>> entryCache = new Dictionary<Type, Dictionary<object, Model>>();

        public static void flush()
        {
            entryCache.Clear();
        }

        public static void flush(Type model)
        {
            if (entryCache.ContainsKey(model)) entryCache.Remove(model);
        }

        public static void flush(Type model, object key)
        {
            if (entryCache.ContainsKey(model) && entryCache[model].ContainsKey(key)) entryCache[model].Remove(key);
        }

        public static T get<T>(Type model, object primaryKey)
        {
            if (entryCache.ContainsKey(model)) {
                if (entryCache[model].ContainsKey(primaryKey)) return (T)(object)entryCache[model][primaryKey];
            }
            return default(T);
        }

        public static Model get(Type model, object primaryKey)
        {
            if (entryCache.ContainsKey(model)) {
                if (entryCache[model].ContainsKey(primaryKey)) return entryCache[model][primaryKey];
            }
            return null;
        }
    
        public static void set(Type model, object primaryKey, Model entry)
        {
            if (!entryCache.ContainsKey(model)) entryCache.Add(model, new Dictionary<object, Model>());
            if (entryCache[model].ContainsKey(primaryKey)) entryCache[model][primaryKey] = entry;
            else entryCache[model].Add(primaryKey, entry);
        }
    }
}
