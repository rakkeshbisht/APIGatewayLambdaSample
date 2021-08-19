using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APIGatewayLambdaSample.Models
{   public class FileInfoObject
    {
        public string FileType { get; set; }
        public IFormFile File { get; set; }
    }
}
