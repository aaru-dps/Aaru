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
 * <p>Clase Java para BarcodeType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="BarcodeType">
 *   &lt;simpleContent>
 *     &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>string">
 *       &lt;attribute name="type" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="aztec"/>
 *             &lt;enumeration value="codabar"/>
 *             &lt;enumeration value="code11"/>
 *             &lt;enumeration value="code128"/>
 *             &lt;enumeration value="code39"/>
 *             &lt;enumeration value="code93"/>
 *             &lt;enumeration value="cpcbinary"/>
 *             &lt;enumeration value="ezcode"/>
 *             &lt;enumeration value="fim"/>
 *             &lt;enumeration value="itf"/>
 *             &lt;enumeration value="itf14"/>
 *             &lt;enumeration value="ean13"/>
 *             &lt;enumeration value="ean8"/>
 *             &lt;enumeration value="maxicode"/>
 *             &lt;enumeration value="isbn"/>
 *             &lt;enumeration value="isrc"/>
 *             &lt;enumeration value="msi"/>
 *             &lt;enumeration value="tof"/>
 *             &lt;enumeration value="shotcode"/>
 *             &lt;enumeration value="rm4scc"/>
 *             &lt;enumeration value="qr"/>
 *             &lt;enumeration value="ean5"/>
 *             &lt;enumeration value="ean2"/>
 *             &lt;enumeration value="qr"/>
 *             &lt;enumeration value="postnet"/>
 *             &lt;enumeration value="postbar"/>
 *             &lt;enumeration value="plessey"/>
 *             &lt;enumeration value="pharmacode"/>
 *             &lt;enumeration value="pdf417"/>
 *             &lt;enumeration value="patchcode"/>
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
@XmlType(name = "BarcodeType", propOrder = {
    "value"
})
public class BarcodeType {

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
