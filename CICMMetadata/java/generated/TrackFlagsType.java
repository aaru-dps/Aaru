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
 * <p>Clase Java para TrackFlagsType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="TrackFlagsType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Quadraphonic">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}boolean">
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="Data">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}boolean">
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="CopyPermitted">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}boolean">
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *         &lt;element name="PreEmphasis">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}boolean">
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "TrackFlagsType", propOrder = {
    "quadraphonic",
    "data",
    "copyPermitted",
    "preEmphasis"
})
public class TrackFlagsType {

    @XmlElement(name = "Quadraphonic")
    protected boolean quadraphonic;
    @XmlElement(name = "Data")
    protected boolean data;
    @XmlElement(name = "CopyPermitted")
    protected boolean copyPermitted;
    @XmlElement(name = "PreEmphasis")
    protected boolean preEmphasis;

    /**
     * Obtiene el valor de la propiedad quadraphonic.
     * 
     */
    public boolean isQuadraphonic() {
        return quadraphonic;
    }

    /**
     * Define el valor de la propiedad quadraphonic.
     * 
     */
    public void setQuadraphonic(boolean value) {
        this.quadraphonic = value;
    }

    /**
     * Obtiene el valor de la propiedad data.
     * 
     */
    public boolean isData() {
        return data;
    }

    /**
     * Define el valor de la propiedad data.
     * 
     */
    public void setData(boolean value) {
        this.data = value;
    }

    /**
     * Obtiene el valor de la propiedad copyPermitted.
     * 
     */
    public boolean isCopyPermitted() {
        return copyPermitted;
    }

    /**
     * Define el valor de la propiedad copyPermitted.
     * 
     */
    public void setCopyPermitted(boolean value) {
        this.copyPermitted = value;
    }

    /**
     * Obtiene el valor de la propiedad preEmphasis.
     * 
     */
    public boolean isPreEmphasis() {
        return preEmphasis;
    }

    /**
     * Define el valor de la propiedad preEmphasis.
     * 
     */
    public void setPreEmphasis(boolean value) {
        this.preEmphasis = value;
    }

}
