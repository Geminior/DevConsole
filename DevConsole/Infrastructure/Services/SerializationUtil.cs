using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace DevConsole.Infrastructure.Services;

public static class SerializationUtil
{
    public static readonly JsonSerializerSettings DefaultSerializerSettings = new()
    {
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter(new DefaultNamingStrategy())
        },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static readonly JsonSerializerSettings CompactSerializerSettings = new()
    {
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter(new DefaultNamingStrategy())
        },
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static string SerializeCompact<T>(T obj) => JsonConvert.SerializeObject(obj, CompactSerializerSettings);

    public static string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, DefaultSerializerSettings);

    public static T? DeserializeCompact<T>(string json) => JsonConvert.DeserializeObject<T>(json, CompactSerializerSettings);

    public static T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, DefaultSerializerSettings);
}