using Common;
namespace TwitchSitePlugin
{
    internal class TwitchSiteOptions : DynamicOptionsBase, ITwitchSiteOptions
    {
        /// <summary>
        /// @コテハンを自動登録するか
        /// </summary>
        public bool NeedAutoSubNickname { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(NeedAutoSubNickname), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
        }
        internal TwitchSiteOptions Clone()
        {
            return (TwitchSiteOptions)this.MemberwiseClone();
        }
        internal void Set(TwitchSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
    }
}
