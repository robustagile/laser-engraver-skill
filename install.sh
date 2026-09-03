#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# Deploys the laser-engraver plugin into a .claude directory.
#
# It copies files and does nothing else. That is deliberate and it is the
# reason this is a shell script rather than a .NET program: the .NET SDK is
# exactly the thing that may not be installed yet, so the installer cannot be
# written in it. The SDK check belongs to the skill, on first use. (R-N8)
#
# Two rules it must never break:
#
#   1. Everything under skills/ and commands/ is disposable and is replaced
#      wholesale. The data store beside the target belongs to the user and is
#      never read, written or looked into by this script. A recipe base is
#      weeks of test burns; an installer that reached into it would destroy
#      the one thing the plugin exists to build. (R-N7)
#
#   2. Re-running it is safe, and it says what it changed. (R-N7)
#
# Usage: ./install.sh <target> [--link]
# ---------------------------------------------------------------------------
set -euo pipefail

SOURCE="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PAYLOAD="$SOURCE/plugin"
SKILLS=(laser-machines laser-lightburn)
COMMANDS=(laser-machine laser-recipes laser-job laser-fix)
DATA_DIR=laser-skill-data

usage() {
  cat <<'USAGE'
Usage: ./install.sh <target> [--link]

  <target>   the .claude directory to install into:
               ~/.claude            available in every directory
               <project>/.claude    confined to one working folder

  --link     developer mode: symlink the skills to this clone instead of
             copying them, so an edit here takes effect with no re-install.
             Not for normal use - `git pull` then changes the installed
             plugin underneath you, which is the point of it.

Your machine records, recipes and burn results live in laser-skill-data beside
<target>, and are never touched. Re-run this to update.
USAGE
}

# --- arguments ------------------------------------------------------------

TARGET=""
LINK=0
for arg in "$@"; do
  case "$arg" in
    --link)     LINK=1 ;;
    -h|--help)  usage; exit 0 ;;
    -*)         echo "install.sh: unknown option $arg" >&2; usage >&2; exit 2 ;;
    *)
      if [ -n "$TARGET" ]; then
        echo "install.sh: more than one target given ($TARGET, $arg)" >&2
        exit 2
      fi
      TARGET="$arg" ;;
  esac
done

if [ -z "$TARGET" ]; then
  echo "install.sh: no target given." >&2
  usage >&2
  exit 2
fi

# --- sanity: are we in a clone, and is the target plausible? ---------------

for skill in "${SKILLS[@]}"; do
  if [ ! -f "$PAYLOAD/skills/$skill/SKILL.md" ]; then
    echo "install.sh: $PAYLOAD/skills/$skill/SKILL.md is missing." >&2
    echo "            Run this script from the repository it came with." >&2
    exit 1
  fi
done

mkdir -p -- "$TARGET"
TARGET="$(cd -- "$TARGET" && pwd)"

# The store is the target's sibling, not something inside it: Claude Code guards
# writes anywhere under a .claude directory, a permissions rule in settings does
# not lift that guard, and a store in there could not be written without an
# interactive approval every session - nor at all from an automated one.
# Measured, not assumed.
STORE="$(dirname -- "$TARGET")/$DATA_DIR"

if [ "$(basename -- "$TARGET")" != ".claude" ]; then
  echo "Note: $TARGET is not named .claude. Installing there anyway - but if"
  echo "      that was a slip, nothing you meant to keep has been touched yet."
  echo
fi

# --- what we are installing, so the report can name it --------------------

git_in_source() { git -C "$SOURCE" "$@" 2>/dev/null; }

NEW_COMMIT="$(git_in_source rev-parse --short HEAD || echo unknown)"
NEW_STATE=clean
if [ "$NEW_COMMIT" = unknown ]; then
  NEW_STATE=unknown
elif [ -n "$(git_in_source status --porcelain -- plugin)" ]; then
  NEW_STATE=modified
fi

version_of() {  # the commit recorded by a previous install, if any
  local file="$1/VERSION"
  [ -f "$file" ] || return 0
  sed -n 's/^commit:[[:space:]]*//p' "$file" | head -1
}

describe_new() {
  if [ "$NEW_STATE" = modified ]; then echo "$NEW_COMMIT, working tree modified"
  else echo "$NEW_COMMIT"; fi
}

echo "Installing the laser-engraver plugin"
echo
echo "  source   $SOURCE  ($(describe_new))"
if [ "$TARGET" = "$HOME/.claude" ]; then
  echo "  target   $TARGET  (user level)"
