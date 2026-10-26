using IslandSurvival.Building;
using IslandSurvival.CameraSystem;
using IslandSurvival.Crafting;
using IslandSurvival.Equipment;
using IslandSurvival.Exploration;
using IslandSurvival.InputSystem;
using IslandSurvival.Interaction;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using IslandSurvival.Localization;
using IslandSurvival.Player;
using IslandSurvival.Platform;
using IslandSurvival.Presentation;
using IslandSurvival.Save;
using IslandSurvival.Survival;
using IslandSurvival.Streaming;
using IslandSurvival.TimeSystem;
using IslandSurvival.UI;
using IslandSurvival.World;
using IslandSurvival.Visual;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandSurvival.Core
{
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        float nextAutosave;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (FindFirstObjectByType<PrototypeBootstrap>() == null) new GameObject("PrototypeBootstrap").AddComponent<PrototypeBootstrap>();
        }

        void Awake()
        {
            ServiceRegistry.Reset();
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
            ServiceRegistry.Tuning = new PrototypeTuning();
            PresentationRuntime.Mode = ServiceRegistry.Tuning.presentationMode;
            ServiceRegistry.Platform = PlatformProfile.Detect();
            PlatformQualityController.Apply(ServiceRegistry.Platform);
            ServiceRegistry.Visuals = VisualAssetResolver.Load(ServiceRegistry.Platform);
            ServiceRegistry.Localization = LocalizationService.LoadDefault();
            ServiceRegistry.Items = new ItemDatabase();
            ServiceRegistry.Inventory = new InventoryModel(ServiceRegistry.Items, 16);
            ServiceRegistry.Equipment = new EquipmentModel(ServiceRegistry.Inventory, ServiceRegistry.Items);
            ServiceRegistry.Discovery = new DiscoverySystem();
            ServiceRegistry.Survival = new SurvivalModel(ServiceRegistry.Tuning);
            BuildRuntime();
        }

        void BuildRuntime()
        {
            var demoWorld = WorldFactory.CreateStartingBeach(ServiceRegistry.Platform);
            var sunObject = new GameObject("Sun"); var sun = sunObject.AddComponent<Light>(); sun.type = LightType.Directional;
            var time = new GameObject("TimeOfDay").AddComponent<TimeOfDayController>(); time.Initialize(sun, ServiceRegistry.Tuning); ServiceRegistry.Time = time;

            var input = new GameObject("MobileInput").AddComponent<MobileInput>();
            var playerObject = new GameObject("Player"); playerObject.tag = "Player"; playerObject.layer = 2; playerObject.transform.position = new Vector3(0f, 1.2f, 0f);
            var controller = playerObject.AddComponent<CharacterController>(); controller.height = 1.8f; controller.radius = 0.38f; controller.center = new Vector3(0f, 0.9f, 0f);
            WorldFactory.CreatePlayerVisualRoot(playerObject.transform);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera"; var camera = cameraObject.AddComponent<UnityEngine.Camera>(); camera.nearClipPlane = 0.15f; camera.farClipPlane = 250f;
            TropicalDaylightProfile.Apply(ServiceRegistry.Visuals.Daylight, ServiceRegistry.Platform, sun, camera);
            PlatformQualityController.Apply(ServiceRegistry.Platform, camera);
            if (PresentationRuntime.IsTwoPointFiveD)
            {
                var cameraFollow = cameraObject.AddComponent<IsometricCameraController>();
                cameraFollow.Initialize(playerObject.transform, ServiceRegistry.Tuning);
            }
            else
            {
                var cameraFollow = cameraObject.AddComponent<ThirdPersonCamera>();
                cameraFollow.Initialize(playerObject.transform, input, ServiceRegistry.Tuning);
            }
            var player = playerObject.AddComponent<PlayerController>(); player.Initialize(input, camera, ServiceRegistry.Tuning, ServiceRegistry.Survival); ServiceRegistry.Player = player;
            demoWorld.BindPlayer(playerObject.transform);
            var interactor = playerObject.AddComponent<PlayerInteractor>(); interactor.Initialize(input, ServiceRegistry.Tuning.interactionRadius);

            var building = new GameObject("BuildSystem").AddComponent<BuildSystem>(); building.Initialize(ServiceRegistry.Inventory, camera); ServiceRegistry.Building = building;
            var zones = new GameObject("ZoneStreaming").AddComponent<ZoneStreamingService>(); zones.Initialize(ServiceRegistry.Platform.maxLoadedZones); ServiceRegistry.Zones = zones;
            var save = new SaveSystem(SaveStoreFactory.Create(Application.persistentDataPath), playerObject.transform.position); ServiceRegistry.Save = save;
            ServiceRegistry.Survival.Died += save.RespawnPlayer;
            var crafting = new CraftingSystem(ServiceRegistry.Inventory, ServiceRegistry.Items, ServiceRegistry.Localization);

            if (FindFirstObjectByType<EventSystem>() == null) { var events = new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<StandaloneInputModule>(); }
            var hud = new GameObject("MobileHUD").AddComponent<PrototypeHUD>(); hud.Initialize(input, interactor, crafting);
            save.Load(); nextAutosave = UnityEngine.Time.unscaledTime + 30f;
        }

        void Update()
        {
            if (UnityEngine.Time.unscaledTime >= nextAutosave) { ServiceRegistry.Save.SaveNow(); nextAutosave = UnityEngine.Time.unscaledTime + 30f; }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9)) ServiceRegistry.Survival.Damage(999f);
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) ServiceRegistry.Save.SaveNow();
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8)) ServiceRegistry.Save.Load();
#endif
        }
        void OnApplicationPause(bool paused) { if (paused && ServiceRegistry.Save != null) ServiceRegistry.Save.SaveNow(); }
        void OnApplicationQuit() { if (ServiceRegistry.Save != null) ServiceRegistry.Save.SaveNow(); }
    }
}
