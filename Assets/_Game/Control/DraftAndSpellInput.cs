// BULWARK — Phase 2.4 SPELL CONTROL shell (draft + cast input). Roadmap §13 2.4 ("draft 3"),
// §5.3 (spell bible — telegraph/counter is the SIM's job), §12 (ECS boundary: this is a
// MonoBehaviour INPUT shell — it ONLY translates UI intent into sim DATA writes; it contains
// NO gameplay simulation), §15 (no new mechanics — it drafts/casts from the already-authored
// SpellDef pool baked into the SpellCatalog; it invents nothing).
//
// WHAT THIS SHELL DOES (and ONLY this):
//   • PRE-BATTLE DRAFT-3 (§13 2.4): the player chooses exactly 3 spell indices from the shared
//     pool. ConfirmDraft writes those into the PLAYER side's SpellSlot buffer (SpellLoadoutTag
//     singleton-per-side), seeding each slot's ChargesLeft from the baked SpellCatalogEntry, and
//     sets DraftState.PlayerDone = 1. The sim's match-start gate waits until both sides drafted.
//   • IN-BATTLE ARM + CAST (§5.3): the player ARMS one of the 3 drafted slots, then a tap/aim
//     APPENDS a SpellCastRequest to the shared SpellCastInboxTag inbox. The SIM (SpellCastSystem)
//     validates cooldown/charges, spends the charge, and spawns the telegraph — this shell never
//     resolves a spell, spends a charge, or applies an effect. Every cast is therefore counterable
//     (the telegraph window is owned by the sim, §5.3).
//
// HARD §12 BOUNDARY: no combat/economy/telegraph logic here. Reads are for UI/validation only
// (e.g. "is this slot ready?"); all CONSEQUENCES happen in Assets/_Game/Sim. World access is via
// World.DefaultGameObjectInjectionWorld.EntityManager; screen→world via an assigned Camera —
// IDENTICAL pattern to the Phase-1 BattleInput.cs control shell (same asmdef references).
//
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// DEFERRED: button/HUD wiring + touch feel are GATE-1 (on-device) concerns; this shell exposes
// the public API (BeginDraft/PickForDraft/ConfirmDraft/ArmSlot/CastAt/CastAtScreen) a thin HUD calls.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;
using Bulwark.Data;

