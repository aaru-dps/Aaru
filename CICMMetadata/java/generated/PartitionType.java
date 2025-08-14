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
 * <p>Clase Java para PartitionType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="PartitionType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Sequence">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}unsignedInt">
 *               &lt;minInclusive value="1"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Name" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="Type" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="StartSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="EndSector" type="{http://www.w3.org/2001/XMLSchema}unsignedLong"/>
 *         &lt;element name="Description" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;element name="FileSystems" type="{}FileSystemsType"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PartitionType", propOrder = {
    "sequence",
    "name",
    "type",
    "startSector",
    "endSector",
    "description",
    "fileSystems"
})
public class PartitionType {

    @XmlElement(name = "Sequence")
    protected long sequence;
    @XmlElement(name = "Name")
    protected String name;
    @XmlElement(name = "Type")
    protected String type;
    @XmlElement(name = "StartSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger startSector;
    @XmlElement(name = "EndSector", required = true)
    @XmlSchemaType(name = "unsignedLong")
    protected BigInteger endSector;
    @XmlElement(name = "Description")
    protected String description;
    @XmlElement(name = "FileSystems", required = true)
    protected FileSystemsType fileSystems;

    /**
     * Obtiene el valor de la propiedad sequence.
     * 
     */
    public long getSequence() {
        return sequence;
    }

    /**
     * Define el valor de la propiedad sequence.
     * 
     */
    public void setSequence(long value) {
        this.sequence = value;
    }

    /**
     * Obtiene el valor de la propiedad name.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getName() {
        return name;
    }

    /**
     * Define el valor de la propiedad name.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setName(String value) {
        this.name = value;
    }

    /**
     * Obtiene el valor de la propiedad type.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getType() {
        return type;
    }

    /**
     * Define el valor de la propiedad type.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setType(String value) {
        this.type = value;
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
     * Obtiene el valor de la propiedad description.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDescription() {
        return description;
    }

    /**
     * Define el valor de la propiedad description.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setDescription(String value) {
        this.description = value;
    }

    /**
     * Obtiene el valor de la propiedad fileSystems.
     * 
     * @return
     *     possible object is
     *     {@link FileSystemsType }
     *     
     */
    public FileSystemsType getFileSystems() {
        return fileSystems;
    }

    /**
     * Define el valor de la propiedad fileSystems.
     * 
     * @param value
     *     allowed object is
     *     {@link FileSystemsType }
     *     
     */
    public void setFileSystems(FileSystemsType value) {
        this.fileSystems = value;
    }

}
