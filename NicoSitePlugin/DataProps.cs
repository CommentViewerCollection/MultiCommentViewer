using Newtonsoft.Json;

namespace NicoSitePlugin
{
    class DataProps
    {
        public string Title
        {
            get
            {
                return (string)_d.program.title;
            }
        }
        public string ProviderType
        {
            get
            {
                return (string)_d.program.providerType;
            }
        }
        public long OpenTime
        {
            get
            {
                return (long)_d.program.openTime;
            }
        }
        public long BeginTime
        {
            get
            {
                return (long)_d.program.beginTime;
            }
        }
        public long VposBaseTime
        {
            get
            {
                return (long)_d.program.vposBaseTime;
            }
        }
        public long EndTime
        {
            get
            {
                return (long)_d.program.endTime;
            }
        }
        public long ScheduledEndTime
        {
            get
            {
                return (long)_d.program.scheduledEndTime;
            }
        }
        public string Status
        {
            get
            {
                return (string)_d.program.status;
            }
        }
        public string WebsocketUrl
        {
            get
            {
                return (string)_d.site.relive.webSocketUrl;
            }
        }
        public string UserId
        {
            get
            {
                return (string)_d.user.id;
            }
        }
        public bool IsLoggedIn
        {
            get
            {
                return (bool)_d.user.isLoggedIn;
            }
        }
        public DataProps(string json)
        {
            Raw = json;
            _d = JsonConvert.DeserializeObject(json);
        }
        readonly dynamic _d;
        public string Raw { get; }
    }
}
