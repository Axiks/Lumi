using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vanilla_App.Services.Bonus.Repository
{
    public class TakeBonusModel
    {
        [JsonPropertyName("message")]
        public required string Message { get; init; }
        [JsonPropertyName("date_of_used")]
        public required DateTime DateOfUsed { get; init; }
    }
}
