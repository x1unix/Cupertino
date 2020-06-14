using Cupertino.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Cupertino.UI
{
    public class MenuItemViewModel
    {
        public delegate void CommandHandler(IWindowRef.MenuItemRef mRef);
        private readonly ICommand _command;

        public MenuItemViewModel(IWindowRef.MenuItemRef mRef, CommandHandler cmdHandler)
        {
            IsSeparator = mRef.IsSeparator;
            if (IsSeparator)
            {
                return;
            }

            _command = new CommandViewModel(cmdHandler, mRef);
            Header = mRef.Label.Replace('&', '_');  // Replace '&' MFC menu mnemonic char with wpf's '_'
            MenuItems = FromMenuRef(mRef.SubMenu, cmdHandler, false);
        }

        public bool IsRoot { get; set; }

        public bool IsSeparator { get; set; }
        public string Header { get; set; }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; set; }

        public ICommand Command
        {
            get
            {
                return _command;
            }
        }

        private void Execute()
        {
            // (NOTE: In a view model, you normally should not use MessageBox.Show()).
            MessageBox.Show("Clicked at " + Header);
        }

        public static ObservableCollection<MenuItemViewModel> FromMenuRef(IWindowRef.MenuRef mRef, CommandHandler handle, bool topMost = true)
        {
            if (mRef?.Items is null) return null;
            return new ObservableCollection<MenuItemViewModel>(mRef.Items.Select(r => new MenuItemViewModel(r, handle) { IsRoot = topMost}));
        }
    }

    public class CommandViewModel : ICommand
    {
        private readonly MenuItemViewModel.CommandHandler _action;
        private readonly IWindowRef.MenuItemRef _item;

        public CommandViewModel(MenuItemViewModel.CommandHandler action, IWindowRef.MenuItemRef item)
        {
            _action = action;
            _item = item;
        }

        public void Execute(object o)
        {
            _action(_item);
        }

        public bool CanExecute(object o)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
