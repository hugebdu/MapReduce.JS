using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor;
using JobProcessor.Interfaces;

namespace JobProcessor.Implementation
{
    public class DefaultCache : ICache
    {
        private Dictionary<string, object> _cache = new Dictionary<string, object>();
        
        public object this[string key]
        {
            get
            {
                return _cache.ContainsKey(key) ? _cache[key] : null;
            }
            set
            {
                if (_cache.ContainsKey(key))
                    _cache[key] = value;
                else
                    _cache.Add(key, value);
            }
        }

        public object Get(string key)
        {
            return _cache.ContainsKey(key) ? _cache[key] : null;
        }

        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
                _cache.Remove(key);
        }

        public object Add(string key, object value)
        {
            if (_cache.ContainsKey(key))
                _cache[key] = value;
            else
                _cache.Add(key, value);

            return null;
        }
    }
}
