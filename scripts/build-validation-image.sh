#!/usr/bin/env bash
# Build the default Dockerfile with public-source validation input and no real credentials.
set -euo pipefail
[[ "$EXPECTED_HEAD" =~ ^[0-9a-f]{40}$ ]]
[[ "$GITHUB_RUN_ID" =~ ^[1-9][0-9]*$ && "$GITHUB_RUN_ATTEMPT" =~ ^[1-9][0-9]*$ ]]
test "$(git rev-parse HEAD)" = "$EXPECTED_HEAD"
context="$RUNNER_TEMP/leave-image-context-${GITHUB_RUN_ID}-${GITHUB_RUN_ATTEMPT}"
tag="leave-validation:${GITHUB_RUN_ID}-${GITHUB_RUN_ATTEMPT}"
test ! -e "$context"
test ! -e "$RUNNER_TEMP/leave-image-id"
test ! -e "$RUNNER_TEMP/leave-image-build-started"
test ! -e "$RUNNER_TEMP/leave-image-build-settled"
if docker image inspect "$tag" >/dev/null 2>&1; then
  echo 'Image tag unexpectedly exists; preserve it' >&2
  exit 1
fi
mkdir -m 700 "$context"
printf '%s\n' "$EXPECTED_HEAD" > "$context/.owned-head"
git archive "$EXPECTED_HEAD" | tar -x -C "$context"
# The tracked public NuGet configuration replaces only the disposable context input.
cp -- "$context/nuget.validation.config" "$context/nuget.config"
cmp -- "$context/nuget.validation.config" "$context/nuget.config"
printf '%s\n' "$EXPECTED_HEAD" > "$RUNNER_TEMP/leave-image-build-started"
timeout --signal=TERM --kill-after=10s 1200s docker build --progress=plain \
  --secret id=nuget_username,src=/dev/null \
  --secret id=nuget_password,src=/dev/null \
  --iidfile "$RUNNER_TEMP/leave-image-id" --tag "$tag" \
  --file "$context/Maliev.LeaveService.Api/Dockerfile" "$context" 2>&1 | tee "$RUNNER_TEMP/leave-image-build.log"
id="$(cat "$RUNNER_TEMP/leave-image-id")"
[[ "$id" =~ ^sha256:[0-9a-f]{64}$ ]]
test "$(docker image inspect --format '{{.Id}}' "$tag")" = "$id"
printf '%s\n' "$id" > "$RUNNER_TEMP/leave-image-build-settled"
if grep -E '(^|[[:space:]])warning (CS|NU|MSB|NETSDK)[0-9]+' "$RUNNER_TEMP/leave-image-build.log"; then
  echo 'Compiler or package warnings found' >&2
  exit 1
fi
printf 'Built exact head=%s image=%s with public validation feed; private restore not checked\n' "$EXPECTED_HEAD" "$id"
