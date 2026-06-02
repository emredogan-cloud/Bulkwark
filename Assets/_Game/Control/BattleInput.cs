// BULWARK — Phase-1 CONTROL shell (1.3). Roadmap §13 P1.3 (squad select/command/drag + possess),
// §3 (PRESERVE: "direct unit control" — the agency hook), §4 ("Kept + extended to squad command").
//
// §12 ECS BOUNDARY (hard rule): this is a MonoBehaviour INPUT shell. It ONLY translates touch/pointer
// into sim DATA writes — it contains NO gameplay simulation. All consequences (movement integration,
// auto-targeting, combat) are resolved by ECS systems under Assets/_Game/Sim. Specifically this shell:
//   • SELECT  : tap a PlayerControlled unit, or drag a box, to build the current selection (Squad grouping).
//   • MOVE    : tap ground with a selection -> writes ManualOrder.MovePos + HasMove=1 on each selected unit.
//   • ATTACK  : tap an enemy unit with a selection -> writes ManualOrder.AttackTarget + HasAttack=1.
//   • POSSESS : long-press a single owned unit -> adds Possessed tag (one at a time). While possessed,
//               drag = continuous move order, tap enemy = manual attack override, otherwise the unit
//               keeps its smart auto-target (resolved in sim). Long-press again releases possession.
// The consuming sim system is Assets/_Game/Sim/Systems/PossessControl.cs (it reads ManualOrder/Possessed).
//
// CANON DISCIPLINE:
//   • No new mechanics/currencies/units invented (§15). Selection/possession use existing Phase-1 components.
//   • No later-phase features: no formations, no spells, no commander abilities, no progression here.
//   • Player is Team 0 (PlayerControlled); we only ever select/command/possess PlayerControlled units.
//   • NEVER log save state (§12). This shell logs nothing sensitive.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities here — ADR-0-001/-002).
// DEFERRED: touch feel / drag thresholds / long-press timing are GATE-1, on-device validation only.
//
// asmdef note: Unity.Collections is referenced EXPLICITLY (Bulwark.Control.asmdef) for
// Allocator/NativeArray/ToEntityArray rather than relying on transitive re-exposure via Unity.Entities.

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Bulwark.Sim;

namespace Bulwark.Control
{
    /// <summary>
    /// Thin pointer/touch input shell for the Phase-1 control model (§13 P1.3).
    /// Translates input into ManualOrder / selection / Possessed writes on PlayerControlled sim units.
    /// Attach to a GameObject in the battle scene and assign <see cref="battleCamera"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleInput : MonoBehaviour
    {
        [Header("Scene wiring")]
        [Tooltip("Camera used for screen->world projection of the 2D battlefield. Required.")]
        [SerializeField] private Camera battleCamera;

        [Tooltip("Local player team id. Canon: Player = Team 0 (PlayerControlled).")]
        [SerializeField] private int playerTeam = 0;

        [Header("Touch feel (DEFERRED — tune on device at GATE 1)")]
        [Tooltip("Seconds a press must be held (without dragging) to toggle possession.")]
        [SerializeField] private float longPressSeconds = 0.45f;

        [Tooltip("Screen-pixel movement past which a press becomes a drag (cancels long-press / starts box).")]
        [SerializeField] private float dragThresholdPixels = 12f;

        [Tooltip("World-space radius for tap-to-pick a unit (screen tap tolerance projected to world).")]
        [SerializeField] private float tapPickRadius = 0.6f;

        [Tooltip("Squad id assigned to a freshly created selection. Phase-1 uses a single working squad.")]
        [SerializeField] private int workingSquadId = 1;

        // ---- runtime state (input bookkeeping only; NO sim state lives here) ----
        private readonly List<Entity> _selection = new List<Entity>(64);
        private Entity _possessed = Entity.Null;

        private bool _pressActive;
        private bool _pressIsDrag;
        private float _pressStartTime;
        private Vector2 _pressStartScreen;
        private Vector2 _pressCurrentScreen;
        private bool _longPressConsumed; // possession already toggled for the current press

        private EntityManager EM
        {
            get
            {
                var world = World.DefaultGameObjectInjectionWorld;
                return world != null && world.IsCreated ? world.EntityManager : default;
            }
        }

        private void Reset()
        {
            battleCamera = Camera.main;
        }

        private void Awake()
        {
            if (battleCamera == null) battleCamera = Camera.main;
        }

        private void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (battleCamera == null || world == null || !world.IsCreated)
                return; // nothing to write into yet (scene/world not ready)

            ReadPointer(out bool down, out bool held, out bool up, out Vector2 screen);

            if (down)
                BeginPress(screen);
            else if (held && _pressActive)
                UpdatePress(screen);
            else if (up && _pressActive)
                EndPress(screen);
        }

