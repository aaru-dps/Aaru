#@IgnoreInspection BashAddShebang
# Maintainer: Natalia Portillo <claunia@claunia.com>
_netcoretarget='netcoreapp3.1'
_aarubase='Aaru'
packager='Natalia Portillo <claunia@claunia.com>'
pkgbase='aaru-git'
pkgname=('aaru-git')
pkgver=v5.1.0.3214.r3.g011ec1e3
pkgrel=1
pkgdesc='Disc image management and creation tool for disks, tapes, optical and solid state media'
arch=('x86_64' 'armv7h' 'aarch64')
url='http://www.aaru.app'
license=('GPL')
source=('git://github.com/aaru-dps/Aaru')
makedepends=('dotnet-sdk>=3.1.0' 'git')
options=('!strip' 'staticlibs')
sha256sums=('SKIP')
provides=('aaru')
conflicts=('aaru')
depends=('icu' 'krb5' 'libcurl.so' 'libunwind' 'openssl-1.0' 'zlib')

if [ $arch == 'aarch64' ]; then
    dotnet_rid=linux-arm64
elif [ $arch == 'armv7h' ]; then
    dotnet_rid=linux-arm
else
    dotnet_rid=linux-x64
fi

pkgver() {
  cd "$SRCDEST"/"${_aarubase}"
  git describe --tags --match "v5*" --long | sed 's/\([^-]*-g\)/r\1/;s/-/./g'
}

prepare() {
  cd "${srcdir}"/"${_aarubase}"
  git submodule update --init --checkout --recursive
}

build() {
    cd "${srcdir}"/"${_aarubase}"/Aaru
    dotnet publish -f ${_netcoretarget} -c Debug --self-contained -r ${dotnet_rid}
}

package() {
    # Install MIME database file
    cd "${srcdir}"/"${_aarubase}"/Aaru
    install -d -m0755 -g 0 "${pkgdir}"/usr/share/mime/packages
    install -m0755 -g 0 -t "${pkgdir}"/usr/share/mime/packages aaruformat.xml

    cd "${srcdir}"/"${_aarubase}"/Aaru/bin/Debug/${_netcoretarget}/${dotnet_rid}/publish

    # Create destination directory
    install -d -m0755 -g 0 "${pkgdir}"/opt/Aaru

    # Copy Linux binary
    install -m0755 -g 0 -t "${pkgdir}"/opt/Aaru aaru

    # Copy Linux dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/Aaru *.so
    install -m0755 -g 0 -t "${pkgdir}"/opt/Aaru *.a
    install -m0755 -g 0 -t "${pkgdir}"/opt/Aaru createdump

    # Copy .NET dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/Aaru *.dll

    # Copy .NET configuration files
    install -m0644 -g 0 -t "${pkgdir}"/opt/Aaru *.json

    # Copy documentation files
    install -m0644 -g 0 -t "${pkgdir}"/opt/Aaru *.md
    install -m0644 -g 0 -t "${pkgdir}"/opt/Aaru LICENSE*

    # Copy .NET debug files
    install -m0644 -g 0 -t "${pkgdir}"/opt/Aaru *.pdb

    # Link executable
    install -d -m0755 -g 0 "${pkgdir}"/usr/bin
    ln -sf /opt/Aaru/aaru "${pkgdir}"/usr/bin/aaru
}

post_install() {
  xdg-icon-resource forceupdate --theme hicolor &>/dev/null
  update-mime-database usr/share/mime &>/dev/null
  update-desktop-database -q
}

post_upgrade() {
  post_install "$1"
}

post_remove() {
  post_install "$1"
}
