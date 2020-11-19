using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SpookVooper.Api.Stocks
{
    public class OwnerData
    {
        [Key]
        [JsonProperty("ownerId")] 
        public string Owner_Id { get; set; }

        [JsonProperty("ownerName")]
        public string Owner_Name { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
