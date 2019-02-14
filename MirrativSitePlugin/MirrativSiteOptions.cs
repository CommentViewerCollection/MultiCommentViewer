using Common;

namespace MirrativSitePlugin
{
    internal class MirrativSiteOptions : DynamicOptionsBase, IMirrativSiteOptions
    {
        /// <summary>
        /// @コテハンを自動登録するか
        /// </summary>
        public bool NeedAutoSubNickname { get => GetValue(); set => SetValue(value); }
        /// <summary>
        /// 配信のメタデータを取得する間隔（秒）
        /// </summary>
        public int PollingIntervalSec { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(NeedAutoSubNickname), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(PollingIntervalSec), new Item { DefaultValue = 1, Predicate = n => n > 0, Serializer = n => n.ToString(), Deserializer = s => int.Parse(s) });
        }
        internal MirrativSiteOptions Clone()
        {
            return (MirrativSiteOptions)this.MemberwiseClone();
        }
        internal void Set(MirrativSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
    }
}
