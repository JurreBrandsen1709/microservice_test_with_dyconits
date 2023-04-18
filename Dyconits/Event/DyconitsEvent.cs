using Dyconits.Event;

public class DyconitsEvent : IDyconitsEvent
{
    private Dictionary<string, object> _properties;

    public DyconitsEvent(Dictionary<string, object> properties)
    {
        _properties = properties;
        AddTimestamp();
    }

    public Dictionary<string, object> Properties => _properties;

    public T GetPropertyValue<T>(string propertyName)
    {
        object value;
        if (_properties.TryGetValue(propertyName, out value))
        {
            return (T)value;
        }
        else
        {
            throw new ArgumentException($"Property '{propertyName}' not found.");
        }
    }

    private void AddTimestamp()
    {
        _properties["Timestamp"] = DateTime.UtcNow;
    }
}


/**
 * var dto = new Dictionary<string, object>() { { "Id", 1 }, { "Name", "DTOName" }, { "Property1", "Value1" }, { "Property2", 123 } }; // example DTO
 * var dyconitEvent = new DyconitEvent(dto);
 * var id = dyconitEvent.GetPropertyValue<int>("Id");
 * var name = dyconitEvent.GetPropertyValue<string>("Name");
 * var property1 = dyconitEvent.GetPropertyValue<string>("Property1");
 * var property2 = dyconitEvent.GetPropertyValue<int>("Property2");
 * var timestamp = dyconitEvent.GetPropertyValue<DateTime>("Timestamp");
 **/