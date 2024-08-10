using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using System.Text.Json.Serialization;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using Dapper;


namespace ANNABEL;

[MinimumApiVersion(80)]
public class ANNABEL : BasePlugin
{
    public override string ModuleName => "Example: With Database (MySQL)";
    public override string ModuleVersion => "2.1.0";
    public override string ModuleAuthor => "Annabel Hugles";
    public override string ModuleDescription => "A plugin that reads and writes from the database.";

    private MySqlConnection _connection = null!;

    public string VpnNotificationMessage { get; set; } = "server=localhost;uid=ogpuser;pwd=0073007;database=store";

    private int resultadoSubtracao;

    private CCSPlayerController? executingPlayer;

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("Loading database from {ConnectionString}", GetConnectionString());

        _connection = new MySqlConnection(GetConnectionString());
        _connection.Open();

        Task.Run(async () =>
        {
            await _connection.ExecuteAsync(@"
         CREATE TABLE IF NOT EXISTS `players` (
              `steamid` BIGINT UNSIGNED NOT NULL,
              `kills` INT NOT NULL DEFAULT 0,
              `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- Added timestamp column
              PRIMARY KEY (`steamid`));");
        });
    }

    private string GetConnectionString()
    {
        return VpnNotificationMessage;
    }

    [GameEventHandler]
    public HookResult OnPlayerKilled(EventPlayerDeath @event, GameEventInfo info)
    {

        if (@event.Attacker == @event.Userid) return HookResult.Continue;

        var steamId = @event.Attacker?.AuthorizedSteamID?.SteamId64;
        var timestamp = DateTime.UtcNow; 

        if (steamId == null) return HookResult.Continue;
        Task.Run(async () =>
        {
            await _connection.ExecuteAsync(@"
          INSERT INTO `players` (`steamid`, `kills`, `timestamp`) VALUES (@SteamId, 1, @Timestamp)
          ON DUPLICATE KEY UPDATE `kills` = `kills` + 1, `timestamp` = @Timestamp;",
                new
                {
                    SteamId = steamId,
                    Timestamp = timestamp
                });
        });

        return HookResult.Continue;
    }

    private bool HasPermission(CCSPlayerController player, string id)
    {
        string permission = string.Empty;

        switch (id)
        {
            case "Permission":
                permission = "@css/custom-permission";
                break;
            case "Permission2":
                permission = "@css/reservation";
                break;
            case "Permission3":
                permission = "@css/generic";
                break;
            case "Permission4":
                permission = "@css/kick";
                break;
            case "Permission5":
                permission = "@css/ban";
                break;
            case "Permission6":
                permission = "@css/vip";
                break;
            case "Permission7":
                permission = "@css/slay";
                break;
            case "Permission8":
                permission = "@css/changemap";
                break;
            case "Permission9":
                permission = "@css/cvar";
                break;
            case "Permission10":
                permission = "@css/config";
                break;
            case "Permission11":
                permission = "@css/chat";
                break;
            case "Permission12":
                permission = "@css/vote";
                break;
            case "Permission13":
                permission = "@css/password";
                break;
             case "Permission14":
                permission = "@css/rcon";
                break;
             case "Permission15":
                permission = "@css/cheats";
                break;
            case "Permission16":
                permission = "@css/root";
                break;
            
        }

        return (string.IsNullOrEmpty(permission) || AdminManager.PlayerHasPermissions(player, permission));
    }


    [ConsoleCommand("css_morte", "Get count of kills for a player")]
    public void OnKillsCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return;
        var steamId = player.AuthorizedSteamID.SteamId64;

        Task.Run(async () =>
        {
            var result = await _connection.QueryFirstOrDefaultAsync<int>(@"SELECT `kills` FROM `players` WHERE `steamid` = @SteamId;",
                new
                {
                    SteamId = steamId
                });

            Server.NextFrame(() => { player?.PrintToChat($"Kills: {result}"); });
        });
    }

     [ConsoleCommand("css_svip", "Get count of Credits for a player")]
    public void OnStoreCommand(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return;

        if (!HasPermission(player, "Permission")){
            player.PrintToChat(Localizer["Credits.ok"]);
            Server.PrintToChatAll(Localizer["Credits.ok", player.PlayerName]);
            return;
        }

        var steamId = player?.AuthorizedSteamID?.SteamId64;

        Task.Run(async () =>
        {
            var result = await _connection.QueryFirstOrDefaultAsync<int>(@"SELECT `Credits` FROM `store_players` WHERE `SteamID` = @SteamId;",
                new
                {
                    SteamId = steamId
                });

            Server.NextFrame(() => { 
                if (result >= 2000) { resultadoSubtracao = result - 2000; 
                player.PrintToChat("{red}[ANNABEL] {blue} Credits:{green} {result} {white}Add Vip Sucess!");
                } else {
                    player?.PrintToChat(Localizer["Credits.insufficient"]);
                    Server.PrintToChatAll(Localizer["Credits.insufficient", player.PlayerName]);
                    }
                
                });
        });

        if(resultadoSubtracao >= 2000){

            var callerName = player == null ? "Console" : player.PlayerName;
            Server.ExecuteCommand($"css_addadmin {steamId} {callerName} @css/custom-permission 40 40000");

         Task.Run(async () =>
        {
            var result = await _connection.QueryFirstOrDefaultAsync<int>(@"UPDATE `store_players` SET `Credits` = `Credits` - 2000 WHERE `SteamID` = @SteamId;",
                new
                {
                    SteamId = steamId
                });

            Server.NextFrame(() => { 
                player?.PrintToChat(Localizer["Credits.insufficient"]);
                Server.PrintToChatAll(Localizer["Credits.svip", player.PlayerName]);
                if (result >= 2000) { resultadoSubtracao = result - 2000;}
                
                });
        });
    }
    }
    

    
}
