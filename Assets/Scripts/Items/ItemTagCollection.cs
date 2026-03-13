using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A collection of item tags with optional value modifiers.
/// <br/>Supports collection initializer syntax for convenient definition:
/// <br/><c>Tags = { TagA, TagB, { TagC, -5 } }</c>
/// <br/>Tags without a modifier are added with just the tag reference.
/// <br/>Tags with a modifier use the tuple syntax { tag, modifier }.
/// </summary>
public class ItemTagCollection : IEnumerable<ItemTagDef>
{
    private readonly List<ItemTagDef> _tags = new List<ItemTagDef>();
    private readonly Dictionary<ItemTagDef, int> _modifiers = new Dictionary<ItemTagDef, int>();

    public int Count => _tags.Count;

    public void Add(ItemTagDef tag)
    {
        _tags.Add(tag);
    }

    public void Add(ItemTagDef tag, int modifier)
    {
        _tags.Add(tag);
        _modifiers[tag] = modifier;
    }

    public bool Contains(ItemTagDef tag) => _tags.Contains(tag);

    /// <summary>
    /// Returns the value modifier for the given tag, or 0 if no modifier is defined.
    /// </summary>
    public int GetModifier(ItemTagDef tag) => _modifiers.TryGetValue(tag, out int mod) ? mod : 0;

    /// <summary>
    /// Returns true if the given tag has an explicit value modifier defined.
    /// </summary>
    public bool HasModifier(ItemTagDef tag) => _modifiers.ContainsKey(tag);

    public IReadOnlyDictionary<ItemTagDef, int> Modifiers => _modifiers;

    public IEnumerator<ItemTagDef> GetEnumerator() => _tags.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
