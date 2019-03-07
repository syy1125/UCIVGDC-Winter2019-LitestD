[System.Serializable]
public class Condition
{
    public enum ConditionType
    {
        Equals,
        NotEquals,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual
    }

    public IntReference intReference;
    public ConditionType conditionType;
    public int value;

    public bool IsMet()
    {
        switch (conditionType)
        {
            case ConditionType.Equals:
                return intReference == value;
            case ConditionType.NotEquals:
                return intReference != value;
            case ConditionType.LessThan:
                return intReference < value;
            case ConditionType.LessThanOrEqual:
                return intReference <= value;
            case ConditionType.GreaterThan:
                return intReference > value;
            case ConditionType.GreaterThanOrEqual:
                return intReference >= value;
        }

        return false;
    }
}
