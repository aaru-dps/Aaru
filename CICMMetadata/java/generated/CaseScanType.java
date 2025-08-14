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
 * <p>Clase Java para CaseScanType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="CaseScanType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="CaseScanElement">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;enumeration value="sleeve"/>
 *               &lt;enumeration value="inner"/>
 *               &lt;enumeration value="inlay"/>
 *               &lt;enumeration value="frontback"/>
 *               &lt;enumeration value="frontfull"/>
 *               &lt;enumeration value="boxfront"/>
 *               &lt;enumeration value="boxback"/>
 *               &lt;enumeration value="boxspine"/>
 *               &lt;enumeration value="external"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Scan" type="{}ScanType"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "CaseScanType", propOrder = {
    "caseScanElement",
    "scan"
})
public class CaseScanType {

    @XmlElement(name = "CaseScanElement", required = true)
    protected String caseScanElement;
    @XmlElement(name = "Scan", required = true)
    protected ScanType scan;

    /**
     * Obtiene el valor de la propiedad caseScanElement.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCaseScanElement() {
        return caseScanElement;
    }

    /**
     * Define el valor de la propiedad caseScanElement.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCaseScanElement(String value) {
        this.caseScanElement = value;
    }

    /**
     * Obtiene el valor de la propiedad scan.
     * 
     * @return
     *     possible object is
     *     {@link ScanType }
     *     
     */
    public ScanType getScan() {
        return scan;
    }

    /**
     * Define el valor de la propiedad scan.
     * 
     * @param value
     *     allowed object is
     *     {@link ScanType }
     *     
     */
    public void setScan(ScanType value) {
        this.scan = value;
    }

}
