# Maintainer: Natalia Portillo <claunia@claunia.com>
_netcoretarget='netcoreapp2.0'
packager='Natalia Portillo <claunia@claunia.com>'
pkgbase='discimagechef'
pkgname=('discimagechef-git' 'discimagechef-gtk-git')
pkgver=v4.5.1.1692.r809.g2e585a2c
pkgrel=1
pkgdesc='Disc image management and creation tool for disks, tapes, optical and solid state media'
arch=('x86_64' 'armv7h' 'aarch64')
url='http://www.discimagechef.app'
license=('GPL')
source=('git://github.com/DiscImageChef/discimagechef'
        )
makedepends=('dotnet-sdk>=2.0.0' 'git')
options=('!strip')
sha256sums=('SKIP')
provides=('discimagechef')
conflicts=('discimagechef')

if [ $arch == 'aarch64' ]; then
    dotnet_rid=linux-arm64
elif [ $arch == 'armv7h' ]; then
    dotnet_rid=linux-arm
else
    dotnet_rid=linux-x64
fi

pkgver() {
  cd "$SRCDEST"
  git describe --long | sed 's/\([^-]*-g\)/r\1/;s/-/./g'
}

prepare() {
  cd "${srcdir}"/"${pkgbase}"
  git submodule update --init --checkout --recursive
}

build() {
    cd "${srcdir}"/"${pkgbase}"
    dotnet restore DiscImageChef.sln
    dotnet build -f ${_netcoretarget} -c Debug DiscImageChef.sln
    dotnet publish -f ${_netcoretarget} -c Debug --self-contained -r ${dotnet_rid} DiscImageChef.sln
    dotnet restore DiscImageChef.Gtk.sln
    dotnet build -f ${_netcoretarget} -c Debug DiscImageChef.Gtk.sln
    dotnet publish -f ${_netcoretarget} -c Debug --self-contained -r ${dotnet_rid} DiscImageChef.Gtk.sln
}

package_discimagechef-git() {
    cd "${srcdir}"/"${pkgbase}"/DiscImageChef/bin/Debug/${_netcoretarget}/${dotnet_rid}/publish

    # Create destination directory
    install -d -m0755 -g 0 "${pkgdir}"/opt/DiscImageChef

    # Copy Linux binary
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef DiscImageChef

    # Copy Linux dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.so
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.a
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef createdump
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef sosdocsunix.txt

    # Copy .NET dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.dll

    # Copy .NET configuration files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.json

    # Copy documentation files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.md
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef LICENSE*

    # Copy .NET debug files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.pdb

    # Link executable
    install -d -m0755 -g 0 "${pkgdir}"/usr/bin
    ln -sf /opt/DiscImageChef/DiscImageChef "${pkgdir}"/usr/bin/dic
}

# TODO: Can optimize so no need to install separately with a depend?
package_discimagechef-gtk-git() {
    depends=('gtk3')

    cd "${srcdir}"/"${pkgbase}"/DiscImageChef.Gtk/bin/Debug/${_netcoretarget}/${dotnet_rid}/publish

    # Create destination directory
    install -d -m0755 -g 0 "${pkgdir}"/opt/DiscImageChef.Gtk

    # Copy Linux binary
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk DiscImageChefGui

    # Copy Linux dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.so
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.a
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk createdump
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk sosdocsunix.txt

    # Copy .NET dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.dll

    # Copy .NET configuration files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.json

    # Copy documentation files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.md
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk LICENSE*

    # Copy .NET debug files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef.Gtk *.pdb

    # Link executable
    install -d -m0755 -g 0 "${pkgdir}"/usr/bin
    ln -sf /opt/DiscImageChef/DiscImageChefGui "${pkgdir}"/usr/bin/dic-gtk
}