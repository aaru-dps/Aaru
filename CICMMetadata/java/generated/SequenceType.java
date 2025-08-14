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


/**
 * Sequence information about a disc
 *             
 * 
 * <p>Clase Java para SequenceType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="SequenceType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="MediaTitle" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="MediaSequence" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="TotalMedia">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedInt">
 *               &lt;minInclusive value="1"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Side" minOccurs="0">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedByte">
 *               &lt;maxInclusive value="2"/>
 *               &lt;minInclusive value="1"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Layer" minOccurs="0">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedByte">
 *               &lt;minInclusive value="0"/>
 *               &lt;maxInclusive value="1"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SequenceType", propOrder = {
    "mediaTitle",
    "mediaSequence",
    "totalMedia",
    "side",
    "layer"
})
public class SequenceType {

    @XmlElement(name = "MediaTitle")
    protected String mediaTitle;
    @XmlElement(name = "MediaSequence")
    @XmlSchemaType(name = "unsignedInt")
    protected long mediaSequence;
    @XmlElement(name = "TotalMedia")
    protected long totalMedia;
    @XmlElement(name = "Side")
    protected Short side;
    @XmlElement(name = "Layer")
    protected Short layer;

    /**
     * Obtiene el valor de la propiedad mediaTitle.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMediaTitle() {
        return mediaTitle;
    }

    /**
     * Define el valor de la propiedad mediaTitle.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMediaTitle(String value) {
        this.mediaTitle = value;
    }

    /**
     * Obtiene el valor de la propiedad mediaSequence.
     * 
     */
    public long getMediaSequence() {
        return mediaSequence;
    }

    /**
     * Define el valor de la propiedad mediaSequence.
     * 
     */
    public void setMediaSequence(long value) {
        this.mediaSequence = value;
    }

    /**
     * Obtiene el valor de la propiedad totalMedia.
     * 
     */
    public long getTotalMedia() {
        return totalMedia;
    }

    /**
     * Define el valor de la propiedad totalMedia.
     * 
     */
    public void setTotalMedia(long value) {
        this.totalMedia = value;
    }

    /**
     * Obtiene el valor de la propiedad side.
     * 
     * @return
     *     possible object is
     *     {@link Short }
     *     
     */
    public Short getSide() {
        return side;
    }

    /**
     * Define el valor de la propiedad side.
     * 
     * @param value
     *     allowed object is
     *     {@link Short }
     *     
     */
    public void setSide(Short value) {
        this.side = value;
    }

    /**
     * Obtiene el valor de la propiedad layer.
     * 
     * @return
     *     possible object is
     *     {@link Short }
     *     
     */
    public Short getLayer() {
        return layer;
    }

    /**
     * Define el valor de la propiedad layer.
     * 
     * @param value
     *     allowed object is
     *     {@link Short }
     *     
     */
    public void setLayer(Short value) {
        this.layer = value;
    }

}
