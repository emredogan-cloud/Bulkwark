# BULWARK UI Implementation Kit (for screen-building agents)

You implement ONE Unity uGUI screen, code-built, from its Construction-Bible spec, on the existing `UiRouter`
shell. Match the established foundation EXACTLY. Presentation-only — **§12: NO ECS, NO `Unity.Entities`, NO
gameplay/balance/AI/economy/backend.** Only use the APIs below + `UnityEngine` / `UnityEngine.UI`.

## File shape
```csharp
// BULWARK — <NAME> (UI Construction Bible · NN). Presentation-only, REMOVABLE.
// <one-paragraph forensic summary citing the spec sections>
using UnityEngine;
using UnityEngine.UI;
namespace Bulwark.Bootstrap
{
    public sealed class <Name>Screen : UiScreen
    {
        protected override void Build() { /* build here */ }
        public override void OnShow() => AudioManager.Instance?.PlayMenuMusic();
        private static Color Hex(string h){ ColorUtility.TryParseHtmlString(h, out var c); return c; }
    }
}
```
Class name MUST be exactly the one given in your task. No asmdef/.meta. Output the COMPLETE .cs file only.

## UiScreen (base) — you extend this
- `protected override void Build()` — build the screen here (called once). 
- `public override void OnShow()/OnHide()` — optional lifecycle.
- `protected RectTransform Rect` — FULL-BLEED root. Parent backgrounds + FX (vignette/glow/embers/scrims) here.
- `protected RectTransform SafeContent` — safe-area inset. Parent ALL interactive/important content here.
- `protected UiRouter Router` — navigation. `Group` = CanvasGroup.
- Do setup in `Build()`, NOT a constructor/Awake (Router/Rect/SafeContent bind right before Build).

## Navigation (Router)
`Router.Show<XScreen>()` push · `Router.Replace<XScreen>()` · `Router.Pop()` · `Router.PopFaded()` ·
`Router.Toast("msg")`. For a destination screen that may not exist yet, use `Router.Toast("X — coming soon")`.
Back button: `UiWidgets.BackButton(SafeContent, () => Router.Pop());`

## Layout convention (CanvasScaler 2340×1080, match height)
- Point-anchor a widget by passing the SAME vector for aMin and aMax, e.g. `new Vector2(0.5f, 0.74f)`.
- Spec gives `fy` measured from the TOP. Convert to Unity anchorY: **anchorY = 1 − fy**.
- Spec fractions → px: `x = frac * 2340`, `y = frac * 1080`. Sizes are in these px units.
- `pos` (anchoredPosition) offsets from the anchor; usually `Vector2.zero` when point-anchored.

