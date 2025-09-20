using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

    public static void Subscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (_handlers.TryGetValue(t, out var existing))
            _handlers[t] = Delegate.Combine(existing, handler);
        else
            _handlers[t] = handler;
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        var t = typeof(T);
        if (_handlers.TryGetValue(t, out var existing))
        {
            var current = Delegate.Remove(existing, handler);
            if (current == null) _handlers.Remove(t);
            else _handlers[t] = current;
        }
    }

    public static void Publish<T>(T evt)
    {
        var t = typeof(T);
        if (_handlers.TryGetValue(t, out var d))
        {
            var cb = d as Action<T>;
            cb?.Invoke(evt);
        }
    }
}

// Ejemplos de eventos
public struct SeasonChanged { public WeatherManager.Season season; }
public struct WeatherChanged { public WeatherManager.Weather weather; }
public struct RaidTriggered { public int threatLevel; }