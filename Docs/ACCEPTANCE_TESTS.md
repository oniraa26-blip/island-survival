# Prototype 0.1 Acceptance Test Matrix

Unity editor assumption: **Unity 2022.3.62f1 LTS**, URP 14.0.11. Automated tests are under `Assets/IslandSurvival/Tests/EditMode`.

Cross-platform build and quality policy is documented separately in `PLATFORM_STRATEGY.md`. These gameplay acceptance tests remain unchanged.

| # | Acceptance test | Status before Unity device/editor run | Evidence / manual check |
|---|---|---|---|
| 1 | Fresh launch reaches Starting Beach without blocking errors | NEEDS UNITY EDITOR TEST | Open `Prototype_StartingBeach`, enter Play Mode, check Console. Runtime bootstrap creates the slice. |
| 2 | Touch movement and camera work simultaneously | NEEDS DEVICE TEST | Hold left dynamic joystick while dragging right-side empty space. |
| 3 | Collect Stick/Stone/Coconut | NEEDS UNITY EDITOR TEST | Approach each pickup; context button must appear and add it. |
| 4 | Inventory stacks and safely refuses overflow | NEEDS UNITY TEST | Automated coverage is authored in `InventoryTests`; also fill all 16 slots in Play Mode. |
| 5 | Craft and equip Stone Axe | NEEDS UNITY TEST | Automated model coverage is authored; gather 2 Stone + Stick + Fiber, BAG → Craft Axe → Equip Axe. |
| 6 | Tree cannot be chopped without equipped axe | PASS (code path); NEEDS UI TEST | Tree `CanInteract` requires `mainHand == stone_axe`; verify no Chop button before equip. |
| 7 | Equipped axe harvests Wood | NEEDS UNITY EDITOR TEST | Equip axe, chop a test palm three times; Wood x4 is awarded. |
| 8 | Hunger/Thirst/Stamina visibly change | NEEDS UNITY TEST | Automated model coverage is authored; sprint and watch upper-left compact HUD. |
| 9 | Coconut affects survival state | PASS (code path); NEEDS UI TEST | BAG → Eat Coconut; Hunger +10, Thirst +15. |
| 10 | Campfire only places on valid ground | NEEDS UNITY EDITOR TEST | BAG → Place Fire; invalid preview is red and cannot confirm. |
| 11 | Leaf Bed places and sets respawn | NEEDS UNITY EDITOR TEST | Place Bed, interact; time advances to morning and save occurs. |
| 12 | Death returns to bed/default spawn | PASS (code path); NEEDS UNITY EDITOR TEST | Development shortcut F9 or deplete stats; test before/after setting bed. |
| 13 | Day/night visibly transitions | NEEDS UNITY EDITOR TEST | Observe sun, ambient light, fog, and phase label for a full accelerated cycle. |
| 14 | Save/load restores required state | PASS (schema/code path); NEEDS UNITY EDITOR TEST | F5 save, change state, F8 load; restart Play Mode. |
| 15 | HUD usable at common landscape ratios | NEEDS DEVICE/GAME VIEW TEST | Verify 16:9, 19.5:9, 4:3 landscape; safe area and 44+ px controls. |
| 16 | No essential workflow requires keyboard/mouse | NEEDS DEVICE TEST | Complete entire loop using touch controls only. |
| 17 | Android build has no editor-only runtime dependency | PASS (static separation); NEEDS ANDROID BUILD | Run the documented Android batch build. |

`PASS` is used only for checks completed in the current cloud machine. Unity is not installed in that machine, so authored Unity tests are explicitly unclaimed until they execute in the Editor or batch runner. Rendering, touch hardware, Play Mode, and Android checks likewise remain unclaimed.

## Prototype assumptions

- Fiber is represented by small green ground pickups; this keeps recipes testable without adding a plant-harvesting system.
- Stone Knife costs 1 Stone + 1 Stick + 1 Fiber. Stone Axe costs 2 Stone + 1 Stick + 1 Fiber.
- Campfire costs 2 Stone + 2 Wood. Leaf Bed costs 3 Wood + 3 Fiber.
- A tree takes three interactions and grants 4 Wood. Regrowth is intentionally only a future hook.
- Bed interaction combines setting respawn and sleeping for the vertical slice.
- Runtime primitives are intentional placeholder art and avoid unreviewable binary assets.
