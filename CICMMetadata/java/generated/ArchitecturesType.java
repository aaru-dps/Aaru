//
// Este archivo ha sido generado por la arquitectura JavaTM para la implantación de la referencia de enlace (JAXB) XML v2.2.8-b130911.1802 
// Visite <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Todas las modificaciones realizadas en este archivo se perderán si se vuelve a compilar el esquema de origen. 
// Generado el: 2020.07.12 a las 10:42:39 PM WEST 
//


package generated;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * CPU architectures this set is intended to be run on
 *             
 * 
 * <p>Clase Java para ArchitecturesType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="ArchitecturesType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Architecture" maxOccurs="unbounded">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="4004"/>
 *               &lt;enumeration value="4040"/>
 *               &lt;enumeration value="6502"/>
 *               &lt;enumeration value="65816"/>
 *               &lt;enumeration value="8008"/>
 *               &lt;enumeration value="8051"/>
 *               &lt;enumeration value="8080"/>
 *               &lt;enumeration value="8085"/>
 *               &lt;enumeration value="aarch64"/>
 *               &lt;enumeration value="am29000"/>
 *               &lt;enumeration value="amd64"/>
 *               &lt;enumeration value="apx432"/>
 *               &lt;enumeration value="arm"/>
 *               &lt;enumeration value="avr"/>
 *               &lt;enumeration value="avr32"/>
 *               &lt;enumeration value="axp"/>
 *               &lt;enumeration value="clipper"/>
 *               &lt;enumeration value="cray"/>
 *               &lt;enumeration value="esa390"/>
 *               &lt;enumeration value="hobbit"/>
 *               &lt;enumeration value="i86"/>
 *               &lt;enumeration value="i860"/>
 *               &lt;enumeration value="i960"/>
 *               &lt;enumeration value="ia32"/>
 *               &lt;enumeration value="ia64"/>
 *               &lt;enumeration value="m56k"/>
 *               &lt;enumeration value="m6800"/>
 *               &lt;enumeration value="m6801"/>
 *               &lt;enumeration value="m6805"/>
 *               &lt;enumeration value="m6809"/>
 *               &lt;enumeration value="m68k"/>
 *               &lt;enumeration value="m88k"/>
 *               &lt;enumeration value="mcs41"/>
 *               &lt;enumeration value="mcs48"/>
 *               &lt;enumeration value="mips32"/>
 *               &lt;enumeration value="mips64"/>
 *               &lt;enumeration value="msp430"/>
 *               &lt;enumeration value="nios2"/>
 *               &lt;enumeration value="openrisc"/>
 *               &lt;enumeration value="parisc"/>
 *               &lt;enumeration value="pdp1"/>
 *               &lt;enumeration value="pdp10"/>
 *               &lt;enumeration value="pdp11"/>
 *               &lt;enumeration value="pdp7"/>
 *               &lt;enumeration value="pdp8"/>
 *               &lt;enumeration value="pic"/>
 *               &lt;enumeration value="power"/>
 *               &lt;enumeration value="ppc"/>
 *               &lt;enumeration value="ppc64"/>
 *               &lt;enumeration value="prism"/>
 *               &lt;enumeration value="renesasrx"/>
 *               &lt;enumeration value="riscv"/>
 *               &lt;enumeration value="s360"/>
 *               &lt;enumeration value="s370"/>
 *               &lt;enumeration value="sh"/>
 *               &lt;enumeration value="sh1"/>
 *               &lt;enumeration value="sh2"/>
 *               &lt;enumeration value="sh3"/>
 *               &lt;enumeration value="sh4"/>
 *               &lt;enumeration value="sh5"/>
 *               &lt;enumeration value="sh64"/>
 *               &lt;enumeration value="sparc"/>
 *               &lt;enumeration value="sparc64"/>
 *               &lt;enumeration value="transputer"/>
 *               &lt;enumeration value="vax"/>
 *               &lt;enumeration value="we32000"/>
 *               &lt;enumeration value="x32"/>
 *               &lt;enumeration value="z80"/>
 *               &lt;enumeration value="z800"/>
 *               &lt;enumeration value="z8000"/>
 *               &lt;enumeration value="z80000"/>
 *               &lt;enumeration value="zarch"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ArchitecturesType", propOrder = {
    "architecture"
})
public class ArchitecturesType {

    @XmlElement(name = "Architecture", required = true)
    protected List<String> architecture;

    /**
     * Gets the value of the architecture property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the architecture property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getArchitecture().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getArchitecture() {
        if (architecture == null) {
            architecture = new ArrayList<String>();
        }
        return this.architecture;
    }

}
