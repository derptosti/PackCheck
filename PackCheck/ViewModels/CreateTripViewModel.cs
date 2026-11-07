using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PackCheck.Contracts;
using PackCheck.Messages;
using PackCheck.Services;
using System.Collections.ObjectModel;

namespace PackCheck.ViewModels;

public sealed class CreateTripViewModel : BaseViewModel, ICreateTripViewModel
{
    private readonly ITemplateDataService templates;
    private string name = string.Empty;
    private DateTime departureDate = DateTime.Today;
    private PackingTemplateDto? selectedTemplate;

    public CreateTripViewModel(ITemplateDataService templates, ITripDataService trips,
        INavigationService navigation, IMessenger messenger) : base(messenger)
    {
        this.templates = templates;
        ReloadCommand = new AsyncRelayCommand(() => ExecuteAsync(LoadTemplatesAsync));
        SaveCommand = new AsyncRelayCommand(() => ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(Name) || Name.Trim().Length > 100)
            {
                ErrorMessage = "Enter a trip name between 1 and 100 characters.";
                return;
            }
            if (SelectedTemplate is null)
            {
                ErrorMessage = "Choose a packing template first.";
                return;
            }
            var trip = await trips.CreateAsync(new CreateTripRequest
            {
                Name = Name.Trim(), DepartureDate = DepartureDate, TemplateKey = SelectedTemplate.Key
            });
            Messenger.Send(new TripCreatedMessage(trip.Id));
            await navigation.GoBackAsync();
            await navigation.NavigateToAsync("trip", trip.Id);
        }));
    }

    public string Name { get => name; set => SetProperty(ref name, value); }
    public DateTime DepartureDate { get => departureDate; set => SetProperty(ref departureDate, value); }
    public ObservableCollection<PackingTemplateDto> Templates { get; } = [];
    public PackingTemplateDto? SelectedTemplate { get => selectedTemplate; set => SetProperty(ref selectedTemplate, value); }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand ReloadCommand { get; }
    protected override Task OnAppearingAsync() => Templates.Count == 0 ? LoadTemplatesAsync() : Task.CompletedTask;

    private async Task LoadTemplatesAsync()
    {
        var result = await templates.GetAllAsync();
        Templates.Clear();
        foreach (var template in result) Templates.Add(template);
        SelectedTemplate = Templates.FirstOrDefault();
    }
}
