namespace NicoSitePlugin.Test
{
    public class MsTest : IMs
    {
        public string Addr { get; set; }
        public string Thread { get; set; }
        public int Port { get; set; }
        public MsTest() { }
        public MsTest(string addr, string thread, int port)
        {
            Addr = addr;
            Thread = thread;
            Port = port;
        }
        public MsTest(string addr, int port, string thread)
        {
            Addr = addr;
            Thread = thread;
            Port = port;
        }
    }
}
