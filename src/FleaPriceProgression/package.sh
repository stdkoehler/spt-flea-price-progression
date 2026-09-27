#!/usr/bin/env bash
# Builds the mod in Release and zips it up as a ready-to-extract SPT_Runtime tree
# (SPT_Runtime/user/mods/FleaPriceProgression/...), the same layout DrakiaXYZ's SPT mods use so a
# release can be extracted straight onto an SPT install.
set -euo pipefail

# Ensure dotnet in PATH on Windows Git Bash environments if not already present
export PATH="/c/Program Files/dotnet:$PATH"

cd "$(dirname "${BASH_SOURCE[0]}")"

# Terminal colors if running in a terminal
if [ -t 1 ] || [ -t 2 ]; then
    RED='\033[0;31m'
    BOLD_RED='\033[1;31m'
    GREEN='\033[0;32m'
    BOLD_GREEN='\033[1;32m'
    YELLOW='\033[1;33m'
    BOLD_YELLOW='\033[1;33m'
    CYAN='\033[0;36m'
    BOLD_CYAN='\033[1;36m'
    BOLD='\033[1m'
    NC='\033[0m'
else
    RED=''
    BOLD_RED=''
    GREEN=''
    BOLD_GREEN=''
    YELLOW=''
    BOLD_YELLOW=''
    CYAN=''
    BOLD_CYAN=''
    BOLD=''
    NC=''
fi

MOD_NAME=FleaPriceProgression
VERSION=$(sed -n 's/.*SemanticVersioning\.Version Version { get; init; } = new("\(.*\)").*/\1/p' FleaPriceProgressionMetadata.cs)
if [ -z "$VERSION" ]; then
    echo "" >&2
    echo -e "${BOLD_RED}################################################################################${NC}" >&2
    echo -e "${BOLD_RED}# ERROR: Could not read version from FleaPriceProgressionMetadata.cs${NC}" >&2
    echo -e "${BOLD_RED}################################################################################${NC}\n" >&2
    exit 1
fi

echo -e "${BOLD_CYAN}================================================================================${NC}"
echo -e "${BOLD_CYAN}==> Packaging ${MOD_NAME} v${VERSION}${NC}"
echo -e "${BOLD_CYAN}================================================================================${NC}"
echo ""

TEST_DIR="../../tests/NoiseVectorTest"
echo "================================================================================"
echo "==> Running test suite (${TEST_DIR})..."
echo "================================================================================"

if [ ! -d "${TEST_DIR}" ]; then
    echo "" >&2
    echo -e "${BOLD_RED}################################################################################${NC}" >&2
    echo -e "${BOLD_RED}# ERROR: Test directory not found!${NC}" >&2
    echo -e "${BOLD_RED}# Expected at: ${TEST_DIR}${NC}" >&2
    echo -e "${BOLD_RED}################################################################################${NC}\n" >&2
    exit 1
fi

set +e
(cd "${TEST_DIR}" && dotnet run)
TEST_EXIT_CODE=$?
set -e

if [ "${TEST_EXIT_CODE}" -ne 0 ]; then
    echo "" >&2
    echo -e "${BOLD_RED}################################################################################${NC}" >&2
    echo -e "${BOLD_RED}# FATAL: TEST SUITE FAILED (exit code: ${TEST_EXIT_CODE})${NC}" >&2
    echo -e "${BOLD_RED}# Packaging ABORTED to prevent creating a broken release.${NC}" >&2
    echo -e "${BOLD_RED}################################################################################${NC}" >&2
    echo -e "${RED}The test suite in ${TEST_DIR} returned a non-zero exit code.${NC}" >&2
    echo -e "${RED}Please inspect the failure output above and resolve all issues before packaging.${NC}" >&2
    echo -e "${BOLD_RED}################################################################################${NC}\n" >&2
    exit "${TEST_EXIT_CODE}"
fi

echo ""
echo -e "${BOLD_GREEN}================================================================================${NC}"
echo -e "${BOLD_GREEN}✓ All tests passed successfully!${NC}"
echo -e "${BOLD_GREEN}================================================================================${NC}"
echo ""

echo "================================================================================"
echo "==> Building ${MOD_NAME} (Release)..."
echo "================================================================================"

set +e
dotnet build -c Release
BUILD_EXIT_CODE=$?
set -e

if [ "${BUILD_EXIT_CODE}" -ne 0 ]; then
    echo "" >&2
    echo -e "${BOLD_RED}################################################################################${NC}" >&2
    echo -e "${BOLD_RED}# FATAL: RELEASE BUILD FAILED (exit code: ${BUILD_EXIT_CODE})${NC}" >&2
    echo -e "${BOLD_RED}# Packaging ABORTED.${NC}" >&2
    echo -e "${BOLD_RED}################################################################################${NC}\n" >&2
    exit "${BUILD_EXIT_CODE}"
fi

PACKAGE_DIR=Package
STAGE_ROOT="${PACKAGE_DIR}/SPT_Runtime"
STAGE_DIR="${STAGE_ROOT}/user/mods/${MOD_NAME}"
ARCHIVE="${PACKAGE_DIR}/${MOD_NAME}-${VERSION}.zip"

echo ""
echo "================================================================================"
echo "==> Creating release archive (${ARCHIVE})..."
echo "================================================================================"

rm -rf "${STAGE_ROOT}"
mkdir -p "${STAGE_DIR}"
cp -r "bin/Release/${MOD_NAME}/." "${STAGE_DIR}/"

rm -f "${ARCHIVE}"
(cd "${PACKAGE_DIR}" && zip -rq "$(basename "${ARCHIVE}")" SPT_Runtime)

echo ""
echo -e "${BOLD_GREEN}================================================================================${NC}"
echo -e "${BOLD_GREEN}✓ Successfully packaged ${MOD_NAME} v${VERSION}!${NC}"
echo -e "${BOLD_GREEN}  Archive: ${ARCHIVE}${NC}"
echo -e "${BOLD_GREEN}================================================================================${NC}"
echo ""
