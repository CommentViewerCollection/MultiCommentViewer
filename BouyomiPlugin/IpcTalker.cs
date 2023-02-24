using FNF.Utility;
using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;

namespace BouyomiPlugin
{
    class IpcTalker : ITalker
    {
        public void Dispose()
        {
            _bouyomiChanClient.Dispose();
        }

        public void TalkText(string text)
        {
            _bouyomiChanClient.AddTalkTask2(text);
        }

        public void TalkText(string text, Int16 voiceSpeed, Int16 voiceTone, Int16 voiceVolume, VoiceType voiceType)
        {
            try
            {
                _bouyomiChanClient.AddTalkTask2(
                    text,
                    voiceSpeed,
                    voiceTone,
                    voiceVolume,
                    voiceType
                );
            }
            catch (RemotingException ex)
            {
                throw new TalkException("", ex);
            }
        }
        private readonly BouyomiChanClient _bouyomiChanClient = new FNF.Utility.BouyomiChanClient();
    }
}
