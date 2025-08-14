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
 * Array of drives information
 * 
 * <p>Clase Java para DumpHardwareArrayType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="DumpHardwareArrayType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="DumpHardware" type="{}DumpHardwareType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "DumpHardwareArrayType", propOrder = {
    "dumpHardware"
})
public class DumpHardwareArrayType {

    @XmlElement(name = "DumpHardware", required = true)
    protected List<DumpHardwareType> dumpHardware;

    /**
     * Gets the value of the dumpHardware property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dumpHardware property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDumpHardware().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DumpHardwareType }
     * 
     * 
     */
    public List<DumpHardwareType> getDumpHardware() {
        if (dumpHardware == null) {
            dumpHardware = new ArrayList<DumpHardwareType>();
        }
        return this.dumpHardware;
    }

}
