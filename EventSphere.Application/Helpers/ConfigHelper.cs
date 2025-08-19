using Microsoft.Extensions.Configuration;
using System;

namespace backend.Helpers
{
    public static class ConfigHelper
    {
        public static string GetRequired(IConfiguration config, string key, string? errorMessage = null)
        {
            var value = config[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorMessage ?? $"Configuration key '{key}' is not set.");
            return value;
        }
    }
}
