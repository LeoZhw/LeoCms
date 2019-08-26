using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Leo.Microservice.Host
{
    public interface IApplicationLifetime
    {
        CancellationToken ApplicationStarted { get; }

        CancellationToken ApplicationStopping { get; }

        CancellationToken ApplicationStopped { get; }


        void StopApplication();

        void NotifyStopped();

        void NotifyStarted();
    }
}
