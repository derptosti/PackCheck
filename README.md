# PackCheck

A .NET MAUI packing assistant for the supplied course project. Create a trip from a camping, beach or weekend template, manage quantities, and see the essentials that still need packing. Photograph **one object at a time**, review the model's suggestion, correct it if needed, then explicitly confirm to pack one item.

The app and API support manual packing before Azure is connected. The API is configured to call your team's Azure Custom Vision iteration **Packcheckv1** using the connection values supplied on 7 November 2026. Add the regenerated private prediction key locally, then verify the published model with a real test photo. Model availability and accuracy have not yet been verified from this workspace.

## Run the project

Requirements: Visual Studio with the .NET 10 MAUI workloads, .NET 10 SDK, and SQL Server Express LocalDB. These are installed on the current development machine. Windows can be built here; an iOS/Mac Catalyst build requires a suitable Mac.

**Both the MAUI app and the local API must be running.** The shared `PackCheck.slnLaunch` file adds a **PackCheck + API** startup profile. Select that profile in Visual Studio's startup dropdown to launch both projects together. Use **Windows Machine** as the MAUI target and **http** as the API target. If the profile is unavailable, right-click the solution → **Configure Startup Projects**, choose multiple startup projects, and set **PackCheck.Api** and **PackCheck** to **Start**, leaving the other projects at **None**. See [Microsoft's multiple startup projects guide](https://learn.microsoft.com/en-us/visualstudio/ide/how-to-set-multiple-startup-projects).

If the app's first request happens before the API is ready, click **Refresh** after the API reports that it is listening on port 5092. On Windows, the app's Settings address is `http://localhost:5092/`. The Azure Custom Vision address is configured on the API, separately from this local app-to-API connection.

To run the API separately from Visual Studio instead:

1. Open `PackCheck.slnx` in Visual Studio.
2. Start the API in a terminal from the solution folder:

   ```powershell
   dotnet run --project PackCheck.Api
   ```

   It listens at `http://localhost:5092`. The first run creates the persistent `PackCheck` database in `(localdb)\MSSQLLocalDB`, including the `Trips` and `PackingItems` tables. Check `http://localhost:5092/health` and `http://localhost:5092/api/templates` in a browser.

3. Set `PackCheck` as the startup project and select **Windows Machine**. Run it from Visual Studio. Keep the API terminal running.
4. Create a camping trip. Use **+ 1**, **− 1** and **Edit** to pack items, change quantities or mark essentials. Add a manually tracked item such as a sleeping bag. Return to the trip list to see the updated summary.

The app has Trips, Create Trip, Checklist, Add/Edit Item, Scan and Settings pages. Page code-behind only initializes XAML and attaches an injected ViewModel. `MainPage` was the unused MAUI counter template and has been replaced by `Views/TripsPage` as the starting screen.

### Android or a physical phone

For your unsigned GitHub IPA build and Sideloadly installation, follow the [iPhone setup guide](docs/IPHONE-SETUP.md). The API address can be changed in the installed app's Settings, so a new tunnel hostname does not require another IPA build.

An Android emulator uses `http://10.0.2.2:5092/` by default to reach the host PC. The Windows default is `http://localhost:5092/`. For a physical phone, follow the course tunnel workflow:

Install the CLI and configure your account token using the [official ngrok Windows setup](https://ngrok.com/download/windows) if it is not already available. Keep that token private. Then forward the API port:

```powershell
ngrok http 5092
```

In the app's **Settings**, enter the forwarded **HTTPS origin**, for example `https://your-tunnel.ngrok-free.app`, then select **Save and test connection**. Do not append `/api`. When the free tunnel address changes, update Settings again. Keep both ngrok and the API running. Development Android configuration permits HTTP for emulator testing; use the HTTPS tunnel on a physical device.

## Train and connect Azure

Read [the training guide](docs/AZURE-TRAINING.md) before collecting images. Use the exact tags `backpack`, `shoe`, `water-bottle`, `sunglasses`, `torch`, `toothbrush`, plus the Negative class. A shoe prediction counts **one shoe**; templates require two. Sunglasses count as one pair.

Your supplied public connection values are already merged into `PackCheck.Api/appsettings.json`:

| Setting | Configured value |
| --- | --- |
| Prediction resource | `mbPackcheck-Prediction` |
| Region | `germanywestcentral` |
| Endpoint | `https://mbpackcheck-prediction.cognitiveservices.azure.com` |
| Project ID | `3ede62a6-b26e-4235-ac51-fac155838df1` |
| Published iteration name | `Packcheckv1` |
| Confidence threshold | `0.65` |

The existing service uses API version `v3.1` and the multiclass image classification route. The resource name and region are recorded here for reference; the HTTPS endpoint identifies the resource used by the request.

The remaining setting is **`CustomVision:PredictionKey`**. In Visual Studio, right-click **PackCheck.Api**, choose **Manage User Secrets**, and merge this into the local secrets file, replacing the placeholder with your regenerated prediction key:

```json
{
  "CustomVision": {
    "PredictionKey": "YOUR_NEW_PREDICTION_KEY"
  }
}
```

Alternatively, run this from the solution folder with your real key substituted locally:

```powershell
dotnet user-secrets set "CustomVision:PredictionKey" "YOUR_NEW_PREDICTION_KEY" --project PackCheck.Api
```

The API project already has a `UserSecretsId`; `dotnet user-secrets init` is unnecessary. The normal launch profile uses Development and loads these secrets. Restart the API after setting or replacing the key. Environment variables named `CustomVision__Endpoint`, `CustomVision__ProjectId`, `CustomVision__PublishedName` and `CustomVision__PredictionKey` are an alternative. If older values exist in user secrets or environment variables, update them because they override `appsettings.json`. Keep the key out of source control and the mobile app.

The service builds this prediction URL:

```text
https://mbpackcheck-prediction.cognitiveservices.azure.com/customvision/v3.1/prediction/3ede62a6-b26e-4235-ac51-fac155838df1/classify/iterations/Packcheckv1/image
```

It sends `Prediction-Key` and `Content-Type: application/octet-stream` with the image bytes. Once the key is configured, start the API and choose a held-out photo in the app's Scan page. Check that the suggestion appears and the checklist changes only after confirmation. A 503 means recognition is still unconfigured or unreachable; a 502 after an Azure response requires checking the key, prediction resource, published iteration and image. Do not treat mocked service-test predictions as proof that the live model works.

The default confidence threshold is `0.65`, configurable as `CustomVision:ConfidenceThreshold`. This is a starting value to evaluate with your dataset. Negative, unsupported and low-confidence results require the user to select a supported category. Every accepted suggestion still requires explicit confirmation. The app uploads photos through the API; the API keeps the Azure key and calls the published prediction endpoint. Cancelled, failed or unconfirmed scans do not change checklist data.

JPEG, PNG, GIF and BMP images up to 4 MB are accepted. The photo service requests resizing/compression where the platform supports it and checks the resulting image type and size. Use JPEG instead of HEIC when necessary. No trained model or Azure resources are created automatically.

## Structure and course methods

```text
PackCheck/                MAUI app
  Views/                  XAML and constructor-only code-behind
  ViewModels/             Interfaces, observable state, validation, async commands
  Services/               Generic ApiService<T>, domain services, navigation, photos, settings
  Messages/               TripCreatedMessage, PackingItemUpdatedMessage, API address changes
  Behaviors/              Page lifecycle to command wiring
PackCheck.Contracts/      Shared request/response models, categories and templates
PackCheck.Api/
  Controllers/            HTTP endpoints
  Services/               Business rules and Azure recognition
  Repositories/           Asynchronous database access
  Entities/               Trip and PackingItem, with a one-to-many relationship
  Data/                   EF Core DbContext
PackCheck.Tests/          Automated tests
PackCheck.ViewModelTests/ Scan confirmation and message/lifecycle regression tests
docs/                     Course mapping, model training and demo checks
```

Views receive ViewModel interfaces through dependency injection. ViewModels use Community Toolkit `ObservableRecipient`, observable collections and relay commands. `INavigationService` resolves pages through DI, while ViewModels pass identifiers without referencing Views. `TripCreatedMessage` invalidates the trip list; `PackingItemUpdatedMessage` invalidates the relevant checklist and progress summaries.

The shared DTOs replace the initial unused app model stubs and keep API request/response fields consistent. API entities remain separate from DTOs. All writes go through service validation and repositories. A packing item belongs to one trip and deleting that trip removes its checklist. The database uses `EnsureCreatedAsync` for the first version; introduce EF migrations before evolving an existing database schema.

See [the requirement-to-code map](docs/COURSE-ALIGNMENT.md) for the relevant slide numbers and [the demo checklist](docs/DEMO-CHECKLIST.md) for the complete workflow.

## API

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/health` | API readiness after database initialization |
| GET | `/api/templates` | Reusable packing templates |
| GET / POST | `/api/trips` | List trips / create a trip from a template |
| GET / PUT / DELETE | `/api/trips/{id}` | Read / edit / delete a trip |
| GET / POST | `/api/trips/{tripId}/items` | Read checklist / add an item |
| GET / PUT / DELETE | `/api/trips/{tripId}/items/{itemId}` | Read / edit / delete one item |
| POST | `/api/recognition` | Classify multipart form field `image`; no checklist write |

Create-trip body: `name`, `departureDate`, `templateKey` (`camping`, `beach`, `weekend`). Item create/update body: `name`, nullable `categoryKey`, `requiredQuantity`, `packedQuantity`, `isEssential`. Quantities obey `1 <= requiredQuantity <= 999` and `0 <= packedQuantity <= requiredQuantity`. Invalid input returns 400; cross-trip or missing items return 404. Recognition returns 503 when unconfigured. API errors use ProblemDetails.

The API has no accounts in this agreed first version. The course tunnel is for the demo; use authentication before hosting it for general use.

## Checks

```powershell
dotnet test PackCheck.Tests/PackCheck.Tests.csproj
dotnet test PackCheck.ViewModelTests/PackCheck.ViewModelTests.csproj
dotnet build PackCheck/PackCheck.csproj -f net10.0-windows10.0.19041.0
dotnet build PackCheck/PackCheck.csproj -f net10.0-android
```

The integration tests use an isolated SQLite relational database, leaving your real LocalDB trip data untouched. Azure service tests use controlled HTTP responses and make no Azure calls. Passing these checks does not prove model accuracy, physical camera permissions or device appearance. Complete the device and trained-model checks in the demo checklist before presenting.

The ViewModel tests compile the actual application source with minimal stand-ins for image preview and permission types. They verify scan confirmation and message behavior without starting a UI. With the API running, you can also check real database writes:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Api.ps1
```

That command uses a process-local execution policy and removes only the test trip it creates.
