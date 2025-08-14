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
 * Describes a dump of a media that is stored in an audio device (like a ZX Spectrum
 *                 cassette)
 *             
 * 
 * <p>Clase Java para AudioMediaType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="AudioMediaType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="Sequence" type="{}SequenceType"/>
 *         &lt;element name="PartNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SerialNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Manufacturer" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Model" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="AccoustID" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Block" type="{}AudioBlockType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="CopyProtection" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Dimensions" type="{}DimensionsType"/>
 *         &lt;element name="Scans" type="{}ScansType" minOccurs="0"/>
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
@XmlType(name = "AudioMediaType", propOrder = {
    "image",
    "size",
    "checksums",
    "sequence",
    "partNumber",
    "serialNumber",
    "manufacturer",
    "model",
    "accoustID",
    "block",
    "copyProtection",
    "dimensions",
    "scans",
    "dumpHardwareArray"
})
public class AudioMediaType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "Sequence", required = true)
    protected SequenceType sequence;
    @XmlElement(name = "PartNumber")
    protected String partNumber;
    @XmlElement(name = "SerialNumber")
    protected String serialNumber;
    @XmlElement(name = "Manufacturer")
    protected String manufacturer;
    @XmlElement(name = "Model")
    protected String model;
    @XmlElement(name = "AccoustID")
    protected String accoustID;
    @XmlElement(name = "Block")
    protected List<AudioBlockType> block;
    @XmlElement(name = "CopyProtection")
    protected String copyProtection;
    @XmlElement(name = "Dimensions", required = true)
    protected DimensionsType dimensions;
    @XmlElement(name = "Scans")
    protected ScansType scans;
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
     * Obtiene el valor de la propiedad model.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getModel() {
        return model;
    }

    /**
     * Define el valor de la propiedad model.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setModel(String value) {
        this.model = value;
    }

    /**
     * Obtiene el valor de la propiedad accoustID.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAccoustID() {
        return accoustID;
    }

    /**
     * Define el valor de la propiedad accoustID.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setAccoustID(String value) {
        this.accoustID = value;
    }

    /**
     * Gets the value of the block property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the block property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getBlock().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AudioBlockType }
     * 
     * 
     */
    public List<AudioBlockType> getBlock() {
        if (block == null) {
            block = new ArrayList<AudioBlockType>();
        }
        return this.block;
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
