using System;

using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;

using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.GeneralIO;
using Crestron.SimplSharpPro.UI;

namespace HomeOfficeControl
{
    public class ControlSystem : CrestronControlSystem
    {
        private AV.ExtronVideoSwitcher[] _switcher;
        private AV.UsbSwitcher _usb;
        private UI.OfficeUI _ui;
        private UI.UserPresets _presets;

        public ControlSystem()
        {
            Thread.MaxNumberOfUserThreads = 50;
            CrestronEnvironment.ProgramStatusEventHandler += OnProgramStatusChange;

            if (CrestronDataStoreStatic.InitCrestronDataStore() != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                CrestronConsole.PrintLine("Could not initialize CrestronDataStore?!");

            _switcher = new AV.ExtronSw4Hd4k[2];
            _usb = new AV.AtenUsbSwitcher();
        }

        public override void InitializeSystem()
        {
            var exp = new CenIoCom102(0xA5, this); // CEN-IO-COM-102 is used for HDMI switchers
            exp.OnlineStatusChange += _exp_OnlineStatusChange;
            exp.Register();

            _switcher[0] = new AV.ExtronSw4Hd4k(); // switcher for Primary monitor
            _switcher[0].UseSerial(exp.ComPorts[1]);

            _switcher[1] = new AV.ExtronSw4Hd4k(); // switcher for Secondary monitor
            _switcher[1].UseSerial(exp.ComPorts[2]);

            if (this.SupportsComPort)
            {
                CrestronConsole.PrintLine("Attaching USB switcher to COM port 1");
                _usb.UseSerial(this.ComPorts[1]); // USB switcher
            }

            var tsw = new Ts770(0x03, this); // TS-770 is the main interface

            tsw.ExtenderTouchDetectionReservedSigs.Use();
            tsw.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 120; // 2 minutes
            tsw.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += _tsw_TouchDetection;

            InitializeUI(tsw);
        }

        void InitializeUI(BasicTriListWithSmartObject tsw)
        {
            CrestronConsole.PrintLine("InitializeUI: {0}", tsw);

            _ui = new UI.OfficeUI(tsw, "Home Office_TS-770_v1.sgd");

            var sw1 = new UI.VideoSwitcherControl(_ui, _switcher[0], new uint[] { 50, 51, 52, 53, 54 }); // primary monitor joins
            var sw2 = new UI.VideoSwitcherControl(_ui, _switcher[1], new uint[] { 60, 61, 62, 63, 64 }); // secondary monitor joins

            _presets = new UI.UserPresets(_ui, new uint[] { 31, 32, 33, 34 }); // 4 programmable presets
            _presets.StorePreset += StoreUserPreset;
            _presets.RecallPreset += RecallUserPreset;

            tsw.Register();
        }

        void OnProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping) // clean things up before the program stops
            {
                _presets.Dispose();
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
                if (args.Sig.Number == 29726) // touch activity
                {
                    if (args.Sig.BoolValue == false) // no activity detected!
                    {
                        _ui.SelectActivity(UI.Activity.Clock);
                    }
                }
            }
        }

        void _exp_OnlineStatusChange(GenericBase dev, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                // Default to Clock activity
                _ui.SelectActivity(UI.Activity.Clock);
            }
        }

        void StoreUserPreset(uint number)
        {
            string key;

            CrestronConsole.PrintLine("Storing preset {0}", number);

            for (uint i = 0; i < _switcher.Length; i++)
            {
                key = String.Format("preset{0}-sw{1}-output1", number, i);
                if (CrestronDataStoreStatic.SetLocalUintValue(key, _switcher[i].Outputs[1].VideoInput) != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                {
                    CrestronConsole.PrintLine("Error saving {0}!", key);
                }
            }
        }

        void RecallUserPreset(uint number)
        {
            string key;
            uint value;

            CrestronConsole.PrintLine("Recalling preset {0}", number);

            for (uint i = 0; i < _switcher.Length; i++)
            {
                key = String.Format("preset{0}-sw{1}-output1", number, i);
                if (CrestronDataStoreStatic.GetLocalUintValue(key, out value) == CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                {
                    _switcher[i].Outputs[1].VideoInput = value;
                }
                else
                {
                    CrestronConsole.PrintLine("Error recalling {0}!", key);
                }
            }

            CrestronConsole.PrintLine("Switching to USB host: {0}", number);
            _usb.Take(number);
        }
    }
}