using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Cupertino.Platform.Win32
{
    class Handle: IDisposable
    {
        private IntPtr HandlePtr;
        public Handle(IntPtr handlePtr)
        {
            HandlePtr = handlePtr;
        }

        public void Dispose()
        {
            Kernel32.CloseHandle(HandlePtr);
            Debug.WriteLine("Handle {0:x8} closed", HandlePtr);
        }

        public static implicit operator IntPtr(Handle h)
        {
            return h.HandlePtr;
        }

        public static implicit operator Handle(IntPtr ptr)
        {
            return new Handle(ptr);
        }

        public static Handle FromPtr(IntPtr ptr)
        {
            return new Handle(ptr);
        }
    }
}
