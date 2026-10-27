using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PackCheck.Models;

public partial class PackingItem : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int tripId;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? categoryKey;

    [ObservableProperty]
    private int requiredQuantity = 1;

    [ObservableProperty]
    private int packedQuantity;
}