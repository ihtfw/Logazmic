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

        private static readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Logazmic\filters_profiles.json");

        private static readonly Lazy<FiltersProfiles> instance = new Lazy<FiltersProfiles>(() => Load<FiltersProfiles>(path));

        public static FiltersProfiles Instance { get { return instance.Value; } }
        public override void Save()
        {
            Save(path);
        }

        #endregion

        private List<FiltersProfile> profiles;

        public List<FiltersProfile> Profiles
        {
            get { return profiles ?? (profiles = new List<FiltersProfile>()); }
            set { profiles = value; }
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
