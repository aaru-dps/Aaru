# Maintainer: Natalia Portillo <claunia@claunia.com>
packager='Natalia Portillo <claunia@claunia.com>'
pkgname='discimagechef'
pkgver=4.5.1.1692
pkgrel=1
pkgdesc='Disc image management and creation tool for disks, tapes, optical and solid state media'
arch=('any')
url='http://www.discimagechef.app'
license=('GPL')
source=('https://github.com/discimagechef/DiscImageChef/releases/download/v4.5.1.1692/DiscImageChef-4.5.1.1692.zip'
        'https://github.com/discimagechef/DiscImageChef/releases/download/v4.5.1.1692/DiscImageChef-4.5.1.1692.zip.asc'
        'discimagechef.sh')
depends=('mono')
options=('!strip')
sha256sums=('f0eeadb1c963e26f6b661943dd73c070e469a27c143e11b7bdf59b1da47cb37a'
            'SKIP'
            'f55f4b5a861856473b21edd5ee7edd3605bf225186af1fdffad9b553789542bb')
validpgpkeys=('236F1E21B540FC0D40F7AD1751D20488C724CA9F')
provides=('discimagechef')

package() {
    cd "${srcdir}"

    # Create destination directory
    install -d -m0755 -g 0 "${pkgdir}"/opt/DiscImageChef

    # Copy .NET binary
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef DiscImageChef.exe

    # Copy .NET dependencies
    install -m0755 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.dll

    # Copy .NET configuration files
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.config

    # Copy documentation
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef *.md
    install -m0644 -g 0 -t "${pkgdir}"/opt/DiscImageChef LICENSE*

    # Install launcher
    install -d -m0755 -g 0 "${pkgdir}"/usr/bin
    install -m0755 -g 0 -T discimagechef.sh "${pkgdir}"/usr/bin/discimagechef
}
