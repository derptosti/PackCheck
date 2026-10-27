using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PackCheck.Models;

public partial class Trip : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private DateTime departureDate = DateTime.Today;

    [ObservableProperty]
    private string tripType = string.Empty;
}