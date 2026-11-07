# PackCheck and the course requirements

The brief is `C:\Users\broec\Downloads\Project.txt` and the pitch is `C:\Users\broec\Downloads\Pitch.txt`. The two supplied Challenge 1 decks contain the same assignment. Slide 3 is the assessment checklist; slide 2 requires useful functionality beyond a recognizer and favorites.

## Requirement map

| Requirement | Course source | Where to inspect and demonstrate it |
| --- | --- | --- |
| .NET MAUI with MVVM Community Toolkit | Challenge 1, slide 3; MAUI 2, slides 2–4, 6 and 10 | `PackCheck/Views`, `PackCheck/ViewModels`, Community Toolkit observable state and asynchronous commands |
| Application logic outside page code-behind | Challenge 1, slide 3; MAUI 2, slides 7–12 | Page constructors initialize the view and receive their ViewModel through DI; commands, validation and data loading belong to ViewModels/services |
| Models and folder conventions | MAUI 1, slides 4–6; MAUI 2, slide 8 | `Views`, `ViewModels`, `Services`, `Messages`, API layers and shared request/response contracts |
| Observable lists and data binding | MAUI 3, slides 2–9 | Trip and packing item collections update the bound list controls |
| Generic API service with domain data services | MAUI 4, slide 7, and accompanying course starter | `IApiService<T>`, abstract `ApiService<T>` and trip/packing item data services |
| Dependency injection | MAUI 2, slide 12; MAUI 3, slide 9; MAUI 4, slide 10 | Registrations in `MauiProgram.cs`; interfaces injected into Views, ViewModels and services |
| Multiple pages and a navigation service | Navigation & Messages, slides 6–10 | Trips, new trip, checklist, packing item, scan and settings pages; ViewModels call the navigation abstraction |
| At least two messages | Challenge 1, slide 3; Navigation & Messages, slides 11–15 | `TripCreatedMessage` refreshes trips; `PackingItemUpdatedMessage` invalidates packing summaries/checklists |
| User input validation | Navigation & Messages, slide 16 | Trip/item names, quantities and API address validated; errors shown in the app |
| .NET API reads and writes | Challenge 1, slide 3 | Create/read/update/delete trip and packing item endpoints |
| Two tables with a one-to-many relationship, both used | Challenge 1, slide 3 | `Trip` has many `PackingItem` entities, stored in the local relational database |
| Layered API, repositories and asynchronous calls | Challenge 1, slide 3 | API Controllers → Services → Repositories → EF Core DbContext |
| Local API/database exposed to a device | Challenge 1, slide 3 | SQL Server LocalDB plus API, accessed from a device through an ngrok HTTPS URL |
| A model created and trained in Azure | Challenge 1, slide 3; Azure 1, slides 10–15 | Train the six packing classes plus Negative, publish an iteration and configure its prediction endpoint |
| Asynchronous image classification service | Azure 1, slides 17–21 | MAUI recognition service uploads one image; API classification service calls the published Azure model |
| Camera/gallery permissions | Azure 1, slide 22 | Platform configuration and the photo service |
| Useful, friendly UI | Challenge 1, slides 2–3 | Saved trips, reusable templates, missing essentials, quantities/progress and explicit scan confirmation |

The other Azure decks show different services. Document Intelligence, object detection, translation and other AI APIs are examples, not additional required integrations for PackCheck. Image classification is sufficient for this project's single-item scanning workflow. A generic, pretrained image-analysis API by itself would not demonstrate that the team trained a model.

## Source decks

All source decks were supplied in `C:\Users\broec\Downloads`:

- `Challenge 1 (2).pptx` and `Challenge 1 (3).pptx`
- `Maui 1. Introduction Maui Framework (1).pptx`
- `Maui 2. Introduction MVVM (1).pptx`
- `Maui 3. MVVM Collections (1).pptx`
- `Maui 4. MVVM DataServices (1).pptx`
- `5. MVVM Navigation & Messages (1).pptx`
- `Azure 1. Custom Vision (1).pptx`
- `Azure 2. Document Intelligence (1).pptx`
- `Azure 3. Object Detection.pptx`
- `Azure 4. More AI Services (2).pptx`

The accompanying `Maui 4. Start.zip` and `Start MVVM 5.zip` illustrate the course's ViewModel contracts, generic HTTP CRUD services, collection bindings and injected Views. Safely awaited commands can replace the starter's `async void` loads without changing the architecture.

## Evidence that still requires the team

Implementation is not evidence that a model has been trained, that camera permission works on a physical device, or that the final UI has been inspected. Complete `AZURE-TRAINING.md` and `DEMO-CHECKLIST.md`, record real test results and keep the Azure iteration/dataset evidence for the presentation. Do not label unperformed checks as passed.
