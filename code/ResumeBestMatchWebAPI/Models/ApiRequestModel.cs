using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResumeBestMatchWebAPI.Models
{
    public class ApiRequestModel
    {
        public string context { get; set; }
        public string category { get; set; }
        public string threshold { get; set; }
        public string noOfMatches { get; set; }
        public string inputPath { get; set; }
    }
}