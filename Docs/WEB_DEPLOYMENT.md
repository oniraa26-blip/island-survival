# Web/PWA Deployment — Starting Beach

## Current milestone status: B. BLOCKED — Unity Editor validation required

The rollback branch is pinned to **Unity 2022.3.62f1 LTS** with URP 14.0.11. The migration branch targets the owner-confirmed Unity **6000.3.27f1 / URP 17.3.0** candidate, but it has not yet compiled or built successfully. A version edit by itself is not a successful migration.

Do not report Safari/iPhone PASS from Unity 2022.3 or from the unvalidated migration branch. The next controlled step is to open `migration/unity-6.3-lts` with the latest patched Unity **6.3 LTS (`6000.3.x`)**, resolve packages and pass compile, Play Mode and regression checks without changing the 2022.3 baseline.

The Codex cloud machine has no Unity Editor, Web Build Support module or browser/device lab. It therefore cannot generate or run the real `Builds/WebPWA` output. Everything below describes the exact owner/build-machine workflow after the migration branch compiles.

### Static audit completed in cloud

- All **39 C# files** parse without C# syntax errors using a Tree-sitter C# parser.
- All assembly-definition and JSON files parse successfully.
- Runtime sources contain no `UnityEditor`, `PlayerSettings`, `BuildTarget`, native Android bridge or direct browser JavaScript calls.
- `git diff --check`, balanced-brace checks and JavaScript syntax checks pass.
- Build Settings statically contains only `Prototype_StartingBeach`.
- No island/NPC/farming/fishing/multiplayer scope was added.

This analysis can find syntax and layering defects, but it cannot resolve Unity assemblies or prove that every Unity 2022/Unity 6 API symbol compiles. Only the actual target Unity Editor can perform that compilation.

## What is already prepared

- Build Settings contains exactly one enabled scene: `Prototype_StartingBeach`.
- Runtime bootstrap creates only the Starting Beach slice and its required services.
- `PlatformProfile.WebMobile()` reduces render scale, shadows, draw distance, LOD bias, vegetation, texture mip level, active-object budget and loaded-zone budget.
- The Web save backend uses `PlayerPrefsSaveStore`; Unity persists Web PlayerPrefs through browser storage/IndexedDB rather than a native filesystem path.
- Touch movement, sprint, right-side camera drag, contextual interaction, inventory/crafting/building and safe-area updates share the gameplay code.
- `Assets/WebGLTemplates/IslandSurvivalPWA` includes the responsive shell, manifest, prototype SVG icon, service worker and Netlify/Cloudflare `_headers` rules.
- The service worker is network-first, so a new online deployment wins over cached content while the last successful response remains an offline fallback.

## 1. Prepare the build machine

For the accepted Safari milestone, use the separately validated Unity 6.3 migration branch. Do not open the only 2022.3 checkout in Unity 6 because project serialization is one-way.

1. Install Unity Hub.
2. Install the selected latest patched **Unity 6.3 LTS (`6000.3.x`)** editor.
3. In **Add modules**, enable **Web Build Support**.
4. Clone the repository or fetch the migration branch into a clean directory.
5. Confirm the branch contains no uncommitted work before opening it.
6. Open the project from Unity Hub.
7. Wait for Package Manager, URP import and script compilation to finish.
8. If Unity offers Safe Mode, enter it and resolve compile errors before continuing.

Unity Build Automation is also valid: configure a Web target using the same migration branch and selected Unity 6.3 patch, then download the complete build artifact. It does not remove the need for physical Safari testing.

## 2. Validate before building

Inside Unity:

1. Open `Assets/Scenes/Prototype_StartingBeach.unity`.
2. Select **Island Survival → Validate Prototype 0.1**.
3. Open **Window → General → Test Runner**.
4. Select **EditMode**, then **Run All**.
5. Enter Play Mode and confirm:
   - no blocking red Console errors;
   - Starting Beach appears;
   - desktop fallback can move, look, interact, craft and enter placement mode;
   - save/load works in the Editor.
6. Exit Play Mode before building.

Do not continue if compilation, automated tests, validation or the basic Play Mode loop fails.

## 3. Create the Web/PWA build

The simple route:

1. Select **Island Survival → Build → Web PWA Mobile Test**.
2. Wait for Unity compilation, shader processing and Web build completion.
3. The output directory is:

   `Builds/WebPWA`

The equivalent batch command is:

```bash
/absolute/path/to/Unity \
  -batchmode -nographics -quit \
  -projectPath /absolute/path/to/island-survival \
  -executeMethod IslandSurvival.Editor.PlatformBuildProfiles.BuildWebPWA \
  -logFile /absolute/path/to/web-build.log
```

Check the Unity exit code and the end of `web-build.log`. A folder existing by itself is not proof that the build succeeded.

After a successful build, `Builds/WebPWA` should contain at least:

- `index.html`
- `manifest.json`
- `service-worker.js`
- `icon.svg`
- `_headers`
- `Build/` containing Unity loader, framework, WebAssembly and data files

Never open `index.html` through `file://`. Unity Web content and service workers require an HTTP server; PWA installation requires HTTPS except on localhost.

## 4. Test locally before deployment

From the repository root, serve the output with any static HTTP server. For example, if Node.js is installed:

