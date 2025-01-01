using System;

using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;

using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharpPro.UI;

using HomeOfficeControl.AV;

namespace HomeOfficeControl
{
    public class ControlSystem : CrestronControlSystem
    {
        private ExtronSw4Hd4k[] _switcher;

        public ControlSystem()
        {
            Thread.MaxNumberOfUserThreads = 50;
            CrestronEnvironment.ProgramStatusEventHandler += OnProgramStatusChange;

            if (CrestronDataStoreStatic.InitCrestronDataStore() != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                CrestronConsole.PrintLine("Could not initialize CrestronDataStore?!");

            _switcher = new ExtronSw4Hd4k[2];
        }

        public override void InitializeSystem()
        {
            var tsw = new Ts770(0x03, this); // TS-770 is the main interface

            tsw.ExtenderTouchDetectionReservedSigs.Use();
            tsw.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 120; // 2 minutes
            tsw.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += _tsw_TouchDetection;

            tsw.Register();

            var exp = new CenIoCom102(0xA5, this); // CEN-IO-COM-102 is used for HDMI switchers
            exp.OnlineStatusChange += _exp_OnlineStatusChange;

            exp.Register();

            _switcher[0] = new ExtronSw4Hd4k(); // Switcher for Primary monitor
            _switcher[0].UseSerial(exp.ComPorts[1]);

            _switcher[1] = new ExtronSw4Hd4k(); // Switcher for Secondary monitor
            _switcher[1].UseSerial(exp.ComPorts[2]);
        }

        void OnProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping) // Clean things up before the program stops
            {
                _switcher[0].Dispose();
                _switcher[1].Dispose();

                CrestronDataStoreStatic.Flush();
            }
        }

        void _tsw_TouchDetection(DeviceExtender ex, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                if (args.Sig.Number == 29726) // Activity
                {
                    if (args.Sig.BoolValue == false) // No activity detected
                    {
                        // TODO: go back to clock activity
                    }
                }
            }
        }

        void _exp_OnlineStatusChange(GenericBase dev, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                // TODO: re-enable polling?
            }
            else
            {
                // TODO: pause polling?
            }
        }
    }
}