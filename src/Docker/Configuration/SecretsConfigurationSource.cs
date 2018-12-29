using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Essentials.AspNetCore.Docker.Configuration
{
    /// <summary>
    /// A <see cref="ConfigurationProvider"/> for docker secrets.
    /// </summary>
    public class SecretsConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SecretsConfigurationSource()
        {
            IgnoreCondition = s => IgnorePrefix != null && s.StartsWith(IgnorePrefix);
        }

        /// <summary>
        /// The secrets directory which will be used if FileProvider is not set. Defaults to /run/secrets
        /// </summary>
        public string SecretsDirectory { get; set; } = "/run/secrets";

        /// <summary>
        /// The FileProvider representing the secrets directory.
        /// </summary>
        public IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Docker secrets that start with this prefix will be excluded. Defaults to 'ignore.'
        /// </summary>
        public string IgnorePrefix { get; set; } = "ignore.";

        /// <summary>
        /// Used to determine if a file should be ignored based on its name.
        /// </summary>
        public Func<string, bool> IgnoreCondition { get; set; }

        /// <summary>
        /// If false, will throw if the secrets directory doesn't exist.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Builds the <see cref="SecretsConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="SecretsConfigurationProvider"/></returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new SecretsConfigurationProvider(this);
        }
    }
}
