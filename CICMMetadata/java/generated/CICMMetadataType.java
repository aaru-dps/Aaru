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
import javax.xml.datatype.XMLGregorianCalendar;


/**
 * Digital Asset Metadata
 * 
 * <p>Clase Java para CICMMetadataType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="CICMMetadataType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Developer" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Publisher" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Author" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Performer" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Name" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Version" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="ReleaseType" minOccurs="0">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="Retail"/>
 *               &lt;enumeration value="Bundle"/>
 *               &lt;enumeration value="Coverdisc"/>
 *               &lt;enumeration value="Subscription"/>
 *               &lt;enumeration value="Demo"/>
 *               &lt;enumeration value="OEM"/>
 *               &lt;enumeration value="Shareware"/>
 *               &lt;enumeration value="FOSS"/>
 *               &lt;enumeration value="Adware"/>
 *               &lt;enumeration value="Donationware"/>
 *               &lt;enumeration value="Digital download"/>
 *               &lt;enumeration value="SaaS"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="ReleaseDate" type="{http://www.w3.org/2001/XMLSchema}date" minOccurs="0"/>
 *         &lt;element name="Barcodes" type="{}BarcodesType" minOccurs="0"/>
 *         &lt;element name="PartNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SerialNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Keywords" type="{}KeywordsType" minOccurs="0"/>
 *         &lt;element name="Magazine" type="{}MagazineType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Book" type="{}BookType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Categories" type="{}CategoriesType" minOccurs="0"/>
 *         &lt;element name="Subcategories" type="{}SubcategoriesType" minOccurs="0"/>
 *         &lt;element name="Languages" type="{}LanguagesType" minOccurs="0"/>
 *         &lt;element name="Systems" type="{}SystemsType" minOccurs="0"/>
 *         &lt;element name="Architectures" type="{}ArchitecturesType" minOccurs="0"/>
 *         &lt;element name="RequiredOperatingSystems" type="{}RequiredOperatingSystemsType" minOccurs="0"/>
 *         &lt;element name="UserManual" type="{}UserManualType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="OpticalDisc" type="{}OpticalDiscType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Advertisement" type="{}AdvertisementType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="LinearMedia" type="{}LinearMediaType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="PCICard" type="{}PCIType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="BlockMedia" type="{}BlockMediaType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="AudioMedia" type="{}AudioMediaType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "CICMMetadataType", propOrder = {
    "developer",
    "publisher",
    "author",
    "performer",
    "name",
    "version",
    "releaseType",
    "releaseDate",
    "barcodes",
    "partNumber",
    "serialNumber",
    "keywords",
    "magazine",
    "book",
    "categories",
    "subcategories",
    "languages",
    "systems",
    "architectures",
    "requiredOperatingSystems",
    "userManual",
    "opticalDisc",
    "advertisement",
    "linearMedia",
    "pciCard",
    "blockMedia",
    "audioMedia"
})
public class CICMMetadataType {

    @XmlElement(name = "Developer")
    protected List<String> developer;
    @XmlElement(name = "Publisher")
    protected List<String> publisher;
    @XmlElement(name = "Author")
    protected List<String> author;
    @XmlElement(name = "Performer")
    protected List<String> performer;
    @XmlElement(name = "Name", required = true)
    protected String name;
    @XmlElement(name = "Version")
    protected String version;
    @XmlElement(name = "ReleaseType")
    protected String releaseType;
    @XmlElement(name = "ReleaseDate")
    @XmlSchemaType(name = "date")
    protected XMLGregorianCalendar releaseDate;
    @XmlElement(name = "Barcodes")
    protected BarcodesType barcodes;
    @XmlElement(name = "PartNumber")
    protected String partNumber;
    @XmlElement(name = "SerialNumber")
    protected String serialNumber;
    @XmlElement(name = "Keywords")
    protected KeywordsType keywords;
    @XmlElement(name = "Magazine")
    protected List<MagazineType> magazine;
    @XmlElement(name = "Book")
    protected List<BookType> book;
    @XmlElement(name = "Categories")
    protected CategoriesType categories;
    @XmlElement(name = "Subcategories")
    protected SubcategoriesType subcategories;
    @XmlElement(name = "Languages")
    protected LanguagesType languages;
    @XmlElement(name = "Systems")
    protected SystemsType systems;
    @XmlElement(name = "Architectures")
    protected ArchitecturesType architectures;
    @XmlElement(name = "RequiredOperatingSystems")
    protected RequiredOperatingSystemsType requiredOperatingSystems;
    @XmlElement(name = "UserManual")
    protected List<UserManualType> userManual;
    @XmlElement(name = "OpticalDisc")
    protected List<OpticalDiscType> opticalDisc;
    @XmlElement(name = "Advertisement")
    protected List<AdvertisementType> advertisement;
    @XmlElement(name = "LinearMedia")
    protected List<LinearMediaType> linearMedia;
    @XmlElement(name = "PCICard")
    protected List<PCIType> pciCard;
    @XmlElement(name = "BlockMedia")
    protected List<BlockMediaType> blockMedia;
    @XmlElement(name = "AudioMedia")
    protected List<AudioMediaType> audioMedia;

    /**
     * Gets the value of the developer property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the developer property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDeveloper().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getDeveloper() {
        if (developer == null) {
            developer = new ArrayList<String>();
        }
        return this.developer;
    }

    /**
     * Gets the value of the publisher property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the publisher property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPublisher().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getPublisher() {
        if (publisher == null) {
            publisher = new ArrayList<String>();
        }
        return this.publisher;
    }

    /**
     * Gets the value of the author property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the author property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAuthor().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getAuthor() {
        if (author == null) {
            author = new ArrayList<String>();
        }
        return this.author;
    }

    /**
     * Gets the value of the performer property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the performer property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPerformer().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getPerformer() {
        if (performer == null) {
            performer = new ArrayList<String>();
        }
        return this.performer;
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
     * Obtiene el valor de la propiedad version.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getVersion() {
        return version;
    }

    /**
     * Define el valor de la propiedad version.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setVersion(String value) {
        this.version = value;
    }

    /**
     * Obtiene el valor de la propiedad releaseType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReleaseType() {
        return releaseType;
    }

    /**
     * Define el valor de la propiedad releaseType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReleaseType(String value) {
        this.releaseType = value;
    }

    /**
     * Obtiene el valor de la propiedad releaseDate.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getReleaseDate() {
        return releaseDate;
    }

    /**
     * Define el valor de la propiedad releaseDate.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setReleaseDate(XMLGregorianCalendar value) {
        this.releaseDate = value;
    }

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
     * Obtiene el valor de la propiedad partNumber.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPartNumber() {
        return partNumber;
    }

    /**
     * Define el valor de la propiedad partNumber.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPartNumber(String value) {
        this.partNumber = value;
    }

    /**
     * Obtiene el valor de la propiedad serialNumber.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSerialNumber() {
        return serialNumber;
    }

    /**
     * Define el valor de la propiedad serialNumber.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSerialNumber(String value) {
        this.serialNumber = value;
    }

    /**
     * Obtiene el valor de la propiedad keywords.
     * 
     * @return
     *     possible object is
     *     {@link KeywordsType }
     *     
     */
    public KeywordsType getKeywords() {
        return keywords;
    }

    /**
     * Define el valor de la propiedad keywords.
     * 
     * @param value
     *     allowed object is
     *     {@link KeywordsType }
     *     
     */
    public void setKeywords(KeywordsType value) {
        this.keywords = value;
    }

    /**
     * Gets the value of the magazine property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the magazine property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMagazine().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link MagazineType }
     * 
     * 
     */
    public List<MagazineType> getMagazine() {
        if (magazine == null) {
            magazine = new ArrayList<MagazineType>();
        }
        return this.magazine;
    }

    /**
     * Gets the value of the book property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the book property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getBook().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BookType }
     * 
     * 
     */
    public List<BookType> getBook() {
        if (book == null) {
            book = new ArrayList<BookType>();
        }
        return this.book;
    }

    /**
     * Obtiene el valor de la propiedad categories.
     * 
     * @return
     *     possible object is
     *     {@link CategoriesType }
     *     
     */
    public CategoriesType getCategories() {
        return categories;
    }

    /**
     * Define el valor de la propiedad categories.
     * 
     * @param value
     *     allowed object is
     *     {@link CategoriesType }
     *     
     */
    public void setCategories(CategoriesType value) {
        this.categories = value;
    }

    /**
     * Obtiene el valor de la propiedad subcategories.
     * 
     * @return
     *     possible object is
     *     {@link SubcategoriesType }
     *     
     */
    public SubcategoriesType getSubcategories() {
        return subcategories;
    }

    /**
     * Define el valor de la propiedad subcategories.
     * 
     * @param value
     *     allowed object is
     *     {@link SubcategoriesType }
     *     
     */
    public void setSubcategories(SubcategoriesType value) {
        this.subcategories = value;
    }

    /**
     * Obtiene el valor de la propiedad languages.
     * 
     * @return
     *     possible object is
     *     {@link LanguagesType }
     *     
     */
    public LanguagesType getLanguages() {
        return languages;
    }

    /**
     * Define el valor de la propiedad languages.
     * 
     * @param value
     *     allowed object is
     *     {@link LanguagesType }
     *     
     */
    public void setLanguages(LanguagesType value) {
        this.languages = value;
    }

    /**
     * Obtiene el valor de la propiedad systems.
     * 
     * @return
     *     possible object is
     *     {@link SystemsType }
     *     
     */
    public SystemsType getSystems() {
        return systems;
    }

    /**
     * Define el valor de la propiedad systems.
     * 
     * @param value
     *     allowed object is
     *     {@link SystemsType }
     *     
     */
    public void setSystems(SystemsType value) {
        this.systems = value;
    }

    /**
     * Obtiene el valor de la propiedad architectures.
     * 
     * @return
     *     possible object is
     *     {@link ArchitecturesType }
     *     
     */
    public ArchitecturesType getArchitectures() {
        return architectures;
    }

    /**
     * Define el valor de la propiedad architectures.
     * 
     * @param value
     *     allowed object is
     *     {@link ArchitecturesType }
     *     
     */
    public void setArchitectures(ArchitecturesType value) {
        this.architectures = value;
    }

    /**
     * Obtiene el valor de la propiedad requiredOperatingSystems.
     * 
     * @return
     *     possible object is
     *     {@link RequiredOperatingSystemsType }
     *     
     */
    public RequiredOperatingSystemsType getRequiredOperatingSystems() {
        return requiredOperatingSystems;
    }

    /**
     * Define el valor de la propiedad requiredOperatingSystems.
     * 
     * @param value
     *     allowed object is
     *     {@link RequiredOperatingSystemsType }
     *     
     */
    public void setRequiredOperatingSystems(RequiredOperatingSystemsType value) {
        this.requiredOperatingSystems = value;
    }

    /**
     * Gets the value of the userManual property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the userManual property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getUserManual().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link UserManualType }
     * 
     * 
     */
    public List<UserManualType> getUserManual() {
        if (userManual == null) {
            userManual = new ArrayList<UserManualType>();
        }
        return this.userManual;
    }

    /**
     * Gets the value of the opticalDisc property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the opticalDisc property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getOpticalDisc().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link OpticalDiscType }
     * 
     * 
     */
    public List<OpticalDiscType> getOpticalDisc() {
        if (opticalDisc == null) {
            opticalDisc = new ArrayList<OpticalDiscType>();
        }
        return this.opticalDisc;
    }

    /**
     * Gets the value of the advertisement property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the advertisement property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAdvertisement().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AdvertisementType }
     * 
     * 
     */
    public List<AdvertisementType> getAdvertisement() {
        if (advertisement == null) {
            advertisement = new ArrayList<AdvertisementType>();
        }
        return this.advertisement;
    }

    /**
     * Gets the value of the linearMedia property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the linearMedia property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLinearMedia().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LinearMediaType }
     * 
     * 
     */
    public List<LinearMediaType> getLinearMedia() {
        if (linearMedia == null) {
            linearMedia = new ArrayList<LinearMediaType>();
        }
        return this.linearMedia;
    }

    /**
     * Gets the value of the pciCard property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the pciCard property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPCICard().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link PCIType }
     * 
     * 
     */
    public List<PCIType> getPCICard() {
        if (pciCard == null) {
            pciCard = new ArrayList<PCIType>();
        }
        return this.pciCard;
    }

    /**
     * Gets the value of the blockMedia property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the blockMedia property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getBlockMedia().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BlockMediaType }
     * 
     * 
     */
    public List<BlockMediaType> getBlockMedia() {
        if (blockMedia == null) {
            blockMedia = new ArrayList<BlockMediaType>();
        }
        return this.blockMedia;
    }

    /**
     * Gets the value of the audioMedia property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the audioMedia property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAudioMedia().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AudioMediaType }
     * 
     * 
     */
    public List<AudioMediaType> getAudioMedia() {
        if (audioMedia == null) {
            audioMedia = new ArrayList<AudioMediaType>();
        }
        return this.audioMedia;
    }

}
