// BULWARK — Analytics client stub (Phase 0.4 scaffold).
// Roadmap MODERNIZE: the original's THREE analytics pipelines collapse to ONE
// consolidated pipeline (data hygiene). This is the single emit seam.
// PRIVACY: never emit save-state payloads or device PII (roadmap REPLACE rule).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 — ADR-0-001).

using System.Collections.Generic;

namespace Bulwark.Services.Analytics
{
    /// <summary>Single consolidated analytics emit seam (one pipeline, § MODERNIZE).</summary>
    public interface IAnalytics
    {
        void Emit(string eventName, IReadOnlyDictionary<string, object> parameters = null);
    }

    /// <summary>No-op stub: lets calling code/CI wire events without a live provider.</summary>
    public sealed class StubAnalytics : IAnalytics
    {
        public int EmittedCount { get; private set; }
        public string LastEvent { get; private set; }

        public void Emit(string eventName, IReadOnlyDictionary<string, object> parameters = null)
        {
            // Intentionally records only the event name/count — NO payload persistence,
            // NO save-state, NO PII. A consolidated provider adapter replaces this.
            EmittedCount++;
            LastEvent = eventName;
        }
    }
}
