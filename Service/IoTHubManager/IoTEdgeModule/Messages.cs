
namespace IoTEdgeModule
{
    using Newtonsoft.Json;
    using System;

    class Messages
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "ambient")]
        public Ambient Ambient { get; set; }

        [JsonProperty(PropertyName = "time_stamp")]
        public DateTime Timestamp { get; set; }
    }

    class Ambient
    {
        [JsonProperty(PropertyName = "temperature")]
        public double Temperature { get; set; }

        [JsonProperty(PropertyName = "humidity")]
        public int Humidity { get; set; }

        [JsonProperty(PropertyName = "moisture")]
        public int Moisture { get; set; }

        [JsonProperty(PropertyName = "pH")]
        public double pH { get; set; }
    }
}
