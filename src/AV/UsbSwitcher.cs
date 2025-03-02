using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HomeOfficeControl.AV
{
    public class UsbSwitcher
    {
        public virtual void UseSerial(ComPort port)
        {
        }

        public virtual void Take(uint host)
        {
        }
    }
}