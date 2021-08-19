using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Amazon.S3.Transfer;
using APIGatewayLambdaSample.Models;

namespace APIGatewayLambdaSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private static readonly Amazon.RegionEndpoint bucketRegion = Amazon.RegionEndpoint.USEast1;
        private const string bucketName = "sample-bucket-rakesh-s3";
        private IAmazonS3 s3Client;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<UploadController> _logger;

        public UploadController(ILogger<UploadController> logger)
        {
            _logger = logger;
            s3Client = new AmazonS3Client(bucketRegion);
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("fileWithBody")]        
        public async Task<IActionResult> UploadFileWithBody([FromForm] FileInfoObject fileInfoObject) 
        {
            IFormFile inputFile = fileInfoObject.File;
            string fileType = fileInfoObject.FileType;

            Console.WriteLine($"File upload start for : {inputFile.FileName} of type {fileType}");

            try
            {
                string inputFileName = Path.GetFileName(inputFile.FileName);
                string inputFilePath = "/tmp/" + inputFileName;     // storing the file in Lambda temporary storage (current limit is 512 MB)

                using (var fs = new FileStream(inputFilePath, FileMode.Create))
                {
                    inputFile.CopyTo(fs);
                    fs.Close();

                    // now send recieved file to S3 bucket
                    var fileTransferUtility = new TransferUtility(s3Client);
                    await fileTransferUtility.UploadAsync(inputFilePath, bucketName);

                    Console.WriteLine($"file upload complete for {inputFileName} : {inputFile.Length} bytes");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR : file upload failed for {inputFile.FileName} due to : {ex.Message}");
                return new OkObjectResult($"File upload failed");
            }

            return new OkObjectResult($"file upload completed");
        }


    }
}
