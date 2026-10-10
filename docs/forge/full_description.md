# Flea Price Progression

**Flea market prices change as you level up**

Static flea prices get boring quickly, and making money early in a wipe feels way too easy. The great [Live Flea Prices](https://sp-mod.com/mod/1131/live-flea-prices) mod connects to the live market, but that mirrors the current real-world wipe, and does not correlate with your own character's progression.

**Flea Price Progression** bridges that gap: flea market prices adapt to your PMC level, replaying an authentic wipe economy at your own pace.

[image: docs/img/season_overview.png] -> ![image](https://i.imgur.com/oX3iCx0.png)

## Details {.tabset}

### Overview

#### What Does It Feel Like?

When you play with this mod, the flea market evolves naturally as you progress:

- **Level 15 (Flea Unlock):** Fresh-wipe scarcity. Early quest items, starter weapon parts, and hideout materials are expensive. Finding rare keys in raid feels like a real jackpot.
- **Around Level 21:** The post-unlock market crash. As more players flood the market with loot, prices hit their lowest point of the wipe. Great time to buy gear!
- **Levels 22 to 60:** Mid-to-late wipe inflation. As players build wealth and upgrade their hideout, crafting consumables (fuel, respirators) become much more valuable, while common weapons become cheaper.
- **Endgame:** Prices stabilize into late-season balance, keeping the economy fun without endless hyperinflation.

#### Every Item Has Its Own Story

Prices don't just use a flat multiplier. Individual items follow real market behavior:

| Item | Level 15 | Level 60 | Trend |
|---|---|---|---|
| **Respirator** | ₽8,800 | ₽49,200 | x5.6 - strong inflation from hideout crafting |
| **Geiger counter** | ₽27,400 | ₽89,400 | x3.3 - medium inflation |
| **Gunpowder "Kite"** | ₽18,700 | ₽18,700 | x1.0 - steady staple component |
| **Colt M4A1** | ₽111,000 | ₽47,900 | x0.43 - deflation as weapons flood the market |
| **Gas station key** | ₽755,000 | ₽20,300 | x0.03 - early scarcity, then extreme crash |

[image: docs/img/example_items_fit.png] -> ![image](https://i.imgur.com/fmsbgfS.png)

#### A Living Market

Prices also **drift from day to day**, independent of leveling, mirroring natural spikes and drops in supply and demand. Every playthrough has its own unique market movements, so no two wipes feel the same.

[image: docs/img/playthrough.png] -> ![image](https://i.imgur.com/9O4kHSj.png)

*Two different playthroughs showing unique market movement around the underlying trend (black line).*

#### Works for Solo and FIKA (Co-op)

Everything works out of the box with zero setup:

- **Solo Play:** The flea market automatically matches whichever character you are currently playing.
- **Co-op with Friends (FIKA):** When playing together, the market automatically scales to the **highest level among all currently active players**, so the whole group shares the same progression.

#### Safe to Install

- Safe to add to or remove from an existing save at any time. Your character profile is never modified.
- Zero performance impact on raids or server loading.

#### More Information

- For the underlying data source, level mapping details, and server mechanics, see the **Technical Details** tab.
- To tweak price swings, drift speed, or season length, see the **Configuration** tab.

### Installation

1. Download the release.
2. Extract the zip directly into your SPT root (it already contains the folder structure to land at the correct place `SPT_Runtime/user/mods/FleaPriceProgression/`).
3. Start the server.

Safe to add to and remove from an existing profile. Your profile is never touched.

### Configuration

Edit `config.jsonc`:

| Setting | Default | Range | What it does |
|---|---|---|---|
| `enabled` | `true` | - | Turn the mod off without uninstalling it. |
| `intensityExponent` | `1.0` | `0` - `10` | How hard prices react to your level. `0` = no scaling. `1` = as real economy. `2` = increased swing. |
| `troughLevel` | `21` | `15` - `79` | The level where prices hit rock bottom. Above it inflation begins. `18` = steep crash, `25` = gentle decline, `15` = skip crash entirely (inflation only). |
| `scalingMaxLevel` | `60` | `16` - `79` | The level where the season runs out. Above it, prices stop trending and only drift. |
| `priceShockSize` | `4.0` | `1` - `20` | How violent occasional price spikes are. `1` = no shocks, just gentle drift. |
| `noise.enabled` | `true` | - | Whether prices drift day to day at all. |
| `noise.amplitudeScale` | `1.0` | `0` - `10` | How big that daily drift is. `0` = off, `2` = more chaotic. |

Out-of-range values are clamped automatically on load (with a note in the
server log).

Debug settings (day/level mapping, price bounds, drift tuning) are documented
inline in `config.jsonc` and in the full README on GitHub.

[image: docs/img/config_effects.png] -> ![image](https://i.imgur.com/torHjur.png)

#### Known quirks

- **Some prices look odd sometimes.** These are price shocks seen in real flea
  data. Set `priceShockSize` to `1` if you don't like that, or disable noise
  entirely with `noise.enabled = false`.
- **Prices move even when you're not playing.** The drift follows real time.
- **Early-wipe prices on some items (rare keys, quest items) are enormous.**
  Change `startDay` in the debug settings to a later day (e.g. `79`) if you
  don't like that.

### Credits

Price data comes from [DrakiaXYZ/SPT-LiveFleaPriceDB](https://github.com/DrakiaXYZ/SPT-LiveFleaPriceDB), the same source used by the excellent [Live Flea Prices](https://sp-mod.com/mod/1131/live-flea-prices) mod.

{.endtabset}
