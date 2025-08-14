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
 * <p>Clase Java para ScansType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="ScansType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="CaseScan" type="{}CaseScanType" minOccurs="0"/>
 *         &lt;element name="Scan" type="{}MediaScanType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ScansType", propOrder = {
    "caseScan",
    "scan"
})
public class ScansType {

    @XmlElement(name = "CaseScan")
    protected CaseScanType caseScan;
    @XmlElement(name = "Scan")
    protected MediaScanType scan;

    /**
     * Obtiene el valor de la propiedad caseScan.
     * 
     * @return
     *     possible object is
     *     {@link CaseScanType }
     *     
     */
    public CaseScanType getCaseScan() {
        return caseScan;
    }

    /**
     * Define el valor de la propiedad caseScan.
     * 
     * @param value
     *     allowed object is
     *     {@link CaseScanType }
     *     
     */
    public void setCaseScan(CaseScanType value) {
        this.caseScan = value;
    }

    /**
     * Obtiene el valor de la propiedad scan.
     * 
     * @return
     *     possible object is
     *     {@link MediaScanType }
     *     
     */
    public MediaScanType getScan() {
        return scan;
    }

    /**
     * Define el valor de la propiedad scan.
     * 
     * @param value
     *     allowed object is
     *     {@link MediaScanType }
     *     
     */
    public void setScan(MediaScanType value) {
        this.scan = value;
    }

}
