using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace HomeOfficeControl.UI
{
    public delegate void PresetHandler(uint index);

    public class UserPresets : IDisposable
    {
        private OfficeUI _ui;
        private uint[] _joins;

        private uint _active;
        private CTimer _held;

        public PresetHandler StorePreset;
        public PresetHandler RecallPreset;

        public UserPresets(OfficeUI ui, uint[] joins)
        {
            _ui = ui;
            _joins = joins;

            _ui.Panel.SigChange += PanelSigChange;

            _active = 0;
            _held = new CTimer(obj =>
                {
                    Store(_active);
                    _active = 0;
                }, Timeout.Infinite); // don't start right away!
        }

        public void Dispose()
        {
            if (!_held.Disposed)
                _held.Dispose();
        }

        void Store(uint index)
        {
            if (index > 0) // only allow 1 - n
            {
                _ui.Panel.BooleanInput[35].BoolValue = true; // show SAVED text
                var timer = new CTimer(obj =>
                    {
                        _ui.Panel.BooleanInput[35].BoolValue = false;
                    }, 1500);

                if (StorePreset != null)
                    StorePreset(index);
            }
        }

        void Recall(uint index)
        {
            if (index > 0) // only allow 1 - n
            {
                if (RecallPreset != null)
                    RecallPreset(index);
            }
        }

        void PanelSigChange(BasicTriList dev, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool)
            {
                for (uint i = 0; i < _joins.Length; i++)
                {
                    if (args.Sig.Number == _joins[i]) // one of the joins we're listening for?
                    {
                        if (args.Sig.BoolValue) // pressed
                        {
                            _active = i + 1; // start at preset 1 instead of 0
                            _held.Reset(2000); // wait 2s before trying to store preset
                        }
                        else // released
                        {
                            if (_active > 0) // released before storing?
                            {
                                Recall(_active);
                                _active = 0;
                            }

                            _held.Stop(); // don't try to store preset
                        }
                        break; // exit for loop
                    }
                }
            }
        }
    }
}