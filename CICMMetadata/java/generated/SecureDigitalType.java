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
 * Contains SecureDigital device information
 * 
 * <p>Clase Java para SecureDigitalType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="SecureDigitalType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="CID" type="{}DumpType"/>
 *         &lt;element name="CSD" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="SCR" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="OCR" type="{}DumpType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SecureDigitalType", propOrder = {
    "cid",
    "csd",
    "scr",
    "ocr"
})
public class SecureDigitalType {

    @XmlElement(name = "CID", required = true)
    protected DumpType cid;
    @XmlElement(name = "CSD")
    protected DumpType csd;
    @XmlElement(name = "SCR")
    protected DumpType scr;
    @XmlElement(name = "OCR")
    protected DumpType ocr;

    /**
     * Obtiene el valor de la propiedad cid.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getCID() {
        return cid;
    }

    /**
     * Define el valor de la propiedad cid.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setCID(DumpType value) {
        this.cid = value;
    }

    /**
     * Obtiene el valor de la propiedad csd.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getCSD() {
        return csd;
    }

    /**
     * Define el valor de la propiedad csd.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setCSD(DumpType value) {
        this.csd = value;
    }

    /**
     * Obtiene el valor de la propiedad scr.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getSCR() {
        return scr;
    }

    /**
     * Define el valor de la propiedad scr.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setSCR(DumpType value) {
        this.scr = value;
    }

    /**
     * Obtiene el valor de la propiedad ocr.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getOCR() {
        return ocr;
    }

    /**
     * Define el valor de la propiedad ocr.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setOCR(DumpType value) {
        this.ocr = value;
    }

}
