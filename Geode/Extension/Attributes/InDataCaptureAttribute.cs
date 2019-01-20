namespace Geode.Extension
{
    public sealed class InDataCaptureAttribute : DataCaptureAttribute
    {
        public InDataCaptureAttribute(ushort id)
            : base(id, false)
        { }
        public InDataCaptureAttribute(string identifier)
            : base(identifier, false)
        { }
    }
}