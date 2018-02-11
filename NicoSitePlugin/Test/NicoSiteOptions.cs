using System;
using Common;
namespace NicoSitePlugin.Test
{
    internal class NicoSiteOptions : DynamicOptionsBase, INicoSiteOptions
    {
        public int OfficialRoomsRetrieveCount { get => GetValue(); set => SetValue(value); }
        public bool CanUploadPlayerStatus { get => GetValue(); set => SetValue(value); }

        protected override void Init()
        {
            Dict.Add(nameof(OfficialRoomsRetrieveCount), new Item { DefaultValue = 3, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
            Dict.Add(nameof(CanUploadPlayerStatus), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });

        }
        internal NicoSiteOptions Clone()
        {
            return (NicoSiteOptions)this.MemberwiseClone();
        }
        internal void Set(NicoSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
    }
}
