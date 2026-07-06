using System;
using System.IO;

namespace DailyNotes.Infrastructure.Helpers
{
    public static class EnvLoader
    {
        public static void Load()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var envFile = Path.Combine(dir.FullName, ".env");
                if (File.Exists(envFile))
                {
                    foreach (var line in File.ReadAllLines(envFile))
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                        var parts = line.Split('=', 2);
                        if (parts.Length != 2) continue;
                        var key = parts[0].Trim();
                        var val = parts[1].Trim();
                        if (val.StartsWith("\"") && val.EndsWith("\"")) val = val[1..^1];
                        if (val.StartsWith("'") && val.EndsWith("'")) val = val[1..^1];
                        
                        Environment.SetEnvironmentVariable(key, val);
                    }
                    break;
                }
                dir = dir.Parent;
            }
        }
    }
}
