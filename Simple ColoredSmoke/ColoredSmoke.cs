using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Admin;
using System.Text.Json.Serialization;


namespace ColoredSmoke;

public class ConfigGen : BasePluginConfig
{
    [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("Flag")] public string Flag { get; set; } = ""; 
    [JsonPropertyName("Color")] public string Color { get; set; } = ""; 
}

public partial class ColoreddSmoke : BasePlugin, IPluginConfig<ConfigGen>
{
    public override string ModuleName => "ColoredSmoke";
    public override string ModuleAuthor => "M1k@c";
    public override string ModuleDescription => "ColoredSmoke";
    public override string ModuleVersion => "V. 1.0.1";

    public ConfigGen Config { get; set; } = null!;
    public void OnConfigParsed(ConfigGen config) { Config = config; }

    public int Round;
    public int ConnectedPlayers;


    public override void Load(bool hotReload)
    {
        RegisterListener<Listeners.OnEntitySpawned>(OnEntitySpawned);
        RegisterListener<Listeners.OnTick>(() =>
        {
            for (int i = 1; i < Server.MaxPlayers; i++)
            {            
                var ent = NativeAPI.GetEntityFromIndex(i);
                if (ent == 0)
                    continue;

                var client = new CCSPlayerController(ent);
                if (Config.Flag != "" && !AdminManager.PlayerHasPermissions(client, Config.Flag)) return;
                if (client == null || !client.IsValid)
                    continue;
            }
        });


        if (hotReload)
        {
            RegisterListener<Listeners.OnMapStart>(name =>
            {
                ConnectedPlayers = 0;
                Round = 0;
            });

        }
    }
    

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (entity.DesignerName != "smokegrenade_projectile") return;

        var smokeGrenadeEntity = new CSmokeGrenadeProjectile(entity.Handle);
        if (smokeGrenadeEntity.Handle == IntPtr.Zero) return;

        if (Config.Enabled)
        {
          Server.NextFrame(() =>
          {
            var entityIndex = smokeGrenadeEntity.Thrower.Value.Controller.Value.Index;

            if (entityIndex == null) return;
            if (Config.Color == "red")
            {
              smokeGrenadeEntity.SmokeColor.X = 255.0f;
              smokeGrenadeEntity.SmokeColor.Y = 0.0f;
              smokeGrenadeEntity.SmokeColor.Z = 0.0f;
            }
            if (Config.Color == "blue")
            {
              smokeGrenadeEntity.SmokeColor.X = 0.0f;
              smokeGrenadeEntity.SmokeColor.Y = 0.0f;
              smokeGrenadeEntity.SmokeColor.Z = 255.0f;
            }
            if (Config.Color == "green")
            {
              smokeGrenadeEntity.SmokeColor.X = 0.0f;
              smokeGrenadeEntity.SmokeColor.Y = 255.0f;
              smokeGrenadeEntity.SmokeColor.Z = 255.0f;
            }
            if (Config.Color == "random")
            {
              smokeGrenadeEntity.SmokeColor.X = Random.Shared.NextSingle() * 255.0f;
              smokeGrenadeEntity.SmokeColor.Y = Random.Shared.NextSingle() * 255.0f;
              smokeGrenadeEntity.SmokeColor.Z = Random.Shared.NextSingle() * 255.0f;
            }
          });
        }
    }
}
