using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HomeOfficeControl.AV
{
    public class ExtronUsbSwitcher
    {
        private ComPort.ComPortSpec _spec;
        private ComPort _port;

        public ExtronUsbSwitcher()
        {
            _spec = new ComPort.ComPortSpec()
            {
                BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600,
                DataBits = ComPort.eComDataBits.ComspecDataBits8,
                Parity = ComPort.eComParityType.ComspecParityNone,
                StopBits = ComPort.eComStopBits.ComspecStopBits1,
                Protocol = ComPort.eComProtocolType.ComspecProtocolRS232
            };
        }

        public void UseSerial(ComPort port)
        {
            _port = port;

            if (_port.Register() == eDeviceRegistrationUnRegistrationResponse.Success)
            {
                _port.SetComPortSpec(_spec);
                _port.SerialDataReceived += ExtronDataReceived;
            }
            else
            {
                CrestronConsole.PrintLine("ExtronUsbSwitcher: Failed to register COM port");
            }
        }

        private void ExtronDataReceived(ComPort port, ComPortSerialDataEventArgs args)
        {
            CrestronConsole.PrintLine("ExtronUsbSwitcher rx: {0}", args.SerialData);
        }

        public void Take(uint host)
        {
            try
            {
                _port.Send(String.Format("{0}!", host));
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception in ExtronUsbSwitcherSend: {0}", e.Message);
            }
        }
    }
}