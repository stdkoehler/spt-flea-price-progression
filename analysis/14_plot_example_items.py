"""Plot the 5 item examples from description.md (similar to item_fits.png).

Saves to docs/img/example_items_fit.png with legend:
- 'fitted trend'
- 'live data measurements'
"""
import sys
from pathlib import Path
import warnings
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd

sys.path.insert(0, str(Path(__file__).parent))
import flealib as F

warnings.filterwarnings("ignore")

# Load data
df = F.load()
C = pd.read_pickle(F.OUT / "level_curves.pkl")
names = F.names()
LV = C.index.to_numpy()
fit_days = F.level_to_day(LV)

# The 5 items from description.md:
# 1. Respirator (59e7715586f7742ee5789605)
# 2. Geiger-Muller counter (5672cb724bdc2dc2088b456b)
# 3. Gunpowder "Kite" (590c5a7286f7747884343aea)
# 4. Colt M4A1 5.56x45 assault rifle (5447a9cd4bdc2dbd208b4567)
# 5. Gas station storage room key (5913877a86f774432f15d444)
ITEMS = [
    ("59e7715586f7742ee5789605", "Respirator", "x5.6 (strong inflation)", "#c0392b"),
    ("5672cb724bdc2dc2088b456b", "Geiger-Müller counter", "x3.3 (medium inflation)", "#d35400"),
    ("590c5a7286f7747884343aea", "Gunpowder \"Kite\"", "x1.0 (steady staple)", "#27ae60"),
    ("5447a9cd4bdc2dbd208b4567", "Colt M4A1", "x0.43 (deflation)", "#2980b9"),
    ("5913877a86f774432f15d444", "Gas station key", "x0.03 (extreme crash)", "#8e44ad"),
]

fig, axes = plt.subplots(1, 5, figsize=(20, 4.3))
fig.suptitle("Example Items: Level Progression Trend vs. Live Market Snapshots",
             fontsize=13, weight="bold", y=0.98)

for ci, (iid, short_name, note, col) in enumerate(ITEMS):
    a = axes[ci]
    s = df[iid].dropna()
    dd = F.day_of_season(s.index).to_numpy()
    vv = s.to_numpy()
    
    pre = dd < F.MAP_DAY_LO
    # Pre-flea unlock data points (grayed out)
    a.plot(dd[pre], vv[pre], ".", ms=2.2, color="#bbbbbb", alpha=0.5, zorder=2)
    # Active wipe data points
    pts = a.plot(dd[~pre], vv[~pre], ".", ms=2.6, color=col, alpha=0.35, label="live data measurements", zorder=3)[0]
    # Fitted trend line (level 15 to level 60/79)
    line = a.plot(fit_days[14:], C[iid].to_numpy()[14:], lw=2.6, color="black", label="fitted trend", zorder=4)[0]
    
    # Shaded band for pre-flea days (0 to 10)
    a.axvspan(0, F.MAP_DAY_LO, color="#f4f4f4", zorder=0)
    
    # Dynamic Y limits with percentile clipping for readability
    lo, hi = np.nanpercentile(vv, [1, 99])
    pad = (hi - lo) * 0.12
    a.set_ylim(max(0, lo - pad), hi + pad)
    a.set_xlim(0, 260)
    
    # Formatting
    full_name = names.get(iid, short_name)
    display_name = short_name if len(full_name) > 30 else full_name
    a.set_title(f"{display_name}\n{note}", fontsize=9.5, weight="semibold", pad=12)
    a.grid(alpha=0.25)
    a.tick_params(labelsize=8)
    a.yaxis.set_major_formatter(lambda v, p: f"{v/1000:,.0f}k" if v >= 1000 else f"{v:.0f}")
    a.set_xlabel("season day", fontsize=9)
    if ci == 0:
        a.set_ylabel("price (RUB)", fontsize=9.5, weight="bold")
    
    # Secondary X-axis for player levels
    top = a.secondary_xaxis("top", functions=(F.day_to_level, F.level_to_day))
    top.set_xticks([15, 21, 30, 40, 50, 60])
    top.tick_params(labelsize=7.5, colors="#555")
    if ci == 2:
        top.set_xlabel("player level", fontsize=8.5, color="#555", labelpad=5)
        
    # Legend on first subplot with 'fitted trend' first, then 'live data measurements'
    if ci == 0:
        a.legend(handles=[line, pts], labels=["fitted trend", "live data measurements"],
                 fontsize=8, loc="upper left", markerscale=3, framealpha=0.9)

plt.tight_layout(rect=[0, 0, 1, 0.94])
out_path = F.DOCS_IMG / "example_items_fit.png"
plt.savefig(out_path, dpi=130)
print(f"Saved: {out_path}")
