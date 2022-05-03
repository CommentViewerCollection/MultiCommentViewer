namespace Mcv.PluginV2;

public class FirstCommentDetector
{
    Dictionary<string, int> _userCommentCountDict = new();
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
