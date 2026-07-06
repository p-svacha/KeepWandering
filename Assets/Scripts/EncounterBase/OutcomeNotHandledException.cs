using System;

public class OutcomeNotHandledException : Exception
{
    public OptionOutcomeDef Outcome { get; private set; }

    public OutcomeNotHandledException(OptionOutcomeDef outcome)
        : base($"Outcome '{outcome.Label}' is not handled.")
    {
        Outcome = outcome;
    }
}
