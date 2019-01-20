namespace Geode.Extension.Attributes
{
    public sealed class OutMessageAttribute : MessageAttribute
    {
        public OutMessageAttribute(string identifier)
            : base(identifier, true)
        { }
    }
}