using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using System.Globalization;
using System.Text.Json.Serialization;
using static CounterStrikeSharp.API.Core.Listeners;

namespace ColoredSmoke;

public class ConfigGen : BasePluginConfig
{
    [JsonPropertyName("Enabled")] public bool Enabled { get; set; } = true;
    [JsonPropertyName("Flag")] public string Flag { get; set; } = string.Empty;
    [JsonPropertyName("Color")] public string Color { get; set; } = "random";
}

public partial class ColoredSmoke : BasePlugin, IPluginConfig<ConfigGen>
{
    public override string ModuleName => "ColoredSmoke";
    public override string ModuleAuthor => "M1k@c";
    public override string ModuleDescription => "ColoredSmoke";
    public override string ModuleVersion => "V. 1.0.2";

    public ConfigGen Config { get; set; } = new ConfigGen();

    public override void Unload(bool hotReload)
    {
        RemoveListener<OnEntitySpawned>(OnEntitySpawned);
    }

    public void OnConfigParsed(ConfigGen config)
    {
        if (config.Enabled)
        {
            RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        }

        Config = config;
    }

    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (entity.DesignerName != "smokegrenade_projectile")
        {
            return;
        }

        CSmokeGrenadeProjectile grenade = new(entity.Handle);

        if (grenade.Handle == IntPtr.Zero)
        {
            return;
        }

        Server.NextFrame(() =>
        {
            CBasePlayerController? player = grenade.Thrower.Value?.Controller.Value;

            if (player == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(Config.Flag))
            {
                CCSPlayerController controller = new CCSPlayerController(player.Handle);

                if (!AdminManager.PlayerHasPermissions(controller, Config.Flag))
                {
                    return;
                }
            }

            string color = Config.Color;

            if (color == "random")
            {
                grenade.SmokeColor.X = Random.Shared.NextSingle() * 255.0f;
                grenade.SmokeColor.Y = Random.Shared.NextSingle() * 255.0f;
                grenade.SmokeColor.Z = Random.Shared.NextSingle() * 255.0f;
            }
            else if (color == "teamcolor")
            {
                byte team = player.TeamNum;

                switch (team)
                {
                    case 1:
                        {
                            grenade.SmokeColor.X = 255.0f;
                            grenade.SmokeColor.Y = 0.0f;
                            grenade.SmokeColor.Z = 0.0f;
                            break;
                        }
                    case 2:
                        {
                            grenade.SmokeColor.X = 0.0f;
                            grenade.SmokeColor.Y = 0.0f;
                            grenade.SmokeColor.Z = 255.0f;
                            break;
                        }
                    default:
                        {
                            grenade.SmokeColor.X = 255.0f;
                            grenade.SmokeColor.Y = 255.0f;
                            grenade.SmokeColor.Z = 255.0f;
                            break;
                        }
                }
            }
            else
            {
                string[] colors = Config.Color.Split(' ');

                grenade.SmokeColor.X = float.Parse(colors[0], CultureInfo.InvariantCulture);
                grenade.SmokeColor.Y = float.Parse(colors[1], CultureInfo.InvariantCulture);
                grenade.SmokeColor.Z = float.Parse(colors[2], CultureInfo.InvariantCulture);
            }
        });
    }
}
