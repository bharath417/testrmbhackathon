using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ResumeBestMatchWebAPI.Models
{
    public class ApiRequestModel
    {
        //public ApiRequestModel();

        public string context { get; set; }
        public string category { get; set; }
        public decimal threshold { get; set; }
        public int noOfMatches { get; set; }
        public string inputPath { get; set; }
    }
}