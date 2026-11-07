# Development validation

Executed on 27 October 2026 on the development Windows machine with .NET SDK 10.0.401.

| Check | Result |
| --- | --- |
| `dotnet test PackCheck.Tests/PackCheck.Tests.csproj --no-restore -v minimal` | 17 passed, 0 failed, 0 skipped. Includes 10 relational API integration cases and 7 controlled HTTP Custom Vision service cases. |
| `dotnet test PackCheck.ViewModelTests/PackCheck.ViewModelTests.csproj --no-restore -v minimal` | 17 passed, 0 failed, 0 skipped. Includes confirmation, corrections, cancellation, stale items, message/lifecycle behavior and recovery from a failed item load. |
| `dotnet build PackCheck/PackCheck.csproj -f net10.0-windows10.0.19041.0 --no-restore -v minimal` | Passed, 0 warnings, 0 errors. |
| `dotnet build PackCheck/PackCheck.csproj -f net10.0-android --no-restore -v minimal` | Passed, 0 warnings, 0 errors. |
| Real LocalDB smoke test with `scripts/Smoke-Api.ps1` | Passed: health, camping trip/template item creation, packed torch write, read-back and progress. The test removed its own trip and checklist. |
| Windows app runtime through Computer Use | Passed: app startup, create a Camping trip, pack one Backpack, edit Shoes required quantity from 2 to 3, open Scan, navigate back, and verify the trips list shows 1 / 10 packed with 6 essentials missing. API reads independently confirmed the saved quantities. The disposable UI test trip was removed. |

The API integration tests use an isolated SQLite relational database. The smoke test separately exercised the configured SQL Server LocalDB database. Custom Vision HTTP tests use controlled responses and do not call Azure. ViewModel tests use real application source with compile-only image-preview and permission stand-ins.

Runtime validation found and corrected two issues: the Shell is now resolved after application resource initialization, and the item editor blocks existing-item writes until its original data loads successfully. Failed item loads expose a retry action. The latter has three regression tests.

## Still to verify

- Verify the supplied Azure model's publication and tags with an authenticated prediction, then record actual holdout results. The connection update below supersedes the earlier untrained-model status.
- Inspect small-screen layout and exercise navigation on the selected phone, if a phone is the demo target. The Windows workflow above has been inspected.
- Check camera/photo permissions, capture, selection and cancellation on that device.
- Configure ngrok and test a physical device against the local API.
- Build iOS or Mac Catalyst on a suitable Mac if those are selected targets.

See [the training guide](AZURE-TRAINING.md) and [the demo checklist](DEMO-CHECKLIST.md). A successful build and unit tests do not substitute for these checks.

## Azure connection update, 7 November 2026

The public API configuration now points to `mbPackcheck-Prediction` in `germanywestcentral`, project `3ede62a6-b26e-4235-ac51-fac155838df1`, iteration `Packcheckv1`, with confidence threshold 0.65. The existing service uses the matching v3.1 multiclass prediction URL, `Prediction-Key` authentication and an image byte request body.

No prediction key was supplied or stored by this update. Local user secrets and the current environment had no prediction key configured at the time of inspection. An authenticated Azure call, the resource/iteration publication and real recognition accuracy have not been verified.

Validation for this update: the JSON parsed successfully, the constructed model URL exactly matched the supplied v3.1 URL, the confidence threshold remained 0.65, and no prediction-key property was present in public configuration. `dotnet test PackCheck.Tests/PackCheck.Tests.csproj --no-restore -v minimal` passed all 17 API/service tests on 7 November 2026. These tests use isolated database data and controlled Azure HTTP responses; they made no live Azure prediction calls.
