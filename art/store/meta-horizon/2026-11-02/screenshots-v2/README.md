# Action screenshot set — v2

Five native 2560 × 1440, 24-bit RGB PNG captures from the runtime renderer in Unity Editor. No generative artwork, extra units, resource grants, marketing overlays or post-processing were added. Enhanced lighting is an existing game setting. The original set remains in screenshots/.

1. Neon battle: 12 friendly and 10 opposing combat units in the selected local battle group.
2. Desert battle: 8 friendly and 6 opposing combat units around an expansion.
3. Industrial army: a local group of 27 military units.
4. Industrial workers: 11 actively gathering or depositing workers near the base.
5. Biological workers: 16 actively gathering or depositing workers near the base.

The two battle images use the shipped replay observer projection. The army and worker images use the ordinary player projection, including its visibility rules. Both sides advance using the normal simulation and Hard AI decisions. No simulation parameters or unit positions were modified for these screenshots.

Each adjacent JSON stores the actual captured PlayerView, camera focus, scale and selection metrics. The figures above count units in the selected local group; objects can overlap or appear at the frame edges.

Reproduce with Frontier.Editor.StoreActionScreenshotCapture.Render. ZIP for upload: art/store/LittleLegion-Meta-Screenshots-v2-2560x1440.zip (five PNG files only).
