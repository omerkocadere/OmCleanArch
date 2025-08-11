using System;
using System.Text.Json.Serialization;
using CleanArch.Domain.Members;

namespace API.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? PublicId { get; set; }

    // Navigation property
    [JsonIgnore]
    public required Member Member { get; set; }
    public required Guid MemberId { get; set; }  // Guid olarak değiştirdim
}
