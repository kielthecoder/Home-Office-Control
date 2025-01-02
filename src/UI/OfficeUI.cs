using System;

using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace HomeOfficeControl.UI
{
    public enum Activity
    {
        None,
        Presets,
        Switching,
        Config,
        Clock
    }

    public class OfficeUI : IDisposable
    {
        private CTimer _textFade;

        public BasicTriListWithSmartObject Panel { get; private set; }

        public OfficeUI(BasicTriListWithSmartObject tsw, string sgd)
        {
            Panel = tsw;

            // Standard event handlers
            Panel.OnlineStatusChange += PanelOnlineStatusChange;
            Panel.SigChange += PanelSigChange;

            try
            {
                // Load SmartGraphics definitions
                Panel.LoadSmartObjects(Path.Combine(Directory.GetApplicationDirectory(), sgd));
                Panel.SmartObjects[1].SigChange += PanelActivityChange;
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("Exception in OfficeUI: {0}", e.Message);
            }

            _textFade = new CTimer(obj => Panel.BooleanInput[45].BoolValue = false, 500); // fade out help text after 0.5s
        }

        public void Dispose()
        {
            if (!_textFade.Disposed)
                _textFade.Dispose();
        }

        public void SelectActivity(Activity act)
        {
            switch (act)
            {
                case Activity.Presets:
                    Panel.StringInput[45].StringValue = "P R E S E T S";
                    break;
                case Activity.Switching:
                    Panel.StringInput[45].StringValue = "S W I T C H I N G";
                    break;
                case Activity.Config:
                    Panel.StringInput[45].StringValue = "C O N F I G";
                    break;
                case Activity.Clock:
                    Panel.StringInput[45].StringValue = "C L O C K";
                    break;
            }

            Panel.BooleanInput[45].BoolValue = true; // show help text
            _textFade.Reset(500); // 0.5s before fading away

            var isPresets = (act == Activity.Presets);
            Panel.BooleanInput[41].BoolValue = isPresets;
            Panel.SmartObjects[1].BooleanInput[11].BoolValue = isPresets;

            var isSwitching = (act == Activity.Switching);
            Panel.BooleanInput[42].BoolValue = isSwitching;
            Panel.SmartObjects[1].BooleanInput[12].BoolValue = isSwitching;

            var isConfig = (act == Activity.Config);
            Panel.BooleanInput[43].BoolValue = isConfig;
            Panel.SmartObjects[1].BooleanInput[13].BoolValue = isConfig;

            var isClock = (act == Activity.Clock);
            Panel.BooleanInput[44].BoolValue = isClock;
            Panel.SmartObjects[1].BooleanInput[14].BoolValue = isClock;
        }

        void PanelOnlineStatusChange(GenericBase dev, OnlineOfflineEventArgs args)
        {

        }

        void PanelSigChange(BasicTriList dev, SigEventArgs args)
        {

        }

        void PanelActivityChange(GenericBase dev, SmartObjectEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                if (args.Sig.BoolValue) // press
                {
                    switch (args.Sig.Name)
                    {
                        case "Item 1 Pressed":
                            SelectActivity(Activity.Presets);
                            break;
                        case "Item 2 Pressed":
                            SelectActivity(Activity.Switching);
                            break;
                        case "Item 3 Pressed":
                            SelectActivity(Activity.Config);
                            break;
                        case "Item 4 Pressed":
                            SelectActivity(Activity.Clock);
                            break;
                    }
                }
            }
        }
    }
}