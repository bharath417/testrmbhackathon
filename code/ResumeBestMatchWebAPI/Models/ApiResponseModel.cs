using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResumeBestMatchWebAPI.Models
{
    public class ApiResponseModel
    {
        [Order(0)]
        public string status { get; set; }
        [Order(1)]
        public int count { get; set; }
        [Order(2)]
        public string confidence { get; set; }
        [Order(3)]
        public List<results> results { get; set; }
        public ApiResponseModel() { }
    }
    public class results
    {
        public int id { get; set; }
        public string score { get; set; }
        public string path { get; set; }
    }
    public class OrderAttribute:Attribute
    {
        public OrderAttribute(int value)
        {
            this.Value = value;
        }
        public int Value { get; private set; }
    }
}