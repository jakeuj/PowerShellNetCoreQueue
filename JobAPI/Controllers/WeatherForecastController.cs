using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IBackgroundTaskQueue taskQueue)
        {
            _logger = logger;
            _taskQueue = taskQueue;
        }

        [HttpGet]
        public async Task<string> Get(int sec=15)
        {
            _logger.LogInformation($"加入一個 {sec} 秒的 PowerShell 作業");
            await _taskQueue.QueueBackgroundWorkItemAsync(async ct =>
            {
                var guid = Guid.NewGuid().ToString();

                _logger.LogInformation($"開始一個 {sec} 秒的 PowerShell 作業 {guid}");
            
                await RunScript($"Start-Sleep -s {sec}", null);
            
                _logger.LogInformation($"完成一個 {sec} 秒的 PowerShell 作業 {guid}");
            });
            return $"加入一個 {sec} 秒的 PowerShell 作業";
        }

        /// <summary>
        /// Runs a PowerShell script with parameters and prints the resulting pipeline objects to the console output. 
        /// </summary>
        /// <param name="scriptContents">The script file contents.</param>
        /// <param name="scriptParameters">A dictionary of parameter names and parameter values.</param>
        private async Task RunScript(string scriptContents, Dictionary<string, object> scriptParameters)
        {
            // create a new hosted PowerShell instance using the default runspace.
            // wrap in a using statement to ensure resources are cleaned up.
   
            using (PowerShell ps = PowerShell.Create())
            {
                // specify the script code to run.
                ps.AddScript(scriptContents);
     
                // specify the parameters to pass into the script.
                if(scriptParameters != null)
                    ps.AddParameters(scriptParameters);
     
                // execute the script and await the result.
                var pipelineObjects = await ps.InvokeAsync().ConfigureAwait(false);
     
                // print the resulting pipeline objects to the console.
                foreach (var item in pipelineObjects)
                {
                    Console.WriteLine(item.BaseObject.ToString());
                }
            }
        }
    }
}