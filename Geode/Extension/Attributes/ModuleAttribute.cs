using System;

namespace Geode.Extension
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleAttribute : Attribute
    {
        public string Title { get; }
        public string Author { get; }
        public string Description { get; }
        public bool UtilizingOnDoubleClick { get; }
        public bool LeaveButtonVisible { get; }

        public ModuleAttribute(string title, string author, string description, bool utilizingondoubleclick = false, bool leavebuttonvisible = true)
        {
            Title = title;
            Author = author;
            Description = description;
            UtilizingOnDoubleClick = utilizingondoubleclick;
            LeaveButtonVisible = leavebuttonvisible;
        }
    }
}