# PackCheck acceptance and demo checklist

Check these on the actual target device. The checklist is a plan; an unchecked item is not a test result.

## Before rehearsing

- [ ] Build the API and MAUI app using the commands in the README.
- [ ] Run the automated tests. Record command, date and actual result.
- [ ] Start SQL Server LocalDB and the API. Confirm `/health` responds.
- [ ] Start ngrok for the API port. Put its current HTTPS URL in the app's Settings.
- [ ] Publish the trained Azure model and configure the API's prediction settings.
- [ ] Check camera permission, gallery selection and cancellation on the target device.
- [ ] Use photos from the holdout set to test recognition; record real results.

## Complete the main workflow

- [ ] Create a trip named “Ardennes camping” with the Camping template and a departure date.
- [ ] Check the template includes a torch, water bottle and sleeping bag.
- [ ] Check new items start with packed quantity zero and trip progress starts at zero.
- [ ] Open an item, change its required quantity, save and check it survives opening the trip again.
- [ ] Add a manual item outside the six recognized categories, such as a sleeping mat.
- [ ] Photograph or choose a photo of one torch. Check that the prediction does not immediately change the checklist.
- [ ] Select the matching entry, confirm, and check exactly one unit is added.
- [ ] Scan a water bottle and confirm its checklist entry.
- [ ] Check the sleeping bag remains a missing essential.
- [ ] Return to the trips list and check the updated packing summary appears.
- [ ] Close/restart the app and API, reopen the trip and verify saved packing quantities persist.

Shoes are counted individually: a template with required quantity two expects two shoes. Show that convention when demonstrating quantities.

## Corrections and failures

- [ ] Correct a wrong prediction by choosing another supported category before confirming.
- [ ] Try an unrelated object. A Negative/unknown/low-confidence result must ask for review rather than mark an arbitrary item packed.
- [ ] Try a supported category absent from the trip. Check the UI explains how to add it manually.
- [ ] Try a category with multiple checklist entries and check only the selected entry changes.
- [ ] Try an already complete entry. Packed quantity must not exceed required quantity.
- [ ] Cancel the camera/gallery picker. No data should change.
- [ ] Deny camera permission. The app should show a useful error and keep manual editing usable.
- [ ] Stop the API or set an incorrect address. Check error/retry behavior, then restore the address.
- [ ] Remove prediction configuration temporarily. Confirm recognition reports unavailable and manual packing still works.
- [ ] Try invalid/blank names, required quantity zero, negative packed quantity and packed quantity above required. Invalid data must not be saved.
- [ ] Delete an item only after confirmation and check it disappears from the checklist and progress.
- [ ] Delete a disposable trip after confirmation and check its packing items are removed with it.
- [ ] Check text contrast, small-screen layout, scrolling and keyboard visibility on the target device.

## Explain the architecture

- [ ] Point to a ViewModel command and the XAML binding that invokes it.
- [ ] Show that page code-behind has no application logic.
- [ ] Show the navigation service and injected ViewModel contracts.
- [ ] Demonstrate `TripCreatedMessage` and `PackingItemUpdatedMessage` refreshing the relevant screens.
- [ ] Trace one API request through controller, service, repository and DbContext.
- [ ] Show the `Trip` → `PackingItem` foreign key in the actual database.
- [ ] Show Azure dataset tags, the published iteration and holdout evaluation results.
- [ ] Explain the confidence threshold and user confirmation without claiming that AI verifies an entire suitcase.

## Evidence log

| Date | Device/platform | Check or command | Actual result | Follow-up |
| --- | --- | --- | --- | --- |
| | | | | |

Keep API/Azure keys out of the demo recording. The project does not include accounts, weather, whole-suitcase detection or generated checklists in this first version.
