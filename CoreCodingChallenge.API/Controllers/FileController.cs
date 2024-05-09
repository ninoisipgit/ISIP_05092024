using CoreCodingChallenge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace CoreCodingChallenge.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private const string FileCounterKey = "FileCounter";
        private const string LoggerMessagesKey = "LoggerMessages";

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Retrieves tracking information.
        /// </summary>
        /// <remarks>
        /// Add X-API-Key header to authenticate the request.
        /// </remarks>
        /// <response code="200">Returns the tracking information.</response>
        [HttpGet("tracking")]
        [ProducesResponseType(typeof(TrackingInfo), 200)]
        public IActionResult GetTrackingInfo()
        {
            var fileCounter = GetSessionValue<int>(FileCounterKey);
            var loggerMessages = GetSessionValue<List<string>>(LoggerMessagesKey);

            var trackingInfo = new TrackingInfo
            {
                FileCounter = fileCounter,
                LoggerMessages = loggerMessages ?? new List<string>()
            };

            return Ok(trackingInfo);
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadFileFromSwagger(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    if (file.ContentType == "text/csv")
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                        {
                            var records = csv.GetRecords<CsvModel>();
                            var average = records.Select(r => r.Age).Average();

                            IncrementFileCounter();
                            LogMessage($"CSV file processed: {file.FileName}, Average Age: {average}");

                            return Ok($"Average: {average}");
                        }
                    }
                    else if (file.ContentType == "application/json")
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var json = await reader.ReadToEndAsync();
                            var data = JsonConvert.DeserializeObject<JsonModel[]>(json);
                            var filteredData = data.Where(d => d.Age >= 30);

                            IncrementFileCounter();
                            LogMessage($"JSON file processed: {file.FileName}, Filtered Data Count: {filteredData.Count()}");

                            return Ok(filteredData);
                        }
                    }
                    else
                    {
                        return BadRequest("Unsupported file format.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error uploading file: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading file: {ex.Message}");
            }
        }

        private void IncrementFileCounter()
        {
            var fileCounter = GetSessionValue<int>(FileCounterKey);
            fileCounter++;
            SetSessionValue(FileCounterKey, fileCounter);
        }

        private void LogMessage(string message)
        {
            var loggerMessages = GetSessionValue<List<string>>(LoggerMessagesKey) ?? new List<string>();
            loggerMessages.Add(message);
            SetSessionValue(LoggerMessagesKey, loggerMessages);
        }

        private T GetSessionValue<T>(string key)
        {
            var data = HttpContext.Session.Get(key);
            if (data == null)
            {
                return default(T);
            }
            else
            {
                string jsonString = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
        }

        private void SetSessionValue<T>(string key, T value)
        {
            string jsonString = JsonConvert.SerializeObject(value);
            byte[] data = Encoding.UTF8.GetBytes(jsonString);
            HttpContext.Session.Set(key, data);
        }

    }
}
