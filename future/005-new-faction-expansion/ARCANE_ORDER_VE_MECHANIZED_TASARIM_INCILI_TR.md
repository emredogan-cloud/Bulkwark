# BULWARK — Arcane Order & Mechanized Dominion Fraksiyon Tasarım İncili (Gelecek Araştırma)

> **⚠️ STATÜ: GELECEK ARAŞTIRMA İZİ — YALNIZCA TAVSİYE NİTELİĞİNDE.**
> Bu belge **aktif geliştirme akışının parçası DEĞİLDİR.** Aktif akış şudur: **CI/CD doğrulama · APK üretimi · Unity doğrulama · kalan Phase 0–3 kapı borç-eritimi.**
> **Bu belge HİÇBİR ŞEYİ:** roadmap'i değiştiremez · kanonu değiştiremez · karar günlüğünü (decision log) değiştiremez · gelecek özellik yetkilendiremez · üretim önceliklerini değiştiremez · implementasyon başlatamaz · güç bütçelerini değiştiremez · onaylanmamış mekanik/birim/fraksiyon ekleyemez.
> **Bu belge YALNIZCA:** keşfeder · analiz eder · tavsiye eder · değerlendirir.
> **Konum:** `future/005-new-faction-expansion/` (canon dökümanlarına dokunulmadı; hiçbir `report/`, `docs/adr/`, `decision log`, `docs/execution/` dosyası değiştirilmedi).
> **Kanonik yuva:** Bu araştırma, **Roadmap §13 Phase 7.3 — "3rd faction"** (3. fraksiyon) ile **full-vision 4. fraksiyon** hedeflerini önceden tasarlar. Aday fraksiyonlar **kanonda isimle anılır**: Roadmap **§5.1** — *"(Future) … Arcane order (caster-centric), Mechanized (siege/structures)"*; Successor Report **§8** — *"an Arcane order (caster-centric, fragile), a Mechanized faction (siege/structures)."* Yani bunlar **icat değil, kanonun öngördüğü** full-vision fraksiyonlardır — ancak **implementasyonu DEFERRED'dir.** Tetikleyici (Decision Log **§2**): *"Two-faction balance stable in telemetry"* (iki-fraksiyon dengesi telemetride stabil). Bu tetikleyici **henüz ateşlenmemiştir** (GATE 1/2/3 hâlâ DEFERRED → §1.2) → **hiçbir şey implemente edilmez.**
> **Tarih:** 2026-06-04 · **Dil:** Türkçe · **Kalite çıtası:** Lead RTS Designer + Faction Designer + Combat Designer + Systems Designer.

---

# Proje Derin Analizi

*(ZORUNLU ön-araştırma fazı — atlanmadı. Aşağıdaki rekonstrüksiyon şu kaynakların okunmasına dayanır: 5 kanonik döküman [`BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`, `ROADMAP_CHANGELOG.md`, `PRODUCTION_DECISION_LOG.md`, `NEXTGEN_RTS_SUCCESSOR_REPORT.md`, `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`] · 5 ADR [0-001, 0-002, 1-001, 2-001, 2-002] · 8 execution-prompt [Phase 0–7 + sistem] · Phase 0–3 implementasyon kodu ve verisi [`CombatTypes.cs`, `UnitDef.cs`, `CommanderDef.cs`, `BalanceConfig.cs`, `Phase2Components.cs`, `Combat.cs`, `CounterMatrix.cs`, `Formation.cs`, `Terrain.cs`, `Spell.cs` + `Data/Units/*.asset` (12 birim) + `Data/Balance/CounterMatrix.asset` + `Data/Commanders/*.asset` (2 komutan)] · 4 faz-tamamlanma raporu · `FIRST_COMPILE_REPORT.md` · `SCAFFOLDING_STATUS.md` · `FormationMember_wiring_plan.md` · komşu gelecek-izleri 000/001/002.)*

## 1.1 Mevcut proje durumu (rekonstrüksiyon)

BULWARK, *Stick War* halefi olan mobil-öncelikli (Android/iOS), tek-cepheli, **doğrudan-kontrollü taktiksel RTS-lite** (Unity 6 LTS · IL2CPP · URP-2D · **ECS/DOTS battle-sim**). Çekirdek döngü kanon olarak korunur: **maden → eğit → ittir → heykeli yık** (§3). Üretim felsefesi: on yıl kanıtlanmış çekirdeği koru; orijinali sınırlayan **iki** şeyi modernize et — (a) sığ savaş alanı → *terrain + formasyon + type×armor counter + positional flank + draft-3 büyü sinerjisi*; (b) kırılgan client-trust ekonomi → *server-authoritative*. Monetizasyon **etik, kozmetik + battle-pass öncülüğünde**; **asla güç satmaz** (§2, §9, §10).

| Eksen | Kanonik değer |
|---|---|
| **Tür** | Doğrudan-kontrollü taktiksel RTS-lite — tek-cephe; PvE + async rekabet |
| **Sütunlar (Pillars)** | P1 Agency · P2 Okunabilir derinlik · P3 Adil ustalık · P4 Oyuncuya saygı |
| **İhlal-edilemez kısıtlar** | Okunabilirlik · adalet/no-P2W · para-birimi server-otoritesi · save-state log'lamama · perf bütçesi · §15 CUT listesi |
| **MVP içerik kanonu** | **2 fraksiyon** (Iron Pact, Ashen Horde) · **12 birim (6+6)** · 2 komutan (1/fraksiyon) · ~12 büyü (draft 3) · 3 harita · 4 para birimi (Gold/Silver/Gems/PassXP) |
| **Full-vision içerik hedefi** | **4 fraksiyon** (Iron Pact, Ashen Horde, **Arcane Order**, **Mechanized**) · ≤9 birim/fraksiyon (hard-cap) · komutan rosteri · biome'lar |
| **Bu araştırmanın hedefi** | Phase 7.3 (3. fraksiyon) + full-vision (4. fraksiyon) — **Arcane Order & Mechanized Dominion** için yalnızca **ön-tasarım doktrini** |

## 1.2 Faz durumu & doğrulama borcu (Current State Reconstruction)

**Kullanıcı beyanı:** Phase 0 COMPLETE · Phase 1 COMPLETE · Phase 2 COMPLETE · Phase 3 COMPLETE · Phase 4 NOT STARTED.

