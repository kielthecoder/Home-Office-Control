using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HomeOfficeControl.AV
{
    public class AtenUsbSwitcher : UsbSwitcher
    {
        private ComPort.ComPortSpec _spec;
        private ComPort _port;

        public AtenUsbSwitcher()
        {
            _spec = new ComPort.ComPortSpec()
            {
                BaudRate = ComPort.eComBaudRates.ComspecBaudRate38400,
                DataBits = ComPort.eComDataBits.ComspecDataBits8,
                Parity = ComPort.eComParityType.ComspecParityNone,
                StopBits = ComPort.eComStopBits.ComspecStopBits1,
                Protocol = ComPort.eComProtocolType.ComspecProtocolRS485
            };
        }

        public override void UseSerial(ComPort port)
        {
            _port = port;

            if (_port.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
            {
                if (_port.Supports485)
                {
                    _port.SetComPortSpec(_spec);
                    _port.SerialDataReceived += AtenDataReceived;
                }
                else
                {
                    CrestronConsole.PrintLine("AtenUsbSwitcher: {0} doesn't support RS-485!", port);
                }
            }
            else
            {
                CrestronConsole.PrintLine("AtenUsbSwitcher: Failed to register COM port");
            }
        }

        private void AtenDataReceived(ComPort port, ComPortSerialDataEventArgs args)
        {
            CrestronConsole.PrintLine("AtenUsbSwitcher rx: {0}", args.SerialData);
        }

        public override void Take(uint host)
        {
            try
            {
                _port.Send(String.Format("sw p0{0}\n", host));
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception in AtenUsbSwitcherSend: {0}", e.Message);
            }
        }
    }
}