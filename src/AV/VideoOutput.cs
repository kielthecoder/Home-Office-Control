using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace HomeOfficeControl.AV
{
    public class VideoOutputEventArgs : EventArgs
    {
        public uint Input { get; private set; }
        public uint Output { get; private set; }

        public VideoOutputEventArgs(uint vidIn, uint vidOut)
        {
            Input = vidIn;
            Output = vidOut;
        }
    }

    public class VideoOutput
    {
        public event EventHandler<VideoOutputEventArgs> VideoInputChange;

        public uint Index
        {
            get;
            private set;
        }

        private uint _input;
        public uint VideoInput
        {
            get { return _input; }
            set { Take(value); }
        }

        public VideoOutput()
        {
            Index = 1; // Default to output 1
        }

        public VideoOutput(uint vidOut)
        {
            Index = vidOut;
        }

        public void Take(uint vidIn)
        {
            _input = vidIn;

            if (VideoInputChange != null)
                VideoInputChange(this, new VideoOutputEventArgs(_input, Index));
        }
    }
}