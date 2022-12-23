#!/usr/bin/env bash
AARU_VERSION=5.3.2
OS_NAME=`uname`

mkdir -p build

# Create standalone versions
cd Aaru
for conf in Debug Release;
do
 for distro in alpine-x64 linux-arm64 linux-arm linux-x64 osx-x64 win-arm64 win-arm win-x64 win-x86 debian-arm debian-arm64 debian-x64 rhel-arm64 rhel-x64 sles-x64;
 do
  dotnet publish -f netcoreapp3.1 -r ${distro} -c ${conf}

# Package the Linux packages (stopped working)
#  if [[ ${distro} == alpine* ]] || [[ ${distro} == linux* ]]; then
#    pkg="tarball"
#  elif [[ ${distro} == win* ]] || [[ ${distro} == osx* ]]; then
#    pkg="zip"
#  elif [[ ${distro} == rhel* ]] || [[ ${distro} == sles* ]]; then
#    pkg="rpm"
#  else
#    pkg="deb"
#  fi
#  dotnet ${pkg} -f netcoreapp3.1 -r ${distro} -c ${conf} -o ../build
#

# Package the Linux packages using MSBuild
  if [[ ${distro} == alpine* ]] || [[ ${distro} == linux* ]]; then
    task="CreateTarball"
    extension="tar.gz"
  elif [[ ${distro} == win* ]] || [[ ${distro} == osx* ]]; then
    task="CreateZip"
    extension="zip"
  elif [[ ${distro} == rhel* ]] || [[ ${distro} == sles* ]]; then
    task="CreateRpm"
    extension="rpm"
  else
    task="CreateDeb"
    extension="deb"
  fi

  dotnet msbuild Aaru.csproj /t:${task} /p:TargetFramework=netcoreapp3.1 /p:RuntimeIdentifier=${distro} /p:Configuration=${conf}
  mv bin/${conf}/netcoreapp3.1/${distro}/*.${extension} ../build

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
cp -r Aaru/bin/Release/netcoreapp3.1/osx-x64/publish/* build/macos/Aaru.app/Contents/MacOS
rm -Rf build/macos-dbg/Aaru.app
mkdir -p build/macos-dbg/Aaru.app/Contents/Resources
mkdir -p build/macos-dbg/Aaru.app/Contents/MacOS
cp Aaru/Aaru.icns build/macos-dbg/Aaru.app/Contents/Resources
cp Aaru/Info.plist build/macos-dbg/Aaru.app/Contents
cp -r Aaru/bin/Debug/netcoreapp3.1/osx-x64/publish/* build/macos-dbg/Aaru.app/Contents/MacOS
