using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// The central registry that contains references to all DefDatabases (1 for each Def-type).
/// <br/>Functions that should be run on all DefDatases should only be executed through this registry.
/// </summary>
public static class DefDatabaseRegistry
{
    // Stores all DefDatabase types registered
    private static readonly List<Type> registeredDefDatabases = new List<Type>();

    // Cached list of DefOf classes so that we don't search for them every time.
    private static List<Type> cachedDefOfClasses;

    /// <summary>
    /// Adds all Defs that are defined in the BlockmapFramework and are useful for all projects to their respective DefDatabases.
    /// </summary>
    public static void InitDefs()
    {
        ClearAllDatabases();

        DefDatabase<TimeOfDayDef>.AddDefs(TimeOfDayDefs.Defs);
        DefDatabase<DangerLevelDef>.AddDefs(DangerLevelDefs.Defs);

        DefDatabase<QuestDef>.AddDefs(QuestDefs.Defs);
        DefDatabase<RumourDef>.AddDefs(RumourDefs.Defs);
        DefDatabase<OptionOutcomeDef>.AddDefs(OptionOutcomeDefs.Defs);
        DefDatabase<StatDef>.AddDefs(StatDefs.Defs);
        DefDatabase<HealthConditionCategoryDef>.AddDefs(HealthConditionCategoryDefs.Defs);
        DefDatabase<HealthConditionDef>.AddDefs(HealthConditionDefs.Defs);
        DefDatabase<CompanionDef>.AddDefs(CompanionDefs.Defs);

        DefDatabase<ConsumptionTypeDef>.AddDefs(ConsumptionTypeDefs.Defs);
        DefDatabase<ItemTagDef>.AddDefs(ItemTagDefs.Defs);
        DefDatabase<ItemDef>.AddDefs(ItemDefs.Defs);

        DefDatabase<BiomeDef>.AddDefs(BiomeDefs.Defs);
        DefDatabase<EncounterDef>.AddDefs(EncounterDefs.Defs);

        ValidateDefOfAttributes();
        ValidateDefOfs();
        ResolveAllReferences();
        OnLoadingDone();
    }

    // Called when a DefDatabase<T> type is accessed for the first time
    public static void RegisterDefDatabase(Type defDatabaseType)
    {
        if (!registeredDefDatabases.Contains(defDatabaseType))
        {
            registeredDefDatabases.Add(defDatabaseType);
        }
    }

    // Calls Clear on each registered DefDatabase type
    public static void ClearAllDatabases()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static ResolveReferences method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }
    }

    /// <summary>
    /// Looks up a Def by its tooltip link ID (format: "TypeName_DefName", e.g. "ConceptDef_Blob").
    /// Returns null if no matching Def is found.
    /// </summary>
    public static Def GetDefByLinkId(string linkId)
    {
        int underscoreIndex = linkId.IndexOf('_');
        if (underscoreIndex <= 0) return null;

        string typeName = linkId.Substring(0, underscoreIndex);
        string defName = linkId.Substring(underscoreIndex + 1);

        foreach (Type dbType in registeredDefDatabases)
        {
            Type defType = dbType.GetGenericArguments()[0];
            if (defType.Name == typeName)
            {
                try
                {
                    MethodInfo getNamedMethod = dbType.GetMethod("GetNamed", BindingFlags.Static | BindingFlags.Public);
                    return (Def)getNamedMethod.Invoke(null, new object[] { defName });
                }
                catch
                {
                    return null;
                }
            }
        }
        return null;
    }

    // Calls ResolveReferences on each registered DefDatabase type
    public static void ResolveAllReferences()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static ResolveReferences method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("ResolveReferences", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }
    }

    // Calls OnLoadingDone on each registered DefDatabase type
    public static void OnLoadingDone()
    {
        foreach (Type defDatabaseType in registeredDefDatabases)
        {
            // Invoke the static OnLoadingDone method
            MethodInfo resolveMethod = defDatabaseType.GetMethod("OnLoadingDone", BindingFlags.Static | BindingFlags.Public);
            resolveMethod?.Invoke(null, null);
        }

        DefDumpUtility.DumpAllDefs();
    }

    /// <summary>
    /// Checks that all classes whose name ends with "DefOf" have the [DefOf] attribute.
    /// Throws an exception if any such class is missing it.
    /// </summary>
    private static void ValidateDefOfAttributes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name != "DefOf" && type.Name.EndsWith("DefOf") && !type.GetCustomAttributes(typeof(DefOf), inherit: false).Any())
                {
                    throw new System.Exception($"Class '{type.FullName}' has a name ending with 'DefOf' but is missing the [DefOf] attribute. Add the [DefOf] attribute to ensure its fields are automatically bound.");
                }
            }
        }
    }

    /// <summary>
    /// Checks that all static fields in all DefOf classes have been bound to a Def.
    /// Throws an exception if any field is still null.
    /// </summary>
    private static void ValidateDefOfs()
    {
        foreach (Type defOfClass in GetAllDefOfClasses())
        {
            FieldInfo[] fields = defOfClass.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(null) == null)
                {
                    throw new System.Exception($"DefOf field '{defOfClass.Name}.{field.Name}' of type {field.FieldType.Name} is not bound to any Def. Make sure a Def with DefName \"{field.Name}\" exists.");
                }
            }
        }
    }

    /// <summary>
    /// Searches all assemblies for types marked with the DefOf attribute and returns them.
    /// The result is cached since these types do not change at runtime.
    /// </summary>
    /// <returns>A list of Types that have the DefOf attribute.</returns>
    private static List<Type> GetAllDefOfClasses()
    {
        if (cachedDefOfClasses == null)
        {
            cachedDefOfClasses = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(DefOf), inherit: false).Any())
                    {
                        cachedDefOfClasses.Add(type);
                    }
                }
            }
        }
        return cachedDefOfClasses;
    }

    /// <summary>
    /// Immediately binds the provided Def instance to all static fields in DefOf classes
    /// where the field name matches the Def's DefName and the field type is compatible with the Def.
    /// This ensures that as soon as a Def is loaded, it is available via its corresponding DefOf.
    /// </summary>
    /// <param name="def">The Def instance to bind.</param>
    public static void BindDefToAllDefOfs(Def def)
    {
        foreach (Type defOfClass in GetAllDefOfClasses())
        {
            FieldInfo[] fields = defOfClass.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                // If the field name matches the Def's DefName and the field's type is compatible with the def's type...
                if (field.Name == def.DefName && field.FieldType.IsAssignableFrom(def.GetType()))
                {
                    field.SetValue(null, def);
                }
            }
        }
    }
}

