using Gosu.MsTestRunner.Core.Config;

namespace Gosu.MsTestRunner.Core.Loading
{
    public class ConfigFileParseResult
    {
        public readonly bool WasParsedSuccessfully;
        public readonly string ErrorDescription;
        public readonly RunnerConfiguration Configuration;

        private ConfigFileParseResult(RunnerConfiguration configuration)
        {
            Configuration = configuration;
            WasParsedSuccessfully = true;
        }

        private ConfigFileParseResult(string errorDescription)
        {
            ErrorDescription = errorDescription;
            WasParsedSuccessfully = false;
        }

        public static ConfigFileParseResult Failure(string errorDescription)
        {
            return new ConfigFileParseResult(errorDescription);
        }

        public static ConfigFileParseResult Success(RunnerConfiguration configuration)
        {
            return new ConfigFileParseResult(configuration);
        }
    }
}