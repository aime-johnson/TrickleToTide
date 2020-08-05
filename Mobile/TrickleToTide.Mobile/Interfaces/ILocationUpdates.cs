using System;
using System.Collections.Generic;
using System.Text;

namespace TrickleToTide.Mobile.Interfaces
{
    public interface ILocationUpdates
    {
        void StartGps();
        
        void StopGps();

        void Start();
        void Stop();
        bool IsRunning { get; }
        bool IsGpsConnected { get; }
        Guid Id { get; }
    }
}
