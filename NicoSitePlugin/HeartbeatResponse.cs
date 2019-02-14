namespace NicoSitePlugin
{
    class HeartbeatResponse
    {
        public bool Success => Heartbeat != null;
        public IHeartbeat Heartbeat { get; }
        public IHeartbeartFail Fail { get; }
        public HeartbeatResponse(IHeartbeat heartbeat)
        {
            Heartbeat = heartbeat;
        }
        public HeartbeatResponse(IHeartbeartFail fail)
        {
            Fail = fail;
        }
    }
}
