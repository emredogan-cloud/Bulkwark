// BULWARK — Phase 1 INFLUENCE MAP (roadmap §13 P1.4 "influence-map targeting"; §4
// "Layered AI (... influence-map units ... budgeted scheduler)"; §12 perf rule:
// targeting/AI must stay ~O(1) per unit — NO O(N^2) all-pairs scans).
//
// Builds a coarse per-team THREAT FIELD over the single horizontal front:
//   cells = X-bins × 3 rows (§11 "one horizontal front, 3 rows"), per team.
// Rebuilt at ~4 Hz (accumulate dt, rebuild every 0.25s) so cost is bounded and
// amortized; Targeting/AI read this instead of scanning all enemies pairwise.
//
// §12 ECS boundary: this is pure battle-sim (ISystem/Burst); no UI/meta logic.
// §15.6 no invented balance: contains NO unit/combat numeric stats — only a spatial
// bucketing of existing Position/Team/Health/AttackState data. The world X-span is
// derived from authored TeamSpawnPoint markers (data), not hard-coded geometry.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Bulwark.Sim
{
    // TeamSpawnPoint (the authored per-side spawn marker that also defines the two ends of the
    // single front for X-span binning, §11/§15.6) is declared ONCE in SimSystemOrder.cs (the
    // de-facto shared home for cross-cutting spawn/catalog types). InfluenceMapSystem CONSUMES
    // it below (ResolveSpan). (Reconciliation: removed the duplicate, field-identical declaration
    // that used to live here — a component type may be declared only once.)

    /// <summary>
    /// Singleton holding the rebuilt threat field + the binning frame so consumers
    /// (Targeting, BasicAI) can map a world Position to a cell deterministically.
    /// Threat[ team*(XBins*Rows) + row*XBins + xbin ] = summed per-unit threat for that team.
    /// The NativeArray is Persistent and owned by InfluenceMapSystem (disposed in OnDestroy).
    /// </summary>
    public struct InfluenceMapSingleton : IComponentData
    {
        public NativeArray<float> Threat; // length = Teams * XBins * Rows
        public int XBins;
        public int Rows;   // 3 (single front)
        public int Teams;  // 2 (Player=0, AI=1)
        public float MinX; // left edge of the front (world X)
        public float MaxX; // right edge of the front (world X)
        public byte Built; // 0 until first rebuild completes

        /// <summary>Map a world X to an X-bin index, clamped into [0, XBins-1].</summary>
        public int XBinOf(float x)
        {
            float span = math.max(MaxX - MinX, 1e-3f);
            float t = (x - MinX) / span;
            int b = (int)math.floor(t * XBins);
            return math.clamp(b, 0, XBins - 1);
        }

        /// <summary>Clamp an arbitrary row to [0, Rows-1] (defensive; rows are authored 0..2).</summary>
        public int RowClamp(int row) => math.clamp(row, 0, Rows - 1);

        /// <summary>Linear cell index for (team,row,xbin). Caller guarantees in-range inputs.</summary>
        public int CellIndex(int team, int row, int xbin)
            => team * (XBins * Rows) + row * XBins + xbin;

        /// <summary>Read accumulated threat a team projects into a (row, xbin) cell. 0 if not built.</summary>
        public float ThreatAt(int team, int row, int xbin)
        {
            if (Built == 0 || !Threat.IsCreated) return 0f;
            if ((uint)team >= (uint)Teams) return 0f;
            row = RowClamp(row);
            xbin = math.clamp(xbin, 0, XBins - 1);
            return Threat[CellIndex(team, row, xbin)];
        }
    }

    /// <summary>
    /// Rebuilds the per-team threat field at ~4 Hz. Cost is O(N) over live units once per
    /// rebuild tick (single pass), NOT O(N^2). Per-unit "threat" is a neutral presence weight
    /// scaled by current health fraction — it is a SPATIAL aggregate of existing sim state, not
    /// a balance value (§15.6). Updates in the sim group before TargetingSystem.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SimPhaseBegin))]
    [UpdateBefore(typeof(SimPhaseEnd))]
    [UpdateAfter(typeof(TrainingSystem))]   // slot 2 → 3: freshly-spawned units exist before the influence pass.
    [UpdateBefore(typeof(TargetingSystem))] // Targeting reads this tick's rebuilt buckets.
    public partial struct InfluenceMapSystem : ISystem
    {
        // Binning constants. These are GRID RESOLUTION (engine cost knobs), not gameplay
        // balance — they tune the influence map's granularity / rebuild cost, nothing combat reads.
        private const int Rows = 3;          // single front, 3 rows (§11)
        private const int Teams = 2;         // Player=0, AI=1
        private const int XBins = 32;        // front resolution along X
        private const float RebuildPeriod = 0.25f; // ~4 Hz rebuild (bounded cost)
        private const float DefaultMinX = -50f;    // fallback front span if no spawn points authored yet
        private const float DefaultMaxX = 50f;

        private float _accum;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var arr = new NativeArray<float>(Teams * XBins * Rows, Allocator.Persistent,
                                             NativeArrayOptions.ClearMemory);

            var singletonEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(singletonEntity, new InfluenceMapSingleton
            {
                Threat = arr,
                XBins = XBins,
                Rows = Rows,
                Teams = Teams,
                MinX = DefaultMinX,
                MaxX = DefaultMaxX,
                Built = 0
            });

            _accum = RebuildPeriod; // force a rebuild on the first frame
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<InfluenceMapSingleton>(out var s) && s.Threat.IsCreated)
                s.Threat.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _accum += SystemAPI.Time.DeltaTime;
            if (_accum < RebuildPeriod)
                return; // skip — ~4 Hz throttle keeps per-frame cost bounded
            _accum = 0f;

            // SystemAPI.GetSingletonRW gives us write access to the persistent array in place.
            var imRef = SystemAPI.GetSingletonRW<InfluenceMapSingleton>();
            ref var im = ref imRef.ValueRW;
            if (!im.Threat.IsCreated)
                return;

            // Refresh the front span from authored spawn points (if present) so binning tracks data.
            ResolveSpan(ref state, ref im);

            // Clear the field, then single O(N) accumulation pass.
            var threat = im.Threat;
            for (int i = 0; i < threat.Length; i++)
                threat[i] = 0f;

            foreach (var (pos, team, health, row) in
                     SystemAPI.Query<RefRO<Position>, RefRO<Team>, RefRO<Health>, RefRO<Row>>()
                              .WithAll<UnitTag>())
            {
                int t = team.ValueRO.Id;
                if ((uint)t >= (uint)Teams) continue; // only the two battle sides feed the map

                int xbin = im.XBinOf(pos.ValueRO.Value.x);
                int r = im.RowClamp(row.ValueRO.Index);

                // Neutral presence weight × current health fraction. NOT a balance number:
                // it is a spatial aggregate of existing Health (a contestation/density signal).
                float maxHp = math.max(health.ValueRO.Max, 1e-3f);
                float frac = math.clamp(health.ValueRO.Current / maxHp, 0f, 1f);
                threat[im.CellIndex(t, r, xbin)] += frac;
            }

            im.Built = 1;
        }

        /// <summary>
        /// Derive [MinX,MaxX] from the two TeamSpawnPoint markers when authored; otherwise keep
        /// the previous/default span. Bounded scan over the (few) spawn markers — not over units.
        /// </summary>
        private void ResolveSpan(ref SystemState state, ref InfluenceMapSingleton im)
        {
            float minX = float.MaxValue, maxX = float.MinValue;
            bool any = false;
            foreach (var sp in SystemAPI.Query<RefRO<TeamSpawnPoint>>())
            {
                float x = sp.ValueRO.Pos.x;
                minX = math.min(minX, x);
                maxX = math.max(maxX, x);
                any = true;
            }

            if (any && maxX > minX)
            {
                // Pad slightly so units spawned exactly at an end still land inside the grid.
                float pad = math.max((maxX - minX) * 0.05f, 1f);
                im.MinX = minX - pad;
                im.MaxX = maxX + pad;
            }
            else if (im.MinX >= im.MaxX)
            {
                im.MinX = DefaultMinX;
                im.MaxX = DefaultMaxX;
            }
        }
    }
}
