#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"
export_dir="$(mktemp -d "${TMPDIR:-/tmp}/little-legion-export.XXXXXX")"
trap 'rm -rf "$export_dir"' EXIT
swiftc -module-cache-path "$export_dir/cache" export.swift -o "$export_dir/export"
mkdir -p upload app-icons
"$export_dir/export" masters/hero-cover.png upload/hero-cover.png 3000 900
"$export_dir/export" masters/cover-landscape.png upload/cover-landscape.png 2560 1440
"$export_dir/export" masters/cover-square.png upload/cover-square.png 1440 1440
"$export_dir/export" masters/cover-portrait.png upload/cover-portrait.png 1008 1440
"$export_dir/export" masters/hero-cover.png upload/mini-landscape.png 1080 360
"$export_dir/export" masters/app-icon.png upload/icon-512.png 512 512
"$export_dir/export" masters/logo-transparent.png upload/logo-transparent.png 2880 1440 alpha
for size in 48 72 96 144 192 256 512 1024; do
  "$export_dir/export" masters/app-icon.png "app-icons/little-legion-${size}.png" "$size" "$size"
done
