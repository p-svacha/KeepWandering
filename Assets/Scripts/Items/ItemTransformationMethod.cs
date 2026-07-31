using System.Collections.Generic;
using UnityEngine;

public class ItemTransformationMethodDef : Def
{
    public override string DefTypeLabel => "Item Transformation Method";
    public ItemTransformationMethodDef(string defName) : base(defName) { }
}

public static class ItemTransformationMethodDefs
{
    public static List<ItemTransformationMethodDef> Defs => new List<ItemTransformationMethodDef>()
    {
        new ItemTransformationMethodDef("Cooking")
        {
            Label = "cooking",
            Sprite = Resources.Load<Sprite>("Camp/Camp_Fire"),
        }
    }; 
}

[DefOf]
public static class ItemTransformationMethodDefOf
{
    public static ItemTransformationMethodDef Cooking;
}
