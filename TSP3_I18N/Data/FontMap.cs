using System.Runtime.Serialization;

namespace TSP3_I18N.Data
{
    [DataContract]
    public class FontMap
    {
        [DataMember]
        public string OriginFont { get; set; }
        [DataMember]
        public string CustomFont { get; set; }
        public UnityEngine.Font StaticFont { get; set; }
        public TMPro.TMP_FontAsset DynamicFont { get; set; }
    }
}
