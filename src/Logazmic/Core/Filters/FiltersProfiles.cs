using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Logazmic.Settings;

namespace Logazmic.Core.Filters
{
    public class FiltersProfiles : JsonSettingsBase
    {
        #region Singleton

        private static readonly string SettingFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Logazmic\filters_profiles.json");

        private static readonly Lazy<FiltersProfiles> LazyInstance = new Lazy<FiltersProfiles>(() => Load<FiltersProfiles>(SettingFilePath));

        public static FiltersProfiles Instance => LazyInstance.Value;

        public override void Save()
        {
            Save(SettingFilePath);
        }

        #endregion

        private List<FiltersProfile> _profiles;

        public List<FiltersProfile> Profiles
        {
            get => _profiles ?? (_profiles = new List<FiltersProfile>());
            set => _profiles = value;
        }

        public FiltersProfile Add()
        {
            var filtersProfile = new FiltersProfile();
            Profiles.Add(filtersProfile);
            return filtersProfile;
        }

        [CanBeNull]
        public FiltersProfile Get(string name)
        {
            return Profiles.FirstOrDefault(p => p.Name == name);
        }

        public bool Remove(string profileName)
        {
            var profile = Get(profileName);
            if (profile != null)
            {
                if (Profiles.Remove(profile))
                {
                    Save();
                    return true;
                }
            }

            return false;
        }

        public void Replace(FiltersProfile filtersProfile)
        {
            if (filtersProfile == null)
                return;

            var copy = new FiltersProfile();
            copy.Apply(filtersProfile);

            Remove(filtersProfile.Name);
            Profiles.Add(copy);

            Save();
        }
    }
}
