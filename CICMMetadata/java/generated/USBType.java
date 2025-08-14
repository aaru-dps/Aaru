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
 * Contains USB device information
 * 
 * <p>Clase Java para USBType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="USBType">
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
 *         &lt;element name="ProductID">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedShort">
 *               &lt;minInclusive value="1"/>
 *               &lt;maxInclusive value="65534"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Descriptors" type="{}DumpType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "USBType", propOrder = {
    "vendorID",
    "productID",
    "descriptors"
})
public class USBType {

    @XmlElement(name = "VendorID")
    protected int vendorID;
    @XmlElement(name = "ProductID")
    protected int productID;
    @XmlElement(name = "Descriptors")
    protected DumpType descriptors;

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
     * Obtiene el valor de la propiedad productID.
     * 
     */
    public int getProductID() {
        return productID;
    }

    /**
     * Define el valor de la propiedad productID.
     * 
     */
    public void setProductID(int value) {
        this.productID = value;
    }

    /**
     * Obtiene el valor de la propiedad descriptors.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDescriptors() {
        return descriptors;
    }

    /**
     * Define el valor de la propiedad descriptors.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDescriptors(DumpType value) {
        this.descriptors = value;
    }

}
