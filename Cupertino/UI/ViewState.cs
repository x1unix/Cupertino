using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;

namespace Cupertino.UI
{
    public class ViewState
    {
        public string ApplicationName { get; set; }

        public IntPtr LastWindowHandle { get; set; }
        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ViewState() { }

    }
}
