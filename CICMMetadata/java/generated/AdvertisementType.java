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
 * <p>Clase Java para AdvertisementType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="AdvertisementType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Manufacturer" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="Product" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="File" type="{}FileType"/>
 *         &lt;element name="FileSize" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Frames" type="{http://www.w3.org/2001/XMLSchema}unsignedLong" minOccurs="0"/>
 *         &lt;element name="Duration" type="{http://www.w3.org/2001/XMLSchema}double"/>
 *         &lt;element name="MeanFrameRate" type="{http://www.w3.org/2001/XMLSchema}float" minOccurs="0"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="AudioTrack" type="{}AudioTracksType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="VideoTrack" type="{}VideoTracksType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="SubtitleTrack" type="{}SubtitleTracksType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Recording" type="{}RecordingType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "AdvertisementType", propOrder = {
    "manufacturer",
    "product",
    "file",
    "fileSize",
    "frames",
    "duration",
    "meanFrameRate",
    "checksums",
    "audioTrack",
    "videoTrack",
    "subtitleTrack",
    "recording"
})
public class AdvertisementType {

    @XmlElement(name = "Manufacturer", required = true)
    protected String manufacturer;
    @XmlElement(name = "Product", required = true)
    protected String product;
    @XmlElement(name = "File", required = true)
    protected FileType file;
    @XmlElement(name = "FileSize", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger fileSize;
    @XmlElement(name = "Frames")
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger frames;
    @XmlElement(name = "Duration")
    protected double duration;
    @XmlElement(name = "MeanFrameRate")
    protected Float meanFrameRate;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "AudioTrack")
    protected List<AudioTracksType> audioTrack;
    @XmlElement(name = "VideoTrack")
    protected List<VideoTracksType> videoTrack;
    @XmlElement(name = "SubtitleTrack")
    protected List<SubtitleTracksType> subtitleTrack;
    @XmlElement(name = "Recording")
    protected RecordingType recording;

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
     * Obtiene el valor de la propiedad product.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getProduct() {
        return product;
    }

    /**
     * Define el valor de la propiedad product.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setProduct(String value) {
        this.product = value;
    }

    /**
     * Obtiene el valor de la propiedad file.
     * 
     * @return
     *     possible object is
     *     {@link FileType }
     *     
     */
    public FileType getFile() {
        return file;
    }

    /**
     * Define el valor de la propiedad file.
     * 
     * @param value
     *     allowed object is
     *     {@link FileType }
     *     
     */
    public void setFile(FileType value) {
        this.file = value;
    }

    /**
     * Obtiene el valor de la propiedad fileSize.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getFileSize() {
        return fileSize;
    }

    /**
     * Define el valor de la propiedad fileSize.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setFileSize(BigInteger value) {
        this.fileSize = value;
    }

    /**
     * Obtiene el valor de la propiedad frames.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getFrames() {
        return frames;
    }

    /**
     * Define el valor de la propiedad frames.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setFrames(BigInteger value) {
        this.frames = value;
    }

    /**
     * Obtiene el valor de la propiedad duration.
     * 
     */
    public double getDuration() {
        return duration;
    }

    /**
     * Define el valor de la propiedad duration.
     * 
     */
    public void setDuration(double value) {
        this.duration = value;
    }

    /**
     * Obtiene el valor de la propiedad meanFrameRate.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getMeanFrameRate() {
        return meanFrameRate;
    }

    /**
     * Define el valor de la propiedad meanFrameRate.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setMeanFrameRate(Float value) {
        this.meanFrameRate = value;
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
     * Gets the value of the audioTrack property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the audioTrack property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAudioTrack().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AudioTracksType }
     * 
     * 
     */
    public List<AudioTracksType> getAudioTrack() {
        if (audioTrack == null) {
            audioTrack = new ArrayList<AudioTracksType>();
        }
        return this.audioTrack;
    }

    /**
     * Gets the value of the videoTrack property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the videoTrack property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getVideoTrack().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link VideoTracksType }
     * 
     * 
     */
    public List<VideoTracksType> getVideoTrack() {
        if (videoTrack == null) {
            videoTrack = new ArrayList<VideoTracksType>();
        }
        return this.videoTrack;
    }

    /**
     * Gets the value of the subtitleTrack property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the subtitleTrack property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSubtitleTrack().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SubtitleTracksType }
     * 
     * 
     */
    public List<SubtitleTracksType> getSubtitleTrack() {
        if (subtitleTrack == null) {
            subtitleTrack = new ArrayList<SubtitleTracksType>();
        }
        return this.subtitleTrack;
    }

    /**
     * Obtiene el valor de la propiedad recording.
     * 
     * @return
     *     possible object is
     *     {@link RecordingType }
     *     
     */
    public RecordingType getRecording() {
        return recording;
    }

    /**
     * Define el valor de la propiedad recording.
     * 
     * @param value
     *     allowed object is
     *     {@link RecordingType }
     *     
     */
    public void setRecording(RecordingType value) {
        this.recording = value;
    }

}
