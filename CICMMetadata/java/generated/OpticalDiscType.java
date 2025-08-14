//
// Este archivo ha sido generado por la arquitectura JavaTM para la implantación de la referencia de enlace (JAXB) XML v2.2.8-b130911.1802 
// Visite <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Todas las modificaciones realizadas en este archivo se perderán si se vuelve a compilar el esquema de origen. 
// Generado el: 2020.07.12 a las 10:42:39 PM WEST 
//


package generated;

import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Clase Java para OpticalDiscType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="OpticalDiscType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Sequence" type="{}SequenceType"/>
 *         &lt;element name="Layers" type="{}LayersType" minOccurs="0"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="PartNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SerialNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="RingCode" type="{}LayeredTextType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="MasteringSID" type="{}LayeredTextType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Toolstamp" type="{}LayeredTextType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="MouldSID" type="{}LayeredTextType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="MouldText" type="{}LayeredTextType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="DiscType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="DiscSubType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Offset" type="{http://www.w3.org/2001/XMLSchema}int" minOccurs="0"/>
 *         &lt;element name="Tracks" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" maxOccurs="unbounded"/>
 *         &lt;element name="Sessions" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="CopyProtection" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Dimensions" type="{}DimensionsType"/>
 *         &lt;element name="Case" type="{}CaseType" minOccurs="0"/>
 *         &lt;element name="Scans" type="{}ScansType" minOccurs="0"/>
 *         &lt;element name="PFI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="DMI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="CMI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="BCA" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="ATIP" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="ADIP" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="PMA" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="DDS" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="SAI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="LastRMD" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="PRI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="MediaID" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="PFIR" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="DCB" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="DI" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="PAC" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="TOC" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="LeadInCdText" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="FirstTrackPregrap" type="{}BorderType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="LeadIn" type="{}BorderType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="LeadOut" type="{}BorderType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Xbox" type="{}XboxType" minOccurs="0"/>
 *         &lt;element name="PS3Encryption" type="{}PS3EncryptionType" minOccurs="0"/>
 *         &lt;element name="MediaCatalogueNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Track" type="{}TrackType" maxOccurs="unbounded"/>
 *         &lt;element name="DumpHardwareArray" type="{}DumpHardwareArrayType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "OpticalDiscType", propOrder = {
    "image",
    "size",
    "sequence",
    "layers",
    "checksums",
    "partNumber",
    "serialNumber",
    "ringCode",
    "masteringSID",
    "toolstamp",
    "mouldSID",
    "mouldText",
    "discType",
    "discSubType",
    "offset",
    "tracks",
    "sessions",
    "copyProtection",
    "dimensions",
    "_case",
    "scans",
    "pfi",
    "dmi",
    "cmi",
    "bca",
    "atip",
    "adip",
    "pma",
    "dds",
    "sai",
    "lastRMD",
    "pri",
    "mediaID",
    "pfir",
    "dcb",
    "di",
    "pac",
    "toc",
    "leadInCdText",
    "firstTrackPregrap",
    "leadIn",
    "leadOut",
    "xbox",
    "ps3Encryption",
    "mediaCatalogueNumber",
    "track",
    "dumpHardwareArray"
})
public class OpticalDiscType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "Sequence", required = true)
    protected SequenceType sequence;
    @XmlElement(name = "Layers")
    protected LayersType layers;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "PartNumber")
    protected String partNumber;
    @XmlElement(name = "SerialNumber")
    protected String serialNumber;
    @XmlElement(name = "RingCode")
    protected List<LayeredTextType> ringCode;
    @XmlElement(name = "MasteringSID")
    protected List<LayeredTextType> masteringSID;
    @XmlElement(name = "Toolstamp")
    protected List<LayeredTextType> toolstamp;
    @XmlElement(name = "MouldSID")
    protected List<LayeredTextType> mouldSID;
    @XmlElement(name = "MouldText")
    protected List<LayeredTextType> mouldText;
    @XmlElement(name = "DiscType", required = true)
    protected String discType;
    @XmlElement(name = "DiscSubType", required = true)
    protected String discSubType;
    @XmlElement(name = "Offset")
    protected Integer offset;
    @XmlElement(name = "Tracks", type = Long.class)
    @XmlSchemaType(name = "unsignedInt")
    protected List<Long> tracks;
    @XmlElement(name = "Sessions")
    @XmlSchemaType(name = "unsignedInt")
    protected long sessions;
    @XmlElement(name = "CopyProtection")
    protected String copyProtection;
    @XmlElement(name = "Dimensions", required = true)
    protected DimensionsType dimensions;
    @XmlElement(name = "Case")
    protected CaseType _case;
    @XmlElement(name = "Scans")
    protected ScansType scans;
    @XmlElement(name = "PFI")
    protected DumpType pfi;
    @XmlElement(name = "DMI")
    protected DumpType dmi;
    @XmlElement(name = "CMI")
    protected DumpType cmi;
    @XmlElement(name = "BCA")
    protected DumpType bca;
    @XmlElement(name = "ATIP")
    protected DumpType atip;
    @XmlElement(name = "ADIP")
    protected DumpType adip;
    @XmlElement(name = "PMA")
    protected DumpType pma;
    @XmlElement(name = "DDS")
    protected DumpType dds;
    @XmlElement(name = "SAI")
    protected DumpType sai;
    @XmlElement(name = "LastRMD")
    protected DumpType lastRMD;
    @XmlElement(name = "PRI")
    protected DumpType pri;
    @XmlElement(name = "MediaID")
    protected DumpType mediaID;
    @XmlElement(name = "PFIR")
    protected DumpType pfir;
    @XmlElement(name = "DCB")
    protected DumpType dcb;
    @XmlElement(name = "DI")
    protected DumpType di;
    @XmlElement(name = "PAC")
    protected DumpType pac;
    @XmlElement(name = "TOC")
    protected DumpType toc;
    @XmlElement(name = "LeadInCdText")
    protected DumpType leadInCdText;
    @XmlElement(name = "FirstTrackPregrap")
    protected List<BorderType> firstTrackPregrap;
    @XmlElement(name = "LeadIn")
    protected List<BorderType> leadIn;
    @XmlElement(name = "LeadOut")
    protected List<BorderType> leadOut;
    @XmlElement(name = "Xbox")
    protected XboxType xbox;
    @XmlElement(name = "PS3Encryption")
    protected PS3EncryptionType ps3Encryption;
    @XmlElement(name = "MediaCatalogueNumber")
    protected String mediaCatalogueNumber;
    @XmlElement(name = "Track", required = true)
    protected List<TrackType> track;
    @XmlElement(name = "DumpHardwareArray")
    protected DumpHardwareArrayType dumpHardwareArray;

    /**
     * Obtiene el valor de la propiedad image.
     * 
     * @return
     *     possible object is
     *     {@link ImageType }
     *     
     */
    public ImageType getImage() {
        return image;
    }

    /**
     * Define el valor de la propiedad image.
     * 
     * @param value
     *     allowed object is
     *     {@link ImageType }
     *     
     */
    public void setImage(ImageType value) {
        this.image = value;
    }

    /**
     * Obtiene el valor de la propiedad size.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getSize() {
        return size;
    }

    /**
     * Define el valor de la propiedad size.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setSize(BigInteger value) {
        this.size = value;
    }

    /**
     * Obtiene el valor de la propiedad sequence.
     * 
     * @return
     *     possible object is
     *     {@link SequenceType }
     *     
     */
    public SequenceType getSequence() {
        return sequence;
    }

    /**
     * Define el valor de la propiedad sequence.
     * 
     * @param value
     *     allowed object is
     *     {@link SequenceType }
     *     
     */
    public void setSequence(SequenceType value) {
        this.sequence = value;
    }

    /**
     * Obtiene el valor de la propiedad layers.
     * 
     * @return
     *     possible object is
     *     {@link LayersType }
     *     
     */
    public LayersType getLayers() {
        return layers;
    }

    /**
     * Define el valor de la propiedad layers.
     * 
     * @param value
     *     allowed object is
     *     {@link LayersType }
     *     
     */
    public void setLayers(LayersType value) {
        this.layers = value;
    }

    /**
     * Obtiene el valor de la propiedad checksums.
     * 
     * @return
     *     possible object is
     *     {@link ChecksumsType }
     *     
     */
    public ChecksumsType getChecksums() {
        return checksums;
    }

    /**
     * Define el valor de la propiedad checksums.
     * 
     * @param value
     *     allowed object is
     *     {@link ChecksumsType }
     *     
     */
    public void setChecksums(ChecksumsType value) {
        this.checksums = value;
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
     * Gets the value of the ringCode property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the ringCode property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getRingCode().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LayeredTextType }
     * 
     * 
     */
    public List<LayeredTextType> getRingCode() {
        if (ringCode == null) {
            ringCode = new ArrayList<LayeredTextType>();
        }
        return this.ringCode;
    }

    /**
     * Gets the value of the masteringSID property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the masteringSID property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMasteringSID().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LayeredTextType }
     * 
     * 
     */
    public List<LayeredTextType> getMasteringSID() {
        if (masteringSID == null) {
            masteringSID = new ArrayList<LayeredTextType>();
        }
        return this.masteringSID;
    }

    /**
     * Gets the value of the toolstamp property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the toolstamp property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getToolstamp().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LayeredTextType }
     * 
     * 
     */
    public List<LayeredTextType> getToolstamp() {
        if (toolstamp == null) {
            toolstamp = new ArrayList<LayeredTextType>();
        }
        return this.toolstamp;
    }

    /**
     * Gets the value of the mouldSID property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the mouldSID property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMouldSID().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LayeredTextType }
     * 
     * 
     */
    public List<LayeredTextType> getMouldSID() {
        if (mouldSID == null) {
            mouldSID = new ArrayList<LayeredTextType>();
        }
        return this.mouldSID;
    }

    /**
     * Gets the value of the mouldText property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the mouldText property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMouldText().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link LayeredTextType }
     * 
     * 
     */
    public List<LayeredTextType> getMouldText() {
        if (mouldText == null) {
            mouldText = new ArrayList<LayeredTextType>();
        }
        return this.mouldText;
    }

    /**
     * Obtiene el valor de la propiedad discType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDiscType() {
        return discType;
    }

    /**
     * Define el valor de la propiedad discType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDiscType(String value) {
        this.discType = value;
    }

    /**
     * Obtiene el valor de la propiedad discSubType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDiscSubType() {
        return discSubType;
    }

    /**
     * Define el valor de la propiedad discSubType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDiscSubType(String value) {
        this.discSubType = value;
    }

    /**
     * Obtiene el valor de la propiedad offset.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getOffset() {
        return offset;
    }

    /**
     * Define el valor de la propiedad offset.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setOffset(Integer value) {
        this.offset = value;
    }

    /**
     * Gets the value of the tracks property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the tracks property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTracks().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Long }
     * 
     * 
     */
    public List<Long> getTracks() {
        if (tracks == null) {
            tracks = new ArrayList<Long>();
        }
        return this.tracks;
    }

    /**
     * Obtiene el valor de la propiedad sessions.
     * 
     */
    public long getSessions() {
        return sessions;
    }

    /**
     * Define el valor de la propiedad sessions.
     * 
     */
    public void setSessions(long value) {
        this.sessions = value;
    }

    /**
     * Obtiene el valor de la propiedad copyProtection.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCopyProtection() {
        return copyProtection;
    }

    /**
     * Define el valor de la propiedad copyProtection.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCopyProtection(String value) {
        this.copyProtection = value;
    }

    /**
     * Obtiene el valor de la propiedad dimensions.
     * 
     * @return
     *     possible object is
     *     {@link DimensionsType }
     *     
     */
    public DimensionsType getDimensions() {
        return dimensions;
    }

    /**
     * Define el valor de la propiedad dimensions.
     * 
     * @param value
     *     allowed object is
     *     {@link DimensionsType }
     *     
     */
    public void setDimensions(DimensionsType value) {
        this.dimensions = value;
    }

    /**
     * Obtiene el valor de la propiedad case.
     * 
     * @return
     *     possible object is
     *     {@link CaseType }
     *     
     */
    public CaseType getCase() {
        return _case;
    }

    /**
     * Define el valor de la propiedad case.
     * 
     * @param value
     *     allowed object is
     *     {@link CaseType }
     *     
     */
    public void setCase(CaseType value) {
        this._case = value;
    }

    /**
     * Obtiene el valor de la propiedad scans.
     * 
     * @return
     *     possible object is
     *     {@link ScansType }
     *     
     */
    public ScansType getScans() {
        return scans;
    }

    /**
     * Define el valor de la propiedad scans.
     * 
     * @param value
     *     allowed object is
     *     {@link ScansType }
     *     
     */
    public void setScans(ScansType value) {
        this.scans = value;
    }

    /**
     * Obtiene el valor de la propiedad pfi.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPFI() {
        return pfi;
    }

    /**
     * Define el valor de la propiedad pfi.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPFI(DumpType value) {
        this.pfi = value;
    }

    /**
     * Obtiene el valor de la propiedad dmi.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDMI() {
        return dmi;
    }

    /**
     * Define el valor de la propiedad dmi.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDMI(DumpType value) {
        this.dmi = value;
    }

    /**
     * Obtiene el valor de la propiedad cmi.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getCMI() {
        return cmi;
    }

    /**
     * Define el valor de la propiedad cmi.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setCMI(DumpType value) {
        this.cmi = value;
    }

    /**
     * Obtiene el valor de la propiedad bca.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getBCA() {
        return bca;
    }

    /**
     * Define el valor de la propiedad bca.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setBCA(DumpType value) {
        this.bca = value;
    }

    /**
     * Obtiene el valor de la propiedad atip.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getATIP() {
        return atip;
    }

    /**
     * Define el valor de la propiedad atip.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setATIP(DumpType value) {
        this.atip = value;
    }

    /**
     * Obtiene el valor de la propiedad adip.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getADIP() {
        return adip;
    }

    /**
     * Define el valor de la propiedad adip.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setADIP(DumpType value) {
        this.adip = value;
    }

    /**
     * Obtiene el valor de la propiedad pma.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPMA() {
        return pma;
    }

    /**
     * Define el valor de la propiedad pma.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPMA(DumpType value) {
        this.pma = value;
    }

    /**
     * Obtiene el valor de la propiedad dds.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDDS() {
        return dds;
    }

    /**
     * Define el valor de la propiedad dds.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDDS(DumpType value) {
        this.dds = value;
    }

    /**
     * Obtiene el valor de la propiedad sai.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getSAI() {
        return sai;
    }

    /**
     * Define el valor de la propiedad sai.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setSAI(DumpType value) {
        this.sai = value;
    }

    /**
     * Obtiene el valor de la propiedad lastRMD.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getLastRMD() {
        return lastRMD;
    }

    /**
     * Define el valor de la propiedad lastRMD.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setLastRMD(DumpType value) {
        this.lastRMD = value;
    }

    /**
     * Obtiene el valor de la propiedad pri.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPRI() {
        return pri;
    }

    /**
     * Define el valor de la propiedad pri.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPRI(DumpType value) {
        this.pri = value;
    }

    /**
     * Obtiene el valor de la propiedad mediaID.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getMediaID() {
        return mediaID;
    }

    /**
     * Define el valor de la propiedad mediaID.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setMediaID(DumpType value) {
        this.mediaID = value;
    }

    /**
     * Obtiene el valor de la propiedad pfir.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPFIR() {
        return pfir;
    }

    /**
     * Define el valor de la propiedad pfir.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPFIR(DumpType value) {
        this.pfir = value;
    }

    /**
     * Obtiene el valor de la propiedad dcb.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDCB() {
        return dcb;
    }

    /**
     * Define el valor de la propiedad dcb.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDCB(DumpType value) {
        this.dcb = value;
    }

    /**
     * Obtiene el valor de la propiedad di.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDI() {
        return di;
    }

    /**
     * Define el valor de la propiedad di.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDI(DumpType value) {
        this.di = value;
    }

    /**
     * Obtiene el valor de la propiedad pac.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPAC() {
        return pac;
    }

    /**
     * Define el valor de la propiedad pac.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPAC(DumpType value) {
        this.pac = value;
    }

    /**
     * Obtiene el valor de la propiedad toc.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getTOC() {
        return toc;
    }

    /**
     * Define el valor de la propiedad toc.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setTOC(DumpType value) {
        this.toc = value;
    }

    /**
     * Obtiene el valor de la propiedad leadInCdText.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getLeadInCdText() {
        return leadInCdText;
    }

    /**
     * Define el valor de la propiedad leadInCdText.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setLeadInCdText(DumpType value) {
        this.leadInCdText = value;
    }

    /**
     * Gets the value of the firstTrackPregrap property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the firstTrackPregrap property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getFirstTrackPregrap().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BorderType }
     * 
     * 
     */
    public List<BorderType> getFirstTrackPregrap() {
        if (firstTrackPregrap == null) {
            firstTrackPregrap = new ArrayList<BorderType>();
        }
        return this.firstTrackPregrap;
    }

    /**
     * Gets the value of the leadIn property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the leadIn property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLeadIn().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BorderType }
     * 
     * 
     */
    public List<BorderType> getLeadIn() {
        if (leadIn == null) {
            leadIn = new ArrayList<BorderType>();
        }
        return this.leadIn;
    }

    /**
     * Gets the value of the leadOut property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the leadOut property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getLeadOut().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BorderType }
     * 
     * 
     */
    public List<BorderType> getLeadOut() {
        if (leadOut == null) {
            leadOut = new ArrayList<BorderType>();
        }
        return this.leadOut;
    }

    /**
     * Obtiene el valor de la propiedad xbox.
     * 
     * @return
     *     possible object is
     *     {@link XboxType }
     *     
     */
    public XboxType getXbox() {
        return xbox;
    }

    /**
     * Define el valor de la propiedad xbox.
     * 
     * @param value
     *     allowed object is
     *     {@link XboxType }
     *     
     */
    public void setXbox(XboxType value) {
        this.xbox = value;
    }

    /**
     * Obtiene el valor de la propiedad ps3Encryption.
     * 
     * @return
     *     possible object is
     *     {@link PS3EncryptionType }
     *     
     */
    public PS3EncryptionType getPS3Encryption() {
        return ps3Encryption;
    }

    /**
     * Define el valor de la propiedad ps3Encryption.
     * 
     * @param value
     *     allowed object is
     *     {@link PS3EncryptionType }
     *     
     */
    public void setPS3Encryption(PS3EncryptionType value) {
        this.ps3Encryption = value;
    }

    /**
     * Obtiene el valor de la propiedad mediaCatalogueNumber.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMediaCatalogueNumber() {
        return mediaCatalogueNumber;
    }

    /**
     * Define el valor de la propiedad mediaCatalogueNumber.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMediaCatalogueNumber(String value) {
        this.mediaCatalogueNumber = value;
    }

    /**
     * Gets the value of the track property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the track property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getTrack().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link TrackType }
     * 
     * 
     */
    public List<TrackType> getTrack() {
        if (track == null) {
            track = new ArrayList<TrackType>();
        }
        return this.track;
    }

    /**
     * Obtiene el valor de la propiedad dumpHardwareArray.
     * 
     * @return
     *     possible object is
     *     {@link DumpHardwareArrayType }
     *     
     */
    public DumpHardwareArrayType getDumpHardwareArray() {
        return dumpHardwareArray;
    }

    /**
     * Define el valor de la propiedad dumpHardwareArray.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpHardwareArrayType }
     *     
     */
    public void setDumpHardwareArray(DumpHardwareArrayType value) {
        this.dumpHardwareArray = value;
    }

}
