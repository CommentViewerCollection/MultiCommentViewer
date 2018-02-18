using SitePlugin;
namespace Common
{
    public class MessageText : IMessageText
    {
        public string Text { get; }
        public MessageText(string text)
        {
            Text = text;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageText text)
            {
                return this.Text.Equals(text.Text);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode();
        }
    }
}
