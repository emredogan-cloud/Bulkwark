# BULWARK — Görsel Üretim Envanteri & Asset Tedarik Master Denetimi (TR)

> **Belge tipi:** Görsel-üretim envanteri ve **tedarik + entegrasyon planı**. Bu belge model/asset **üretmez**; BULWARK'ın ihtiyaç duyduğu **her** görsel asset'i tanımlar, sınıflandırır, kaynak önerir ve Unity'ye giriş hattını (pipeline) belirler.
> **Otorite:** Bu denetim, kanonik dökümanlara tabidir — `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (yasa), `NEXTGEN_RTS_SUCCESSOR_REPORT.md`, `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`, `PRODUCTION_DECISION_LOG.md`, `ROADMAP_CHANGELOG.md`. Görsel kararlar **§6 (kozmetik-güvenlik), §10/Blueprint §10 (sanat bible), §11 (dünya), §12 (perf)** kurallarına uyar.
> **Tarih:** 2026-06-03 · **Kapsam:** Phase 0–3 implementasyonu (scaffold) çapraz-kontrol edildi · **Varsayım:** *Hiçbir görsel asset mevcut değil* (Art/README + Phase 0–3 raporları teyit eder — her şey "authored, NOT compiled" iskelet).
> **Hedef okuyucu:** Lead Technical Artist · Lead Environment Artist · Art Director · Unity Technical Artist. Bu belgeyi okuyan bir ekip üyesi, **roadmap'i tekrar okumadan** tüm sanat asset'lerini tedarik edebilmelidir.

---

## 0. Yönetici Özeti & Kritik Bulgular (Çapraz-Kontrol)

BULWARK, *Stick War* halefi, mobil-öncelikli (Android/iOS, Unity 6 LTS, URP 2D, IL2CPP), tek-cepheli taktiksel **RTS-lite**'tir. Çekirdek döngü: **maden → eğit → ittir → heykeli yık**. MVP içeriği bilinçli olarak **kısıtlı ve okunabilir** tutulur (anti-bloat). İmplementasyon, içeriği **veri (ScriptableObject `.asset`)** olarak somutlaştırmış durumda — bu denetim o somut verinin üstünden gider, hayal kurmaz.

**MVP somut içerik (implementasyondan sayıldı):** 2 fraksiyon · 12 birim (6+6) · 2 komutan · 12 büyü · 3 harita · 20 seviyelik Kampanya (Act 1) · Endless · Async Ghost Ladder · 4 para birimi · 5 kozmetik kademe.

### Art Director / Tech Artist çapraz-kontrolünden çıkan 7 kritik bulgu

| # | Bulgu | Etki | Eylem |
|---|---|---|---|
| **KB-1** | **Kanon sanat yönü = 2D Spine iskeletsel animasyon** (Roadmap §2, Blueprint §10, ChangeLog §1 [PRESERVE]). Ancak görevin tedarik kaynakları (Synty, POLYGON, Quaternius) ağırlıkla **3D low-poly**. Bu, temel bir **pipeline çatallanması**dır. | 2D mi 3D/2.5D mi sorusu, tüm tedariki belirler. "Spine 2D readability" **[PRESERVE]** dispozisyonudur → değiştirmek **ADR gerektirir (§15/§16, ihlal edilemez değil ama kanon)**. | Bölüm 5'te **iki yol** sunulur (Kanonik-2D ve 2.5D-3D). **Öneri: ADR ile 2.5D düşük-poligon 3D'ye geçiş** (ortografik kamera ile 2D okunur, içerik-hızı yüksek). GD/TA onayı şart. |
| **KB-2** | **Gizli karakter asset'leri** 12-birim sayımında YOK ama görsel olarak ZORUNLU: **Houndmaster'ın tazıları**, **Razorbeast (binek/canavar)**, **summongiant (Dev)**, **summonpouncer (Pouncer)** büyü-yaratıkları, **2 komutan** (kahraman-ölçek). | Gerçek karakter rig sayısı 12 değil **~18'dir**. | Envantere (Bölüm 1) eklendi; her biri ayrı rig + animasyon. |
| **KB-3** | **Heykel tek asset değil:** 2 fraksiyon × **4 hasar durumu** (Intact/Cracked/Breaking/Destroyed) + **kalkan fazı** overlay'i = görsel olarak **8+ durum**. Implementasyon `StatuePhase` enum ile teyit eder. | En kritik (P0) çevre asset'i, en yüksek görünürlük (oyunun amacı). | Bölüm 7 + 8'de durum-bazlı üretildi. |
| **KB-4** | **Kozmetik sistem her şeyi ×5 çarpar:** Standard→Veteran→Elite→Legendary→Mythic. §6 gelir motoru AMA §16'nın **1 numaralı ticari riski** ("kozmetik içerik hızı"). | Taban asset seçimi **recolor/material-dostu** olmalı (palet-swap, trim, VFX-renk). Silüet kilitli kalır. | Bölüm 6'da kademe-sistemi; Bölüm 12'de maliyet çarpanı. |
| **KB-5** | **Okunabilirlik İHLAL EDİLEMEZ** (§6, §11, §12). Silüet kilitli; her arketip RTS kamerasında ayırt edilmeli; fraksiyon-rengi kimliği korunur. Ranked "clarity mode" standart okunabilir skin ister. | Tedarik kısıtı: satın alınan asset'ler **arketip başına ayrık silüete** sahip olmalı ve **uzak kamerada** okunmalı. | Bölüm 3 "Kamera Mesafesi" + Bölüm 6 silüet kuralları. |
| **KB-6** | **Şu an SIFIR art mevcut.** Phase 0–3 "authored, NOT compiled"; `Art/` sadece README + klasör kancası. | Görevin "hiçbir asset yok" varsayımı **doğrulandı**. | Tüm envanter sıfırdan tedarik. |
| **KB-7** | **Mobil perf bütçesi sert kapı** (§12, her faz). Yüzlerce birim mid-range telefonda. | Poligon/texture/particle bütçeleri, **LOD + GPU instancing + atlas** zorunlu. ECS/DOTS render yolu (Entities Graphics) instancing'i sever. | Bölüm 11 entegrasyon + mobil limitler. |

### Kanon görsel kuralları (tedarik için bağlayıcı kısıtlar)
- **Stil:** Temiz, stilize, **bold okunur silüetler**; "detaydan çok okunabilirlik" (§2, §11).
- **Fraksiyon paletleri:** **Iron Pact = çelik/kobalt (steel/cobalt)** · **Ashen Horde = kor/öküzkanı (ember/oxblood)** (Blueprint §10).
- **VFX renk-kodlu hasar tipi:** Slash=çelik · Pierce=beyaz · Blunt=toz · Magic=mor · Fire=turuncu (Blueprint §10). + Poison (Hexcaster) = zehir-yeşili (implementasyon `DamageType.Poison`).
- **Renk-körü güvenli paletler** zorunlu; silüet-ayırt-edilebilirlik her birim için imza ister.
- **Kozmetik ASLA değiştiremez:** silüet, birim boyutu/hitbox, animasyon zamanlaması, yetenek-VFX okunabilirliği, fraksiyon-renk kimliği (Art/README, §6 İHLAL EDİLEMEZ).

---

## İçindekiler
- **Phase 1** — Master Asset Envanteri (her görsel obje)
- **Phase 2** — Sınıflandırma (kategoriler)
- **Phase 3** — Asset başına dokümantasyon (Ad/Amaç/Rol/Fraksiyon/Stil/Kamera/Önem)
- **Phase 4** — Build / Buy / Kitbash / Generate kararı + gerekçe
- **Phase 5** — Tedarik stratejisi (Asset Store / Synty / Polygon / Quaternius / Kenney / OpenGameArt / CGTrader / Sketchfab) + En İyi/Yedek/Bütçe
- **Phase 6** — Karakter görsel kimliği (silüet/renk/zırh sınıfı/kozmetik sınıfı)
- **Phase 7** — Fraksiyon görsel rehberi (mimari/materyal/silah/zırh/bayrak/heykel/komutan)
- **Phase 8** — Harita üretim denetimi (terrain/foliage/kaya/cover/köprü/prop/harabe/hava-FX)
- **Phase 9** — Animasyon denetimi (reuse/retarget/custom)
- **Phase 10** — VFX denetimi (saldırı/vuruş/büyü/buff/debuff/ölüm/komutan/UI)
- **Phase 11** — Asset Entegrasyon Rehberi (FBX/Rig/Avatar/Materyal/Addressables/LOD/Optimizasyon/Mobil)
- **Phase 12** — Bütçe analizi (MVP/Launch/Full Vision asset sayıları; ücretsiz/indie/profesyonel yol)
- **Phase 13** — Master Tedarik Planı (P0/P1/P2/P3 öncelik)

---
## PHASE 1 — MASTER ASSET ENVANTERİ

Her görsel obje, kanonik dökümanlardan + Phase 0–3 implementasyon `.asset` verisinden çıkarıldı. **Hiçbiri varsayım değil**; her satır bir kanon bölümüne veya somut `.asset` dosyasına dayanır. ID şeması izlenebilirlik içindir (Bölüm 3–13 boyunca aynı ID kullanılır).

### 1.A Karakterler — Savaş Birimleri (12 arketip, veri ile teyitli)
Kaynak: `Assets/_Game/Data/Units/*.asset` (12 dosya, tam stat okundu).

| ID | Birim | Fraksiyon | Rol (UnitRole) | Hasar Tipi | Zırh | HP | Hız | Hasar/Menzil | Not (silüet ipucu) |
|---|---|---|---|---|---|---|---|---|---|
| CHR-01 | **Miner** (Iron Pact) | Iron Pact | Miner | Melee | Light | 60 | 2.4 | 4 / 1.0 | Kazma; ekonomi; zayıf savaş |
| CHR-02 | **Shieldman** | Iron Pact | Frontline | Melee | **Shielded** | 200 | 1.6 | 8 / 1.1 | Büyük kalkan + kısa kılıç; hat-tutucu |
| CHR-03 | **Legionary** | Iron Pact | Skirmisher | Melee | Heavy | 110 | 2.2 | 16 / 1.2 | Gladius; disiplinli lejyoner |
| CHR-04 | **Crossbow** | Iron Pact | Ranged | **Pierce** | Light | 70 | 2.0 | 14 / **6.0** | Arbalet; menzilli (→ mermi PRJ) |
| CHR-05 | **Battlemage** | Iron Pact | Caster | **Fire** | Light | 75 | 1.9 | 12 / 5.0 | Asa/tome; ateş AoE (→ mermi+VFX) |
| CHR-06 | **Ironclad** | Iron Pact | Heavy | Melee | Heavy | 260 | 1.4 | 30 / 1.3 | Dev balyoz; kuşatma/anti-yapı |
| CHR-07 | **Miner** (Ashen) | Ashen Horde | Miner | Melee | Light | 55 | 2.6 | 3 / 1.0 | Kaba kazma; daha hızlı/zayıf |
| CHR-08 | **Raider** | Ashen Horde | Skirmisher | Melee | Light | 85 | 2.8 | 14 / 1.1 | Balta/satır; ucuz hızlı DPS |
| CHR-09 | **Houndmaster** | Ashen Horde | **Flanker** | Melee | Light | 80 | **3.2** | 12 / 1.0 | Kırbaç + **TAZILAR** (bkz CHR-13) |
| CHR-10 | **Slinger** | Ashen Horde | Ranged | **Blunt** | Light | 60 | 2.2 | 12 / 5.5 | Sapan/bola; künt mermi (→ PRJ) |
| CHR-11 | **Hexcaster** | Ashen Horde | Caster | **Poison** | Light | 70 | 2.0 | 9 / 4.5 | Totem/asa; zehir debuff (→ VFX) |
| CHR-12 | **Razorbeast** | Ashen Horde | Heavy | Melee | Heavy | 240 | 1.8 | 22 / 1.2 | **Canavar** (binek-ölçek); doğal silah |

### 1.B Karakterler — Gizli/Türev (envanterde sıklıkla atlanan — KB-2)
| ID | Asset | Kaynak/gerekçe | Ölçek | Not |
|---|---|---|---|---|
| CHR-13 | **War Hound (tazı)** ×N | Houndmaster "Flanker" kimliği — tazısız okunmaz | Küçük dört-ayaklı | Sürü halinde; ayrı rig+anim |
| CHR-14 | **Giant (Dev)** — summon | `spell_summongiant.asset` (Summon kategorisi) | **Kahraman/dev** | Çağrılan birim; ayrı rig |
| CHR-15 | **Pouncer** — summon | `spell_summonpouncer.asset` | Orta-yırtıcı | Çağrılan birim; sıçrayan |
| CHR-16 | **Iron Warden** (Komutan) | `cmd_ironpact_warden.asset` | **Kahraman-ölçek** | Rally + Quartermaster; portre ister |
| CHR-17 | **Ashen Warchief** (Komutan) | `cmd_ashen_warchief.asset` | **Kahraman-ölçek** | WarCry + Bloodthirst; portre ister |
| CHR-18 | **Possessed birim** overlay | §4 "possess" — kontrol edilen birim vurgusu | — | Mevcut rig + seçim/aura overlay (yeni mesh değil) |

> **Çapraz-kontrol notu:** Iron Pact'in **Flanker'ı yok**, Ashen'in **Frontline'ı yok** (asimetri, §5.2). Razorbeast, Ashen'de Heavy *hat-tutucu* görevini de görür → silüeti hem "ağır" hem "canavar" okunmalı.

### 1.C Silahlar (WPN) — birim başına ana silah + kozmetik skin tabanı
| ID | Silah | Sahip | Tip |
|---|---|---|---|
| WPN-01 | Kazma (Iron) | CHR-01 | Melee/araç |
| WPN-02 | Kule kalkanı + kısa kılıç | CHR-02 | Shielded |
| WPN-03 | Gladius (+küçük kalkan) | CHR-03 | Slash |
| WPN-04 | Arbalet (crossbow) | CHR-04 | Pierce ranged |
| WPN-05 | Ateş asası / tome | CHR-05 | Fire caster |
| WPN-06 | Savaş balyozu / maul | CHR-06 | Blunt siege |
| WPN-07 | Kaba kazma (Ashen) | CHR-07 | Melee/araç |
| WPN-08 | Balta / satır | CHR-08 | Slash |
| WPN-09 | Kırbaç + tazı tasması | CHR-09 | Flank |
| WPN-10 | Sapan / bola | CHR-10 | Blunt ranged |
| WPN-11 | Zehir totemi / asası | CHR-11 | Poison caster |
| WPN-12 | Doğal silah (diş/boynuz/pençe) | CHR-12, 13, 15 | Beast |
| WPN-13 | Komutan silahları ×2 (tören-kılıç / savaş-baltası) | CHR-16, 17 | Kahraman |
| WPN-14 | Dev sopası/yumruk | CHR-14 | Summon |
| WPN-15+ | **Silah skin'leri** (kozmetik, aynı silüet) | tüm birimler ×5 kademe | §6 weapon skins |

### 1.D Zırh (ARM) — 4 zırh sınıfı, görsel okunur + 5 kozmetik kademe
| ID | Zırh sınıfı | Görsel dil | Birimler |
|---|---|---|---|
| ARM-01 | **Light** | Hafif kumaş/deri; hızlı silüet | Miner, Crossbow, Battlemage, Raider, Houndmaster, Slinger, Hexcaster |
| ARM-02 | **Shielded** | Büyük kalkan ön-cephe; kütlesel | Shieldman |
| ARM-03 | **Heavy** | Plaka/kalın; ağır silüet | Legionary, Ironclad, Razorbeast |
| ARM-04 | **Unarmored/Structure** (proviz.) | Çıplak/yapı | Summon/yapılar (faz ileri) |
| ARM-05 | Kozmetik zırh varyantları | Standard→Veteran→Elite→Legendary→Mythic | 12 birim × 5 = **60 varyant tabanı** |

### 1.E Yapılar & Heykeller (BLD/STA)
| ID | Asset | Kaynak | Durum/Adet |
|---|---|---|---|
| STA-01 | **Iron Pact Heykeli** — 4 hasar durumu | `StatuePhase`: Intact/Cracked/Breaking/Destroyed | 4 görsel durum |
| STA-02 | **Ashen Horde Heykeli** — 4 hasar durumu | aynı enum | 4 görsel durum |
| STA-03 | **Heykel kalkan fazı** overlay (×2 fraksiyon) | `StatueState.ShieldActive` | Kalkan kabuğu VFX/mesh |
| BLD-01 | **Eğitim yapısı / spawn portalı** (×2 fraksiyon) | Training/queue (§13 P1.2) — birimler bir yerden çıkar | Kamp çadırı/baraka/portal |
| BLD-02 | (Opsiyonel) Sıra/queue görsel göstergesi | TrainOrder buffer | UI'ye bağlı |

### 1.F Madenler (MIN)
| ID | Asset | Kaynak | Not |
|---|---|---|---|
| MIN-01 | **Altın maden düğümü** (nötr) | `MinePlacement` (harita başına 2–4) | Kontrol edilebilir; miner-cap |
| MIN-02 | Maden doluluk/tükenme görseli | `MineNode.Occupants/Capacity` | İşgal göstergesi (0/cap) |
| MIN-03 | Altın damarı / cevher kümesi varyantları | 3 harita farklı yerleşim | Çevre entegrasyonu |

### 1.G Çevre & Arazi Tabanı (ENV) — 3 harita
Kaynak: `map_chokepass / map_openfield / map_ridgeline.asset` (terrain + mine yerleşimleri okundu).

| ID | Asset | Harita | Not |
|---|---|---|---|
| ENV-01 | **Choke Pass** zemin/cephe | map_chokepass | Dar geçit temalı |
| ENV-02 | **Open Field** zemin/cephe | map_openfield | Açık ova temalı |
| ENV-03 | **Ridgeline** zemin/cephe | map_ridgeline | Sırt/yükselti temalı |
| ENV-04 | Parallax arka plan katmanları ×3 | §11 "temiz parallax derinlik" | Her harita 2–3 katman |
| ENV-05 | Gökyüzü/backdrop + atmosfer ×2 fraksiyon teması | §11 "fraksiyon-temalı ışık/palet" | Iron=soğuk, Ashen=sıcak |
| ENV-06 | Zemin doku seti (3 satır okunur taban) | §11 3-satır cephe | Satır ayrımı okunur |

### 1.H Arazi Özellikleri (TER) — 4 tip (TerrainKind teyitli)
Kaynak: `TerrainKind` enum + 3 haritanın terrain listesi.

| ID | Arazi | Görsel | Haritalarda kullanım (somut) |
|---|---|---|---|
| TER-01 | **HighGround (yüksek zemin)** | Tepe/ramp/sırt; +menzil/+hasar bölgesi | openfield(1), ridgeline(2×), chokepass(1) |
| TER-02 | **Choke (darboğaz)** | Dar geçit/kapı/kanyon ağzı | chokepass(1) |
| TER-03 | **Cover (siper/orman)** | Ağaç kümesi/moloz; LoS keser, −menzilli hasar | openfield(2×), ridgeline(1) |
| TER-04 | **Hazard (tehlike)** | DoT bölgesi: lav/zift/diken/zehir gölü | chokepass(2×), ridgeline(1) |

### 1.I Ağaçlar/Bitki Örtüsü (TRE) & Kayalar (ROK)
| ID | Asset | Bağlam |
|---|---|---|
| TRE-01 | Ağaç seti (cover ormanı) — birkaç varyant | TER-03 cover |
| TRE-02 | Çalı/funda/ot kümeleri | dekoratif + cover dolgu |
| TRE-03 | Zemin bitki dekoru (otlar, mantar) | parallax dolgu |
| ROK-01 | Kaya formasyonları / kayalıklar | TER-01 yüksek zemin |
| ROK-02 | Tekil kayalar / molozlar (cover) | TER-03 + dekor |
| ROK-03 | Uçurum/sırt kenarı parçaları | ridgeline siluet |

### 1.J Proplar (PRP), Köprüler, Harabeler
| ID | Asset | Bağlam |
|---|---|---|
| PRP-01 | **Bayraklar/sancaklar ×2 fraksiyon** | Cephe kimliği, kamp |
| PRP-02 | Meşaleler / ateş kapları | Atmosfer + ışık |
| PRP-03 | Bariyerler / kazıklar / çitler | Choke/savunma dekoru |
| PRP-04 | Kamp propları (çadır, sandık, fıçı, varil) | Spawn/kamp |
| PRP-05 | Kemikler / leşler / savaş enkazı | Ashen teması, zemin dekoru |
| BRG-01 | **Köprü** (choke varyantı) | Choke geçidi alternatifi |
| RUI-01 | **Harabe duvar/sütun parçaları** | Cover + dekor + dünya kimliği |
| RUI-02 | Yıkık heykel/anıt parçaları | Atmosfer |

### 1.K Mermiler (PRJ)
| ID | Mermi | Kaynak birim/büyü |
|---|---|---|
| PRJ-01 | Arbalet oku/cıvatası | CHR-04 Crossbow |
| PRJ-02 | Sapan taşı / bola | CHR-10 Slinger |
| PRJ-03 | Ateş topu / alev | CHR-05 Battlemage |
| PRJ-04 | Zehir/hex bolt | CHR-11 Hexcaster |
| PRJ-05 | Ok yağmuru okları | spell_arrowstorm |
| PRJ-06 | Yıldırım vuruşu | spell_lightningstorm |
| PRJ-07 | Mermi iz/trail + isabet decal'leri | tüm menzilli |

### 1.L VFX / FX (özet — detay Phase 10)
Saldırı (hasar-tipi başına 6), isabet/impact, **12 büyü efekti + telegraph**, **7 statü efekti** (Chilled/Burning/Poisoned/Stunned/Hasted/Raged/GoldBoost), ölümler, **komutan auraları (Rally/WarCry)**, heykel yıkımı, madencilik parıltısı, altın toplama, UI geri-bildirim. Toplam tahmini **~90–120 efekt**.

### 1.M UI Ekranları & HUD
| ID | Ekran | Kaynak |
|---|---|---|
| UI-01 | **Savaş HUD'u** (Altın sayacı, birim eğitim sırası, **3 büyü slotu**, komutan yetenek butonu, formasyon toggle) | Blueprint §10 HUD; Control katmanı |
| UI-02 | **Büyü Draft ekranı** (havuzdan 3 seç) | `DraftAndSpellInput` (§13 2.4) |
| UI-03 | Sağlık barları + heykel sağlık göstergesi | §11 okunur HUD |
| UI-04 | Ana menü | meta shell |
| UI-05 | Mod seçimi (Campaign/Endless/Ladder) | §7 modlar |
| UI-06 | **Kampanya harita ekranı** (20 seviye, yıldız) | §13 3.3 |
| UI-07 | Fraksiyon + Komutan seçim ekranı | §5.5 |
| UI-08 | **Sonuç/Ödül ekranı** (Victory/Defeat, yıldız, ganimet) | §7 reward |
| UI-09 | **Mağaza (Shop)** | §10 IAP |
| UI-10 | **Battle Pass** ikili-iz ekranı | §10 |
| UI-11 | **Sandık (Chest)** ekranı + açılış | §8 |
| UI-12 | **Armory/Yükseltme** ekranı (capped upgrades) | §13 3.2 |
| UI-13 | Ladder/sıralama ekranı | §7 async ladder |
| UI-14 | Ayarlar + erişilebilirlik (renk-körü, ölçek, reduced-motion) | §9 |
| UI-15 | Onboarding/tutorial overlay'leri | §9 |

### 1.N İkonlar (ICN), Portreler (POR), Yükleme (LOD)
| ID | Asset | Adet |
|---|---|---|
| ICN-01 | Birim ikonları | 12 |
| ICN-02 | Büyü ikonları | 12 |
| ICN-03 | Komutan ikonları | 2 |
| ICN-04 | Para birimi ikonları (Gold/Silver/Gems/PassXP) | 4 |
| ICN-05 | Yükseltme stat ikonları (Health/Damage/MoveSpeed/AttackSpeed) | 4 |
| ICN-06 | Formasyon ikonları (Line/Tight/Loose) | 3 |
| ICN-07 | Arazi ikonları (HighGround/Choke/Cover/Hazard) | 4 |
| ICN-08 | Statü efekt ikonları (7 statü) | 7 |
| ICN-09 | Sandık ikonları (Wood/Silver/Gold/Seasonal) | 4 |
| ICN-10 | Nadirlik çerçeveleri (Common→Mythic) | 5 |
| ICN-11 | Fraksiyon amblemleri/crest | 2 |
| ICN-12 | Mod ikonları + çeşitli sistem ikonları (ayarlar, sıralama, görev) | ~15 |
| POR-01 | Komutan portreleri (kahraman sanatı) | 2 |
| POR-02 | Birim portreleri (kart/armory) | 12 |
| LOD-01 | Yükleme ekranları (fraksiyon splash + kampanya act sanatı) | ~3–5 |
| LOD-02 | İpucu/tip ekranı şablonu | 1 |

### 1.O Mağaza/Sandık/Ödül 3B-2B Objeler (SHP/CHS/RWD)
| ID | Asset | Kaynak |
|---|---|---|
| CHS-01 | **Wood / Silver / Gold / Seasonal sandık** modelleri + açılış animasyonu | §8 chest mimarisi |
| CHS-02 | Sandık açılış patlama VFX'i (nadirlik-renkli) | §8 |
| RWD-01 | Ödül popup kartları (kozmetik unlock, currency burst) | §7 |
| RWD-02 | Yıldız derecelendirme (1–3) görselleri | §7 first-clear |
| RWD-03 | Victory/Defeat afiş/banner | §7 |
| SHP-01 | Mağaza öne-çıkan kozmetik vitrini + bundle sanatı | §10 shop rotation |
| SHP-02 | Gem paketi görselleri (değer merdiveni) | §10 |
| SHP-03 | Banner & emote kozmetikleri (savaş-dışı) | §6 |

### 1.P Animasyonlar (ANM — özet, detay Phase 9) & Ses (AUD)/Müzik (MUS)
- **ANM:** arketip başına idle/yürü/koş/saldırı/vuruş-irkilme/ölüm + role-özel (madenci=kaz, caster=büyü, shielded=blok, possessed=nişan al, zafer). Komutan yetenekleri, summon'lar. ~**15–22 klip × paylaşılan iskelet**.
- **AUD (SFX):** saldırı/impact (hasar-tipi başına), ölüm, 12 büyü, UI tık, madencilik, altın, heykel çatlama/yıkım, ortam. ~**150–250 SFX**.
- **MUS:** menü teması, **2 fraksiyon savaş teması**, calm→intense adaptif geçiş, victory/defeat stinger, campaign/endless/ladder, final-push. ~**8–12 parça**.
- **VO (opsiyonel, §9 "light VO barks"):** birim onay sesleri, komutan sesi, alt-yazılı.

---
## PHASE 2 — SINIFLANDIRMA

Tüm asset'ler, görevin tanımladığı **23 kategori**ye atandı. "MVP adedi" = soft-launch için zorunlu taban; varyant/kozmetik çarpanları Bölüm 12'de.

| # | Kategori | İçerik | ID'ler | MVP adet (taban) | Önem |
|---|---|---|---|---|---|
| 1 | **Characters** | 12 birim + 2 komutan + 2 summon + tazı + possess-overlay | CHR-01…18 | **~18 rig** | P0 kritik |
| 2 | **Weapons** | Birim başına ana silah + komutan + summon silahları | WPN-01…14 | **~14 ana** (+skin tabanları) | P0 |
| 3 | **Armor** | 4 zırh sınıfı görseli + 5 kozmetik kademe tabanı | ARM-01…05 | 4 sınıf (×5 kademe = 60 varyant) | P0 (sınıf), P1 (kademe) |
| 4 | **Buildings** | Eğitim yapısı/spawn ×2 + queue göstergesi | BLD-01…02 | **2** | P1 |
| 5 | **Statues** | 2 fraksiyon × 4 hasar durumu + kalkan fazı | STA-01…03 | **8 durum (+2 kalkan)** | **P0 — en kritik obje** |
| 6 | **Mines** | Altın düğümü + doluluk + cevher varyantı | MIN-01…03 | **~3** | P0 |
| 7 | **Environment** | 3 harita zemini + parallax + gökyüzü/atmosfer | ENV-01…06 | **3 harita seti** | P0 |
| 8 | **Trees** (Foliage) | Ağaç/çalı/ot setleri (cover) | TRE-01…03 | **~3 set** | P1 |
| 9 | **Rocks** | Kaya formasyonu/moloz/uçurum | ROK-01…03 | **~3 set** | P1 |
| 10 | **Terrain** | HighGround/Choke/Cover/Hazard okunur parçaları | TER-01…04 | **4 tip** | **P0 — okunabilirlik** |
| 11 | **Props** | Bayrak/meşale/bariyer/kamp/leş + köprü + harabe | PRP-01…05, BRG-01, RUI-01…02 | **~10–12** | P1–P2 |
| 12 | **FX** | Saldırı/impact/büyü/buff/debuff/ölüm/komutan/UI | FX (Phase 10) | **~90–120 efekt** | P0 (çekirdek), P1 (cila) |
| 13 | **UI** | HUD + 15 ekran | UI-01…15 | **~15 ekran** | P0 (HUD/draft/result), P1 (mağaza/pass) |
| 14 | **Icons** | Birim/büyü/komutan/currency/stat/formasyon/arazi/statü/sandık/nadirlik/amblem | ICN-01…12 | **~75 ikon** | P0 (oynanış), P1 (meta) |
| 15 | **Portraits** | Komutan + birim portreleri | POR-01…02 | **~14** | P1 |
| 16 | **Loading Screens** | Fraksiyon/act splash + tip şablonu | LOD-01…02 | **~4–6** | P2 |
| 17 | **Shops** | Mağaza vitrin + bundle + gem-paketi + banner/emote | SHP-01…03 | **~6–10** | P2 (Phase 4) |
| 18 | **Chests** | 4 sandık modeli + açılış + patlama | CHS-01…02 | **~4–6** | P2 (Phase 4) |
| 19 | **Rewards** | Ödül kartı/yıldız/victory-defeat banner | RWD-01…03 | **~6** | P1 |
| 20 | **Projectiles** | Ok/taş/ateş/zehir/yıldırım/trail/decal | PRJ-01…07 | **~7** | P0 (menzilli savaş) |
| 21 | **Animations** | Arketip iskelet klipleri + role-özel + komutan + summon | ANM (Phase 9) | **~15–22 klip** (paylaşımlı) | P0 |
| 22 | **Audio** | SFX (saldırı/impact/ölüm/büyü/UI/maden/heykel/ortam) | AUD | **~150–250 SFX** | P0 (çekirdek), P1 (zenginlik) |
| 23 | **Music** | Menü/2 fraksiyon savaş/adaptif/stinger/mod | MUS | **~8–12 parça** | P1 |

**Sınıflandırma toplamı (MVP taban, kozmetik kademe HARİÇ):** ~18 karakter rig · ~14 silah · 8 heykel durumu · 3 harita çevre seti · 4 arazi tipi · ~90–120 VFX · ~15 UI ekranı · ~75 ikon · ~7 mermi · ~15–22 anim klibi · ~150–250 SFX · ~8–12 müzik. **Kozmetik kademe çarpanı (×5) yalnızca karakter/silah/zırh/VFX-renk için uygulanır** (Bölüm 12).

> **Hiçbir kategori atlanmadı.** "Statues" ve "Mines" bilinçli olarak "Buildings"den ayrı tutuldu (oyun-kritik objeler). "Terrain" ile "Environment" ayrı: Terrain = mekanik-okunur özellik tile'ları; Environment = estetik zemin/atmosfer.

---
## PHASE 3 — ASSET BAŞINA DOKÜMANTASYON

**Kamera Mesafesi tanımları** (mobil RTS-lite tek-cephe; bu, detay/poligon bütçesini belirler):
- **UZAK** = RTS savaş kamerası. Çoğu birim burada görünür → **silüet-kritik, yüz/detay önemsiz**, düşük-poly/küçük-texture yeterli.
- **ORTA** = vurgu anları: komutan, possess edilen birim, summon dev. Biraz daha detay.
- **YAKIN** = UI/portre/mağaza/kozmetik vitrin → **tam detay** gerekir (ayrı yüksek-çözünürlük asset veya 3D'den render).
- **ARKA PLAN** = parallax çevre → düşük detay, atmosfer.

**Önem Düzeyi tanımları:** **P0** (oynanış çalışmaz) · **P1** (MVP kalitesi) · **P2** (launch/monetizasyon) · **P3** (cila).

### 3.A Karakterler (her birim ayrı)

| ID | Ad | Amaç | Oynanış Rolü | Fraksiyon | Görsel Stil | Kamera | Önem |
|---|---|---|---|---|---|---|---|
| CHR-01 | Miner (Iron) | Altın madenciliği | Ekonomi; zayıf savaş | Iron Pact | Hafif işçi; çelik/kobalt aksan; kazma | UZAK | **P0** |
| CHR-02 | Shieldman | Hattı tutar, ön-cephe denial | Frontline/Shielded tank | Iron Pact | Kütlesel kule-kalkan silüeti; ağır | UZAK | **P0** |
| CHR-03 | Legionary | Disiplinli melee DPS | Skirmisher/Heavy zırh | Iron Pact | Lejyoner; gladius; düzgün hat | UZAK | **P0** |
| CHR-04 | Crossbow | Menzilli delici DPS | Ranged/Pierce | Iron Pact | Arbalet; hafif; nişancı duruşu | UZAK | **P0** |
| CHR-05 | Battlemage | Ateş AoE/utility | Caster/Fire | Iron Pact | Asa+tome; turuncu ateş VFX | UZAK (VFX ORTA) | **P0** |
| CHR-06 | Ironclad | Kuşatma/anti-yapı | Heavy/siege | Iron Pact | En ağır silüet; dev balyoz | UZAK | **P0** |
| CHR-07 | Miner (Ashen) | Altın madenciliği | Ekonomi | Ashen Horde | Kaba işçi; kor/öküzkanı; daha çevik | UZAK | **P0** |
| CHR-08 | Raider | Ucuz hızlı melee | Skirmisher (cheap) | Ashen Horde | Çevik akıncı; balta/satır | UZAK | **P0** |
| CHR-09 | Houndmaster | Hızlı kanat baskını | **Flanker** | Ashen Horde | Kırbaç; **tazılarla** okunur | UZAK | **P0** |
| CHR-10 | Slinger | Menzilli künt DPS | Ranged/Blunt | Ashen Horde | Sapan/bola; hafif | UZAK | **P0** |
| CHR-11 | Hexcaster | Zehir debuff/utility | Caster/Poison | Ashen Horde | Totem/asa; zehir-yeşili VFX | UZAK (VFX ORTA) | **P0** |
| CHR-12 | Razorbeast | Hat-tutar + kuşatma | Heavy/mobile beast | Ashen Horde | **Canavar** silüeti; doğal silah | UZAK/ORTA | **P0** |
| CHR-13 | War Hound | Houndmaster refakatçisi | Flank baskı (sürü) | Ashen Horde | Küçük dört-ayaklı; hızlı | UZAK | **P0** (Houndmaster'a bağlı) |
| CHR-14 | Giant (summon) | Güçlü geçici birim | Summon (offensive) | Nötr/büyü | Dev-ölçek; ağır basış | ORTA | P1 |
| CHR-15 | Pouncer (summon) | Hızlı geçici yırtıcı | Summon (skirmish) | Nötr/büyü | Sıçrayan yırtıcı | UZAK | P1 |
| CHR-16 | Iron Warden (Komutan) | Fraksiyon kimliği + Rally | Force-multiplier (≤%12 güç) | Iron Pact | Kahraman-ölçek; tören zırhı; sancak | ORTA + YAKIN(portre) | **P0/P1** |
| CHR-17 | Ashen Warchief (Komutan) | Fraksiyon kimliği + WarCry | Force-multiplier (≤%13 güç) | Ashen Horde | Kahraman-ölçek; kabile/kemik motifi | ORTA + YAKIN(portre) | **P0/P1** |
| CHR-18 | Possess overlay | Kontrol edilen birim vurgusu | Agency hook (§4) | Her ikisi | Seçim halkası/aura; yeni mesh değil | UZAK/ORTA | P1 |

### 3.B Silahlar, Zırh, Yapılar, Heykeller, Madenler

| ID | Ad | Amaç | Oynanış Rolü | Fraksiyon | Görsel Stil | Kamera | Önem |
|---|---|---|---|---|---|---|---|
| WPN-01…14 | Birim/komutan silahları | Saldırı kimliği + hasar-tipi okunması | Silüet + hasar-tipi ipucu | İlgili | Hasar-tipine göre okunur (delici/künt/ateş/zehir) | UZAK (skin YAKIN) | P0 (ana), P1 (skin) |
| ARM-01 | Light zırh dili | Hafif birim okunması | Hız/kırılganlık ipucu | Her ikisi | Kumaş/deri | UZAK | P0 |
| ARM-02 | Shielded dili | Ön-cephe denial okunması | Tank ipucu | Iron Pact | Büyük kalkan kütlesi | UZAK | P0 |
| ARM-03 | Heavy dili | Ağır birim okunması | Dayanıklılık ipucu | Her ikisi | Plaka/kalın | UZAK | P0 |
| BLD-01 | Eğitim yapısı/spawn ×2 | Birimlerin doğduğu nokta | Üretim kaynağı | Her fraksiyon | Kamp/baraka/portal; fraksiyon temalı | UZAK/ARKA | P1 |
| STA-01/02 | Fraksiyon Heykeli (Intact) | **Kazan/kaybet objesi** | Ana hedef | Her fraksiyon | İkonik anıt; fraksiyon kimliği | UZAK (büyük) | **P0** |
| STA-01/02 | Heykel: Cracked/Breaking/Destroyed | Okunur doruk/hasar geri-bildirimi | Durum okunması | Her fraksiyon | Artan yıkım; çatlak/moloz/duman | UZAK | **P0** |
| STA-03 | Heykel kalkan fazı | Kalkan emer (shield) | Faz göstergesi | Her fraksiyon | Yarı-saydam kabuk + kırılma VFX | UZAK | **P0** |
| MIN-01 | Altın maden düğümü | Ekonomi düğümü | Kontrol edilebilir kaynak | Nötr | Altın damarı/cevher; parıltı | UZAK | **P0** |
| MIN-02 | Doluluk göstergesi | Miner-cap okunması | Spatial contest | Nötr | İşgal/doluluk vurgusu | UZAK | P1 |

### 3.C Çevre, Arazi, Foliage, Kaya, Prop, Köprü, Harabe

| ID | Ad | Amaç | Oynanış Rolü | Fraksiyon | Görsel Stil | Kamera | Önem |
|---|---|---|---|---|---|---|---|
| ENV-01/02/03 | 3 harita zemini | Savaş alanı tabanı | Cephe okunması (3 satır) | Nötr | Temiz stilize; map-teması | UZAK/ARKA | **P0** |
| ENV-04 | Parallax katmanları | Derinlik | Estetik | Nötr | Çok-katman; düşük detay | ARKA PLAN | P1 |
| ENV-05 | Gökyüzü/atmosfer ×2 | Fraksiyon ruh hali | Estetik+okunabilirlik | Her ikisi | Iron=soğuk mavi, Ashen=sıcak kızıl | ARKA PLAN | P1 |
| TER-01 | HighGround | +menzil/+hasar bölgesi | **Mekanik-okunur** | Nötr | Tepe/ramp; net kenar | UZAK | **P0** |
| TER-02 | Choke | AoE değer artışı | **Mekanik-okunur** | Nötr | Dar geçit/kapı | UZAK | **P0** |
| TER-03 | Cover/orman | −menzilli hasar, LoS keser | **Mekanik-okunur** | Nötr | Ağaç/moloz kümesi | UZAK | **P0** |
| TER-04 | Hazard | DoT bölgesi | **Mekanik-okunur** | Nötr | Lav/zift/zehir; tehlike-renk | UZAK | **P0** |
| TRE-01…03 | Ağaç/çalı/ot | Cover dolgu + dekor | Cover'ı somutlar | Nötr | Stilize; düşük-poly | UZAK/ARKA | P1 |
| ROK-01…03 | Kaya/moloz/uçurum | HighGround/cover somutlar | Arazi okunması | Nötr | Stilize kaya | UZAK/ARKA | P1 |
| PRP-01 | Bayraklar ×2 | Fraksiyon kimliği | Estetik+kimlik | Her ikisi | Crest + palet | UZAK/ARKA | P1 |
| PRP-02…05 | Meşale/bariyer/kamp/leş | Atmosfer + dünya | Dekor | Tema | Stilize | ARKA PLAN | P2 |
| BRG-01 | Köprü | Choke varyantı | Geçiş darboğazı | Nötr | Map-teması | UZAK | P2 |
| RUI-01/02 | Harabe duvar/sütun | Cover + dünya kimliği | Cover/dekor | Nötr | Yıkık taş | UZAK/ARKA | P2 |

### 3.D Mermiler, VFX, Animasyon (özet — detay Phase 9/10)

| ID | Ad | Amaç | Oynanış Rolü | Görsel Stil | Kamera | Önem |
|---|---|---|---|---|---|---|
| PRJ-01…06 | Ok/cıvata/taş/ateş/zehir/yıldırım | Menzilli saldırı görseli | İsabet okunması | Hasar-tipi renkli | UZAK | **P0** |
| FX (genel) | Saldırı/impact/ölüm/statü/büyü/komutan | Geri-bildirim + okunabilirlik | **Telegraph/counter okunması** | Renk-kodlu, mobil-bütçeli | UZAK | **P0** çekirdek |
| ANM (genel) | Idle/yürü/saldırı/ölüm/role-özel | Birim canlılığı + telegraph | Animasyon-zamanlaması KİLİTLİ (§6) | Spine/Mecanim | UZAK | **P0** |

### 3.E UI, İkon, Portre, Yükleme, Mağaza, Sandık, Ödül, Ses, Müzik

| ID | Ad | Amaç | Oynanış Rolü | Görsel Stil | Kamera | Önem |
|---|---|---|---|---|---|---|
| UI-01 | Savaş HUD'u | Altın/sıra/3 büyü/komutan/formasyon | **Çekirdek kontrol** | Minimal, başparmak-erişimli, yüksek-kontrast | YAKIN | **P0** |
| UI-02 | Büyü Draft | 3 büyü seç | Pre-battle loadout | Kart-tabanlı | YAKIN | **P0** |
| UI-06 | Kampanya harita | 20 seviye ilerleme | PvE iskele | Harita + yıldız | YAKIN | P1 |
| UI-08 | Sonuç/Ödül | Victory/Defeat + ganimet | Döngü kapanışı | Banner + kart | YAKIN | **P0** |
| UI-09…11 | Mağaza/Pass/Sandık | Monetizasyon | Gelir (Phase 4) | Vitrin/iz/sandık | YAKIN | P2 |
| UI-12 | Armory/Yükseltme | Capped upgrades | Progression | Liste + stat | YAKIN | P1 |
| ICN-01…12 | Tüm ikonlar (~75) | Tanıma/okunabilirlik | Her sistem | Tutarlı ikon dili | YAKIN | P0 (oynanış), P1 (meta) |
| POR-01/02 | Portreler (~14) | Kimlik/koleksiyon | Kozmetik+kimlik | Yüksek-detay sanat | YAKIN | P1 |
| LOD-01/02 | Yükleme ekranları | Bekleme + dünya | Atmosfer | Splash sanatı | YAKIN | P2 |
| CHS-01/02 | Sandıklar | Ödül pacing | Free-track loop | 4 kademe model | YAKIN | P2 |
| RWD-01…03 | Ödül kartı/yıldız/banner | Tatmin geri-bildirimi | Reward feel | Parlak/celebratory | YAKIN | P1 |
| SHP-01…03 | Mağaza vitrin/bundle/gem | Satış | Gelir | Şeffaf değer | YAKIN | P2 |
| AUD | SFX seti (~150–250) | İşitsel geri-bildirim | Combat feel + okunabilirlik | Punchy, fraksiyon-renkli | — | P0 (çekirdek) |
| MUS | Müzik (~8–12) | Ruh hali/adaptif | Retention/atmosfer | Fraksiyon motifi; calm→intense | — | P1 |

---
## PHASE 4 — BUILD / BUY / KITBASH / GENERATE KARARI

**Karar çerçevesi (kanona dayalı):**
- **BUY** = hazır asset al → *içerik hızı + maliyet* (Blueprint §13 "kozmetik/overflow dışarıdan"; §16 risk 4). Jenerik, düşük-kimlik, bol-bulunur objeler için.
- **KITBASH** = satın alınan parçaları birleştir/modifiye et → *paylaşımlı iskelet + fraksiyon reskin* kanonu (§10 content strategy). Fraksiyon varyantları, çevre montajı için.
- **BUILD** = sıfırdan bespoke → *IP-kritik, en yüksek görünürlük, okunabilirlik-tanımlayıcı* objeler (heykel, marka UI, fraksiyon crest, müzik kimliği).
- **GENERATE** = araç/prosedürel/şablon/AI → *kozmetik recolor/material kademeleri* (§6: kozmetik yalnızca palet/material/trim/VFX-renk) + trail/decal shader + bazı arka-plan.

> **Yol bağımlılığı (KB-1):** Karakter/çevre için BUY/KITBASH'ın *ne aldığı*, 2D-Spine mı 2.5D-3D mi seçimine bağlıdır (Bölüm 5). Aşağıdaki kararlar **strateji** olarak yoldan bağımsızdır; *kaynak* Bölüm 5'te ayrışır.

| Asset sınıfı | Karar | Neden (kanon-bağlantılı) |
|---|---|---|
| **Birimler (12 arketip)** | **BUY taban + KITBASH fraksiyon** | Paylaşımlı arketip iskeleti + fraksiyon reskin **kanon içerik-hızı motoru** (§10). Sıfırdan 12 rig, indie bütçeyi yer (Blueprint §13). Taban modüler karakter paketi al, 2 fraksiyona kitbash et. |
| **Komutanlar (2)** | **KITBASH + BUILD aksan** | Kahraman kimliği gerek (§6) ama yalnızca 2 adet → taban birimden kitbash + bespoke detay (sancak, tören zırhı) ekle. Portre = BUILD. |
| **Canavarlar (Razorbeast, Hound, Giant, Pouncer)** | **BUY + KITBASH** | Yaratık paketleri bol; sıfırdan canavar pahalı. Al, fraksiyon paletine boya. |
| **Possess overlay** | **GENERATE** (shader/decal) | Yeni mesh değil; seçim halkası/aura = shader. |
| **Silahlar (ana ×14)** | **BUY + KITBASH** | Modüler silah paketleri ucuz; silüet-taşıyıcı ama jenerik. Hasar-tipi okunması için kitbash. |
| **Silah/zırh kozmetik kademeleri (×5)** | **GENERATE** (material/recolor) | §6: kozmetik = recolor/material/trim. Şablon-tabanlı material varyant üretimi → düşük maliyet, yüksek hız. |
| **Zırh sınıfı dili (4)** | **BUY + KITBASH** | Modüler zırh parçaları; silüet okunması için kitbash. |
| **Heykel (2 fraksiyon × 4 durum + kalkan)** | **BUILD** (bespoke) | **En ikonik, en görünür, oyunun amacı** (§11 "ikonik objektif"). Jenerik asset olamaz — IP kimliği. Hasar durumları authored; kırılma **GENERATE** (prosedürel fracture) ile hızlandırılabilir. |
| **Eğitim yapısı/spawn (2)** | **BUY + KITBASH** | Kamp/baraka jenerik; fraksiyon temasına boya. |
| **Altın maden düğümü** | **BUY + KITBASH** | Kaya/cevher propu bol; ucuz. |
| **Çevre zemini (3 harita)** | **BUY modüler kit + KITBASH** | Modüler environment kit (Synty/Quaternius/Kenney) → 3 haritayı montajla. **En yüksek BUY kaldıracı** (Blueprint §13). |
| **Arazi özellik tile'ları (4: HighGround/Choke/Cover/Hazard)** | **BUILD/KITBASH + shader** | **Mekanik-okunur** olmalı (§11 İHLAL EDİLEMEZ). Satın alınan zemine net okunur kenar/decal/shader (yüksek-zemin highlight, hazard glow) BUILD edilir. |
| **Ağaç/kaya/bitki** | **BUY** | Doğa paketleri en bol/ucuz asset sınıfı. |
| **Proplar (meşale/bariyer/kamp/leş)** | **BUY** | Jenerik dekor; bol. |
| **Bayraklar/crest (2 fraksiyon)** | **BUILD** | Fraksiyon kimliği = marka; bespoke crest. |
| **Köprü/harabe** | **BUY + KITBASH** | Jenerik; modüler kitten. |
| **Mermiler (7)** | **BUY VFX + GENERATE trail** | VFX paketlerinde mevcut; trail/iz = shader generate. |
| **VFX (~90–120)** | **BUY taban + KITBASH recolor** | VFX paketleri büyük zaman-tasarrufu; hasar-tipi renk-kodu (§10) için recolor kitbash. Telegraph okunması BUILD ayarı. |
| **UI çerçeve/panel/buton** | **BUY UI kit + KITBASH** | Kenney/UI kitleri hızlandırır; markaya kitbash. |
| **İkonlar (~75)** | **BUILD/GENERATE** | Tutarlı ikon dili = marka + okunabilirlik; şablon-tabanlı üretim. |
| **Portreler (~14)** | **BUILD** (veya Generate-taban + paintover) | Kahraman/koleksiyon kimliği; yüksek-değer sanat. |
| **Yükleme/key art** | **BUILD/GENERATE** | Marka; AI-taban + paintover kabul edilebilir (oynanış-dışı). |
| **Sandık/mağaza objeleri** | **BUY + KITBASH** | Jenerik; Phase 4, sonra. |
| **Animasyonlar** | **BUY/RETARGET + BUILD telegraph** | Anim kütüphanesi (Mixamo/Synty) retarget; **telegraph/counter klipleri** (§5.3 İHLAL EDİLEMEZ zamanlama) custom BUILD. |
| **SFX (~150–250)** | **BUY** (kütüphane) | Ses kütüphaneleri ucuz/bol. |
| **Müzik (~8–12)** | **BUILD** (besteci/outsource) | Fraksiyon motifi + adaptif (§10) = kimlik; bespoke veya lisanslı-özel. |

**Özet dağılım (asset-sınıf bazında):** BUY/KITBASH ağırlıklı (çevre, birim-taban, prop, VFX, SFX) → indie-feasible. **BUILD yalnızca IP-kritik** (heykel, crest, UI-dili, müzik, portre, telegraph anim). **GENERATE = kozmetik kademe çarpanı** (§6 gelir motoru, düşük maliyet). Bu dağılım, Blueprint §13'ün "çekirdek in-house, kozmetik/overflow dışarıdan" ve §16'nın içerik-hızı riskine doğrudan yanıt verir.

---
## PHASE 5 — TEDARİK STRATEJİSİ

### 5.0 Önce karar verilmesi gereken: 2D-Spine (kanon) mı, 2.5D düşük-poligon 3D (önerilen) mi? — KB-1

Görevin saydığı kaynakların çoğu (**Synty, POLYGON, Quaternius**) **3D düşük-poligon**dur. Roadmap kanonu ise **2D Spine** der. İki yol:

| Boyut | **Yol A — 2D Spine (kanonik)** | **Yol B — 2.5D düşük-poligon 3D (ÖNERİLEN, ADR-gerektirir)** |
|---|---|---|
| Kanon uyumu | Doğrudan ([PRESERVE] "Spine 2D readability") | **ADR gerekir** (§15/§16; "Spine 2D" dispozisyonunu değiştirir — ihlal-edilemez değil ama kanon) |
| Synty/POLYGON/Quaternius uyumu | **DÜŞÜK** (3D paketler doğrudan kullanılamaz) | **YÜKSEK** (tam uyum — sebebi bu kaynakların seçilmesi) |
| İçerik hızı (§16 risk 4) | Spine reskin ucuz ama her arketip elde-çizim/rig | **Çok yüksek** — modüler kitbash + material recolor |
| Animasyon | Spine iskeletsel (elde) | **Mixamo/Synty retarget** (Humanoid avatar) → ucuz |
| ECS/DOTS render | 2D sprite (Entities Graphics 2D sınırlı) | **Entities Graphics + GPU instancing** doğal (yüzlerce birim) |
| Mobil perf | Çok iyi (2D) | İyi (düşük-poly + LOD + instancing) |
| Okunabilirlik (§6/§11) | Mükemmel (2D silüet) | İyi — **ortografik/eğik kamera + güçlü silüet** ile 2D-gibi okunur |
| Kozmetik kademe (§6) | Spine skin (recolor) | **Material/texture swap** (5 kademe kolay) |

**Önerim (Lead Tech Artist + Art Director):** **Yol B — 2.5D düşük-poligon 3D, ortografik/eğik kamerayla.** Gerekçe: (1) görevin tedarik kaynakları zaten 3D; (2) modern mobil RTS-lite halefleri (içerik-hızı baskısı altında) bunu yapar; (3) Synty/Quaternius **paylaşımlı modüler iskelet + fraksiyon reskin** kanonunu (§10) doğrudan, ucuza karşılar; (4) ECS/DOTS + GPU instancing yüzlerce birimi sever; (5) okunabilirlik, ortografik kamera + silüet-disiplini ile korunur. **AMA bu, GD/TA onaylı bir ADR ister** (§16) — kanon "Spine 2D" der, ajan tek başına değiştiremez (§15.6). **Bu denetimin Bölüm 5–11 tedarik/entegrasyon önerileri Yol B'ye göredir; Yol A kalırsa kaynaklar 2D'ye kayar (her tabloda not düşüldü).**

> Eğer **Yol A (2D Spine)** seçilirse: kaynaklar **Unity Asset Store 2D**, **Kenney 2D**, **GameDev Market**, **itch.io**, **OpenGameArt 2D sprite**, Spine-hazır karakter paketleri olur; Synty/POLYGON/Quaternius **uygulanamaz** (yalnızca konsept/referans). Animasyon = Spine elde-rig.

### 5.1 Kaynak Uyumluluk Matrisi (genel)

| Kaynak | Tip | Stil-uyumu (BULWARK) | Lisans | Mobil | İçerik-hızı | En iyi olduğu sınıf | Risk/Not |
|---|---|---|---|---|---|---|---|
| **Synty Studios (POLYGON)** | 3D low-poly | ★★★★★ | Asset Store EULA (royalty-free, telifsiz oyun-içi) | ★★★★☆ | ★★★★★ | Karakter, çevre, prop, silah, VFX | Tek-tutarlı-stil → **omurga kaynağı**. Stil "Synty" diye tanınır (farklılaşma için kitbash+recolor). |
| **POLYGON (= Synty hattı)** | 3D low-poly | ★★★★★ | (Synty ile aynı) | ★★★★☆ | ★★★★★ | (yukarıyla aynı) | "Polygon" = Synty'nin POLYGON serisidir; ayrı satıcı değil. |
| **Quaternius** | 3D low-poly | ★★★★☆ | **CC0 (ücretsiz)** | ★★★★★ | ★★★★☆ | Karakter, canavar, doğa, silah | **Bütçe/ücretsiz kahraman.** "Ultimate" serisi rigli+animli. Kalite Synty'nin biraz altı ama CC0. |
| **Kenney** | 2D + 3D + UI + Audio | ★★★☆☆ (3D), ★★★★☆ (UI/proto) | **CC0 (ücretsiz)** | ★★★★★ | ★★★☆☆ | **UI, ikon, prototip, ses, particle** | Düşük-fidelity ama dev breadth. UI/ikon/SFX için **bütçe omurgası**. |
| **OpenGameArt (OGA)** | 2D/3D/Audio | ★★☆☆☆ (karışık) | **Karışık** (CC0/CC-BY/GPL) | ★★★★☆ | ★★☆☆☆ | SFX, müzik, 2D sprite, placeholder | **Lisans-diligence şart** (atıf/GPL tuzakları). Tutarsız kalite. |
| **CGTrader** | 3D marketplace | ★★★☆☆ (model-bazlı) | **Per-asset** (royalty-free / editorial) | değişken | ★★☆☆☆ | Spesifik kahraman/canavar/prop | Stil tutarlılığı yok → tekil özel modeller; **lisans+poly-bütçe kontrolü**. |
| **Sketchfab** | 3D marketplace+free | ★★★☆☆ | **Karışık** (CC/Store) | değişken | ★★☆☆☆ | Spesifik prop/canavar/referans | glTF/FBX import kolay; **CC atıf/NC kontrolü**, poly-temizlik gerek. |
| **Unity Asset Store (genel)** | 2D/3D/VFX/UI/Audio | ★★★★☆ | Store EULA | ★★★★☆ | ★★★★☆ | VFX, UI, anim, audio, environment | Synty de buradadır; **VFX/anim/UI için ana pazar**. |
| **Mixamo** (ek) | 3D anim+rig | ★★★★☆ | Ücretsiz (Adobe) | ★★★★★ | ★★★★★ | **Animasyon retarget** (Humanoid) | Yol B'de anim omurgası; insansı rig şart. |

### 5.2 Asset sınıfı başına Tedarik — En İyi / Yedek / Bütçe (Yol B: 2.5D)

| Asset sınıfı | **En İyi (Best)** | **Yedek (Fallback)** | **Bütçe (Budget/Free)** |
|---|---|---|---|
| **Birimler (12)** | **Synty POLYGON** Fantasy/Knights/Vikings **modüler karakter** (fraksiyon kitbash + material recolor) | CGTrader/Sketchfab stilize karakter paketi (per-model) | **Quaternius Ultimate Modular Characters** (CC0) + Mixamo anim |
| **Komutanlar (2)** | Synty modüler taban + **bespoke aksan (BUILD)** | CGTrader kahraman model + kitbash | Quaternius + elde detay |
| **Canavarlar (Razorbeast/Hound/Giant/Pouncer)** | **Synty POLYGON** (yaratık var) / kitbash | Sketchfab/CGTrader creature | **Quaternius Ultimate Monsters** (CC0) |
| **Silahlar (14)** | **Synty POLYGON** silah paketleri (stil eşi) | CGTrader modüler silah | Quaternius / Kenney |
| **Zırh sınıfı + kademe** | Synty modüler zırh + **material varyant (GENERATE)** | CGTrader zırh | Quaternius + recolor |
| **Heykel (8 durum)** | **BUILD bespoke** (komisyon/in-house) + prosedürel fracture | Synty heykel/harabe propundan kitbash | Sketchfab CC heykel + ağır modifikasyon |
| **Eğitim yapısı/spawn (2)** | **Synty POLYGON** kamp/yapı | CGTrader | Quaternius/Kenney |
| **Altın maden** | Synty kaya/maden propu | Sketchfab/CGTrader | Quaternius/Kenney rock |
| **Çevre/zemin (3 harita)** | **Synty POLYGON Nature / Fantasy Kingdom / Dungeon** (modüler montaj) | Unity Asset Store environment kit | **Quaternius Ultimate Nature + Kenney Nature Kit** (CC0) |
| **Arazi tile (4 okunur)** | **BUILD/KITBASH + shader** (yüksek-zemin highlight, hazard glow) | Asset Store terrain decal | Kenney + custom shader |
| **Ağaç/kaya/bitki** | **Synty POLYGON Nature** | Asset Store nature | **Quaternius/Kenney Nature** (CC0) |
| **Proplar/köprü/harabe** | **Synty POLYGON** (Dungeon/Kingdom) | CGTrader/Sketchfab | Kenney/Quaternius |
| **Bayrak/crest (2)** | **BUILD** (fraksiyon marka) | kitbash + custom texture | custom 2D texture |
| **Mermiler (7)** | **JMO Cartoon FX / War FX** (Asset Store) + Synty Particle | Diğer Asset Store VFX | **Kenney Particle Pack** + VFX Graph |
| **VFX (~90–120)** | **JMO "Cartoon FX Remaster" + "War FX"** (Asset Store, mobil-dostu) + **Synty POLYGON Particle FX** | Asset Store stilize VFX paketleri | Kenney Particle + Unity VFX Graph + OGA |
| **UI kit/çerçeve** | Asset Store **fantasy/RTS UI kit** + markaya kitbash | GameDev Market UI | **Kenney UI Pack** (CC0) |
| **İkonlar (~75)** | **BUILD/GENERATE** tutarlı ikon dili | Asset Store ikon seti | **Kenney Game Icons** (CC0) + OGA |
| **Portreler (~14)** | **BUILD** (komisyon kahraman sanatı) | Asset Store portre + paintover | AI-taban + paintover (oynanış-dışı) |
| **Yükleme/key art** | **BUILD/komisyon** | Asset Store key-art | AI-taban + paintover |
| **Sandık/mağaza obj** | Synty/Asset Store sandık-prop | CGTrader/Sketchfab | Kenney/Quaternius |
| **Animasyonlar** | **Synty anim paketleri** (rig eşi) + custom telegraph | Asset Store anim seti | **Mixamo (ücretsiz)** + Quaternius rigli |
| **SFX (~150–250)** | Asset Store / **Sonniss GDC bundle** / ticari kütüphane | A Sound Effect / Soundsnap | **Kenney Audio + freesound(CC0) + OGA** |
| **Müzik (~8–12)** | **Besteci komisyonu** (fraksiyon motifi + adaptif) | Asset Store RTS müzik paketi | OGA CC0 + Kevin MacLeod (CC-BY) |

### 5.3 Tedarik notları (önemli uyarılar)
- **Synty stil-tanınırlığı:** Synty'nin görünümü tanınır. Farklılaşma için **fraksiyon-renk recolor + kitbash + custom heykel/crest/UI** şart (marka ihlal-edilemez kimliğini Synty'ye kaptırma).
- **Lisans diligence (zorunlu):** OGA/Sketchfab/CGTrader **karışık lisans** → her asset için ticari-kullanım + atıf + (varsa) GPL/NC kontrolü; bir kayıt tablosu tutulmalı. Synty/Quaternius/Kenney temizdir (Store EULA / CC0).
- **Mobil poly/texture kontrolü:** CGTrader/Sketchfab modelleri sık sık yüksek-poly → **mobil için decimate + texture atlas + LOD** (Bölüm 11).
- **Okunabilirlik testi (KB-5):** her satın alınan karakter **RTS-uzak-kamerada silüet testinden** geçmeli; geçmezse silüet düzenleme/seç (kitbash).
- **Kozmetik-güvenlik (§6):** satın alınan taban, **silüet kilidini** desteklemeli; kademeler yalnızca material/recolor (yeni silüet değil).

---
## PHASE 6 — KARAKTER GÖRSEL KİMLİĞİ

**Bağlayıcı kurallar (§6 İHLAL EDİLEMEZ, Art/README):**
- **Silüet kilidi:** her arketibin **tek kanonik silüeti** vardır — rakibin cephe boyunca okuduğu şey budur. Kozmetik bunu ASLA değiştiremez.
- **RTS-uzak-kamera testi:** her karakter, küçük ölçekte + kalabalıkta **yalnız silüetinden** tanınmalı (renk-körü modunda bile).
- **Fraksiyon-renk kimliği:** Iron Pact = **çelik+kobalt**, Ashen = **kor+öküzkanı** — her zaman anında ayırt edilir.
- **Kozmetik kademeler (5):** Standard → Veteran → Elite → Legendary → Mythic = yalnızca **palet/material/trim/particle-renk/idle-zafer flourish**. Nadirlik (Common→Mythic) = prestij, **avantaj yok**. Ranked "clarity mode" standart okunur skin'e zorlar.

### 6.1 Silüet ailesi (arketip okuması — fraksiyonlar paylaşır, reskin ayrışır)
| Arketip | Silüet okuması (RTS-uzak) | Ayırt-edici işaret |
|---|---|---|
| Miner | Küçük, eğik, **alet taşır** | Savaşçı-değil okuması; kazma |
| Frontline (Shieldman) | **Geniş, kalkan-baskın "duvar"** | Büyük ön-cephe kalkanı |
| Skirmisher (Legionary/Raider) | Orta, çevik, **silah-önde** | Kılıç/balta hamlesi |
| Ranged (Crossbow/Slinger) | Orta, **menzilli-silah gövde-arkası**, nişan duruşu | Arbalet/sapan profili |
| Caster (Battlemage/Hexcaster) | **Asa/totem yukarıda**, cüppeli, parlak aksan | Büyü-glow (ateş/zehir) |
| Heavy (Ironclad/Razorbeast) | **En büyük kütle**, hantal | Dev silah / canavar gövdesi |
| Flanker (Houndmaster) | İnce + **tazılarla** çevrili | Sürü-refakat okuması |

### 6.2 Iron Pact karakterleri (çelik + kobalt; disiplin/plaka)

| Birim | Silüet | Renk dili | Zırh sınıfı | Kozmetik kademe yönü |
|---|---|---|---|---|
| **Miner** | Küçük, eğik; bir elde kazma, sırtta çuval | Çelik-gri + sönük kobalt; düşük-doygunluk | **Light** | Std: sade çelik → Mythic: parlatılmış kobalt + altın-toz parıltı (idle'da kazma kıvılcımı) |
| **Shieldman** | **Geniş kule-kalkan duvarı**; alçak duruş | Ağır çelik + kobalt hanedan | **Shielded** | Trim + kalkan-amblemi zenginleşir; Mythic: enerji-hat kalkan kenarı (renk-only) |
| **Legionary** | Orta, dik; gladius + küçük yuvarlak kalkan | Çelik plaka + kobalt pelerin aksanı | **Heavy** | Pelerin/tüy zenginleşir; Mythic: metalik-mavi parıltı |
| **Crossbow** | Orta, **arbalet gövde-önünde**; hafif | Hafif çelik + kobalt kumaş | **Light** | Kumaş/kapüşon trim; Mythic: kobalt-cıvata izi (PRJ recolor) |
| **Battlemage** | Cüppeli, **asa+tome yukarıda**; turuncu ateş-glow | Kobalt cüppe + **turuncu ateş VFX** | **Light** | VFX-renk yoğunlaşır (turuncu→beyaz-sıcak); cüppe-trim |
| **Ironclad** | **En büyük IP kütlesi**; dev balyoz | Koyu çelik + kobalt; ağır plaka | **Heavy** | Plaka-gravür + Mythic: akkor-mavi balyoz-glow (renk-only) |

### 6.3 Ashen Horde karakterleri (kor + öküzkanı; ham/kemik/kürk)

| Birim | Silüet | Renk dili | Zırh sınıfı | Kozmetik kademe yönü |
|---|---|---|---|---|
| **Miner** | Küçük, çevik; kaba kazma | Öküzkanı + kor aksan; toprak-tonu | **Light** | Std: paçavra → Mythic: kor-damarlı paçavra + kül parıltı |
| **Raider** | Çevik, **balta-önde**; düşük zırh | Öküzkanı deri + kor savaş-boyası | **Light** | Savaş-boyası deseni + Mythic: kor-glow balta izi |
| **Houndmaster** | İnce + **tazı sürüsü** (refakat okuması) | Kor deri + öküzkanı; tazılar koyu | **Light** | Tazı tasma/boya kademelenir; Mythic: kor-gözlü tazılar (renk-only) |
| **Slinger** | Orta, **sapan dönüşü**; hafif | Toprak + kor kumaş | **Light** | Sapan/bola süsü; Mythic: kor-iz künt mermi |
| **Hexcaster** | Cüppeli/eğik, **totem yukarıda**; zehir-glow | Öküzkanı cüppe + **zehir-yeşili VFX** | **Light** | VFX-renk (yeşil→asit-parlak); totem-tüy/kemik trim |
| **Razorbeast** | **Canavar kütlesi**; dört-ayak/binek; boynuz/diken | Koyu öküzkanı post + kor diken | **Heavy** | Post-deseni + Mythic: kor-çatlak deri (renk-only) |

### 6.4 Komutanlar (kahraman-ölçek; kimlik ≤%12–13 güç, §6)

| Komutan | Silüet | Renk dili | Kimlik işareti | Kozmetik (skin/VFX/ses — §6 sınırı) |
|---|---|---|---|---|
| **Iron Warden** (Rally + Quartermaster) | Kahraman-ölçek; **dik, tören-plaka, sancak** | Parlatılmış çelik + zengin kobalt + altın hanedan | Rally aurası = kobalt disiplin-halkası | Skin/VFX/ses-only; ranked normalize (talent capped) |
| **Ashen Warchief** (WarCry + Bloodthirst) | Kahraman-ölçek; **eğik-saldırgan, kemik-taç, kürk** | Öküzkanı + kor + kemik-beyazı | WarCry aurası = kor öfke-dalgası | Skin/VFX/ses-only; ranked normalize |

> Komutan **portreleri** (POR-01) YAKIN-kamera, tam-detay sanat — savaş mesh'inden ayrı, yüksek-çözünürlük.

### 6.5 Yaratıklar & Summon'lar
| Asset | Silüet | Renk | Not |
|---|---|---|---|
| **War Hound** | Küçük hızlı dört-ayaklı; sürü | Ashen koyu + kor-göz aksan | Houndmaster okumasının parçası |
| **Giant (summon)** | **Dev-ölçek**; ağır basış; nötr-büyü teması | Büyü-nötr + çağıran-fraksiyon aksan | ORTA kamera; nadir görülür |
| **Pouncer (summon)** | Sıçrayan yırtıcı; orta | Büyü-nötr aksan | Hızlı, ince |

### 6.6 Kozmetik kademe sistemi (12 birim × 5 = 60 varyant tabanı + komutan skinleri)
| Kademe | Görsel zenginlik (silüet SABİT) | Tipik kaynak |
|---|---|---|
| **Standard** | Taban fraksiyon-renk, sade material | İlk skin = taban asset |
| **Veteran** | Trim + ikincil renk + hafif material | GENERATE (material varyant) |
| **Elite** | Metalik/kumaş upgrade + küçük particle aksan | GENERATE + küçük VFX |
| **Legendary** | Zengin material + idle/zafer flourish + VFX-renk | GENERATE + custom flourish anim |
| **Mythic** | En zengin material + tam VFX-renk teması (silüet hâlâ kilitli) | GENERATE + bespoke VFX-renk |

**Kritik:** Tüm 60 varyant **GENERATE** (material/recolor şablonu) ile üretilir — yeni mesh/silüet YOK. Bu, §6 gelir motorunu §16 içerik-hızı bütçesi içinde tutar.

---
## PHASE 7 — FRAKSİYON GÖRSEL REHBERİ

İki **asimetrik** fraksiyon (§5.1). Görsel dil, *doktrini* anında okutmalı: Iron Pact = disiplin/duvar/dayanıklılık; Ashen = hız/sürü/saldırganlık.

### 7.1 THE IRON PACT — disiplinli lejyon (çelik + kobalt)

| Eksen | Görsel yönerge |
|---|---|
| **Fantezi** | Kırılmaz kalkan-duvarı ordusu; savaş = yıpratma + disiplin. "Düzen, metal, sebat." |
| **Mimari** | Düzgün, simetrik, taş+çelik; **kale/garnizon** estetiği. Spawn = düzenli **garnizon barakası/kapı**. Dik açılar, hanedan flamalar, demir kapılar. |
| **Materyaller** | Fırçalanmış çelik, dövme demir, kobalt-emaye trim, gri taş, altın hanedan vurgusu. Düşük-doygunluk, **soğuk** palet. Mat metal + nokta-parlak hanedan. |
| **Silahlar** | Gladius, kule-kalkan, arbalet (delici), tören-balyoz, ateş-asası. **Düzgün/üretilmiş** hat — el-yapımı değil fabrikasyon okuması. |
| **Zırh** | Plaka (Heavy), kule-kalkan (Shielded), hafif çelik+kumaş (Light). Standartlaştırılmış üniforma okuması — "ordu", birey değil. |
| **Bayrak/crest** | Kobalt zemin üstüne **çelik hanedan amblem** (kalkan/kule motifi); keskin geometrik. Cephe boyunca dikili sancaklar. |
| **Heykel** | **İkonik kobalt-çelik savaşçı/koruyucu anıtı**; dik, kalkanlı, otoriter. Hasar: Intact (parlak) → Cracked (çatlak emaye) → Breaking (düşen plaka + duman) → Destroyed (devrik moloz). Kalkan fazı = kobalt enerji-kabuk. |
| **Komutan** | **Iron Warden** — tören-plaka, sancak, parlatılmış çelik+altın; dik otorite duruşu. |
| **VFX teması** | Soğuk: kobalt enerji, çelik kıvılcım, beyaz-mavi; Battlemage istisna = turuncu ateş (sıcak aksan, kasıtlı kontrast). |
| **Atmosfer** | Soğuk mavi ışık, net gölge, düzenli kamp; sis yok-disiplin. |

### 7.2 THE ASHEN HORDE — sürü saldırısı (kor + öküzkanı)

| Eksen | Görsel yönerge |
|---|---|
| **Fantezi** | Hızlı, harcanabilir bir dalga; sen hazır olmadan ezer. "Kül, kor, vahşet." |
| **Mimari** | Düzensiz, asimetrik, kemik+ahşap+post; **göçebe savaş-kampı** estetiği. Spawn = **kaba çadır/kazık-totem/kor-çukur**. Çapraz açılar, yağma-bayrak, ateş. |
| **Materyaller** | Kavrulmuş deri, kemik, ham demir, kül-grisi, **kor-turuncu damar**, öküzkanı kumaş. Yüksek-kontrast, **sıcak** palet. Pürüzlü/yıpranmış yüzeyler. |
| **Silahlar** | Balta/satır, sapan/bola (künt), zehir-totemi, doğal silah (Razorbeast diş/boynuz). **El-yapımı/yağma** okuması — düzensiz, kişisel. |
| **Zırh** | Çoğu **Light** (paçavra/deri); Razorbeast Heavy (post+diken). Bireysel/yağmalanmış okuma — üniforma değil. |
| **Bayrak/crest** | Öküzkanı zemin üstüne **kor-damarlı kemik/pençe amblem**; pürüzlü, kabilesel. Kazık-totemlerde asılı. |
| **Heykel** | **Kor-damarlı kemik/post canavar-totem anıtı**; eğik, saldırgan, ilkel. Hasar: Intact (kor parlak) → Cracked (sönen kor) → Breaking (düşen kemik + kül) → Destroyed (sönmüş yığın). Kalkan fazı = kor enerji-kabuk. |
| **Komutan** | **Ashen Warchief** — kemik-taç, kürk, kor savaş-boyası; eğik saldırgan duruş. |
| **VFX teması** | Sıcak: kor-turuncu, kül, kan-kızılı; Hexcaster istisna = zehir-yeşili (soğuk aksan, kasıtlı kontrast). |
| **Atmosfer** | Sıcak kızıl-turuncu ışık, kor-parçacık, kül-sis, dağınık ateşler. |

### 7.3 Fraksiyon kontrast tablosu (anında ayırt-edilebilirlik — §6/§11)
| Boyut | Iron Pact | Ashen Horde |
|---|---|---|
| Ana renk | Çelik-gri + **kobalt** | Kül-gri + **öküzkanı/kor** |
| Sıcaklık | **Soğuk** | **Sıcak** |
| Form dili | **Düzgün/simetrik/üretilmiş** | **Pürüzlü/asimetrik/yağma** |
| Yüzey | Mat metal + parlak hanedan | Yıpranmış deri/kemik + kor-damar |
| Siluet ritmi | Düzenli, dik | Düzensiz, eğik |
| Aksan-VFX (kontrast) | Turuncu ateş (Battlemage) | Yeşil zehir (Hexcaster) |

> **Çapraz-kontrol (KB-5):** İki fraksiyon, **renk + sıcaklık + form-dili** üç eksende ayrışır → renk-körü modunda bile (sıcaklık+form) okunur. Bu, §6 "fraksiyon-renk kimliği İHLAL EDİLEMEZ" kuralını üç-katmanlı güvenceye alır.

### 7.4 Fraksiyon-bazlı asset listesi (üretim için)
**Iron Pact:** 6 birim + Iron Warden + (gelecek summon paylaşımlı) · garnizon-spawn · kobalt-çelik heykel ×4 durum + kalkan · IP bayrak/crest · IP silah seti (gladius/kule-kalkan/arbalet/balyoz/ateş-asası) · IP zırh dili (plaka/kule/hafif) · soğuk VFX teması · IP müzik motifi.
**Ashen Horde:** 6 birim + War Hounds + Ashen Warchief · çadır/totem-spawn · kor-kemik heykel ×4 durum + kalkan · Ashen bayrak/crest · Ashen silah seti (balta/sapan/totem/doğal) · Ashen zırh dili (deri/post) · sıcak VFX teması · Ashen müzik motifi.
**Paylaşımlı/nötr:** Giant + Pouncer summon · altın madenler · 3 harita arazi/çevre · arazi-özellik tile'ları · UI/ikon/HUD · sandık/mağaza · genel SFX.

---
## PHASE 8 — HARİTA ÜRETİM DENETİMİ

3 harita = **tek battlefield arketipi, 3 terrain layout** (§5.4). Hepsi: tek yatay cephe, **3 satır**, frontLength=40, uçlarda heykeller (team0 x0 / team1 x40). Aşağıdaki terrain/mine yerleşimleri **doğrudan `.asset` verisinden** (varsayım değil). Her harita için gereken görsel asset'ler listelenir.

### 8.1 MAP — Choke Pass (`map_chokepass`) — dar geçit teması
**Veri-teyitli arazi:** Choke @x20 r1 (w3) · Hazard @x20 r0 (w2) · Hazard @x20 r2 (w2) · HighGround @x15 r1 (w3). **Madenler:** x12 (cap2), x28 (cap2).

| Tür | Gerekli asset |
|---|---|
| **Terrain pieces** | Merkez **darboğaz geçidi** (r1, w3) — net daralan kapı/kanyon; **yüksek-zemin sırtı** (x15, r1) geçit-öncesi |
| **Cover** | (Bu haritada cover yok — choke + hazard baskın) |
| **Hazard** | **2 tehlike bölgesi** (r0+r2, x20) — geçidi kuşatan; lav/zift/diken; DoT-glow shader + parçacık |
| **Foliage** | Az; kurak geçit teması — seyrek kuru çalı, yosun |
| **Rocks** | **Yoğun** — kanyon duvarları, darboğaz kaya blokları, sırt kayası |
| **Bridges** | **Köprü varyantı** burada uygundur (darboğaz = köprü/geçit) — opsiyonel BRG-01 |
| **Props** | Geçit kapısı/totem, kazıklar, uyarı işaretleri (hazard kenarı) |
| **Ruins** | Geçidi çerçeveleyen yıkık kapı/sütun (cover+kimlik) |
| **Weather/Atmosfer FX** | Hazard'dan **ısı-pus/duman**; dar-geçit toz; kıstırılmış ışık |
| **Mines** | 2 altın düğümü (x12, x28) — geçidin iki yanında, düşük-cap (2) = kıt/çekişmeli |

### 8.2 MAP — Open Field (`map_openfield`) — açık ova teması
**Veri-teyitli arazi:** HighGround @x20 r1 (w4) · Cover @x10 r0 (w3) · Cover @x30 r2 (w3). **Madenler:** x18 (cap3), x22 (cap3).

| Tür | Gerekli asset |
|---|---|
| **Terrain pieces** | Merkez **geniş tepe/yüksek-zemin** (x20, r1, w4) — haritanın hakim noktası |
| **Cover** | **2 orman/siper kümesi** (x10 r0, x30 r2) — çapraz konumlu; LoS keser, −menzilli hasar |
| **Hazard** | (Yok — temiz açık savaş) |
| **Foliage** | **Yoğun** — 2 cover ormanı (ağaç kümeleri) + ova otları, çiçek, çalı dolgu |
| **Rocks** | Orta — tepe kayaları, dağınık taşlar |
| **Bridges** | (Yok) |
| **Props** | Açık-ova: çitler, eski tarım/sınır taşları, dikili sancaklar |
| **Ruins** | Hafif — ova ortasında yıkık sınır-taşı/anıt (kimlik) |
| **Weather/Atmosfer FX** | Açık gökyüzü, **rüzgar+ot-dalgalanma**, hafif toz; berrak ışık |
| **Mines** | 2 altın düğümü (x18, x22) — **merkeze yakın bitişik**, yüksek-cap (3) = merkez-kontrol çatışması |

### 8.3 MAP — Ridgeline (`map_ridgeline`) — sırt/yükselti teması
**Veri-teyitli arazi:** HighGround @x14 r0 (w4) · HighGround @x26 r2 (w4) · Cover @x20 r1 (w3) · Hazard @x20 r0 (w2). **Madenler:** x20 (cap4), x8 (cap2), x32 (cap2).

| Tür | Gerekli asset |
|---|---|
| **Terrain pieces** | **2 karşıt yüksek-zemin sırtı** (x14 r0, x26 r2) — çapraz hakimiyet; merkez sırt-geçidi |
| **Cover** | **1 merkez orman/siper** (x20, r1) — sırtlar arası |
| **Hazard** | **1 tehlike** (x20, r0) — üst sırt kenarında DoT |
| **Foliage** | Orta — merkez cover ormanı + sırt-yamacı çalı/çam |
| **Rocks** | **Çok yoğun** — sırt/kayalık ana tema; uçurum kenarları, kaya basamakları |
| **Bridges** | Opsiyonel — sırtlar arası geçit köprüsü |
| **Props** | Yükselti bayrakları, gözcü-totemleri, kaya-merdiven |
| **Ruins** | Sırt-tepesinde yıkık kale/gözcü-kulesi (kimlik+cover) |
| **Weather/Atmosfer FX** | **Yükselti sisi/rüzgar**, bulut-gölge geçişi; hazard ısı-glow |
| **Mines** | **3 altın düğümü** (x20 cap4 merkez-zengin, x8+x32 cap2 kanat) — en zengin ekonomi haritası |

### 8.4 Harita-genel paylaşımlı asset'ler (3 haritada tekrar kullanılır)
| Tür | Asset |
|---|---|
| **Heykeller** | Her haritada 2 fraksiyon heykeli (x0, x40) — STA-01/02 (paylaşımlı) |
| **Madenler** | MIN-01 altın düğümü (yerleşim haritaya göre) |
| **Arazi-özellik kiti** | TER-01…04 okunur tile/shader (4 tip) — tüm haritalarda |
| **Satır-okuma** | 3-satır zemin ayrımı (subtle), her haritada |
| **Spawn yapıları** | BLD-01 ×2 fraksiyon (uçlarda) |
| **Parallax/gökyüzü** | ENV-04/05 — harita-temasına göre tonlanır |

### 8.5 Harita üretim özeti
- **Toplam benzersiz terrain feature instance:** Choke Pass 4 · Open Field 3 · Ridgeline 4 = **11 yerleşim**, ama yalnızca **4 tip** (HighGround/Choke/Cover/Hazard) → **modüler tile + shader** ile karşılanır (BUY+KITBASH+shader).
- **En kaya-yoğun:** Ridgeline + Choke Pass · **En foliage-yoğun:** Open Field.
- **Weather/atmosfer:** Ağır sistem değil — §11 "restrained particles, mobile budget". Harita-teması ışık + hafif parçacık (toz/sis/kor) yeterli.
- **Modülerlik kazancı:** 3 harita, tek **modüler doğa/kaya/orman kiti** (Synty POLYGON Nature veya Quaternius/Kenney) + arazi-shader ile montajlanır → düşük maliyet, hızlı yeni-harita (Launch'ta +1 harita, §13 6.2).

---
## PHASE 9 — ANİMASYON DENETİMİ

**Karar kategorileri:**
- **REUSE** = paylaşılan iskelet üstünde tek klip, tüm birimlerde tekrar (§10 paylaşımlı-iskelet motoru). En ucuz.
- **RETARGET** = kütüphaneden (Mixamo/Synty) Humanoid avatar'a aktar. Ucuz, hızlı.
- **CUSTOM** = bespoke yazım. **Telegraph/attack-windup zamanlaması KİLİTLİ** (§5.3/§6 İHLAL EDİLEMEZ — kozmetik bunu değiştiremez; counter dürüstlüğü) → bunlar custom/ince-ayar.

**İlke (§10 + §16):** Lokomosyon + tepkiler **Retarget/Reuse** (içerik-hızı); **imza saldırı + telegraph + cast** **Custom** (okunabilirlik). Bu, "paylaşılan iskelet → fraksiyon reskin" kanonunu animasyona taşır.

### 9.1 Çekirdek lokomosyon & tepki (tüm insansı birimlerde paylaşılır)
| Klip | Sahip | Karar | Neden |
|---|---|---|---|
| Idle | Tüm birimler | **RETARGET** (1 taban + fraksiyon flavor) | Paylaşılır; ucuz |
| Walk | Tüm birimler | **RETARGET** | Paylaşılır |
| Run/charge | Hızlı birimler (Raider/Houndmaster/Legionary) | **RETARGET** | Hız okuması; paylaşılır |
| Move→combat geçiş | Tüm | **REUSE** | Blend; tek geçiş |
| Hit/flinch | Tüm | **RETARGET** | Vuruş geri-bildirimi; paylaşılır |
| Death (×2 varyant) | Tüm | **RETARGET + bazı CUSTOM** | Genel ölüm retarget; ağır/canavar custom |
| Spawn/emerge | Tüm | **RETARGET** | Eğitim→deploy |

### 9.2 Arketip-imza saldırı & rol (zamanlama-kilitli → Custom ağırlıklı)
| Klip | Sahip | Karar | Neden |
|---|---|---|---|
| Melee saldırı (kılıç/balta) | Legionary, Raider | **RETARGET + CUSTOM zamanlama** | Telegraph dürüstlüğü (§6) |
| Ağır saldırı (balyoz/doğal) | Ironclad, Razorbeast, Giant | **CUSTOM** | Ağır windup okunmalı (counter) |
| Kalkan blok/brace | Shieldman | **CUSTOM** | Frontline kimliği; ön-cephe denial okuması |
| Ranged windup+release | Crossbow, Slinger | **CUSTOM** | Mermi-anı + telegraph (§5.3) |
| Cast (büyü) | Battlemage, Hexcaster | **CUSTOM** | Cast telegraph okunmalı (counter) |
| Mine/work loop | Miner ×2 | **RETARGET/CUSTOM** | Kazma döngüsü (ekonomi okuması) |
| Flank-dash | Houndmaster | **RETARGET** | Flanker hız okuması |

### 9.3 Possess & kozmetik flourish (§4 agency, §6 kozmetik)
| Klip | Sahip | Karar | Neden |
|---|---|---|---|
| Possessed aim/manual | Possess-edilebilir birimler | **CUSTOM** | Agency hook (§4); manuel nişan |
| Victory/idle flourish (kademe) | Legendary/Mythic kozmetik | **CUSTOM** (kademe başına) | §6 "idle/victory flourish" (yalnızca üst kademe; silüet sabit) |

### 9.4 Komutan & summon & yaratık (özel)
| Klip | Sahip | Karar | Neden |
|---|---|---|---|
| Rally cast + aura | Iron Warden | **CUSTOM** | İmza yetenek (§6); okunur aura |
| WarCry cast + aura | Ashen Warchief | **CUSTOM** | İmza yetenek (§6) |
| Komutan idle/presence | 2 komutan | **RETARGET** | Kahraman duruşu |
| Giant: idle/walk/attack/death | CHR-14 | **RETARGET + CUSTOM attack** | Dev-ölçek basış |
| Pouncer: idle/run/pounce/death | CHR-15 | **RETARGET + CUSTOM pounce** | Sıçrama imzası |
| War Hound: idle/run/attack/death | CHR-13 | **RETARGET** | Dört-ayak (creature pack) |
| Razorbeast: idle/walk/attack/death | CHR-12 | **RETARGET + CUSTOM** | Canavar gövde |

### 9.5 Heykel & obje (iskeletsel değil — durum/shader/VFX)
| "Animasyon" | Karar | Neden |
|---|---|---|
| Heykel hasar-durumu geçişleri (Intact→Cracked→Breaking→Destroyed) | **CUSTOM** (mesh-swap + VFX + prosedürel fracture) | §11 okunur doruk; en görünür obje |
| Heykel kalkan aktif/kırılma | **CUSTOM VFX** | Faz göstergesi |
| Maden doluluk/tükenme | **REUSE** (shader/scale) | Hafif geri-bildirim |
| Mermi uçuş/iz | **GENERATE** (shader trail) | PRJ-07 |

### 9.6 Animasyon bütçe özeti
- **Benzersiz klip tahmini (MVP):** ~**8 paylaşımlı lokomosyon/tepki** (Reuse/Retarget) + ~**10–14 imza/cast/role** (Custom) + ~**6–8 komutan/summon/yaratık** = **~24–30 klip**, ama **paylaşım sayesinde 18 insansı birim ~8–12 benzersiz iskelet-seti üzerinde çalışır**.
- **Custom oranı:** ~%50 (telegraph/okunabilirlik kilidi) — bu, §6'nın "animasyon zamanlaması kilitli" kuralının doğrudan maliyeti; kısılamaz.
- **Retarget kaynağı (Yol B):** Mixamo (ücretsiz) + Synty anim paketleri (rig-eşi) → Custom yalnızca imza/telegraph'a harcanır.
- **Yol A (2D Spine) farkı:** Tüm klipler Spine'da elde-rig/key → Retarget kaybolur, **Custom oranı ~%100** → animasyon maliyeti belirgin artar (2D-yol seçilirse bütçeye yansıt).

---
## PHASE 10 — VFX DENETİMİ

**Renk-kodu (Blueprint §10 — bağlayıcı, okunabilirlik):** Slash/Melee=çelik · Pierce=beyaz · Blunt=toz · Magic=mor · **Fire=turuncu** · **Poison=zehir-yeşili** (implementasyon `DamageType.Poison`). **Mobil bütçe (§12):** sıkı parçacık limitleri; GPU-instancing; particle cap; her VFX "okunur ama ucuz".

### 10.1 Saldırı + İsabet (hasar-tipi başına — 5 tip × 2)
| Hasar tipi | Saldırı VFX | İsabet/Impact VFX | Sahip birim |
|---|---|---|---|
| **Melee/Slash** (çelik) | Kılıç/balta iz (steel arc) | Çelik kıvılcım + kesik | Miner, Shieldman, Legionary, Raider, Houndmaster, Ironclad, Razorbeast |
| **Pierce** (beyaz) | Cıvata/ok beyaz streak | Delici beyaz puf + sapma | Crossbow |
| **Blunt** (toz) | Sapan dönüş bulanık | Toz darbe + sarsıntı | Slinger |
| **Fire** (turuncu) | Ateş topu + alev iz | Turuncu patlama + **Burning tutuşma** | Battlemage |
| **Poison** (yeşil) | Zehir-bolt yeşil iz | Yeşil sıçrama + **Poisoned bulut** | Hexcaster |

### 10.2 Mermiler (PRJ — uçuş + iz + decal)
Arbalet-cıvata · sapan-taşı/bola · ateş-topu · hex-bolt · ok-yağmuru okları · yıldırım vuruşu · **trail (shader)** + **isabet decal** (her tip). → ~7 mermi-VFX + paylaşımlı trail/decal sistemi.

### 10.3 Büyü efektleri (12 büyü × [telegraph + effect]) — veri-teyitli
Her büyünün **telegraph'ı** (counter penceresi, §5.3 — `ActiveTelegraph`) + **effect'i** var. Telegraph = yer-halkası/gösterge (dodge/cleanse/spread okuması).

| Büyü | Kategori | Telegraph VFX | Effect VFX | Statü/sinerji |
|---|---|---|---|---|
| **Freeze** | Control | Mavi yer-halkası (r3) | Donma patlaması + buz-zemin | → **Chilled** (sinerji kurar) |
| **Shatter** | Offensive | Beyaz-mavi halka | Kırılma şok-dalgası | **Chilled'a karşı bonus** (kanon sinerji) |
| **Arrowstorm** | Offensive | Yer hedef-alanı (gölge) | Ok yağmuru + toz-isabet | — |
| **Lightningstorm** | Offensive | Gökyüzü-işaret halkaları | Yıldırım vuruşları + ark | → **Burning** |
| **Poisoncloud** | Control/Off | Yeşil yer-halkası | Zehir bulutu (DoT) | → **Poisoned** |
| **Stun** | Control | Sarı yer-halkası | Sersemletme şoku + yıldız | → **Stunned** |
| **GoldRush** | Economy | Altın ışıma (ally) | Altın dalga | → **GoldBoost** |
| **RaiseGold** | Economy | Altın parıltı | Altın yükseliş | (ekonomi) |
| **Rage** | Buff | Kızıl ally-halka | Öfke aurası | → **Raged** |
| **Haste** | Buff | Mavi-beyaz ally-halka | Hız çizgileri aurası | → **Hasted** |
| **SummonGiant** | Summon | Mor çağrı-halkası | Çağrı portalı + giriş | Giant (CHR-14) |
| **SummonPouncer** | Summon | Mor çağrı-halkası (küçük) | Çağrı portalı + giriş | Pouncer (CHR-15) |

### 10.4 Buff auraları (4 — pozitif, ally üstünde sürekli)
| Statü | VFX | Kaynak |
|---|---|---|
| **Hasted** | Mavi-beyaz hız-çizgi aura | Haste / komutan |
| **Raged** | Kızıl öfke-parıltı aura | Rage |
| **GoldBoost** | Altın parıltı aura | GoldRush |
| (Commander buff) | Fraksiyon-renk halka | Rally/WarCry (§10.6) |

### 10.5 Debuff efektleri (3 — negatif, enemy üstünde)
| Statü | VFX | Okuma |
|---|---|---|
| **Chilled** | Buz-kristal kabuk + mavi ton | Yavaş + Shatter-hazır (sinerji) |
| **Burning** | Turuncu alev tutuşma (DoT) | Yanıyor |
| **Poisoned** | Yeşil damla/bulut (DoT) | Zehirli |
| **Stunned** | Baş-üstü yıldız/şimşek | Aksiyon-dışı |

> **Sinerji okuması (kanon):** Chilled (buz-kabuk) bir birime uygulanınca, Shatter o birime vurduğunda **"bonus" patlama** görsel olarak farklı (daha büyük kırılma) → oyuncu sinerjiyi *görür*. Bu, §5.3 "synergy tags" görsel karşılığı.

### 10.6 Komutan yetenekleri (2 imza — okunur aura, ≤%12–13 güç §6)
| Yetenek | Komutan | VFX |
|---|---|---|
| **Rally** (active) + Quartermaster (passive) | Iron Warden | Kobalt **disiplin-halkası** yayılır (formasyon-buff okuması); pasif = sönük çelik-parıltı |
| **WarCry** (active) + Bloodthirst (passive) | Ashen Warchief | Kor **öfke-dalgası** yayılır (hız/saldırganlık okuması); pasif = sönük kor-parıltı |

### 10.7 Ölüm efektleri (arketip-bazlı)
Hafif birim = toz-dağılma · Ağır/zırhlı = plaka-düşme + toz · Caster = büyü-sönme (fraksiyon-renk fizzle) · Canavar/Razorbeast/Giant = ağır çöküş + sarsıntı · Summon = portal-geri-sönme. → ~5 ölüm-VFX ailesi (paylaşımlı).

### 10.8 Heykel & ekonomi & çevre VFX
| Olay | VFX |
|---|---|
| Heykel: Cracked | Çatlak-toz puf |
| Heykel: Breaking | Düşen parça + duman + (Ashen kor / Iron kıvılcım) |
| Heykel: Destroyed | Büyük yıkım patlaması + toz sütunu |
| Heykel kalkan: aktif/kırılma | Yarı-saydam kabuk parıltı / kırılma şoku |
| Madencilik | Altın kıvılcım loop (maden başında) |
| Altın bankalama | Altın "+N" pop (banked Gold) |
| Hazard bölgesi | Lav/zift/zehir DoT-glow + parçacık (TER-04) |
| Yüksek-zemin | Hafif highlight/decal (TER-01 okuması) |

### 10.9 UI geri-bildirim VFX
Buton-basış · cooldown süpürme (büyü/komutan) · büyü-hazır flash · eğitim-tamam parıltı · currency kazanç/harcama pop · sağlık-bar hasar/heal · hasar-sayısı toggle · seviye-atlama · **sandık-açılış patlaması** (nadirlik-renkli) · victory/defeat flourish · yıldız-kazanma. → ~15 UI-VFX.

### 10.10 VFX bütçe özeti
- **Toplam tahmini:** ~5 saldırı + ~5 impact + ~7 mermi + ~24 büyü (12 telegraph + 12 effect) + ~7 statü-aura + ~2 komutan + ~5 ölüm + ~8 heykel/ekonomi/çevre + ~15 UI = **~80–95 çekirdek VFX** (+kozmetik VFX-renk varyantları üst kademe).
- **En kritik (P0):** hasar-tipi impact (okunabilirlik), büyü telegraph (counter), statü-aura (sinerji okuması), heykel-yıkım (objektif).
- **Renk-kodu disiplini:** Tüm VFX §10 paletine sadık → renk-körü modunda da hasar-tipi/sinerji okunur.
- **Kozmetik (§6):** Üst kademeler yalnızca **VFX-renk** değiştirir (silüet/okuma sabit) → GENERATE ile çoğaltılır.
- **Kaynak (Yol B):** JMO Cartoon FX/War FX + Synty Particle (BUY) → recolor (KITBASH) → §10 paletine uydur; telegraph okuması Custom-ayar.

---
## PHASE 11 — ASSET ENTEGRASYON REHBERİ

**Proje gerçeği (Phase 0 manifest + kanon §12):** Unity 6 LTS · IL2CPP · **URP** · **ECS/DOTS battle-sim** (Entities/Collections/Burst) · **Addressables + remote catalog/CDN** · mobil-öncelik · mid-range telefon perf bütçesi (sert kapı). **Sim sınırı:** savaş = ECS; UI/meta = MonoBehaviour/UGUI.

> **Render sonucu (KB-1/KB-7):** ECS'te yüzlerce birim → **Entities Graphics + GPU instancing** doğal yol. Bu, **Yol B (3D low-poly)** için tek-mesh-çok-instance demektir (çok ucuz draw-call). **Yol A (2D Spine)** için ECS'te sprite-render daha custom (SpriteRenderer ECS-uyumlu değil; özel mesh/quad gerek) → ek mühendislik. Entegrasyon rehberi Yol B'ye göredir; Yol A farkları not edilir.

### 11.1 Boru hattı (her asset tipi için adım adım)

| Aşama | Karakterler (birim/komutan/yaratık) | Çevre/prop/terrain | VFX/mermi | UI/ikon/portre |
|---|---|---|---|---|
| **FBX import** | Metre-ölçek, Y-up, mesh-compression Med, **Read/Write OFF**, normal import | Aynı; static-batch flag | — (particle/prefab) | — (sprite/PNG) |
| **Rig** | İnsansı = **Humanoid**; yaratık = Generic; statik aksesuar = none | **Static mesh** (rig yok) | yok | yok |
| **Avatar** | **Humanoid Avatar** + Avatar Mask (Mixamo/Synty **retarget**) | yok | yok | yok |
| **Materials** | URP **Simple Lit/Unlit**; **texture atlas** (fraksiyon-set); **MaterialPropertyBlock** ile kozmetik recolor; **GPU instancing ON** | URP Lit/Simple Lit; atlaslı; SRP Batcher | URP particle (VFX Graph/Shuriken); additive; mobil-shader | UI/Unlit; sprite-atlas |
| **Addressables** | Grup: `Units/<Faction>/<Archetype>` (Art/README layout) | `Terrain/<kind>`, `Env/<map>` | `Spells/<id>` | `UI/...`, `Commanders/<id>` |
| **LOD** | LOD0/LOD1 + **uzak impostor**; ECS = mesafe-LOD/instancing | **LODGroup** (ağaç/kaya/yapı) | mesafe-cull + particle-LOD | yok (2D) |
| **Optimizasyon** | Atlas, instancing, tri-bütçe, decimate (CGTrader/Sketchfab) | Static batch, occlusion, atlas | Particle cap, pool, GPU | Sprite atlas, 9-slice |
| **Mobil limit** | ASTC, tri/texture cap (aşağıda) | ASTC, draw-call batch | concurrent particle cap | atlas max-size |

### 11.2 Addressables grup mimarisi (Art/README layout + remote CDN)
```
Addressables Groups (remote catalog → CDN, §12):
  Units/IronPact/<Archetype>   → mesh+atlas+anim+recolor varyant (label: faction_ironpact, type_unit)
  Units/Ashen/<Archetype>      → (label: faction_ashen, type_unit)
  Creatures/<id>               → Giant/Pouncer/Hound/Razorbeast
  Commanders/<id>              → mesh + portre + banner (out-of-battle kozmetik)
  Spells/<spell_id>            → telegraph + effect VFX (readability-locked)
  Terrain/<kind>               → HighGround/Choke/Cover/Hazard tile+shader
  Env/<map_id>                 → zemin+parallax+atmosfer (chokepass/openfield/ridgeline)
  Statues/<faction>            → 4 hasar durumu + kalkan
  UI/<screen>                  → HUD/draft/shop/pass/chest/result
  Icons/<set>                  → unit/spell/currency/status/...
  Audio/<bank>                 → SFX bankları + müzik (stream)
  Cosmetics/<unit>/<tier>      → material/recolor varyant (Phase 4; remote — canlı event)
```
**Neden:** Kozmetik + event içeriği **remote CDN'den canlı** gelir (§12 "Addressables + CDN; original local-only idi"). Kozmetikler ayrı grup → app-update'siz yeni skin. Label'lar fraksiyon/tip/kademe bazlı yükleme + bellek yönetimi sağlar.

### 11.3 Mobil limitler (mid-range telefon — sert perf kapısı §12)
| Kaynak | Hedef bütçe (mid-range, yüzlerce birim) |
|---|---|
| **Tri/birim (LOD0)** | ~500–1.500 tri (low-poly); LOD1 ~%50; uzak impostor/billboard |
| **Birim mesh stratejisi** | **Tek mesh → GPU instance** (ECS Entities Graphics); fraksiyon-set tek atlas |
| **Texture** | **ASTC** sıkıştırma; karakter-atlas 1024–2048; UI-atlas 2048; mip ON |
| **Materyal/birim** | 1–2 (atlaslı); **GPU instancing ON**; SRP Batcher uyumlu shader |
| **Draw call** | Instancing ile yüzlerce birim → onlarca draw-call (yüzlerce değil) |
| **Particle** | Eşzamanlı cap (örn. ekran-başı sınır); pool'lu; mobil-shader; no overdraw-bomb |
| **Işık/gölge** | Baked/az dinamik; gerçek-zaman gölge minimal (mobil) |
| **Shader** | Simple Lit/Unlit; pahalı efekt (refraction vb.) yok |
| **Bellek** | Recovered referans ~660MB RSS — bütçe içinde kal; atlas+stream+addressables-unload |
| **Frame** | Her faz perf-gate (§12); ECS sim fixed-timestep; budgeted AI scheduler |

### 11.4 Kozmetik recolor pipeline (§6 gelir motoru — material-only)
- Taban birim: **tek mesh + tek "skin-atlas"**. Kozmetik kademe = **material varyant** (renk/trim/emissive maskesi), MaterialPropertyBlock veya material-swap.
- **Silüet/mesh/UV SABİT** — kademeler yalnızca texture/material. Bu, §6 "silüet değişmez" + §16 içerik-hızı için zorunlu.
- **Clarity mode (ranked):** Addressables label `clarity_safe` → rakip birimler standart okunur skin ile render (kozmetik okuma-avantajı sıfır).
- **GENERATE:** kademe varyantları şablon-material + maske ile çoğaltılır (elde-mesh yok).

### 11.5 İçe-aktarma kalite kontrol (satın-alınan asset'ler için zorunlu)
1. **Ölçek/pivot:** metre-ölçek, ayak-pivot (birimler), zemin-pivot (prop).
2. **Poly-temizlik:** CGTrader/Sketchfab yüksek-poly → decimate + retopo (mobil bütçe).
3. **Atlas birleştirme:** fraksiyon-set tek atlas (draw-call).
4. **Rig doğrulama:** Humanoid avatar map (retarget testi — Mixamo klip oynar mı?).
5. **Silüet testi (KB-5):** RTS-uzak-kamerada okunur mu? Renk-körü mod?
6. **Lisans kaydı:** kaynak + lisans + atıf-gereksinimi tabloya (OGA/Sketchfab/CGTrader).
7. **Addressables ata:** doğru grup + label + (kozmetik ise) remote.

### 11.6 Yol A (2D Spine) entegrasyon farkı (kanon kalırsa)
- Spine → `spine-unity` runtime; SkeletonAnimation/SkeletonGraphic. ECS render için **özel sprite-mesh köprüsü** gerekir (ek mühendislik — TA riski).
- Atlas = Spine atlas; kozmetik = **Spine skin** (slot-recolor). LOD yok (2D); "uzak" = daha küçük sprite.
- Addressables yapısı aynı; mesh yerine Spine skeleton-data + atlas.
- **Animasyon:** tüm klipler Spine'da (retarget yok) → §9 Custom-oranı ~%100.

---
## PHASE 12 — BÜTÇE ANALİZİ

Kapsam tierleri Blueprint §2 + Decision Log §4'ten: **MVP (soft launch)** → **Launch (S1)** → **Full Vision**. Sayılar **taban asset** (kozmetik kademe çarpanı ayrı satırda).

### 12.1 Asset sayıları — kategori × kapsam

| Kategori | **MVP (taban)** | **Launch (+S1/cadence)** | **Full Vision** |
|---|---|---|---|
| Karakter rig (birim+kmd+yaratık) | **~18** | +1 içerik-slotu (1 birim *veya* kmd *veya* harita/sezon) + clarity-skin | **~40–55** (4 fraksiyon ×6–9 + kmd roster + biome-yaratık) |
| Silah (ana) | ~14 | +1–2/sezon | ~40+ |
| Heykel durumu | **8** (+2 kalkan) | (clarity/kozmetik heykel skin) | 4 fraksiyon × 4 = **16** (+kalkan) |
| Harita çevre seti | **3** | +1/sezon (slot ise) | biomes ×4 × birden çok = **12+** |
| Arazi tile tipi | **4** | (biome varyant) | 4 × biome temaları |
| Maden/prop/kaya/foliage | ~20 | +biome propları | ~60+ |
| Mermi | 7 | +yeni büyü mermisi | ~15+ |
| VFX (çekirdek) | **~80–95** | +sezon büyü/kozmetik-VFX | ~200+ |
| Animasyon klibi | **~24–30** | +yeni birim/kmd setleri | ~80+ |
| UI ekranı | **~15** | +ranked/clan/event UI | ~30+ |
| İkon | **~75** | +yeni içerik ikonları | ~200+ |
| Portre | ~14 | +yeni kmd/birim | ~40+ |
| Yükleme/key art | ~4 | +sezon key-art | ~20+ |
| Sandık/mağaza obj | ~10 (Phase 4) | +sezon sandık/bundle | ~30+ |
| SFX | **~150–250** | +sezon | ~500+ |
| Müzik | **~8–12** | +1–2 sezon teması | ~30+ |
| **Kozmetik kademe (×5)** | Standard (12) + Battle Pass S0 başlangıç hattı | **60 varyant** rollout (12×5) + banner/emote + clarity (12) | **yüzlerce** (birim×kademe×varyant) — *gelir motoru* |

**MVP benzersiz görsel asset tahmini (kozmetik kademe hariç):** ~**450–650 ayrık asset/parça** (karakter+silah+çevre+VFX+UI+ikon+anim + ses/müzik dahil). Kozmetik **Standard taban** ile başlar; **×5 kademe = Phase 4/Launch** (gelir motoru, kademeli açılır).

### 12.2 Üç tedarik yolu — maliyet & karakter

| Yol | Yaklaşım | Asset-harcaması (kabaca) | Artı | Eksi | Kanon-uyumu |
|---|---|---|---|---|---|
| **A) Ücretsiz/Free** | **Quaternius (CC0) + Kenney (CC0) + Mixamo + OGA + freesound + Kevin MacLeod (CC-BY)**; bespoke (heykel/crest/UI/müzik) founder-sweat | **~$0 asset** (yüksek emek) | Sıfır asset-maliyet; CC0 temiz | Stil-tutarsızlığı; Synty-look yok ama "free-look" riski; ağır entegrasyon emeği; atıf-yönetimi (OGA/CC-BY) | MVP-mümkün, kalite riski |
| **B) İndie bütçe** ⭐ | **Synty POLYGON paket(ler)i** (omurga) + seçili Asset Store VFX/UI/audio + Mixamo + **komisyon bespoke** (heykel/crest/portre/müzik/telegraph-anim) | **Asset packs ~$500–2.500** + komisyon bespoke ~$3k–15k = **~$4k–18k** asset; gerisi maaş (Blueprint §13: toplam $0.8–1.5M maaş-ağırlıklı) | Tutarlı stil (Synty) + hız + makul maliyet; içerik-hızı yüksek | Synty-tanınırlığı (kitbash+recolor ile aş); bazı bespoke şart | **Önerilen** — Blueprint §13 "çekirdek in-house, kozmetik/audio/loc dışarıdan" ile birebir |
| **C) Profesyonel** | **Custom art ekibi** (Blueprint §13: 2 Spine/3D artist) bespoke fraksiyon sanatı + tam kozmetik katalog + pro VFX/audio/müzik; Asset Store yalnızca referans/hızlandırıcı | Maaş-ağırlıklı (full vision $4–8M+) | En yüksek kalite/özgünlük; tam IP sahipliği; Synty-look yok | En pahalı; en yavaş; studio-ölçek (kazanılmalı, §14) | Full-vision; MVP'de aşırı |

### 12.3 Önerilen bütçe stratejisi (Blueprint §13/§14 ile hizalı)
1. **MVP = Yol B (indie bütçe).** Synty/Quaternius omurga + komisyon bespoke (yalnızca IP-kritik: heykel, crest, UI-dili, müzik, portre, telegraph-anim). Kozmetik **Standard taban** ile başla; **×5 kademe Phase 4'te** kademeli (gelir motoru gelir getirdikçe finanse eder).
2. **Bütçe sıkışırsa → Yol A'ya düş** (Quaternius/Kenney/Mixamo CC0) ama bespoke-kritikleri (heykel/crest/UI/müzik) **asla** atlama — bunlar marka+okunabilirlik.
3. **Full Vision = Yol C**, ama **kanıtlanmış MVP'den sonra** (§14 "studio-ölçeği kazan, öne-yükleme yapma").
4. **Kozmetik içerik-hızı (§16 risk #1/#4):** taban asset'leri **recolor-dostu** seç (Yol B Synty/Quaternius material-swap'i destekler) → ×5 kademe + sezon-hatları GENERATE ile ucuz çoğalır. *Kozmetik gelir motorunun maliyet-verimliliği, oyunun finansal sürdürülebilirliğinin #1 koşuludur.*

### 12.4 Maliyet uyarıları (Tech Artist gözüyle)
- **Bespoke kaçınılmazları:** Heykel (8 durum), fraksiyon crest, UI-dili, müzik kimliği, komutan portreleri, telegraph-animasyonları → **hiçbir yolda satın-alınamaz**; her zaman BUILD. Bütçenin "sert çekirdeği".
- **Animasyon Custom-oranı:** §6 zamanlama-kilidi → ~%50 Custom (Yol B). **Yol A (2D Spine) seçilirse ~%100 Custom** → animasyon bütçesi ~2× artar (kapsam kararına yansıt).
- **Mobil optimizasyon emeği:** CGTrader/Sketchfab modelleri decimate/retopo emeği ister (gizli maliyet) → Synty/Quaternius zaten mobil-hazır (tercih sebebi).
- **Lisans-yönetimi maliyeti:** Free/marketplace yolu atıf+lisans takibi emeği (gizli) → CC0 (Quaternius/Kenney) + Store-EULA (Synty) bunu minimize eder.

---
## PHASE 13 — MASTER TEDARİK PLANI (P0/P1/P2/P3)

Öncelik, **roadmap faz-kapılarına** bağlıdır — kanon sırası budur. Combat **fun-gate'ten** önce hiçbir meta/monetizasyon asset'i alınmaz (§13, §15 no-phase-jumping).

### 13.0 ⛔ BLOKLAYAN ÖN-KARAR (her tedarikın önünde)
**ADR: Yol A (2D Spine) mı, Yol B (2.5D low-poly 3D) mı? (KB-1).** Karakter/animasyon/çevre asset'leri **bu karar verilmeden satın alınamaz** — kaynaklar yola göre tamamen değişir. **GD+TA onayı şart** (§16; "Spine 2D" kanonunu değiştirmek ADR ister). *Bu, tedarik planının 0. adımıdır.* **Bu denetimin önerisi: Yol B (ADR ile).**

### 13.1 P0 — DERHAL (Phase 1 FUN GATE; 1 fraksiyon / 4 birim)
> Amaç: "**combat eğlenceli mi?**" bağlayıcı kapısını test et. Minimum, ama eksiksiz bir mikro-savaş. Combat eğlenceli değilse her şey durur (§13 GATE 1).

| Öncelik | Asset | Kaynak (Yol B) |
|---|---|---|
| P0-1 | **Iron Pact 4 birim:** Miner, Shieldman, Legionary, Crossbow (rig+anim) | Synty POLYGON modüler + Mixamo |
| P0-2 | **1 Statue (Iron Pact)** ×4 hasar durumu + kalkan | **BUILD bespoke** |
| P0-3 | **Altın maden düğümü** + doluluk | Synty/Quaternius prop |
| P0-4 | **1 harita** (Open Field — en sade) çevre + 3-satır zemin | Synty Nature/Quaternius |
| P0-5 | **Çekirdek combat VFX:** melee-slash impact, pierce impact, ölüm, mermi (ok) | JMO War FX + recolor |
| P0-6 | **Çekirdek animasyon:** idle/yürü/saldırı/vuruş/ölüm/mine | Mixamo retarget + Custom telegraph |
| P0-7 | **Savaş HUD'u** (Altın, eğitim-sırası, sağlık-bar, heykel-sağlık) | Kenney UI + kitbash |
| P0-8 | **Çekirdek SFX:** saldırı/impact/ölüm/maden/heykel-hasar | Kenney Audio + library |
| P0-9 | Possess/seçim overlay (shader) | GENERATE |

### 13.2 P1 — YÜKSEK (Phase 2–3: Vertical Slice + Meta)
> Amaç: pillarları kanıtla (2 fraksiyon, terrain/formasyon/counter, spell-draft, komutan) + meta kabuk.

| Öncelik | Asset | Not |
|---|---|---|
| P1-1 | **Iron Pact +2 birim** (Battlemage, Ironclad) + **Ashen Horde 6 birim** | Tam 12-birim roster |
| P1-2 | **War Hounds + Razorbeast + 2 summon** (Giant, Pouncer) | Yaratık paketleri |
| P1-3 | **2 Komutan** (Iron Warden, Ashen Warchief) + portreler | Kitbash + BUILD aksan |
| P1-4 | **2. Statue (Ashen)** ×4 durum + kalkan | BUILD |
| P1-5 | **Arazi tile (4):** HighGround/Choke/Cover/Hazard okunur + shader | BUILD/KITBASH |
| P1-6 | **+2 harita** (Choke Pass, Ridgeline) çevre/kaya/foliage | Synty/Quaternius |
| P1-7 | **12 büyü VFX** (telegraph + effect) + **7 statü-aura** + sinerji okuması | JMO + Synty Particle recolor |
| P1-8 | **Komutan aura VFX** (Rally/WarCry) | Custom |
| P1-9 | **Formasyon görselleri** (Line/Tight/Loose) + ikonları | UI |
| P1-10 | **Meta UI:** Kampanya harita (20 lvl), Armory/Upgrade, Sonuç/Ödül, Mod-seç, Fraksiyon/Komutan-seç, Ladder | Kenney UI + brand kitbash |
| P1-11 | **İkon seti (~75):** birim/büyü/komutan/currency/stat/formasyon/arazi/statü | BUILD/GENERATE + Kenney Icons |
| P1-12 | **Birim portreleri (12)** | BUILD |
| P1-13 | **Müzik:** menü + 2 fraksiyon savaş teması + victory/defeat + adaptif | Komisyon/BUILD |
| P1-14 | **Spawn yapıları (2)** + bayrak/crest (2) | Synty kitbash + BUILD crest |
| P1-15 | Ödül VFX (yıldız, victory/defeat banner) | UI VFX |

### 13.3 P2 — ORTA (Phase 4: Monetizasyon & Live-ops Kabuk)
> Amaç: kozmetik + battle pass + mağaza + sandık. **Sadece fun-gate + vertical-slice geçtikten sonra** (§13).

| Öncelik | Asset | Not |
|---|---|---|
| P2-1 | **Kozmetik kademe sistemi (×5):** 12 birim × Standard→Mythic = 60 varyant tabanı | **GENERATE** (material/recolor) |
| P2-2 | **Clarity-mode skinleri (12)** — ranked okuma-güvenli | §6 İHLAL EDİLEMEZ |
| P2-3 | **Mağaza UI** + öne-çıkan vitrin + bundle sanatı | §10 |
| P2-4 | **Battle Pass S0** ikili-iz ekranı + ödül görselleri | §10 |
| P2-5 | **4 Sandık** (Wood/Silver/Gold/Seasonal) model + açılış + patlama VFX | §8 |
| P2-6 | **Banner & emote** kozmetikleri (savaş-dışı) | §6 |
| P2-7 | **Gem-paketi görselleri** (değer merdiveni) + currency ikon cilası | §10 |
| P2-8 | Komutan **skin/VFX/ses** kozmetikleri | §6 (yalnızca kozmetik) |
| P2-9 | Üst-kademe **kozmetik VFX-renk** varyantları + idle/zafer flourish | §6 + §9.3 |

### 13.4 P3 — DÜŞÜK (Phase 5–6: Soft Launch / Season 1 / cila)
| Öncelik | Asset | Not |
|---|---|---|
| P3-1 | Yükleme ekranı varyantları + tip şablonu | Cila |
| P3-2 | **S1 içerik-slotu:** 1 yeni {birim *veya* komutan *veya* harita} (8-hafta sezon) | §13 6.2 — **tek slot** |
| P3-3 | Sezon kozmetik-hattı + sezon sandık/bundle | Live-ops |
| P3-4 | Ek SFX/müzik zenginliği + VO barks (opsiyonel) | §9 |
| P3-5 | Erişilebilirlik cilası (renk-körü palet doğrulama, reduced-motion) | §9 |

### 13.5 ❌ ŞİMDİ TEDARİK EDİLMEYECEK (§15 CUT/DEFER — kanon yasağı)
Bu asset'ler **kanon tarafından yasak veya ertelenmiş** — tedarik etmeyin (revisit-trigger'ları PRODUCTION_DECISION_LOG.md'de):
- **3./4. fraksiyon** (Arcane/Mechanized) görselleri — Phase 7 (DEFER); iki-fraksiyon dengesi kanıtlanmadan **hayır**.
- **Biome** çevre setleri (çöl/orman/kar/volkanik) — Phase 7 (DEFER); MVP tek-arketip 3 harita.
- **Komutan koleksiyon roster'ı** (2'den fazla) — Phase 7 (DEFER).
- **Clan/clan-wars UI**, **Ranked-lig** görselleri, **gerçek-zaman PvP** — Phase 7 (DEFER/CUT).
- **Loot-box/gacha açılış** görselleri — **CUT (ilkeli, kalıcı)**; sandıklar disclosed-odds kozmetik-only, gacha-açılış değil.
- **Enerji/stamina** UI — **CUT**.
- **Interstitial reklam** yerleşimleri — **CUT** (yalnızca opt-in rewarded).

### 13.6 Tedarik sıralaması (özet akış)
```
0. ADR: Yol A vs B  (GD+TA)  ← BLOKLAYICI
1. P0  → Synty/Quaternius taban + Mixamo + bespoke Statue + Kenney UI  → FUN GATE testi
2. (GATE 1 PASS) → P1 → tam roster + komutan + büyü-VFX + meta-UI + müzik  → GATE 2 playtest
3. (GATE 2/3 PASS) → P2 → kozmetik ×5 + mağaza + pass + sandık (GENERATE ağırlıklı)
4. (GATE 4/5 PASS) → P3 → launch cila + S1 tek-slot içerik
   (Phase 7 asset'leri: revisit-trigger fire etmeden ASLA)
```

---

## EK — İzlenebilirlik & Açık Kararlar

### A. Asset → Kanon/Veri izlenebilirliği (örnek)
Her envanter satırı bir kanon-bölümüne veya `.asset` dosyasına dayanır. Örnekler: 12 birim → `Assets/_Game/Data/Units/*.asset` (stat-teyitli) · 12 büyü → `Assets/_Game/Data/Spells/*.asset` · 2 komutan → `cmd_*.asset` · 3 harita terrain → `map_*.asset` (yerleşim-teyitli) · 8 heykel durumu → `StatuePhase` enum · 7 statü → `StatusKind` enum · 4 arazi → `TerrainKind` enum · kozmetik ×5 → Roadmap §6 + `Art/README.md`.

### B. Art Director'a açık ADR/karar kalemleri (STOP-and-ask — §15.6)
1. **[BLOKLAYICI] Yol A (2D Spine) vs Yol B (2.5D 3D)** — tüm tedariki belirler. *Öneri: B.*
2. **5. hasar tipi adı** (`DamageType.Melee` provizyonel) + **4. zırh sınıfı** (`Unarmored` provizyonel) — VFX/zırh-görsel kesinleşmeden önce LSD/ADR onayı (CombatTypes.cs not düşer).
3. **Heykel hasar-eşikleri** görsel karşılığı (Intact>0.66, Cracked>0.33 — StatueDamage.cs) — durum-geçiş VFX zamanlaması.
4. **Kozmetik kademe içeriği** (hangi kademe hangi VFX-renk) — §6 sınırı içinde LP/Art onayı.
5. **VO kapsamı** (§9 "light VO barks") — opsiyonel; bütçe kararı.

### C. Kanon-uyum beyanı
Bu denetim **hiçbir** yeni birim/büyü/fraksiyon/mekanik icat etmedi (§15.1); yalnızca mevcut kanon + implementasyon verisindeki **görsel asset gereksinimlerini** çıkardı, sınıflandırdı, kaynak/entegrasyon önerdi. İhlal-edilemez kısıtlar (okunabilirlik, kozmetik-güvenlik/no-P2W, mobil-perf, fraksiyon-renk kimliği) her bölümde korundu. Kanon-değişikliği gerektiren tek kalem — **2D→2.5D sanat-yönü** — açıkça **ADR olarak işaretlendi**, kararlaştırılmadı (§15.6 STOP-and-ask).

---

*Belge tipi: görsel-üretim envanteri + tedarik/entegrasyon planı. Model/asset üretmez. Kanonik dökümanlara tabidir; çelişkide roadmap kazanır. Üretim öncesi tek bloklayıcı: §13.0 ADR (Yol A vs B).*

