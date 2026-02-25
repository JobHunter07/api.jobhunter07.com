using System;
using System.IO;
using System.Text.Json;

public static class TestConfig
{
    private static readonly JsonElement? _root = LoadJsonRoot();

    private static JsonElement? LoadJsonRoot()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();
            var path = Path.Combine(baseDir, "appsettings.json");
            if (!File.Exists(path)) return null;
            using var fs = File.OpenRead(path);
            var doc = JsonDocument.Parse(fs);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public static string? GetString(string key)
    {
        var env = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(env)) return env;

        if (_root.HasValue && _root.Value.TryGetProperty(key, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
            return prop.ToString();
        }

        return null;
    }

    public static int GetInt(string key, int fallback)
    {
        var s = GetString(key);
        if (int.TryParse(s, out var v)) return v;
        return fallback;
    }

    public static bool GetBool(string key, bool fallback)
    {
        var s = GetString(key);
        if (bool.TryParse(s, out var v)) return v;
        // allow numeric 1/0
        if (s == "1") return true;
        if (s == "0") return false;
        return fallback;
    }
}
