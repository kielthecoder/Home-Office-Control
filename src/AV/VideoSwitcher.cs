using System;
using System.Collections.Generic;
using Crestron.SimplSharp;

namespace HomeOfficeControl.AV
{
    public delegate void VideoSwitcherPollHandler();

    public class VideoSwitcher : IDisposable
    {
        public VideoSwitcherPollHandler Poll;
        public event EventHandler<VideoOutputEventArgs> VideoOutputFeedback;

        public uint InputCount { get; private set; }
        public uint OutputCount { get; private set; }

        public Dictionary<uint, VideoOutput> Outputs { get; private set; }

        private CTimer _timer;

        public VideoSwitcher(uint ins, uint outs)
        {
            InputCount = ins;
            OutputCount = outs;

            Outputs = new Dictionary<uint, VideoOutput>();

            for (uint i = 1; i <= OutputCount; i++)
            {
                Outputs[i] = new VideoOutput(i);
            }

            _timer = new CTimer(obj =>
            {
                if (Poll != null)
                    Poll();
            }, Timeout.Infinite);
        }

        public void Dispose()
        {
            if (!_timer.Disposed)
                _timer.Dispose();
        }

        public void EnableDevicePolling(long period)
        {
            if (!_timer.Disposed)
                _timer.Reset(0, period);
        }

        public void EnableDevicePolling()
        {
            EnableDevicePolling(30000); // every 30s is good
        }

        public void DisableDevicePolling()
        {
            if (!_timer.Disposed)
                _timer.Stop();
        }

        protected void ReportVideoFeedback(uint vidIn, uint vidOut)
        {
            if (VideoOutputFeedback != null)
                VideoOutputFeedback(this, new VideoOutputEventArgs(vidIn, vidOut));
        }
    }
}