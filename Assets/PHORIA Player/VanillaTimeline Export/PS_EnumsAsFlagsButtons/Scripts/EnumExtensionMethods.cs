using System;

public static class EnumExtension {

    public static bool HasFlag(this Enum value, Enum compareValue) {
        if (value == null)
            return false;

        if (compareValue == null)
            throw new ArgumentNullException("value");

        if (Enum.IsDefined(value.GetType(), compareValue) == false) {
            throw new ArgumentException(string.Format("Enum type mismatch. The source enum is of type '{0}' compared to type '{1}'.", value.GetType(), compareValue.GetType()));
        }

        ulong compareFlag = Convert.ToUInt64(compareValue);
        return ((Convert.ToUInt64(value) & compareFlag) == compareFlag);
    }

}