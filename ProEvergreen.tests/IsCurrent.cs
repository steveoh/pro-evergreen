namespace ProEvergreen.tests
{
    using System;
    using Octokit;
    using Xunit;

    public class IsCurrent
    {
        [Theory]
        [InlineData("1.0.0", "v1.0.0", true)] // versions match
        [InlineData("1.0.0-beta.1", "v1.0.0-beta.1", true)] // versions match
        [InlineData("1.0.0-beta.0", "v1.0.0-beta.1", false)] // beta version is smaller
        [InlineData("1.0.0", "v1.1.0", false)] // tag version is higher
        [InlineData("1.0.0", "v1.0.1", false)] // tag version is higher
        [InlineData("2.0.0", "v1.0.0", true)] // current version is higher than tag. Active development?
        public void IsCurrent_responds_correctly(string currentVersion, string tagVersion, bool isCurrent)
        {
            var patient = new Evergreen("user", "repo");
            var version = CreateReleaseFromTag(tagVersion);

            Assert.Equal(isCurrent, patient.IsCurrent(currentVersion, version));
        }

        [Theory]
        [InlineData("1.0", "v1.0.0")] // invalid semver
        [InlineData("1.0.0.0", "v1.0.0")] // invalid semver
        [InlineData("1.0.0.1", "v1.0.0")] // invalid semver
        public void IsCurrent_throws_on_invalid_semversions(string currentVersion, string tagVersion)
        {
            var patient = new Evergreen("user", "repo");
            var version = CreateReleaseFromTag(tagVersion);

            Assert.Throws<FormatException>(() => patient.IsCurrent(currentVersion, version));
        }

        [Fact]
        public void Throws_if_release_is_null()
        {
            var patient = new Evergreen("user", "repo");
            const string currentVersion = "1.0.0";

            Assert.Throws<ArgumentNullException>(() => patient.IsCurrent(currentVersion, null));
        }

        public static Release CreateReleaseFromTag(string version)
        {
            var s = string.Empty;

            return new Release(s, s, s, s, 0, s, version, s, s, s, false, false, DateTimeOffset.Now, DateTimeOffset.Now, new Author(), s, s,
                               null);
        }
    }
}
