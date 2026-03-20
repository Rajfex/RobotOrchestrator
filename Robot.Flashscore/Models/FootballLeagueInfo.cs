using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Robot.Models
{
    public class FootballLeagueInfo
    {
        [JsonPropertyName("country")]
        public string Country { get; set; }
        [JsonPropertyName("leagueName")]
        public string LeaguseName { get; set; }
    }
}
