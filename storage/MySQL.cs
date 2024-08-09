using Config;
using CounterStrikeSharp.API.Core;
using System.Reflection;
using System.Text.Json.Serialization;
public class ModelConfig : BasePluginConfig
{

    [JsonPropertyName("MenuType")] public string MenuType { get; set; } = "centerhtml"; // chat or centerhtml

    [JsonPropertyName("StorageType")] public string StorageType { get; set; } = "sqlite";

    [JsonPropertyName("MySQL_IP")] public string MySQLIP { get; set; } = "";
    [JsonPropertyName("MySQL_Port")] public string MySQLPort { get; set; } = "";
    [JsonPropertyName("MySQL_User")] public string MySQLUser { get; set; } = "";
    [JsonPropertyName("MySQL_Password")] public string MySQLPassword { get; set; } = "";
    [JsonPropertyName("MySQL_Database")] public string MySQLDatabase { get; set; } = "";
    [JsonPropertyName("MySQL_Table")] public string MySQLTable { get; set; } = "playermodelchanger";

    [JsonPropertyName("ModelForBots")] public BotsConfig ModelForBots { get; set; } = new BotsConfig();

    [JsonPropertyName("DisablePrecache")] public bool DisablePrecache { get; set; } = false;
    [JsonPropertyName("DisableRandomModel")] public bool DisableRandomModel { get; set; } = false;
    [JsonPropertyName("DisableAutoCheck")] public bool DisableAutoCheck { get; set; } = false;
    [JsonPropertyName("AutoResyncCache")] public bool AutoResyncCache { get; set; } = false;
}

public interface IStorage
{
    public string? GetPlayerTModel(ulong SteamID);
    public string? GetPlayerCTModel(ulong SteamID);
    public Task<int> SetPlayerTModel(ulong SteamID, string modelName, bool permissionBypass);

    public Task<int> SetPlayerCTModel(ulong SteamID, string modelName, bool permissionBypass);

    public Task<int> SetAllTModel(string tmodel, bool permissionBypass);
    public Task<int> SetAllCTModel(string ctmodel, bool permissionBypass);
}