else
  echo "  target   $TARGET  (project level)"
fi
echo

# --- the skills: replaced wholesale ---------------------------------------

mkdir -p -- "$TARGET/skills" "$TARGET/commands"

for skill in "${SKILLS[@]}"; do
  dest="$TARGET/skills/$skill"
  was="$(version_of "$dest")"

  # Guard the removal on the name we just built, never on a variable that
  # could have come out empty.
  case "$dest" in
    */skills/"$skill") rm -rf -- "$dest" ;;
    *) echo "install.sh: refusing to remove $dest" >&2; exit 1 ;;
  esac

  if [ "$LINK" = 1 ]; then
    ln -s -- "$PAYLOAD/skills/$skill" "$dest"
    printf '  %-26s linked to the clone\n' "skills/$skill"
    continue
  fi

  cp -R -- "$PAYLOAD/skills/$skill" "$dest"

  # Build output is not payload: the generator is built from source on the
  # user's machine, and a stale bin/ carried over from here would be the one
  # copy nobody thought to look at.
  find "$dest" -type d \( -name bin -o -name obj \) -prune -exec rm -rf -- {} +

  cat > "$dest/VERSION" <<EOF
commit:    $NEW_COMMIT
state:     $NEW_STATE
installed: $(date -u '+%Y-%m-%dT%H:%M:%SZ')
source:    $SOURCE
EOF

  if   [ -z "$was" ];                 then note="installed"
  elif [ "$was" != "$NEW_COMMIT" ];   then note="updated from $was"
  elif [ "$NEW_STATE" = modified ];   then note="re-copied (same commit, working tree modified)"
  else                                     note="re-copied (already $NEW_COMMIT)"
  fi
  printf '  %-26s %s\n' "skills/$skill" "$note"
done

# --- the commands ---------------------------------------------------------

for cmd in "${COMMANDS[@]}"; do
  src="$PAYLOAD/commands/$cmd.md"
  dest="$TARGET/commands/$cmd.md"
  if   [ ! -f "$dest" ];        then note="installed"
  elif cmp -s -- "$src" "$dest"; then note="unchanged"
  else                               note="updated"
  fi
  [ "$note" = unchanged ] || cp -- "$src" "$dest"
  printf '  %-26s %s\n' "commands/$cmd.md" "$note"
done

# A laser-*.md we did not just write is either from a version that had a
# command this one does not, or the user's own. Both are their call, so say so
# rather than deleting someone's file.
shopt -s nullglob
for stale in "$TARGET"/commands/laser-*.md; do
  keep=0
  for cmd in "${COMMANDS[@]}"; do
    [ "$(basename -- "$stale")" = "$cmd.md" ] && keep=1
  done
  if [ "$keep" = 0 ]; then
    printf '  %-26s left in place - not part of this version. Delete it if it\n' "commands/$(basename -- "$stale")"
    printf '  %-26s came from an older install.\n' ""
  fi
done
shopt -u nullglob

# --- the user's data directory: named, never touched ----------------------

echo
if [ -d "$STORE" ]; then
  echo "Your data directory is where it was, untouched:"
  echo "  $STORE"
else
  echo "Your data will live in $STORE"
  echo "  - it does not exist yet; the skill creates it the first time it has"
  echo "    something of yours to write."
fi

# --- project level: keep records out of the project's history -------------

if [ "$TARGET" != "$HOME/.claude" ]; then
  repo="$(git -C "$TARGET" rev-parse --show-toplevel 2>/dev/null || true)"
  if [ -n "$repo" ]; then
    rel="${STORE#"$repo"/}/"
    # The trailing slash matters: a directory-only pattern is not matched by
    # check-ignore unless the queried path carries one, and this directory
    # usually does not exist yet at install time.
    if ! git -C "$repo" check-ignore -q "$STORE/" 2>/dev/null; then
      echo
      echo "Your data store lands in a git repository, at its root. Machine"
      echo "records and burn results do not belong in a project's history."
      echo "Add this to $repo/.gitignore:"
      echo
      echo "  $rel"
    fi
  fi
fi

echo
if [ "$LINK" = 1 ]; then
  echo "Done - developer mode. The skills point at this clone, so edits here are"
  echo "live and a git pull changes the installed plugin."
else
  echo "Done. To update: git pull here, then run this again with the same target."
fi
