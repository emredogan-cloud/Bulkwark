# BULWARK — Büyü Sinerji & Karşıtlık Ağı Tasarım Raporu (Gelecek Araştırma)

> **⚠️ STATÜ: GELECEK ARAŞTIRMA İZİ — YALNIZCA TAVSİYE NİTELİĞİNDE.**
> Bu belge **aktif geliştirme akışının parçası DEĞİLDİR.** Aktif akış şudur: **CI/CD doğrulama · APK üretimi · Unity doğrulama · kalan Phase 0–3 kapı borç-eritimi.**
> **Bu belge HİÇBİR ŞEYİ:** roadmap'i değiştiremez · kanonu değiştiremez · karar günlüğünü (decision log) değiştiremez · gelecek özellik yetkilendiremez · üretim önceliklerini değiştiremez · implementasyon başlatamaz · güç bütçelerini değiştiremez · onaylanmamış mekanik ekleyemez.
> **Bu belge YALNIZCA:** keşfeder · analiz eder · tavsiye eder · değerlendirir.
> **Konum:** `future/002-spell-synergy-web/` (canon dökümanlarına dokunulmadı; hiçbir `report/`, `docs/adr/`, `decision log`, `docs/execution/` dosyası değiştirilmedi).
> **Kanonik yuva:** Bu araştırma, **Decision Log §4 — "Spell pool: *Vision = deep synergy web*; MVP = ~12, draft 3"** ile **Blueprint §2 — "Post-launch (S1–S3): +pool, synergies → Full vision: deep synergy web"** hedefini önceden tasarlar. Bu, **post-launch içerik havuz büyümesidir** ve roadmap §15 (kanon kapalı) gereği **her yeni büyü/kategori bir ADR ile** açılmalıdır. Tetikleyici henüz ateşlenmemiştir → hiçbir şey implemente edilmez.
> **Tarih:** 2026-06-03 · **Dil:** Türkçe · **Kalite çıtası:** Lead RTS Designer + Combat Designer + Systems Designer + LiveOps Designer.

---

# Proje Derin Analizi

