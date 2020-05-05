
namespace EdgeManagement.DeviceService.Models
{
    using System.Collections.Generic;
    using System;

    public class Messages
    {
        public IDictionary<string, MessageInfo> MessageInfos { get; set; }
    }

    public class MessageInfo
    {
        public DateTime Timestamp { get; set; }
        public int Message_Count { get; set; }
        public string Message { get; set; }
    }
}
