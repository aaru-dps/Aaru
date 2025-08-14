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
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.XmlValue;


/**
 * <p>Clase Java para ChecksumType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="ChecksumType">
 *   &lt;simpleContent>
 *     &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>string">
 *       &lt;attribute name="type" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="fletcher16"/>
 *             &lt;enumeration value="fletcher32"/>
 *             &lt;enumeration value="adler32"/>
 *             &lt;enumeration value="crc16"/>
 *             &lt;enumeration value="crc16ccitt"/>
 *             &lt;enumeration value="crc32"/>
 *             &lt;enumeration value="crc64"/>
 *             &lt;enumeration value="md4"/>
 *             &lt;enumeration value="md5"/>
 *             &lt;enumeration value="dm6"/>
 *             &lt;enumeration value="ripemd128"/>
 *             &lt;enumeration value="ripemd160"/>
 *             &lt;enumeration value="ripemed320"/>
 *             &lt;enumeration value="sha1"/>
 *             &lt;enumeration value="sha224"/>
 *             &lt;enumeration value="sha256"/>
 *             &lt;enumeration value="sha384"/>
 *             &lt;enumeration value="sha512"/>
 *             &lt;enumeration value="sha3"/>
 *             &lt;enumeration value="skein"/>
 *             &lt;enumeration value="snefru"/>
 *             &lt;enumeration value="blake256"/>
 *             &lt;enumeration value="blake512"/>
 *             &lt;enumeration value="tiger"/>
 *             &lt;enumeration value="whirlpool"/>
 *             &lt;enumeration value="spamsum"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *     &lt;/extension>
 *   &lt;/simpleContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ChecksumType", propOrder = {
    "value"
})
public class ChecksumType {

    @XmlValue
    protected String value;
    @XmlAttribute(name = "type", required = true)
    protected String type;

    /**
     * Obtiene el valor de la propiedad value.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getValue() {
        return value;
    }

    /**
     * Define el valor de la propiedad value.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setValue(String value) {
        this.value = value;
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

}
