using System;

using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

using HomeOfficeControl.AV;

namespace HomeOfficeControl.UI
{
    public class VideoSwitcherControl
    {
        private OfficeUI _ui;
        private uint[] _joins;

        public VideoSwitcher Switcher { get; private set; }

        public VideoSwitcherControl(OfficeUI ui, VideoSwitcher sw, uint[] joins)
        {
            _ui = ui;
            _joins = joins;

            _ui.Panel.SigChange += PanelSigChange;

            Switcher = sw;
            Switcher.VideoOutputFeedback += SwitcherVideoOutputFeedback;
        }

        void PanelSigChange(BasicTriList dev, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                if (args.Sig.BoolValue) // press
                {
                    for (uint i = 0; i < _joins.Length; i++)
                    {
                        if (args.Sig.Number == _joins[i]) // one of the joins we're listening for?
                        {
                            Switcher.Outputs[1].VideoInput = i;
                        }
                    }
                }
            }
        }

        void SwitcherVideoOutputFeedback(object sender, VideoOutputEventArgs args)
        {
            for (uint i = 0; i < _joins.Length; i++)
            {
                _ui.Panel.BooleanInput[_joins[i]].BoolValue = (i == args.Input);
            }
        }
    }
}