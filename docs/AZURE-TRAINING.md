# Train and connect the PackCheck classifier

The course workflow is Azure 1, slides 10–15: collect/tag images, include a Negative class, train, test and publish. Slides 17–22 place classification behind an asynchronous service and configure camera/gallery access.

PackCheck scans **one object at a time**. It suggests a category; the user chooses the matching checklist entry and confirms before its packed quantity changes. A photograph does not prove that an item is inside a suitcase.

**Connection update, 7 November 2026:** the team supplied the prediction resource `mbPackcheck-Prediction` in `germanywestcentral`, project `3ede62a6-b26e-4235-ac51-fac155838df1`, and published iteration name `Packcheckv1`. These public connection values and the 0.65 confidence threshold are now configured in the API. The regenerated private prediction key is still required locally. Live publication, model tags and holdout accuracy remain unverified from this workspace. Follow the connection steps in the root README; use the training guidance below when creating or improving the dataset.

## Collect a dataset

Use these exact category keys as Azure tag names so predictions map to the app:

| Azure tag | What belongs in it |
| --- | --- |
| `backpack` | One backpack |
| `shoe` | One shoe; quantities count individual shoes |
| `water-bottle` | One reusable or disposable water bottle |
| `sunglasses` | One pair of sunglasses |
| `torch` | One handheld torch/flashlight |
| `toothbrush` | One toothbrush |
| Negative | Unrelated objects, empty backgrounds and images outside the supported categories |

Collect varied examples for each category: different physical objects, lighting, angles, distances and backgrounds. Use your own photos or photos whose license allows this use. Avoid a dataset where every class has a distinct background; the model may learn the background instead of the object.

Start with **about 50 varied training photos per supported tag**, plus **about 10 separate holdout photos per tag**. That is roughly 300 training and 60 holdout photos for the six supported categories. Collect a similarly varied Negative set, with its own held-out examples. Microsoft's [Custom Vision overview](https://learn.microsoft.com/azure/ai-services/custom-vision-service/overview) recommends 50 images per label as a starting point. The 10-photo holdout target is our practical project suggestion. These targets are neither assignment thresholds nor a guarantee of accuracy. The service's classification [minimum is five labeled images per tag](https://learn.microsoft.com/en-us/azure/ai-services/custom-vision-service/limits-and-quotas), which is an upload/training minimum rather than a useful evaluation target.

Reserve a separate holdout set **before uploading training images**. Keep near-duplicates and photos of the same photo burst in the same split. Where practical, use some physical objects and backgrounds in the holdout set that do not appear in training. Record counts by class; keep class counts reasonably balanced. Follow the portal's current minimum image requirements, and collect more varied images if a class performs poorly.

## Create and train the project

1. Sign into your course Azure subscription and open [Custom Vision](https://www.customvision.ai/).
2. Use the course-provided training and prediction resources, or create them under the subscription's authorized budget.
3. Create a classification project with **Multiclass (single tag per image)**. Choose an appropriate general domain.
4. Add the six tags above. Add out-of-scope examples using the portal's **Negative** tag/class option.
5. Upload only the training set, check tags and train an iteration.
6. Test photographs from the holdout set using Quick Test. Review the predicted tag and confidence, including Negative images.
7. If results expose a weakness, collect new training examples of that weakness, train another iteration and evaluate with new holdout photos. Keep the final evaluation images out of training.

Do not claim training or accuracy until it has actually run. Do not use the portal's training-set score as the only evidence of useful recognition.

## Evaluate and keep evidence

Record the final project/iteration name, train/holdout counts and actual results. A useful report records true class, predicted class and confidence for each holdout photo. Summarize correct/total counts per class, confusion between classes and how often Negative images were accepted as supported items. Include examples of errors and explain why the app asks for confirmation.

Copy `training-results-template.csv` and fill one row per actual held-out image. Enter probability as a number from 0 to 1 and whether the prediction was correct. Keep the template empty until real evaluation runs; test fixtures in automated tests are not model evaluation results.

The app's initial confidence threshold is 0.65. Treat it as a starting setting, not an accuracy guarantee. Review its effect using your own holdout results. A low-confidence or Negative result should lead the user to choose a category or add an item manually.

## Publish and configure

1. Publish the chosen iteration to the prediction resource. Copy the published iteration **name**, project ID and prediction endpoint.
2. Configure the API through .NET user secrets or environment variables. Never put the prediction key in MAUI source, XAML, committed configuration or screenshots.
3. In the API project, use the configuration keys documented in its `appsettings.json` and the root README. The endpoint is the resource's HTTPS origin, the project ID is a GUID, and the published name must exactly match Azure.
4. Start the API, set the MAUI API address in Settings, and test one held-out image through the app.
5. Confirm that selecting a photo alone causes no checklist write. Confirm that pressing the confirmation button increments only the chosen entry by one and updates the trip progress.

The prediction service sends the image to:

```http
POST https://<resource>.cognitiveservices.azure.com/customvision/v3.1/prediction/<project-id>/classify/iterations/<published-name>/image
Prediction-Key: <server-side prediction key>
Content-Type: application/octet-stream
```

Use a JPEG, PNG, GIF or BMP image below 4 MB. Keep rejected, cancelled and failed uploads from changing packing data. If recognition is unconfigured, manual checklist editing must continue to work and the API should explain that recognition is unavailable.

## References

- [Microsoft: Custom Vision classification quickstart](https://learn.microsoft.com/en-us/azure/ai-services/custom-vision-service/quickstarts/image-classification)
- [Microsoft: prediction REST API and image constraints](https://learn.microsoft.com/en-us/rest/api/customvision/image-classifier/classify-image?view=rest-customvision-v3.1)
- [Microsoft: Custom Vision migration options](https://learn.microsoft.com/azure/ai-services/custom-vision-service/migration-options) lists retirement on 25 September 2028. Follow the course's supported setup for this assignment and discuss the future migration if the app is continued beyond the course.
