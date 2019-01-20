using System;
using System.Linq;
using System.Collections.Generic;

namespace Geode.Habbo.Messages
{
    public sealed class MessagesResolveException : Exception
    {
        public string Revision { get; }
        public Dictionary<string, IList<string>> Unresolved { get; }

        public MessagesResolveException(string revision, IDictionary<string, IList<string>> unresolved)
            : base($"Failed to resolve {unresolved.Count:n0} identifiers for {revision}.")
        {
            Revision = revision;
            Unresolved = new Dictionary<string, IList<string>>(
                unresolved.ToDictionary(u => u.Key, u => u.Value));
        }
    }
}