namespace Logazmic.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using Caliburn.Micro;

    using Logazmic.Services;

    public class LogSource : PropertyChangedBase
    {
        private bool isChecked;

        private string fullName;

        public LogSource(LogSource parent = null)
        {
            fullName = null;
            Parent = parent;
            Children = new BindableCollection<LogSource>();
            isChecked = true;
        }

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

                Messaging.Publish(new RefreshCheckEvent());
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

        public void Find(IReadOnlyList<string> loggerNames)
        {
            Find(loggerNames, this, 0);
        }

        protected void Find(IReadOnlyList<string> loggerNames, LogSource parent, int index)
        {
            if (loggerNames.Count <= index)
            {
                return;
            }

            var name = loggerNames[index];
            var logSource = parent.Children.FirstOrDefault(s => s.Name == name);

            if (logSource == null)
            {
                logSource = new LogSource(parent)
                {
                    Name = name,
                };

                parent.Children.Add(logSource);
            }

            Find(loggerNames, logSource, index + 1);
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