using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class ProfilesFiltersViewModel : PropertyChangedBase, IActivate
    {
        private readonly FiltersProfile _filtersProfile;
        private readonly LogPaneServices _logPaneServices;

        public ProfilesFiltersViewModel(FiltersProfile filtersProfile, LogPaneServices logPaneServices)
        {
            _filtersProfile = filtersProfile;
            _logPaneServices = logPaneServices;
        }
        
        public string ProfileName
        {
            get => _filtersProfile.Name;
            set => _filtersProfile.Name = value;
        }
        
        public BindableCollection<string> ProfileNames { get; } = new();
        
        public void SaveCurrentProfile()
        {
            FiltersProfiles.Instance.Replace(_filtersProfile);
            ReloadProfileNames();
        }

        public void LoadProfile(string profileName)
        {
            var profile = FiltersProfiles.Instance.Get(profileName);
            if (profile == null)
                return;

            _filtersProfile.Apply(profile);

            NotifyOfPropertyChange(nameof(ProfileName));

            _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Filters);
        }

        public void RemoveProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
                return;

            FiltersProfiles.Instance.Remove(profileName);
            ProfileNames.Remove(profileName);
        }

        public Task ActivateAsync(CancellationToken cancellationToken)
        {
            ReloadProfileNames();

            Activated?.Invoke(this, new ActivationEventArgs());

            return Task.CompletedTask;
        }

        private void ReloadProfileNames()
        {
            ProfileNames.Clear();
            ProfileNames.AddRange(FiltersProfiles.Instance.Profiles.Select(p => p.Name));
        }

        public bool IsActive { get; private set; }
        public event EventHandler<ActivationEventArgs> Activated;
    }
}
