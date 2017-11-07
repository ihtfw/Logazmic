using System;
using System.Linq;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class ProfilesFiltersViewModel : PropertyChangedBase,IActivate
    {
        private readonly FiltersProfile filtersProfile;
        private readonly LogPaneServices logPaneServices;

        public ProfilesFiltersViewModel(FiltersProfile filtersProfile, LogPaneServices logPaneServices)
        {
            this.filtersProfile = filtersProfile;
            this.logPaneServices = logPaneServices;
        }
        
        public string ProfileName
        {
            get { return filtersProfile.Name; }
            set { filtersProfile.Name = value; }
        }
        
        public BindableCollection<string> ProfileNames { get; } = new BindableCollection<string>();
        
        public void SaveCurrentProfile()
        {
            FiltersProfiles.Instance.Replace(filtersProfile);
            ReloadProfileNames();
        }

        public void LoadProfile(string profileName)
        {
            var profile = FiltersProfiles.Instance.Get(profileName);
            if (profile == null)
                return;

            filtersProfile.Apply(profile);

            NotifyOfPropertyChange(nameof(ProfileName));

            logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Filters);
        }

        public void RemoveProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
                return;

            FiltersProfiles.Instance.Remove(profileName);
            ProfileNames.Remove(profileName);
        }

        public void Activate()
        {
            ReloadProfileNames();

            Activated?.Invoke(this, new ActivationEventArgs());
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
