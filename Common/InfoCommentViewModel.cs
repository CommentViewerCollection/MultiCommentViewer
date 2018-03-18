using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SitePlugin;
namespace Common
{
    public class InfoCommentViewModel : CommentViewModelBase, IInfoCommentViewModel
    {
        public override string UserId => "-";

        public InfoType Type { get; }

        public InfoCommentViewModel(ICommentOptions options, string message, InfoType type)
            : base(options)
        {
            IsInfo = true;
            MessageItems = new List<IMessagePart>
            {
                new MessageText(message),
            };
            Type = type;
        }
        [Obsolete]
        public InfoCommentViewModel(ICommentOptions options, string message)
            : base(options)
        {
            IsInfo = true;
            MessageItems = new List<IMessagePart>
            {
                new MessageText(message),
            };
            Type =  InfoType.Debug;
        }
    }
    public interface IInfoCommentViewModel : ICommentViewModel
    {
        InfoType Type { get; }
    }
    public enum InfoType
    {
        /// <summary>
        /// 無し
        /// </summary>
        None,
        /// <summary>
        /// 例外全て
        /// </summary>
        Debug,
        /// <summary>
        /// 
        /// </summary>
        Notice,
        /// <summary>
        /// 致命的なエラーがあった場合だけ。必要最小限の情報
        /// </summary>
        Error,
    }

}
