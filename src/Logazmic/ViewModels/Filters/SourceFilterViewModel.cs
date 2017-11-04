using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Logazmic.Core.Filters;
using Logazmic.ViewModels.Events;

namespace Logazmic.ViewModels.Filters
{
    public class SourceFilterViewModel : PropertyChangedBase
    {
        private readonly SourceFilter sourceFilter;
        private readonly LogPaneServices logPaneServices;

        public SourceFilterViewModel(SourceFilter sourceFilter, LogPaneServices logPaneServices)
        {
            this.sourceFilter = sourceFilter;
            this.logPaneServices = logPaneServices;
        }

        public bool IsSelected { get; set; }

        public bool IsChecked
        {
            get { return sourceFilter.IsChecked; }
            set
            {
                sourceFilter.IsChecked = value;
                foreach (var child in Children)
                {
                    child.IsChecked = value;
                }

                logPaneServices.EventAggregator.PublishOnCurrentThread(RefreshEvent.Full);
            }
        }

        public string Name
        {
            get { return sourceFilter.Name; }
        }
        
        public BindableCollection<SourceFilterViewModel> Children { get; } = new BindableCollection<SourceFilterViewModel>();

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
                var newSourceFilter = new SourceFilter(parent.sourceFilter)
                {
                    Name = name
                };
                parent.sourceFilter.Children.Add(newSourceFilter);

                sourceFilterViewModel = new SourceFilterViewModel(newSourceFilter, logPaneServices);
                parent.Children.Add(sourceFilterViewModel);
            }

            var source = Find(loggerNames, sourceFilterViewModel, index + 1);
            return source ?? sourceFilterViewModel;
        }
    }
}
