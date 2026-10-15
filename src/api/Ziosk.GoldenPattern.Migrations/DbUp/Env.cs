using System;

namespace Ziosk.DbUp;

public enum Env
{
    Undefined = 0,
    LOCAL = 1,
    DEV = 2,
    QA = 3,
    UAT = 4,
    Prod = 5
}

public static class EnvParser
{
    public static Env Parse(string? value)
    {
        return value switch
        {
            null => Env.Undefined,
            "" => Env.Prod,
            _ => Enum.TryParse(value, true, out Env returnValue) ? returnValue : Env.Undefined
        };
    }
}