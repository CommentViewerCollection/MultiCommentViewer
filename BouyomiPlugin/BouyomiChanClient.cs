//BouyomiChanClient.cs - Ver0.1.10.0
//棒読みちゃんにIpcClientChannelで接続して読み上げを行うためのクラスです。
//ご自由にお使いください。
//System.Runtime.Remotingを参照設定する必要があります
using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace FNF.Utility
{

    /// <summary>
    /// 声の種類。(0:デフォルト、1～8:AquesTalk、10001～：SAPI5)
    /// </summary>
    public enum VoiceType { Default = 0, Female1 = 1, Female2 = 2, Male1 = 3, Male2 = 4, Imd1 = 5, Robot1 = 6, Machine1 = 7, Machine2 = 8 }

    /// <summary>
    /// 棒読みちゃんへ接続するためのクラス。
    /// </summary>
    public class BouyomiChanClient : IDisposable
    {
        protected IpcClientChannel ClientChannel;
        protected BouyomiChanRemoting RemotingObject;

        /// <summary>
        /// オブジェクト生成。
        /// 利用後にはDispose()で開放してください。
        /// </summary>
        public BouyomiChanClient()
        {
            ClientChannel = new IpcClientChannel("hogehoge", null); //チャンネル名は何でもいい
            ChannelServices.RegisterChannel(ClientChannel, false);
            RemotingObject = (BouyomiChanRemoting)Activator.GetObject(typeof(BouyomiChanRemoting), "ipc://BouyomiChan/Remoting");
        }

        /// <summary>
        /// ファイナライザ（Dispose Finalizeパターン実装）
        /// </summary>
        ~BouyomiChanClient()
        {
            Dispose(false);
        }

        /// <summary>
        /// オブジェクト開放。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// オブジェクト開放。(Dispose Finalizeパターン実装)
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (ClientChannel != null)
            {
                ChannelServices.UnregisterChannel(ClientChannel);
                ClientChannel = null;
            }
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        public void AddTalkTask(string sTalkText)
        {
            RemotingObject.AddTalkTask(sTalkText);
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。(音程指定無し版。以前のバージョンとの互換性の為に残しています。)
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        /// <param name="iSpeed"   >再生。(-1で棒読みちゃん側の画面で選んでいる速度)</param>
        /// <param name="iVolume"  >音量。(-1で棒読みちゃん側の画面で選んでいる音量)</param>
        /// <param name="vType"    >声の種類。(Defaultで棒読みちゃん側の画面で選んでいる声)</param>
        public void AddTalkTask(string sTalkText, int iSpeed, int iVolume, VoiceType vType)
        {
            RemotingObject.AddTalkTask(sTalkText, iSpeed, iVolume, (int)vType);
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        /// <param name="iSpeed"   >速度。(-1で棒読みちゃん側の画面で選んでいる速度)</param>
        /// <param name="iTone"    >音程。(-1で棒読みちゃん側の画面で選んでいる音程)</param>
        /// <param name="iVolume"  >音量。(-1で棒読みちゃん側の画面で選んでいる音量)</param>
        /// <param name="vType"    >声の種類。(Defaultで棒読みちゃん側の画面で選んでいる声)</param>
        public void AddTalkTask(string sTalkText, int iSpeed, int iTone, int iVolume, VoiceType vType)
        {
            RemotingObject.AddTalkTask(sTalkText, iSpeed, iTone, iVolume, (int)vType);
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。読み上げタスクIDを返します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        /// <returns>読み上げタスクID。</returns>
        public int AddTalkTask2(string sTalkText)
        {
            return RemotingObject.AddTalkTask2(sTalkText);
        }

        /// <summary>
        /// 棒読みちゃんに音声合成タスクを追加します。読み上げタスクIDを返します。
        /// </summary>
        /// <param name="sTalkText">喋らせたい文章</param>
        /// <param name="iSpeed"   >速度。(-1で棒読みちゃん側の画面で選んでいる速度)</param>
        /// <param name="iTone"    >音程。(-1で棒読みちゃん側の画面で選んでいる音程)</param>
        /// <param name="iVolume"  >音量。(-1で棒読みちゃん側の画面で選んでいる音量)</param>
        /// <param name="vType"    >声の種類。(Defaultで棒読みちゃん側の画面で選んでいる声)</param>
        /// <returns>読み上げタスクID。</returns>
        public int AddTalkTask2(string sTalkText, int iSpeed, int iTone, int iVolume, VoiceType vType)
        {
            return RemotingObject.AddTalkTask2(sTalkText, iSpeed, iTone, iVolume, (int)vType);
        }

        /// <summary>
        /// 棒読みちゃんの残りのタスクを全て消去します。
        /// </summary>
        public void ClearTalkTasks()
        {
            RemotingObject.ClearTalkTasks();
        }

        /// <summary>
        /// 棒読みちゃんの現在のタスクを中止して次の行へ移ります。
        /// </summary>
        public void SkipTalkTask()
        {
            RemotingObject.SkipTalkTask();
        }

        /// <summary>
        /// 棒読みちゃんの現在のタスク数（再生待ち行数）を取得します。
        /// </summary>
        public int TalkTaskCount
        {
            get { return RemotingObject.TalkTaskCount; }
        }

        /// <summary>
        /// 棒読みちゃんの現在再生中のタスクIDを取得します。
        /// </summary>
        public int NowTaskId
        {
            get { return RemotingObject.NowTaskId; }
        }

        /// <summary>
        /// 棒読みちゃんが現在、音声を再生している最中かどうかを取得します。
        /// </summary>
        public bool NowPlaying
        {
            get { return RemotingObject.NowPlaying; }
        }

        /// <summary>
        /// 棒読みちゃんが一時停止中かどうかを取得・設定します。
        /// ※現在の行の再生が終了するまで停止しません。
        /// </summary>
        public bool Pause
        {
            get { return RemotingObject.Pause; }
            set { RemotingObject.Pause = value; }
        }
    }

    /// <summary>
    /// .NET Remotingのためのクラス。（本クラスの内容を変更してしまうと通信できなくなってしまいます）
    /// </summary>
    public class BouyomiChanRemoting : MarshalByRefObject
    {
        public void AddTalkTask(string sTalkText) { }
        public void AddTalkTask(string sTalkText, int iSpeed, int iVolume, int vType) { }
        public void AddTalkTask(string sTalkText, int iSpeed, int iTone, int iVolume, int vType) { }
        public int AddTalkTask2(string sTalkText) { throw null; }
        public int AddTalkTask2(string sTalkText, int iSpeed, int iTone, int iVolume, int vType) { throw null; }
        public void ClearTalkTasks() { }
        public void SkipTalkTask() { }

        public int TalkTaskCount { get { throw null; } }
        public int NowTaskId { get { throw null; } }
        public bool NowPlaying { get { throw null; } }
        public bool Pause { get { throw null; } set { } }
    }
}
