# Little Legion website

Static, dependency-free English landing and `/privacy`, `/support`, `/tos` pages.
Contact: `support@littlelegion.app`. Public identity: Little Legion.

## Unified stack

`compose.online.yml` is the only runtime Compose file. It starts PostgreSQL,
Nakama, the dedicated multiplayer server and Caddy serving this website.

On the configured production host run `bin/deploy` from the repository root.
It pulls the current branch, builds images, backs up and restore-tests the
existing database, migrates and starts all services. See
[`docs/deploy-production.md`](../docs/deploy-production.md) for prerequisites.

The stack serves `https://littlelegion.app` and proxies the game API at
`https://server-us-1.littlelegion.app`. Both DNS records must reach the host.
Caddy keeps certificates in the existing persistent volumes. Port 8090 binds
only to loopback for the local web preview at http://localhost:8090.

There is no separate web Compose or production override. The old ignored
`.cache/online/compose.prod.yml` and Caddyfile are no longer loaded. Review any
customizations there before migrating. The website is a read-only bind mount
in Caddy and can later move to another host without changing the game code.

Mail links open the visitor’s email app. The support mailbox is provisioned
separately from Docker. Public DNS and TLS need verification on the host.

## Content and assets

The homepage design and artwork come from `dev:landing/`, adapted to English
and served at `/`. The original layout, faction cards and platform sections
are preserved. Privacy, support and terms share its header, footer and palette.

Edit `web/public/index.html` and `styles.css` directly; supporting pages also
use `documents.css`. No build step. The hero is labeled as concept art.
Keep policy text aligned with actual game and hosting behavior.
