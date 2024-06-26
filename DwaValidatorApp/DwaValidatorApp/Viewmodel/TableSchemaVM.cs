using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Viewmodel
{
    public enum EntityType 
    {
        Unknown = 0,
        Primary = 1,
        Nto1 = 2,
        MtoN = 3,
        BridgeMtoN = 4,
        User = 5,
        BridgeUser = 6,
        Document = 7,
        Image = 8,
        Role = 9,
        UserRole = 10,
    }

    public class TableSchemaVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public IEnumerable<EntityType> AllEntityTypes { get; set; } = Enum.GetValues<EntityType>();

        public EntityType EntityType { get; set; }

        public string TableName { get; set; }

        private int _count;
        public int Count 
        {
            get
            {
                return _count;
            }

            set
            {
                _count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public static EntityType AsEntityType(string comment)
            => comment switch
            {
                var s when s == null => EntityType.Unknown,
                var s when s.Contains("Primary") => EntityType.Primary,
                var s when s.Contains("1-to-N") => EntityType.Nto1,
                var s when s.Contains("M-to-N-bridge") => EntityType.BridgeMtoN,
                var s when s.Contains("M-to-N") => EntityType.MtoN,
                var s when s.Contains("User-M-to-N-bridge") => EntityType.BridgeUser,
                var s when s.Contains("UserRole") => EntityType.UserRole,
                var s when s.Contains("User") => EntityType.User,
                var s when s.Contains("Image") => EntityType.Image,
                _ => EntityType.Unknown
            };
    }
}
