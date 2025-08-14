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
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * Contains PCMCIA card information
 * 
 * <p>Clase Java para PCMCIAType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="PCMCIAType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="CIS" type="{}DumpType"/>
 *         &lt;element name="Compliance" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="ManufacturerCode" type="{http://www.w3.org/2001/XMLSchema}unsignedShort" minOccurs="0"/>
 *         &lt;element name="CardCode" type="{http://www.w3.org/2001/XMLSchema}unsignedShort" minOccurs="0"/>
 *         &lt;element name="Manufacturer" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="ProductName" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="AdditionalInformation" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PCMCIAType", propOrder = {
    "cis",
    "compliance",
    "manufacturerCode",
    "cardCode",
    "manufacturer",
    "productName",
    "additionalInformation"
})
public class PCMCIAType {

    @XmlElement(name = "CIS", required = true)
    protected DumpType cis;
    @XmlElement(name = "Compliance")
    protected String compliance;
    @XmlElement(name = "ManufacturerCode")
    @XmlSchemaType(name = "unsignedShort")
    protected Integer manufacturerCode;
    @XmlElement(name = "CardCode")
    @XmlSchemaType(name = "unsignedShort")
    protected Integer cardCode;
    @XmlElement(name = "Manufacturer")
    protected String manufacturer;
    @XmlElement(name = "ProductName")
    protected String productName;
    @XmlElement(name = "AdditionalInformation")
    protected List<String> additionalInformation;

    /**
     * Obtiene el valor de la propiedad cis.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getCIS() {
        return cis;
    }

    /**
     * Define el valor de la propiedad cis.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setCIS(DumpType value) {
        this.cis = value;
    }

    /**
     * Obtiene el valor de la propiedad compliance.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCompliance() {
        return compliance;
    }

    /**
     * Define el valor de la propiedad compliance.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCompliance(String value) {
        this.compliance = value;
    }

    /**
     * Obtiene el valor de la propiedad manufacturerCode.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getManufacturerCode() {
        return manufacturerCode;
    }

    /**
     * Define el valor de la propiedad manufacturerCode.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setManufacturerCode(Integer value) {
        this.manufacturerCode = value;
    }

    /**
     * Obtiene el valor de la propiedad cardCode.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getCardCode() {
        return cardCode;
    }

    /**
     * Define el valor de la propiedad cardCode.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setCardCode(Integer value) {
        this.cardCode = value;
    }

    /**
     * Obtiene el valor de la propiedad manufacturer.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getManufacturer() {
        return manufacturer;
    }

    /**
     * Define el valor de la propiedad manufacturer.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setManufacturer(String value) {
        this.manufacturer = value;
    }

    /**
     * Obtiene el valor de la propiedad productName.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getProductName() {
        return productName;
    }

    /**
     * Define el valor de la propiedad productName.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setProductName(String value) {
        this.productName = value;
    }

    /**
     * Gets the value of the additionalInformation property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the additionalInformation property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAdditionalInformation().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getAdditionalInformation() {
        if (additionalInformation == null) {
            additionalInformation = new ArrayList<String>();
        }
        return this.additionalInformation;
    }

}
