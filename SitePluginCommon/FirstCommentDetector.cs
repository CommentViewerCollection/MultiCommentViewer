using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePluginCommon
{
    public class FirstCommentDetector
    {
        Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
        public bool IsFirstComment(string userId)
        {
            bool isFirstComment;
            if (_userCommentCountDict.ContainsKey(userId))
            {
                _userCommentCountDict[userId]++;
                isFirstComment = false;
            }
            else
            {
                _userCommentCountDict.Add(userId, 1);
                isFirstComment = true;
            }
            return isFirstComment;
        }
        public void Reset()
        {
            _userCommentCountDict = new Dictionary<string, int>();
        }
    }
}
