using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace HomeOfficeControl.AV
{
    public class ExtronVideoSwitcher : VideoSwitcher
    {
        private ComPort.ComPortSpec _spec;
        private ComPort _port;

        public ExtronVideoSwitcher(uint ins, uint outs) : base(ins, outs)
        {
            _spec = new ComPort.ComPortSpec()
            {
                BaudRate = ComPort.eComBaudRates.ComspecBaudRate9600,
                DataBits = ComPort.eComDataBits.ComspecDataBits8,
                Parity = ComPort.eComParityType.ComspecParityNone,
                StopBits = ComPort.eComStopBits.ComspecStopBits1,
                Protocol = ComPort.eComProtocolType.ComspecProtocolRS232
            };

            Poll += ExtronPoll;

            for (uint i = 1; i <= outs; i++)
            {
                Outputs[i].VideoInputChange += ExtronVideoInputChange;
            }
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
                CrestronConsole.PrintLine("ExtronVideoSwitcher: Failed to register COM port");
            }
        }

        void ExtronPoll()
        {
            if (_port != null)
                _port.Send("I");
        }

        void ExtronDataReceived(ComPort port, ComPortSerialDataEventArgs args)
        {
            var words = args.SerialData.Trim().Split(' ');

            if (words[0].StartsWith("In")) // input change
            {
                try
                {
                    var input = uint.Parse(words[0].Substring(2));

                    if (words[1] == "All")
                    {
                        for (uint output = 1; output <= OutputCount; output++)
                        {
                            ReportVideoFeedback(input, output);
                        }
                    }
                }
                catch (Exception e)
                {
                    CrestronConsole.PrintLine("Exception in ExtronDataReceived: {0}", e.Message);
                }
            }
            else
            {
                CrestronConsole.PrintLine("ExtronDataReceived: {0}", args.SerialData);
            }
        }

        void ExtronVideoInputChange(object output, VideoOutputEventArgs args)
        {
            if (OutputCount == 1)
            {
                _port.Send(String.Format("{0}!", args.Input));
            }
            else
            {
                _port.Send(String.Format("{0}*{1}!", args.Input, args.Output));
            }
        }
    }
}