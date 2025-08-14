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
 * Information about track in non-abstracted block based media
 * 
 * <p>Clase Java para BlockTrackType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="BlockTrackType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Image" type="{}ImageType"/>
 *         &lt;element name="Size" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Head" type="{http://www.w3.org/2001/XMLSchema}unsignedShort"/>
 *         &lt;element name="Cylinder" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="StartSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="EndSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Sectors" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="BytesPerSector" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="Format" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "BlockTrackType", propOrder = {
    "image",
    "size",
    "head",
    "cylinder",
    "startSector",
    "endSector",
    "sectors",
    "bytesPerSector",
    "checksums",
    "format"
})
public class BlockTrackType {

    @XmlElement(name = "Image", required = true)
    protected ImageType image;
    @XmlElement(name = "Size", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger size;
    @XmlElement(name = "Head")
    @XmlSchemaType(name = "unsignedShort")
    protected int head;
    @XmlElement(name = "Cylinder")
    @XmlSchemaType(name = "unsignedInt")
    protected long cylinder;
    @XmlElement(name = "StartSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger startSector;
    @XmlElement(name = "EndSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger endSector;
    @XmlElement(name = "Sectors", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger sectors;
    @XmlElement(name = "BytesPerSector")
    @XmlSchemaType(name = "unsignedInt")
    protected long bytesPerSector;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "Format")
    protected String format;

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
     * Obtiene el valor de la propiedad head.
     * 
     */
    public int getHead() {
        return head;
    }

    /**
     * Define el valor de la propiedad head.
     * 
     */
    public void setHead(int value) {
        this.head = value;
    }

    /**
     * Obtiene el valor de la propiedad cylinder.
     * 
     */
    public long getCylinder() {
        return cylinder;
    }

    /**
     * Define el valor de la propiedad cylinder.
     * 
     */
    public void setCylinder(long value) {
        this.cylinder = value;
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
     * Obtiene el valor de la propiedad sectors.
     * 
     * @return
     *     possible object is
     *     {@link BigInteger }
     *     
     */
    public BigInteger getSectors() {
        return sectors;
    }

    /**
     * Define el valor de la propiedad sectors.
     * 
     * @param value
     *     allowed object is
     *     {@link BigInteger }
     *     
     */
    public void setSectors(BigInteger value) {
        this.sectors = value;
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
     * Obtiene el valor de la propiedad format.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFormat() {
        return format;
    }

    /**
     * Define el valor de la propiedad format.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFormat(String value) {
        this.format = value;
    }

}
