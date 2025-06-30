using kufar_to_telegram.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace kufar_to_telegram.Telegram;

public class MediaGroupBuilder
{
    private readonly List<MediaItem> _mediaItems = new();
    private readonly List<(ByteArrayContent Content, string Name)> _attachments = new();
    private readonly string? _caption;
    private int _attachmentIndex = 0;

    public MediaGroupBuilder(string? caption = null)
    {
        _caption = caption;
    }

    public void AddPhoto(byte[] photoBytes)
    {
        var attachName = $"photo_{_attachmentIndex++}";

        var content = new ByteArrayContent(photoBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

        _attachments.Add((content, attachName));

        var mediaItem = new MediaItem
        {
            Media = $"attach://{attachName}",
            Type = "photo",
            Caption = (_attachmentIndex == 1 && !string.IsNullOrWhiteSpace(_caption)) ? _caption : null
        };

        _mediaItems.Add(mediaItem);
    }

    public MultipartFormDataContent Build(string chatId)
    {
        var formData = new MultipartFormDataContent();

        foreach (var (content, name) in _attachments)
        {
            formData.Add(content, name, name);
        }

        formData.Add(new StringContent(chatId), "chat_id");

        var json = JsonSerializer.Serialize(_mediaItems, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        formData.Add(new StringContent(json, Encoding.UTF8, "application/json"), "media");

        return formData;
    }

    public bool HasMedia => _mediaItems.Count > 0;
}
