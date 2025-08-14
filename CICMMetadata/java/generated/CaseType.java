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
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Clase Java para CaseType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="CaseType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="CaseType">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="jewel"/>
 *               &lt;enumeration value="bigjewel"/>
 *               &lt;enumeration value="slimjewel"/>
 *               &lt;enumeration value="sleeve"/>
 *               &lt;enumeration value="qpack"/>
 *               &lt;enumeration value="digisleeve"/>
 *               &lt;enumeration value="discboxslider"/>
 *               &lt;enumeration value="compacplus"/>
 *               &lt;enumeration value="keepcase"/>
 *               &lt;enumeration value="snapcase"/>
 *               &lt;enumeration value="softcase"/>
 *               &lt;enumeration value="ecopack"/>
 *               &lt;enumeration value="liftlock"/>
 *               &lt;enumeration value="spindle"/>
 *               &lt;enumeration value="ps2case"/>
 *               &lt;enumeration value="ps3case"/>
 *               &lt;enumeration value="bluraykeepcase"/>
 *               &lt;enumeration value="pscase"/>
 *               &lt;enumeration value="dccase"/>
 *               &lt;enumeration value="saturncase"/>
 *               &lt;enumeration value="xboxcase"/>
 *               &lt;enumeration value="xbox360case"/>
 *               &lt;enumeration value="xboxonecase"/>
 *               &lt;enumeration value="saturnbigcase"/>
 *               &lt;enumeration value="gccase"/>
 *               &lt;enumeration value="wiicase"/>
 *               &lt;enumeration value="unknown"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Scans" type="{}ScansType"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "CaseType", propOrder = {
    "caseType",
    "scans"
})
public class CaseType {

    @XmlElement(name = "CaseType", required = true)
    protected String caseType;
    @XmlElement(name = "Scans", required = true)
    protected ScansType scans;

    /**
     * Obtiene el valor de la propiedad caseType.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCaseType() {
        return caseType;
    }

    /**
     * Define el valor de la propiedad caseType.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCaseType(String value) {
        this.caseType = value;
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

}
