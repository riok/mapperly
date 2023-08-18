#!/usr/bin/env bash

set -Eeuo pipefail

# pack a nupkg for each roslyn version that is supported by Mapperly
# and merge them together into one nupkg

roslyn_versions=('4.0' '4.4' '4.5')

RELEASE_VERSION=${RELEASE_VERSION:-"0.0.1-dev.$(date +%s)"}
RELEASE_NOTES=${RELEASE_NOTES:-''}

# https://stackoverflow.com/a/246128/3302887
script_dir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)
artifacts_dir="${script_dir}/../artifacts"

echo "building Mapperly v${RELEASE_VERSION}"
echo "cleaning artifacts dir"
mkdir -p "${artifacts_dir}"
rm -rf "${artifacts_dir:?}"/*

artifacts_dir="$(realpath "$artifacts_dir")"
source_generator_path="$(realpath "${script_dir}/../src/Riok.Mapperly")"

for roslyn_version in "${roslyn_versions[@]}"; do
    echo "building for Roslyn ${roslyn_version}"
    dotnet pack \
        "$source_generator_path" \
        --verbosity quiet \
        -c Release \
        /p:ROSLYN_VERSION="${roslyn_version}" \
        -o "${artifacts_dir}/roslyn-${roslyn_version}" \
        /p:Version="${RELEASE_VERSION}" \
        /p:PackageReleaseNotes=\""${RELEASE_NOTES}"\"
done

echo "merging multi targets to a single nupkg"
zipmerge "${artifacts_dir}/Riok.Mapperly.${RELEASE_VERSION}.nupkg" "${artifacts_dir}"/*/*.nupkg
echo "built ${artifacts_dir}/Riok.Mapperly.${RELEASE_VERSION}.nupkg"
