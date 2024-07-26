using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EventBus.Base.Events
{
    public class IntegrationEvent
    {
        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public DateTime CreateTime { get; private set; }

        public IntegrationEvent(DateTime createTime, Guid id)
        {
            CreateTime = createTime;
            Id = id;
        }

        [JsonConstructor]
        public IntegrationEvent()
        {
            CreateTime = DateTime.Now;
            Id = Guid.NewGuid();
        }

    }
}
