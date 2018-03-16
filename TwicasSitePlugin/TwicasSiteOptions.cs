using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Text.RegularExpressions;
namespace TwicasSitePlugin
{
    internal class TwicasSiteOptions : DynamicOptionsBase
    {
        //コメント取得インターバル
        //キートス
        protected override void Init()
        {
        }
        internal TwicasSiteOptions Clone()
        {
            return (TwicasSiteOptions)this.MemberwiseClone();
        }
        internal void Set(TwicasSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
    }
}
