using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PackCheck.Models;
using PackCheck.ViewModels.Interfaces;

namespace PackCheck.ViewModels;

public class TripsViewModel : ObservableObject, ITripsViewModel
{
    public ObservableCollection<Trip> Trips { get; } = new();

    public TripsViewModel()
    {
        Trips.Add(new Trip
        {
            Id = 1,
            Name = "Camping weekend",
            DepartureDate = DateTime.Today.AddDays(7),
            TripType = "Camping"
        });

        Trips.Add(new Trip
        {
            Id = 2,
            Name = "City break",
            DepartureDate = DateTime.Today.AddDays(14),
            TripType = "Weekend"
        });
    }
}