```bash
npx serve Builds/WebPWA
```

Open the localhost URL printed by the tool. This desktop check can catch missing files and loader failures, but it is not an iPhone/Safari PASS.

## 5. Deploy with Netlify Drop (easiest non-technical route)

1. Visit `https://app.netlify.com/drop` in a desktop browser.
2. Sign in or create a Netlify account.
3. In Finder/File Explorer, open `Builds`.
4. Drag the **WebPWA folder itself** onto the Netlify deployment area. Do not drag only `index.html`.
5. Wait until deployment completes.
6. Netlify displays a public address similar to:

   `https://random-name-123.netlify.app`

7. Open that URL on desktop once and check the browser console/network panel for missing `.wasm`, `.js`, `.data` or compression errors.
8. In Netlify site settings, optionally choose a readable site name. The resulting `*.netlify.app` address remains HTTPS.

The included `_headers` file supplies Brotli MIME/encoding rules recognized by Netlify and Cloudflare Pages. Unity also enables decompression fallback for test-host compatibility.

### Cloudflare Pages alternative

1. Sign in at `https://dash.cloudflare.com`.
2. Open **Workers & Pages → Create → Pages → Direct Upload**.
3. Upload the complete `Builds/WebPWA` directory.
4. Deploy and copy the resulting `https://….pages.dev` URL.

Do not use a raw GitHub file URL, Dropbox preview URL or another document-preview service. The host must serve the directory as a website over HTTPS.

## 6. Test on Safari for iPhone

Use a physical iPhone supported by the selected Unity version.

1. Copy or message the public HTTPS URL to the iPhone.
2. Open it specifically in **Safari**.
3. Wait for the Unity loading screen to complete; keep the tab foregrounded.
4. Rotate the phone to landscape. Safari may refuse programmatic orientation lock, so the page intentionally shows a rotate overlay in portrait.
5. Test both landscape directions and verify that no controls sit behind the notch or Dynamic Island.
6. Complete this touch-only smoke flow:
   - move with the left dynamic joystick while dragging the camera on the right;
   - hold `RUN` while moving;
   - pick up Stick, Stone, Coconut and Fiber;
   - open `BAG`, craft and equip the Stone Axe;
   - chop a tree and collect Wood;
   - place the Campfire and Leaf Bed;
   - interact with the bed and set respawn;
   - consume a Coconut and observe stats;
   - wait for/save state, close the tab, reopen the same URL and verify restoration.
7. Watch for tab reloads, black canvas, excessive heat, lost touches, audio/context errors, memory termination or saves disappearing. Record the iPhone model, iOS version and Safari behavior.

Only after this succeeds may Safari/iPhone acceptance items be marked PASS.

## 7. Add to Home Screen

Safari controls installation; the page cannot force it.

1. Open the public URL in Safari.
2. Tap the **Share** button.
3. Scroll and choose **Add to Home Screen**.
4. Confirm the name and tap **Add**.
5. Launch Island Survival from its new Home Screen icon.
6. Repeat the touch and save/reload smoke tests in this standalone mode.

The repository contains a scalable prototype icon. iOS may use a generated site snapshot instead of SVG for the Home Screen icon. Before external distribution, provide reviewed PNG icons—at minimum an Apple touch icon around 180×180 and manifest icons at 192×192 and 512×512—and link them from the template. This cosmetic limitation does not justify fabricating unreviewed binary art.

## 8. Update the deployed build later

1. Commit the new code and rerun validation/tests.
2. Create a fresh `Builds/WebPWA` build. Do not mix files from two Unity builds.
3. In Netlify, open the site and choose **Deploys → Add new deploy / Deploy manually**.
4. Upload the complete new `Builds/WebPWA` folder.
5. Keep the same site to preserve the same public origin and therefore the same browser-storage namespace.
6. Open the URL in Safari while online. The network-first service worker fetches and caches current files.
7. Close and relaunch the installed Home Screen app.
8. If an old shell remains during development, remove the Home Screen app, clear that site's Safari Website Data, and reinstall. Treat this as troubleshooting, not the normal update path.

Changing hostname/domain can make the previous IndexedDB/PlayerPrefs save inaccessible because browser storage is origin-scoped. Production save-schema migration and cloud save remain outside Prototype 0.1.

## Hosting and browser limitations

- HTTPS is mandatory for service workers and install-like PWA behavior.
- Safari can suspend or discard background tabs; no background gameplay simulation is promised.
- Private Browsing, storage pressure, site-data clearing and hostname changes can remove or isolate prototype saves.
- A successful desktop Web build does not prove mobile Safari memory/performance.
- The prototype intentionally ships only Starting Beach. It must not add future archipelago scenes to Build Settings for this milestone.
- The SVG icon is sufficient metadata for a prototype shell but not final iOS branding.

## NEXT ACTION FOR OWNER

Do **not** treat a Unity 2022.3 WebGL export as the accepted iPhone build. Create or authorize a dedicated `migration/unity-6.3-lts` branch, install the latest patched Unity 6.3 LTS editor with Web Build Support, and run the controlled migration plan in `PLATFORM_STRATEGY.md`. Once that branch compiles and its Prototype 0.1 regression checks pass, execute sections 3–7 above to build, deploy and test the public URL on a physical iPhone in Safari.
