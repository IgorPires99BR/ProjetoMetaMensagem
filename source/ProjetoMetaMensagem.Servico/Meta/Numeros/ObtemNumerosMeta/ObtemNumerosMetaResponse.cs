using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Meta.Numeros.ObtemNumerosMeta
{
    public class ObtemNumerosMetaResponse
    {
        [JsonProperty("data")]
        public List<MetaPhoneNumberData> Data { get; set; }

        [JsonProperty("paging")]
        public MetaPaging Paging { get; set; }

    }

    public class MetaPhoneNumberData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("display_phone_number")]
        public string DisplayPhoneNumber { get; set; }

        [JsonProperty("verified_name")]
        public string VerifiedName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("quality_rating")]
        public string QualityRating { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("account_mode")]
        public string AccountMode { get; set; }

        [JsonProperty("messaging_limit_tier")]
        public string MessagingLimitTier { get; set; }
    }

    public class MetaPaging
    {
        [JsonProperty("cursors")]
        public MetaCursors Cursors { get; set; }
    }

    public class MetaCursors
    {
        [JsonProperty("before")]
        public string Before { get; set; }

        [JsonProperty("after")]
        public string After { get; set; }
    }
}
