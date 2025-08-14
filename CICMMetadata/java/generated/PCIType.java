//
// Este archivo ha sido generado por la arquitectura JavaTM para la implantación de la referencia de enlace (JAXB) XML v2.2.8-b130911.1802 
// Visite <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Todas las modificaciones realizadas en este archivo se perderán si se vuelve a compilar el esquema de origen. 
// Generado el: 2020.07.12 a las 10:42:39 PM WEST 
//


package generated;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * Contains PCI/PCI-X/PCIe card information
 * 
 * <p>Clase Java para PCIType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="PCIType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="VendorID">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedShort">
 *               &lt;minInclusive value="1"/>
 *               &lt;maxInclusive value="65534"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="DeviceID">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedShort">
 *               &lt;minInclusive value="1"/>
 *               &lt;maxInclusive value="65534"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Configuration" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="ExpansionROM" type="{}LinearMediaType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PCIType", propOrder = {
    "vendorID",
    "deviceID",
    "configuration",
    "expansionROM"
})
public class PCIType {

    @XmlElement(name = "VendorID")
    protected int vendorID;
    @XmlElement(name = "DeviceID")
    protected int deviceID;
    @XmlElement(name = "Configuration")
    protected DumpType configuration;
    @XmlElement(name = "ExpansionROM")
    protected LinearMediaType expansionROM;

    /**
     * Obtiene el valor de la propiedad vendorID.
     * 
     */
    public int getVendorID() {
        return vendorID;
    }

    /**
     * Define el valor de la propiedad vendorID.
     * 
     */
    public void setVendorID(int value) {
        this.vendorID = value;
    }

    /**
     * Obtiene el valor de la propiedad deviceID.
     * 
     */
    public int getDeviceID() {
        return deviceID;
    }

    /**
     * Define el valor de la propiedad deviceID.
     * 
     */
    public void setDeviceID(int value) {
        this.deviceID = value;
    }

    /**
     * Obtiene el valor de la propiedad configuration.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getConfiguration() {
        return configuration;
    }

    /**
     * Define el valor de la propiedad configuration.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setConfiguration(DumpType value) {
        this.configuration = value;
    }

    /**
     * Obtiene el valor de la propiedad expansionROM.
     * 
     * @return
     *     possible object is
     *     {@link LinearMediaType }
     *     
     */
    public LinearMediaType getExpansionROM() {
        return expansionROM;
    }

    /**
     * Define el valor de la propiedad expansionROM.
     * 
     * @param value
     *     allowed object is
     *     {@link LinearMediaType }
     *     
     */
    public void setExpansionROM(LinearMediaType value) {
        this.expansionROM = value;
    }

}
