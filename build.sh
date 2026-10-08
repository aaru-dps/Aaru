#!/usr/bin/env bash
AARU_VERSION=6.0.0-beta.2
OS_NAME=$(uname)

mkdir -p build

# Create standalone versions
cd Aaru
for conf in Debug Release;
do
 for distro in linux-arm64 linux-arm linux-x64 osx-x64 osx-arm64 win-arm64 win-x64 win-x86;
 do
  dotnet publish -f net10.0 -r ${distro} -c ${conf}

# Package the Linux packages (stopped working)
#  if [[ ${distro} == alpine* ]] || [[ ${distro} == linux* ]]; then
#    dotnet tarball -f net10.0 -r ${distro} -c ${conf} -o ../build
#    dotnet rpm -f net10.0 -r ${distro} -c ${conf} -o ../build
#    dotnet deb -f net10.0 -r ${distro} -c ${conf} -o ../build
#  elif [[ ${distro} == win* ]] || [[ ${distro} == osx* ]]; then
#    dotnet zip -f net10.0 -r ${distro} -c ${conf} -o ../build
#  elif [[ ${distro} == rhel* ]] || [[ ${distro} == sles* ]]; then
#    pkg="rpm"
#  else
#    pkg="deb"
#  fi

  done
done

cd ..

# If we are compiling on Linux, create distro-specific packages
if [[ ${OS_NAME} == Linux ]]; then
 OS_RELEASE=`pcregrep -o1 -e "^ID=(?<distro_id>\w+)" /etc/os-release`

 # Build Arch Linux package
 if [[ ${OS_RELEASE} == arch || ${OS_RELEASE} == cachyos ]]; then
  echo "Building Arch Linux package..."
  tar --exclude-vcs --exclude="*/bin" --exclude="*/obj" --exclude="build" --exclude="pkg/pacman/*/*.tar.xz" \
   --exclude="pkg/pacman/*/src" --exclude="pkg/pacman/*/pkg"  --exclude="pkg/pacman/*/*.tar" \
   --exclude="pkg/pacman/*/*.asc" --exclude="*.user" --exclude=".idea" --exclude=".vs" --exclude=".vscode" \
   --exclude="build.iso" --exclude=".DS_Store" -cvf pkg/pacman/stable/aaru-src-${AARU_VERSION}.tar .
  mv .globalconfig .globalconfig.bak
  cd pkg/pacman/stable
  xz -v9e aaru-src-${AARU_VERSION}.tar
  gpg --armor --detach-sign aaru-src-${AARU_VERSION}.tar.xz
  cp PKGBUILD PKGBUILD.bak
  echo -e \\n >> PKGBUILD
  makepkg -g >> PKGBUILD
  # Build for all architectures: dotnet cross-compiles, so override CARCH per arch
  for pkgarch in x86_64 armv7h aarch64; do
   echo ""
   echo "========================================"
   echo "Building Arch Linux package for ${pkgarch}..."
   echo "========================================"
   MAKEPKG_CONF_ARCH=$(mktemp)
   cat /etc/makepkg.conf > "${MAKEPKG_CONF_ARCH}"
   echo "CARCH=${pkgarch}" >> "${MAKEPKG_CONF_ARCH}"
   makepkg --config "${MAKEPKG_CONF_ARCH}" 2>&1 | tee ../../../build/pacman-build-${pkgarch}.log || \
    echo "WARNING: Arch Linux package build for ${pkgarch} failed, but continuing..."
   rm -f "${MAKEPKG_CONF_ARCH}"
  done
  mv PKGBUILD.bak PKGBUILD
  mv aaru-src-${AARU_VERSION}.tar.xz aaru-src-${AARU_VERSION}.tar.xz.asc ../../../build
  cd ../../..
  mv .globalconfig.bak .globalconfig
  mv pkg/pacman/stable/*.pkg.tar.zst build/ 2>/dev/null || true
 fi

 # Build Debian packages for all architectures (on any distro if tools are available)
 if command -v dpkg-buildpackage &> /dev/null; then
  echo "Building Debian packages for all architectures..."

  # Create a temporary debian directory symlink for building
  if [ ! -L debian ]; then
   ln -s pkg/debian debian
  fi

  # Determine which tool to use
  if command -v debuild &> /dev/null; then
   BUILD_CMD="debuild"
   echo "Using debuild to build Debian packages"
  else
   BUILD_CMD="dpkg-buildpackage"
   echo "Using dpkg-buildpackage to build Debian packages"
  fi

  # Build for all three architectures
  for arch in amd64 arm64 armhf; do
   echo ""
   echo "========================================"
   echo "Building Debian package for ${arch}..."
   echo "========================================"

   # Build the package
   if [ "$BUILD_CMD" == "debuild" ]; then
    debuild -a ${arch} -us -uc -b 2>&1 | tee build/debian-build-${arch}.log || {
     echo "WARNING: Debian package build for ${arch} failed, but continuing..."
     pkg/debian/rules clean 2>/dev/null || true
     continue
    }
   else
    dpkg-buildpackage -a ${arch} -us -uc -b 2>&1 | tee build/debian-build-${arch}.log || {
     echo "WARNING: Debian package build for ${arch} failed, but continuing..."
     pkg/debian/rules clean 2>/dev/null || true
     continue
    }
   fi

   # Move generated packages to build directory
   if [ -f ../aaru_*_${arch}.deb ]; then
    mv ../aaru_*_${arch}.deb build/ 2>/dev/null || true
    echo "✓ Debian package for ${arch} moved to build directory"
   fi

   # Clean up build artifacts
   mv ../*.changes build/ 2>/dev/null || true
   mv ../*.buildinfo build/ 2>/dev/null || true
   rm -f ../*.dsc 2>/dev/null || true

   # Clean between builds
   if [ -f pkg/debian/rules ]; then
    pkg/debian/rules clean 2>/dev/null || true
   fi
  done

  # Remove temporary symlink
  rm -f debian 2>/dev/null || true

  echo ""
  echo "========================================"
  echo "Debian package build summary:"
  ls -lh build/aaru_*.deb 2>/dev/null || echo "No .deb files found"
  echo "========================================"
 else
  if [[ ${OS_RELEASE} == debian || ${OS_RELEASE} == ubuntu || ${OS_RELEASE} == linuxmint || ${OS_RELEASE} == pop || ${OS_RELEASE} == elementary ]]; then
   echo "dpkg-buildpackage not found, skipping Debian package build"
   echo "Install with: sudo apt-get install dpkg-dev debhelper"
  elif [[ ${OS_RELEASE} == arch || ${OS_RELEASE} == cachyos ]]; then
   echo "dpkg-buildpackage not found, skipping Debian package build"
   echo "Install with: sudo pacman -S dpkg debhelper fakeroot"
  fi
 fi

 # Build RPM packages for all architectures (on any distro if tools are available)
 if command -v rpmbuild &> /dev/null; then
  echo ""
  echo "Building RPM packages for all architectures..."

  # Set up RPM build environment
  RPMBUILD_DIR="${HOME}/rpmbuild"
  mkdir -p "${RPMBUILD_DIR}"/{BUILD,RPMS,SOURCES,SPECS,SRPMS}

  # Create source tarball
  echo "Creating source tarball for RPM..."
  # RPM uses tilde for pre-release versions, transform dash to tilde
  RPM_VERSION="${AARU_VERSION/-/\~}"
  tar --exclude-vcs --exclude="*/bin" --exclude="*/obj" --exclude="build" \
      --exclude="pkg/pacman/*/*.tar*" --exclude="pkg/pacman/*/src" --exclude="pkg/pacman/*/pkg" \
      --exclude="pkg/pacman/*/*.asc" --exclude="*.user" --exclude=".idea" --exclude=".vs" \
      --exclude=".vscode" --exclude="build.iso" --exclude=".DS_Store" --exclude="nupkgs" \
      --exclude="BenchmarkDotNet.Artifacts" --exclude=".fleet" \
      -czf "${RPMBUILD_DIR}/SOURCES/aaru-${RPM_VERSION}.tar.gz" \
      --transform="s,^,aaru-${RPM_VERSION}/," .

  # Verify the tarball was created with the correct name
  if [ ! -f "${RPMBUILD_DIR}/SOURCES/aaru-${RPM_VERSION}.tar.gz" ]; then
   echo "ERROR: Source tarball was not created with expected name"
   echo "Expected: ${RPMBUILD_DIR}/SOURCES/aaru-${RPM_VERSION}.tar.gz"
   echo "Found in SOURCES/:"
   ls -la "${RPMBUILD_DIR}/SOURCES/" || true
   exit 1
  fi
  echo "✓ Source tarball created: aaru-${RPM_VERSION}.tar.gz"

  # Copy spec file
  cp pkg/rpm/aaru.spec "${RPMBUILD_DIR}/SPECS/"

  # Build for all three architectures
  for arch in x86_64 aarch64 armv7hl; do
   echo ""
   echo "========================================"
   echo "Building RPM package for ${arch}..."
   echo "========================================"

   # Build the RPM
   rpmbuild -bb --target ${arch} "${RPMBUILD_DIR}/SPECS/aaru.spec" 2>&1 | tee build/rpm-build-${arch}.log || {
    echo "WARNING: RPM package build for ${arch} failed, but continuing..."
    continue
   }

   # Move generated packages to build directory
   if [ -f "${RPMBUILD_DIR}/RPMS/${arch}/aaru-"*.rpm ]; then
    cp "${RPMBUILD_DIR}/RPMS/${arch}/aaru-"*.rpm build/ 2>/dev/null || true
    echo "✓ RPM package for ${arch} moved to build directory"
   fi
  done

  echo ""
  echo "========================================"
  echo "RPM package build summary:"
  ls -lh build/aaru-*.rpm 2>/dev/null || echo "No .rpm files found"
  echo "========================================"
 else
  if [[ ${OS_RELEASE} == fedora || ${OS_RELEASE} == rhel || ${OS_RELEASE} == rocky || ${OS_RELEASE} == almalinux || ${OS_RELEASE} == centos ]]; then
   echo "rpmbuild not found, skipping RPM package build"
   echo "Install with: sudo dnf install rpm-build rpmdevtools"
  elif [[ ${OS_RELEASE} == opensuse* || ${OS_RELEASE} == sles ]]; then
   echo "rpmbuild not found, skipping RPM package build"
   echo "Install with: sudo zypper install rpm-build"
  elif [[ ${OS_RELEASE} == arch || ${OS_RELEASE} == cachyos ]]; then
   echo "rpmbuild not found, skipping RPM package build"
   echo "Install with: sudo pacman -S rpm-tools"
  fi
 fi
