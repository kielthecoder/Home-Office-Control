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
        public ControlSystem()
        {
            Thread.MaxNumberOfUserThreads = 50;
            CrestronEnvironment.ProgramStatusEventHandler += OnProgramStatusChange;

            if (CrestronDataStoreStatic.InitCrestronDataStore() != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
                CrestronConsole.PrintLine("Could not initialize CrestronDataStore?!");
        }

        public override void InitializeSystem()
        {
            var tsw = new Ts770(0x03, this); // TS-770 is the main interface

            tsw.ExtenderTouchDetectionReservedSigs.Use();
            tsw.ExtenderTouchDetectionReservedSigs.Time.UShortValue = 120; // 2 minutes
            tsw.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange += _tsw_TouchDetection;

            tsw.Register();
        }

        void OnProgramStatusChange(eProgramStatusEventType type)
        {
            if (type == eProgramStatusEventType.Stopping) // Clean things up before the program stops
            {

            }
        }

        void _tsw_TouchDetection(DeviceExtender ex, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                CrestronConsole.PrintLine("_tsw_TouchDetection: {0} is {1}", args.Sig.Name, args.Sig.BoolValue);
            }
        }
    }
}