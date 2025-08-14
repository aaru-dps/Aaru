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
 * Track information
 * 
 * <p>Clase Java para TrackType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="TrackType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Sequence" type="{}TrackSequenceType"/>
 *         &lt;element name="StartMSF" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="EndMSF" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="StartSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="EndSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Indexes" type="{}TrackIndexesType" minOccurs="0"/>
 *         &lt;element name="Flags" type="{}TrackFlagsType" minOccurs="0"/>
 *         &lt;element name="ISRC" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="TrackType">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="audio"/>
 *               &lt;enumeration value="mode0"/>
 *               &lt;enumeration value="mode1"/>
 *               &lt;enumeration value="mode2"/>
 *               &lt;enumeration value="m2f1"/>
 *               &lt;enumeration value="m2f2"/>
 *               &lt;enumeration value="dvd"/>
 *               &lt;enumeration value="hddvd"/>
 *               &lt;enumeration value="bluray"/>
 *               &lt;enumeration value="ddcd"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="BytesPerSector" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="AccoustID" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="SubChannel" type="{}SubChannelType" minOccurs="0"/>
 *         &lt;element name="FileSystemInformation" type="{}FileSystemInformationType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TrackType", propOrder = {
    "image",
    "size",
    "sequence",
    "startMSF",
    "endMSF",
    "startSector",
    "endSector",
    "indexes",
    "flags",
    "isrc",
    "trackType",
    "bytesPerSector",
    "accoustID",
    "checksums",
    "subChannel",
    "fileSystemInformation"
})
public class TrackType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "Sequence", required = true)
    protected TrackSequenceType sequence;
    @XmlElement(name = "StartMSF")
    protected String startMSF;
    @XmlElement(name = "EndMSF")
    protected String endMSF;
    @XmlElement(name = "StartSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger startSector;
    @XmlElement(name = "EndSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger endSector;
    @XmlElement(name = "Indexes")
    protected TrackIndexesType indexes;
    @XmlElement(name = "Flags")
    protected TrackFlagsType flags;
    @XmlElement(name = "ISRC")
    protected String isrc;
    @XmlElement(name = "TrackType", required = true)
    protected String trackType;
    @XmlElement(name = "BytesPerSector")
    @XmlSchemaType(name = "unsignedInt")
    protected long bytesPerSector;
    @XmlElement(name = "AccoustID")
    protected String accoustID;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "SubChannel")
    protected SubChannelType subChannel;
    @XmlElement(name = "FileSystemInformation")
    protected FileSystemInformationType fileSystemInformation;

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
     *     {@link TrackSequenceType }
     *     
     */
    public TrackSequenceType getSequence() {
        return sequence;
    }

    /**
     * Define el valor de la propiedad sequence.
     * 
     * @param value
     *     allowed object is
     *     {@link TrackSequenceType }
     *     
     */
    public void setSequence(TrackSequenceType value) {
        this.sequence = value;
    }

    /**
     * Obtiene el valor de la propiedad startMSF.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getStartMSF() {
        return startMSF;
    }

    /**
     * Define el valor de la propiedad startMSF.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setStartMSF(String value) {
        this.startMSF = value;
    }

    /**
     * Obtiene el valor de la propiedad endMSF.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getEndMSF() {
        return endMSF;
    }

    /**
     * Define el valor de la propiedad endMSF.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setEndMSF(String value) {
        this.endMSF = value;
    }

    /**
     * Obtiene el valor de la propiedad startSector.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getStartSector() {
        return startSector;
    }

    /**
     * Define el valor de la propiedad startSector.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setStartSector(BigInteger value) {
        this.startSector = value;
    }

    /**
     * Obtiene el valor de la propiedad endSector.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getEndSector() {
        return endSector;
    }

    /**
     * Define el valor de la propiedad endSector.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setEndSector(BigInteger value) {
        this.endSector = value;
    }

    /**
     * Obtiene el valor de la propiedad indexes.
     * 
     * @return
     *     possible object is
     *     {@link TrackIndexesType }
     *     
     */
    public TrackIndexesType getIndexes() {
        return indexes;
    }

    /**
     * Define el valor de la propiedad indexes.
     * 
     * @param value
     *     allowed object is
     *     {@link TrackIndexesType }
     *     
     */
    public void setIndexes(TrackIndexesType value) {
        this.indexes = value;
    }

    /**
     * Obtiene el valor de la propiedad flags.
     * 
     * @return
     *     possible object is
     *     {@link TrackFlagsType }
     *     
     */
    public TrackFlagsType getFlags() {
        return flags;
    }

    /**
     * Define el valor de la propiedad flags.
     * 
     * @param value
     *     allowed object is
     *     {@link TrackFlagsType }
     *     
     */
    public void setFlags(TrackFlagsType value) {
        this.flags = value;
    }

    /**
     * Obtiene el valor de la propiedad isrc.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getISRC() {
        return isrc;
    }

    /**
     * Define el valor de la propiedad isrc.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setISRC(String value) {
        this.isrc = value;
    }

    /**
     * Obtiene el valor de la propiedad trackType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTrackType() {
        return trackType;
    }

    /**
     * Define el valor de la propiedad trackType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTrackType(String value) {
        this.trackType = value;
    }

    /**
     * Obtiene el valor de la propiedad bytesPerSector.
     * 
     */
    public long getBytesPerSector() {
        return bytesPerSector;
    }

    /**
     * Define el valor de la propiedad bytesPerSector.
     * 
     */
    public void setBytesPerSector(long value) {
        this.bytesPerSector = value;
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
     * Obtiene el valor de la propiedad subChannel.
     * 
     * @return
     *     possible object is
     *     {@link SubChannelType }
     *     
     */
    public SubChannelType getSubChannel() {
        return subChannel;
    }

    /**
     * Define el valor de la propiedad subChannel.
     * 
     * @param value
     *     allowed object is
     *     {@link SubChannelType }
     *     
     */
    public void setSubChannel(SubChannelType value) {
        this.subChannel = value;
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

}
