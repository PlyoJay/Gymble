using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gymble.Models
{
    public class NavigationItemModel
    {
        public PackIconKind IconKind { get; set; } = PackIconKind.MonitorDashboard;
        public string Label { get; set; } = string.Empty;
        public string TagName { get; set; } = string.Empty;
        public ICommand Command { get; set; } = null!;
    }
}
