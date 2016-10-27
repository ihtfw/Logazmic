namespace Logazmic.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using Caliburn.Micro;

    using Logazmic.Services;

    public class LogSource : PropertyChangedBase
    {
        private readonly LogPaneServices logPaneServices;

        private bool isChecked;

        private string fullName;

        public LogSource(LogPaneServices logPaneServices, LogSource parent = null)
        {
            this.logPaneServices = logPaneServices;
            fullName = null;
            Parent = parent;
            Children = new BindableCollection<LogSource>();
            isChecked = true;
        }

        public bool IsSelected { get; set; }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                foreach (var child in Children)
                {
                    child.IsChecked = value;
                }
                isChecked = value;

                logPaneServices.EventAggregator.PublishOnCurrentThread(new RefreshCheckEvent());
            }
        }

        public string Name { get; set; }

        public string FullName
        {
            get
            {
                if (fullName == null)
                {
                    if (Parent != null)
                    {
                        if (string.IsNullOrEmpty(Parent.FullName))
                        {
                            fullName = Name;
                        }
                        else
                        {
                            fullName += Parent.FullName + "." + Name;    
                        }
                    }
                    else
                    {
                        if (Name == "Root")
                        {
                            fullName = string.Empty;
                        }
                        else
                        {
                            fullName = Name;    
                        }
                    }
                }
                return fullName;
            }
        }

        public LogSource Parent { get; private set; }

        public BindableCollection<LogSource> Children { get; private set; }

        public IEnumerable<string> Checked
        {
            get
            {
                if (IsChecked)
                {
                    yield return Name;
                }
                foreach (var logSource in Children)
                {
                    foreach (var str in logSource.Checked)
                    {
                        yield return str;
                    }
                }
            }
        }

        public LogSource Find(IReadOnlyList<string> loggerNames)
        {
            return Find(loggerNames, this, 0);
        }

        protected LogSource Find(IReadOnlyList<string> loggerNames, LogSource parent, int index)
        {
            if (loggerNames.Count <= index)
            {
                return null;
            }

            var name = loggerNames[index];
            var logSource = parent.Children.FirstOrDefault(s => s.Name == name);

            if (logSource == null)
            {
                logSource = new LogSource(logPaneServices, parent)
                {
                    Name = name,
                };

                parent.Children.Add(logSource);
            }

            var source = Find(loggerNames, logSource, index + 1);
            return source ?? logSource;
        }

        public IEnumerable<LogSource> Leaves()
        {
            foreach (var logSource in Children)
            {
                foreach (var leaf in logSource.Leaves())
                {
                    yield return leaf;
                }
            }

            if (!Children.Any())
                yield return this;
        }
    }
}