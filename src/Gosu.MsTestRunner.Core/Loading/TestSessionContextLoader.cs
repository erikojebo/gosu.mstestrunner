using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gosu.MsTestRunner.Core.Config;
using Gosu.MsTestRunner.Core.Exceptions;
using Gosu.MsTestRunner.Core.Extensions;
using Gosu.MsTestRunner.Core.Listeners;
using Gosu.MsTestRunner.Core.Runner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosu.MsTestRunner.Core.Loading
{
    public class TestSessionContextLoader
    {
        private readonly ConfigFileLoader _configLoader;

        public TestSessionContextLoader(ILogger logger)
        {
            _configLoader = new ConfigFileLoader(logger);
        }

        public async Task<TestSessionContext> Load(string configFilePath)
        {
            var config = await _configLoader.LoadAsync(configFilePath).ConfigureAwait(false);

            if (config.WasParsedSuccessfully)
            {
                return await Task.Run(() => Load(config.Configuration)).ConfigureAwait(false);
            }

            // TODO: This should not be thrown here. Throw in the config loader or pass the result upwards
            throw new GosuMsTestRunnerException("Failed to load config file");
        }

        public async Task<TestSessionContext> LoadAsync(RunnerConfiguration configuration)
        {
            return await Task.Run(() => Load(configuration));
        }

        public TestSessionContext Load(RunnerConfiguration configuration)
        {
            var testSessionContext = new TestSessionContext();

            foreach (var assemblyConfiguration in configuration.Assemblies.Where(x => x.Platform != "x86"))
            {
                var assemblyTestCases = Load(assemblyConfiguration);

                testSessionContext.AddTestCases(assemblyTestCases);
            }

            return testSessionContext;
        }

        private static IEnumerable<TestCase> Load(AssemblyConfiguration assemblyConfiguration)
        {
            var assemblyFileName = Path.GetFileNameWithoutExtension(assemblyConfiguration.Path);
            var assemblyBaseDirectory = Path.GetDirectoryName(assemblyConfiguration.Path);

            AppDomainSetup appDomainSetup = new AppDomainSetup
            {
                ApplicationBase = assemblyBaseDirectory,
                ConfigurationFile = assemblyConfiguration.Path + ".config"
            };

            AppDomain testDomain = AppDomain.CreateDomain($"{assemblyFileName}Domain", null, appDomainSetup);

            var assemblyTestCaseLoader = testDomain.CreateInstance<AssemblyTestCaseLoader>();

            return assemblyTestCaseLoader.Load(assemblyConfiguration.Path);
        }
    }
}