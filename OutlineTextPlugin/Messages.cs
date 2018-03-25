using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
namespace OutlineTextPlugin
{
    class ShowOptionsViewMessage : MessageBase
    {
        public ShowOptionsViewMessage(OptionsViewModel vm)
        {
            Vm = vm;
        }
        public OptionsViewModel Vm { get; }
    }
    class CloseOptionsViewMessage : MessageBase { }
}
