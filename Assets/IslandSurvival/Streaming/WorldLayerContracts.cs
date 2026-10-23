using System;

namespace IslandSurvival.Streaming
{
    // Durable facts only: inventory, placed objects, time, relationships, discoveries.
    public interface IPersistentWorldState
    {
        int SchemaVersion { get; }
    }

    // Lightweight simulation for zones that have no loaded scene or rendered GameObjects.
    public interface IBackgroundSimulation
    {
        void Advance(double elapsedGameHours);
    }

    // Short-lived presentation for a loaded zone. Never owns authoritative persistent state.
    public interface IActiveWorldZone
    {
        string ZoneId { get; }
        void Bind(IPersistentWorldState state);
        void Release();
    }

    [Serializable]
    public sealed class WorldZoneDefinition
    {
        public string zoneId;
        public string sceneName;
        public bool keepLoaded;
    }
}
