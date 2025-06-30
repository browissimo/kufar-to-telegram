using System;
using System.Text.Json.Serialization;

namespace KufarParserApp.Models
{
    public class HomesModel
    {
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }

        [JsonPropertyName("account_parameters")]
        public AccountParameter[] AccountParameters { get; set; } = Array.Empty<AccountParameter>();

        [JsonPropertyName("ad_id")]
        public int AdId { get; set; }

        [JsonPropertyName("ad_link")]
        public string AdLink { get; set; } = string.Empty;

        [JsonPropertyName("ad_parameters")]
        public AdParameter[] AdParameters { get; set; } = Array.Empty<AdParameter>();

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("body_short")]
        public string BodyShort { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("company_ad")]
        public bool CompanyAd { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public Image[] Images { get; set; } = Array.Empty<Image>();

        [JsonPropertyName("list_id")]
        public int ListId { get; set; }

        [JsonPropertyName("list_time")]
        public DateTime ListTime { get; set; }

        [JsonPropertyName("message_id")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("paid_services")]
        public PaidServices PaidServices { get; set; } = new();

        [JsonPropertyName("phone_hidden")]
        public bool PhoneHidden { get; set; }

        [JsonPropertyName("price_byn")]
        public string PriceByn { get; set; } = string.Empty;

        [JsonPropertyName("price_usd")]
        public string PriceUsd { get; set; } = string.Empty;

        [JsonPropertyName("remuneration_type")]
        public string RemunerationType { get; set; } = string.Empty;

        [JsonPropertyName("show_parameters")]
        public ShowParameters ShowParameters { get; set; } = new();

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class AccountParameter
    {
        public string Pl { get; set; } = string.Empty;
        public string Vl { get; set; } = string.Empty;
        public string P { get; set; } = string.Empty;
        public string V { get; set; } = string.Empty;
        public string Pu { get; set; } = string.Empty;
        public G[]? G { get; set; }
    }

    public class AdParameter
    {
        public string Pl { get; set; } = string.Empty;

        [JsonPropertyName("vl")]
        public object? Vl { get; set; }

        public string P { get; set; } = string.Empty;

        [JsonPropertyName("v")]
        public object? V { get; set; }

        public string Pu { get; set; } = string.Empty;

        public G[]? G { get; set; }
    }

    public class G
    {
        public int Gi { get; set; }
        public string Gl { get; set; } = string.Empty;
        public int Go { get; set; }
        public int Po { get; set; }
    }

    public class PaidServices
    {
        public bool Halva { get; set; }
        public bool Highlight { get; set; }
        public bool Polepos { get; set; }
        public object? Ribbons { get; set; }
    }

    public class ShowParameters
    {
        public bool ShowCall { get; set; }
        public bool ShowChat { get; set; }
        public bool ShowImportLink { get; set; }
        public bool ShowWebShopLink { get; set; }
    }

    public class Image
    {
        public string Id { get; set; } = string.Empty;
        public string MediaStorage { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool YamsStorage { get; set; }
    }
}
