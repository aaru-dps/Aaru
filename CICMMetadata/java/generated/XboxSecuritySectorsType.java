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
 * <p>Clase Java para XboxSecuritySectorsType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="XboxSecuritySectorsType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="RequestVersion" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="RequestNumber" type="{http://www.w3.org/2001/XMLSchema}unsignedInt"/>
 *         &lt;element name="SecuritySectors" type="{}DumpType"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "XboxSecuritySectorsType", propOrder = {
    "requestVersion",
    "requestNumber",
    "securitySectors"
})
public class XboxSecuritySectorsType {

    @XmlElement(name = "RequestVersion")
    @XmlSchemaType(name = "unsignedInt")
    protected long requestVersion;
    @XmlElement(name = "RequestNumber")
    @XmlSchemaType(name = "unsignedInt")
    protected long requestNumber;
    @XmlElement(name = "SecuritySectors", required = true)
    protected DumpType securitySectors;

    /**
     * Obtiene el valor de la propiedad requestVersion.
     * 
     */
    public long getRequestVersion() {
        return requestVersion;
    }

    /**
     * Define el valor de la propiedad requestVersion.
     * 
     */
    public void setRequestVersion(long value) {
        this.requestVersion = value;
    }

    /**
     * Obtiene el valor de la propiedad requestNumber.
     * 
     */
    public long getRequestNumber() {
        return requestNumber;
    }

    /**
     * Define el valor de la propiedad requestNumber.
     * 
     */
    public void setRequestNumber(long value) {
        this.requestNumber = value;
    }

    /**
     * Obtiene el valor de la propiedad securitySectors.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getSecuritySectors() {
        return securitySectors;
    }

    /**
     * Define el valor de la propiedad securitySectors.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setSecuritySectors(DumpType value) {
        this.securitySectors = value;
    }

}
