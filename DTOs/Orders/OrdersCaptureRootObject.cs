using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.Orders
{
    public class OrdersCaptureRootObject
    {
        [JsonProperty("captured")]
        public bool Captured { get; set; }

        [JsonProperty("messages")]
        public List<Message> Messages { get; set; }
        public OrdersCaptureRootObject()
        {
            Messages = new List<Message>();
        }


        public class Message
        {
            [JsonProperty("messagetype")]
            public string MessageType { get; set; }
            [JsonProperty("messagetext")]
            public string MessageText { get; set; }
        }


    }
}