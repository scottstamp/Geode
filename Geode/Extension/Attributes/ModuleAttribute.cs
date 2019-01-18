using System;

namespace Geode.Extension
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ModuleAttribute : Attribute
    {
        public string Title { get; }
        public string Author { get; }
        public string Description { get; }

        public ModuleAttribute(string title, string author, string description)
        {
            Title = title;
            Author = author;
            Description = description;
        }
    }
}