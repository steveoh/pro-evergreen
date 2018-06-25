using System;
using Octokit;

namespace ProEvergreen.models
{
    public class GitHubOptions
    {
        /// <summary>
        /// The github enterprise url
        /// http://octokitnet.readthedocs.io/en/latest/getting-started/#connect-to-github-enterprise
        /// </summary>
        public Uri GitHubEnterpriseUri { get; set; }
        /// <summary>
        /// the github credentials for private repositories
        /// http://octokitnet.readthedocs.io/en/latest/getting-started/#authenticated-access
        /// </summary>
        public Credentials Credentials { get; set; }
    }
}
