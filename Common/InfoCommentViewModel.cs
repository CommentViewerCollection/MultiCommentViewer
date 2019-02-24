using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using SitePlugin;
namespace Common
{
    public class SystemInfoCommentViewModel : CommentViewModelBase, IInfoCommentViewModel
    {
        public override string UserId => "-";
        private readonly static IUser _user = new UserTest("-") { Nickname = "-" };
        public InfoType Type { get; }
        public override MessageType MessageType { get; protected set; } = MessageType.SystemInfo;

        public SystemInfoCommentViewModel(ICommentOptions options, string message, InfoType type)
            : base(options, _user, null, false)
        {
            MessageItems = new List<IMessagePart>
            {
                MessagePartFactory.CreateMessageText(message),
            };
            Type = type;
        }
    }
    public class BroadcastInfoCommentViewModel : CommentViewModelBase, IInfoCommentViewModel
    {
        public override string UserId => "-";
        private static readonly IUser DefaultUser = new UserTest("-") { Nickname = "-" };
        public InfoType Type { get; }
        public override MessageType MessageType { get; protected set; } = MessageType.BroadcastInfo;

        /// <summary>
        /// 来場者数とかのような特定のユーザが出したものではない場合にこっち
        /// </summary>
        /// <param name="options"></param>
        /// <param name="message"></param>
        public BroadcastInfoCommentViewModel(ICommentOptions options, string message)
            : base(options, DefaultUser, null, false)
        {
            MessageItems = new List<IMessagePart>
            {
                MessagePartFactory.CreateMessageText(message),
            };
        }
        /// <summary>
        /// 投げ銭とかアイテムとかユーザを識別したい場合に使う
        /// </summary>
        /// <param name="options"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        public BroadcastInfoCommentViewModel(ICommentOptions options, string message, IUser user, ICommentProvider cp)
            : this(options, new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) }, user, cp)
        { }
        /// <summary>
        /// 投げ銭とかアイテムとかユーザを識別したい場合に使う
        /// </summary>
        /// <param name="options"></param>
        /// <param name="message"></param>
        /// <param name="user"></param>
        public BroadcastInfoCommentViewModel(ICommentOptions options, List<IMessagePart> messageItems, IUser user, ICommentProvider cp)
            : base(options, user, cp, false)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            MessageItems = messageItems;
        }
    }
    /// <summary>
    /// 情報コメントのインタフェース
    /// </summary>
    public interface IInfoCommentViewModel : ICommentViewModel
    {
        InfoType Type { get; }
    }
    /// <summary>
    /// 情報の種類。
    /// デバッグ情報や軽微なエラー情報が必要無い場合があるため分類する。
    /// </summary>
    /// <remarks>大小比較ができるように</remarks>
    public enum InfoType
    {
        /// <summary>
        /// 無し
        /// </summary>
        None,
        /// <summary>
        /// 致命的なエラーがあった場合だけ。必要最小限の情報
        /// </summary>
        Error,
        /// <summary>
        /// 
        /// </summary>
        Notice,
        /// <summary>
        /// 例外全て
        /// </summary>
        Debug,
    }
    public static class InfoTypeRelatedOperations
    {
        /// <summary>
        /// 文字列をInfoTypeに変換する。
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>InfoTypeをEnumではなくclassにしてこのメソッドもそこに含めたほうが良いかも</remarks>
        public static InfoType ToInfoType(string s)
        {
            if (!Enum.TryParse(s, out InfoType type))
            {
                type = InfoType.Notice;
            }
            return type;
        }
    }
}
