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
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Clase Java para XboxType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="XboxType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="PFI" type="{}DumpType"/>
 *         &lt;element name="DMI" type="{}DumpType"/>
 *         &lt;element name="SecuritySectors" type="{}XboxSecuritySectorsType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "XboxType", propOrder = {
    "pfi",
    "dmi",
    "securitySectors"
})
public class XboxType {

    @XmlElement(name = "PFI", required = true)
    protected DumpType pfi;
    @XmlElement(name = "DMI", required = true)
    protected DumpType dmi;
    @XmlElement(name = "SecuritySectors", required = true)
    protected List<XboxSecuritySectorsType> securitySectors;

    /**
     * Obtiene el valor de la propiedad pfi.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getPFI() {
        return pfi;
    }

    /**
     * Define el valor de la propiedad pfi.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setPFI(DumpType value) {
        this.pfi = value;
    }

    /**
     * Obtiene el valor de la propiedad dmi.
     * 
     * @return
     *     possible object is
     *     {@link DumpType }
     *     
     */
    public DumpType getDMI() {
        return dmi;
    }

    /**
     * Define el valor de la propiedad dmi.
     * 
     * @param value
     *     allowed object is
     *     {@link DumpType }
     *     
     */
    public void setDMI(DumpType value) {
        this.dmi = value;
    }

    /**
     * Gets the value of the securitySectors property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the securitySectors property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSecuritySectors().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link XboxSecuritySectorsType }
     * 
     * 
     */
    public List<XboxSecuritySectorsType> getSecuritySectors() {
        if (securitySectors == null) {
            securitySectors = new ArrayList<XboxSecuritySectorsType>();
        }
        return this.securitySectors;
    }

}
