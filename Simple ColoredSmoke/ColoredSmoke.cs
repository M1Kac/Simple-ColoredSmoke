using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;


namespace ColoreddSmoke;

public partial class ColoreddSmoke : BasePlugin
{
    public override string ModuleName => "ColoredSmoke";
    public override string ModuleAuthor => "M1k@c";
    public override string ModuleDescription => "ColoredSmoke";
    public override string ModuleVersion => "V. 1.0.0";

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

        Server.NextFrame(() =>
        {
            var entityIndex = smokeGrenadeEntity.Thrower.Value.Controller.Value.Index;

            if (entityIndex == null) return;
            if (entityIndex == 0) return;
            smokeGrenadeEntity.SmokeColor.X = Random.Shared.NextSingle() * 255.0f;
            smokeGrenadeEntity.SmokeColor.Y = Random.Shared.NextSingle() * 255.0f;
            smokeGrenadeEntity.SmokeColor.Z = Random.Shared.NextSingle() * 255.0f;
        });
    }
}
