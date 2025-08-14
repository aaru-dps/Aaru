//
// Este archivo ha sido generado por la arquitectura JavaTM para la implantación de la referencia de enlace (JAXB) XML v2.2.8-b130911.1802 
// Visite <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Todas las modificaciones realizadas en este archivo se perderán si se vuelve a compilar el esquema de origen. 
// Generado el: 2020.07.12 a las 10:42:39 PM WEST 
//


package generated;

import java.math.BigInteger;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * Describes a dump of a linear media, that is, a media that is read byte-by-byte like for
 *                 example, a ROM chip, a game cartridge, a PCMCIA SRAM card, etc...
 *             
 * 
 * <p>Clase Java para LinearMediaType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="LinearMediaType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="ImageChecksums" type="{}ChecksumsType"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType" minOccurs="0"/>
 *         &lt;element name="PartNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SerialNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Title" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Sequence" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="ImageInterleave" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="Interleave" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="Manufacturer" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Model" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Package" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Interface" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Dimensions" type="{}DimensionsType" minOccurs="0"/>
 *         &lt;element name="Scans" type="{}ScansType" minOccurs="0"/>
 *         &lt;element name="DumpHardwareArray" type="{}DumpHardwareArrayType" minOccurs="0"/>
 *         &lt;element name="PCMCIA" type="{}PCMCIAType" minOccurs="0"/>
 *         &lt;element name="CopyProtection" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "LinearMediaType", propOrder = {
    "image",
    "size",
    "imageChecksums",
    "checksums",
    "partNumber",
    "serialNumber",
    "title",
    "sequence",
    "imageInterleave",
    "interleave",
    "manufacturer",
    "model",
    "_package",
    "_interface",
    "dimensions",
    "scans",
    "dumpHardwareArray",
    "pcmcia",
    "copyProtection"
})
public class LinearMediaType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "ImageChecksums", required = true)
    protected ChecksumsType imageChecksums;
    @XmlElement(name = "Checksums")
    protected ChecksumsType checksums;
    @XmlElement(name = "PartNumber")
    protected String partNumber;
    @XmlElement(name = "SerialNumber")
    protected String serialNumber;
    @XmlElement(name = "Title", required = true)
    protected String title;
    @XmlElement(name = "Sequence")
    @XmlSchemaType(name = "unsignedInt")
    protected Long sequence;
    @XmlElement(name = "ImageInterleave")
    @XmlSchemaType(name = "unsignedInt")
    protected Long imageInterleave;
    @XmlElement(name = "Interleave")
    @XmlSchemaType(name = "unsignedInt")
    protected Long interleave;
    @XmlElement(name = "Manufacturer")
    protected String manufacturer;
    @XmlElement(name = "Model")
    protected String model;
    @XmlElement(name = "Package", required = true)
    protected String _package;
    @XmlElement(name = "Interface")
    protected String _interface;
    @XmlElement(name = "Dimensions")
    protected DimensionsType dimensions;
    @XmlElement(name = "Scans")
    protected ScansType scans;
    @XmlElement(name = "DumpHardwareArray")
    protected DumpHardwareArrayType dumpHardwareArray;
    @XmlElement(name = "PCMCIA")
    protected PCMCIAType pcmcia;
    @XmlElement(name = "CopyProtection")
    protected String copyProtection;

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
     * Obtiene el valor de la propiedad imageChecksums.
     * 
     * @return
     *     possible object is
     *     {@link ChecksumsType }
     *     
     */
    public ChecksumsType getImageChecksums() {
        return imageChecksums;
    }

    /**
     * Define el valor de la propiedad imageChecksums.
     * 
     * @param value
     *     allowed object is
     *     {@link ChecksumsType }
     *     
     */
    public void setImageChecksums(ChecksumsType value) {
        this.imageChecksums = value;
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
     * Obtiene el valor de la propiedad title.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTitle() {
        return title;
    }

    /**
     * Define el valor de la propiedad title.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTitle(String value) {
        this.title = value;
    }

    /**
     * Obtiene el valor de la propiedad sequence.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getSequence() {
        return sequence;
    }

    /**
     * Define el valor de la propiedad sequence.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setSequence(Long value) {
        this.sequence = value;
    }

    /**
     * Obtiene el valor de la propiedad imageInterleave.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getImageInterleave() {
        return imageInterleave;
    }

    /**
     * Define el valor de la propiedad imageInterleave.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setImageInterleave(Long value) {
        this.imageInterleave = value;
    }

    /**
     * Obtiene el valor de la propiedad interleave.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getInterleave() {
        return interleave;
    }

    /**
     * Define el valor de la propiedad interleave.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setInterleave(Long value) {
        this.interleave = value;
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
     * Obtiene el valor de la propiedad package.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPackage() {
        return _package;
    }

    /**
     * Define el valor de la propiedad package.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPackage(String value) {
        this._package = value;
    }

    /**
     * Obtiene el valor de la propiedad interface.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInterface() {
        return _interface;
    }

    /**
     * Define el valor de la propiedad interface.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInterface(String value) {
        this._interface = value;
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

    /**
     * Obtiene el valor de la propiedad pcmcia.
     * 
     * @return
     *     possible object is
     *     {@link PCMCIAType }
     *     
     */
    public PCMCIAType getPCMCIA() {
        return pcmcia;
    }

    /**
     * Define el valor de la propiedad pcmcia.
     * 
     * @param value
     *     allowed object is
     *     {@link PCMCIAType }
     *     
     */
    public void setPCMCIA(PCMCIAType value) {
        this.pcmcia = value;
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

}
