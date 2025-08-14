//
// Este archivo ha sido generado por la arquitectura JavaTM para la implantación de la referencia de enlace (JAXB) XML v2.2.8-b130911.1802 
// Visite <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Todas las modificaciones realizadas en este archivo se perderán si se vuelve a compilar el esquema de origen. 
// Generado el: 2020.07.12 a las 10:42:39 PM WEST 
//


package generated;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Clase Java para AudioTracksType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="AudioTracksType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Languages" type="{}LanguagesType" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="TrackNumber" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedInt">
 *             &lt;minInclusive value="1"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="AccoustID" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="Codec" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="Channels" use="required" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" />
 *       &lt;attribute name="SampleRate" use="required" type="{http://www.w3.org/2001/XMLSchema}double" />
 *       &lt;attribute name="MeanBitrate" use="required" type="{http://www.w3.org/2001/XMLSchema}long" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "AudioTracksType", propOrder = {
    "languages"
})
public class AudioTracksType {

    @XmlElement(name = "Languages")
    protected LanguagesType languages;
    @XmlAttribute(name = "TrackNumber", required = true)
    protected long trackNumber;
    @XmlAttribute(name = "AccoustID")
    protected String accoustID;
    @XmlAttribute(name = "Codec", required = true)
    protected String codec;
    @XmlAttribute(name = "Channels", required = true)
    @XmlSchemaType(name = "unsignedInt")
    protected long channels;
    @XmlAttribute(name = "SampleRate", required = true)
    protected double sampleRate;
    @XmlAttribute(name = "MeanBitrate", required = true)
    protected long meanBitrate;

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
     * Obtiene el valor de la propiedad trackNumber.
     * 
     */
    public long getTrackNumber() {
        return trackNumber;
    }

    /**
     * Define el valor de la propiedad trackNumber.
     * 
     */
    public void setTrackNumber(long value) {
        this.trackNumber = value;
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
     * Obtiene el valor de la propiedad codec.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCodec() {
        return codec;
    }

    /**
     * Define el valor de la propiedad codec.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCodec(String value) {
        this.codec = value;
    }

    /**
     * Obtiene el valor de la propiedad channels.
     * 
     */
    public long getChannels() {
        return channels;
    }

    /**
     * Define el valor de la propiedad channels.
     * 
     */
    public void setChannels(long value) {
        this.channels = value;
    }

    /**
     * Obtiene el valor de la propiedad sampleRate.
     * 
     */
    public double getSampleRate() {
        return sampleRate;
    }

    /**
     * Define el valor de la propiedad sampleRate.
     * 
     */
    public void setSampleRate(double value) {
        this.sampleRate = value;
    }

    /**
     * Obtiene el valor de la propiedad meanBitrate.
     * 
     */
    public long getMeanBitrate() {
        return meanBitrate;
    }

    /**
     * Define el valor de la propiedad meanBitrate.
     * 
     */
    public void setMeanBitrate(long value) {
        this.meanBitrate = value;
    }

}
