using Mcv.PluginV2;

namespace Mcv.Core
{
    class McvCoreOptions : DynamicOptionsBase, IMcvCoreOptions
    {
        public string PluginDir { get => GetValue(); set => SetValue(value); }
        public string SettingsDirPath { get => GetValue(); set => SetValue(value); }

        protected override void Init()
        {
            Dict.Add(nameof(PluginDir), new Item { DefaultValue = "plugins", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
            Dict.Add(nameof(SettingsDirPath), new Item { DefaultValue = "settings", Predicate = c => true, Serializer = c => c, Deserializer = s => s });
        }
    }

}
