// BULWARK — ECS authoring/baker (Phase 0.2 scaffold).
// Bridges editor-authored MonoBehaviour data into runtime ECS components.
// Stat values are baked FROM data (UnitDef), resolved through the 3-tier config
// resolver (RC → SO → literal, §9/§12) — never hard-coded in the sim.
// SCAFFOLD STATUS: authored, NOT compiled (no Unity 6 / Entities — ADR-0-001).

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Bulwark.Sim.Authoring
{
    /// <summary>
    /// Authoring component placed in a subscene; baked into the sim entity.
    /// References a UnitDef (the data schema, content-empty at P0).
    /// </summary>
    public class UnitAuthoring : MonoBehaviour
    {
        public int teamId;
        public Vector2 spawnPosition;
        // public UnitDef unit;  // wired in Phase 1 when content/data populate; schema-only at P0.

        public class Baker : Baker<UnitAuthoring>
        {
            public override void Bake(UnitAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<UnitTag>(e);
                AddComponent(e, new Team { Id = authoring.teamId });
                AddComponent(e, new Position { Value = new float2(authoring.spawnPosition.x, authoring.spawnPosition.y) });
                // Health/Attack/Movement stats are populated from resolved UnitDef in Phase 1.
                AddComponent(e, new Health { Current = 0f, Max = 0f });
                AddComponent(e, new Movement { Speed = 0f });
                AddComponent(e, new AttackState { Target = Entity.Null });
            }
        }
    }
}
