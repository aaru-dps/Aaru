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
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.XmlValue;


/**
 * <p>Clase Java para BlockSizeType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="BlockSizeType">
 *   &lt;simpleContent>
 *     &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>unsignedInt">
 *       &lt;attribute name="startingBlock" use="required" type="{http://www.w3.org/2001/XMLSchema}unsignedInt" />
 *     &lt;/extension>
 *   &lt;/simpleContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "BlockSizeType", propOrder = {
    "value"
})
public class BlockSizeType {

    @XmlValue
    @XmlSchemaType(name = "unsignedInt")
    protected long value;
    @XmlAttribute(name = "startingBlock", required = true)
    @XmlSchemaType(name = "unsignedInt")
    protected long startingBlock;

    /**
     * Obtiene el valor de la propiedad value.
     * 
     */
    public long getValue() {
        return value;
    }

    /**
     * Define el valor de la propiedad value.
     * 
     */
    public void setValue(long value) {
        this.value = value;
    }

    /**
     * Obtiene el valor de la propiedad startingBlock.
     * 
     */
    public long getStartingBlock() {
        return startingBlock;
    }

    /**
     * Define el valor de la propiedad startingBlock.
     * 
     */
    public void setStartingBlock(long value) {
        this.startingBlock = value;
    }

}
