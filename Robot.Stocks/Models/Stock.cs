using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Robot.Stocks.Models
{
    public class Stock
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("period")]
        public string Period { get; set; }
    }
}
