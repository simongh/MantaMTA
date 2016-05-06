using OpenManta.Core;

namespace WebInterface.Models.API.v1
{
    public class UpdateOutboundRuleViewModel
    {
        public int PatternID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? VirtualMTA { get; set; }
        public OutboundMxPatternType Type { get; set; }
        public string PatternValue { get; set; }
        public int MaxConnections { get; set; }
        public int MaxMessagesConn { get; set; }
        public int MaxMessagesHour { get; set; }
    }
}