fi

# Remove stray files from published folders
rm -R Aaru/bin/*/net10.0/*/publish/BuildHost-net* Aaru/bin/*/net10.0/*/publish/*.xml Aaru/bin/*/net10.0/*/publish/*.pdb

# Package tarballs
cd Aaru/bin/Debug/net10.0/linux-arm/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_armhf-dbg.tar -- *
cd ../../linux-arm64/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_arm64-dbg.tar -- *
cd ../../linux-x64/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_amd64-dbg.tar -- *
cd ../../osx-arm64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_macos_aarch64-dbg.zip *
cd ../../osx-x64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_macos-dbg.zip *
cd ../../win-arm64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_aarch64-dbg.zip *
cd ../../win-x64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_x64-dbg.zip *
cd ../../win-x86/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_x86-dbg.zip *
cd ../../../../Release/net10.0/linux-arm/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_armhf.tar -- *
cd ../../linux-arm64/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_arm64.tar -- *
cd ../../linux-x64/publish/
tar cvf ../../../../../../build/aaru-${AARU_VERSION}_linux_amd64.tar -- *
cd ../../osx-arm64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_macos_aarch64.zip *
cd ../../osx-x64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_macos.zip *
cd ../../win-arm64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_aarch64.zip *
cd ../../win-x64/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_x64.zip *
cd ../../win-x86/publish/
7z a ../../../../../../build/aaru-${AARU_VERSION}_windows_x86.zip *
cd ../../../../../../

cd build
xz -9e -- *.tar
for i in *.deb *.rpm *.zip *.tar.xz *.pkg.tar.zst;
do
 gpg --armor --detach-sign "$i"
done

cd ..
rm -Rf build/macos/Aaru.app
mkdir -p build/macos/Aaru.app/Contents/Resources
mkdir -p build/macos/Aaru.app/Contents/MacOS
cp Aaru/Aaru.icns build/macos/Aaru.app/Contents/Resources
cp Aaru/Info.plist build/macos/Aaru.app/Contents
cp -r Aaru/bin/Release/net10.0/osx-x64/publish/* build/macos/Aaru.app/Contents/MacOS
rm -Rf build/macos-dbg/Aaru.app
mkdir -p build/macos-dbg/Aaru.app/Contents/Resources
mkdir -p build/macos-dbg/Aaru.app/Contents/MacOS
cp Aaru/Aaru.icns build/macos-dbg/Aaru.app/Contents/Resources
cp Aaru/Info.plist build/macos-dbg/Aaru.app/Contents
cp -r Aaru/bin/Debug/net10.0/osx-x64/publish/* build/macos-dbg/Aaru.app/Contents/MacOS
