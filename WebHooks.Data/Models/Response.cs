using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebHooks.Data.Models
{
    public class Response<T>
    {
        [JsonProperty(PropertyName = "value")]
        public List<T> Value { get; set; }
    }
}
