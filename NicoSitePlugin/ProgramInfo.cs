using System.Collections.Generic;

namespace NicoSitePlugin
{
    class ProgramInfo : IProgramInfo
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public bool IsMemberOnly { get; private set; }
        public long VposBaseAt { get; private set; }
        public long BeginAt { get; private set; }
        public long EndAt { get; private set; }
        public string Status { get; private set; }
        public List<string> Categories { get; private set; } = new List<string>();
        public List<Room> Rooms { get; private set; } = new List<Room>();
        public ProviderType Type { get; private set; }
        public string ProviderId { get; private set; }
        public string ProviderName { get; private set; }
        public ProgramInfo(Low.ProgramInfo.RootObject low)
        {
            Title = low.Data.Title;
            Description = low.Data.Description;
            IsMemberOnly = low.Data.IsMemberOnly;
            VposBaseAt = low.Data.VposBaseAt;
            BeginAt = low.Data.BeginAt;
            EndAt = low.Data.EndAt;
            Status = low.Data.Status;
            foreach (var category in low.Data.Categories)
            {
                Categories.Add(category);
            }
            foreach (var r in low.Data.Rooms)
            {
                Rooms.Add(new Room(r));
            }
        }
    }
}
