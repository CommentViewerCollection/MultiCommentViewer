using System;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    public interface ISplitBuffer
    {
        event EventHandler<List<string>> Added;
        void Add(byte[] data, int start, int length);
    }
}
