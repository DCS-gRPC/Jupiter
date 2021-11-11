namespace RurouniJones.Jupiter.Core.Models
{
    public class EventSummary
    {
        public double Timestamp { get; }
        public string PlayerName { get; }
        public string EventType { get; }
        public string UnitId { get; }
        public string Details { get; }

        public EventSummary(double timestamp, string eventType, string playerName,  string unitId, string details)
        {
            Timestamp = timestamp;
            PlayerName = playerName;
            EventType = eventType;
            UnitId = unitId;
            Details = details;
        }
    }
}
