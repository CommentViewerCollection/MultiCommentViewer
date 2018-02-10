using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
namespace Common
{
    public abstract class DynamicOptionsBase : INotifyPropertyChanged
    {
        Dictionary<string, Item> _dict = new Dictionary<string, Item>();
        protected Dictionary<string, Item> Dict { get => _dict; }
        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var kv in _dict)
            {
                var k = kv.Key;
                var v = kv.Value;
                sb.Append(k);
                sb.Append("=");
                sb.Append(v.Serializer(v.Value));
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        protected abstract void Init();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">name=value separated by CRLF</param>
        public void Deserialize(string s)
        {
            Reset();
            if (string.IsNullOrEmpty(s))
                return;
            var arr = s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in arr)
            {
                var kv = line.Split('=');
                if (kv.Length != 2)
                    continue;
                var k = kv[0];
                var v = kv[1];
                if (_dict.TryGetValue(k, out Item item))
                {
                    try
                    {
                        item.Value = item.Deserializer(v);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        //最初にReset()しているから、ここに来たらitem.Value==item.DefaultValue
                    }
                    if (!item.Predicate(item.Value))
                        item.Value = item.DefaultValue;
                }
            }
        }
        protected dynamic GetValue([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var item = _dict[propertyName];
            return item.Value;
        }
        protected void SetValue(dynamic d, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var item = _dict[propertyName];
            item.Value = d;
            RaisePropertyChanged(propertyName);
        }
        public DynamicOptionsBase()
        {
            Init();
            Reset();
        }
        /// <summary>
        /// すべての値を初期化する
        /// </summary>
        public void Reset()
        {
            foreach (var kv in _dict)
            {
                var v = kv.Value;
                v.Value = v.DefaultValue;
            }
        }
        private void CheckValidation()
        {
            foreach (var kv in _dict)
            {
                var k = kv.Key;
                var v = kv.Value;
                if (!v.Predicate(v.Value))
                {
                    v.Value = v.DefaultValue;
                }
            }
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion

        protected class Item
        {
            /// <summary>
            /// 文字列をDeserializerに通した後の値の妥当性を評価する。
            /// 文字列に形式上の問題がある場合はDeserializerで例外が投げられるだろうからcatchしてDefaultValueを入れる
            /// </summary>
            public Predicate<dynamic> Predicate { get; set; }
            public dynamic DefaultValue { get; set; }
            public dynamic Value { get; set; }
            public Func<dynamic, string> Serializer { get; set; }
            public Func<string, dynamic> Deserializer { get; set; }

        }
    }
}
