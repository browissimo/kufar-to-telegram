using System;
using System.Text.Json.Serialization;

namespace KufarParserApp.Models
{
    public class HomesModel
    {
        public int AccountId { get; set; }
        public AccountParameter[] AccountParameters { get; set; } = Array.Empty<AccountParameter>();
        public int AdId { get; set; }
        public string AdLink { get; set; } = string.Empty;
        public AdParameter[] AdParameters { get; set; } = Array.Empty<AdParameter>();
        public string? Body { get; set; }
        public string BodyShort { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool CompanyAd { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Image[] Images { get; set; } = Array.Empty<Image>();
        public int ListId { get; set; }
        public DateTime ListTime { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public PaidServices PaidServices { get; set; } = new();
        public bool PhoneHidden { get; set; }
        public string PriceByn { get; set; } = string.Empty;
        public string PriceUsd { get; set; } = string.Empty;
        public string RemunerationType { get; set; } = string.Empty;
        public ShowParameters ShowParameters { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
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
