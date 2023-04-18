using System;
using System.Collections.Generic;

namespace Dyconits.Event
{
    public interface IDyconitsEvent
    {
        Dictionary<string, object> Properties { get; }
        T GetPropertyValue<T>(string propertyName);
    }
}
