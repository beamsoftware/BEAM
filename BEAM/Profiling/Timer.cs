using System;
using System.Diagnostics;

namespace BEAM.Profiling;

public delegate void TimerEndEvent(TimerEventArgs timerEventArgs);

public class TimerEventArgs(string name, Stopwatch watch)
{
    public string Name => name;
    public Stopwatch Watch => watch;
}

public class TimerScopeToken(string name, Stopwatch watch) : IDisposable
{
    public event TimerEndEvent OnTimerEnd = delegate { };

    public void Dispose()
    {
        OnTimerEnd?.Invoke(new TimerEventArgs(name, watch));
        GC.SuppressFinalize(this);
    }
}

public static class Timer
{
    public static event TimerEndEvent TimerEnd = delegate { };

    public static TimerScopeToken Start(string? name = null)
    {
        var method = new StackFrame(1).GetMethod()!;
        var className = method.DeclaringType!.Name;
        var ns = method.DeclaringType.Namespace;

        var token = new TimerScopeToken(name ?? $"{ns}.{className}.{method.Name}()", Stopwatch.StartNew());
        token.OnTimerEnd += TimerEnd;
        return token;
    }
}