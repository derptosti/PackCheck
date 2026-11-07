# PackCheck on an iPhone with Sideloadly

Keep the unsigned IPA build and install it through Sideloadly as you do for your other MAUI projects. Sideloadly handles signing for this installation route. The GitHub build produces the mobile app; the API and SQL Server database continue to run on your computer.

## Connect the iPhone to the API

1. Keep `PackCheck.Api` running on the PC. If it is stopped, run this from the solution folder:

   ```powershell
   dotnet run --project PackCheck.Api
   ```

2. Check `http://localhost:5092/health` on the PC. It should return `"status":"ready"`.
3. Install and connect ngrok using the [official Windows setup](https://ngrok.com/download/windows), if needed, then open a second terminal and run:

   ```powershell
   ngrok http 5092
   ```

4. Copy the forwarded **HTTPS** address printed by ngrok, for example `https://your-assigned-host.ngrok-free.app`.
5. On the iPhone, open Safari and visit `https://your-assigned-host.ngrok-free.app/api/templates`. Confirm that it reaches the API and returns the packing templates. A browser-only ngrok introduction page may appear first. PackCheck already sends the `ngrok-skip-browser-warning` header on its API requests.
6. Install the IPA through Sideloadly. Open PackCheck → **Settings** and enter the forwarded HTTPS address as the **API base address**, then press **Save and test connection**. Enter the hostname only; the app adds `/api` itself.
7. Return to Your trips and press **Refresh**, then try the packing and photo workflows.

`localhost` refers to the device making a request. On the iPhone, `http://localhost:5092/` points to the iPhone itself. `10.0.2.2` is an Android emulator address. Use the public ngrok HTTPS address for this setup.

Keep the PC awake, connected to the internet, and running both the API and ngrok while using the phone. If the tunnel address changes, update PackCheck Settings; the IPA does not need rebuilding. This public tunnel works without putting the phone on the same Wi-Fi network as the PC.

The app sends images to your .NET API. The API calls Azure Custom Vision with the key already stored in the API's local user secrets. Configure the phone with your API's ngrok address. The Azure prediction endpoint remains in API configuration, and the prediction key stays on the server.

## iOS platform configuration

The project already contains the camera and photo-library usage descriptions needed by the photo service. Public HTTPS requests through ngrok use the normal iOS networking rules; this setup requires no broad App Transport Security exception or local-network permission declaration. Connecting directly to a private LAN address would be a separate configuration.

This project now registers `SceneDelegate : MauiUISceneDelegate` in the iOS `Info.plist`. This is required for apps built with Xcode 27. The existing AppDelegate and MAUI window/navigation setup remain in use. The privacy manifest also declares `CA92.1` for the app's Preferences API usage, which saves the selected API address.

## Your build workflow

The existing unsigned IPA / Payload / Sideloadly route is retained. There is no need to introduce Apple distribution certificates into GitHub for that route. `xcode-27` is a valid GitHub-hosted runner label.

Your workflow's project search should find `PackCheck/PackCheck.csproj`. You can set that path directly if you want to avoid project discovery in this multi-project solution.

Keep the installed .NET MAUI workload compatible with the selected Xcode version. Microsoft's documented Xcode 27.1 pairing is .NET SDK `10.0.402` with workload set `10.0.402.1`; MAUI `10.0.110` or newer is recommended for scene lifecycle support. Your current `10.0.x` / latest-workload approach can remain in place when its installed versions match. If a runner reports a toolchain mismatch, use the exact matching versions rather than disabling Xcode version validation.

The source changes have been checked locally for plist structure and platform inclusion. Building the IPA and verifying cold launch, camera/photo access and API connectivity on the iPhone still require the macOS GitHub runner and your device.

## References

- [Microsoft: prepare an existing MAUI app for Xcode 27](https://devblogs.microsoft.com/dotnet/migrate-dotnet-maui-apps-uikit-scene-lifecycle/)
- [Microsoft: Xcode 27.1 workload release and required versions](https://github.com/dotnet/macios/releases/tag/dotnet-10.0.1xx-xcode27.1-10018)
- [GitHub: hosted runner labels](https://docs.github.com/en/actions/reference/runners/github-hosted-runners)
- [Microsoft: MAUI iOS privacy manifest and Preferences](https://learn.microsoft.com/dotnet/maui/ios/privacy-manifest)
- [ngrok: HTTPS endpoints and local upstreams](https://ngrok.com/docs/gateway/endpoints/http)
