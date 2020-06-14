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
        private readonly ICommand _command;

        public MenuItemViewModel()
        {
            _command = new CommandViewModel(Execute);
        }

        public MenuItemViewModel(IWindowRef.MenuItemRef item) : this()
        {
            // Replace '&' MFC menu mnemonic char with wpf's '_'
            if (item.IsSeparator)
            {
                IsSeparator = true;
                return;
            }
            Header = item.Label.Replace('&', '_');
            MenuItems = FromMenuRef(item.SubMenu, false);
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

        public static ObservableCollection<MenuItemViewModel> FromMenuRef(IWindowRef.MenuRef mRef, Boolean topMost = true)
        {
            if (mRef?.Items is null) return null;
            return new ObservableCollection<MenuItemViewModel>(mRef.Items.Select(r => new MenuItemViewModel(r) { IsRoot = topMost}));
        }
    }

    public class CommandViewModel : ICommand
    {
        private readonly Action _action;

        public CommandViewModel(Action action)
        {
            _action = action;
        }

        public void Execute(object o)
        {
            _action();
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
