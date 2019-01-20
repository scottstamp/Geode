namespace Geode.Extension
{
    public sealed class OutDataCaptureAttribute : DataCaptureAttribute
    {
        public OutDataCaptureAttribute(ushort id)
            : base(id, true)
        { }
        public OutDataCaptureAttribute(string identifier)
            : base(identifier, true)
        { }
    }
}