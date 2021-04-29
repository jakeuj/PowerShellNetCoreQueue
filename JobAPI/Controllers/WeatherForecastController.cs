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

                _logger.LogInformation($"開始一個 PowerShell 作業 {guid}");
                (await PowerShell.Create()
                        .AddScript($"Start-Sleep -s {sec}")
                        .InvokeAsync())
                    .ToList()
                    .ForEach(Console.WriteLine);
                _logger.LogInformation($"完成一個 PowerShell 作業 {guid}");
            });
            return $"加入一個 {sec} 秒的 PowerShell 作業";
        }
    }
}