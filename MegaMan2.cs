using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrowdControl.Common;
using JetBrains.Annotations;

namespace CrowdControl.Games.Packs
{
    [UsedImplicitly]
    public class MegaMan2 : NESEffectPack
    {
        [NotNull]
        private readonly IPlayer _player;

        public MegaMan2([NotNull] IPlayer player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(responseHandler, statusUpdateHandler) => _player = player;

        private volatile bool _quitting = false;
        public override void Dispose()
        {
            _quitting = true;
            base.Dispose();
        }

        private const ushort ADDR_AREA = 0x002A;
        private const ushort ADDR_PHYSICS = 0x003D;
        private const ushort ADDR_IFRAMES = 0x004B;
        private const ushort ADDR_WEAPONS = 0x009A;
        private const ushort ADDR_ITEMS = 0x009B;
        private const ushort ADDR_ENERGY_HEAT = 0x009C;
        private const ushort ADDR_ENERGY_AIR = 0x009D;
        private const ushort ADDR_ENERGY_WOOD = 0x009E;
        private const ushort ADDR_ENERGY_BUBBLE = 0x009F;
        private const ushort ADDR_ENERGY_QUICK = 0x00A0;
        private const ushort ADDR_ENERGY_FLASH = 0x00A1;
        private const ushort ADDR_ENERGY_METAL = 0x00A2;
        private const ushort ADDR_ENERGY_CRASH = 0x00A3;
        private const ushort ADDR_ENERGY_ITEM1 = 0x00A4;
        private const ushort ADDR_ENERGY_ITEM2 = 0x00A5;
        private const ushort ADDR_ENERGY_ITEM3 = 0x00A6;
        private const ushort ADDR_ETANKS = 0x00A7;
        private const ushort ADDR_LIVES = 0x00A8;
        private const ushort ADDR_POWER = 0x00A9;
        private const ushort ADDR_HP = 0x06C0;
        private const ushort ADDR_BOSS_HP = 0x06C1;
        private const ushort ADDR_ENEMY_HP = 0xD78E;
        private const ushort ADDR_SFX = 0x0580;
        private const ushort ADDR_SFX_ENABLE = 0x0066;
        private const ushort ADDR_FLIP_PLAYER = 0x8904;
        private const ushort ADDR_JUMP_HEIGHT = 0x003C;
        private const ushort ADDR_PALETTE1 = 0x0356;
        private const ushort ADDR_PALETTE2 = 0x0357;
        private const ushort ADDR_PALETTE3 = 0x0358;
        private const ushort ADDR_HERO_COLOR_OUTLINE = 0x0367;
        private const ushort ADDR_HERO_COLOR_LIGHT = 0x0368;
        private const ushort ADDR_HERO_COLOR_DARK = 0x0369;

        private const ushort ADDR_UNKNOWN1 = 0x0069;//should be 0x0E

        private Dictionary<string, (string weapon, string bossName, byte value, BossDefeated bossFlag, SuitColor light, SuitColor dark, ushort address, byte limit)> _wType = new Dictionary<string, (string, string, byte, BossDefeated, SuitColor, SuitColor, ushort, byte)>(StringComparer.InvariantCultureIgnoreCase)
        {
            {"buster", ("Mega Buster", "Mega Man", 0, 0, SuitColor.DefaultLight, SuitColor.DefaultDark, 0, 0)},
            {"fire", ("Atomic Fire", "Heat Man", 1, BossDefeated.HeatMan, SuitColor.HLight, SuitColor.HDark, ADDR_ENERGY_HEAT, 14)},
            {"air", ("Air Shooter", "Air Man", 2, BossDefeated.AirMan, SuitColor.ALight, SuitColor.ADark, ADDR_ENERGY_AIR, 14)},
            {"leaf", ("Leaf Shield", "Wood Man,", 3, BossDefeated.WoodMan, SuitColor.WLight, SuitColor.WDark, ADDR_ENERGY_WOOD, 14)},
            {"bubble", ("Bubble Lead", "Bubble Man", 4, BossDefeated.BubbleMan, SuitColor.BLight, SuitColor.BDark, ADDR_ENERGY_BUBBLE, 14)},
            {"quick", ("Quick Boomerang", "Quick Man", 5, BossDefeated.QuickMan, SuitColor.QLight, SuitColor.QDark, ADDR_ENERGY_QUICK, 14)},
            {"time", ("Time Stopper", "Flash Man", 6, BossDefeated.FlashMan, SuitColor.FLight, SuitColor.FDark, ADDR_ENERGY_FLASH, 14)},
            {"metal", ("Metal Blade", "Metal Man", 7, BossDefeated.MetalMan, SuitColor.MLight, SuitColor.MDark, ADDR_ENERGY_METAL, 14)},
            {"crash", ("Crash Bomber", "Crash Man", 8, BossDefeated.CrashMan, SuitColor.CLight, SuitColor.CDark, ADDR_ENERGY_CRASH, 14)},
            {"item1", ("Item 1", "Heat Man", 9,BossDefeated.HeatMan, SuitColor.ItemLight, SuitColor.ItemDark, ADDR_ENERGY_ITEM1, 14)},
            {"item2", ("Item 2", "Air Man", 10,BossDefeated.AirMan, SuitColor.ItemLight, SuitColor.ItemDark, ADDR_ENERGY_ITEM2, 14)},
            {"item3", ("Item 3", "Flash Man", 11,BossDefeated.FlashMan, SuitColor.ItemLight, SuitColor.ItemDark, ADDR_ENERGY_ITEM3, 14)}
        };

        private Dictionary<byte, string> _aInfo = new Dictionary<byte, string>()
            {
                {0x00, "Heat Man"},
                {0x01, "Air Man"},
                {0x02, "Wood Man"},
                {0x03, "Bubble Man"},
                {0x04, "Quick Man"},
                {0x05, "Flash Man"},
                {0x06, "Metal Man"},
                {0x07, "Crash Man"},
                {0x08, "the boss"},
                {0x09, "the boss"},
                {0x0A, "the boss"}
            };

        private enum SuitColor : byte
        {
            Black = 0x0F,
            DefaultLight = 0x2C,
            DefaultDark = 0x11,
            HLight = 0x28,
            HDark = 0x15,
            ALight = 0x30,
            ADark = 0x11,
            WLight = 0x30,
            WDark = 0x19,
            BLight = 0x30,
            BDark = 0x00,
            QLight = 0x34,
            QDark = 0x25,
            FLight = 0x34,
            FDark = 0x14,
            MLight = 0x37,
            MDark = 0x18,
            CLight = 0x30,
            CDark = 0x26,
            ItemLight = 0x30,
            ItemDark = 0x16,
        }

        [Flags]
        private enum BossDefeated : byte
        {
            HeatMan = 0x01,
            AirMan = 0x02,
            WoodMan = 0x04,
            BubbleMan = 0x08,
            QuickMan = 0x10,
            FlashMan = 0x20,
            MetalMan = 0x40,
            CrashMan = 0x80,
            All = 0xFF
        }

        private enum ItemType : byte
        {
            Item1 = 0x01,
            Item2 = 0x02,
            Item3 = 0x04
        }

        public override List<Effect> Effects
        {
            get
            {
                List<Effect> effects = new List<Effect>
                {
                    new Effect("Give Lives", "lives", new[] {"quantity9"}),
                    new Effect("Give E-Tanks", "etank"),
                    new Effect("Boss E-Tank", "bosshpfull"),
                    new Effect("Refill Health", "hpfull"),
                    new Effect("Weapon Lock (45 seconds)", "lockweapon", ItemKind.Folder),
                    new Effect("Refill Weapon Energy", "refillweapon", ItemKind.Folder),
                    new Effect("Rebuild Robot Master", "reviveboss", ItemKind.Folder),
                    //new Effect("Black Armor Mega Man", "barmor"),
                    new Effect("Grant Invulnerability (15 seconds)", "iframes"),
                    new Effect("Moonwalk (45 seconds)", "moonwalk"),
                    new Effect("Magnet Floors (45 seconds)", "magfloors"),
                    new Effect("One-Hit KO (15 seconds)", "ohko"),
                    //new Effect("Kill Player", "kill")
                };

                effects.AddRange(_wType.Select(t => new Effect($"Force Weapon to {t.Value.weapon} (15 seconds)", $"lock_{t.Key}", "lockweapon")));
                effects.AddRange(_wType.Skip(1).Select(t => new Effect($"Refill {t.Value.weapon}", $"refill_{t.Key}", "refillweapon")));
                effects.AddRange(_wType.Skip(1).Take(8).Select(t => new Effect($"Rebuild {t.Value.bossName}", $"revive_{t.Key}", "reviveboss")));

                return effects;
            }
        }

        public override List<Common.ItemType> ItemTypes => new List<Common.ItemType>(new[]
        {
            new Common.ItemType("Quantity", "quantity9", Common.ItemType.Subtype.Slider, "{\"min\":1,\"max\":9}")
        });

        public override List<ROMInfo> ROMTable => new List<ROMInfo>(new[]
        {
            new ROMInfo("Mega Man 2", null, Patching.Ignore, ROMStatus.ValidPatched,s => Patching.MD5(s, "caaeb9ee3b52839de261fd16f93103e6")),
            new ROMInfo("Rockman 2 - Dr. Wily no Nazo", null, Patching.Ignore, ROMStatus.ValidPatched,s => Patching.MD5(s, "055fb8dc626fb1fbadc0a193010a3e3f")),
            new ROMInfo("Mega Man 2 Randomizer", null, Patching.Ignore, ROMStatus.ValidPatched, s=>s.Length==262160)
        });

        private enum SFXType : byte
        {
            TimeStop = 0x21,
            Explosion = 0x22,
            MetalBlade = 0x23,
            BusterShot = 0x24,
            EnemyShot = 0x25,
            Damage = 0x26,
            QuickmanBeam = 0x27,
            HPIncrement = 0x28,
            JumpLanding = 0x29,
            DamageEnemy = 0x2B,
            CrashBomb = 0x2E,
            Teleport = 0x30,
            LeafShield = 0x31,
            Menu = 0x32,
            Doors = 0x34,
            Heat1 = 0x35,
            Heat2 = 0x36,
            Heat3 = 0x37,
            AtomicFire = 0x38,
            AirShooter = 0x3F,
            Death = 0x41,
            Item = 0x42,
            Doors2 = 0xFE
        }

        public override List<(string, Action)> MenuActions => new List<(string, Action)>();

        public override Game Game { get; } = new Game(11, "Mega Man 2", "MegaMan2", "NES", ConnectorType.NESConnector);

        protected override bool IsReady(EffectRequest request) => Connector.Read8(0x00b1, out byte b) && (b < 0x80);

        protected override void RequestData(DataRequest request) => Respond(request, request.Key, null, false, $"Variable name \"{request.Key}\" not known");

        protected override void StartEffect(EffectRequest request)
        {
            if (!IsReady(request))
            {
                DelayEffect(request, TimeSpan.FromSeconds(5));
                return;
            }

            string[] codeParams = request.FinalCode.Split('_');
            switch (codeParams[0])
            {
                case "ohko":
                {
                    byte origHP = 0;
                    var s = RepeatAction(request, TimeSpan.FromSeconds(15),
                        () => Connector.Read8(ADDR_HP, out origHP) && (origHP > 1),
                        () => Connector.SendMessage($"{request.DisplayViewer} disabled your structural shielding."),
                        TimeSpan.FromSeconds(1),
                        () => Connector.IsNonZero8(ADDR_HP), TimeSpan.FromSeconds(1),
                        () => Connector.Write8(ADDR_HP, 0x00), TimeSpan.FromSeconds(1), true, "health");
                    s.WhenCompleted.Then(t =>
                    {
                        Connector.Write8(ADDR_HP, origHP);
                        Connector.SendMessage("Your shielding has been restored.");
                    });
                    return;
                }
                case "kill":
                    TryEffect(request,
                        () => Connector.IsNonZero8(ADDR_HP),
                        ()=>Connector.Write8(ADDR_HP, 0));
                    return;
                case "lock":
                    {
                        var wType = _wType[codeParams[1]];
                        ForceWeapon(request, wType.value, (byte)wType.bossFlag, wType.light, wType.dark, wType.weapon, wType.address);
                        return;
                    }
                case "lives":
                    {
                        if (!byte.TryParse(codeParams[1], out byte lives))
                        {
                            Respond(request, EffectStatus.FailTemporary, "Invalid life quantity.");
                            return;
                        }
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_LIVES, lives, 0, 9, false),
                            () => true,
                            () =>
                            {
                                Connector.SendMessage($"{request.DisplayViewer} sent you {lives} lives.");
                                PlaySFX(SFXType.Item);
                            });
                        return;
                    }
                case "etank":
                    TryEffect(request,
                    () => Connector.RangeAdd8(ADDR_ETANKS, 1, 0, 4, false),
                    () => true,
                    () =>
                    {
                        Connector.SendMessage($"{request.DisplayViewer} sent you an E-Tank.");
                        PlaySFX(SFXType.Item);
                    });
                    return;
                case "hpfull":
                    TryEffect(request,
                        () => Connector.Read8(ADDR_HP, out byte b) && (b < 14),
                        () => Connector.Write8(ADDR_HP, 28),
                        () =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer} refilled your health.");
                            PlaySFX(SFXType.HPIncrement);
                        }, TimeSpan.FromSeconds(1));
                    return;
                case "bosshpfull":
                    {
                        if (!Connector.Read8(ADDR_AREA, out byte area))
                        {
                            DelayEffect(request);
                            return;
                        }
                        if ((area == 0x09) || (area == 0x0B))
                        {
                            DelayEffect(request, TimeSpan.FromSeconds(30));
                            return;
                        }
                        TryEffect(request,
                            () => Connector.Read8(ADDR_BOSS_HP, out byte hp) && (hp != 0) && (hp <= 14),
                            () => Connector.Write8(ADDR_BOSS_HP, 28),
                            () =>
                            {
                                Connector.SendMessage($"{request.DisplayViewer} refilled {TryGetBossName()}'s health.");
                                PlaySFX(SFXType.HPIncrement);
                            }, TimeSpan.FromSeconds(1));
                        return;
                    }
                case "refill":
                    {
                        var wType = _wType[codeParams[1]];
                        FillWeapon(request, wType.address, wType.weapon, wType.limit);
                        return;
                    }
                case "revive":
                    {
                        var wType = _wType[codeParams[1]];
                        if (!Connector.Read8(ADDR_AREA, out byte b))
                        {
                            DelayEffect(request);
                            return;
                        }
                        if (b < 8) { ReviveBoss(request, ADDR_WEAPONS, (byte)wType.bossFlag, wType.bossName); }
                        else { DisableWeapon(request, ADDR_WEAPONS, wType.bossFlag, wType.weapon); }
                        return;
                    }
                case "barmor":
                    TryEffect(request,
                        () => Connector.Write8(ADDR_HERO_COLOR_LIGHT, (byte)SuitColor.BDark),
                        () => Connector.Write8(ADDR_HERO_COLOR_DARK, (byte)SuitColor.Black),
                        () => Connector.SendMessage($"{request.DisplayViewer}: equipped Black Armor."));
                    PlaySFX(SFXType.HPIncrement);
                    return;
                case "iframes":
                    RepeatAction(request, TimeSpan.FromSeconds(15),
                        () => Connector.IsZero8(ADDR_IFRAMES),
                        () => Connector.Write8(ADDR_IFRAMES, 0xFF) && Connector.SendMessage($"{request.DisplayViewer} deployed an invulnerability field."), TimeSpan.FromSeconds(0.5),
                        () => true, TimeSpan.FromSeconds(5),
                        () => Connector.Write8(ADDR_IFRAMES, 0xFF), TimeSpan.FromSeconds(0.5), true)
                        .WhenCompleted.ContinueWith(t => Connector.SendMessage($"{request.DisplayViewer}'s invulnerability field has dispersed."));
                    return;
                case "moonwalk":
                    StartTimed(request,
                        () => Connector.Read8(0x8904, out byte b) && (b != 0x49),
                        () =>
                        {
                            bool result = Connector.Write8(0x8904, 0x49);
                            if (result) { Connector.SendMessage($"{request.DisplayViewer} inverted your left/right."); }
                            return result;
                        },
                        TimeSpan.FromSeconds(45));
                    return;
                case "magfloors":
                    StartTimed(request,
                        () => Connector.Read8(0xd3c8, out byte b) && (b != 0x03),
                        () =>
                        {
                            bool result = Connector.Write8(0xd3c8, 0x03);
                            if (result) { Connector.SendMessage($"{request.DisplayViewer} has magnetized the floors."); }
                            return result;
                        },
                        TimeSpan.FromSeconds(45));
                    return;
            }
        }

        private volatile bool _forceActive = false;
        private void ForceWeapon(EffectRequest request, byte wType, byte bossClear, SuitColor lightColor, SuitColor darkColor, string weaponName, ushort weaponAddress)
        {
            bool hadBefore = false;
            RepeatAction(request, TimeSpan.FromSeconds(45),
                () => {
                    if (_forceActive) { return false; }
                    if (!(Connector.Read8(ADDR_UNKNOWN1, out byte b) && (b == 0x0E))) { return false; }
                    bool result = Connector.Read8(ADDR_WEAPONS, out byte w);
                    hadBefore = ((w & bossClear) == bossClear);
                    return result;
                },
                () =>
                {
                    _forceActive = true;
                    Connector.SendMessage($"{request.DisplayViewer} set your weapon to {weaponName}.");
                    PlaySFX(SFXType.BusterShot);
                    return true;
                }, TimeSpan.FromSeconds(5),
                () => (Connector.Read8(ADDR_UNKNOWN1, out byte b) && (b == 0x0E)), TimeSpan.FromSeconds(5),
                () => Connector.Write8(ADDR_POWER, wType) &&
                      Connector.Write8(weaponAddress, 28) &&
                      Connector.Write8(ADDR_HERO_COLOR_LIGHT, (byte)lightColor) &&
                      Connector.Write8(ADDR_HERO_COLOR_DARK, (byte)darkColor),
                TimeSpan.FromSeconds(1), true).WhenCompleted.Then(async t =>
                {
                    while (!(Connector.Read8(ADDR_UNKNOWN1, out byte b) && (b == 0x0E)))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        if (_quitting) { return; }
                    }
                    Connector.Write8(ADDR_HERO_COLOR_LIGHT, (byte)SuitColor.DefaultLight);
                    Connector.Write8(ADDR_HERO_COLOR_DARK, (byte)SuitColor.DefaultDark);
                    Connector.Write8(ADDR_POWER, 0x00);
                    if (!hadBefore) { Connector.UnsetBits(ADDR_WEAPONS, bossClear, out _); }
                    _forceActive = false;
                });
        }

        private void FillWeapon(EffectRequest request, ushort address, string weaponName, byte limit)
            => TryEffect(request,
                () => Connector.Read8(address, out byte b) && (b < limit),
                () => Connector.Write8(address, 28),
                () =>
                {
                    Connector.SendMessage($"{request.DisplayViewer} filled your {weaponName} power.");
                    PlaySFX(SFXType.HPIncrement);
                });

        private void ReviveBoss(EffectRequest request, ushort address, byte bossDefeated, string bossName)
        {
            byte b = 0;
            TryEffect(request,
                () => Connector.Read8(address, out b) && ((b & bossDefeated) == bossDefeated),
                () => Connector.UnsetBits(address, bossDefeated, out _),
                () =>
                {
                    Log.Message($"{b} {bossDefeated}");
                    Connector.SendMessage($"{request.DisplayViewer} rebuilt {bossName}.");
                    PlaySFX(SFXType.HPIncrement);
                });
        }

        private void DisableWeapon(EffectRequest request, ushort address, BossDefeated bossDefeated, string weaponName)
            => StartTimed(request,
                () => Connector.Read8(address, out byte b) && ((b & (byte)bossDefeated) == (byte)bossDefeated),
                () =>
                {
                    bool result = Connector.UnsetBits(address, (byte)bossDefeated, out _);
                    if (result)
                    {
                        Connector.SendMessage($"{request.DisplayViewer} disabled your {weaponName}.");
                        PlaySFX(SFXType.HPIncrement);
                    }
                    return result;
                }, TimeSpan.FromSeconds(15));

        private string TryGetBossName()
        {
            try { return Connector.Read8(ADDR_AREA, out byte b) ? _aInfo[b] : "the boss"; }
            catch { return "the boss"; }
        }

        protected override bool StopEffect(EffectRequest request)
        {
            switch (request.InventoryItem.BaseItem.Code)
            {
                case "moonwalk":
                    {
                        Connector.Write8(0x8904, 0x29);
                        Connector.SendMessage($"{request.DisplayViewer}'s control inversion has ended.");
                        return true;
                    }
                case "magfloors":
                    {
                        Connector.Write8(0xd3c8, 0x00);
                        Connector.SendMessage($"{request.DisplayViewer}'s magnetic field has ended.");
                        return true;
                    }
                case "revive":
                    {
                        string[] codeParams = request.FinalCode.Split('_');
                        var wType = _wType[codeParams[1]];
                        Connector.SetBits(ADDR_WEAPONS, (byte)wType.bossFlag, out _);
                        Connector.SendMessage($"{wType.weapon} is back online.");
                        return true;
                    }
                default:
                    return false;
            }
        }

        public override bool StopAllEffects() => base.StopAllEffects() && Connector.Write8(0x8904, 0x29) && Connector.Write8(0xd3c8, 0x00);

        private void PlaySFX(SFXType type)
        {
            Connector.Write8(ADDR_SFX, (byte)type);
            Connector.Write8(ADDR_SFX_ENABLE, 1);
        }
    }
}
