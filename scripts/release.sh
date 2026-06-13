#!/usr/bin/env bash
set -euo pipefail

# ============================================================================
# Release + Deploy
# ============================================================================
# Cuts a new release and ships it:
#   1. validates a clean tree on the default branch, up to date with origin
#   2. creates an annotated vX.Y.Z tag and pushes it
#      -> the tag push triggers .github/workflows/release.yml, which
#         publishes the GitHub Release with auto-generated notes
#   3. runs the normal deploy (scripts/deploy.sh)
#
# Usage:
#   make release              # auto-bump the patch (v0.1.0 -> v0.1.1)
#   make release v=0.2.0      # explicit version
#   make release bump=minor   # bump minor; bump=major for major
#   make release-only         # tag + GitHub Release, no deploy
# ============================================================================

YELLOW="\033[38;2;255;209;0m"; GREEN="\033[38;2;72;199;142m"
RED="\033[38;2;255;85;85m"; DIM="\033[2m"; BOLD="\033[1m"; RESET="\033[0m"

VERSION_ARG="${VERSION_ARG:-}"
BUMP="${BUMP:-patch}"
DEPLOY=true
for a in "$@"; do [ "$a" = "--no-deploy" ] && DEPLOY=false; done

die() { printf "\n  ${RED}${BOLD}%s${RESET}\n\n" "$1"; exit 1; }

BRANCH="$(git symbolic-ref --short HEAD 2>/dev/null || echo "")"
DEFAULT="$(git symbolic-ref --short refs/remotes/origin/HEAD 2>/dev/null | sed 's#^origin/##' || echo main)"
[ "$BRANCH" = "$DEFAULT" ] || die "Releases must be cut from '$DEFAULT' (on '$BRANCH')."
[ -z "$(git status --porcelain)" ] || die "Working tree is not clean. Commit or stash first."

git fetch --quiet --tags origin || true
if BEHIND="$(git rev-list --count "HEAD..origin/$DEFAULT" 2>/dev/null)" && [ "${BEHIND:-0}" -gt 0 ]; then
  die "Local '$DEFAULT' is $BEHIND commit(s) behind origin. Run 'git pull' first."
fi

LATEST="$(git tag --list 'v*' --sort=-v:refname | head -n1)"; LATEST="${LATEST:-v0.0.0}"
if [ -n "$VERSION_ARG" ]; then
  NEXT="v${VERSION_ARG#v}"
else
  IFS=. read -r MA MI PA <<<"${LATEST#v}"
  case "$BUMP" in
    major) MA=$((MA+1)); MI=0; PA=0 ;;
    minor) MI=$((MI+1)); PA=0 ;;
    patch|*) PA=$((PA+1)) ;;
  esac
  NEXT="v${MA}.${MI}.${PA}"
fi
git rev-parse "$NEXT" >/dev/null 2>&1 && die "Tag $NEXT already exists."

printf "\n  ${YELLOW}${BOLD}Release${RESET}  ${DIM}%s${RESET}  ${BOLD}->${RESET}  ${GREEN}${BOLD}%s${RESET}\n" "$LATEST" "$NEXT"
printf "  ${DIM}branch %s · deploy after: %s${RESET}\n\n" "$BRANCH" "$DEPLOY"

git tag -a "$NEXT" -m "Release $NEXT"
git push origin "$NEXT"
printf "  ${GREEN}tagged + pushed %s${RESET}  ${DIM}(GitHub Release publishing via Actions)${RESET}\n\n" "$NEXT"

if [ "$DEPLOY" = true ]; then
  exec "$(dirname "$0")/deploy.sh"
fi
