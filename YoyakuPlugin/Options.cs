using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using System.Runtime.Serialization;

namespace OpenrecYoyakuPlugin
{
    public class DynamicOptions : DynamicOptionsBase
    {
        internal const string Default_Reserved_Se = "";
        internal const string Default_Reserved_Message = "$nameさんの予約うけつけましたー";

        internal const string Default_Delete_Se = "";
        internal const string Default_Delete_Message = "$nameさんの予約を取り消しました";

        internal const string Default_Call_Se = "se1";
        internal const string Default_Call_Message = "大変おまたせしました $name 指定の場所にお越し下さい";

        internal const string Default_AlreadyReserved_Se = "";
        internal const string Default_AlreadyReserved_Message = "$nameさんは既にご予約されております";

        internal const string Default_HandleNameNotSubscribed_Se = "";
        internal const string Default_HandleNameNotSubscribed_Message = "$no 先にコテハン登録をお願いいたします。@名前で登録できます";

        internal const string Default_NotReserved_Se = "";
        internal const string Default_NotReserved_Message = "$nameさんはご予約はされていないようです";

        internal const string Default_DeleteByOther_Se = "";
        internal const string Default_DeleteByOther_Message = "$no 予約されたご本人か、お名前が一致していないようです";

        internal const string Default_Explain_Se = "";
        internal const string Default_Explain_Message = "予約の仕方 /yoyaku 予約 /torikeshi 取り消し";

        public string Reserved_Se { get => GetValue(); set => SetValue(value); }

        public string Reserved_Message { get => GetValue(); set => SetValue(value); }

        public string Delete_Se { get => GetValue(); set => SetValue(value); }

        public string Delete_Message { get => GetValue(); set => SetValue(value); }

        public string Call_Se { get => GetValue(); set => SetValue(value); }

        public string Call_Message { get => GetValue(); set => SetValue(value); }

        public string AlreadyReserved_Se { get => GetValue(); set => SetValue(value); }

        public string AlreadyReserved_Message { get => GetValue(); set => SetValue(value); }

        public string HandleNameNotSubscribed_Se { get => GetValue(); set => SetValue(value); }

        public string HandleNameNotSubscribed_Message { get => GetValue(); set => SetValue(value); }

        public string NotReserved_Se { get => GetValue(); set => SetValue(value); }

        public string NotReserved_Message { get => GetValue(); set => SetValue(value); }

        public string DeleteByOther_Se { get => GetValue(); set => SetValue(value); }

        public string DeleteByOther_Message { get => GetValue(); set => SetValue(value); }

        public string Explain_Se { get => GetValue(); set => SetValue(value); }

        public string Explain_Message { get => GetValue(); set => SetValue(value); }

        public string HcgSettingFilePath { get => GetValue(); set => SetValue(value); }
        public bool IsEnabled { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(Reserved_Se), new Item { DefaultValue = Default_Reserved_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Reserved_Message), new Item { DefaultValue = Default_Reserved_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(Delete_Se), new Item { DefaultValue = Default_Delete_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Delete_Message), new Item { DefaultValue = Default_Delete_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(Call_Se), new Item { DefaultValue = Default_Call_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Call_Message), new Item { DefaultValue = Default_Call_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(AlreadyReserved_Se), new Item { DefaultValue = Default_AlreadyReserved_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(AlreadyReserved_Message), new Item { DefaultValue = Default_AlreadyReserved_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(HandleNameNotSubscribed_Se), new Item { DefaultValue = Default_HandleNameNotSubscribed_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(HandleNameNotSubscribed_Message), new Item { DefaultValue = Default_HandleNameNotSubscribed_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(NotReserved_Se), new Item { DefaultValue = Default_NotReserved_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(NotReserved_Message), new Item { DefaultValue = Default_NotReserved_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(DeleteByOther_Se), new Item { DefaultValue = Default_DeleteByOther_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(DeleteByOther_Message), new Item { DefaultValue = Default_DeleteByOther_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });

            Dict.Add(nameof(Explain_Se), new Item { DefaultValue = Default_Explain_Se, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
            Dict.Add(nameof(Explain_Message), new Item { DefaultValue = Default_Explain_Message, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });


            Dict.Add(nameof(IsEnabled), new Item { DefaultValue = false, Predicate = b => true, Serializer = b => b.ToString(), Deserializer = s => bool.Parse(s) });
            Dict.Add(nameof(HcgSettingFilePath), new Item { DefaultValue = _hcgSettingFilePath, Predicate = s => !string.IsNullOrEmpty(s), Serializer = s => s, Deserializer = s => s });
        }
        private static readonly string _hcgSettingFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"hcg\setting.xml");
    }
}