## UiTheme (exact Bible palette) — `UiTheme.<Name>`
Obsidian, Charcoal, FieldDark, FieldTop, ChannelDark, ChannelMid, Vignette,
GoldHi(#f0d27a), Gold(#caa04a), GoldShadow, GoldFillHi, GoldFill, GoldFillLo, Parchment(#d9c79a), ParchGold, Ember,
IronBlue(#2b56c8), IronBlueHi(#4f8bff), IronSteel, IronBanner,
Oxblood(#7a1f1a), Ember2(#d8452b), AshBanner,
Amethyst(#5a2db0), AmethystHi(#9e6bf0), StrokeDark.
Helpers: `UiTheme.A(color, alpha)` set alpha; `UiTheme.Track("TEXT")` letter-spaces a string for wide UPPERCASE titles.
Type sizes: `UiTheme.Display/H1/H2/H3/Body/Small/Tiny` (ints). For any other exact hex use the `Hex("#rrggbb")` helper.

## UiWidgets (static builders) — preferred building blocks
- `RectTransform Rect(name, parent, aMin, aMax, pos, size)` — empty child.
- `Image Stretch(name, parent, col, spriteKey=null)` — full-bleed image (bg). spriteKeys: "bg_menu","bg_battle","bg_victory","bg_defeat","panel","button","gold" (else null→solid col).
- `Image Panel(parent, aMin, aMax, pos, size, col)` — 9-slice panel.
- `RectTransform Card(parent, aMin, aMax, pos, size, fill=null)` — dark glass card + gold hairline. Parent content inside.
- `RectTransform OrnateFrame(parent, aMin, aMax, pos, size, fieldColor=null, finials=true, inset=20)` — ornate gold frame; RETURNS the inner field rect (parent content inside it).
- `Text Label(parent, text, size, aMin, aMax, pos, sz, align, col)` — shadowed label (non-raycast).
- `Text LabelAt(parent, text, size, anchor, sz, align, col)` — centered label at one anchor.
- `Text TitleLabel(parent, text, size, aMin, aMax, pos, sz, align, top=null, bottom=null, track=true)` — PRESTIGE gold-gradient UPPERCASE title (use for all gold serif titles). Pass top/bottom Colors to recolor the gradient.
- `Text SectionHeader(parent, text, aMin, aMax, pos, size)` — left gold header.
- `Button Button(parent, text, aMin, aMax, pos, size, tint, onClick, fontSize=44)` — textured button.
- `Button GemButton(parent, label, aMin, aMax, pos, size, body, onClick, fontSize=40, glow=false, leadingIcon=true, labelAlign=TextAnchor.MiddleLeft)` — glossy gold-rimmed gem button (primary CTAs).
- `Button BackButton(parent, onClick)` — top-left back.
- `Button IconTile(parent, caption, aMin, aMax, pos, diameter, ring, onClick, badge=false, badgeCount=0)` — round icon + caption + optional red badge (rails / shortcuts).
- `Text CurrencyChip(parent, iconColor, value, chipIndex, out Image icon)` — top-right currency pill (chipIndex 0 = rightmost). Use UiTheme.Gold / UiTheme.AmethystHi for gold/gems.
- `Image Vignette(parent, strength=0.55f)` — full-bleed radial vignette (add to Rect).
- `Image Glow(parent, col, aMin, aMax, pos, size, power=1.8f)` — soft radial glow (god-rays/fire/focal bloom).
- `Image Finial(parent, anchor, pos, size=40f)` — gold diamond ornament.
- `Image Divider(parent, aMin, aMax, pos, width, thickness=3f, col=null)` — gold rule.
- `Button[] TabBar(parent, string[] tabs, aMin, aMax, pos, size, active, System.Action<int> onSelect)` — horizontal tabs (active tab gold).
- `void StarRating(parent, aMin, aMax, pos, total, filled, starSize=48f, gap=8f)` — gold star pips.
- `Image ProgressBar(parent, aMin, aMax, pos, size, fillCol, amount)` — simple fill bar.
- `BarParts GoldBar(parent, aMin, aMax, pos, size, amount, caps=true, tip=true)` — ornate gold bar (`.Fill` Image). `UiWidgets.UpdateBarTip(parts)` after changing fillAmount.
- `void NotifyBadge(parent, count=0)` — red badge top-right of parent.
- `Color Lighten(c,t)` / `Color Darken(c,t)`.

## UiTex (procedural sprites) — when you need a raw sprite
`UiTex.Solid(col)`, `VGradient(top,bottom,h=64)`, `HGradient(left,right,w=64)`, `Radial(inner,outer,size=128,power=1.6f)`,
`Frame(hi,mid,shadow,size=64,border=12)` (9-slice border), `Diamond(col,size=48)`, `Disc(col,size=48)`.
Use as `someImage.sprite = UiTex.VGradient(a,b);` (set `.type = Image.Type.Sliced` for Frame).

## UiFx (components — AddComponent then set fields). All run on UNSCALED time.
- `PulseGraphic{ Graphic target; float min,max,period }` — pulse alpha (CTA/glow). `var p=go.AddComponent<PulseGraphic>(); p.target=img;`
- `PulseScale{ float min,max,period }` — gentle breathing scale.
- `KenBurns{ float from,to,duration }` — slow bg zoom (add to a bg Image's GameObject).
- `Spin{ float degPerSec }` — constant rotation (wheels/reticles).
- `EmberField{ int count; Color color }` — drifting embers (add to a Rect over the lower frame).
- `Sheen` / `CountUp` — advanced; CountUp: `var c=go.AddComponent<CountUp>(); c.Bind(text, v=>v.ToString("N0"), 0); c.To(target, seconds);`
- `UiGradientText` — gold vertical gradient on a Text: `var g=text.gameObject.AddComponent<UiGradientText>(); g.top=UiTheme.GoldHi; g.bottom=UiTheme.Gold;` (TitleLabel already does this).

## UiStub (display-only data — never gameplay)
`UiStub.Gold` (int), `UiStub.Gems` (int), `UiStub.TrySpendGems(n)`, `UiStub.GrantGems(n)`, `UiStub.GrantGold(n)`.
`UiStub.SeasonName`, `PassTier`, `PassXp`, `PassXpPerTier`, `PassTierCount`, `PassPremiumOwned`, `PassPremiumPriceGems`.
`UiStub.DailyQuests` (array of `UiStub.Quest{ string Title; int Progress,Target; bool Gem; int Reward; bool Claimed }`).
`UiStub.GemPacks` (array of `UiStub.GemPack{ string Name; int Gems; string Price; bool Best }`).
Commanders: `UiStub.WardenName/WardenTitle/WardenActive/WardenActiveDesc/WardenPassive/WardenPassiveDesc/WardenLevel`
and `Warchief*` equivalents; `UiStub.SelectedCommander`.
For data the stub lacks, define local arrays/consts INSIDE your screen (clearly display-only) — never invent economy values that imply balance.

## Audio
`AudioManager.Instance?.Click();` (buttons already click via the builders), `?.PlayMenuMusic()`.

## Hard rules
- Reproduce the spec's node tree, layout math, exact hex, typography sizes, components/states, animation (use UiFx), particle/FX, and honor every NEGATIVE RULE + ACCEPTANCE CRITERION.
- Full-bleed bg + FX → `Rect`; interactive/important content → `SafeContent`.
- NO `using Unity.Entities;` / NO ECS / NO new MonoBehaviour types / NO prefabs / NO `Resources.Load` of art.
- Don't run the compile checker; don't edit other files; output only your one .cs file's full content.
- Keep within the documented signatures EXACTLY (wrong arg counts = compile failure).
```
