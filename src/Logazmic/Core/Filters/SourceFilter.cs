using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Logazmic.Core.Filters
{
    public class SourceFilter
    {
        private string fullName;

        public SourceFilter(SourceFilter parent = null)
        {
            fullName = null;
            Parent = parent;
            Children = new List<SourceFilter>();
            IsChecked = true;
        }
        
        public bool IsChecked { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
        public SourceFilter Parent { get; private set; }

        public List<SourceFilter> Children { get; private set; }

        [JsonIgnore]
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

        public IEnumerable<SourceFilter> Leaves()
        {
            foreach (var logSource in Children)
            {
                foreach (var leaf in logSource.Leaves())
                {
                    yield return leaf;
                }
            }

            yield return this;
        }


        public SourceFilter Clone()
        {
            var parentClone = new SourceFilter
            {
                Name = Name,
                IsChecked = IsChecked
            };

            foreach (var child in Children)
            {
                var childClone = child.Clone();
                childClone.Parent = parentClone;
                parentClone.Children.Add(childClone);
            }

            return parentClone;
        }
    }
}