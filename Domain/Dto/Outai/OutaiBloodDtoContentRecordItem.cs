namespace MyDotnet.Domain.Dto.Outai
{
    public class OutaiBloodDtoContentRecordItem
    {
        public string bgNotes { get; set; }
        public string bloodSourceValue { get; set; }
        public int dietRecordId { get; set; }
        public bool hasDietRecord { get; set; }
        public bool hasSportRecord { get; set; }
        public int noteState { get; set; }
        public int sportRecordId { get; set; }
        public string time { get; set; }
        public string trend { get; set; }
        public double value { get; set; }
        public string direction { get; set; }
        public DateTime timeFormat { get; set; }
    }
}