namespace Bulwark.Control
{
    /// <summary>
    /// Thin pointer/HUD input shell for the Phase-2.4 spell model (§13 2.4 / §5.3). Translates the
    /// player's draft picks and in-battle casts into sim DATA writes on the player side's spell
    /// loadout + the shared cast inbox. Attach to a GameObject in the battle scene and assign
    /// <see cref="battleCamera"/>. Contains NO gameplay simulation (§12) — the sim resolves everything.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DraftAndSpellInput : MonoBehaviour
    {
        [Header("Scene wiring")]
        [Tooltip("Camera used for screen->world projection of the 2D battlefield. Required for tap-to-aim casts.")]
        [SerializeField] private Camera battleCamera;

        [Tooltip("Local player team id. Canon: Player = Team 0 (the side this shell drafts/casts for).")]
        [SerializeField] private int playerTeam = 0;

        [Tooltip("Spells to draft (§13 2.4 'draft 3'). Exactly this many picks are required to confirm.")]
        [SerializeField] private int draftSize = 3;

        // ---- runtime input bookkeeping ONLY (no sim state lives here) ----
        // The player's in-progress draft picks (indices into the shared SpellCatalog pool).
        private readonly List<int> _draftPicks = new List<int>(3);
        private bool _draftConfirmed;

        // Which of the 3 drafted slots is currently ARMED for casting (-1 = none). This is a UI
        // selection only; the authoritative cooldown/charge gate lives in SpellCastSystem.
        private int _armedSlot = -1;

        private EntityManager EM
        {
            get
            {
                var world = World.DefaultGameObjectInjectionWorld;
                return world != null && world.IsCreated ? world.EntityManager : default;
            }
        }

        private void Reset() => battleCamera = Camera.main;

        private void Awake()
        {
            if (battleCamera == null) battleCamera = Camera.main;
        }

        // =====================================================================================
        // PRE-BATTLE DRAFT-3 (§13 2.4)
        // =====================================================================================

        /// <summary>Start a fresh draft (clears any prior in-progress picks). Call from the draft HUD open.</summary>
        public void BeginDraft()
        {
            _draftPicks.Clear();
            _draftConfirmed = false;
        }

        /// <summary>How many spells exist in the baked shared pool (HUD uses it to list choices). 0 if not baked yet.</summary>
        public int PoolSize()
        {
            var em = EM;
            if (!em.World.IsCreated) return 0;
            if (!em.CreateEntityQuery(ComponentType.ReadOnly<SpellCatalogTag>()).TryGetSingletonEntity<SpellCatalogTag>(out var catalog))
                return 0;
            return em.HasBuffer<SpellCatalogEntry>(catalog) ? em.GetBuffer<SpellCatalogEntry>(catalog).Length : 0;
        }

        /// <summary>Current count of in-progress draft picks (HUD progress, 0..draftSize).</summary>
        public int DraftPickCount => _draftPicks.Count;

        /// <summary>The pool index chosen at draft position <paramref name="i"/> (for HUD highlight). -1 if none.</summary>
        public int DraftPickAt(int i) => (i >= 0 && i < _draftPicks.Count) ? _draftPicks[i] : -1;

        /// <summary>
        /// Toggle a spell <paramref name="poolIndex"/> into/out of the in-progress draft. Rejects
        /// out-of-pool indices, duplicates (toggles off instead), and additions past draftSize.
        /// Pure local bookkeeping — writes NOTHING to the sim until <see cref="ConfirmDraft"/>.
        /// Returns true if the pick set changed.
        /// </summary>
        public bool PickForDraft(int poolIndex)
        {
            if (_draftConfirmed) return false;
            int pool = PoolSize();
            if (poolIndex < 0 || (pool > 0 && poolIndex >= pool)) return false;

            int existing = _draftPicks.IndexOf(poolIndex);
            if (existing >= 0)
            {
                _draftPicks.RemoveAt(existing); // tap again to de-select
                return true;
            }
            if (_draftPicks.Count >= draftSize) return false; // already have the full draft
            _draftPicks.Add(poolIndex);
            return true;
        }

        /// <summary>
        /// Finalize the draft (§13 2.4): writes the player side's SpellSlot buffer from the picks
        /// (one slot per pick, ChargesLeft seeded from the baked SpellCatalogEntry.Charges,
        /// Cooldown 0 = ready) and sets DraftState.PlayerDone = 1 so the sim's start gate can
        /// proceed once the AI side is also done. Requires exactly draftSize picks and a baked
        /// player SpellLoadoutTag buffer. Returns false (no writes) if either is missing.
        /// This is the ONLY sim write the draft phase makes — all consequences are the sim's.
        /// </summary>
        public bool ConfirmDraft()
        {
            if (_draftConfirmed) return true;
            if (_draftPicks.Count != draftSize) return false;

            var em = EM;
            if (!em.World.IsCreated) return false;

            if (!TryGetPlayerLoadout(em, out Entity loadoutEntity) ||
                !em.HasBuffer<SpellSlot>(loadoutEntity))
                return false;

            // Read the baked catalog to seed each slot's charges from DATA (§15.6 — no literal).
            DynamicBuffer<SpellCatalogEntry> catalog = default;
            bool hasCatalog =
                em.CreateEntityQuery(ComponentType.ReadOnly<SpellCatalogTag>())
                  .TryGetSingletonEntity<SpellCatalogTag>(out var catalogEntity) &&
                em.HasBuffer<SpellCatalogEntry>(catalogEntity);
            if (hasCatalog)
                catalog = em.GetBuffer<SpellCatalogEntry>(catalogEntity);

            DynamicBuffer<SpellSlot> slots = em.GetBuffer<SpellSlot>(loadoutEntity);
            slots.Clear();
            for (int i = 0; i < _draftPicks.Count; i++)
            {
                int spellIndex = _draftPicks[i];
                int charges = 1;
                if (hasCatalog && spellIndex >= 0 && spellIndex < catalog.Length)
                    charges = math.max(1, catalog[spellIndex].Charges);

                slots.Add(new SpellSlot
                {
                    SpellIndex = spellIndex,
                    Cooldown = 0f,        // freshly drafted spells start ready (§13 2.4)
                    ChargesLeft = charges // seeded from DATA (SpellCatalogEntry.Charges)
                });
            }

            SetPlayerDraftDone(em);
            _draftConfirmed = true;
            _armedSlot = -1;
            return true;
        }

        /// <summary>True once the player has confirmed a valid draft this session (HUD gate).</summary>
        public bool DraftConfirmed => _draftConfirmed;

        // =====================================================================================
        // IN-BATTLE ARM + CAST (§5.3) — append a SpellCastRequest (data only)
        // =====================================================================================

        /// <summary>
        /// ARM one of the 3 drafted slots for casting (slotIndex 0..draftSize-1). Pure UI state —
        /// no sim write. A subsequent <see cref="CastAt"/>/<see cref="CastAtScreen"/> uses it.
        /// Out-of-range disarms (-1). Returns the armed slot index.
        /// </summary>
        public int ArmSlot(int slotIndex)
        {
            _armedSlot = (slotIndex >= 0 && slotIndex < draftSize) ? slotIndex : -1;
            return _armedSlot;
        }

        /// <summary>Currently armed slot index (-1 = none). Read-only view for the HUD.</summary>
        public int ArmedSlot => _armedSlot;

        /// <summary>
        /// UI-only readiness check for the armed slot (off cooldown && charges>0), so the HUD can
        /// grey out a not-ready spell. NOT authoritative — SpellCastSystem re-validates on consume.
        /// Returns false if nothing is armed or the loadout isn't baked yet.
        /// </summary>
        public bool ArmedSlotReady()
        {
            var em = EM;
            if (!em.World.IsCreated || _armedSlot < 0) return false;
            if (!TryGetPlayerLoadout(em, out Entity loadoutEntity) ||
                !em.HasBuffer<SpellSlot>(loadoutEntity)) return false;
            var slots = em.GetBuffer<SpellSlot>(loadoutEntity);
            if (_armedSlot >= slots.Length) return false;
            SpellSlot s = slots[_armedSlot];
            return s.Cooldown <= 0f && s.ChargesLeft > 0;
        }

        /// <summary>
        /// CAST the armed slot at a screen point (tap/aim). Projects screen→world via the camera,
        /// resolves an optional target unit under the point, then appends a SpellCastRequest to the
        /// shared inbox. Returns true if a request was appended (NOT whether the cast succeeds —
        /// the sim validates and may reject if the slot went on cooldown / out of charges). After a
        /// successful append the slot is DISARMED so a single tap = a single request.
        /// </summary>
        public bool CastAtScreen(Vector2 screen)
        {
            if (battleCamera == null) return false;
            float2 world = WorldPoint(screen);
            Entity target = PickUnit(world); // nearest unit under the tap (Entity.Null if none)
            return CastAt(world, target);
        }

        /// <summary>
        /// CAST the armed slot at a world position with an optional explicit target entity (the sim
        /// chooses whether the spell uses the point or the entity per its TargetShape). Appends a
        /// SpellCastRequest to the SpellCastInboxTag inbox — the SAME inbox SquadAI uses, so player
        /// and AI casts share one counterable path (§5.3). Data-write only (§12). Returns true if
        /// a request was appended. Disarms the slot on success.
        /// </summary>
        public bool CastAt(float2 worldPos, Entity target)
        {
            if (_armedSlot < 0) return false;

            var em = EM;
            if (!em.World.IsCreated) return false;

            // Locate the shared cast inbox (a singleton baked at battle setup). Without it there is
            // nowhere to write the request — fail closed (the sim owns the inbox lifetime).
            if (!em.CreateEntityQuery(ComponentType.ReadOnly<SpellCastInboxTag>())
                   .TryGetSingletonEntity<SpellCastInboxTag>(out var inboxEntity))
                return false;
            if (!em.HasBuffer<SpellCastRequest>(inboxEntity)) return false;

            DynamicBuffer<SpellCastRequest> inbox = em.GetBuffer<SpellCastRequest>(inboxEntity);
            inbox.Add(new SpellCastRequest
            {
                Team = playerTeam,
                SlotIndex = _armedSlot,
                TargetPos = worldPos,
                TargetEntity = target,
            });

            _armedSlot = -1; // one tap → one request; HUD re-arms for the next cast.
            return true;
        }

        // =====================================================================================
        // sim lookups (read-only / DATA writes; no gameplay logic)
        // =====================================================================================

        /// <summary>Locate the PLAYER side's SpellLoadoutTag singleton-per-side that holds its SpellSlot buffer.</summary>
        private bool TryGetPlayerLoadout(EntityManager em, out Entity loadoutEntity)
        {
            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<SpellLoadoutTag>(),
                ComponentType.ReadWrite<SpellSlot>());
            using var ents = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < ents.Length; i++)
            {
                if (em.GetComponentData<SpellLoadoutTag>(ents[i]).Team != playerTeam) continue;
                loadoutEntity = ents[i];
                return true;
            }
            loadoutEntity = Entity.Null;
            return false;
        }

        /// <summary>Set DraftState.PlayerDone = 1 (the §13 2.4 draft gate the sim's start logic reads).</summary>
        private static void SetPlayerDraftDone(EntityManager em)
        {
            var query = em.CreateEntityQuery(ComponentType.ReadWrite<DraftState>());
            if (!query.TryGetSingletonEntity<DraftState>(out var e)) return;
            DraftState ds = em.GetComponentData<DraftState>(e);
            ds.PlayerDone = 1;
            em.SetComponentData(e, ds);
        }

        // =====================================================================================
        // picking / projection (identical pattern to BattleInput.cs)
        // =====================================================================================

        /// <summary>Project a screen point onto the z=0 plane of the 2D battlefield.</summary>
        private float2 WorldPoint(Vector2 screen)
        {
            Vector3 sp = new Vector3(screen.x, screen.y, -battleCamera.transform.position.z);
            Vector3 wp = battleCamera.ScreenToWorldPoint(sp);
            return new float2(wp.x, wp.y);
        }

        // Tap tolerance for picking a unit under a cast aim (world-space). DEFERRED feel (GATE-1).
        private const float k_TapPickRadius = 0.6f;

        /// <summary>Nearest UnitTag entity to the world point within the tap radius, or Entity.Null. O(units) on tap only.</summary>
        private Entity PickUnit(float2 worldPos)
        {
            var em = EM;
            if (!em.World.IsCreated) return Entity.Null;

            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<UnitTag>());
            using var ents = query.ToEntityArray(Allocator.Temp);

            Entity best = Entity.Null;
            float bestSq = k_TapPickRadius * k_TapPickRadius;
            for (int i = 0; i < ents.Length; i++)
            {
                float2 p = em.GetComponentData<Position>(ents[i]).Value;
                float d = math.distancesq(worldPos, p);
                if (d <= bestSq) { bestSq = d; best = ents[i]; }
            }
            return best;
        }
    }
}
