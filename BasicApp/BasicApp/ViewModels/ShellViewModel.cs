﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using BasicApp.Contracts.Services;
using BasicApp.Helpers;
using BasicApp.Properties;

using MahApps.Metro.Controls;

namespace BasicApp.ViewModels
{
    public class ShellViewModel : Observable
    {
        private readonly INavigationService _navigationService;
        private readonly IUserDataService _userDataService;
        private HamburgerMenuItem _selectedMenuItem;
        private HamburgerMenuItem _selectedOptionsMenuItem;
        private RelayCommand _goBackCommand;
        private ICommand _menuItemInvokedCommand;
        private ICommand _optionsMenuItemInvokedCommand;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        public HamburgerMenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { Set(ref _selectedMenuItem, value); }
        }

        public HamburgerMenuItem SelectedOptionsMenuItem
        {
            get { return _selectedOptionsMenuItem; }
            set { Set(ref _selectedOptionsMenuItem, value); }
        }

        // TODO WTS: Change the icons and titles for all HamburgerMenuItems here.
        public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Resources.ShellMainPage, Glyph = "\uE8A5", TargetPageType = typeof(MainViewModel) },
        };

        public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
        };

        public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, CanGoBack));

        public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ?? (_menuItemInvokedCommand = new RelayCommand(OnMenuItemInvoked));

        public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ?? (_optionsMenuItemInvokedCommand = new RelayCommand(OnOptionsMenuItemInvoked));

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(INavigationService navigationService, IUserDataService userDataService)
        {
            _navigationService = navigationService;
            _navigationService.Navigated += OnNavigated;
            _userDataService = userDataService;
            _userDataService.UserDataUpdated += OnUserDataUpdated;
        }

        private void OnLoaded()
        {
            var user = _userDataService.GetUser();
            var userMenuItem = new HamburgerMenuImageItem()
            {
                Thumbnail = user.Photo,
                Label = user.Name,
                Command = new RelayCommand(OnUserItemSelected)
            };

            OptionMenuItems.Insert(0, userMenuItem);
        }

        private void OnUnloaded()
        {
            _navigationService.Navigated -= OnNavigated;
            _userDataService.UserDataUpdated -= OnUserDataUpdated;
        }

        private void OnUserDataUpdated(object sender, UserViewModel user)
        {
            var userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
            if (userMenuItem != null)
            {
                userMenuItem.Label = user.Name;
                userMenuItem.Thumbnail = user.Photo;
            }
        }

        private bool CanGoBack()
            => _navigationService.CanGoBack;

        private void OnGoBack()
            => _navigationService.GoBack();

        private void OnMenuItemInvoked()
            => NavigateTo(SelectedMenuItem.TargetPageType);

        private void OnOptionsMenuItemInvoked()
            => NavigateTo(SelectedOptionsMenuItem.TargetPageType);

        private void OnUserItemSelected()
            => NavigateTo(typeof(SettingsViewModel));

        private void NavigateTo(Type targetViewModel)
        {
            if (targetViewModel != null)
            {
                _navigationService.NavigateTo(targetViewModel.FullName);
            }
        }

        private void OnNavigated(object sender, string viewModelName)
        {
            var item = MenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            if (item != null)
            {
                SelectedMenuItem = item;
            }
            else
            {
                SelectedOptionsMenuItem = OptionMenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            }

            GoBackCommand.OnCanExecuteChanged();
        }
    }
}
