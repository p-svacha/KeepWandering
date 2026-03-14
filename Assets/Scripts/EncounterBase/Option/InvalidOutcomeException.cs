public class InvalidOutcomeException : System.Exception
{
    public InvalidOutcomeException() : base("Invalid outcome") { }
    public InvalidOutcomeException(OptionOutcomeDef outcome) : base($"Invalid outcome: {outcome}") { }
}