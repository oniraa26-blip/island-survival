# World Map Foundation

The full Origin Island map is a UI presentation, not a second gameplay camera. Tap the HUD minimap to open it; `Escape`, Android Back, or the Close button returns to gameplay. `MobileInput.SetGameplayBlocked` clears movement, look, sprint, jump, and interaction state while the map is open.

`WorldMapDefinition` is the replaceable data/art boundary. The temporary runtime definition preserves the eight existing `OriginDemoZones` IDs. A future authored `Resources/UI/OriginIslandMap.asset` can replace it without changing gameplay code. `TemporaryOriginMapArt` is original generated placeholder art and is explicitly not production artwork.

## Coordinates

World X maps left-to-right. World Z maps bottom-to-top (north-up). The configured Origin Island X/Z bounds are converted to normalized `0..1` coordinates and clamped for UI placement. The same conversion is used for players, teammates, landmarks, Home/Bed, and future marker categories.

## Discovery and markers

Starting Beach is visible initially. Every other region and landmark reads the existing `DiscoverySystem`; no save schema or discovery ID changes are introduced. `MinimapMarkerRegistry` is shared by minimap and full map and supports one local player, three future teammates, Home/Bed, and a lightweight custom pin API. Networking and fast travel are intentionally absent.

Use `Island Survival > UI > Validate World Map` in Play Mode for runtime wiring validation. The map is inactive while closed and refreshes at 5 Hz only while open.
