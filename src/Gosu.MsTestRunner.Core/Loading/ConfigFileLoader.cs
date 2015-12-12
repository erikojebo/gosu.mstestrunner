using System;
using System.IO;
using System.Threading.Tasks;
using Gosu.MsTestRunner.Core.Config;
using Gosu.MsTestRunner.Core.Listeners;
using Newtonsoft.Json;

namespace Gosu.MsTestRunner.Core.Loading
{
    public class ConfigFileLoader
    {
        private readonly ILogger _logger;

        public ConfigFileLoader(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<ConfigFileParseResult> LoadAsync(string configFilePath)
        {
            return await Task.Run(() => Load(configFilePath)).ConfigureAwait(false);
        }

        public ConfigFileParseResult Load(string configPath)
        {
            if (!File.Exists(configPath))
            {
                string errorMessage = $"No configuration file could be found at the specified path: {configPath}";

                _logger.WriteError(errorMessage);
                
                return ConfigFileParseResult.Failure(errorMessage);
            }

            RunnerConfiguration config;

            try
            {
                var fileContents = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<RunnerConfiguration>(fileContents);
            }
            catch (Exception ex)
            {
                var message = $"Could not parse configuration file at path '{configPath}'.\r\nException: {ex}";

                _logger.WriteError(message);

                return ConfigFileParseResult.Failure(message);
            }

            return ConfigFileParseResult.Success(config);
        }
    }
}