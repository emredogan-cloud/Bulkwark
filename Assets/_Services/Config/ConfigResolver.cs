// BULWARK — 3-tier config resolver (Phase 0.3 scaffold).
// Roadmap §9/§12: a value resolves RemoteConfig (server) -> typed ScriptableObject
// (designer) -> literal (code default), in that precedence. This is the [PRESERVE]'d
// 3-tier RemoteConfig pattern recovered from the original, now SERVER-VALIDATED.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 — ADR-0-001). GATE 0.3
// (designer-edit reflects; RC override supersedes; literal fallback) cannot be
// executed without the editor + a backend, so it is reported BLOCKED, not PASS.

using System;
using System.Collections.Generic;

namespace Bulwark.Services.Config
{
    /// <summary>Source that won, for diagnostics/telemetry (which tier supplied a value).</summary>
    public enum ConfigSource { Literal, ScriptableObject, RemoteConfig }

    public readonly struct Resolved<T>
    {
        public readonly T Value;
        public readonly ConfigSource Source;
        public Resolved(T value, ConfigSource source) { Value = value; Source = source; }
    }

    /// <summary>Supplies server-pushed overrides (tier 1). Backed by BaaS RemoteConfig in 0.4.</summary>
    public interface IRemoteConfigStore
    {
        bool TryGet(string key, out string raw); // raw string; typed-parsed by the resolver
    }

    /// <summary>Supplies designer-authored values from typed ScriptableObjects (tier 2).</summary>
    public interface IScriptableConfigStore
    {
        bool TryGet(string key, out string raw);
    }

    /// <summary>
    /// Resolves config keys by precedence RC -> SO -> literal default.
    /// Pure C# (no UnityEngine dependency) so it is unit-testable on CI without the editor.
    /// </summary>
    public sealed class ConfigResolver
    {
        private readonly IRemoteConfigStore _remote;
        private readonly IScriptableConfigStore _scriptable;

        public ConfigResolver(IRemoteConfigStore remote, IScriptableConfigStore scriptable)
        {
            _remote = remote;
            _scriptable = scriptable;
        }

        public Resolved<float> ResolveFloat(string key, float literalDefault)
        {
            if (_remote != null && _remote.TryGet(key, out var r) && float.TryParse(r, out var rv))
                return new Resolved<float>(rv, ConfigSource.RemoteConfig);
            if (_scriptable != null && _scriptable.TryGet(key, out var s) && float.TryParse(s, out var sv))
                return new Resolved<float>(sv, ConfigSource.ScriptableObject);
            return new Resolved<float>(literalDefault, ConfigSource.Literal);
        }

        public Resolved<int> ResolveInt(string key, int literalDefault)
        {
            if (_remote != null && _remote.TryGet(key, out var r) && int.TryParse(r, out var rv))
                return new Resolved<int>(rv, ConfigSource.RemoteConfig);
            if (_scriptable != null && _scriptable.TryGet(key, out var s) && int.TryParse(s, out var sv))
                return new Resolved<int>(sv, ConfigSource.ScriptableObject);
            return new Resolved<int>(literalDefault, ConfigSource.Literal);
        }
    }

    /// <summary>In-memory store for tests/local; production swaps in BaaS-backed stores (0.4).</summary>
    public sealed class DictionaryConfigStore : IRemoteConfigStore, IScriptableConfigStore
    {
        private readonly Dictionary<string, string> _map;
        public DictionaryConfigStore(Dictionary<string, string> map) { _map = map ?? new Dictionary<string, string>(); }
        public bool TryGet(string key, out string raw) => _map.TryGetValue(key, out raw);
    }
}
