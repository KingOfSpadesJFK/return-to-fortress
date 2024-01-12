#!/bin/sh
echo -ne '\033c\033]0;return-to-fortress\a'
base_path="$(dirname "$(realpath "$0")")"
"$base_path/return.x86_64" "$@"
