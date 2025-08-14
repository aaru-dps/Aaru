#!/usr/bin/env bash
AARU_VERSION=5.4.1
OS_NAME=`uname`

mkdir -p build

# Create standalone versions
cd Aaru
for conf in Debug Release;
do
 for distro in linux-arm64 linux-arm linux-x64 osx-x64 osx-arm64 win-arm64 win-x64 win-x86;
 do
  dotnet publish -f net8.0 -r ${distro} -c ${conf}

# Package the Linux packages (stopped working)
  if [[ ${distro} == alpine* ]] || [[ ${distro} == linux* ]]; then
    dotnet tarball -f net8.0 -r ${distro} -c ${conf} -o ../build
    dotnet rpm -f net8.0 -r ${distro} -c ${conf} -o ../build
    dotnet deb -f net8.0 -r ${distro} -c ${conf} -o ../build
  elif [[ ${distro} == win* ]] || [[ ${distro} == osx* ]]; then
    dotnet zip -f net8.0 -r ${distro} -c ${conf} -o ../build
#  elif [[ ${distro} == rhel* ]] || [[ ${distro} == sles* ]]; then
#    pkg="rpm"
#  else
#    pkg="deb"
  fi

  done
done

cd ..

# If we are compiling on Linux check if we are on Arch Linux and then create the Arch Linux package as well
if [[ ${OS_NAME} == Linux ]]; then
 OS_RELEASE=`pcregrep -o1 -e "^ID=(?<distro_id>\w+)" /etc/os-release`

 if [[ ${OS_RELEASE} != arch ]]; then
  exit 0
 fi

 tar --exclude-vcs --exclude="*/bin" --exclude="*/obj" --exclude="build" --exclude="pkg/pacman/*/*.tar.xz" \
  --exclude="pkg/pacman/*/src" --exclude="pkg/pacman/*/pkg"  --exclude="pkg/pacman/*/*.tar" \
  --exclude="pkg/pacman/*/*.asc" --exclude="*.user" --exclude=".idea" --exclude=".vs" --exclude=".vscode" \
  --exclude="build.iso" --exclude=".DS_Store" -cvf pkg/pacman/stable/aaru-src-${AARU_VERSION}.tar .
 cd pkg/pacman/stable
 xz -v9e aaru-src-${AARU_VERSION}.tar
 gpg --armor --detach-sign aaru-src-${AARU_VERSION}.tar.xz
 cp PKGBUILD PKGBUILD.bak
 echo -e \\n >> PKGBUILD
 makepkg -g >> PKGBUILD
 makepkg
 mv PKGBUILD.bak PKGBUILD
 mv aaru-src-${AARU_VERSION}.tar.xz aaru-src-${AARU_VERSION}.tar.xz.asc ../../../build
 cd ../../..

fi

mv pkg/pacman/stable/*.pkg.tar.zst build/

cd build
for i in *.deb *.rpm *.zip *.tar.gz *.pkg.tar.zst;
do
 gpg --armor --detach-sign "$i"
done

cd ..
rm -Rf build/macos/Aaru.app
mkdir -p build/macos/Aaru.app/Contents/Resources
mkdir -p build/macos/Aaru.app/Contents/MacOS
cp Aaru/Aaru.icns build/macos/Aaru.app/Contents/Resources
cp Aaru/Info.plist build/macos/Aaru.app/Contents
cp -r Aaru/bin/Release/net8.0/osx-x64/publish/* build/macos/Aaru.app/Contents/MacOS
rm -Rf build/macos-dbg/Aaru.app
mkdir -p build/macos-dbg/Aaru.app/Contents/Resources
mkdir -p build/macos-dbg/Aaru.app/Contents/MacOS
cp Aaru/Aaru.icns build/macos-dbg/Aaru.app/Contents/Resources
cp Aaru/Info.plist build/macos-dbg/Aaru.app/Contents
cp -r Aaru/bin/Debug/net8.0/osx-x64/publish/* build/macos-dbg/Aaru.app/Contents/MacOS