**Dürüst implementasyon gerçeği (ADR'ler + faz raporları + ilk-derleme raporu + son commit'lerden):** Tüm Phase 0–3 **deliverable'ları AUTHORED, canon-verified, integration-audited ve commit edildi.** `FIRST_COMPILE_REPORT.md` (run **26884262912**, sha `363076e`) ile kod tabanı artık **CI'da 0 hata ile DERLENİYOR** (Unity 6000.0.75f1; Entities-IL + Burst post-processor'lar `Bulwark.Sim` üzerinde çalıştı). İlk-derleme raporundan **bu yana** depo, doğrulama borcunu eritmeye başladı: son commit'ler **Unity Editor ilk konfigürasyonu** (`1b1b12b`), **geçerli asset YAML + Android SDK seviyeleri** (`b74afee`) ve **test-runner check-run** CI'ı (`d9db777`) içeriyor — yani §7'deki "tek-seferlik Unity-editör kurulumu" **şu anda aktif olarak eritiliyor.** Yine de **çalışma-zamanı doğrulama kapıları hâlâ DEFERRED** (PASS değil):

| Kapı / Öğe | Durum | Kaynak |
|---|---|---|
| Phase 0 çıkış | **CONDITIONALLY ACCEPTED** (deferred validations outstanding) | ADR-0-002 |
| **Compile (Phase 0–3)** | **PASS** (stabil, 0 hata, CI kanıtlı) | FIRST_COMPILE_REPORT §2 |
| Unity-editör konfigürasyonu (URP global / saved scene / Addressables / Android SDK) | **IN PROGRESS** (ilk konfig + Android SDK + asset YAML commit'lendi) | git `1b1b12b`/`b74afee`, FIRST_COMPILE_REPORT §7 |
| Android build / APK | **FAIL → DEFERRED** (henüz APK üretilmedi) | FIRST_COMPILE_REPORT §3, §5 |
| EditMode/PlayMode test | **DID NOT EXECUTE → DEFERRED** | FIRST_COMPILE_REPORT §4 |
| GATE 1 (FUN) | **OPEN / DEFERRED** (on-device "eğlenceli mi?" verdict çalıştırılmadı) | ADR-1-001, ADR-2-001 |
| GATE 2 (vertical-slice playtest) | **DEFERRED** (≥%40 session-2 + "okunur & eğlenceli" rubriği çalıştırılmadı) | ADR-2-001, phase-2 raporu |
| GATE 3 (MVP feature-complete; ekonomi server-validated) | **DEFERRED**; **Phase 4'e yetki = WITHHELD** | ADR-2-001, phase-3 raporu §10 |
| FormationMember kablolaması | **ERTELENDİ** (formasyonlar authored; üyelik ataması yok) | FormationMember_wiring_plan.md |

**Aktif blokerler / kalan doğrulama borcu:** (1) Tek-seferlik Unity-editör konfigürasyonu tamamlanmalı → APK + test yürütmeyi açar; (2) ardından **GATE 1 fun → GATE 2 playtest → GATE 3 server-validated** sırayla eritilmeli; (3) PlayFab canlı entegrasyon. Bu borç eritilene dek tüm çalışma-zamanı kapıları DEFERRED. **Hiçbir ihlal-edilemez kısıt gevşetilmedi** — yalnızca *doğrulama zamanlaması* ertelendi (ADR-0-002 §4).

> **Bu araştırma için anlam (kritik):** Yeni fraksiyon = **Phase 7.3 / full-vision** — yani **çok uzak.** Phase 7.3'ün tetikleyicisi *"iki-fraksiyon dengesi telemetride stabil"* (Decision Log §2). Telemetri için önce **GATE 1 (fun) → GATE 2 (playtest) → GATE 3 (server-validated) → GATE 5 (soft-launch LTV SCALE-OR-STOP)** geçmeli ve **mevcut 2 fraksiyon canlıda dengelenmeli.** Bunların hiçbiri henüz olmadı. Bu rapor o günü **önceden** hazırlar — bugünü değiştirmez. **3. fraksiyon, mevcut 12 birim eğlenceli ve adil olduğu *kanıtlanmadan* asla başlamaz.**

## 1.3 Fraksiyon-ilgili kanon denetimi (canon audit)

Bu, raporun temelidir. Yeni fraksiyonlar aşağıdaki **kapalı kanonun** (§15: "Canon is closed") üstüne inşa edilir; hiçbiri yeniden icat edilmez.

### 1.3.a Fraksiyon felsefesi (kanon kuralları)

| Kanon kuralı | Kaynak | Bağlayıcılık |
|---|---|---|
| Her fraksiyon = **doktrin + ayrı kit + net güç/zaafiyet** | Blueprint §5, Roadmap §5.1 | Yapısal |
| **Asimetri ZORUNLU** (replayability + ranked çeşitliliği) | §5.1 | Tasarım kısıtı |
| **Hiçbir fraksiyon kesin-baskın (strictly dominant) olamaz** | §5.1 | **İHLAL EDİLEMEZ (fairness)** |
| Her fraksiyon **TAM 6 birim** sahalar; asimetri = **hangi rolü ekler/çıkarır** | §5.2 | Yapısal |
| Full-vision **hard-cap = ≤9 birim/fraksiyon** (dengeyi korumak için) | §5.2 | Yapısal |
| **Hiçbir birim, ayrı bir rol + counter olmadan gönderilmez** | §5 ("Restraint is canonical") | Tasarım kısıtı |
| Modlar **TEK savaş çekirdeğini** paylaşır; hiçbir mod/içerik **dengeyi çatallayan bespoke mekanik** getirmez | §7, §15.7 | **İHLAL EDİLEMEZ** |
| Okunabilirlik (silüet/renk/telegraph) **ihlal edilemez** | §6, §11, §12 | **İHLAL EDİLEMEZ** |

### 1.3.b Iron Pact doktrini (mevcut — kopyalanmayacak, *farklılaşılacak* referans)

- **Fantezi:** kırılamaz kalkan-duvarı ordusu; savaş = yıpratma + disiplin (Blueprint §5).
- **Doktrin:** hattı tut, zırhta üstün gel, uzun dövüşü kazan.
- **Kimlik:** dayanıklılık, formasyon, frontal-denial; düşük burst, yavaş.
- **Roster (6):** Miner · **Shieldman** (Frontline/Shielded) · **Legionary** (Skirmisher/Heavy-zırh) · **Crossbow** (Ranged/Pierce/Light) · **Battlemage** (Caster/Fire/Light) · **Ironclad** (Heavy/Heavy-zırh).
- **Çıkardığı rol:** **Flanker yok** (Frontline'ı var).
- **Zaafiyetler:** flanking, Magic, mobilite, burst-tempo.

### 1.3.c Ashen Horde doktrini (mevcut — referans)

- **Fantezi:** hızlı, harcanabilir bir dalga; sen hazır olmadan ezer (Blueprint §5).
- **Doktrin:** tempo, flank, swarm; erken bitir.
- **Kimlik:** hız, ucuz kütle, flanking burst; kırılgan, stall'da ekonomi zayıf.
- **Roster (6):** Miner · **Raider** (Skirmisher/Light) · **Slinger** (Ranged/Blunt/Light) · **Hexcaster** (Caster/Poison/Light) · **Razorbeast** (Heavy/Heavy-zırh) · **Houndmaster** (Flanker/Light).
- **Çıkardığı rol:** **Frontline yok** (Flanker'ı var; Razorbeast/Heavy hattı tutar).
- **Zaafiyetler:** sürekli savunma, AoE, yıpratma, stall'lanmış ekonomi.

### 1.3.d 7-rol paleti (kanon §5.2 — LAW)

Paylaşılan **7-rol paleti**: **Miner · Frontline · Skirmisher · Ranged · Caster · Heavy · Flanker.** Her fraksiyon Miner + bu 6 rolden **5'ini** sahalar (tam 6 birim → **tam 1 rol çıkarır**). Bu raporun yeni fraksiyonları bu paleti **birebir** kullanır.

> **⚠️ ÇELİŞKİ İŞARETİ (görev metni ↔ kanon):** Görev metnindeki "rol paleti" şunları listeler: *Miner, Frontline, Skirmisher, Ranged, **Specialist**, **Siege/Elite**, **Commander Support Logic**.* Bu, kanonik palet **değildir** ve gevşek eş-anlamlılar içerir. **Kanon LAW'dır**; eşleme şöyle yapılır: **"Specialist" → `Caster`** (kanon §5.2); **"Siege/Elite" → `Heavy`** (kanon §5.2); **"Commander Support Logic" → birim DEĞİL → ayrı Komutan sistemi** (§5.5/§6 → bu raporun **Part 12**'si). Kanonun **`Flanker`** rolü görev listesinde yok ama kanonda **vardır** ve bu rapor onu kullanır. Özet: **6 birim = Miner + 5 kanon-rolü; komutan, 6-birim rosterinin parçası değildir.**

### 1.3.e Type × Armor matrisi (implementasyon gerçeği — denetimin omurgası)

Kod, **5 hasar-tipi × 4 zırh-sınıfı** matris taşır (`CounterMatrix.asset`, `BalanceConfig.cs`). **İhlal-edilemez kural:** yeni fraksiyonlar **yeni zırh-sınıfı / hasar-tipi EKLEYEMEZ** (ekleme = ADR + matris-genişlemesi + power-creep riski). Mevcut matris (PROVISIONAL/LSD-owned değerler, `CounterMatrix.asset`):

| ↓Hasar \ →Zırh | **Light** | **Shielded** | **Heavy** | **Unarmored** |
|---|---|---|---|---|
| **Melee** | 1.25 | 0.75 | 0.9 | **1.3** |
| **Pierce** | 1.0 | **1.5** | 0.8 | 1.1 |
| **Blunt** | 0.9 | **1.4** | **1.2** | 1.0 |
| **Fire** | **1.3** | 1.1 | 1.0 | **1.4** |
| **Poison** | 1.2 | 0.9 | 1.1 | **1.3** |

- **Zırh sınıfları (4):** `Light · Shielded · Heavy · Unarmored`. *(Blueprint §4 bu 4.'yü "Structure" olarak da anar; implementasyonda enum adı `Unarmored`. İsim farkı PROVISIONAL/ADR-owned bir yuvadır — §1.4'te işaretli.)*
- **Hasar tipleri (5):** `Melee · Pierce · Blunt · Fire · Poison`. *(Blueprint §4 illüstratif matris "Slash/Pierce/Blunt/Magic/Fire" der; `Melee≈Slash`, `Poison≈Magic` provisional eşlemeleri ADR ile sonlanır.)*
- **Pozisyonel çarpan (`Terrain.cs` / `PositionalSystem`):** **front 1.0 · flank 1.5 · back 2.0** (back-konisi `cos ≤ −0.5`, yani ~120° arka). Bu §4 kanon sabitidir.
- **Hasar zinciri (`Combat.cs`):** `final = round(base × (1+Lvl×perLvl)) × typeArmor × positional × terrain × difficulty × cover × raged`.

> **🔑 TASARIM FIRSATI (raporun tezi):** Mevcut 12 MVP biriminin **HİÇBİRİ `Unarmored` zırhını kullanmaz**; **`Blunt`** yalnızca Ashen Slinger'da, **`Fire`** yalnızca Iron Battlemage'da, **`Poison`** yalnızca Ashen Hexcaster'da kullanılır. Yani matrisin **`Unarmored` sütunu tamamen boş**, **`Blunt`/`Fire` satırları neredeyse boş.** İki yeni fraksiyon, **var-olan-ama-kullanılmayan hücreleri doldurarak** sıfır yeni kategori ile derin asimetri yaratabilir → bu, "hiçbir birim matrisi kırmaz" ispatının (Part 10) anahtarıdır.

### 1.3.f Mevcut diğer kanon kelime dağarcığı (yeniden-kullanım tabanı)

| Sistem | Mevcut atomlar | Kaynak |
|---|---|---|
| **StatusKind (8)** | None · Chilled · Burning · Poisoned · Stunned · Hasted · Raged · GoldBoost | `CombatTypes.cs` |
| **TerrainKind** | HighGround · Choke · Cover · Hazard | §4/§11 |
| **FormationType (3)** | Line · Tight · Loose | `Phase2Components.cs` |
| **SpellCategory (5)** | Offensive · Control · Economy · Summon · Buff | §5.3, `CombatTypes.cs` |
| **Komutan** | side başına **TEK** `CommanderRuntime`; 1 active + 1 passive; **güç bütçesi ≤%15** (`k_PowerBudgetCeiling=0.15`, hard-clamp); ADR-2-002 stacking (komutan buff'ları ≤bütçeye clamp; spell buff'ları ayrı) | §6, ADR-2-002 |
| **Mevcut 2 komutan** | Iron Warden (Rally cd45/mag0.12/r5/dur6 + Quartermaster 0.08; budget 0.12) · Ashen Warchief (WarCry cd40/mag0.15/r5/dur5 + Bloodthirst 0.08; budget 0.13) | `Data/Commanders/*` |
| **Ekonomi** | maden→eğit→ittir→heykel; miner-cap'li sabit Gold node'lar; statue shield-phase + damage states; 4 para birimi | §3, §11 |

## 1.4 Çelişki kontrolü (contradiction check — ön-tarama)

Bu raporun tüm tavsiyeleri aşağıdaki kurallara **uyacak** şekilde tasarlandı (tam denetim Part 10/11'de). **Dürüstlük ilkesi:** kanonu aşan her talep gizlenmez, **işaretlenir.**

| Kontrol | Bu raporun duruşu |
|---|---|
| Roadmap ihlali? | **Hayır** — Phase 7.3/full-vision'ı ön-tasarlar; faz kapılarını/öncelikleri değiştirmez. |
| ADR ihlali? | **Hayır** — komutan tasarımları ADR-2-002 stacking + ≤0.15 clamp'e uyar. |
| Decision Log ihlali? | **Hayır** — 3./4. fraksiyonu §2 tetikleyicisine (*"two-faction balance stable in telemetry"*) bağlı tutar; öne çekmez. |
| Asimetri / strictly-dominant? | **Hayır** — her fraksiyonun net counter'ı var (Part 6/9); "kesin baskın yok" §5.1 korunur. |
| **Yeni zırh/hasar sınıfı?** | **Hayır (sıfır)** — yalnızca mevcut 4 zırh + 5 hasar kullanılır; boş `Unarmored`/`Blunt`/`Fire` hücreleri doldurulur (Part 10). |
| **"Mechanized = siege/structures" → çekirdek-döngü çatallar mı?** | **İŞARETLENDİ (en yüksek risk).** Kanon §7/§15.7: tek savaş çekirdeği, bespoke mekanik yok. Mechanized **base-builder DEĞİLDİR**; "structures" kimliği **Heavy/siege rolü + (opsiyonel, ADR-gated) deployable-emplacement** ile ifade edilir. Base-building **REDDEDİLİR.** |
| **Görev "rol paleti" ↔ kanon paleti** | **İŞARETLENDİ (§1.3.d).** Specialist→Caster, Siege/Elite→Heavy, Commander Support Logic→Komutan sistemi (Part 12). Kanon LAW. |
| Yeni sim-hook / mekanik? | **İŞARETLENDİ.** Bazı birim/komutan yetenekleri mevcut enum/hook'ları aşar (deploy, blink, target-mark, heal-aura, apply-status-on-hit). Her biri ⚙️ **[YENİ SİM-HOOK]** etiketli ve **gelecek ADR gerektirir**; mevcut atomlarla karşılananlar ♻️ **[MEVCUT MEKANİK]** etiketli. Hiçbiri bugün kanona girmez. |
| Sayısal değerler | Tüm HP/hasar/cd/maliyet değerleri **PROVISIONAL/LSD-owned** (§15.6); kanon olarak iddia edilmez. |
| Birim sayısı | **Uyumlu** — fraksiyon başına **tam 6 birim** (§5.2); full-vision ≤9 hard-cap'i aşılmaz. |

> **Önemli dürüstlük notu:** Tasarlanan iki fraksiyonun kimliğinin *tam* ifadesi (Arcane'in "apply-control-on-hit" caster'ı, Mechanized'in "deployable structure"ı) mevcut `UnitDef`/sim-hook'larının **ötesine** geçer. Bunlar **bugünün kanonu değildir**; Phase 7.3'te birer ADR ile değerlendirilmesi gereken **gelecek tasarım önerileridir.** Bu rapor, fraksiyonların **kanon-güvenli çekirdeğini** (mevcut zırh/hasar/StatusKind/formasyon ile) tasarlar ve derin-fantezi kancalarını **ayrıca ⚙️ işaretler** — "no implementation / no canon change" kuralına sadakatin garantisi budur.

---

# PART 2 — Fraksiyon Tasarım Felsefesi

## 2.1 Iron Pact neden çalışıyor?

| Boyut | Analiz |
|---|---|
| **Net fantezi** | "Kırılmaz duvar." Tek cümlede okunur; her birim bu fanteziyi taşır (Shieldman, Ironclad). |
| **Mekanik kimlik** | **Shielded + Heavy zırh** yoğunluğu + formasyon + sustain. Matriste: Pierce'e (Crossbow) ve frontal-denial'a yaslanır. |
| **Net zaafiyet = adil counter** | Yavaş + Flanker'sız + Magic'e zayıf → rakip **flank (×1.5/×2.0)** ve **tempo** ile cevaplar. Zaafiyet *okunur* ve *sömürülebilir.* |
| **Oynanış kancası** | "Hattı tut, dövüşü uzat, kazanan zırh olsun." Sabırlı/pozisyonel oyuncu ödüllenir. |

**Neden işe yarıyor:** güç fantezisi (dayanıklılık) ile yapısal zaafiyet (yavaşlık/flank) **simetrik** — güçlü olduğu yer (frontal) ile zayıf olduğu yer (yan/arka) **aynı geometrik eksende.** Bu, "okunur derinlik" (P2) için altın standarttır.

## 2.2 Ashen Horde neden çalışıyor?

| Boyut | Analiz |
|---|---|
| **Net fantezi** | "Harcanabilir sel." Hız + kütle + flank-burst. |
| **Mekanik kimlik** | **Light zırh** yoğunluğu + en yüksek `moveSpeed` + tek **Flanker** (Houndmaster). Matriste: hedefin **arkasına** ulaşıp ×2.0 alır. |
| **Net zaafiyet = adil counter** | Kırılgan + AoE'ye zayıf + stall'da ekonomi çöker → rakip **AoE** ve **sürekli savunma** ile cevaplar. |
| **Oynanış kancası** | "Erken bas, geç kalma." Agresif/tempo oyuncu ödüllenir. |

**Neden işe yarıyor:** Iron Pact'in **tam zıttı** olarak tasarlanmış — yavaş/dayanıklı ↔ hızlı/kırılgan. İki fraksiyon bir **eksen** (tempo↔attrition) oluşturur; her maç bu eksende bir çekişmedir. **Asimetri burada "ayna-zıtlık" deseniyle** sağlanır.

## 2.3 İki fraksiyonun ortak başarı formülü

1. **Tek-cümle fantezi** (duvar / sel).
2. **Tek baskın mekanik eksen** (zırh-yoğunluğu / hız-yoğunluğu).
3. **Geometrik-okunur zaafiyet** (flank/AoE).
4. **Çekirdek döngüyü çatallamaz** — ikisi de aynı maden→eğit→ittir→heykel'i oynar; fark *nasıl* oynadığında.
5. **Counter karşılıklı** — A'nın gücü B'nin zaafiyetini, B'nin gücü A'nın zaafiyetini hedefler.

## 2.4 Keşfedilmemiş tasarım alanları (yeni fraksiyonların yuvası)

Mevcut iki fraksiyon **tempo↔attrition** eksenini doldurur. **Boş kalan eksenler** (yeni fraksiyonların meşru yuvası):

| Boş eksen | Açıklama | Aday fraksiyon |
|---|---|---|
| **Kontrol/AoE ↔ tekil-hedef** | İki MVP fraksiyonu da *kinetik tekil-hedef* ağırlıklı (Caster'lar var ama merkez değil). "Savaş alanını **kontrol** eden / **alan-reddi** kuran" bir fraksiyon yok. | **Arcane Order** (control + AoE merkezli, kırılgan) |
| **Kuşatma/menzil-üstünlüğü ↔ yakın-dövüş** | Heavy/siege rolü her iki fraksiyonda *var* ama merkez değil; "uzaktan **yık** ve **menzille hükmet**" doktrini boş. | **Mechanized Dominion** (siege + kinetik + dayanıklı, AoE'siz) |
| **Magic ↔ Machine (atomik zıtlık)** | En güçlü tematik zıtlık. | Arcane (**tüm büyü, kas yok**) ↔ Mechanized (**tüm makine, büyü yok**) |

> **Raporun tezi:** Arcane Order ve Mechanized **birbirinin ayna-zıttı** olarak tasarlanmalı — tıpkı Iron↔Ashen gibi. **Arcane = `Heavy` rolü YOK** (kas yerine büyü; en zayıf sieger). **Mechanized = `Caster` rolü YOK** (büyü yerine makine; AoE/kontrol yok). Bu, full-vision'a **ikinci bir denge-ekseni** (control/AoE ↔ siege/kinetic) ekler ve mevcut tempo↔attrition eksenine **dik** durur → 4-fraksiyonlu derinlik.

---

# PART 3 — Gelecek Fraksiyon Genişleme Çerçevesi (Metodoloji)

Bu, BULWARK'a fraksiyon eklemek için **resmî metodolojidir** (tavsiye; kanon değil). Amaç: **reskin'den, power-creep'ten ve jenerik ırklardan kaçınmak.**

## 3.1 Beş tasarım ilkesi

1. **Tek-cümle doktrini** — fraksiyon bir cümlede özetlenmiyorsa, kesilir.
2. **Tek baskın eksen** — fraksiyon *bir* mekanik fikre yaslanır (kontrol / siege); iki fikir = bulanık kimlik.
3. **Geometrik-okunur zaafiyet** — gücün karşılığı, rakibin **görüp sömürebileceği** bir zaafiyet olmalı (flank, AoE, tempo, menzil).
4. **Çekirdek-döngü dokunulmazlığı** — yeni fraksiyon **maden→eğit→ittir→heykel**'i oynar; bespoke ekonomi/base-building **yasak** (§7, §15.7).
5. **Sıfır-yeni-kategori önceliği** — yeni asimetri **önce mevcut matris hücrelerinden** (boş `Unarmored`/`Blunt`/`Fire`) devşirilir; yeni zırh/hasar/StatusKind **son çare** ve **ADR-gated.**

## 3.2 Asimetri kuralı — "Benzersiz-Çıkarım Izgarası" (Unique-Omission Grid)

Kanon zaten asimetriyi "hangi rolü çıkarır" ile tanımlar (§5.2). Bu raporun önerdiği **çerçeve:** full-vision'da **her fraksiyon FARKLI bir rol çıkarsın.** 7-rol paletinde Miner + 5 rol sahalanır; 4 fraksiyon, 4 farklı rolü çıkarırsa **her rol tam 3 fraksiyonda bulunur, tam 1'inde yoktur** → dengeli full-vision rol dağılımı + her fraksiyona keskin, okunur bir zaafiyet.

| Fraksiyon | **Çıkarılan rol** | Sahalanan (Miner +) | Kimlik | Yapısal zaafiyet |
|---|---|---|---|---|
| **Iron Pact** | **Flanker** *(kanon)* | Frontline, Skirmisher, Ranged, Caster, Heavy | zırh-duvarı / attrition | mobilite yok → flank'lanır |
| **Ashen Horde** | **Frontline** *(kanon)* | Skirmisher, Ranged, Caster, Heavy, Flanker | swarm / flank-tempo | tank yok → attrition'da çöker |
| **Arcane Order** | **Heavy** *(öneri)* | Frontline, Skirmisher, Ranged, Caster, Flanker | control + magic, kırılgan | brute/siege yok → **en yavaş sieger** |
| **Mechanized** | **Caster** *(öneri)* | Frontline, Skirmisher, Ranged, Heavy, Flanker | siege + kinetik, dayanıklı | büyü/AoE yok → **swarm'a zayıf** |

> **⚠️ Kanon sınırı:** Iron=çıkar-Flanker ve Ashen=çıkar-Frontline **kanondur** (§5.2). Arcane=çıkar-Heavy ve Mechanized=çıkar-Caster bu raporun **tavsiyesidir** (LSD/GD bir ADR ile değiştirebilir). Bu çerçeve **icat değil, kanonik desenin (her fraksiyon bir rol çıkarır) full-vision'a tutarlı uzantısıdır.**

## 3.3 Denge limitleri (balance limits)

| Limit | Kural | Gerekçe |
|---|---|---|
| **Matris bütünlüğü** | Yeni birim **mevcut 5×4 hücrede** yaşar; yeni satır/sütun = ADR. | Power-creep'i ve kombinatoryal-patlamayı engeller (§16 risk #5). |
| **Sayı paritesi** | Yeni fraksiyonun toplam stat-bütçesi (HP×DPS×menzil×hız) **mevcut fraksiyonların bandında** kalır. | "No strictly dominant" (§5.1). |
| **Birim-cap** | Lansman = 6 birim; full-vision ≤9 (§5.2). | İçerik-maliyeti + denge-izlenebilirliği. |
| **Counter-zorunluluğu** | Her yeni birimin **en az bir net counter'ı** olmalı (Part 6/9). | §5: "no unit without a counter." |
| **Komutan bütçesi** | ≤%15 maç-etkisi (`k_PowerBudgetCeiling`), ADR-2-002 clamp. | §6 İHLAL EDİLEMEZ. |

## 3.4 Okunabilirlik gereksinimleri (readability requirements)

1. **Silüet ayrımı** — her yeni birim, **3 metre kuraldan** ayırt edilebilir bir silüete sahip olmalı (§6 base-locked silhouette; §10 sign-off).
2. **Renk-kimliği** — fraksiyon paleti renk-körü-güvenli (§11). Arcane = mor/altın leyline; Mechanized = pirinç/gunmetal/amber.
3. **Hasar-tipi VFX kodu** — mevcut kod korunur (Pierce beyaz, Blunt toz, Fire turuncu, Poison/Magic mor); yeni fraksiyon **yeni VFX-dili icat etmez**, mevcut koda uyar (§10).
4. **Telegraph** — her güçlü etki (komutan active, siege-atışı) **telegraph + counter** taşır (§5.3, §6).

## 3.5 Power-creep'siz benzersizlik — ispat deseni

**Soru:** Yeni fraksiyon, *eskisini gölgede bırakmadan* nasıl benzersiz olur?
**Cevap (3 kaldıraç, hepsi power-nötr):**
1. **Hücre-yeniden-dağıtımı** — Arcane `Unarmored` (yeni kırılganlık) + `Fire`; Mechanized `Blunt` (anti-zırh/anti-yapı) + `Heavy`. **Aynı matris, farklı işgal** → yeni hisler, sıfır yeni güç.
2. **Rol-çıkarımı** — eksik rol (Heavy / Caster) = yapısal *zaafiyet*, güç değil. Benzersizlik **eksiklikten** gelir.
3. **Geometri/tempo profili** — Arcane = yüksek-mobilite-kırılgan; Mechanized = düşük-mobilite-dayanıklı. `moveSpeed`/`attackInterval` dağılımı kimliği taşır, **toplam güç sabit.**

> **İlke:** Yeni fraksiyon oyuncuya "**daha güçlü**" değil "**farklı bir problem**" sunar. Güç sabit; *çözülen denklem* değişir.

---

# PART 4 — Arcane Order Doktrini

## 4.1 Çekirdek kimlik

| Boyut | Tanım |
|---|---|
| **Tek-cümle fantezi** | *"Kas yok, irade var: savaş alanını büyüyle **kontrol et**, kırılgan bedenleri zekayla koru, düşman daha hattını kuramadan onu **dağıt**."* |
| **Görsel kimlik** | Mor/altın leyline enerjisi, yüzen kristaller, cübbeler, rün-dövmeleri, hafif/uçuşan silüetler. Heykel = **kristalin Leyline-Dikilitaşı** (shield-phase = enerji-kalkanı; damage-states = çatlayan kristal). Palet renk-körü-güvenli mor+altın. |
| **Savaş-alanı kimliği** | **Kontrol + AoE + mobilite**; en kırılgan bedenler (`Unarmored`/`Light`), en yüksek "alan-etkisi"; **en yavaş sieger** (Heavy yok). |
| **Baskın eksen** | **Control/AoE** (yeni denge-ekseni — Part 2.4). |
| **Çıkardığı rol** | **Heavy** (brute/siege yok). |
| **Ekonomi stili** | Çekirdek döngü **birebir** (maden→eğit→ittir→heykel). Flavor: "mana-Gold" leyline node'larından (mekanik **aynı**; §15 yeni-ekonomi yasak). Birimler **pahalı + düşük-HP** (glass-cannon ekonomisi) → erken baskıya zayıf, geç-oyunda büyü-üstünlüğü. *(Tüm değerler PROVISIONAL.)* |
| **Komutan felsefesi** | Caster-çekirdeğini **amplifiye eden** zamanlama kaldıracı (Part 12); ≤%15 bütçe. |
| **Büyü felsefesi** | Draft-3 havuzuyla **en sinerjik** fraksiyon: kendi Fire/kontrolü + Control/Offensive büyüleri → Freeze→Shatter zincirleri (Part 13, ref `future/002`). |
| **Formasyon felsefesi** | **Loose** tercih eder (kırılgan casterları AoE'den korur) + kendi AoE'siyle düşman **Tight**'ını cezalandırır (Part 11). |

## 4.2 Güç & zaafiyet profili

| Güç | Zaafiyet |
|---|---|
| En iyi **AoE/clump-cezası** (Archmage Fire) → swarm'a ve Tight'a cevap | **En kırılgan backline** (`Unarmored` casterlar; Melee ×1.3, Fire ×1.4 yer) |
| Yüksek **mobilite** (Phase-Stalker flanker) → düşman backline'ını avlar | **En yavaş sieger** (Heavy yok → heykeli kapatması zor; tempo'da kazanmalı) |
| **Pierce** (Spellslinger) → Shielded duvarları deler (×1.5) | **Heavy zırha zayıf** (Melee 0.9 / Pierce 0.8 vs Heavy → Ironclad/Razorbeast'i tek-hedefte zor kırar) |
| Büyü-sinerjisinde tavan | **Frontal-denial zayıf** (Ward-Weaver düşük-HP; gerçek tank değil) |

## 4.3 Arcane nasıl farklılaşır?

- **vs Iron Pact:** Iron = dayanıklı/yavaş **kinetik duvar**; Arcane = kırılgan/hızlı **büyü-kontrolü.** Iron frontal kazanır; Arcane onu **AoE + flank + kontrol** ile çözer, ama Iron'un Heavy'lerine (Ironclad) ve sustain'ine karşı **zaman baskısı** altındadır.
- **vs Ashen Horde:** İkisi de kırılgan/hızlı — ama Ashen = **kütle/tekil-hedef tempo**, Arcane = **AoE/kontrol.** Arcane'in AoE'si Ashen swarm'ının doğal counter'ı; Ashen'in hızı Arcane'in `Unarmored` backline'ının doğal counter'ı. **Bıçak-sırtı yarış.**
- **vs Mechanized (ayna-zıt):** Arcane = tüm-büyü/kas-yok; Mechanized = tüm-makine/büyü-yok. Arcane Mechanized'in yavaş makinelerini **kite + flank + kontrol** eder; Mechanized'in `Heavy` zırhı Arcane'in tek-hedef hasarını yer (Pierce 0.8), `Blunt`'ı Arcane'in `Unarmored`/`Shielded` bedenlerini ezer.

---

# PART 5 — Arcane Order Birim Rosteri (6 birim)

*(Palet: Miner · Frontline · Skirmisher · Ranged · Caster · Flanker — **Heavy YOK.** Tüm stat'lar PROVISIONAL/LSD-owned; yalnızca **rol/zırh/hasar** matris-kanonudur. ♻️ = mevcut mekanik; ⚙️ = yeni sim-hook, ADR-gated.)*

### 5.1 Leyline Çırağı (Leyline Acolyte) — **Miner**
- **Rol:** Miner (ekonomi). **Zırh:** `Light`. **Hasar:** `Melee`. ♻️
- **Lore:** Leyline yarıklarından ham mana-Gold süzen acemi büyücü.
- **Savaş amacı:** Ekonomi node'larını madener; zayıf savaşçı. Iron/Ashen miner'ının **birebir** rol-eşi (hız ↑, HP ↓ — glass ekonomi).
- **Güç:** ucuz, hızlı ekonomi. **Zaaf:** savunmasız (her miner gibi); flank'ta anında ölür.

### 5.2 Ward-Weaver (Bariyer Örücüsü) — **Frontline**
- **Rol:** Frontline (hat-tutar). **Zırh:** `Shielded`. **Hasar:** `Melee` (düşük). ♻️ *(çekirdek)* / ⚙️ *(ward-aura opsiyonel)*
- **Lore:** Kasla değil, kinetik bariyerle hattı tutan örücü.
- **Savaş amacı:** Fraksiyonun **tek dayanıklı bedeni** — ama **gerçek tank değil:** `Shielded` ama **düşük-HP** (Shieldman'in yarısı). Frontal-denial sağlar, casterlara zaman kazandırır. ⚙️ *Opsiyonel ward-aura:* önündeki müttefiklere küçük frontal hasar-azaltımı (Cover-benzeri) — **[YENİ SİM-HOOK] / ADR** (mevcut "aura" hook'u yalnızca komutanda var).
- **Güç:** frontal `Pierce`/`Ranged`'i yavaşlatır; caster-kalkanı. **Zaaf:** `Pierce` ×1.5 + `Blunt` ×1.4 vs Shielded → Crossbow/Autocannon/Slinger onu deler; düşük-HP → flank'ta (×1.5/×2.0) hızla düşer; Magic/Poison.

### 5.3 Rün-Bıçağı (Runeblade) — **Skirmisher**
- **Rol:** Skirmisher (melee DPS). **Zırh:** `Light`. **Hasar:** `Melee`. ♻️
- **Lore:** Kesici rünler kanalize eden düellocu.
- **Savaş amacı:** Casterları düşman Skirmisher/Flanker'larından korur; `Light`/`Unarmored` hedeflere hızlı DPS (Melee ×1.25/×1.3).
- **Güç:** hızlı melee; backline-savunması. **Zaaf:** `Light` → kırılgan; `Heavy`-zırhlı Skirmisher'lara (Iron Legionary) kaybeder (Melee 0.9 vs Heavy); kütleye ezilir.

### 5.4 Büyü-Oklusu (Spellslinger) — **Ranged**
- **Rol:** Ranged (menzilli DPS). **Zırh:** `Unarmored`. **Hasar:** `Pierce` (arcane cıvata). ♻️ *(matris-boş hücre dolumu)*
- **Lore:** Saf kuvvetten delici cıvatalar yoğuran nişancı.
- **Savaş amacı:** **Anti-Shielded/anti-duvar** menzil (Pierce ×1.5 vs Shielded) → Iron Shieldman, Mech Bulwark-Walker, kendi Ward'ını bile deler. **`Unarmored` zırhı = matrisin boş sütununu doldurur.**
- **Güç:** Shielded'ı paramparça eder; iyi menzil. **Zaaf:** **`Unarmored` = oyunun en kırılganı** (Melee ×1.3, Fire ×1.4 yer) → kendisine ulaşan her şeye ölür; flanker'lara (Houndmaster, Outrider) hard-counter'lanır; kendini koruyamaz.

### 5.5 Baş-Büyücü (Archmage) — **Caster** ⭐ *(fraksiyon merkezi)*
- **Rol:** Caster (AoE/utility). **Zırh:** `Unarmored`. **Hasar:** `Fire`. ♻️ *(Battlemage modeli)* / ⚙️ *(apply-control-on-hit opsiyonel)*
- **Lore:** Leyline ateşini büken ustanın kendisi; fraksiyonun kalbi.
- **Savaş amacı:** **En iyi AoE** (clump/Tight-cezası, Fire ×1.3 vs Light) + fraksiyonun **tek siege yolu** (Fire `1.0 + burn` vs Structure → Heavy'siz heykel-baskısı, **yavaş**). ⚙️ *Derin-fantezi:* saldırıyla Chilled/Stunned uygulama mevcut `UnitDef`'te **yok** (status yalnızca **büyü** katmanından gelir) → **[YENİ SİM-HOOK] / ADR** (`UnitDef.appliesStatus`). Kanon-güvenli çekirdek: Battlemage-benzeri Fire-AoE (♻️); kontrol kimliği **büyü-draftından** gelir (Part 13).
- **Güç:** en iyi clump-cezası; Heavy'siz fraksiyonun siege-can-simidi. **Zaaf:** `Unarmored` + yavaş + pahalı; **en kötü sieger** (Fire 1.0 vs yapı) → Arcane *kontrolle* kazanmalı, *kuşatmayla* değil.

### 5.6 Faz-Avcısı (Phase-Stalker) — **Flanker**
- **Rol:** Flanker (hızlı mobilite). **Zırh:** `Light`. **Hasar:** `Melee`. ♻️ *(yüksek moveSpeed)* / ⚙️ *(blink opsiyonel)*
- **Lore:** Uzayda kısa-adımlar atan büyücü-avcı.
- **Savaş amacı:** Düşman backline'ını (caster/ranged) avlar — Houndmaster'ın arcane-eşi. Flank ×1.5 / back ×2.0 ile `Unarmored`/`Light` hedefleri biçer.
- **Güç:** en yüksek mobilite (PROVISIONAL `moveSpeed≈3.2`); backline-katili. **Zaaf:** `Light` + düşük-HP → frontal dövüşte ölür; siege-değeri sıfır. ⚙️ *Blink/teleport* mevcut sim'de yok → **[YENİ SİM-HOOK]/ADR**; kanon-güvenli çekirdek = sadece yüksek `moveSpeed` (♻️).

### Arcane roster özeti

| Birim | Rol | Zırh | Hasar | Doldurduğu matris-hücresi | Yeni hook? |
|---|---|---|---|---|---|
| Leyline Çırağı | Miner | Light | Melee | (standart) | ♻️ |
| Ward-Weaver | Frontline | Shielded | Melee | (standart) | ♻️ / ⚙️ ward-aura |
| Rün-Bıçağı | Skirmisher | Light | Melee | (standart) | ♻️ |
| Büyü-Oklusu | Ranged | **Unarmored** | Pierce | **Pierce×Unarmored (boş)** | ♻️ |
| Baş-Büyücü | Caster | **Unarmored** | Fire | **Fire×Unarmored (boş)** | ♻️ / ⚙️ control-on-hit |
| Faz-Avcısı | Flanker | Light | Melee | (standart) | ♻️ / ⚙️ blink |

---

# PART 6 — Arcane Order Counterplay Analizi

*(Tüm örnekler implementasyon matrisi (§1.3.e) + pozisyonel ×1.5/×2.0 üzerinden.)*

## 6.1 Arcane → Iron Pact zırhını nasıl ele alır?

- **Iron Shieldman (Shielded):** **Büyü-Oklusu `Pierce` ×1.5** → doğal counter; Arcane duvarı deler. ✅
- **Iron Ironclad/Legionary (Heavy):** Arcane'in zayıf noktası — `Melee 0.9`, `Pierce 0.8` vs Heavy. **Çözüm:** Faz-Avcısı ile **arkadan ×2.0** (0.8×2.0 = 1.6 efektif) **veya** Baş-Büyücü AoE + büyü-kontrolü (Freeze→Shatter). Tek-hedef frontal'da Arcane **kaybeder** → flank/kontrol *zorunlu* (okunur asimetri).
- **Iron Crossbow/Battlemage (Light):** Baş-Büyücü `Fire` ×1.3 + Faz-Avcısı `Melee` ×1.3 → backline'ı yakar/biçer. ✅

## 6.2 Arcane → Iron formasyonlarını nasıl ele alır?

- **Iron Line (frontal blok):** frontal'dan Arcane zayıf → **Faz-Avcısı yan/arka** (×1.5/×2.0) + Baş-Büyücü Line'ın *kenarına* AoE. Line'ın zaafı = yanları.
- **Iron Tight (yoğun):** **Baş-Büyücü AoE'nin rüyası** — Tight = kümelenmiş hedefler → tek AoE çok-vuruş. Arcane, Tight'ı *cezalandırmak* için tasarlanmış.

## 6.3 Arcane → Ashen swarm'ını ve mobilitesini nasıl ele alır?

- **Ashen swarm (kümelenmiş Light):** **Baş-Büyücü `Fire` ×1.3 + AoE** = swarm'ın doğal counter'ı. ✅ Arcane'in *birincil* anti-swarm aracı.
- **Ashen mobilite/flank (Houndmaster):** **Arcane'in zaafı** — Houndmaster, `Unarmored` Büyü-Oklusu/Baş-Büyücü'yü arkadan (×2.0) avlar. **Çözüm:** Rün-Bıçağı + Ward-Weaver ile **backline-ekranı**; Faz-Avcısı ile karşı-flank. **Yarış:** Arcane swarm'ı AoE'lerken, Ashen casterları flank'lar → kim önce. (Net, okunur, simetrik gerilim.)

## 6.4 Counter-özet

| Arcane şuna **güçlü** | Arcane şuna **zayıf** |
|---|---|
| Shielded duvarlar (Pierce ×1.5) | Heavy zırh (Melee 0.9 / Pierce 0.8) |
| Kümelenmiş/Tight ordular (AoE) | Hızlı flanker'ların backline-baskını |
| Light backline (Fire ×1.3) | Erken tempo (pahalı/kırılgan ekonomi) |
| Büyü-sinerjisi | Heykel-kapatma (Heavy yok → yavaş siege) |

---

# PART 7 — Mechanized Dominion Doktrini

## 7.1 Çekirdek kimlik

| Boyut | Tanım |
|---|---|
| **Tek-cümle fantezi** | *"Büyü yok, demir var: ağır makinelerle hattı **dayanıklılıkla** taşı, menzille **hükmet** ve düşman heykelini **kuşatmayla yık.**"* |
| **Görsel kimlik** | Pirinç/gunmetal/amber, duman, dişliler, perçinler, ağır/köşeli silüetler. Heykel = **Demir Döküm-Çekirdeği (Foundry-Core) / kale** (shield-phase = zırh-plaka; damage-states = sökülen panel). Palet renk-körü-güvenli amber+çelik. |
| **Savaş-alanı kimliği** | **Siege + kinetik + dayanıklılık**; en yüksek HP/zırh; en iyi heykel-baskısı; **AoE/kontrol YOK** (Caster yok). |
| **Baskın eksen** | **Siege/menzil-üstünlüğü** (yeni denge-ekseninin diğer ucu — Part 2.4). |
| **Çıkardığı rol** | **Caster** (büyü/AoE yok). |
| **Ekonomi stili** | Çekirdek döngü **birebir.** Flavor: "cevher-Gold" döküm-node'larından. Birimler **pahalı + yavaş + yüksek-HP** (kurulum-yoğun ekonomi) → erken zayıf, geç-oyunda dayanıklılık-üstünlüğü. *(PROVISIONAL.)* |
| **Komutan felsefesi** | Yavaş kuşatma-itişini **sürdüren/odaklayan** kaldıraç (Part 12); ≤%15 bütçe. |
| **Siege felsefesi** | Heykel/Shielded/Heavy'yi `Blunt` ile **yıkar** (matrisin en iyi anti-zırh/anti-yapı hattı). |
| **Formasyon felsefesi** | **Line** tercih eder (frontal artillery-duvarı) veya **Tight** (choke-kırma); yavaş birimler formasyon-disiplinini doğal kılar (Part 11). |

## 7.2 Güç & zaafiyet profili

| Güç | Zaafiyet |
|---|---|
| En iyi **siege/heykel-baskısı** (Siege-Engine `Blunt`) | **AoE/kontrol YOK** (Caster yok) → swarm'ı verimli temizleyemez |
| En yüksek **dayanıklılık** (`Heavy`/`Shielded` yoğunluğu) | **En yavaş** → kite'lanır, flank'lanır (×1.5/×2.0) |
| **Anti-zırh `Blunt`** (Shielded ×1.4, Heavy ×1.2) + **anti-Shielded `Pierce`** (×1.5) | **Kurulum-süresi** gerektirir → erken-baskıya açık |
| Menzil-üstünlüğü | Tek mobilite = tek Outrider (öngörülebilir) |

## 7.3 Mechanized nasıl farklılaşır?

- **vs Iron Pact:** İkisi de yavaş/dayanıklı — ama Iron = **defansif attrition-duvarı** (sustain, formasyon, frontal-denial); Mechanized = **ofansif siege-yıkıcı** (Blunt/Pierce ile zırh-deler, menzille bunaltır). Iron uzun-frontal-dövüşü kazanır; Mechanized **kuşatma + menzil** ile *bitirir.* Mechanized `Blunt` (1.2 vs Heavy, 1.4 vs Shielded) Iron'u Iron'un Mechanized'i kırdığından **daha iyi** kırar → ama Iron daha hızlı/flank'lar.
- **vs Ashen Horde:** **Mechanized'in kâbusu** — Caster yok = swarm'ı AoE'leyemez; yavaş birimler Houndmaster/Raider tarafından kite/flank'lanır. `Heavy` zırhı swarm'ı *yer* (Pierce 0.8 / Melee 0.9 vs Heavy) ama tek-hedef → **temizleyemez.** Outrider + Autocannon ile kite etmeli. (Swarm-fraksiyonu, siege-fraksiyonunun AoE-yokluğunu cezalandırır → mükemmel asimetri.)
- **vs Arcane (ayna-zıt):** Mechanized'in `Heavy` birimleri Arcane'in tek-hedef hasarını yer (Pierce 0.8, Fire 1.0 vs Heavy); `Blunt`/`Pierce` Arcane'in `Unarmored`/`Shielded` bedenlerini ezer. Ama Arcane Mechanized'i **kite + kontrol + flank** eder ve AoE'siz Mechanized Arcane'in kümelenmiş casterlarını cezalandıramaz. **Kapat-ve-yık (Mech) ↔ kite-ve-kontrol (Arcane).**

---

# PART 8 — Mechanized Dominion Birim Rosteri (6 birim)

*(Palet: Miner · Frontline · Skirmisher · Ranged · Heavy · Flanker — **Caster YOK.** Stat'lar PROVISIONAL/LSD-owned.)*

### 8.1 Dökümhane-Hizmetkârı (Foundry-Servitor) — **Miner**
- **Rol:** Miner. **Zırh:** `Light`. **Hasar:** `Melee`. ♻️
- **Lore:** Cevher toplayan salvage-droidi.
- **Savaş amacı:** Ekonomi; rol-eşi. Diğer miner'lardan biraz **dayanıklı + yavaş** (mekanik kimlik).
- **Güç:** dayanıklı ekonomi. **Zaaf:** yavaş; savaşçı değil.

### 8.2 Sur-Yürüyücü (Bulwark-Walker) — **Frontline**
- **Rol:** Frontline. **Zırh:** `Shielded`. **Hasar:** `Melee`. ♻️
- **Lore:** İki ayaklı, plaka-kalkanlı zırhlı yürüteç.
- **Savaş amacı:** Oyunun **en yüksek HP'li** frontline'ı; arkasında topçunun ateş ettiği yürüyen duvar. Line-formasyonunu çapalar.
- **Güç:** muazzam HP + Shielded frontal-denial. **Zaaf:** `Pierce` ×1.5 / `Blunt` ×1.4 vs Shielded (kendi fraksiyonunun Blunt'ı bile ayna-counter); çok yavaş → flank'lanır; Magic/Poison.

### 8.3 Siper-Kırıcı (Trench-Breaker) — **Skirmisher**
- **Rol:** Skirmisher (melee DPS). **Zırh:** `Heavy`. **Hasar:** `Blunt` (pnömatik balyoz). ♻️ *(Blunt satırı dolumu)*
- **Lore:** Pnömatik çekiçli yakın-dövüş servitörü.
- **Savaş amacı:** **Dayanıklı melee** — `Heavy` zırh ile menzilli ateşi yer (Pierce 0.8); `Blunt` ile Shielded/Heavy ezer (1.4/1.2).
- **Güç:** Pierce'e dayanıklı (Heavy 0.8); anti-zırh melee. **Zaaf:** yavaş → kite'lanır; `Blunt` ayna-zaafı (1.2 vs Heavy); flank ×2.0.

### 8.4 Oto-Top (Autocannon) — **Ranged**
- **Rol:** Ranged (menzilli DPS). **Zırh:** `Light`. **Hasar:** `Pierce`. ♻️
- **Lore:** Paletli silah-platformu.
- **Savaş amacı:** Uzun-menzil `Pierce` (×1.5 vs Shielded) → Iron Shieldman, Arcane Ward, düşman walker'larını deler. Walker-duvarı arkasında korunan kırılgan topçu.
- **Güç:** Shielded'ı parçalar; iyi menzil. **Zaaf:** `Light` (kendisi kırılgan; Melee 1.25 / Fire 1.3); flanker'lara ölür → walker-ekranına bağımlı.

### 8.5 Kuşatma-Motoru (Siege-Engine) — **Heavy** ⭐ *(fraksiyon merkezi)*
- **Rol:** Heavy (siege/anti-yapı). **Zırh:** `Heavy`. **Hasar:** `Blunt`. ♻️ *(çekirdek)* / ⚙️ *(deploy opsiyonel)*
- **Lore:** Devasa havan-yürüteç; fraksiyonun kazanma-koşulu.
- **Savaş amacı:** **Anti-Structure/anti-heykel siege** (`Blunt` matrisin en iyi anti-yapı hattı; Blueprint §4: Blunt 1.5 vs Structure) + anti-Shielded (1.4) + anti-Heavy (1.2). Mechanized'in **win-condition'ı.** ⚙️ *"Structures" fantezisi:* yerine-kök-salıp sabit-taret olma (deploy: +menzil/−mobilite) mevcut sim'de **yok** → **[YENİ SİM-HOOK] / ADR.** **Base-building DEĞİL** (§7/§15.7 reddi); çekirdek-güvenli sürüm = yürüyen-havan (♻️).
- **Güç:** heykel/duvar/zırh yıkıcısı; çok yüksek HP. **Zaaf:** **çok yavaş + pahalı + düşük atış-hızı**; swarm'a çaresiz (AoE yok); flank ×2.0; `Blunt` ayna-zaafı.

### 8.6 Akıncı-Sürücü (Outrider) — **Flanker**
- **Rol:** Flanker (hızlı mobilite). **Zırh:** `Light`. **Hasar:** `Blunt`. ♻️
- **Lore:** Hızlı tekerlekli keşif-aracı; Mechanized'in **tek** hızlı birimi.
- **Savaş amacı:** Mechanized'in **yegâne mobilite kaldıracı** — erken harita-baskısı + düşman backline-flank'ı.
- **Güç:** hızlı; `Unarmored`/`Light` backline'ı flank'lar (Blunt 1.0/0.9 — orta; pozisyonel ×1.5/×2.0 telafi eder). **Zaaf:** `Light` + kırılgan; siege-değeri yok; **öngörülebilir** (fraksiyonun tek flanker'ı).

> **⚙️ Tasarım-seçimi notu (Outrider):** "Benzersiz-Çıkarım Izgarası" (Part 3.2) Mechanized'e Caster yerine **Flanker** verir. Bu, "yavaş siege fraksiyonu"na **tek** bir hız-çıkışı ekler (StarCraft Terran'ın Hellion'u gibi) — **kasıtlı, okunur bir sınır.** **Alternatif (ADR-owned):** LSD bunun yerine Mechanized'i *omit-Flanker / keep-mechanical-Caster* ("Arc-Tech" tesla-birimi, Fire/Poison) yapabilir. Bu rapor **ızgarayı önerir** (her rolün full-vision'da tam 1 fraksiyonda eksik olması daha temiz denge); ama bu **LSD/GD kararıdır.**

### Mechanized roster özeti

| Birim | Rol | Zırh | Hasar | Doldurduğu matris-hücresi | Yeni hook? |
|---|---|---|---|---|---|
| Dökümhane-Hizmetkârı | Miner | Light | Melee | (standart) | ♻️ |
| Sur-Yürüyücü | Frontline | Shielded | Melee | (standart) | ♻️ |
| Siper-Kırıcı | Skirmisher | Heavy | **Blunt** | **Blunt×Heavy (seyrek)** | ♻️ |
| Oto-Top | Ranged | Light | Pierce | (standart) | ♻️ |
| Kuşatma-Motoru | Heavy | Heavy | **Blunt** | **Blunt×Structure (siege)** | ♻️ / ⚙️ deploy |
| Akıncı-Sürücü | Flanker | Light | **Blunt** | **Blunt×Light** | ♻️ |

---

# PART 9 — Mechanized Counterplay Analizi

## 9.1 Mechanized → Iron Pact zırhı & formasyonları

- **Iron Shieldman (Shielded):** **Oto-Top `Pierce` ×1.5** + **Kuşatma-Motoru `Blunt` ×1.4** → çifte-counter. ✅
- **Iron Ironclad/Legionary (Heavy):** **`Blunt` ×1.2** (Siper-Kırıcı/Siege-Engine) → Iron'un Pierce'i Mechanized Heavy'ye 0.8 yerken, Mechanized Blunt Iron Heavy'ye 1.2 → **dayanıklılık-ticaretini Mechanized kazanır.**
- **Iron Line/Tight:** Mechanized **menzil-üstünlüğü** (Autocannon) + Siege-Engine ile Line'ı *uzaktan* aşındırır. Ama Iron **flank'lar** (Mechanized yavaş) → Mechanized **Line-facing** tutmalı (Part 11).

## 9.2 Mechanized → Ashen swarm & mobilite *(en zor matchup)*

- **Ashen swarm:** **Mechanized'in yapısal zaafı** — AoE yok → swarm'ı tek-hedef kırar, temizleyemez. `Heavy` zırh swarm-hasarını emer (Melee 0.9), ama yeterince hızlı öldüremez. **Çözüm:** **büyü-draftı** (Arrow Storm / Lightning Storm) AoE-boşluğunu doldurur (Part 13) — Mechanized **büyüye en bağımlı** fraksiyon.
- **Ashen Houndmaster (flank):** yavaş Mechanized'i arkadan (×2.0) avlar. **Çözüm:** Outrider ile karşı-flank; Bulwark-Walker'ı Loose-değil-Line tutup yanları kapatma.

## 9.3 Mechanized → Arcane büyü-baskısı

- **Arcane AoE (Baş-Büyücü):** Mechanized kümelenirse (Tight) Fire-AoE yer → **Mechanized Line/dağınık durmalı** (Part 11).
- **Arcane `Unarmored` backline:** Mechanized `Blunt`/`Pierce` + Outrider-flank → Büyü-Oklusu/Baş-Büyücü'yü tek-vuruşta ezer (Melee/Blunt `Unarmored`'a 1.3/1.0; pozisyonel ×2.0). Arcane casterı **bir kez** yakalanırsa ölür.
- **Arcane kite/kontrol:** Mechanized'in yavaşlığı burada cezalanır — Chilled/Stunned (büyü) Mechanized'i kilitler. **Çözüm:** komutan-cleanse (Part 12, ⚙️) veya Outrider-baskısıyla casteri erken düşürme.

## 9.4 4-Fraksiyon counter-ağı (özet)

| Saldıran ↓ \ Hedef → | Iron Pact | Ashen | Arcane | Mechanized |
|---|---|---|---|---|
| **Iron Pact** | — | flank+sustain ⚖️ | Heavy-zırh ✅ | flank+tempo ⚖️ |
| **Ashen** | swarm-stall ⚖️ | — | flank-backline ✅ | **swarm vs no-AoE ✅✅** |
| **Arcane** | Pierce-duvar + AoE ✅ | AoE vs swarm ✅ | — | kite+kontrol ✅ |
| **Mechanized** | Blunt-attrition ✅ | **no-AoE vs swarm ❌** | Heavy-zırh + Blunt ✅ | — |

> **Denge okuması:** Hiçbir fraksiyon **kesin-baskın değil** (§5.1 ✅). Her birinin en az bir kötü-matchup'ı var: Iron↔flank, Ashen↔AoE, Arcane↔Heavy/flank, Mechanized↔swarm. **Taş-kağıt-makas değil, taş-kağıt-makas-kertenkele** (dik iki eksen: tempo↔attrition **ve** control/AoE↔siege/kinetik). Tüm değerler **PROVISIONAL**, telemetri/RC ile ayarlanır (§16 risk #5).

---

# PART 10 — Armor Matrix Entegrasyon Denetimi

**Amaç:** her yeni birimin matris-içinde yaşadığını ve **hiçbirinin matrisi kırmadığını** ispatla.

## 10.1 12 yeni birimin matris-haritası

| Fraksiyon | Birim | Zırh-kategorisi | Hasar-kategorisi | Yeni kategori? |
|---|---|---|---|---|
| Arcane | Leyline Çırağı | Light | Melee | Hayır |
| Arcane | Ward-Weaver | Shielded | Melee | Hayır |
| Arcane | Rün-Bıçağı | Light | Melee | Hayır |
| Arcane | Büyü-Oklusu | **Unarmored** | Pierce | Hayır (boş hücre dolumu) |
| Arcane | Baş-Büyücü | **Unarmored** | Fire | Hayır (boş hücre dolumu) |
| Arcane | Faz-Avcısı | Light | Melee | Hayır |
| Mech | Dökümhane-Hizmetkârı | Light | Melee | Hayır |
| Mech | Sur-Yürüyücü | Shielded | Melee | Hayır |
| Mech | Siper-Kırıcı | Heavy | **Blunt** | Hayır (seyrek satır dolumu) |
| Mech | Oto-Top | Light | Pierce | Hayır |
| Mech | Kuşatma-Motoru | Heavy | **Blunt** | Hayır |
| Mech | Akıncı-Sürücü | Light | **Blunt** | Hayır |

**Sonuç:** **Sıfır yeni zırh-sınıfı, sıfır yeni hasar-tipi.** Matris **5×4** kalır. Yeni asimetri tamamen **var-olan-ama-kullanılmayan hücrelerin** (`Unarmored` sütunu; `Blunt`/`Fire` satırları) doldurulmasıyla gelir → §3.5 power-nötr benzersizlik ispatı.

## 10.2 Hücre-kapsama analizi (matris artık ne kadar "canlı")

| Hücre | MVP'de kullanan | + Yeni fraksiyonlarla kullanan |
|---|---|---|
| `Unarmored` sütunu | **(hiç)** | **Arcane** Büyü-Oklusu + Baş-Büyücü → sütun **canlanır** |
| `Blunt` satırı | yalnız Ashen Slinger | + **Mech** Siper-Kırıcı, Kuşatma-Motoru, Akıncı-Sürücü → satır **canlanır** |
| `Fire` satırı | yalnız Iron Battlemage | + **Arcane** Baş-Büyücü → satır canlanır |
| `Pierce` satırı | Iron Crossbow | + Arcane Büyü-Oklusu, Mech Oto-Top |

> **İçgörü:** Yeni fraksiyonlar matrisi **genişletmeden derinleştirir** — daha önce ölü olan hücreler artık fiilî counter-ilişkilerine dönüşür. Bu, "modern ama kısıtlı, fantasy-bloat'tan kaçın" (§5) ilkesinin doğrudan uygulanışıdır.

## 10.3 Tespit edilen denge riskleri (dürüst)

| # | Risk | Açıklama | Mitigasyon (LSD/ADR) |
|---|---|---|---|
| R1 | **`Unarmored` aşırı-kırılgan** | Melee 1.3 / Fire 1.4 / Poison 1.3 → Arcane backline "çok cam" hissedip un-fun olabilir. | HP/menzil/maliyet ile telafi (PROVISIONAL); ekran-birimleri (Ward/Rün-Bıçağı) zorunlu. |
| R2 | **`Pierce` ×1.5 vs Shielded — Shielded enflasyonu** | Hem Arcane (Büyü-Oklusu) hem Mech (Oto-Top) Pierce taşır → 4-fraksiyonlu metada **Shielded frontline'lar zayıflayabilir** (Iron Shieldman, Arcane Ward, Mech Walker hepsi Pierce'e 1.5 yer). | Shielded HP-bandını gözden geçir; Pierce-birim maliyet/menzilini ayarla; telemetri-izle. |
| R3 | **Blueprint↔implementasyon Pierce/Shielded çelişkisi** | Blueprint §4 illüstratif matris: `Pierce 0.6 (frontal) vs Shielded` ("shields negate frontal arrows"); ama `CounterMatrix.asset`: **`Pierce 1.5 vs Shielded`.** Ters yönler. | **PROVISIONAL/LSD-ADR ile sonlandırılmalı.** Bu rapor **implementasyon değerini** (1.5) taban alır ve çelişkiyi işaretler — **kanon değişikliği önermez.** |
| R4 | **`Structure` zırhı ↔ `Unarmored` enum adı** | Blueprint §4 4. zırh = "Structure"; enum = `Unarmored`. Mechanized siege-matematiği (Blunt vs Structure) bu yuvaya bağlı; ayrıca heykel kendi **shield-phase**'ine sahip (§11, ayrı throttle). | Anti-Structure siege-değeri **PROVISIONAL**; Structure-zırh adı + heykel-etkileşimi **ADR-owned.** |
| R5 | **Mechanized AoE-yokluğu telafi-bağımlılığı** | Caster yok → swarm'a karşı büyü-drafta bağımlı; draft kötüyse Mechanized "yapısal olarak zayıf" hissedilebilir. | Büyü-meta tuning (Part 13); Outrider/Heavy-zırh kite-değeri. |
| R6 | **Yeni sim-hook'lar (⚙️) denge-yüzeyi açar** | Ward-aura, deploy, blink, target-mark, heal-aura, control-on-hit → her biri yeni denge-değişkeni. | Hepsi **ADR-gated, opsiyonel**; çekirdek-fraksiyon bunlarsız da tam-işlevsel (♻️ sürümler). |

> **İspat:** Yukarıdaki risklerin **hiçbiri matrisi kırmaz** — hepsi *değer-tuning* (R1/R2/R5) veya *isim/ADR-sonlandırma* (R3/R4) veya *opsiyonel-hook* (R6) seviyesinde. Matrisin **yapısı** (5×4, mevcut counter-mantığı) dokunulmadan kalır. "No unit breaks the matrix" → **ispatlandı.**

---

# PART 11 — Formasyon Etkileşim Denetimi

*(FormationType: Line · Tight · Loose — `Formation.cs`. NOT: `FormationMember` kablolaması **DEFERRED** (phase raporları) — formasyonlar authored, üyelik-ataması pre-GATE-2 follow-up. Bu bölüm **ileriye-dönük** tasarımdır.)*

## 11.1 Formasyon mekaniği (implementasyon)

| Formasyon | Geometri (`Formation.cs`) | Taktiksel etki |
|---|---|---|
| **Line** | geniş, sığ yanal yelpaze (tek-rank, `k_FileSpacing≈0.6`) | yanal hasar-girişini yayar; frontal güçlü, **flank/AoE'ye zayıf** |
| **Tight** | kümelenmiş rank'lar (`k_TightScale≈0.7`, 4/rank) | yüksek melee-yoğunluğu/DPS, **AoE'ye açık** |
| **Loose** | dağınık geniş+derin (`k_LooseScale≈1.8`) | **AoE-dirençli**, ranged-dostu |

## 11.2 Arcane formasyon doktrini

- **Tercih: Loose.** Kırılgan `Unarmored` casterları (Büyü-Oklusu/Baş-Büyücü) **AoE'den ve kümelenme-cezasından** korur. Arcane kendisi AoE-fraksiyonu olduğundan, kümelenmeyi sevmez (kendi zaafına düşmez).
- **Karşı-formasyon avı:** Arcane'in Baş-Büyücü'sü düşman **Tight**'ını cezalandırır → rakibi Loose'a zorlar (DPS-yoğunluğunu kırar). Arcane = "düşmanı dağılmaya zorlayan" fraksiyon.
- **Faz-Avcısı:** formasyon-dışı operatör; düşman **Line**'ının yanına/arkasına sızar (×1.5/×2.0).

## 11.3 Mechanized formasyon doktrini

- **Tercih: Line.** Sur-Yürüyücü'ler **Line** frontal-duvarı; arkada Oto-Top + Kuşatma-Motoru ateş eder (artillery-doktrini). Yavaş birimler Line-disiplinini doğal kılar.
- **Choke-kırma: Tight.** Bir choke'u (TerrainKind.Choke) yoğun-itişle aşmak için Tight; ama **Arcane/Ashen AoE'sine açık** → riskli, durum-bağımlı.
- **Flank-savunması:** Mechanized yavaş → **Line-facing'i tehdide döndürmek** kritik; yanları açık Line = ölüm. Outrider yan-tarama yapar.

## 11.4 Formasyon-counter özeti

| Fraksiyon | Sever | Kaçınır | Sömürdüğü düşman-formasyonu |
|---|---|---|---|
| Iron Pact | Line/Tight (sustain) | — | Loose (düşük yoğunluk) |
| Ashen | Loose (flank-yayılımı) | Tight (AoE-yemi) | Line yanları |
| **Arcane** | **Loose** (caster-koruma) | Tight (kendi AoE-zaafı) | düşman **Tight** (AoE-cezası) |
| **Mechanized** | **Line** (artillery-duvarı) | Loose (DPS-seyrelmesi) | düşman dağınık-Line yanları (yavaş→sınırlı) |

---

# PART 12 — Komutan Genişleme Konseptleri

*(Görev "rol paleti"ndeki **"Commander Support Logic"** buraya düşer — §1.3.d. Tüm konseptler: 1 active + 1 passive; **≤%15 bütçe** (`k_PowerBudgetCeiling`); ADR-2-002 stacking (komutan-buff'ları ≤bütçeye clamp, spell-buff'ları ayrı); **earnable + cosmetic-only premium**; **ranked-normalized**; **sidegrade, power-creep değil** (§6). Yeni `CommanderActiveKind`/`PassiveKind` enum'ları = ⚙️ **[YENİ SİM-HOOK]/ADR**, tıpkı `future/001` gibi. Değerler PROVISIONAL.)*

## 12.1 Arcane Order — 2 komutan konsepti

| # | Komutan | Kimlik | Active | Passive | Savaş rolü | Bütçe notu |
|---|---|---|---|---|---|---|
| A1 | **The Conduit (Kanal-Usta)** | büyü-çekirdeğini hızlandıran | ♻️ *Mana Surge* — yakın caster/ranged'e kısa **Hasted** (mevcut StatusKind) | ♻️ *Leyline Attunement* — küçük ekonomi/regen (**GoldBoost**, Quartermaster-benzeri) | caster-amplifikatörü | **≤0.15**; Hasted-clamp ADR-2-002 |
| A2 | **Warden of Seals (Mühür Bekçisi)** | anti-büyü/koruma | ⚙️ *Null-Field* — küçük bölgede düşman **büyü-telegraph'ını bastır** / müttefik status-cleanse → **[YENİ SİM-HOOK]/ADR** | ♻️ *Warded Ranks* — küçük frontal hasar-azaltımı (Cover-benzeri aura) | kontrol-kıran destek | active = yeni hook; bütçe ≤0.15 |

## 12.2 Mechanized Dominion — 2 komutan konsepti

| # | Komutan | Kimlik | Active | Passive | Savaş rolü | Bütçe notu |
|---|---|---|---|---|---|---|
| M1 | **The Foreman (Ustabaşı)** | yavaş itişi sürdüren | ♻️ *Overclock* — yakın makinelere kısa **Hasted+Raged** (Rally/WarCry-benzeri) | ⚙️ *Field Repair* — yavaş HP-regen aura → **[YENİ SİM-HOOK]/ADR** (mevcut "Healing" StatusKind yok). Çekirdek-güvenli alternatif: ♻️ hasar-azaltım aura | sustain-itiş | passive = yeni hook; ≤0.15 |
| M2 | **Siege-Marshal (Kuşatma-Mareşali)** | kuşatma-amplifikatörü | ⚙️ *Bombardment Mark* — hedef-bölgeyi müttefik ranged/siege için **+hasar işaretle** (telegraph'lı, counter'lı) → **[YENİ SİM-HOOK]/ADR** | ⚙️/♻️ *Breacher Protocol* — Structure/Shielded'a küçük +hasar (koşullu passive) | siege-odaklı | active+passive = yeni hook; ≤0.15 |

## 12.3 Komutan-bütçe uyum ispatı

- Her active+passive bir birimde ADR-2-002 ile **≤%15'e clamp'lenir** (komutan-attributable); spell-buff'ları ayrı katman (§5.3).
- Hepsi **tempo/utility/koşullu** — **raw stat-inflation yok** (§6). Örn. Siege-Marshal'ın "+hasar"ı **işaretli-bölge + telegraph + counter** ile sınırlı, global stat değil.
- **Ranked normalizes** (mevcut `RankedNormalized` hook) → talent-set capped; strictly-dominant pick yok.
- **Earnable + cosmetic-only:** komutanlar play/battle-pass ile; premium yalnız skin/VFX/voice (§6) → **no P2W.**
- ⚙️-işaretli 4 hook (Null-Field, Field-Repair, Bombardment-Mark, Breacher) **bugünün kanonu değil** → Phase 7.3/7.5 ADR'leri gerektirir. Çekirdek-güvenli ♻️ alternatifleri her zaman mevcut.

---

# PART 13 — Büyü Ekosistemi Entegrasyonu

*(Referans: `future/002-spell-synergy-web` çerçevesi. **Kritik:** büyüler **fraksiyon-agnostiktir** — oyuncu ~12'lik havuzdan **draft-3** yapar (§5.3); büyüler fraksiyona ait değildir. Bu rapor **yeni büyü/kategori icat ETMEZ** (002 zaten bunları ADR-gated işaretledi); yalnızca **her fraksiyonun mevcut 5 kategoriyle nasıl etkileştiğini** analiz eder.)*

## 13.1 Mevcut 5 kategori × 2 yeni fraksiyon

| SpellCategory | Arcane etkileşimi | Mechanized etkileşimi |
|---|---|---|
| **Offensive** (Shatter, Arrow Storm, Lightning Storm) | **Yüksek sinerji** — Baş-Büyücü Fire + Control-büyüleri → Freeze→Shatter ×2 zincirleri | **Hayatî** — Caster-yokluğunun **AoE telafisi** (Arrow/Lightning Storm = Mechanized'in tek alan-temizliği) |
| **Control** (Freeze, Stun, Poison Cloud) | **Yüksek** — Arcane'in kontrol-kimliğini büyü-katmanından besler (çekirdek-güvenli; Part 5.5 ⚙️ yerine) | Orta — yavaş düşmanı kilitleyip siege-penceresi açar |
| **Economy** (Gold Rush, Raise Gold) | Orta — pahalı glass-ekonomiyi fonlar | **Yüksek** — pahalı/yavaş makineleri erken fonlar (kurulum-ekonomisi) |
| **Summon** (Pouncer, Giant) | Düşük — Arcane'in zaten bedeni var | **Yüksek** — Summon Giant ek-Heavy/blocker sağlar; Mechanized kütleyi sever |
| **Buff** (Rage, Haste) | Orta — Faz-Avcısı/Rün-Bıçağı'na tempo | Orta — yavaş birimlere Haste değerli |

## 13.2 İki kilit içgörü

1. **Büyü-draftı yapısal-boşluğu kapatır (self-balancing).** Mechanized'in *Caster-yokluğu* bir **körlük değil, bir draft-kararıdır:** Mechanized oyuncusu AoE için Offensive-büyü draftlar. Fraksiyon-agnostik havuz, fraksiyon-asimetrisini **otomatik dengeler** → §7 "modlar tek çekirdeği paylaşır" ilkesinin zarif sonucu.
2. **Arcane + Control-büyüleri = oppression riski (işaretli).** Arcane *zaten* kontrol-fraksiyonu; üstüne Freeze+Stun+Poison draft ederse **çift-kontrol** NPE/oppression doğabilir. **Mitigasyon (LSD/002-ref):** telegraph/counter/cooldown disiplini (§5.3 "no un-counterable spell") + ranked-normalization + Arcane'in kırılganlığının kontrol-süresini sınırlaması. Bu, **denge-tuning**, kanon-değişikliği değil.

## 13.3 Kanon-uyum

- **Sıfır yeni büyü/kategori/StatusKind** önerilir (hepsi 002'de ADR-gated). Yeni fraksiyonlar **mevcut havuzu** kullanır.
- Büyü-buff'ları komutan-bütçesinden **ayrı** (ADR-2-002) → fraksiyon-eklemesi bu ayrımı **değiştirmez.**
- Fraksiyon-spesifik "güçlü draft"lar bir **meta-gözlemidir** (LSD/telemetri), kanon-kuralı değil.

---

# PART 14 — Görsel Üretim Denetimi

*(Referans: `future/000-assets-roadmap` (asset master audit). İlke: **paylaşılan-iskelet + fraksiyon-reskin** = içerik-hız motoru (§10). Renk-körü-güvenli paletler + silüet-sign-off zorunlu. Tüm sayılar **PROVISIONAL tahmin.**)*

## 14.1 Fraksiyon başına asset tahmini

| Kategori | Arcane Order | Mechanized | Not |
|---|---|---|---|
| **Birim iskeletleri (Spine)** | 6 (3'ü mevcut archetype-iskelet reuse) | 6 (3'ü reuse) | §10 paylaşılan-iskelet |
| **Birim reskin/silüet** | 6 base-silüet | 6 base-silüet | read-locked (§6) |
| **Silahlar** | 6+ (rün-bıçağı, cıvata, asa, çırak-kazması) | 8+ (balyoz, oto-top, havan, tekerlek) | Mechanized daha çok mekanik-parça |
| **Zırh/outfit-class (5 tier)** | 6 birim × 5 tier = 30 kozmetik-set | 30 | §6 monetizasyon |
| **Heykel (statue)** | 1 (Leyline-Dikilitaşı) + 4 damage-state | 1 (Foundry-Core) + 4 damage-state | §11 ikonik objektif |
| **Maden (mine) varyantı** | 1 (leyline-node) | 1 (cevher-döküm) | flavor-reskin |
| **VFX setleri** | **Yüksek** — büyü/AoE/telegraph/kontrol (mor-altın) | Orta — duman/namlu/perçin/deploy | Arcane VFX-ağırlıklı |
| **Komutan (2)** | 2 + skinler | 2 + skinler | Part 12 |
| **⚙️ Yapı/deploy assetleri** | 0 (gerekmez) | **+ deployable-emplacement** (ADR'lenirse) | Mechanized'e ekstra |

## 14.2 Perf-bütçesi uyarıları (§11/§12 — mobil hard-gate)

- **Arcane = VFX-perf riski.** Yoğun parçacık/AoE → mobil parçacık-cap'ine (§10 "strict mobile particle budgets") çarpabilir. **Mitigasyon:** instancing + parçacık-LOD + telegraph-öncelikli VFX.
- **Mechanized = draw-call/rig riski.** Karmaşık mekanik rig'ler + (ADR'lenirse) sabit-yapılar → draw-call/crowd-instancing baskısı. **Mitigasyon:** GPU-instancing (§7), modüler-parça reuse.

---

# PART 15 — Genişleme Maliyet Analizi (Arcane vs Mechanized)

| Efor ekseni | **Arcane Order** | **Mechanized Dominion** |
|---|---|---|
| **Tasarım** | Orta — kontrol/AoE archetype + NPE-tasarımı (oppression-önleme) | Orta — siege archetype; davranışlar daha basit (kinetik) |
| **Sanat** | **Yüksek** — VFX-ağırlıklı büyü-dili | **Yüksek** — mekanik rig + (ADR'lenirse) yapı + duman |
| **Mühendislik** | **Düşük–Orta** — büyük ölçüde mevcut sim-hook reuse; ⚙️ hook'lar opsiyonel | **Yüksek** — deploy/structure/repair/mark = **NET-NEW sim-hook + ADR'ler**; çekirdek-döngü-çatallama riski (§7/§15.7) |
| **Dengeleme** | **Yüksek** — kırılgan-kontrol *en zor dengelenen* archetype (control-NPE, AoE-vs-swarm tuning) | **Orta** — kinetik, net counter'lar; ama no-AoE-vs-swarm dikkat ister |
| **Live-ops** | Orta — büyü-meta etkileşimleri (Part 13.2 oppression-izleme) | Orta — siege-meta; AoE-telafi draft-izleme |
| **Çekirdek-döngü riski** | **Düşük** — döngüyü çatallamaz | **Yüksek** — "structures" base-building'e kayarsa §7 ihlali (sıkı ADR-bekçiliği şart) |

**Net:** **Arcane = inşası daha ucuz, dengelemesi daha zor.** **Mechanized = inşası/riski daha yüksek (yeni sistemler + çekirdek-döngü tehdidi), dengelemesi daha kolay.**

---

# PART 16 — Yayın Stratejisi Analizi

## 16.1 Karar kriterleri

| Kriter | Arcane önce | Mechanized önce |
|---|---|---|
| **Üretim maliyeti/risk** | ✅ düşük mühendislik; framework'ü ucuza kanıtlar | ❌ NET-NEW yapı-sistemleri; erken-risk |
| **Çekirdek-döngü bütünlüğü** | ✅ döngüyü çatallamaz | ❌ "structures" en büyük §7/§15.7 tehdidi |
| **Oyuncu çekiciliği / yenilik** | ✅ iki melee-merkezli fraksiyona **maksimum kontrast** (büyü) | ⚖️ siege tatmin-edici ama "ağır makine" daha az kontrast |
| **Counter-web esnekliği testi** | ✅ control/AoE eksenini erken test eder | ⚖️ siege ekseni |
| **Denge riski** | ❌ kırılgan-kontrol = en zor denge + NPE | ✅ kinetik = daha öngörülebilir denge |
| **Kanon sıralama-ipucu** | ✅ §5.1 + Successor §8: "Arcane … Mechanized" sırası | — |

## 16.2 Tavsiye (advisory)

**Arcane Order ÖNCE** (Phase 7.3 "3. fraksiyon" / S2), **Mechanized SONRA** (full-vision 4. fraksiyon). Gerekçe:
1. **Mühendislik + çekirdek-döngü riski belirleyici.** Mechanized'in deployable-structure hook'ları en tehlikeli erken-risktir (§7 çatallama). Arcane **neredeyse tümüyle mevcut sim üstünde** kurulur → **fraksiyon-genişleme framework'ünü** (Part 3) önce **ucuza ve güvenle kanıtlar.**
2. **Maksimum kontrast = maksimum yenilik.** İki MVP fraksiyonu da kinetik/melee-merkezli; Arcane (büyü/kontrol) en taze deneyimi ve counter-web'in **esneklik-stres-testini** verir.
3. **Kanon-ipucu** Arcane-önce sırasını destekler (§5.1, Successor §8).

> **Dürüst karşı-argüman:** Arcane'in kırılgan-kontrol archetype'ı **en zor dengelenen** ve NPE-riski en yüksek olandır. **Denge-riskinden** kaçınmayı önceleyen bir GD, **Mechanized-önce** seçebilir (kinetik = güvenli denge). Bu rapor **Arcane-önce** önerir çünkü **çekirdek-döngü bütünlüğü riski** (Mechanized) **denge-riskinden** (Arcane) daha tehlikeli ve geri-alınması zordur; Arcane'in denge-riski mevcut **telemetri/RC omurgası** + ertelenmiş-kapı disipliniyle yönetilebilir. **Bu, LSD/GD'nin ADR-kararıdır.**

---

# PART 17 — Nihai Tavsiye

## 17.1 Fraksiyon sıralaması & öncelik

| Sıra | Fraksiyon | Kanonik yuva | Doktrin-uyumu | Tavsiye |
|---|---|---|---|---|
| **1.** | **Arcane Order** | Phase 7.3 "3rd faction" / S2 | ✅ tam (control/AoE ekseni; sıfır yeni kategori) | **İlk büyük fraksiyon-genişlemesi** |
| **2.** | **Mechanized Dominion** | full-vision 4th faction | ✅ tam (siege ekseni) **eğer** structures ADR-gated kalır & döngü çatallanmazsa | İkinci; team+telemetri olgunlaştıktan sonra |

## 17.2 Her ikisi de BULWARK doktrinine **uyar — şu koşullarla:**

1. **Benzersiz-Çıkarım Izgarası** benimsenir (Arcane omit-Heavy; Mechanized omit-Caster) → her fraksiyon farklı rol çıkarır, dengeli full-vision rol-dağılımı (Part 3.2).
2. **Sıfır yeni zırh/hasar sınıfı** — yalnız boş `Unarmored`/`Blunt`/`Fire` hücreleri doldurulur (Part 10) → matris kırılmaz.
3. **Mechanized "structures" ASLA çekirdek-döngüyü çatallamaz** — Heavy/siege rolü + (opsiyonel) ADR-gated deployable; **base-building reddedilir** (§7/§15.7).
4. **Tüm ⚙️ sim-hook'lar** (ward-aura, deploy, blink, target-mark, heal-aura, control-on-hit, null-field) **birer ADR gerektirir**; çekirdek-fraksiyon bunlarsız tam-işlevsel.
5. **Tüm değerler PROVISIONAL/LSD-owned** (§15.6); telemetri/RC ile dengelenir.
6. **Phase 7.3 tetikleyicisine bağlı** — *"two-faction balance stable in telemetry"* ateşlenene dek (GATE 1/2/3 + soft-launch LTV sonrası) **hiçbir şey başlamaz.**

## 17.3 Önerilen genişleme yol-haritası (advisory — roadmap değişikliği DEĞİL)

```
[BUGÜN] Phase 0–3 authored + compile PASS → Unity-config (in-progress)
   ↓ (zorunlu kapılar — bu rapordan tamamen bağımsız)
GATE 1 (fun) → GATE 2 (playtest) → GATE 3 (server-validated) → GATE 4 → GATE 5 (soft-launch LTV)
   ↓
[Decision Log §2 tetikleyici] "two-faction balance stable in telemetry" ATEŞLENİR
   ↓
S2 / Phase 7.3 → ADR: Arcane Order (3. fraksiyon)  ← bu raporun ön-tasarımı girdi olur
   ↓
full-vision → ADR: Mechanized Dominion (4. fraksiyon)  ← deployable-structure ADR'leriyle
```

## 17.4 Tek-cümle nihai tavsiye

> **Arcane Order ve Mechanized Dominion, BULWARK'ın kanonen-öngörülen full-vision fraksiyonlarıdır ve doktrine *uyarlar*; "Magic-yok-Kas (Mechanized) ↔ Kas-yok-Magic (Arcane)" ayna-zıtlığı + Benzersiz-Çıkarım Izgarası + sıfır-yeni-matris-kategorisi ile güçlü asimetri sağlanır. ÖNCE Arcane Order (düşük mühendislik-riski, framework'ü kanıtlar, maksimum kontrast), SONRA Mechanized (structures sıkı ADR-bekçiliğiyle) önerilir — ancak yalnızca Phase 7.3 tetikleyicisi ateşlendikten sonra. Bugün hiçbir şey değişmez.**

---

## ⛔ Yetki & Kapsam Beyanı (closing)

Bu rapor **yalnızca gelecek-araştırmadır.** **Şunları YAPMAZ:** roadmap/kanon/decision-log/ADR değiştirmek · özellik/birim/fraksiyon/komutan/büyü yetkilendirmek · güç-bütçesi veya matris-değeri değiştirmek · faz-önceliği değiştirmek · implementasyon başlatmak. **Şunları YAPAR:** keşfeder · analiz eder · tavsiye eder · değerlendirir. Tüm yeni mekanik önerileri ⚙️ **[YENİ SİM-HOOK]** ile işaretlidir ve **birer ADR gerektirir**; mevcut atomlarla karşılananlar ♻️ **[MEVCUT MEKANİK]** etiketlidir. Tüm sayısal değerler **PROVISIONAL/LSD-owned** (§15.6). Hiçbir `report/`, `docs/adr/`, `decision log`, `docs/execution/` dosyası değiştirilmemiştir. **Aktif geliştirme akışı değişmeden devam eder:** CI/CD doğrulama · APK üretimi · Unity doğrulama · Phase 0–3 kapı borç-eritimi.

*Kalite çıtası: Lead RTS Designer · Faction Designer · Combat Designer · Systems Designer. Reskin yok · power-creep yok · jenerik fantezi/sci-fi yok · güçlü asimetri + matris-etkileşimi + counterplay + okunabilirlik + rekabetçi bütünlük korunur. — `future/005-new-faction-expansion/`, 2026-06-04.*