        // ---------------------------------------------------------------- pointer
        // Single-pointer abstraction over touch (preferred) and mouse (editor).
        private void ReadPointer(out bool down, out bool held, out bool up, out Vector2 screen)
        {
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                screen = t.position;
                down = t.phase == TouchPhase.Began;
                held = t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary;
                up = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
                return;
            }

            screen = Input.mousePosition;
            down = Input.GetMouseButtonDown(0);
            held = Input.GetMouseButton(0);
            up = Input.GetMouseButtonUp(0);
        }

        private void BeginPress(Vector2 screen)
        {
            _pressActive = true;
            _pressIsDrag = false;
            _longPressConsumed = false;
            _pressStartTime = Time.unscaledTime;
            _pressStartScreen = screen;
            _pressCurrentScreen = screen;
        }

        private void UpdatePress(Vector2 screen)
        {
            _pressCurrentScreen = screen;

            if (!_pressIsDrag &&
                Vector2.Distance(_pressStartScreen, screen) > dragThresholdPixels)
            {
                _pressIsDrag = true;
            }

            // While possessing: a drag continuously re-issues a move order to the possessed unit.
            if (_possessed != Entity.Null && EM.Exists(_possessed) && _pressIsDrag)
            {
                IssueMove(WorldPoint(screen), onlyPossessed: true);
                return;
            }

            // Not possessing: a hold-in-place past longPressSeconds toggles possession on the
            // unit under the original press point (one possessed unit at a time).
            if (!_pressIsDrag && !_longPressConsumed &&
                Time.unscaledTime - _pressStartTime >= longPressSeconds)
            {
                _longPressConsumed = true;
                TryTogglePossess(WorldPoint(_pressStartScreen));
            }
        }

        private void EndPress(Vector2 screen)
        {
            _pressActive = false;
            float2 world = WorldPoint(screen);

            // A long-press already handled this gesture (possession toggle) — consume the release.
            if (_longPressConsumed)
                return;

            if (_pressIsDrag)
            {
                // Possessed drag-move was streamed during UpdatePress; nothing to finalize there.
                if (_possessed != Entity.Null && EM.Exists(_possessed))
                {
                    IssueMove(world, onlyPossessed: true);
                    return;
                }

                // Otherwise a drag is a selection box over PlayerControlled units.
                SelectBox(_pressStartScreen, screen);
                return;
            }

            // ---- a clean TAP ----
            Entity enemy = PickUnit(world, friendly: false);
            if (enemy != Entity.Null)
            {
                IssueAttack(enemy, onlyPossessed: _possessed != Entity.Null);
                return;
            }

            Entity friendly = PickUnit(world, friendly: true);
            if (friendly != Entity.Null && _possessed == Entity.Null)
            {
                SelectSingle(friendly);
                return;
            }

            // Tap on empty ground = move order to the current selection (or possessed unit).
            IssueMove(world, onlyPossessed: _possessed != Entity.Null);
        }

        // ---------------------------------------------------------------- selection
        private void SelectSingle(Entity unit)
        {
            ClearSquadTags();
            _selection.Clear();
            _selection.Add(unit);
            TagSelectionSquad();
        }

        private void SelectBox(Vector2 aScreen, Vector2 bScreen)
        {
            var em = EM;
            if (!em.World.IsCreated) return;

            Rect box = Rect.MinMaxRect(
                Mathf.Min(aScreen.x, bScreen.x), Mathf.Min(aScreen.y, bScreen.y),
                Mathf.Max(aScreen.x, bScreen.x), Mathf.Max(aScreen.y, bScreen.y));

            ClearSquadTags();
            _selection.Clear();

            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<PlayerControlled>(),
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<UnitTag>());

            using var ents = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < ents.Length; i++)
            {
                Entity e = ents[i];
                if (em.GetComponentData<Team>(e).Id != playerTeam) continue;

                float2 wp = em.GetComponentData<Position>(e).Value;
                Vector3 sp = battleCamera.WorldToScreenPoint(new Vector3(wp.x, wp.y, 0f));
                if (box.Contains(new Vector2(sp.x, sp.y)))
                    _selection.Add(e);
            }

            TagSelectionSquad();
        }

        private void TagSelectionSquad()
        {
            var em = EM;
            for (int i = 0; i < _selection.Count; i++)
            {
                Entity e = _selection[i];
                if (!em.Exists(e)) continue;
                if (em.HasComponent<Squad>(e))
                    em.SetComponentData(e, new Squad { Id = workingSquadId });
                else
                    em.AddComponentData(e, new Squad { Id = workingSquadId });
            }
        }

        private void ClearSquadTags()
        {
            var em = EM;
            for (int i = 0; i < _selection.Count; i++)
            {
                Entity e = _selection[i];
                if (em.Exists(e) && em.HasComponent<Squad>(e))
                    em.SetComponentData(e, new Squad { Id = 0 }); // 0 = ungrouped
            }
        }

        // ---------------------------------------------------------------- orders (write ManualOrder)
        private void IssueMove(float2 worldPos, bool onlyPossessed)
        {
            var em = EM;
            if (onlyPossessed)
            {
                if (_possessed != Entity.Null && em.Exists(_possessed))
                    WriteMoveOrder(em, _possessed, worldPos);
                return;
            }

            for (int i = 0; i < _selection.Count; i++)
            {
                Entity e = _selection[i];
                if (em.Exists(e)) WriteMoveOrder(em, e, worldPos);
            }
        }

        private void IssueAttack(Entity enemy, bool onlyPossessed)
        {
            var em = EM;
            if (onlyPossessed)
            {
                if (_possessed != Entity.Null && em.Exists(_possessed))
                    WriteAttackOrder(em, _possessed, enemy);
                return;
            }

            for (int i = 0; i < _selection.Count; i++)
            {
                Entity e = _selection[i];
                if (em.Exists(e)) WriteAttackOrder(em, e, enemy);
            }
        }

        private static void WriteMoveOrder(EntityManager em, Entity unit, float2 pos)
        {
            ManualOrder order = em.HasComponent<ManualOrder>(unit)
                ? em.GetComponentData<ManualOrder>(unit)
                : default;
            order.MovePos = pos;
            order.HasMove = 1;
            SetOrAdd(em, unit, order);
        }

        private static void WriteAttackOrder(EntityManager em, Entity unit, Entity target)
        {
            ManualOrder order = em.HasComponent<ManualOrder>(unit)
                ? em.GetComponentData<ManualOrder>(unit)
                : default;
            order.AttackTarget = target;
            order.HasAttack = 1;
            SetOrAdd(em, unit, order);
        }

        private static void SetOrAdd(EntityManager em, Entity unit, ManualOrder order)
        {
            if (em.HasComponent<ManualOrder>(unit)) em.SetComponentData(unit, order);
            else em.AddComponentData(unit, order);
        }

        // ---------------------------------------------------------------- possession (Possessed tag)
        private void TryTogglePossess(float2 worldPos)
        {
            var em = EM;

            // Already possessing: long-press anywhere releases.
            if (_possessed != Entity.Null)
            {
                if (em.Exists(_possessed) && em.HasComponent<Possessed>(_possessed))
                    em.RemoveComponent<Possessed>(_possessed);
                _possessed = Entity.Null;
                return;
            }

            Entity unit = PickUnit(worldPos, friendly: true);
            if (unit == Entity.Null) return;

            // Possess exactly one owned unit (defensively clear any stray possession first).
            ClearAllPossession(em);
            if (!em.HasComponent<Possessed>(unit))
                em.AddComponent<Possessed>(unit);
            _possessed = unit;

            // Focus the selection on the possessed unit for clarity.
            SelectSingle(unit);
        }

        private void ClearAllPossession(EntityManager em)
        {
            if (!em.World.IsCreated) return;
            var query = em.CreateEntityQuery(ComponentType.ReadOnly<Possessed>());
            using var ents = query.ToEntityArray(Allocator.Temp);
            for (int i = 0; i < ents.Length; i++)
                em.RemoveComponent<Possessed>(ents[i]);
        }

        // ---------------------------------------------------------------- picking / projection
        private float2 WorldPoint(Vector2 screen)
        {
            // 2D battlefield: project the screen ray onto the z=0 plane.
            Vector3 sp = new Vector3(screen.x, screen.y, -battleCamera.transform.position.z);
            Vector3 wp = battleCamera.ScreenToWorldPoint(sp);
            return new float2(wp.x, wp.y);
        }

        /// <summary>Nearest unit (friendly or enemy) to the world point within tapPickRadius. O(picked-frame) only.</summary>
        private Entity PickUnit(float2 worldPos, bool friendly)
        {
            var em = EM;
            if (!em.World.IsCreated) return Entity.Null;

            var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<Position>(),
                ComponentType.ReadOnly<Team>(),
                ComponentType.ReadOnly<UnitTag>(),
                ComponentType.ReadOnly<Health>());

            using var ents = query.ToEntityArray(Allocator.Temp);

            Entity best = Entity.Null;
            float bestSq = tapPickRadius * tapPickRadius;
            for (int i = 0; i < ents.Length; i++)
            {
                Entity e = ents[i];
                bool isFriendly = em.GetComponentData<Team>(e).Id == playerTeam;
                if (isFriendly != friendly) continue;

                float2 p = em.GetComponentData<Position>(e).Value;
                float d = math.distancesq(worldPos, p);
                if (d <= bestSq) { bestSq = d; best = e; }
            }
            return best;
        }

        // ---------------------------------------------------------------- read accessors (for HUD/debug)
        /// <summary>Currently possessed unit, or Entity.Null. Read-only view for UI; no sim state here.</summary>
        public Entity PossessedUnit => _possessed;

        /// <summary>Count of currently selected owned units. Read-only view for UI.</summary>
        public int SelectionCount => _selection.Count;
    }
}
