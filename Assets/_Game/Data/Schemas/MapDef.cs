// BULWARK — MapDef ScriptableObject schema (Data layer). Phase 2 §13 2.7 + §11.
// A map = one battlefield archetype expressed as a single horizontal front (3 rows, §11)
// with 2–4 terrain features (high ground / choke / cover / hazard), statues at the ends,
// and contestable miner-capped mine nodes. MVP ships 3 such layouts (§5.4). No biomes
// (Phase 7). All positions/values PROVISIONAL / LSD-owned (§16).
// SCAFFOLD STATUS: authored, NOT compiled (no Unity — ADR-0-001/-002).

using System.Collections.Generic;
using UnityEngine;

namespace Bulwark.Data
{
    public enum TerrainKind : int
    {
        None = 0, HighGround = 1, Choke = 2, Cover = 3, Hazard = 4, // §4/§11 the four terrain features
    }

    [System.Serializable]
    public struct TerrainPlacement
    {
        public TerrainKind kind;
        public float x;     // position along the front
        public int row;     // 0..2 (§11)
        public float width; // extent along the front
    }

    [System.Serializable]
    public struct MinePlacement
    {
        public float x;
        public int capacity;     // miner cap (§11)
        public float yieldPerSec;
        public int ownerTeam;    // -1 = contestable/neutral
    }

    [CreateAssetMenu(fileName = "MapDef", menuName = "BULWARK/Data/MapDef", order = 4)]
    public class MapDef : ScriptableObject
    {
        [Header("Identity (§5.4)")]
        public string id;
        public string displayName;

        [Header("Layout (§11 — single front, 3 rows)")]
        public float frontLength = 40f;
        public int rows = 3;
        public float team0StatueX = 0f;   // player statue at the left end
        public float team1StatueX = 40f;  // AI statue at the right end

        [Header("Features (2–4 per §11) + mine nodes — PROVISIONAL")]
        public List<TerrainPlacement> terrain = new List<TerrainPlacement>();
        public List<MinePlacement> mines = new List<MinePlacement>();
    }
}
