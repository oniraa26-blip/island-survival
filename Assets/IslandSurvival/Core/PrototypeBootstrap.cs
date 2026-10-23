using System.IO;
using IslandSurvival.Building;
using IslandSurvival.CameraSystem;
using IslandSurvival.Crafting;
using IslandSurvival.Equipment;
using IslandSurvival.InputSystem;
using IslandSurvival.Interaction;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using IslandSurvival.Player;
using IslandSurvival.Save;
using IslandSurvival.Survival;
using IslandSurvival.TimeSystem;
using IslandSurvival.UI;
using IslandSurvival.World;
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
            if (FindObjectOfType<PrototypeBootstrap>() == null) new GameObject("PrototypeBootstrap").AddComponent<PrototypeBootstrap>();
        }

        void Awake()
        {
            ServiceRegistry.Reset();
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            ServiceRegistry.Tuning = new PrototypeTuning();
            ServiceRegistry.Items = new ItemDatabase();
            ServiceRegistry.Inventory = new InventoryModel(ServiceRegistry.Items, 16);
            ServiceRegistry.Equipment = new EquipmentModel(ServiceRegistry.Inventory, ServiceRegistry.Items);
            ServiceRegistry.Survival = new SurvivalModel(ServiceRegistry.Tuning);
            BuildRuntime();
        }

        void BuildRuntime()
        {
            WorldFactory.CreateStartingBeach();
            var sunObject = new GameObject("Sun"); var sun = sunObject.AddComponent<Light>(); sun.type = LightType.Directional; sun.shadows = LightShadows.Soft;
            var time = new GameObject("TimeOfDay").AddComponent<TimeOfDayController>(); time.Initialize(sun, ServiceRegistry.Tuning); ServiceRegistry.Time = time;

            var input = new GameObject("MobileInput").AddComponent<MobileInput>();
            var playerObject = new GameObject("Player"); playerObject.tag = "Player"; playerObject.layer = 2; playerObject.transform.position = new Vector3(0f, 1.2f, 0f);
            var controller = playerObject.AddComponent<CharacterController>(); controller.height = 1.8f; controller.radius = 0.38f; controller.center = new Vector3(0f, 0.9f, 0f);
            var body = WorldFactory.Primitive("PlayerVisual", PrimitiveType.Capsule, playerObject.transform.position, new Vector3(0.72f, 0.9f, 0.72f), new Color(0.95f, 0.5f, 0.18f), playerObject.transform); body.layer = 2; body.transform.localPosition = Vector3.up * 0.9f; Destroy(body.GetComponent<Collider>());
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera"; var camera = cameraObject.AddComponent<UnityEngine.Camera>(); camera.nearClipPlane = 0.15f; camera.farClipPlane = 250f;
            var cameraFollow = cameraObject.AddComponent<ThirdPersonCamera>(); cameraFollow.Initialize(playerObject.transform, input, ServiceRegistry.Tuning);
            var player = playerObject.AddComponent<PlayerController>(); player.Initialize(input, camera, ServiceRegistry.Tuning, ServiceRegistry.Survival); ServiceRegistry.Player = player;
            var interactor = playerObject.AddComponent<PlayerInteractor>(); interactor.Initialize(input, ServiceRegistry.Tuning.interactionRadius);

            var building = new GameObject("BuildSystem").AddComponent<BuildSystem>(); building.Initialize(ServiceRegistry.Inventory, camera); ServiceRegistry.Building = building;
            var save = new SaveSystem(Path.Combine(Application.persistentDataPath, "prototype_0_1_save.json"), playerObject.transform.position); ServiceRegistry.Save = save;
            ServiceRegistry.Survival.Died += save.RespawnPlayer;
            var crafting = new CraftingSystem(ServiceRegistry.Inventory);

            if (FindObjectOfType<EventSystem>() == null) { var events = new GameObject("EventSystem"); events.AddComponent<EventSystem>(); events.AddComponent<StandaloneInputModule>(); }
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
