using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace DwaValidatorApp.Viewmodel
{
    public class SchemaMatchVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private SchemaMatchVM _parent;

        public ObservableCollection<SchemaMatchVM> Children { get; set; }

        public static SchemaMatchVM Create(string name, SchemaMatchVM parent = null)
        {
            var child = new SchemaMatchVM(name, parent)
            {
                Children = new ObservableCollection<SchemaMatchVM>()
            };

            parent.Children.Add(child);

            return child;
        }

        public static SchemaMatchVM Create(string name, IList<SchemaMatchVM> container = null)
        {
            var child = new SchemaMatchVM(name, null)
            {
                Children = new ObservableCollection<SchemaMatchVM>()
            };

            container.Add(child);

            return child;
        }

        public SchemaMatchVM(string name, SchemaMatchVM parent)
        {
            _name = name;
            _parent = parent;
        }

        private string _name;

        public string Name 
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public SchemaMatchVM AddChild(string name)
        {
            var newChild = new SchemaMatchVM(name, this);
            Children.Add(newChild);
            
            return newChild;
        }
    }
}
