using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Essentials.AspNetCore.Docker.Configuration
{
    /// <summary>
    /// Extensions on IWebHostBuilder for docker configuration
    /// </summary>
    public static class SecretsWebHostBuilderExtensions
    {
        /// <summary>
        /// Adds docker secrets configuration to app configuration
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="secretsDirectory">Path to secrets directory</param>
        /// <param name="optional">Whether the directory is optional</param>
        /// <returns>The <see cref="IWebHostBuilder"/>WebHostBuilder with Docker secrets configuration provider added</returns>
        public static IWebHostBuilder AddDockerSecrets(this IWebHostBuilder builder,
                                                       string secretsDirectory,
                                                       bool optional)
        {
            return builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                                                         configurationBuilder.AddDockerSecrets(secretsDirectory, optional));
        }
        
        /// <summary>
        /// Adds docker secrets configuration to app configuration, using the default secrets directory of /run/secrets
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="optional">Whether the directory is optional</param>
        /// <returns>The <see cref="IWebHostBuilder"/>WebHostBuilder with Docker secrets configuration provider added</returns>
        public static IWebHostBuilder AddDockerSecrets(this IWebHostBuilder builder,
                                                       bool optional)
        {
            return builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                                                         configurationBuilder.AddDockerSecrets(optional));
        }
        
        
        /// <summary>
        /// Adds docker secrets configuration to app configuration, using the default secrets directory of /run/secrets,
        /// and throwing an exception if it does not exist.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        public static IWebHostBuilder AddDockerSecrets(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, configurationBuilder) =>
                                                         configurationBuilder.AddDockerSecrets());
        }

        /// <summary>
        /// Adds docker secrets configuration to app configuration only if the hosting environment is not development
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <returns>The <see cref="IWebHostBuilder"/></returns>
        public static IWebHostBuilder AddDockerSecretsExceptInDevelopment(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                if (context.HostingEnvironment.IsDevelopment()) return;
                configurationBuilder.AddDockerSecrets(false);
            });
        }
    }
}