*(ZORUNLU ön-araştırma fazı — atlanmadı. Aşağıdaki rekonstrüksiyon şu kaynakların okunmasına dayanır: 5 kanonik döküman [`BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `ROADMAP_CHANGELOG.md`, `PRODUCTION_DECISION_LOG.md`, `NEXTGEN_RTS_SUCCESSOR_REPORT.md`, `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`] · 5 ADR [0-001, 0-002, 1-001, 2-001, 2-002] · 8 execution-prompt [Phase 0–7 + sistem] · Phase 0–3 implementasyon kodu [`Spell.cs`, `CommanderAbility.cs`, `Phase2Components.cs`, `CombatTypes.cs`, `SpellDef.cs`, `CommanderDef.cs`, `BalanceConfig.cs` + 12 `Data/Spells/*.asset`] · 4 faz-tamamlanma raporu · `FIRST_COMPILE_REPORT.md` · `SCAFFOLDING_STATUS.md` · `FormationMember_wiring_plan.md`.)*

## 1.1 Mevcut proje durumu (rekonstrüksiyon)

BULWARK, *Stick War* halefi olan mobil-öncelikli (Android/iOS), tek-cepheli, **doğrudan-kontrollü taktiksel RTS-lite** (Unity 6 LTS · IL2CPP · URP-2D · **ECS/DOTS battle-sim**). Çekirdek döngü kanon olarak korunur: **maden → eğit → ittir → heykeli yık** (§3). Üretim felsefesi: on yıl kanıtlanmış çekirdeği koru; orijinali sınırlayan **iki** şeyi modernize et — (a) sığ savaş alanı → *terrain + formasyon + type×armor counter + positional flank + draft-3 büyü sinerjisi*; (b) kırılgan client-trust ekonomi → *server-authoritative*. Monetizasyon **etik, kozmetik + battle-pass öncülüğünde**; **asla güç satmaz** (§2, §9, §10).

| Eksen | Kanonik değer |
|---|---|
| **Tür** | Doğrudan-kontrollü taktiksel RTS-lite — tek-cephe; PvE + async rekabet |
| **Sütunlar (Pillars)** | P1 Agency · P2 Okunabilir derinlik · P3 Adil ustalık · P4 Oyuncuya saygı |
| **İhlal-edilemez kısıtlar** | Okunabilirlik · adalet/no-P2W · para-birimi server-otoritesi · save-state log'lamama · perf bütçesi · §15 CUT listesi |
| **MVP içerik kanonu** | 2 fraksiyon · 12 birim (6+6) · **2 komutan (1/fraksiyon)** · **~12 büyü (draft 3)** · 3 harita · 4 para birimi (Gold/Silver/Gems/PassXP) |
| **Bu araştırmanın hedefi** | Full-vision **"deep synergy web"** (Decision Log §4) — büyü havuzunun post-launch genişlemesi için **ön-tasarım** |

## 1.2 Faz durumu & doğrulama borcu (Current State Reconstruction)

**Kullanıcı beyanı:** Phase 0 COMPLETE · Phase 1 COMPLETE · Phase 2 COMPLETE · Phase 3 COMPLETE · Phase 4 NOT STARTED.

**Dürüst implementasyon gerçeği (ADR'ler + faz raporları + ilk-derleme raporundan):** Tüm Phase 0–3 **deliverable'ları AUTHORED, canon-verified, integration-audited ve commit edildi**; ek olarak `FIRST_COMPILE_REPORT.md` (run 26884262912, sha `363076e`) ile kod tabanı artık **CI'da 0 hata ile DERLENİYOR** (Unity 6000.0.75f1; Entities IL + Burst post-processor'lar çalıştı). Ancak **çalışma-zamanı doğrulama kapıları hâlâ DEFERRED** (PASS değil).

| Kapı / Öğe | Durum | Kaynak |
|---|---|---|
| Phase 0 çıkış | **CONDITIONALLY ACCEPTED** (deferred validations outstanding) | ADR-0-002 |
| **Compile (Phase 0–3)** | **PASS** (stabil, 0 hata, CI kanıtlı) | FIRST_COMPILE_REPORT §2 |
| Android build / APK | **FAIL → DEFERRED** (post-compile: kaydedilmemiş sahne / Addressables / URP-global-settings / Android SDK seviyesi) | FIRST_COMPILE_REPORT §3, §7 |
| EditMode/PlayMode test | **DID NOT EXECUTE → DEFERRED** (runner proje-config'te abort) | FIRST_COMPILE_REPORT §4 |
| GATE 1 (FUN) | **OPEN / DEFERRED** (on-device "eğlenceli mi?" verdict çalıştırılmadı) | ADR-1-001, ADR-2-001 |
| GATE 2 (vertical-slice playtest) | **DEFERRED** (≥%40 session-2 dönüş + "okunur & eğlenceli" rubriği çalıştırılmadı) | ADR-2-001, phase-2 raporu |
| GATE 3 (MVP feature-complete; ekonomi server-validated) | **DEFERRED**; **Phase 4'e yetki = WITHHELD** | ADR-2-001, phase-3 raporu |
| FormationMember kablolaması | **ERTELENDİ** (formasyonlar authored; üyelik ataması yok) | FormationMember_wiring_plan.md |
| Commander/spell buff-stacking | **ÇÖZÜLDÜ & implemente edildi** (ADR-2-002) | phase-3 raporu §8 |

**Aktif blokerler / kalan doğrulama borcu:** (1) Tek seferlik Unity-editör proje konfigürasyonu (URP global settings, ≥1 kaydedilmiş ECS sahnesi, Addressables grupları, Android SDK API seviyesi) → APK + test yürütmeyi açar; (2) ardından **GATE 1 fun → GATE 2 playtest → GATE 3 server-validated** sırayla eritilmeli; (3) PlayFab canlı entegrasyon (SDK + CloudScript/Title Data). Bu borç eritilene dek tüm çalışma-zamanı kapıları DEFERRED. **Hiçbir ihlal-edilemez kısıt gevşetilmedi** — yalnızca *doğrulama zamanlaması* ertelendi (ADR-0-002 §4).

> **Bu araştırma için anlam:** "Deep synergy web" full-vision'dır — büyü havuzu genişlemesi **post-launch (S1–S3 sonrası)** olur. Ondan **çok önce** GATE 1/2/3 + soft-launch LTV (GATE 5 SCALE-OR-STOP) geçmeli ve **~12'lik MVP havuzu telemetri ile dengelenmeli.** Bu rapor o günü **önceden** hazırlar; bugünü değiştirmez.

## 1.3 Mevcut büyü sistemi — implementasyon gerçeği (zaten tasarlanmış)

BULWARK'ta **çalışır bir draft-3 büyü iskeleti zaten authored** (Phase 2.4). Sinerji ağı sıfırdan icat edilmez — **mevcut kanonun üstüne** inşa edilir.

**Şema (`SpellDef.cs`) — her büyünün taşıdığı kanon alanları:**
`id, displayName, category, cooldown, charges, telegraphTime (>0 ZORUNLU), targetShape, radius, magnitude, duration, appliesStatus, summonUnitIndex, synergyBonusVsStatus, synergyMultiplier, counterNote (ZORUNLU).`
Kod yorumu kanonu açıkça kodlar: *"every spell has cooldown + charges, a TELEGRAPH, a COUNTER, and SYNERGY tags — 'no un-counterable spell ships'."*

**Enumlar (`CombatTypes.cs`) — sinerji/karşıtlık ağının atomları:**
- `SpellCategory` (kanon §5.3, birebir): **Offensive · Control · Economy · Summon · Buff** (5 kategori).
- `StatusKind` (8): **None · Chilled · Burning · Poisoned · Stunned · Hasted · Raged · GoldBoost.** *(Yeni bir status = ADR gerektirir.)*
- `DamageType` (5): Melee · Pierce · Blunt · Fire · Poison *(bazı isimler PROVISIONAL/ADR-owned)*.
- `ArmorClass` (4): Light · Shielded · Heavy · Unarmored *(4. isim PROVISIONAL)*. Blueprint §4 bu matrisi "Light/Heavy/Shielded/Structure" + "Slash/Pierce/Blunt/Magic/Fire" olarak da anar; isim farkları ADR ile sonlandırılacak provisional yuvalardır.
- `TargetShape` (5): Point · Area · Line · Self · AllyArea.
- `TerrainKind`: HighGround · Choke · Cover · Hazard (§4/§11).

**Mevcut 12 büyü (`Data/Spells/*.asset`, PROVISIONAL/LSD-owned değerler):**

| # | Büyü | Kategori | CD | Telegraph | Şekil/Yarıçap | Etki | Status | Sinerji |
|---|---|---|---|---|---|---|---|---|
| 1 | **Freeze** | Control | 14 | 1.0 | Area r3 | yavaşlatma | Chilled | — (kurucu) |
| 2 | **Stun** | Control | 15 | 0.9 | Area r2 | devre dışı | Stunned | — |
| 3 | **Poison Cloud** | Control | 14 | 1.1 | Area r3.5 | DoT/alan-reddi | Poisoned | — |
| 4 | **Shatter** | Offensive | 13 | 1.0 | Area r3 | patlama | — | **×2.0 vs Chilled** |
| 5 | **Arrow Storm** | Offensive | 12 | 1.2 | Area r4 | barajı | — | — |
| 6 | **Lightning Storm** | Offensive | 16 | 1.4 | Area r3.5 | kalıcı hasar | Burning | — |
| 7 | **Rage** | Buff | 20 | 0.6 | AllyArea r3 | +saldırı (Raged) | Raged | — |
| 8 | **Haste** | Buff | 18 | 0.6 | AllyArea r3 | +hız (Hasted) | Hasted | — |
| 9 | **Gold Rush** | Economy | 30 | 0.5 | Self | madencilik+ (GoldBoost) | GoldBoost | — |
| 10 | **Raise Gold** | Economy | 25 | 0.5 | Self | anında 150 Gold | — | — |
| 11 | **Summon Pouncer** | Summon | 18 | 1.0 | Point | Skirmisher çağır | — | — |
| 12 | **Summon Giant** | Summon | 30 | 1.4 | Point | Heavy çağır | — | — |

**Kanon sinerji örneği (çalışır):** `Freeze`→Chilled uygular; `Shatter` `synergyBonusVsStatus=Chilled, synergyMultiplier=2.0` taşır → zaten-Chilled hedefe **×2 hasar**. Bu, raporun tüm sinerji ağının **referans desenidir.**

**Çalışma-zamanı boru hattı (`Spell.cs`, 1054 satır):**
1. `SpellCatalog` — havuzun derlenmiş hali (kategori, telegraph, sinerji alanları).
2. `StatusQuery` — status→§4-modifier-chain matematiğinin **TEK doğruluk kaynağı** (Chilled/Hasted hareket, Raged/Cover hasar, Stunned gating, `CommanderBuffMultiplier` bütçe-clamp'i).
3. `AddOrRefreshStatus` — kanonik ekle/yenile politikası (kind başına tek girdi; `max(remaining)`, `max(magnitude)`; `StatusSource` ile Spell/Commander ayrımı).
4. `SpellCastSystem` — 0-telegraph'ı **REDDEDER** (`k_MinTelegraph`); bir `ActiveTelegraph` (counterplay penceresi) spawn eder.
5. `TelegraphResolveSystem` — pencere ≤0 olunca **kategoriye göre** çözer (`ResolveOffensive/Control/Economy/Summon/Buff`) ve **sinerjiyi** uygular (`SynergyFactor`: hedef zaten `SynergyBonusVsStatus` taşıyorsa ×`SynergyMultiplier`).

**Tek savaş çekirdeği (§4, no-fork):** `final = base × (1+upgrade×perLevel) × typeArmor × positional(flank 1.5/back 2.0) × terrain × difficulty`; buna spell sinerji, Raged (giden), Cover (gelen), stun-gate ve Chilled/Hasted/Choke hareket-ölçeği eklenir. **Büyüler bu çekirdeği besler; ayrı bir hasar yolu yoktur.**

## 1.4 Komutan etkileşimleri (kanon)

| Komutan | Fraksiyon | Active | Passive | Bütçe |
|---|---|---|---|---|
| **Iron Warden** | Iron Pact | **Rally** (Raged+Hasted, saldırı-ağırlıklı; r5, 6 sn, mag 0.12) | **Quartermaster** (madencilere GoldBoost, mag 0.08) | 0.12 |
| **Ashen Warchief** | Ashen Horde | **WarCry** (Hasted+Raged, hız-ağırlıklı; r5, 5 sn, mag 0.15) | **Bloodthirst** (savaş birimlerine Raged, mag 0.08) | 0.13 |

**Kritik kanon (ADR-2-002, implemente):** Komutan-kaynaklı buff'lar `StatusSource.Commander` etiketlenir; `StatusQuery.CommanderBuffMultiplier` aynı türden **tüm komutan girdilerini BİRLEŞTİRİR ve ≤ `PowerBudgetPct`'e clamp'ler** (active+passive bir birimde §6 bütçesini **asla** aşamaz; kod sabiti `k_PowerBudgetCeiling = 0.15`). **Spell-kaynaklı** Raged/Hasted ise **AYRI, §6 ile sınırlanmayan** taktik katmandır — dengesi cooldown/telegraph/counter ile tutulur (§5.3). Bu ayrım, büyü buff'larının komutan buff'larıyla **çarpımsal P2W sızıntısı** yapmasını engeller ve bu raporun komutan-etkileşim analizinin (Part 9) temelidir.

## 1.5 Kanon Denetimi (Canon Audit)

| Eksen | Kanon |
|---|---|
| **Korunan (PRESERVE)** | Maden→eğit→ittir→heykel döngüsü; doğrudan birim kontrolü/possess; okunur lane savaşı; kalıcı *capped* ilerleme; **büyü kalıcılığı → draft-3 roguelite loadout** (§3, ChangeLog §1); async-first rekabet; O(1) perf-öncelikli AI; 3-katmanlı RemoteConfig; CRDT ledger (server altında cache) |
| **Modernize (MODERNIZE)** | Nokta-etkili büyüler → **draft-3, sinerji + telegraph/counter**'lı roguelite loadout (ChangeLog §2); flat counter → type×armor 5×4; binary backstab → positional flank/back; reaktif FSM → katmanlı utility AI |
| **Yasak (CUT — §15, ASLA)** | Loot box / gacha-for-power; interstitial reklam; energy/stamina kapıları; **pay-to-win / satılabilir ham güç**; açıklanmamış oranlı paralı kutular; save-state log; client-otoriteli para; **Phase 7 öncesi** real-time PvP, biome, clan, 3. fraksiyon, komutan koleksiyonu |
| **Non-goals** | Real-time-PvP-öncelikli (Clash Royale) DEĞİL; whale SLG/4X DEĞİL; pasif autobattler DEĞİL; gacha DEĞİL; P2W DEĞİL; energy-gated DEĞİL |
| **Monetizasyon kısıtı** | Kozmetik + battle-pass; gems **convenience + prestij** alır, **güç ALMAZ**; **büyüler monetizasyon yüzeyinde DEĞİLDİR** (oynayarak kazanılır; yalnızca VFX-recolor kozmetik, clarity-mode altında) |
| **Komutan kısıtı** | 1 active + 1 passive; güç ≤%10–15 (hard-clamp); tempo/utility, raw stat değil; earnable; ranked-normalized; koleksiyon = Phase 7.5 |
| **Fraksiyon kısıtı** | Yalnızca 2 MVP fraksiyon (Iron Pact, Ashen Horde); Arcane/Mechanized = Phase 7.3 (DEFER); strictly-dominant fraksiyon yok |
| **Büyü kısıtı** | Havuz ~12 (MVP), draft 3; **her büyü: cooldown+charge + telegraph(>0) + counter + synergy tag**; un-counterable büyü gönderilmez; 5 kanon kategori; 8 kanon StatusKind; kategoriler **icat edilemez** (→ ADR) |
| **Teknik kısıt** | Battle-sim = ECS/DOTS (UI = MonoBehaviour); MVP non-deterministic + server stat-sanity (determinism = Phase 7); server-authoritative ekonomi; perf bütçesi her faz hard-gate; içerik **data (SO)→config**; **§15.6: sayısal değerleri icat etme** (LSD-owned, provisional) |

## 1.6 Çelişki Kontrolü (Contradiction Check) & Yetki Kuralı

Bu raporun tavsiyeleri aşağıdaki testlerden geçirilmiştir. **Tespit edilen 3 gerilim açıkça işaretlenir** (gizlenmez):

| Gerilim | Kanon | Bu raporun çözümü |
|---|---|---|
| **Taksonomi farkı** | Kanon kategoriler: Offensive/Control/Economy/**Summon**/**Buff** (§5.3). Görev, "Support" ve "Recon/Information" ister. | Part 3'te **iki katman** sunulur: (a) **kanon 5 kategori = otorite**; (b) **araştırma merceği** (Support = Buff+Summon+heal/shield üst-kümesi; **Recon/Information = NET-YENİ**). Recon ve "Cleanse/Far Sight" gibi öğeler **mevcut kategoriye sığmaz → ADR şarttır** diye işaretlenir. |
| **20+ büyü & yeni kategori** | MVP ~12; kanon kapalı (§15). | 20 büyü = **12 kanon (taşınan) + 8 net-yeni (ADR-kapılı, post-launch)**. Hepsi **full-vision "deep synergy web"** kapsamında (Decision Log §4) → **tavsiye**, implementasyon değil. |
| **Yeni status/mekanik** | 8 kanon StatusKind; yeni status/mekanik (vision, displacement, dynamic terrain, status-cleanse) yok. | Her yeni öğe **açıkça ADR-flag'li** (Part 4, 12, 13). Hiçbiri "kanon" diye sunulmaz. |

**Tüm tavsiyeler şunları İHLAL ETMEZ:** ✗ roadmap ✗ ADR'ler ✗ decision log ✗ güç bütçeleri (komutan ≤%15; ADR-2-002 katman ayrımı korunur) ✗ monetizasyon (büyü = güç satışı **yok**) ✗ readability (telegraph zorunlu) ✗ no-P2W ✗ un-counterable combo yok.

**Yetki Kuralı (binding):** Gelecek araştırma **yalnızca tavsiyedir.** Bu rapor roadmap/canon/decision-log'u değiştiremez, implementasyon/özellik yetkilendiremez. Yalnızca **keşfeder, analiz eder, tavsiye eder, değerlendirir.** Buradaki her sayısal değer **PROVISIONAL/LSD-owned**'dır (§15.6/§16); kanon olarak iddia edilmez.

## 1.7 Roadmap kısıtları & gelecek genişleme limitleri

- Büyü havuzu büyümesi **post-launch** içeriğidir (Blueprint §2: "S1–S3: +pool, synergies"); **sezon tek-içerik-slotu {birim|komutan|harita}** büyüyü içermez (§13 6.2) → büyüler **ayrı bir ADR-kapılı içerik akışıdır.**
- **Combinatorial balance** roadmap'in #5 riskidir → telemetri + RemoteConfig live-tuning + ranked normalizasyon ile yönetilir. Bu rapor "küçük, okunur, dengelenebilir havuz" disiplinini korur.
- **Restraint kanonik** (§5): hiçbir büyü **distinct rol + counter** olmadan gönderilmez.

---

# PART 2 — Büyü Sistemi Felsefesi

## 2.1 Tür analizleri ve BULWARK için dersler

| Kaynak tür | Güçlü yan | Zayıf/tehlikeli yan | BULWARK dersi |
|---|---|---|---|
| **Klasik RTS spell** (SC/WC3) — micro-yoğun, hedefli, yüksek-tavan | Yüksek ustalık tavanı; pozisyonel; "spell as a read" | Mobilde tıklama-yükü; okunmazlık; APM duvarı | **Telegraph + draft** ile tavanı koru, yükü düşür; possess anında manuel-aim büyüsü değil, **squad-ölçekli** etki |
| **MOBA sinerji** (combo zinciri, CC-chain) | Tatmin edici combo'lar; takım kimliği | **CC-zinciri = kaçışsız ölüm**; "perma-stun"; counter'sız ult | **Her güçlü combo'nun counter'ı** zorunlu (§5.3); CC diminishing/charge/cooldown; **Cleanse** (anti-status) tasarıma dahil |
| **Draft / roguelite** (autobattler, survivor-like) | "Build-craft" dopamini; tekrar-oynanırlık; az içerikle çok varyasyon | Görünmez güç; "broken draft"; okunmazlık | **Draft 3-of-N, savaş öncesi, okunur**; sinerji-tag'leri tooltip'te; **pay yok, beceri var** |
| **Counter sistemleri** (taş-kâğıt-makas) | Okunabilir; kompozisyon kararı | Aşırı katı → "yanlış draft = kayıp" | **Yumuşak** counter (type×armor multiplier'ları, pozisyon, timing); tek bir büyü maçı bitirmez |
| **Taktik büyü** (terrain/zone-control) | Pozisyon/zaman ustalığı; düşük güç-enflasyonu | VFX karmaşası → okunmazlık | **Zone/terrain etkileşimi** favori; ama mobil parçacık bütçesi + clarity sabit |

## 2.2 BULWARK NEYİ KORUMALI

1. **"No un-counterable spell" yasası (§5.3).** Her büyü `telegraphTime>0` + `counterNote` taşır; kod 0-telegraph'ı reddeder. **Bu, türü tanımlayan ahlaki kuraldır** — asla taviz verilmez.
2. **Tek savaş çekirdeği (§4).** Büyüler counter-matrix/positional/terrain çarpanlarını **besler**, kendi gizli hasar yolunu açmaz. Bu, dengeyi yönetilebilir tutar.
3. **Draft = beceri, para değil.** Büyüler oynayarak kazanılır; draft savaş öncesi okunur bir stratejik seçimdir (§3 PREPARE).
4. **Status-tabanlı sinerji** (Chilled→Shatter deseni). Sinerji **data-driven** (`synergyBonusVsStatus`/`synergyMultiplier`) — kodda hard-code değil; bu ölçeklenmenin anahtarıdır (Part 10).
5. **Komutan/spell katman ayrımı** (ADR-2-002). Buff'lar iki ayrı, doğru-sınırlanmış katmanda kalır.
6. **Restraint.** Küçük, okunur havuz; her büyü distinct.

## 2.3 BULWARK NEDEN KAÇINMALI

1. **Kaçışsız CC-zinciri.** Freeze→Stun→Poison→Shatter zinciri **garantili ölüm**e dönüşmemeli → diminishing returns, charge limitleri, **Cleanse** ve "disperse" counter'ı zorunlu.
2. **Basit hasar nükleri ve güç-enflasyonu.** Görev de bunu yasaklar. Yeni büyüler **pozisyon/formasyon/zaman/ekonomi/bilgi** üstüne kurulur; ham `magnitude` artışı çözüm değildir.
3. **Görünmez güç / "broken draft".** Sinerji tooltip'te telegraf edilir; hiçbir 3'lü kombinasyon **counter'sız** olmamalı (Part 8/12).
4. **VFX karmaşası.** Güçlü etki = **daha okunur** telegraph demek, daha gürültülü değil (§10, §11 clarity).
5. **Para-duvarı büyüler.** Büyü asla gems/IAP ile güç olarak satılmaz (§9/§10, CUT).
6. **Kategori/status enflasyonu.** Her yeni status combinatorial yükü patlatır → katı ADR disiplini (Part 10).

---

# PART 3 — Büyü Taksonomisi

**İki katmanlı taksonomi.** (a) **Kanon 5 kategori** = `SpellCategory` enum'u, otorite. (b) **Araştırma merceği** = görevin istediği 5 rol; kanona **eşlenir** ve farkı işaretlenir.

| Araştırma rolü | Kanon kategori karşılığı | Kanon durumu |
|---|---|---|
| Offensive | **Offensive** | ✅ birebir |
| Control | **Control** | ✅ birebir |
| Economy | **Economy** | ✅ birebir |
| **Support** | **Buff** + **Summon** + (heal/shield/cleanse) | ⚠️ Buff & Summon kanon; heal/shield/cleanse = **NET-YENİ (ADR)** |
| **Recon / Information** | *(kanon karşılığı YOK)* | ⛔ **NET-YENİ kategori — ADR + bilgi/fog sistemi şart** |

## 3.1 Offensive (Saldırı)
- **Amaç:** Yoğunlaşmış/yanlış-pozisyonlanmış düşmanı cezalandırmak; combo *payoff*'u (Shatter).
- **Güç:** Tempo kırma, formasyon cezası, combo finisher.
- **Zayıf:** Tek başına "dumb nuke" → düşük derinlik; telegraph'tan kaçılabilir; dağınık düşmana etkisiz.
- **Draft rolü:** Bir *enabler* (Freeze/Tar Pit) olmadan düşük değer; combo'nun "vuruş" yarısı. **Asla** ham hasar enflasyonu için draftlanmaz.

## 3.2 Control (Kontrol)
- **Amaç:** Hareket/aksiyon reddi, alan-reddi, **combo enabler** (Chilled, Stunned, Poisoned, slow-zone).
- **Güç:** Pozisyon dikte etme, push durdurma, sinerji kurma (ağın *kalbi*).
- **Zayıf:** Düşük doğrudan-hasar; aşırı yığılırsa kaçışsız → adalet riski; Cleanse'e açık.
- **Draft rolü:** Ağın bağlayıcı dokusu; hemen her arketipte 1 kontrol büyüsü mantıklı. **Yüksek** draft değeri.

## 3.3 Economy (Ekonomi)
- **Amaç:** Gold tempo'su (üretim hızı, anlık enjeksiyon) ve **eko baskısı** — savaş-dışı kazanç ekseni.
- **Güç:** Kompozisyon avantajı satın alma; geç-oyun ölçeklenmesi; *snowball* potansiyeli.
- **Zayıf:** Anlık savaş gücü vermez → **tempo-cezalanabilir** (yatırım penceresinde saldırı). Uzun cooldown.
- **Draft rolü:** Eko/attrition arketipinin omurgası; agresif draftta zayıf (tempo kaybı). **Sadece Gold** (in-battle) — asla premium para. Kanon counterNote: "punish with tempo while the opponent invests."

## 3.4 Support (Destek) — *Buff + Summon + (yeni) heal/shield/cleanse*
- **Amaç:** Müttefik güçlendirme (Raged/Hasted), takviye (summon), **dayanıklılık/iyileştirme/temizleme** (yeni).
- **Güç:** Combo'yu mümkün kılma; tempo penceresi açma; **Cleanse = kontrol-ağına resmi counterplay.**
- **Zayıf:** Geçici (buff pencereleri); summon **focus-fire**'a açık; ward/heal pozisyon-bağımlı.
- **Draft rolü:** Tempo & defensive arketipler; **Cleanse** kontrol-ağır metaya panzehir → meta-dengeleyici. Buff/Summon kanon; **heal/shield/cleanse net-yeni (ADR).**

## 3.5 Recon / Information (Keşif / Bilgi) — ⛔ NET-YENİ KATEGORİ
- **Amaç:** Bilgi asimetrisi yaratmak/kapatmak — düşman kompozisyonu/draft'ı/kuyruğunu ifşa (Far Sight), görüş hattını engelleyip kanat baskını (Smoke).
- **Güç:** Düşük güç-enflasyonu ile yüksek beceri-ifadesi; "okuma" oyununu derinleştirir; pozisyonel combo'yu (flanking advantage) mümkün kılar.
- **Zayıf:** **Bir görüş/fog/gizli-bilgi sistemi GEREKTİRİR — MVP'de YOK** (en ağır ADR); kötü tasarlanırsa "kalıcı görüş = bilgi-istismarı" (Part 12).
- **Draft rolü:** Recon arketipinin tanımı; niş ama yüksek-tavan. **Tüm kategori ADR-kapılı** — yalnızca tam-vizyon araştırması.

---

# PART 4 — 20 Büyü Tasarımı

**Kompozisyon:** **12 kanon (taşınan)** + **8 net-yeni (ADR-kapılı, post-launch).** Basit nüklerden kaçınıldı; pozisyon/formasyon/terrain/zaman/ekonomi/bilgi vurgulandı. Her büyü kanon §5.3 sözleşmesini taşır: **cooldown+charge · telegraph(>0) · counter · synergy.** Sayısal değerler **PROVISIONAL/LSD-owned** (§15.6).

> **Etiketler:** `[KANON]` = mevcut `Data/Spells/*.asset`; `[YENİ:ADR]` = net-yeni, ADR + (gerekiyorsa) yeni sistem/status şart. **Kapsam** = "düşük/orta/yüksek" yeni-sistem maliyeti.

### Cooldown felsefesi (bantlar — kanon değerlerinden türetilmiş)
- **12–16 sn** = sık taktik araçlar (offense/control); maç-içi ritmi belirler.
- **18–20 sn** = güçlü buff/summon pencereleri; "ne zaman" kararı.
- **25–30 sn** = oyun-değiştiren eko/dev summon; nadir, yüksek-bahis.
- **Charge** çoğunlukla 1 (tek-atış pencere); eko/recon için 1, asla "spam"a izin vermez.

---

### S1 — Dondurma *(Freeze / spell_freeze)* — Control `[KANON]`
- **Fantazi:** Ani bir don dalgası cepheyi kilitler.
- **Etki:** Area r3, Chilled (yavaşlatma) uygular; mag 0.5, telegraph 1.0, CD 14.
- **Cooldown felsefesi:** Sık enabler bandı — combo'yu kurar, kendisi öldürmez.
- **Telegraph:** Yerde genişleyen mavi-buz halkası + kristal-çıtırtı sesi (1.0 sn).
- **Counterplay:** Telegraph'tan çık · cleanse · **dağıl** (az birim Chilled olsun).
- **Draft değeri:** **Yüksek** — ağın #1 enabler'ı (Shatter/Heavy Impact payoff'unun ön-koşulu).

### S2 — Sersemletme *(Stun / spell_stun)* — Control `[KANON]`
- **Fantazi:** Sersemletici şok dalgası.
- **Etki:** Area r2, Stunned (kısa devre-dışı), telegraph 0.9, CD 15.
- **Cooldown felsefesi:** Kısa-pencere disable; yüksek-değer ama küçük yarıçap (anti-perma-CC).
- **Telegraph:** Sarı şok-halkası + alçak gümbürtü (0.9 sn — en kısa kontrol penceresi).
- **Counterplay:** İlerlemeyi **kademelendir** (stagger); kümeyi dağıt; timing ile pencereyi yut.
- **Draft değeri:** Orta-yüksek — tempo kesintisi; CC-zinciri riskinden ötürü diminishing'e tabi (Part 12).

### S3 — Zehir Bulutu *(Poison Cloud / spell_poisoncloud)* — Control `[KANON]`
- **Fantazi:** Yayılan zehirli sis; alan reddi.
- **Etki:** Area r3.5, Poisoned (DoT), mag 6, telegraph 1.1, CD 14.
- **Cooldown felsefesi:** Kalıcı alan-reddi; anlık değil → konumlandırma cezası.
- **Telegraph:** Yeşil sis-bulutu yavaş açılır + tıslama (1.1 sn); kalıcı olduğu görünür.
- **Counterplay:** **Bulutu terk et** (anlık değil); choke'a girmeden dolaş.
- **Draft değeri:** Orta — choke/Tar Pit ile birlikte güçlü; attrition arketipi.

### S4 — Paramparça *(Shatter / spell_shatter)* — Offensive `[KANON]`
- **Fantazi:** Donmuş bedenleri kıran kırılma darbesi.
- **Etki:** Area r3, mag 16, **synergyBonusVsStatus=Chilled, ×2.0**; telegraph 1.0, CD 13.
- **Cooldown felsefesi:** Combo *payoff*'u — tek başına vasat, Chilled'a karşı oyun-değiştiren.
- **Telegraph:** Beyaz kırılma-çatlağı zemini + cam-kırılma sesi (1.0 sn).
- **Counterplay:** Çözülürken **Chilled olma**; dağıl (sinerji vuruşunu sınırla).
- **Draft değeri:** **Yüksek (koşullu)** — Freeze olmadan düşük; ağın referans combo'su.

### S5 — Ok Yağmuru *(Arrow Storm / spell_arrowstorm)* — Offensive `[KANON]`
- **Fantazi:** Gökten ok barajı.
- **Etki:** Area r4 (en geniş), mag 18, telegraph 1.2, CD 12 (en kısa).
- **Cooldown felsefesi:** Sık **anti-yığılma/anti-Tight-formasyon** cezası — formasyon kararını cezalandırır, kör nüke değil.
- **Telegraph:** Yerde geniş hedef-dairesi + ıslık-çınlama (1.2 sn).
- **Counterplay:** **Dağıl / Loose formasyon**; işaretli alanı terk et.
- **Draft değeri:** Orta — Tight formasyon/choke meta'sına karşı; tek başına telegraph'tan kaçılır.

### S6 — Şimşek Fırtınası *(Lightning Storm / spell_lightningstorm)* — Offensive `[KANON]`
- **Fantazi:** Cepheyi tarayan kalıcı yıldırım fırtınası.
- **Etki:** Area r3.5, mag 22 (en yüksek), Burning (DoT), telegraph 1.4 (en uzun), CD 16.
- **Cooldown felsefesi:** Yüksek-bahis kalıcı zone; uzun telegraph = adil counterplay.
- **Telegraph:** Toplanan kara bulut + ön-şimşek çakımı + gök gürültüsü (1.4 sn).
- **Counterplay:** Birimleri **fırtınadan yürüt**; kümeleme; uzun pencereden faydalan.
- **Draft değeri:** Orta-yüksek — alan-reddi + Burning; pozisyon zorlar.

### S7 — Öfke *(Rage / spell_rage)* — Buff (Support) `[KANON]`
- **Fantazi:** Savaş çığlığı saldırıyı körükler.
- **Etki:** AllyArea r3, Raged (+saldırı), mag 0.3, telegraph 0.6, CD 20.
- **Cooldown felsefesi:** Uzun-CD tempo penceresi; "ne zaman commit" kararı.
- **Telegraph:** Kırmızı aura + savaş-borusu (0.6 sn, müttefik buff = kısa pencere).
- **Counterplay:** **Disengage**, pencere bitince re-commit; spell-katmanı (komutan-bütçesinden ayrı, ADR-2-002).
- **Draft değeri:** Orta-yüksek — agresif/tempo; Haste ile "all-in" penceresi.

### S8 — Hız *(Haste / spell_haste)* — Buff (Support) `[KANON]`
- **Fantazi:** Doğaüstü çeviklik.
- **Etki:** AllyArea r3, Hasted (+hız), mag 0.4, telegraph 0.6, CD 18.
- **Cooldown felsefesi:** Reposition/flank enabler; ucuz pencere.
- **Telegraph:** Mavi hız-izleri + yükselen ıslık (0.6 sn).
- **Counterplay:** **Pozisyonu koru**; buff hızla biter; flank'i öngör.
- **Draft değeri:** **Yüksek** — flank/recon combo'larının (Smoke→flank) motoru.

### S9 — Altın Hücumu *(Gold Rush / spell_goldrush)* — Economy `[KANON]`
- **Fantazi:** Madencilik çılgınlığı.
- **Etki:** Self, GoldBoost (madencilik hız ×), mag 0.5, telegraph 0.5, CD 30 (en uzun).
- **Cooldown felsefesi:** Oyun-içi en uzun CD; tek seferlik eko-snowball kararı.
- **Telegraph:** Madenlerde altın-parıltı + jeton sesi (0.5 sn; savaş-dışı, kısa).
- **Counterplay:** **Tempo ile cezalandır** — rakip yatırım yaparken bas.
- **Draft değeri:** Orta — eko/attrition; agresif metada zayıf.

### S10 — Altın Yükselt *(Raise Gold / spell_raisegold)* — Economy `[KANON]`
- **Fantazi:** Anlık hazine.
- **Etki:** Self, anında 150 Gold, telegraph 0.5, CD 25.
- **Cooldown felsefesi:** Anlık enjeksiyon → ani kompozisyon sıçraması; spend-window açık.
- **Telegraph:** HUD'da altın-akış animasyonu (0.5 sn).
- **Counterplay:** **Harcama penceresinde baskı** (rakip henüz birime çevirmeden).
- **Draft değeri:** Orta — tempo-spike eko; tek başına savaş gücü yok.

### S11 — Sıçrayıcı Çağır *(Summon Pouncer / spell_summonpouncer)* — Summon (Support) `[KANON]`
- **Fantazi:** Çevik bir avcı belirir.
- **Etki:** Point, Skirmisher çağırır (`summonUnitIndex=2`), telegraph 1.0, CD 18.
- **Cooldown felsefesi:** Orta-CD esnek takviye; flank/baskı.
- **Telegraph:** Spawn-pentagramı + hırıltı (1.0 sn).
- **Counterplay:** **Spawn'ı focus-fire** et (iniş hitlerinden önce).
- **Draft değeri:** Orta — tempo/flank; Smoke ile gizli-flank.

### S12 — Dev Çağır *(Summon Giant / spell_summongiant)* — Summon (Support) `[KANON]`
- **Fantazi:** Devasa bir kuşatma yaratığı.
- **Etki:** Point, Heavy çağırır (`summonUnitIndex=5`), telegraph 1.4, CD 30.
- **Cooldown felsefesi:** Nadir, oyun-değiştiren; uzun telegraph = adil.
- **Telegraph:** Yer sarsıntısı + büyük gölge + uzun ön-kükreme (1.4 sn).
- **Counterplay:** **Kite + focus-fire**; büyük/yavaş; Pierce-Ranged (anti-Heavy) ile eritilir.
- **Draft değeri:** Orta-yüksek — anti-statue/anti-Heavy; ekonomi gerektirir.

---

### S13 — Ağır Darbe *(Heavy Impact)* — Offensive `[YENİ:ADR]` · Kapsam: orta
- **Fantazi:** Cepheyi sarsan kuşatma-çekici darbesi.
- **Etki:** **Line** şekli (telegrafık koridor), Blunt-hasar **Heavy/Shielded/Structure**'a bonus (mevcut counter-matrix); **Tight/Line formasyon kohezyonunu dağıtır** (yeni: scatter mekaniği); `synergyBonusVsStatus=Chilled` → **görevin "Freeze + Heavy Impact = Shatter" sinerjisi** (Chilled hedefe kırılma bonusu). CD ~14, telegraph ~1.2.
- **Cooldown felsefesi:** Shatter'ın **blunt/anti-formasyon** kardeşi; combo finisher + zırh-delici.
- **Telegraph:** Yerde ağırlaşan koridor-gölgesi + yükselen "şarj" gümbürtüsü (1.2 sn).
- **Counterplay:** **Loose formasyon** + dağıl; koridordan çık; Chilled olma; Cleanse.
- **Draft değeri:** **Yüksek (koşullu)** — Freeze/Tar Pit enabler ile; Iron Pact'in shield-wall'una karşı asimetrik cevap.
- **ADR notu:** "Formasyon scatter" yeni mekanik; Chilled-sinerji mevcut desen.

### S14 — Zift Bataklığı *(Tar Pit / Mire)* — Control `[YENİ:ADR]` · Kapsam: orta
- **Fantazi:** Cepheyi yutan yapışkan bataklık.
- **Etki:** Geçici **spell-spawn'lı terrain zone** (mevcut `TerrainFeature` Choke/Hazard hibridi): MoveMult↓ (yavaşlatma — Chilled semantiğini yeniden kullanabilir) + isteğe bağlı hafif Hazard DoT; süre-bağlı. CD ~16, telegraph ~1.1.
- **Cooldown felsefesi:** Kalıcı pozisyon dikte aracı; choke yaratır/güçlendirir.
- **Telegraph:** Koyu-kahve kabarcıklı zemin yayılır + balçık sesi (1.1 sn).
- **Counterplay:** **Etrafından yürü**; içine birim **huni'leme**; alanı Arrow Storm'la birleştirme tuzağına düşme.
- **Draft değeri:** **Yüksek** — yapay choke = AoE/Arrow Storm/Lightning sinerji platformu; control/attrition.
- **ADR notu:** "Dinamik (spell-spawn'lı) terrain" yeni sistem; mevcut TerrainFeature bileşenini yeniden kullanır.

### S15 — Rüzgâr Duvarı / İtiş *(Gale Wall / Repulse)* — Control `[YENİ:ADR]` · Kapsam: yüksek
- **Fantazi:** Cepheyi geri savuran basınç dalgası.
- **Etki:** **Line** displacement — düşman ön-hattını geri iter (hasarsız); **şarj/timing'i bozar**, flank'leri sıfırlar. Hasar yok, saf tempo/pozisyon. CD ~16, telegraph ~1.0.
- **Cooldown felsefesi:** "Reset" butonu — kötü angajmanı geri al, push'u kır; **zamanlama ustalığı** ödülü.
- **Telegraph:** Şişen yarı-saydam basınç-yayı + alçalan uğultu (1.0 sn).
- **Counterplay:** **Pencereden sonra re-commit** (timing); dağıl (itiş daha az birime değsin); ranged ile mesafe koru.
- **Draft değeri:** Orta-yüksek — defensive/tempo; statue-rush savunması; "anti-all-in".
- **ADR notu:** **Displacement/knockback yeni mekanik** (fizik/pozisyon yazımı) — en ağır kontrol öğesi; yeni status YOK.

### S16 — İkmal Feneri *(Supply Beacon)* — Economy (Support) `[YENİ:ADR]` · Kapsam: orta
- **Fantazi:** Cephe-gerisi ikmal sancağı.
- **Etki:** **Zone-scoped GoldBoost kaynağı** (mevcut GoldBoost status'unu yeniden kullanır): fener yakınındaki madencilik/eğitimi hızlandırır; süre-bağlı, sabit konum. **Görevin "Supply Beacon + Gold Rush = Economic Surge" sinerjisi.** CD ~25, telegraph ~0.6.
- **Cooldown felsefesi:** Pozisyonel eko — "nereye" kararı (kontrol edilen madene yakın).
- **Telegraph:** Dikilen sancak + altın halka (0.6 sn).
- **Counterplay:** **Fener bölgesini contest et/bas**; tempo ile cezalandır; konumu sabit (öngörülebilir).
- **Draft değeri:** **Yüksek** — eko/attrition omurgası; Gold Rush ile snowball (Part 12 mitigasyonlu).
- **ADR notu:** "Kalıcı zone-buff entity (madenciliği etkileyen)" yeni; GoldBoost mevcut.

### S17 — Siper Sancağı / Koruma *(Bulwark Ward / Rally Banner)* — Buff (Support) `[YENİ:ADR]` · Kapsam: orta
- **Fantazi:** Saflarda parıldayan koruyucu sancak.
- **Etki:** **Zone**: içindeki müttefiklere gelen-hasar azaltımı (**yeni "Warded" status**); **Line/Tight formasyonun yer tutmasını ödüllendirir.** Mevcut Cover terrain'i zaten DefenseMult uygular → matematik var, status+zone yeni. CD ~20, telegraph ~0.7.
- **Cooldown felsefesi:** Defensive pencere; "tut ve dayan" kararı; Iron Pact kimliğiyle uyumlu.
- **Telegraph:** Dikilen sancak + mavi kalkan-kubbe (0.7 sn).
- **Counterplay:** **Etrafından flank** (zone sabit); birimleri **sancaktan çek/çekiştir**; kümeyi AoE'le; Heavy Impact ile zorla.
- **Draft değeri:** Orta-yüksek — defensive/attrition; statue savunması.
- **ADR notu:** **Yeni StatusKind ("Warded")** + zone entity → ADR. *(Yeni status = combinatorial maliyet; Part 10/12.)*

### S18 — Arınma / İkinci Nefes *(Cleanse / Second Wind)* — Support `[YENİ:ADR]` · Kapsam: düşük
- **Fantazi:** Saf bir nefes lanetleri söker.
- **Etki:** AllyArea r3 — müttefiklerden **negatif status'leri kaldırır** (Chilled/Burning/Poisoned/Stunned). **Kontrol-ağına resmi counterplay** = "her güçlü combo'nun counter'ı" yasasının büyü-cisimleşmesi. CD ~20, telegraph ~0.6.
- **Cooldown felsefesi:** Reaktif kurtarma; "doğru an" okuması (combo'yu boşa düşürür).
- **Telegraph:** Beyaz arınma-dalgası + çan-tını (0.6 sn).
- **Counterplay:** **Cleanse'i bait'le**, sonra re-apply (timing düellosu); ikinci kontrol dalgası; uzun CD'yi sömür.
- **Draft değeri:** **Çok yüksek (meta-bağımlı)** — kontrol-ağır metada panzehir; CC'yi dengeleyen denge supabı.
- **ADR notu:** "StatusEffect kaldırma" yeni *verb* (kod basit: buffer'dan girdi sil) — düşük sistem maliyeti, yüksek denge etkisi.

### S19 — İleri Görüş / Keşif Sinyali *(Far Sight / Scout Pulse)* — Recon/Information `[YENİ:ADR]` · Kapsam: **çok yüksek**
- **Fantazi:** Bir an için savaş alanını kuş bakışı gör.
- **Etki:** Bir pencere boyunca **düşman kompozisyonunu / draftlanmış büyülerini / eğitim kuyruğunu ifşa eder** (bilgi savaşı). Hasar/CC yok.
- **Cooldown felsefesi:** Bilgi-tempo'su; "ne draftladı/ne geliyor" okuması; bilgi **bozulur** (kalıcı değil).
- **Telegraph:** Yükselen keşif-feneri + radar-pingi (~0.7 sn); **kendini telegraf eder** (rakip "görüldüm" bilir).
- **Counterplay:** **Feint** (yanlış kuyruk göster); bilgi penceresi kısa; timing.
- **Draft değeri:** Niş-yüksek — Recon arketipinin tanımı; tek-başına düşük, okuma-becerisiyle yüksek.
- **ADR notu:** ⛔ **MVP'de görüş/fog/gizli-bilgi sistemi YOK** → bu büyü **yeni bir bilgi katmanı** gerektirir (en ağır ADR). Ayrıca "draft gizliliği" bir tasarım ön-koşulu (şu an draft savaş öncesi açık olabilir). **Yalnızca tam-vizyon.**

### S20 — Sis Perdesi *(Smoke Screen)* — Recon/Control `[YENİ:ADR]` · Kapsam: yüksek
- **Fantazi:** Yükselen sis görüşü keser, baskına kapı açar.
- **Etki:** **Görüş-hattı engelleyen zone** (mevcut Cover'ın "blocks line of sight" kavramına yaslanır): ranged içinden hedefleyemez → **gizli flank/baskın** (= görevin "Smoke + Ambush = Flanking Advantage"). Hasar yok. CD ~16, telegraph ~0.8.
- **Cooldown felsefesi:** Pozisyonel kurulum; Haste/flanker ile birlikte "ambush" maneuver'ı.
- **Telegraph:** Yayılan gri sis + fısıltı/uğultu (0.8 sn).
- **Counterplay:** **Kör takip etme**; sisi AoE'le (Arrow Storm/Lightning); reposition; flank'i öngörüp arkayı kapat.
- **Draft değeri:** **Yüksek** — flank/tempo/recon; positional bonus (flank ×1.5/back ×2.0) ile birleşince güçlü.
- **ADR notu:** LoS-block + spawn'lı zone (Cover kavramı kısmen var) → orta-yüksek ADR. **"Ambush"** ayrı büyü değil; Smoke'un **mevcut positional sistemle** mümkün kıldığı maneuver'dır (Haste/Pouncer/flanker + flank çarpanı).

---

# PART 5 — Büyü Sinerji Ağı

**Gösterim seçimi (okunabilirlik kanonu).** 20×20 = 400-hücreli ızgara yerine, ağ **(a) sinerji seviyesi lejantı + (b) komşuluk listesi (her büyünün en güçlü partnerleri) + (c) anahtarlı combo dosyası** olarak verilir. Bu, "complete network"ü pratikte sağlar ve P2-Readable-depth ile tutarlıdır.

### Sinerji seviyesi lejantı
| Seviye | Kod | Anlam |
|---|---|---|
| **S (İmza)** | ◆◆◆ | Tasarlanmış, oyun-değiştiren combo (data: status→synergy). Counter ZORUNLU. |
| **Güçlü** | ◆◆ | Güçlü ama koşullu (pozisyon/zaman gerektirir). |
| **Orta** | ◆ | Durumsal; iyi oyuncunun değer çıkardığı. |
| **Nötr** | · | Anlamlı etkileşim yok. |
| **Anti-sinerji** | ✗ | Birbirini boşa düşürür (örn. iki uzun-CD eko = tempo intiharı). |

### Sinerji omurgası — enabler → payoff zincirleri (◆◆◆ İmza)
| Enabler | Payoff | Combo sonucu | Taktik amaç | Kanon |
|---|---|---|---|---|
| **Freeze (Chilled)** | **Shatter** | **Paramparça** (×2.0) | Donmuş kümeyi yok et | ✅ implemente |
| **Freeze (Chilled)** | **Heavy Impact** | **Kırılma** (anti-armor) | Shield-wall'ı kır | YENİ |
| **Tar Pit (slow-zone)** | **Arrow Storm / Lightning** | **Yapay-choke AoE** | Yavaşlatıp barajı | YENİ |
| **Supply Beacon** | **Gold Rush** | **Economic Surge** | Eko-snowball penceresi | YENİ |
| **Smoke (LoS-block)** | **Haste + flank** | **Flanking Advantage** | Gizli kanat → back ×2.0 | YENİ |
| **Stun / Freeze** | **(herhangi AoE)** | **Pin-and-punish** | Sabitle, vur | Kısmî kanon |

### Komşuluk listesi (her büyünün başlıca sinerji partnerleri)
| Büyü | ◆◆◆ İmza | ◆◆ Güçlü | ◆ Orta | ✗ Anti |
|---|---|---|---|---|
| S1 Freeze | S4 Shatter, S13 Heavy Impact | S2 Stun, S5/S6 AoE | S20 Smoke | — |
| S2 Stun | — | S4/S5/S6 (pin-punish), S1 | S13 | S18 (kendi tarafına gereksiz) |
| S3 Poison Cloud | — | S14 Tar Pit (zone-üstü-zone), S2 | S5 | — |
| S4 Shatter | **S1 Freeze** | S2 Stun | S14 (slow→Chilled değil; sınırlı) | — |
| S5 Arrow Storm | — | **S14 Tar Pit**, S1/S2 (sabit hedef) | S20 (sisi temizle) | — |
| S6 Lightning Storm | — | **S14 Tar Pit**, S1/S2 | S3 | — |
| S7 Rage | — | **S8 Haste** (all-in), S11/S12 summon | S17 Ward (dur-ve-vur) | S15 (itiş kendi push'unu bozar) |
| S8 Haste | **S20 Smoke** (flank) | S7 Rage, S11 Pouncer | S15 (reposition) | — |
| S9 Gold Rush | **S16 Supply Beacon** | S10 Raise Gold | S12 Giant (eko→dev) | S15/S5 (savaş büyüsü değil) |
| S10 Raise Gold | — | S12 Giant (anında dev), S16 | S9 | — |
| S11 Pouncer | — | **S8 Haste**, S20 Smoke (gizli flank) | S7 | — |
| S12 Giant | — | S10 Raise Gold (anında), S7 Rage | S17 Ward | — |
| S13 Heavy Impact | **S1 Freeze** | S14 Tar Pit (sabit hedef), S2 | S5 | — |
| S14 Tar Pit | **S5/S6 AoE** | S3 Poison, S13, S2 | S20 | — |
| S15 Gale Wall | — | S17 Ward (it→tut), S2 (kır push) | S5 (geri-itip-vur) | S7/S8 (kendi push'unu sıfırlar) |
| S16 Supply Beacon | **S9 Gold Rush** | S10, S12 (eko→dev) | S17 | S15 |
| S17 Bulwark Ward | — | **S15 Gale Wall**, S12 Giant | S7 | S8 (hareket-buff'ı dur-buff'ıyla çelişir) |
| S18 Cleanse | — | (kendi tarafına: anti-CC kurtarma) | — | — *(rakip combo'ya counter; müttefik-sinerjisi sınırlı)* |
| S19 Far Sight | — | **tüm draft** (bilgi her şeyi besler) | — | — |
| S20 Smoke | **S8 Haste, S11 Pouncer** (flank) | S5/S6 (sisi temizleyen rakibe karşı) | S1 | — |

### Anahtarlı combo dosyası (taktik amaç)
1. **Freeze→Shatter (◆◆◆, kanon):** Donmuş kümeye ×2; *amaç*: Tight formasyon/choke yığılmasını anında imha. *Counter*: dağıl + Cleanse.
2. **Freeze→Heavy Impact (◆◆◆):** Chilled + anti-armor blunt; *amaç*: Iron Pact shield-wall'ını kır. *Counter*: Loose + Cleanse.
3. **Tar Pit→Lightning/Arrow (◆◆◆):** Yapay choke + kalıcı AoE; *amaç*: yavaşlat, sonra alanı tara — kaçışı pahalı kıl. *Counter*: zone'a girmeden dolaş.
4. **Supply Beacon→Gold Rush (◆◆◆ "Economic Surge"):** Üst-üste eko; *amaç*: rakipten önce kompozisyon-üstünlüğü. *Counter*: tempo-bas + feneri contest.
5. **Smoke→Haste-flank (◆◆◆ "Flanking Advantage"):** Görüşü kes, kanattan back ×2.0; *amaç*: ranged hattını arkadan dağıt. *Counter*: kör-takip etme, arkayı kapat, sisi AoE'le.
6. **Stun→AoE (◆◆ pin-punish):** Sabitle, vur; *amaç*: telegraph'tan kaçışı engelle. *Counter*: stagger + Cleanse.
7. **Rage+Haste (◆◆ all-in penceresi):** Saldırı+hız; *amaç*: kesin anda kırıcı push. *Counter*: Gale Wall / disengage / Ward.
8. **Gale Wall→Ward (◆◆ "kale duvarı"):** İt, sonra tut; *amaç*: statue savunması, all-in'i kır. *Counter*: flank (zone sabit), eko ile aş.
9. **Raise Gold→Summon Giant (◆◆):** Anında para→anında dev; *amaç*: sürpriz tempo-spike. *Counter*: Pierce-Ranged ile dev'i erit.

**Ağ ilkesi:** Her ◆◆◆ combo'nun **en az iki** counter ekseni vardır (pozisyon + Cleanse/timing). Bu, "no unavoidable win-combo" yasasının (görev + §5.3) sayısal garantisidir; Part 6 her büyü için bunu tablolar.

---

# PART 6 — Karşıtlık Ağı (Counter Web)

**Dört counter ekseni** (kanon counterNote'ların formalize hali):
- **Doğrudan (Direct):** Etkiyi iptal/söken araç (Cleanse, dispel, focus-fire).
- **Yumuşak (Soft):** Etkiyi azaltan armor/terrain/formasyon (Loose, Cover, type×armor).
- **Zamanlama (Timing):** Telegraph penceresini/CD'yi sömürme (dodge, bait, re-commit).
- **Pozisyon (Positioning):** Geometriyle kaçınma (dağıl, dolaş, flank'i kapat).

| Büyü | Doğrudan | Yumuşak | Zamanlama | Pozisyon |
|---|---|---|---|---|
| S1 Freeze | **Cleanse** | (yok) | telegraph-dodge | **dağıl** (az Chilled) |
| S2 Stun | Cleanse | — | stagger/dodge | kümeyi aç |
| S3 Poison Cloud | Cleanse | — | bulut açılırken çık | bulutu terk et |
| S4 Shatter | Cleanse (Chilled'ı sök) | — | Freeze'i izle | **Chilled iken dağıl** |
| S5 Arrow Storm | — | **Loose formasyon** | telegraph-dodge | işaretli alanı terk et |
| S6 Lightning Storm | — | Loose | uzun pencereden yürü | kümeleme |
| S7 Rage | — | — | pencere bitince commit | disengage |
| S8 Haste | — | — | flank'i öngör | pozisyonu koru |
| S9 Gold Rush | — | — | **tempo-bas** (yatırım penceresi) | madeni contest |
| S10 Raise Gold | — | — | **harcama penceresinde bas** | — |
| S11 Pouncer | **focus-fire spawn** | type×armor (Light) | iniş öncesi vur | flank'i kapat |
| S12 Giant | focus-fire | **Pierce-Ranged (anti-Heavy)** | kite | mesafe koru |
| S13 Heavy Impact | Cleanse (Chilled) | **Loose** (scatter'ı azalt) | koridor-dodge | koridordan çık |
| S14 Tar Pit | — | — | zone bitmesini bekle | **etrafından dolaş** |
| S15 Gale Wall | — | — | **itişten sonra re-commit** | dağıl (az itiş) |
| S16 Supply Beacon | feneri yık/contest | — | tempo-bas | madeni bas |
| S17 Bulwark Ward | (zone'u zorla) | Heavy Impact ile kır | süre bitmesini bekle | **flank** (zone sabit) |
| S18 Cleanse | — | — | **bait + re-apply** | ikinci dalga |
| S19 Far Sight | — | — | bilgi-penceresi kısa | **feint** (yanlış kuyruk) |
| S20 Smoke | AoE'le sisi (Arrow/Lightning) | — | flank zamanını oku | **arkayı kapat**, kör-takip etme |

**Karşıtlık-matrisi okuması:** Her satırda **≥1 dolu eksen** vardır → tek bir büyü counter'sız değildir. **Cleanse (S18)** ve **type×armor / formasyon / terrain** sistemleri, kontrol ve nüke ağının yapısal panzehirleridir. Recon (S19/S20) counter'ları **bilgi/timing** eksenindedir (yeni katman → Part 12 istismar mitigasyonu).

---

# PART 7 — Telegraph Denetimi

Kanon kuralı (`SpellCastSystem`): **telegraphTime > 0 zorunlu; 0 reddedilir.** Görsel + işitsel telegraf, pencere ve okunabilirlik aşağıda. Mobil parçacık bütçesi + color-blind-safe palet sabittir (§10/§11). Kanon büyülerin pencereleri asset'lerden; yeni büyülerinki PROVISIONAL.

| Büyü | Görsel telegraf | İşitsel telegraf | Pencere (sn) | Okunabilirlik notu |
|---|---|---|---|---|
| S1 Freeze | Genişleyen mavi-buz halkası | Kristal-çıtırtı | 1.0 | Yüksek — soğuk-mavi alan-kodu |
| S2 Stun | Sarı şok-halkası | Alçak gümbürtü | 0.9 | Orta — küçük yarıçap, net merkez |
| S3 Poison Cloud | Yavaş açılan yeşil sis | Tıslama | 1.1 | Yüksek — kalıcılık görünür |
| S4 Shatter | Beyaz kırılma-çatlağı | Cam-kırılma | 1.0 | Yüksek — "kırılma" semantiği net |
| S5 Arrow Storm | Geniş yer-hedef dairesi | Islık-çınlama | 1.2 | Yüksek — en geniş, en görünür |
| S6 Lightning Storm | Kara bulut + ön-çakım | Gök gürültüsü | 1.4 | Yüksek — en uzun pencere |
| S7 Rage | Kırmızı müttefik-aura | Savaş-borusu | 0.6 | Orta — müttefik buff (kısa, ally-renk) |
| S8 Haste | Mavi hız-izleri | Yükselen ıslık | 0.6 | Orta — hareket-vektörü ipucu |
| S9 Gold Rush | Madende altın-parıltı | Jeton sesi | 0.5 | Düşük-görünür (savaş-dışı, kabul edilebilir) |
| S10 Raise Gold | HUD altın-akış | Hazine sesi | 0.5 | HUD-only (savaş-dışı) |
| S11 Pouncer | Spawn-pentagramı | Hırıltı | 1.0 | Yüksek — spawn-noktası net |
| S12 Giant | Yer sarsıntısı + gölge | Uzun kükreme | 1.4 | Çok yüksek — büyük tehdit = büyük telegraf |
| S13 Heavy Impact | Ağırlaşan koridor-gölge | Şarj gümbürtüsü | 1.2 | Yüksek — Line koridoru net |
| S14 Tar Pit | Kabarcıklı kahve zemin | Balçık sesi | 1.1 | Yüksek — kalıcı zone görünür |
| S15 Gale Wall | Şişen basınç-yayı | Alçalan uğultu | 1.0 | Orta-yüksek — yön-oku ipucu (itiş yönü) |
| S16 Supply Beacon | Sancak + altın halka | Bayrak/boru | 0.6 | Orta — sabit konum okunur |
| S17 Bulwark Ward | Sancak + mavi kalkan-kubbe | Çan/koruma | 0.7 | Yüksek — kubbe sınırı net |
| S18 Cleanse | Beyaz arınma-dalgası | Çan-tını | 0.6 | Orta — müttefik-renk, kısa |
| S19 Far Sight | Keşif-feneri + radar-ping | Ping | 0.7 | **Çift-yönlü**: rakip "görüldüm" bilir (adil bilgi) |
| S20 Smoke | Yayılan gri sis | Fısıltı/uğultu | 0.8 | **Tasarım dikkati:** sis okunabilirliği DÜŞÜRÜR → sınır net, dost/düşman silüet clarity-mode'da korunur |

**Okunabilirlik denetimi sonucu:** Tüm güçlü etkiler okunur. **İki risk işaretlenir:** (1) **S20 Smoke** doğası gereği görüşü azaltır → ranked'da clarity-mode silüet-koruması ve net zone-sınırı zorunlu; (2) **S19 Far Sight** bilgi verir → çift-yönlü telegraf (rakip ifşa edildiğini görmeli) = adil. Geri kalan 18 büyü standart telegraf-clarity'sine uyar.

---

# PART 8 — Draft Meta Analizi

Draft = **3-of-N, savaş öncesi** (§3 PREPARE). İyi draft = bir **enabler + bir payoff + bir esneklik/eko/sigorta.** Aşağıdaki paketler **kanon havuzdan + (post-launch) yeni havuzdan** örneklenir; her paket **fraksiyon-agnostik** çekirdek + fraksiyon-eğilimi notuyla.

### En iyi 3'lü combo çekirdekleri
- **Donmuş-İmha:** Freeze + Shatter + (Cleanse/Haste). *En saf combo; counter'ı Cleanse.*
- **Choke-Tarama:** Tar Pit + Lightning/Arrow + Stun. *Pozisyon dikte + AoE.*
- **Eko-Snowball:** Supply Beacon + Gold Rush + Summon Giant. *Geç-oyun ölçek.*
- **Gizli-Flank:** Smoke + Haste + Pouncer. *Back ×2.0 baskını.*
- **Kale-Savunma:** Bulwark Ward + Gale Wall + Stun. *All-in kırıcı.*

### Arketipler ve önerilen paketler
| Arketip | Felsefe | Önerilen 3'lü paket | Fraksiyon eğilimi | Birincil counter |
|---|---|---|---|---|
| **Aggressive** | Erken kırıcı tempo | **Rage + Haste + Shatter** (Freeze splash'i için kombine) | Ashen Horde | Gale Wall / Ward / disengage |
| **Defensive** | Yer-tut, attrition'a sürükle | **Bulwark Ward + Gale Wall + Stun** | Iron Pact | eko-aş, flank |
| **Economic** | Eko-üstünlük al, sonra ez | **Supply Beacon + Gold Rush + Raise Gold** | her ikisi | **tempo-bas** (en kritik) |
| **Tempo** | Pencere yarat & sömür | **Haste + Smoke + Pouncer** | Ashen Horde | arkayı kapat, AoE-sis |
| **Control** | Cepheyi dikte et | **Freeze + Tar Pit + Stun** | her ikisi | Cleanse, dağıl |
| **Attrition** | Kalıcı zone + dayanıklılık | **Poison Cloud + Tar Pit + Bulwark Ward** | Iron Pact | zone-dolaş, burst |
| **Recon** | Bilgi-asimetrisi (post-launch) | **Far Sight + Smoke + Haste** | Ashen Horde | feint, kör-takip-etmeme |

**Meta-denge gözlemi:** **Control + Aggressive** doğal olarak güçlü (combo enabler bolluğu) → **Cleanse (S18)** ve **Defensive (Ward/Gale)** paketleri bunları dengeler. **Economic** en büyük "yüksek-tavan/yüksek-risk" arketipidir (tempo-cezası). **Recon** niş ama yüksek-beceri tavanı (yalnızca post-launch, bilgi-sistemi ADR'sinden sonra). Hiçbir arketip strictly-dominant değildir; her birinin tablo-içi counter'ı vardır → ranked-normalizasyon + RemoteConfig tuning ile yönetilir.

---

# PART 9 — Komutan Etkileşim Denetimi

**Çerçeve (ADR-2-002):** Komutan buff'ları (`StatusSource.Commander`) BİRLEŞTİRİLİR ve ≤ `PowerBudgetPct`'e (≤0.15) clamp'lenir; **spell buff'ları AYRI, §6-sınırsız** ama telegraph/counter/CD ile sınırlı katmandır. Bu ayrım, aşağıdaki risklerin **çoğunu yapısal olarak zaten kapatır** — yine de tasarım-düzeyi dikkatleri sıralıyoruz.

### Iron Warden (Rally = Raged+Hasted; Quartermaster = GoldBoost/madenci)
| Etkileşim | Risk | Değerlendirme | Mitigasyon |
|---|---|---|---|
| Rally **+ Rage/Haste** (aynı tür buff) | Çarpımsal stacking | **Düşük** — komutan payı clamp'li; spell payı ayrı & counterlanabilir (disengage/Gale) | ADR-2-002 yeterli; LSD spell-buff tavanını telemetri ile izlesin |
| Quartermaster **+ Gold Rush + Supply Beacon** | Eko-snowball | **Orta** — üç eko kaynağı üst-üste | GoldBoost komutan-payı clamp'li; eko-büyüler uzun-CD + **tempo-cezalanabilir**; diminishing returns (Part 12) |
| Rally **+ Bulwark Ward** | "kır-geçmez" savunma penceresi | **Düşük-orta** | Ward süre-bağlı; flank (zone sabit); Heavy Impact ile kır |

### Ashen Warchief (WarCry = Hasted+Raged, hız-ağırlıklı; Bloodthirst = Raged/savaş birimi)
| Etkileşim | Risk | Değerlendirme | Mitigasyon |
|---|---|---|---|
| WarCry **+ Haste + Smoke** | Gizli yıldırım-flank (back ×2.0) | **Orta-yüksek** — en tehlikeli tempo combo'su | Smoke telegraph'lı; Cleanse hız-değil ama; **arkayı kapat**; Gale Wall reset; ranked clarity-mode |
| Bloodthirst **+ Rage** (Raged stack) | Çarpımsal saldırı | **Düşük** — komutan Raged clamp'li, spell Raged ayrı/counterlanabilir | ADR-2-002; disengage; pencere-bekle |
| WarCry **+ Pouncer/Raider flank** | Erken all-in | **Orta** | Gale Wall; Ward; defensive draft |

### Genel stacking/komutan riskleri & mitigasyonları
1. **"Aynı-tür çarpımsal buff" (en klasik risk):** ADR-2-002 ile **çözüldü** — komutan katmanı clamp'li, spell katmanı ayrı. Yeni buff/Support büyüleri (Ward, Cleanse, Rage/Haste varyantları) **bu ayrımı korumalı**: yeni büyüler `StatusSource.Spell` yazar, asla komutan-bütçesine dokunmaz.
2. **Eko-komutan + eko-büyü snowball'u:** En gerçek combinatorial risk. Mitigasyon: GoldBoost komutan-payı clamp + eko-büyülerde diminishing/uzun-CD + tempo-cezalanabilirlik + telemetri-RC tavanı.
3. **Yeni "Warded" status'u (S17) + komutan:** Yeni status eklenirse, komutan-kaynaklı bir savunma-buff'ı **ADR-2-002 clamp'ine dahil edilmeli** (yoksa yeni bir bütçe-sızıntı yüzeyi). → Part 13 backend notu.
4. **Recon + komutan:** Bilgi büyüleri güç-buff'ı vermez → komutan-bütçe etkileşimi yok; risk **bilgi-istismarı** ekseninde (Part 12), güç ekseninde değil.

**Sonuç:** Komutan ≤%15 kanonu ve ADR-2-002 katman-ayrımı, sinerji ağını genişletirken bile **airtight** kalır — **şart:** her yeni buff/Support büyüsü `StatusSource.Spell` yazsın ve hiçbir yeni komutan-kaynaklı buff bütçe-clamp'i atlamasın.

---

# PART 10 — Gelecek Genişleme Kapasitesi (20 → 40+)

**Hedef:** Havuz 20→40+ büyürken **denge yönetilebilir** ve **draft okunur** kalsın. Strateji **veri-odaklı** (kod değil):

1. **Tag-tabanlı sinerji (N² yok).** Sinerji **status üzerinden** çözülür (`appliesStatus` → `synergyBonusVsStatus`), elle yazılmış 40×40 tablo değil. Yeni büyü yalnızca **mevcut status'lere bağlanır** → combinatorial patlama **lineer** kalır. Bu zaten implemente (`SynergyFactor`).
2. **Status disiplini (sert ADR kapısı).** 8 kanon StatusKind. **Her yeni status = bir ADR + bir counter (Cleanse kapsamı) + bir telegraph.** Hedef: 40 büyüde ≤12 status. Yeni status, yeni combinatorial maliyetin **ana kaynağıdır** → en pahalı genişleme ekseni; nadir kullanılır.
3. **Kategori bütçeleri.** 40-havuz için önerilen dağılım (PROVISIONAL): Offensive ~7 · Control ~9 · Economy ~5 · Support ~10 · Recon ~5 · (rezerv ~4). **Recon kategorisi yalnızca bilgi-sistemi ADR'sinden sonra** açılır.
4. **Draft okunabilirliği sabiti.** Her zaman **3-of-N**; havuz büyüse de draft 3 kalır. Tooltip **sinerji-tag'lerini ve counter-note'u** gösterir; "broken draft" telegraf edilir.
5. **Her yeni büyü sözleşmesi (ADR şablonu):** `telegraph>0` · `counterNote` · **≥1 sinerji-bağı** · **≥1 net counter ekseni** · distinct rol · §15.6 provisional-değer. Bu olmadan büyü gönderilmez (§5).
6. **İçerik kadansı.** Büyüler **post-launch havuz büyümesi** (Blueprint §2), sezon tek-slotu {birim|komutan|harita}'dan **ayrı**, kendi ADR-akışıyla — küçük partiler (örn. sezon başına 1–2 büyü) telemetri ile dengelenir.
7. **Telemetri + RemoteConfig.** Tüm değerler RC-tunable (§12); pick/win-rate izlenir; outlier'lar app-update'siz retune edilir; ranked normalizasyon strictly-dominant'ı engeller.

**Genişleme adayları (Part 4 dışı, not edildi):** *Sabotage/Pillage* (eko-denial — düşman madenciliğini düşür; bilgi-eko köprüsü), *Ambush* (ayrı büyü olarak — şu an Smoke+flank maneuver'ıyla karşılanıyor), terrain-spesifik varyantlar (HighGround/Cover ile etkileşen). Hepsi ADR-kapılı.

---

# PART 11 — Ana Sinerji Tablosu (Master Reference)

**Karmaşıklık derecesi (1–5):** 1 = okunur/tek-etki; 5 = çok-sistemli/yüksek-beceri. **Kapsam** = yeni-sistem maliyeti (`[KANON]` = 0).

| # | Büyü | Kanon Kat. | Araştırma Rolü | Başlıca Counter | Başlıca Sinerji | Arketip Uyumu | Komutan Uyumu | Karmaşıklık | Kapsam |
|---|---|---|---|---|---|---|---|---|---|
| S1 | Freeze | Control | Control | Cleanse, dağıl | →Shatter, →Heavy Impact | Control, Aggressive | her ikisi | 2 | KANON |
| S2 | Stun | Control | Control | stagger, Cleanse | →AoE (pin) | Control, Defensive | her ikisi | 2 | KANON |
| S3 | Poison Cloud | Control | Control | bulut-terk | +Tar Pit | Attrition, Control | Iron Pact | 2 | KANON |
| S4 | Shatter | Offensive | Offensive | Chilled-olma, Cleanse | ←Freeze (×2) | Aggressive, Control | her ikisi | 3 | KANON |
| S5 | Arrow Storm | Offensive | Offensive | Loose, dodge | +Tar Pit, +Stun | Aggressive | her ikisi | 2 | KANON |
| S6 | Lightning Storm | Offensive | Offensive | yürü-çık | +Tar Pit | Control, Aggressive | her ikisi | 3 | KANON |
| S7 | Rage | Buff | Support | disengage | +Haste | Aggressive, Tempo | **Warchief (Bloodthirst)** | 2 | KANON |
| S8 | Haste | Buff | Support | pozisyon-koru | +Smoke (flank) | Tempo, Recon | **Warchief (WarCry)** | 2 | KANON |
| S9 | Gold Rush | Economy | Economy | tempo-bas | +Supply Beacon | Economic, Attrition | **Warden (Quartermaster)** | 2 | KANON |
| S10 | Raise Gold | Economy | Economy | spend-window bas | +Giant | Economic, Tempo | Warden | 1 | KANON |
| S11 | Summon Pouncer | Summon | Support | focus-fire | +Haste, +Smoke | Tempo, Aggressive | Warchief | 2 | KANON |
| S12 | Summon Giant | Summon | Support | Pierce, kite | +Raise Gold, +Rage | Economic, Defensive | her ikisi | 3 | KANON |
| S13 | Heavy Impact | Offensive | Offensive | Loose, Cleanse | ←Freeze, +Tar Pit | Aggressive, Control | Warden | 4 | orta:ADR |
| S14 | Tar Pit | Control | Control | dolaş | +AoE (Arrow/Lightning) | Control, Attrition | her ikisi | 3 | orta:ADR |
| S15 | Gale Wall | Control | Control | re-commit, dağıl | +Ward, +Stun | Defensive, Tempo | her ikisi | 4 | yüksek:ADR |
| S16 | Supply Beacon | Economy | Economy/Support | feneri-bas | +Gold Rush (Surge) | Economic, Attrition | **Warden (Quartermaster)** | 3 | orta:ADR |
| S17 | Bulwark Ward | Buff | Support | flank, Heavy Impact | +Gale Wall | Defensive, Attrition | **Warden (Rally)** | 3 | orta:ADR (yeni status) |
| S18 | Cleanse | (Buff?) | Support | bait+re-apply | (anti-CC, müttefik) | Defensive, Control, Tempo | her ikisi | 3 | düşük:ADR (verb) |
| S19 | Far Sight | **(yok)** | Recon | feint, kısa-pencere | tüm draft (bilgi) | Recon | nötr | 4 | **çok yüksek:ADR (bilgi sistemi)** |
| S20 | Smoke Screen | Control | Recon/Control | arkayı-kapat, AoE-sis | +Haste, +Pouncer (flank) | Tempo, Recon | **Warchief** | 4 | yüksek:ADR (LoS/zone) |

*Not: S18 (Cleanse) ve S19 (Far Sight) **mevcut `SpellCategory` enum'una temiz eşleşmez** → kategorizasyon ADR'si şart (S18→Buff/utility'e veya yeni "Support"; S19→yeni "Recon"). İşaretlendi (çelişki-kontrolü §1.6).*

---

# PART 12 — Denge & Exploit Denetimi

| İstismar sınıfı | Senaryo | Risk | Mitigasyon planı |
|---|---|---|---|
| **Sonsuz döngü** | Cleanse + uzun savaş = kontrol asla iş görmez | Düşük | Cleanse uzun-CD (~20) + charge 1; bait-edilebilir; ikinci CC-dalgası geçer. Asla "perma-immunity" değil |
| **Sonsuz eko** | Supply Beacon + Gold Rush + Quartermaster + Raise Gold üst-üste | **Orta-yüksek** | (1) GoldBoost komutan-payı clamp'li; (2) eko-büyüler uzun-CD + charge 1; (3) **diminishing returns** GoldBoost mag üzerinde (kanon diminishing deseni); (4) **tempo-cezalanabilirlik** (yatırım penceresi); (5) RC win-rate tavanı |
| **Baskıcı combo** | Freeze→Shatter→Heavy Impact zincir-CC + burst = kaçışsız | **Orta** | (1) **Cleanse** doğrudan counter; (2) Chilled/Stun **diminishing** (kanon WC3 deseni — tekrarlı CC süresi azalır); (3) charge/CD limitleri; (4) **dağıl** pozisyon-counter'ı her zaman var; (5) telegraph pencereleri (1.0–1.4) dodge'a izin verir |
| **Kaçışsız zafer** | Tek 3'lü draft maçı garantiler | **Yasak (§5.3 + görev)** | Part 6 her büyüye **≥1 dolu counter ekseni** garanti eder; Part 8 her arketipe counter; ranked normalizasyon; "no strictly-dominant" |
| **Ekonomik istismar** | Raise Gold spam → anında ordu | Düşük | charge 1 + CD 25 + spend-window cezası; sunucu eko (Gold in-battle, non-persistent) — meta-cüzdan etkilenmez |
| **Bilgi istismarı (Recon)** | Far Sight kalıcı görüş / draft tam-ifşa = okuma-oyununu öldürür | **Yüksek (bu yüzden ADR-kapılı)** | (1) **Bilgi bozulur** (kısa pencere); (2) çift-yönlü telegraf (rakip ifşa-edildiğini bilir); (3) **feint** counter'ı; (4) draft-gizliliği bir ön-koşul ADR; (5) **asla** kalıcı/pasif tam-görüş. Recon büyüleri yalnızca bilgi-sistemi + bu mitigasyonlar onaylanırsa |
| **Yeni-status sızıntısı** | "Warded" gibi yeni status komutan-bütçesini atlar | Orta | Yeni savunma-status'u **ADR-2-002 clamp'ine dahil** + yalnızca `StatusSource.Spell` (spell-katmanı); komutan-kaynaklı varyant ayrı ADR |
| **VFX/okunabilirlik istismarı** | Smoke ile kendi combo'sunu gizleme | Orta | Ranked clarity-mode silüet-koruması; net zone-sınırı; telegraf parçacık-bütçesi sabit (§11) |

**Genel ilke:** Hiçbir combo **counter'sız** değildir; her güçlü etki **telegraf'lı + diminishing/CD-limitli + pozisyon-kaçışlı**. En yüksek iki risk — **eko-snowball** ve **bilgi-istismarı** — en sıkı kapılanan iki ekseni (eko diminishing/tempo; Recon ADR + bilgi-bozulması) açıklar.

---

# PART 13 — Implementasyon Hazırlığı

> **Maliyet = yalnızca tahmin; implementasyon YETKİSİ YOK.** `[KANON]` 12 büyü için yeni-sistem maliyeti ~0 (zaten authored); maliyet 8 yeni büyüden gelir.

### Gerekli gameplay sistemleri (yeni)
| Sistem | Hangi büyüler | Maliyet | Not |
|---|---|---|---|
| **Status-cleanse verb** | S18 | **Düşük** | `StatusEffect` buffer'dan negatif girdileri sil; `StatusQuery`/`Spell.cs`'e küçük ek |
| **Spell-spawn'lı dinamik terrain** | S14 | Orta | Mevcut `TerrainFeature`/`TerrainOccupancy` yeniden kullan; süre-bağlı spawn/despawn |
| **Zone-buff entity (eko/savunma)** | S16, S17 | Orta | Kalıcı alan-entity'si; GoldBoost (var) + yeni "Warded" status |
| **Yeni StatusKind "Warded"** | S17 | Orta | Enum + `StatusQuery.IncomingDamageMultiplier` zaten Cover için var → yeniden kullanılabilir |
| **Displacement/knockback** | S15 | **Yüksek** | Pozisyon yazımı + kohezyon; non-deterministic sim'de dikkat |
| **Formasyon-scatter** | S13 | Orta | `FormationMember`/cohesion'a etki (önce FormationMember kablolaması şart!) |
| **Bilgi/görüş/fog + gizli-bilgi** | S19, (S20 kısmî) | **Çok yüksek** | MVP'de yok; LoS (S20) Cover kavramına yaslanır; tam-ifşa (S19) yeni katman |

### VFX gereksinimleri
- Her büyü için **telegraf VFX'i** (Part 7 tablosu) + resolve VFX'i; **damage-type-kodlu palet** (§10); mobil **parçacık bütçesi** sabit.
- **En riskli:** S20 Smoke (okunabilirlik!), S6 Lightning (kalıcı alan), S12 Giant (büyük). Clarity-mode varyantları gerekir.

### UI gereksinimleri
- **Draft ekranı** (3-of-N, savaş öncesi) + **3 slot HUD** + cooldown/charge göstergesi (zaten §10 HUD'da "3 spell slots").
- **Sinerji-tag + counter-note tooltip'leri** (okunabilirlik & "broken draft" telegrafı).
- Recon için: ifşa-overlay (S19), zone-sınır göstergesi (S14/S16/S17/S20).

### Backend gereksinimleri
- **Büyü sahipliği server-authoritative** (oynayarak kazanılır; §9); **draft owned-pool'dan, sunucu-doğrulanmış** (anti-cheat; ghost-ladder `StatSanityValidator` over-pool draft'ı reddetsin).
- **Hiçbir büyü gems/IAP ile güç olarak satılmaz** (CUT); yalnızca VFX-recolor kozmetik.
- Tüm sayısal değerler **RemoteConfig** (`EconomyResolver`/`ConfigResolver` deseni) → app-update'siz tuning.
- **ADR-2-002 clamp'i** yeni buff/Support büyülerinde korunmalı (yeni status komutan-bütçesini atlamasın).

### Dengeleme maliyeti
- **En yüksek proje riski #5 (combinatorial balance).** 20 büyü × 8 status × terrain × formasyon × 2 komutan = geniş uzay. Mitigasyon: **tag-tabanlı sinerji** (lineer), **telemetri-RC tuning**, **ranked normalizasyon**, **küçük partiler halinde release**. İlk 12 (kanon) zaten dar/dengelenebilir.

---

# PART 14 — Nihai Tavsiye

### 20 büyünün sıralaması (sinerji-derinliği × okunabilirlik × counterplay × düşük-sistem-maliyeti)

| Sıra | Büyü | Gerekçe | Sınıf |
|---|---|---|---|
| 1 | **Freeze** | Ağın #1 enabler'ı; kanon; okunur | **MVP** |
| 2 | **Shatter** | Referans imza-combo payoff'u; kanon | **MVP** |
| 3 | **Stun** | Çok-yönlü pin; kanon | **MVP** |
| 4 | **Haste** | Tempo/flank motoru; kanon | **MVP** |
| 5 | **Rage** | All-in penceresi; kanon | **MVP** |
| 6 | **Arrow Storm** | Anti-formasyon AoE; kanon | **MVP** |
| 7 | **Poison Cloud** | Alan-reddi; kanon | **MVP** |
| 8 | **Gold Rush** | Eko ekseni; kanon | **MVP** |
| 9 | **Lightning Storm** | Kalıcı AoE + Burning; kanon | **MVP** |
| 10 | **Summon Giant** | Anti-Heavy/statue; kanon | **MVP** |
| 11 | **Raise Gold** | Eko-spike; kanon | **MVP** |
| 12 | **Summon Pouncer** | Esnek flank; kanon | **MVP** |
| 13 | **Cleanse** | Kontrol-ağına panzehir; **düşük sistem maliyeti, yüksek denge değeri** | **Launch** |
| 14 | **Tar Pit** | Yapay choke = AoE platformu; orta maliyet | **Launch** |
| 15 | **Heavy Impact** | Anti-armor/formasyon; görev örnek-combo'su | **Launch** |
| 16 | **Supply Beacon** | Pozisyonel eko; "Economic Surge" | **Launch** |
| 17 | **Bulwark Ward** | Defensive kimlik; yeni status (orta) | **Post-launch** |
| 18 | **Smoke Screen** | Flank/recon; okunabilirlik dikkati (yüksek) | **Post-launch** |
| 19 | **Gale Wall** | Displacement; **en ağır kontrol mekaniği** | **Post-launch** |
| 20 | **Far Sight** | Bilgi savaşı; **bilgi-sistemi ADR'si şart (çok yüksek)** | **Post-launch** |

### Sınıflandırma özeti
- **MVP adayı (12):** Tam olarak **mevcut kanon havuz** (S1–S12) — sıfır yeni-sistem, kanıtlanmış, dengelenebilir, tüm 5 kanon kategoriyi kapsar.
- **Launch adayı (4):** Cleanse, Tar Pit, Heavy Impact, Supply Beacon — düşük/orta ADR, ağı belirgin derinleştirir, mevcut status/terrain'i yeniden kullanır.
- **Post-launch adayı (4):** Bulwark Ward, Smoke, Gale Wall, Far Sight — yeni status/sistem (zorlu ADR); en yüksek tasarım-değeri ama en yüksek maliyet/risk.

### Önerilen **ilk 12-büyü launch roster'ı**
**Mevcut kanon 12'yi (S1–S12) koru.** Gerekçe: (1) **sıfır yeni-sistem riski** — hepsi authored + CI-derleniyor; (2) **tüm 5 kanon kategoriyi** dengeli kapsar (3 Offensive / 3 Control / 2 Economy / 2 Buff / 2 Summon); (3) **kanıtlanmış imza-combo** (Freeze→Shatter) içerir; (4) her biri telegraph+counter taşır; (5) **GATE 1/2/3 burn-down'ı bu 12 ile yapılır** — yeni büyü eklemek doğrulama borcunu artırır.

> **Köprü tavsiyesi:** İlk genişlemede (post-GATE, post-launch S1) **Cleanse**'i ekle (düşük maliyet, kontrol-meta'sını dengeler), ardından **Tar Pit + Heavy Impact** (combo-derinliği). Bu, 12→15 yolunu **en düşük risk / en yüksek denge-getirisi** ile açar. Her ekleme **ayrı ADR + telemetri** ile.

---

## Kapanış

Bu rapor **yalnızca gelecek araştırmadır.** Hiçbir implementasyon yapılmadı; hiçbir roadmap/canon/decision-log/ADR dosyası değiştirilmedi; hiçbir kapı PASS işaretlenmedi; hiçbir özellik yetkilendirilmedi. Tüm sayısal değerler **PROVISIONAL/LSD-owned** (§15.6). Önerilen her **net-yeni** büyü, status, kategori ve sistem **bir ADR ile** kanon-kapısından geçmek zorundadır (§15, kanon kapalı).

**Aktif geliştirme akışı değişmeden devam eder:** CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi (GATE 1 fun → GATE 2 playtest → GATE 3 server-validated). Bu rapordaki tasarımlar yalnızca **full-vision "deep synergy web"** (Decision Log §4) için ön-hazırlıktır ve **MVP'nin ~12-büyü havuzunu telemetri ile dengeledikten ve soft-launch LTV kapısını geçtikten sonra**, post-launch içerik akışında değerlendirilmelidir.

*Felsefe sabit: counterplay · telegraphing · formasyon/terrain etkileşimi · zamanlama ustalığı · savaş-alanı kontrolü — basit hasar enflasyonu ve kaçışsız combo DEĞİL.*

— *Hazırlayan kalite çıtası: Lead RTS Designer · Combat Designer · Systems Designer · LiveOps Designer.*
