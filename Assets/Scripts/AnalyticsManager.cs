using System.Collections.Generic;
using UnityEngine;

public static class AnalyticsManager
{
    public static void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
#if UNITY_ANALYTICS
        UnityEngine.Analytics.Analytics.CustomEvent(eventName, parameters);
#else
        Debug.Log($"[Analytics] {eventName}: {FormatParams(parameters)}");
#endif
    }

    private static string FormatParams(Dictionary<string, object> p)
    {
        if (p == null) return "{}";
        var parts = new List<string>();
        foreach (var kv in p) parts.Add($"{kv.Key}={kv.Value}");
        return "{" + string.Join(",", parts) + "}";
    }
}
