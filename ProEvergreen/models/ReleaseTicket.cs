using System.Collections.Generic;
using Octokit;

namespace ProEvergreen.models
{
    public class ReleaseTicket
    {
        public ReleaseTicket()
        {
            Punched = false;
            Releases = new List<Release>();
        }

        public bool Punched { get; set; }
        public IReadOnlyCollection<Release> Releases { get; set; }
    }
}
