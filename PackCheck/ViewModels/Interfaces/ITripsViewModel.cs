using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PackCheck.Models;

namespace PackCheck.ViewModels.Interfaces;

public interface ITripsViewModel : INotifyPropertyChanged
{
    ObservableCollection<Trip> Trips { get; }
}