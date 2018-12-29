using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Essentials.AspNetCore.Docker.Configuration
{
    /// <summary>
    /// A docker secrets based <see cref="ConfigurationProvider"/>. Docker secrets casing does not matter, but nested
    /// configuration paths (such as logging:minimumLevel) need to be named with an underscore (_) instead of colon (:).
    /// This is a limitation of *nix docker hosts
    /// </summary>
    public class SecretsConfigurationProvider : ConfigurationProvider
    {
        SecretsConfigurationSource Source { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="source">The configuration.</param>
        public SecretsConfigurationProvider(SecretsConfigurationSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Loads the docker secrets.
        /// </summary>
        public override void Load()
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Options didn't set a FileProvider, default to PhysicalFileProvider
            if (Source.FileProvider == null)
            {
                if (Directory.Exists(Source.SecretsDirectory))
                {
                    Source.FileProvider = new PhysicalFileProvider(Source.SecretsDirectory);
                }
                else if (Source.Optional)
                {
                    return;
                }
                else
                {
                    throw new DirectoryNotFoundException("DockerSecrets directory doesn't exist and is not optional.");
                }
            }

            var secretsDir = Source.FileProvider.GetDirectoryContents("/");
            if (!secretsDir.Exists && !Source.Optional)
            {
                throw new DirectoryNotFoundException("DockerSecrets directory doesn't exist and is not optional.");
            }

            foreach (var file in secretsDir)
            {
                // Ignore nested directories
                if (file.IsDirectory)
                {
                    continue;
                }

                if (Source.IgnoreCondition != null && Source.IgnoreCondition(file.Name)) continue;
                
                using (var stream = file.CreateReadStream())
                using (var streamReader = new StreamReader(stream))
                {
                    var configKey = NormalizeKey(file.Name);
                    var value = streamReader.ReadToEnd();
                    Data.Add(configKey, value);
                }
            }
        }
        
        /// <summary>
        /// Normalize the name of a key, replacing double underscores with KeyDelimiter (usually a colon). This corrects file
        /// names from *nix-compatible to be config-friendly. 
        /// <param name="key"></param>
        /// <returns></returns>
        private static string NormalizeKey(string key)
        {
            // Replacing double underscores with KeyDelimeter
            // see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2#conventions
            return key.Replace("__", ConfigurationPath.KeyDelimiter);
        }
    }
}