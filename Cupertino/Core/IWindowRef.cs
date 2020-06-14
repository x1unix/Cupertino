using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Cupertino.Core
{
    public interface IWindowRef
    {
        public int ProcessID { get; }
        public string ExecutablePath { get; }
        public string Title { get; }
        public string ApplicationName { get; }

        public MenuRef Menu { get; }

        public class MenuItemRef
        {
            public int Ref { get; set; }
            public uint Index { get; set; }
            public bool IsSeparator { get; set; }
            public string Label { get; set; }
            public MenuRef SubMenu { get; set; }
        }

        public class MenuRef
        {
            public IntPtr Ref { get; set; }

            public List<MenuItemRef> Items { get; set; }


        }

    }
}
