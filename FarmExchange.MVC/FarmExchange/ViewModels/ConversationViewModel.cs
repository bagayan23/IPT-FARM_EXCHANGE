using FarmExchange.Models;

namespace FarmExchange.ViewModels
{
    public class ConversationViewModel
    {
        public Guid PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public Message LastMessage { get; set; } = null!;
        public int UnreadCount { get; set; }
    }
}