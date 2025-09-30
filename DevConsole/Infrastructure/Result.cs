using System;
using System.Diagnostics.CodeAnalysis;

namespace DevConsole.Infrastructure;

/// <summary>
///     Represents the result of an operation, which can either succeed or fail.
/// </summary>
/// <typeparam name="TValue">The type of value returned if the operation is successful</typeparam>
/// <typeparam name="TStatus">The status of the operation. NOTE: The default value of this type must represent success, all other values will represent failure.</typeparam>
public readonly struct Result<TValue, TStatus> where TStatus : Enum
{
    private Result(TValue? value, TStatus status, bool success)
    {
        Value = value;
        Status = status;
        Successful = success;
    }

    public TValue? Value { get; }

    public TStatus Status { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool Successful { get; }

    public static implicit operator Result<TValue, TStatus>(TValue? value) => new(value, default!, value is not null);

    public static implicit operator Result<TValue, TStatus>(TStatus status) => new(default, status, false);
}

public readonly struct Result<TValue>
{
    private Result(TValue? value, bool success)
    {
        Value = value;
        Successful = success;
    }

    public TValue? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    public bool Successful { get; }

    public static Result<TValue> Fail() => new(default, false);

    public static implicit operator Result<TValue>(TValue? value) => new(value, value is not null);
}

public readonly struct ValueResult<TValue> where TValue : unmanaged
{
    private ValueResult(TValue value, bool success)
    {
        Value = value;
        Successful = success;
    }

    public TValue Value { get; }

    public bool Successful { get; }

    public static ValueResult<TValue> Fail() => new(default, false);

    public static implicit operator ValueResult<TValue>(TValue value) => new(value, true);
}