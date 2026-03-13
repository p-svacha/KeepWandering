using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This declaration is needed to the init keyword works. Don't remove
namespace System.Runtime.CompilerServices
{
    class IsExternalInit
    {

    }
}

/// <summary>
/// Defs are the data definitions that determine properties, behaviours and relationship of nearly all content in the framework.
/// <br/>Each Def is essentially a blueprint for a particular type of object or behaviour, like items, characters, surfaces, events or skills.
/// <br/>All properties within a def should have the "{ get; init; } = DEFAULT_VALUE" accessors so they can easily and optionally be set in an initializer but not anymore afterwards.
/// </summary>
public abstract class Def : ITooltipTarget
{
    private string LoadingErrorPrefix => "Error on loading Def '" + DefName + "' of type " + GetType() + ": ";

    /// <summary>
    /// The label of the type of this Def. Set once in each subclass to display in tooltips or unlocks.
    /// </summary>
    public abstract string DefTypeLabel { get; }

    /// <summary>
    /// The name of this Def. It is used as a unique identifier within the same type.
    /// </summary>
    public string DefName { get; private set; } = null;

    /// <summary>
    /// A globally unique identifier for this Def, combining its type name and DefName.
    /// <br/>Format: "{TypeName}_{DefName}", e.g. "ConceptDef_Blob".
    /// </summary>
    public string Id => $"{GetType().Name}_{DefName}";

    /// <summary>
    /// A human-readable label used to identify this in game.
    /// <br/>Uncapitalized.
    /// </summary>
    public string Label { get; init; } = "";

    /// <summary>
    /// A human-readable label used to identify this in game.
    /// <br/>Capitalized.
    /// </summary>
    public string LabelCap => Label.CapitalizeFirst();

    /// <summary>
    /// A human-readable label used to identify this in game.
    /// <br/>Each Word Capitalized.
    /// </summary>
    public string LabelCapWord => Label.CapitalizeEachWord();

    /// <summary>
    /// A human-readable description given when the Def is inspected by players.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// The path to the sprite that is used in UI elements for representing this Def.
    /// </summary>
    public virtual Sprite Sprite { get; init; } = null;

    public Def(string defName)
    {
        DefName = defName;
    }


    /// <summary>
    /// Gets called after all Defs are loaded into the database. Used to resolve references to other Defs.
    /// </summary>
    public virtual void ResolveReferences() { }


    /// <summary>
	/// Gets called after all Defs are loaded and all DefOfs are bound.
	/// </summary>
    public virtual void OnLoadingDefsDone() { }

    /// <summary>
    /// Gets called when loading a Def and returns if it is valid. If not an error is thrown.
    /// </summary>
    public virtual bool Validate()
    {
        if (DefName == null) ThrowValidationError("DefName must not be null.");
        if (DefName == "") ThrowValidationError("DefName must not be empty.");
        return true;
    }
    public void ThrowValidationError(string msg)
    {
        throw new Exception(LoadingErrorPrefix + msg);
    }

    public override string ToString()
    {
        return DefName;
    }

    #region ITooltipTarget

    public virtual string GetTooltipTitle() => LabelCapWord;
    public virtual string GetTooltipType() => DefTypeLabel;
    public virtual string GetTooltipBodyText() => Description;

    #endregion
}

