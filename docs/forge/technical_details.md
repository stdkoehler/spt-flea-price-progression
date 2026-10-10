# Technical Details

## The Data Foundation

The prices we used to model this come from **948 real snapshots** of the live flea market, taken every six hours across the entire 1.0 season (15 November 2025 to 2 August 2026).

Price data comes from [DrakiaXYZ/SPT-LiveFleaPriceDB](https://github.com/DrakiaXYZ/SPT-LiveFleaPriceDB), the same source used by the [Live Flea Prices](https://sp-mod.com/mod/1131/live-flea-prices) mod. Our mod takes a different angle on it: prices adapt to your character level to simulate a season, rather than tracking the live market in real time. The price movement is learned from the snapshots.

## Progression Mapping

To simulate flea market movement over a wipe, seasonal flea prices are mapped to player level: at level 15 you see flea prices as in the beginning of a wipe, at level 79 you see flea prices as in the end of a wipe.

For a typical item, the average price arc looks like this:

| Your level | 15 | 20 | 21 | 40 | 60 | 79 |
|---|---|---|---|---|---|---|
| Price | 100% | 67% | **65%** | 74% | 87% | 87% |

Prices are expensive when everything is sparse at the beginning, bottom out around level 21, and then inflate up to level 60.

At default config the whole season is mapped between level 15 and level **60**. The real season only lasted 250 days, and in that time we approximate live players got to roughly level 50-70. We decided against assuming the end of the season was a level 79 state. Instead we cap the price trend movement from level 60 onward. They just keep drifting day to day. You can move `scalingMaxLevel` to change the level at which the trend movement stops (see the Configuration tab).

The following graph shows the trend movement we fitted to the live data. During the playthrough we have random price drifts and price shocks around this trend.

[image: docs/img/example_items_fit.png]

## Daily Drift & Seed Generation

Prices also **drift from day to day**, independent of leveling, mirroring the spikes and drops seen in real supply and demand. We learned the variance for each item from the live price data. We also see price shocks in the data (instantaneuos large jumps to higher or lower prices). Both are modelled as pseudo-random movements.

The seed for these pseudo-random movements is calculated from your **profile ID and registration date**, so every playthrough sees different market movement.

## Local and FIKA (Co-op) Server Mechanics

Because flea prices in EFT are global across the server, the mod handles single-player and multiplayer sessions as follows:

- **At server startup (no players connected yet):** If you have a single profile on disk, prices initialize to your character's level immediately. If multiple profiles exist (FIKA / multi-profile setup), the server defaults safely to level 15 baseline and waits for an active player to connect.
- **Solo play (Local server or solo on FIKA):** When you log into your profile, the flea market automatically adjusts to your active character level immediately. For single-player, this is the preferred behavior because the economy always reflects whichever character you are currently playing.
- **Co-op play with friends (FIKA):** When multiple players are online simultaneously on a FIKA server, the flea market dynamically scales to the **highest level among all currently active players**.
  - If a Level 15 player is playing and a Level 17 friend joins, the flea market smoothly updates to Level 17 so the economy matches the group's highest progression.
  - If the higher-level player logs off, prices automatically adjust back down after 30 minutes of inactivity.
  - *Consideration for FIKA groups:* In a dedicated co-op group, one could argue the market should be anchored to the highest-level profile existing on the server overall rather than switching dynamically depending on who is actively connected. This would keep the group's shared world progression steady. We have not made a final decision on this yet and are keeping it dynamic for now.
