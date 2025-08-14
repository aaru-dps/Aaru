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
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.datatype.XMLGregorianCalendar;


/**
 * <p>Clase Java para MagazineType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="MagazineType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Barcodes" type="{}BarcodesType" minOccurs="0"/>
 *         &lt;element name="Cover" type="{}CoverType" minOccurs="0"/>
 *         &lt;element name="Name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Editorial" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="PublicationDate" type="{http://www.w3.org/2001/XMLSchema}date" minOccurs="0"/>
 *         &lt;element name="Number" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="Language" type="{}LanguagesType" minOccurs="0"/>
 *         &lt;element name="Pages" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="PageSize" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Scan" type="{}ScanType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "MagazineType", propOrder = {
    "barcodes",
    "cover",
    "name",
    "editorial",
    "publicationDate",
    "number",
    "language",
    "pages",
    "pageSize",
    "scan"
})
public class MagazineType {

    @XmlElement(name = "Barcodes")
    protected BarcodesType barcodes;
    @XmlElement(name = "Cover")
    protected CoverType cover;
    @XmlElement(name = "Name", required = true)
    protected String name;
    @XmlElement(name = "Editorial")
    protected String editorial;
    @XmlElement(name = "PublicationDate")
    @XmlSchemaType(name = "date")
    protected XMLGregorianCalendar publicationDate;
    @XmlElement(name = "Number")
    @XmlSchemaType(name = "unsignedInt")
    protected Long number;
    @XmlElement(name = "Language")
    protected LanguagesType language;
    @XmlElement(name = "Pages")
    @XmlSchemaType(name = "unsignedInt")
    protected Long pages;
    @XmlElement(name = "PageSize")
    protected String pageSize;
    @XmlElement(name = "Scan")
    protected ScanType scan;

    /**
     * Obtiene el valor de la propiedad barcodes.
     * 
     * @return
     *     possible object is
     *     {@link BarcodesType }
     *     
     */
    public BarcodesType getBarcodes() {
        return barcodes;
    }

    /**
     * Define el valor de la propiedad barcodes.
     * 
     * @param value
     *     allowed object is
     *     {@link BarcodesType }
     *     
     */
    public void setBarcodes(BarcodesType value) {
        this.barcodes = value;
    }

    /**
     * Obtiene el valor de la propiedad cover.
     * 
     * @return
     *     possible object is
     *     {@link CoverType }
     *     
     */
    public CoverType getCover() {
        return cover;
    }

    /**
     * Define el valor de la propiedad cover.
     * 
     * @param value
     *     allowed object is
     *     {@link CoverType }
     *     
     */
    public void setCover(CoverType value) {
        this.cover = value;
    }

    /**
     * Obtiene el valor de la propiedad name.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getName() {
        return name;
    }

    /**
     * Define el valor de la propiedad name.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setName(String value) {
        this.name = value;
    }

    /**
     * Obtiene el valor de la propiedad editorial.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getEditorial() {
        return editorial;
    }

    /**
     * Define el valor de la propiedad editorial.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setEditorial(String value) {
        this.editorial = value;
    }

    /**
     * Obtiene el valor de la propiedad publicationDate.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getPublicationDate() {
        return publicationDate;
    }

    /**
     * Define el valor de la propiedad publicationDate.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setPublicationDate(XMLGregorianCalendar value) {
        this.publicationDate = value;
    }

    /**
     * Obtiene el valor de la propiedad number.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getNumber() {
        return number;
    }

    /**
     * Define el valor de la propiedad number.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setNumber(Long value) {
        this.number = value;
    }

    /**
     * Obtiene el valor de la propiedad language.
     * 
     * @return
     *     possible object is
     *     {@link LanguagesType }
     *     
     */
    public LanguagesType getLanguage() {
        return language;
    }

    /**
     * Define el valor de la propiedad language.
     * 
     * @param value
     *     allowed object is
     *     {@link LanguagesType }
     *     
     */
    public void setLanguage(LanguagesType value) {
        this.language = value;
    }

    /**
     * Obtiene el valor de la propiedad pages.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getPages() {
        return pages;
    }

    /**
     * Define el valor de la propiedad pages.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setPages(Long value) {
        this.pages = value;
    }

    /**
     * Obtiene el valor de la propiedad pageSize.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPageSize() {
        return pageSize;
    }

    /**
     * Define el valor de la propiedad pageSize.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPageSize(String value) {
        this.pageSize = value;
    }

    /**
     * Obtiene el valor de la propiedad scan.
     * 
     * @return
     *     possible object is
     *     {@link ScanType }
     *     
     */
    public ScanType getScan() {
        return scan;
    }

    /**
     * Define el valor de la propiedad scan.
     * 
     * @param value
     *     allowed object is
     *     {@link ScanType }
     *     
     */
    public void setScan(ScanType value) {
        this.scan = value;
    }

}
