using System.Runtime.Serialization;

namespace TheStanleyParableUltraDeluxe_Chinese
{
    [DataContract]
    public class Dict
    {
        [DataMember]
        public string Term { get; set; }
        [DataMember]
        public string Chinese { get; set; }
    }
}
