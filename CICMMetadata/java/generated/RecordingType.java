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
 * <p>Clase Java para RecordingType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="RecordingType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Broadcaster" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="BroadcastPlatform" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="SourceFormat">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="ITU-A"/>
 *               &lt;enumeration value="ITU-B"/>
 *               &lt;enumeration value="ITU-C"/>
 *               &lt;enumeration value="ITU-D"/>
 *               &lt;enumeration value="ITU-E"/>
 *               &lt;enumeration value="ITU-F"/>
 *               &lt;enumeration value="ITU-G"/>
 *               &lt;enumeration value="ITU-H"/>
 *               &lt;enumeration value="ITU-I"/>
 *               &lt;enumeration value="ITU-J"/>
 *               &lt;enumeration value="ITU-K"/>
 *               &lt;enumeration value="ITU-L"/>
 *               &lt;enumeration value="ITU-M"/>
 *               &lt;enumeration value="ITU-N"/>
 *               &lt;enumeration value="PAL-B"/>
 *               &lt;enumeration value="SECAM-B"/>
 *               &lt;enumeration value="PAL-D"/>
 *               &lt;enumeration value="SECAM-D"/>
 *               &lt;enumeration value="PAL-G"/>
 *               &lt;enumeration value="SECAM-G"/>
 *               &lt;enumeration value="PAL-H"/>
 *               &lt;enumeration value="PAL-I"/>
 *               &lt;enumeration value="PAL-K"/>
 *               &lt;enumeration value="SECAM-K"/>
 *               &lt;enumeration value="NTSC-M"/>
 *               &lt;enumeration value="PAL-N"/>
 *               &lt;enumeration value="PAL-M"/>
 *               &lt;enumeration value="SECAM-M"/>
 *               &lt;enumeration value="MUSE"/>
 *               &lt;enumeration value="PALplus"/>
 *               &lt;enumeration value="FM"/>
 *               &lt;enumeration value="AM"/>
 *               &lt;enumeration value="COFDM"/>
 *               &lt;enumeration value="CAM-D"/>
 *               &lt;enumeration value="DAB"/>
 *               &lt;enumeration value="DAB+"/>
 *               &lt;enumeration value="DRM"/>
 *               &lt;enumeration value="DRM+"/>
 *               &lt;enumeration value="FMeXtra"/>
 *               &lt;enumeration value="ATSC"/>
 *               &lt;enumeration value="ATSC2"/>
 *               &lt;enumeration value="ATSC3"/>
 *               &lt;enumeration value="ATSC-M/H"/>
 *               &lt;enumeration value="DVB-T"/>
 *               &lt;enumeration value="DVB-T2"/>
 *               &lt;enumeration value="DVB-S"/>
 *               &lt;enumeration value="DVB-S2"/>
 *               &lt;enumeration value="DVB-S2X"/>
 *               &lt;enumeration value="DVB-C"/>
 *               &lt;enumeration value="DVB-C2"/>
 *               &lt;enumeration value="DVB-H"/>
 *               &lt;enumeration value="DVB-NGH"/>
 *               &lt;enumeration value="DVB-SH"/>
 *               &lt;enumeration value="ISDB-T"/>
 *               &lt;enumeration value="ISDB-Tb"/>
 *               &lt;enumeration value="ISDB-S"/>
 *               &lt;enumeration value="ISDB-C"/>
 *               &lt;enumeration value="1seg"/>
 *               &lt;enumeration value="DTMB"/>
 *               &lt;enumeration value="CCMB"/>
 *               &lt;enumeration value="T-DMB"/>
 *               &lt;enumeration value="S-DMB"/>
 *               &lt;enumeration value="IPTV"/>
 *               &lt;enumeration value="DVB-MT"/>
 *               &lt;enumeration value="DVB-MC"/>
 *               &lt;enumeration value="DVB-MS"/>
 *               &lt;enumeration value="ADR"/>
 *               &lt;enumeration value="SDR"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Timestamp" type="{http://www.w3.org/2001/XMLSchema}dateTime"/>
 *         &lt;element name="Software" type="{}SoftwareType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="Coordinates" type="{}CoordinatesType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "RecordingType", propOrder = {
    "broadcaster",
    "broadcastPlatform",
    "sourceFormat",
    "timestamp",
    "software",
    "coordinates"
})
public class RecordingType {

    @XmlElement(name = "Broadcaster")
    protected String broadcaster;
    @XmlElement(name = "BroadcastPlatform")
    protected String broadcastPlatform;
    @XmlElement(name = "SourceFormat", required = true)
    protected String sourceFormat;
    @XmlElement(name = "Timestamp", required = true)
    @XmlSchemaType(name = "dateTime")
    protected XMLGregorianCalendar timestamp;
    @XmlElement(name = "Software")
    protected List<SoftwareType> software;
    @XmlElement(name = "Coordinates")
    protected CoordinatesType coordinates;

    /**
     * Obtiene el valor de la propiedad broadcaster.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getBroadcaster() {
        return broadcaster;
    }

    /**
     * Define el valor de la propiedad broadcaster.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setBroadcaster(String value) {
        this.broadcaster = value;
    }

    /**
     * Obtiene el valor de la propiedad broadcastPlatform.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getBroadcastPlatform() {
        return broadcastPlatform;
    }

    /**
     * Define el valor de la propiedad broadcastPlatform.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setBroadcastPlatform(String value) {
        this.broadcastPlatform = value;
    }

    /**
     * Obtiene el valor de la propiedad sourceFormat.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSourceFormat() {
        return sourceFormat;
    }

    /**
     * Define el valor de la propiedad sourceFormat.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSourceFormat(String value) {
        this.sourceFormat = value;
    }

    /**
     * Obtiene el valor de la propiedad timestamp.
     * 
     * @return
     *     possible object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public XMLGregorianCalendar getTimestamp() {
        return timestamp;
    }

    /**
     * Define el valor de la propiedad timestamp.
     * 
     * @param value
     *     allowed object is
     *     {@link XMLGregorianCalendar }
     *     
     */
    public void setTimestamp(XMLGregorianCalendar value) {
        this.timestamp = value;
    }

    /**
     * Gets the value of the software property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the software property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSoftware().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SoftwareType }
     * 
     * 
     */
    public List<SoftwareType> getSoftware() {
        if (software == null) {
            software = new ArrayList<SoftwareType>();
        }
        return this.software;
    }

    /**
     * Obtiene el valor de la propiedad coordinates.
     * 
     * @return
     *     possible object is
     *     {@link CoordinatesType }
     *     
     */
    public CoordinatesType getCoordinates() {
        return coordinates;
    }

    /**
     * Define el valor de la propiedad coordinates.
     * 
     * @param value
     *     allowed object is
     *     {@link CoordinatesType }
     *     
     */
    public void setCoordinates(CoordinatesType value) {
        this.coordinates = value;
    }

}
