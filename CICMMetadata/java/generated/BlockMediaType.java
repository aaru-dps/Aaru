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
 * Describes a dump of a block (sector) layered media
 * 
 * <p>Clase Java para BlockMediaType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="BlockMediaType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="ContentChecksums" type="{}ChecksumsType"/>
 *         &lt;element name="Sequence" type="{}SequenceType"/>
 *         &lt;element name="Manufacturer" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Model" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Serial" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Firmware" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Interface" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="PartNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SerialNumber" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="PhysicalBlockSize" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="LogicalBlockSize" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="LogicalBlocks" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="VariableBlockSize" type="{}VariableBlockSizeType" minOccurs="0"/>
 *         &lt;element name="TapeInformation" type="{}TapeInformationType" minOccurs="0"/>
 *         &lt;element name="Scans" type="{}ScansType" minOccurs="0"/>
 *         &lt;element name="ATA" type="{}ATAType" minOccurs="0"/>
 *         &lt;element name="PCI" type="{}PCIType" minOccurs="0"/>
 *         &lt;element name="PCMCIA" type="{}PCMCIAType" minOccurs="0"/>
 *         &lt;element name="SecureDigital" type="{}SecureDigitalType" minOccurs="0"/>
 *         &lt;element name="MultiMediaCard" type="{}MultiMediaCardType" minOccurs="0"/>
 *         &lt;element name="SCSI" type="{}SCSIType" minOccurs="0"/>
 *         &lt;element name="USB" type="{}USBType" minOccurs="0"/>
 *         &lt;element name="MAM" type="{}DumpType" minOccurs="0"/>
 *         &lt;element name="Heads" type="{http://www.w3.org/2001/XMLSchema}unsignedShort" minOccurs="0"/>
 *         &lt;element name="Cylinders" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" minOccurs="0"/>
 *         &lt;element name="SectorsPerTrack" type="{http://www.w3.org/2001/XMLSchema}unsignedLong" minOccurs="0"/>
 *         &lt;element name="Track" type="{}BlockTrackType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="CopyProtection" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Dimensions" type="{}DimensionsType"/>
 *         &lt;element name="FileSystemInformation" type="{}FileSystemInformationType" minOccurs="0"/>
 *         &lt;element name="DumpHardwareArray" type="{}DumpHardwareArrayType" minOccurs="0"/>
 *         &lt;element name="DiskType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="DiskSubType" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "BlockMediaType", propOrder = {
    "image",
    "size",
    "checksums",
    "contentChecksums",
    "sequence",
    "manufacturer",
    "model",
    "serial",
    "firmware",
    "_interface",
    "partNumber",
    "serialNumber",
    "physicalBlockSize",
    "logicalBlockSize",
    "logicalBlocks",
    "variableBlockSize",
    "tapeInformation",
    "scans",
    "ata",
    "pci",
    "pcmcia",
    "secureDigital",
    "multiMediaCard",
    "scsi",
    "usb",
    "mam",
    "heads",
    "cylinders",
    "sectorsPerTrack",
    "track",
    "copyProtection",
    "dimensions",
    "fileSystemInformation",
    "dumpHardwareArray",
    "diskType",
    "diskSubType"
})
public class BlockMediaType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "ContentChecksums", required = true)
    protected ChecksumsType contentChecksums;
    @XmlElement(name = "Sequence", required = true)
    protected SequenceType sequence;
    @XmlElement(name = "Manufacturer")
    protected String manufacturer;
    @XmlElement(name = "Model")
    protected String model;
    @XmlElement(name = "Serial")
    protected String serial;
    @XmlElement(name = "Firmware")
    protected String firmware;
    @XmlElement(name = "Interface")
    protected String _interface;
    @XmlElement(name = "PartNumber")
    protected String partNumber;
    @XmlElement(name = "SerialNumber")
    protected String serialNumber;
    @XmlElement(name = "PhysicalBlockSize")
    @XmlSchemaType(name = "unsignedInt")
    protected long physicalBlockSize;
    @XmlElement(name = "LogicalBlockSize")
    @XmlSchemaType(name = "unsignedInt")
    protected long logicalBlockSize;
    @XmlElement(name = "LogicalBlocks", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger logicalBlocks;
    @XmlElement(name = "VariableBlockSize")
    protected VariableBlockSizeType variableBlockSize;
    @XmlElement(name = "TapeInformation")
    protected TapeInformationType tapeInformation;
    @XmlElement(name = "Scans")
    protected ScansType scans;
    @XmlElement(name = "ATA")
    protected ATAType ata;
    @XmlElement(name = "PCI")
    protected PCIType pci;
    @XmlElement(name = "PCMCIA")
    protected PCMCIAType pcmcia;
    @XmlElement(name = "SecureDigital")
    protected SecureDigitalType secureDigital;
    @XmlElement(name = "MultiMediaCard")
    protected MultiMediaCardType multiMediaCard;
    @XmlElement(name = "SCSI")
    protected SCSIType scsi;
    @XmlElement(name = "USB")
    protected USBType usb;
    @XmlElement(name = "MAM")
    protected DumpType mam;
    @XmlElement(name = "Heads")
    @XmlSchemaType(name = "unsignedShort")
    protected Integer heads;
    @XmlElement(name = "Cylinders")
    @XmlSchemaType(name = "unsignedInt")
    protected Long cylinders;
    @XmlElement(name = "SectorsPerTrack")
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger sectorsPerTrack;
    @XmlElement(name = "Track")
    protected List<BlockTrackType> track;
    @XmlElement(name = "CopyProtection")
    protected String copyProtection;
    @XmlElement(name = "Dimensions", required = true)
    protected DimensionsType dimensions;
    @XmlElement(name = "FileSystemInformation")
    protected FileSystemInformationType fileSystemInformation;
    @XmlElement(name = "DumpHardwareArray")
    protected DumpHardwareArrayType dumpHardwareArray;
    @XmlElement(name = "DiskType", required = true)
    protected String diskType;
    @XmlElement(name = "DiskSubType", required = true)
    protected String diskSubType;

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
     * Obtiene el valor de la propiedad contentChecksums.
     * 
     * @return
     *     possible object is
     *     {@link ChecksumsType }
     *     
     */
    public ChecksumsType getContentChecksums() {
        return contentChecksums;
    }

    /**
     * Define el valor de la propiedad contentChecksums.
     * 
     * @param value
     *     allowed object is
     *     {@link ChecksumsType }
     *     
     */
    public void setContentChecksums(ChecksumsType value) {
        this.contentChecksums = value;
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
     * Obtiene el valor de la propiedad serial.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSerial() {
        return serial;
    }

    /**
     * Define el valor de la propiedad serial.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSerial(String value) {
        this.serial = value;
    }

    /**
     * Obtiene el valor de la propiedad firmware.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFirmware() {
        return firmware;
    }

    /**
     * Define el valor de la propiedad firmware.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFirmware(String value) {
        this.firmware = value;
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
     * Obtiene el valor de la propiedad physicalBlockSize.
     * 
     */
    public long getPhysicalBlockSize() {
        return physicalBlockSize;
    }

    /**
     * Define el valor de la propiedad physicalBlockSize.
     * 
     */
    public void setPhysicalBlockSize(long value) {
        this.physicalBlockSize = value;
    }

    /**
     * Obtiene el valor de la propiedad logicalBlockSize.
     * 
     */
    public long getLogicalBlockSize() {
        return logicalBlockSize;
    }

    /**
     * Define el valor de la propiedad logicalBlockSize.
     * 
     */
    public void setLogicalBlockSize(long value) {
        this.logicalBlockSize = value;
    }

    /**
     * Obtiene el valor de la propiedad logicalBlocks.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getLogicalBlocks() {
        return logicalBlocks;
    }

    /**
     * Define el valor de la propiedad logicalBlocks.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setLogicalBlocks(BigInteger value) {
        this.logicalBlocks = value;
    }

    /**
     * Obtiene el valor de la propiedad variableBlockSize.
     * 
     * @return
     *     possible object is
     *     {@link VariableBlockSizeType }
     *     
     */
    public VariableBlockSizeType getVariableBlockSize() {
        return variableBlockSize;
    }

    /**
     * Define el valor de la propiedad variableBlockSize.
     * 
     * @param value
     *     allowed object is
     *     {@link VariableBlockSizeType }
     *     
     */
    public void setVariableBlockSize(VariableBlockSizeType value) {
        this.variableBlockSize = value;
    }

    /**
     * Obtiene el valor de la propiedad tapeInformation.
     * 
     * @return
     *     possible object is
     *     {@link TapeInformationType }
     *     
     */
    public TapeInformationType getTapeInformation() {
        return tapeInformation;
    }

    /**
     * Define el valor de la propiedad tapeInformation.
     * 
     * @param value
     *     allowed object is
     *     {@link TapeInformationType }
     *     
     */
    public void setTapeInformation(TapeInformationType value) {
        this.tapeInformation = value;
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
     * Obtiene el valor de la propiedad ata.
     * 
     * @return
     *     possible object is
     *     {@link ATAType }
     *     
     */
    public ATAType getATA() {
        return ata;
    }

    /**
     * Define el valor de la propiedad ata.
     * 
     * @param value
     *     allowed object is
     *     {@link ATAType }
     *     
     */
    public void setATA(ATAType value) {
        this.ata = value;
    }

    /**
     * Obtiene el valor de la propiedad pci.
     * 
     * @return
     *     possible object is
     *     {@link PCIType }
     *     
     */
    public PCIType getPCI() {
        return pci;
    }

    /**
     * Define el valor de la propiedad pci.
     * 
     * @param value
     *     allowed object is
     *     {@link PCIType }
     *     
     */
    public void setPCI(PCIType value) {
        this.pci = value;
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
     * Obtiene el valor de la propiedad secureDigital.
     * 
     * @return
     *     possible object is
     *     {@link SecureDigitalType }
     *     
     */
    public SecureDigitalType getSecureDigital() {
        return secureDigital;
    }

    /**
     * Define el valor de la propiedad secureDigital.
     * 
     * @param value
     *     allowed object is
     *     {@link SecureDigitalType }
     *     
     */
    public void setSecureDigital(SecureDigitalType value) {
        this.secureDigital = value;
    }

    /**
     * Obtiene el valor de la propiedad multiMediaCard.
     * 
     * @return
     *     possible object is
     *     {@link MultiMediaCardType }
     *     
     */
    public MultiMediaCardType getMultiMediaCard() {
        return multiMediaCard;
    }

    /**
     * Define el valor de la propiedad multiMediaCard.
     * 
     * @param value
     *     allowed object is
     *     {@link MultiMediaCardType }
     *     
     */
    public void setMultiMediaCard(MultiMediaCardType value) {
        this.multiMediaCard = value;
    }

    /**
     * Obtiene el valor de la propiedad scsi.
     * 
     * @return
     *     possible object is
     *     {@link SCSIType }
     *     
     */
    public SCSIType getSCSI() {
        return scsi;
    }

    /**
     * Define el valor de la propiedad scsi.
     * 
     * @param value
     *     allowed object is
     *     {@link SCSIType }
     *     
     */
    public void setSCSI(SCSIType value) {
        this.scsi = value;
    }

    /**
     * Obtiene el valor de la propiedad usb.
     * 
     * @return
     *     possible object is
     *     {@link USBType }
     *     
     */
    public USBType getUSB() {
        return usb;
    }

    /**
     * Define el valor de la propiedad usb.
     * 
     * @param value
     *     allowed object is
     *     {@link USBType }
     *     
     */
    public void setUSB(USBType value) {
        this.usb = value;
    }

    /**
     * Obtiene el valor de la propiedad mam.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getMAM() {
        return mam;
    }

    /**
     * Define el valor de la propiedad mam.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setMAM(DumpType value) {
        this.mam = value;
    }

    /**
     * Obtiene el valor de la propiedad heads.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getHeads() {
        return heads;
    }

    /**
     * Define el valor de la propiedad heads.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setHeads(Integer value) {
        this.heads = value;
    }

    /**
     * Obtiene el valor de la propiedad cylinders.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getCylinders() {
        return cylinders;
    }

    /**
     * Define el valor de la propiedad cylinders.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setCylinders(Long value) {
        this.cylinders = value;
    }

    /**
     * Obtiene el valor de la propiedad sectorsPerTrack.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getSectorsPerTrack() {
        return sectorsPerTrack;
    }

    /**
     * Define el valor de la propiedad sectorsPerTrack.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setSectorsPerTrack(BigInteger value) {
        this.sectorsPerTrack = value;
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
     * {@link BlockTrackType }
     * 
     * 
     */
    public List<BlockTrackType> getTrack() {
        if (track == null) {
            track = new ArrayList<BlockTrackType>();
        }
        return this.track;
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
     * Obtiene el valor de la propiedad fileSystemInformation.
     * 
     * @return
     *     possible object is
     *     {@link FileSystemInformationType }
     *     
     */
    public FileSystemInformationType getFileSystemInformation() {
        return fileSystemInformation;
    }

    /**
     * Define el valor de la propiedad fileSystemInformation.
     * 
     * @param value
     *     allowed object is
     *     {@link FileSystemInformationType }
     *     
     */
    public void setFileSystemInformation(FileSystemInformationType value) {
        this.fileSystemInformation = value;
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
     * Obtiene el valor de la propiedad diskType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDiskType() {
        return diskType;
    }

    /**
     * Define el valor de la propiedad diskType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDiskType(String value) {
        this.diskType = value;
    }

    /**
     * Obtiene el valor de la propiedad diskSubType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDiskSubType() {
        return diskSubType;
    }

    /**
     * Define el valor de la propiedad diskSubType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDiskSubType(String value) {
        this.diskSubType = value;
    }

}
