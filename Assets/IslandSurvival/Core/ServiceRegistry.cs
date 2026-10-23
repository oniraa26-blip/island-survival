using IslandSurvival.Building;
using IslandSurvival.Equipment;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using IslandSurvival.Localization;
using IslandSurvival.Player;
using IslandSurvival.Platform;
using IslandSurvival.Save;
using IslandSurvival.Survival;
using IslandSurvival.Streaming;
using IslandSurvival.TimeSystem;

namespace IslandSurvival.Core
{
    public static class ServiceRegistry
    {
        public static PrototypeTuning Tuning { get; internal set; }
        public static PlatformProfile Platform { get; internal set; }
        public static ItemDatabase Items { get; internal set; }
        public static LocalizationService Localization { get; internal set; }
        public static InventoryModel Inventory { get; internal set; }
        public static EquipmentModel Equipment { get; internal set; }
        public static SurvivalModel Survival { get; internal set; }
        public static PlayerController Player { get; internal set; }
        public static BuildSystem Building { get; internal set; }
        public static TimeOfDayController Time { get; internal set; }
        public static SaveSystem Save { get; internal set; }
        public static ZoneStreamingService Zones { get; internal set; }

        public static void Reset()
        {
            Tuning = null; Platform = null; Items = null; Localization = null; Inventory = null; Equipment = null;
            Survival = null; Player = null; Building = null; Time = null; Save = null; Zones = null;
        }
    }
}
