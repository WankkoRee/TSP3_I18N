using System.Runtime.Serialization;

namespace TSP3_I18N.Data
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
