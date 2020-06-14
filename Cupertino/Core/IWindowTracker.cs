using System;
using System.Collections.Generic;
using System.Text;

namespace Cupertino.Core
{
    public interface IWindowTracker : IDisposable
    {
        public delegate void WindowSelectedHandler(IWindowRef window);

        public event WindowSelectedHandler WindowSelected;

        public void Attatch() { }
    }
}
