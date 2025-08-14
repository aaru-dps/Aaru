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
 * Contains SCSI device information
 * 
 * <p>Clase Java para SCSIType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="SCSIType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Inquiry" type="{}DumpType"/>
 *         &lt;element name="EVPD" type="{}EVPDType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="ModeSense" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="ModeSense10" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="LogSense" type="{}DumpType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SCSIType", propOrder = {
    "inquiry",
    "evpd",
    "modeSense",
    "modeSense10",
    "logSense"
})
public class SCSIType {

    @XmlElement(name = "Inquiry", required = true)
    protected DumpType inquiry;
    @XmlElement(name = "EVPD")
    protected List<EVPDType> evpd;
    @XmlElement(name = "ModeSense")
    protected DumpType modeSense;
    @XmlElement(name = "ModeSense10")
    protected DumpType modeSense10;
    @XmlElement(name = "LogSense")
    protected DumpType logSense;

    /**
     * Obtiene el valor de la propiedad inquiry.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getInquiry() {
        return inquiry;
    }

    /**
     * Define el valor de la propiedad inquiry.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setInquiry(DumpType value) {
        this.inquiry = value;
    }

    /**
     * Gets the value of the evpd property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the evpd property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getEVPD().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link EVPDType }
     * 
     * 
     */
    public List<EVPDType> getEVPD() {
        if (evpd == null) {
            evpd = new ArrayList<EVPDType>();
        }
        return this.evpd;
    }

    /**
     * Obtiene el valor de la propiedad modeSense.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getModeSense() {
        return modeSense;
    }

    /**
     * Define el valor de la propiedad modeSense.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setModeSense(DumpType value) {
        this.modeSense = value;
    }

    /**
     * Obtiene el valor de la propiedad modeSense10.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getModeSense10() {
        return modeSense10;
    }

    /**
     * Define el valor de la propiedad modeSense10.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setModeSense10(DumpType value) {
        this.modeSense10 = value;
    }

    /**
     * Obtiene el valor de la propiedad logSense.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getLogSense() {
        return logSense;
    }

    /**
     * Define el valor de la propiedad logSense.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setLogSense(DumpType value) {
        this.logSense = value;
    }

}
