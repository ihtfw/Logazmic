using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class SourceFilterViewModel : PropertyChangedBase
    {
        private SourceFilter _sourceFilter;
        private readonly LogPaneServices _logPaneServices;

        public SourceFilterViewModel(SourceFilter sourceFilter, LogPaneServices logPaneServices)
        {
            _sourceFilter = sourceFilter;
            _logPaneServices = logPaneServices;
        }

        public void Rebuild(SourceFilter rootSourceFilter)
        {
            _sourceFilter = rootSourceFilter;

            Children.Clear();

            foreach (var child in _sourceFilter.Children)
            {
                AddChild(child);
            }

            IsSelected = false;
            NotifyOfPropertyChange(nameof(IsChecked));
            NotifyOfPropertyChange(nameof(Name));
        }

        private void AddChild(SourceFilter child)
        {
            var childViewModel = new SourceFilterViewModel(child, _logPaneServices);
            Children.Add(childViewModel);

            foreach (var subChild in child.Children)
            {
                childViewModel.AddChild(subChild);
            }
        }

        public bool IsSelected { get; set; }

        public bool IsChecked
        {
            get => _sourceFilter.IsChecked;
            set
            {
                _sourceFilter.IsChecked = value;
                foreach (var child in Children)
                {
                    child.IsChecked = value;
                }

                _logPaneServices.EventAggregator.PublishOnCurrentThreadAsync(RefreshEvent.Full);
            }
        }

        public string Name => _sourceFilter.Name;

        public BindableCollection<SourceFilterViewModel> Children { get; } = new();

        public SourceFilterViewModel Find(IReadOnlyList<string> loggerNames)
        {
            return Find(loggerNames, this, 0);
        }

        protected SourceFilterViewModel Find(IReadOnlyList<string> loggerNames, SourceFilterViewModel parent, int index)
        {
            if (loggerNames.Count <= index)
            {
                return null;
            }

            var name = loggerNames[index];
            var sourceFilterViewModel = parent.Children.FirstOrDefault(s => s.Name == name);

            if (sourceFilterViewModel == null)
            {
                var newSourceFilter = new SourceFilter(parent._sourceFilter)
                {
                    Name = name
                };
                parent._sourceFilter.Children.Add(newSourceFilter);

                sourceFilterViewModel = new SourceFilterViewModel(newSourceFilter, _logPaneServices);
                parent.Children.Add(sourceFilterViewModel);
            }

            var source = Find(loggerNames, sourceFilterViewModel, index + 1);
            return source ?? sourceFilterViewModel;
        }
    }
}
