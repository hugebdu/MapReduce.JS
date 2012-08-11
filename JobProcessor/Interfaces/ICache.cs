using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobProcessor.Interfaces
{
    interface ICache
    {
        object this[string key] { get; set; }
        object Get(string key);
        void Remove(string key);
        object Add(string key, object value);
    }
}
