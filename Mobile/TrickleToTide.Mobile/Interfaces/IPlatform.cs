using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TrickleToTide.Mobile.Interfaces
{
    /// <summary>
    /// Some platform specific stuff.
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// API Key for TTT API
        /// </summary>
        string ApiKey { get; }
        string ApiEndpoint { get; }
        string LogDirectory { get; }
    }
}
