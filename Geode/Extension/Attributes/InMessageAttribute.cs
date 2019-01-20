namespace Geode.Extension.Attributes
{
    public sealed class InMessageAttribute : MessageAttribute
    {
        public InMessageAttribute(string identifier)
            : base(identifier, false)
        { }
    }
}