// BULWARK — ConfigResolver tests (Phase 0.3 scaffold).
// Proves the RC -> SO -> literal PRECEDENCE in pure C# (no editor/device needed),
// which is the one piece of GATE 0.3 verifiable without Unity. The remaining 0.3
// proof (a *designer* SO edit changing live sim behavior in-editor) still requires
// Unity, so GATE 0.3 overall is BLOCKED (see docs/adr/ADR-0-001).
// SCAFFOLD STATUS: authored, NOT executed here (no NUnit/Unity test runner).

using System.Collections.Generic;
using NUnit.Framework;
using Bulwark.Services.Config;

namespace Bulwark.Services.Tests
{
    public class ConfigResolverTests
    {
        [Test]
        public void RemoteConfig_Supersedes_ScriptableObject_And_Literal()
        {
            var remote = new DictionaryConfigStore(new Dictionary<string, string> { { "k", "3.0" } });
            var so     = new DictionaryConfigStore(new Dictionary<string, string> { { "k", "2.0" } });
            var r = new ConfigResolver(remote, so).ResolveFloat("k", 1.0f);
            Assert.AreEqual(3.0f, r.Value);
            Assert.AreEqual(ConfigSource.RemoteConfig, r.Source);
        }

        [Test]
        public void ScriptableObject_Supersedes_Literal_When_No_Remote()
        {
            var remote = new DictionaryConfigStore(new Dictionary<string, string>()); // empty
            var so     = new DictionaryConfigStore(new Dictionary<string, string> { { "k", "2.0" } });
            var r = new ConfigResolver(remote, so).ResolveFloat("k", 1.0f);
            Assert.AreEqual(2.0f, r.Value);
            Assert.AreEqual(ConfigSource.ScriptableObject, r.Source);
        }

        [Test]
        public void Literal_Is_Used_When_Both_Absent()
        {
            var empty = new DictionaryConfigStore(new Dictionary<string, string>());
            var r = new ConfigResolver(empty, empty).ResolveFloat("k", 1.0f);
            Assert.AreEqual(1.0f, r.Value);
            Assert.AreEqual(ConfigSource.Literal, r.Source);
        }
    }
}
