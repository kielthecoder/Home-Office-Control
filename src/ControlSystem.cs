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
using HomeOfficeControl.UI;

namespace HomeOfficeControl
{
    public class ControlSystem : CrestronControlSystem
    {
        private ExtronSw4Hd4k[] _switcher;
        private OfficeUI _ui;

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
            var exp = new CenIoCom102(0xA5, this); // CEN-IO-COM-102 is used for HDMI switchers
            exp.OnlineStatusChange += _exp_OnlineStatusChange;
            exp.Register();

            _switcher[0] = new ExtronSw4Hd4k(); // Switcher for Primary monitor
            _switcher[0].UseSerial(exp.ComPorts[1]);

            _switcher[1] = new ExtronSw4Hd4k(); // Switcher for Secondary monitor
            _switcher[1].UseSerial(exp.ComPorts[2]);

            var tsw = new Ts770(0x03, this); // TS-770 is the main interface

            tsw.ExtenderTouchDetectionReservedSigs.Use();
            tsw.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 120; // 2 minutes
            tsw.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += _tsw_TouchDetection;

            InitializeUI(tsw);
        }

        void InitializeUI(BasicTriListWithSmartObject tsw)
        {
            CrestronConsole.PrintLine("InitializeUI: {0}", tsw);

            _ui = new OfficeUI(tsw, "Home Office_TS-770_v1.sgd");

            var uiSw1 = new VideoSwitcherControl(_ui, _switcher[0], new uint[] { 50, 51, 52, 53, 54 }); // Primary monitor joins
            var uiSw2 = new VideoSwitcherControl(_ui, _switcher[1], new uint[] { 60, 61, 62, 63, 64 }); // Secondary monitor joins

            tsw.Register();
        }

        void OnProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping) // Clean things up before the program stops
            {
                _ui.Dispose();

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
                        _ui.SelectActivity(Activity.Clock);
                    }
                }
            }
        }

        void _exp_OnlineStatusChange(GenericBase dev, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                // Default to Clock activity
                _ui.SelectActivity(Activity.Clock);
            }
        }
    }
}