using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Anexia.Monitoring;
using Anexia.Monitoring.Controllers;
using Anexia.Monitoring.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace VersionMonitorNetCore.Test.UnitTests.Controllers
{
	
    [Trait("Category", "GitHub")]
    public class MonitoringControllerTests
    {
        private readonly string _accestoken;
        private readonly MonitoringController _monitoringController;

        public MonitoringControllerTests()
        {
            _monitoringController = new MonitoringController();
            _accestoken = Guid.NewGuid().ToString();
            VersionMonitor.SetAccessToken(_accestoken);
        }

        [Fact]
        public void GetServiceStatesTest()
        {
            var result = _monitoringController.GetServiceStates(_accestoken);
            if (result is OkObjectResult okObjectResult)
            {
                Assert.True((string)okObjectResult.Value == "OK");
            }
            else
            {
                Assert.True(false, "Not a OkObjectResult");
            }
        }

        [Fact]
        public async Task GetModulesInfoTest()
        {
            var result = await _monitoringController.GetModulesInfo(_accestoken);
            if (result is OkObjectResult okObjectResult)
            {
                var val = okObjectResult.Value.GetType().GetProperty("runtime")?.GetValue(okObjectResult.Value, null);
                if (val is RuntimeInfo runtimeInfoValue)
                {
                    var frameWorkVersion = PlatformServices.Default.Application.RuntimeFramework;
                    Assert.True(
                        frameWorkVersion.Version.ToString() == runtimeInfoValue.FrameworkInstalledVersion,
                        "Not a Actual version");
                }
                else
                {
                    Assert.True(false, "RuntimeInfo not existing");
                }

                val = okObjectResult.Value.GetType().GetProperty("modules")?.GetValue(okObjectResult.Value, null);
                if (val is List<ModuleInfo> listModuleInfo)
                {
                    Assert.NotEmpty(listModuleInfo);
                }
                else
                {
                    Assert.True(false, "List module info is not existing");
                }
            }
            else
            {
                Assert.True(false, "Not a OkObjectResult or Value not DynamicObject");
            }
        }
    }
}