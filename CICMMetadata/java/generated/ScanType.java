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
 * <p>Clase Java para ScanType complex type.
 * 
 * <p>El siguiente fragmento de esquema especifica el contenido que se espera que haya en esta clase.
 * 
 * <pre>
 * &lt;complexType name="ScanType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="File" type="{}FileType"/>
 *         &lt;element name="Checksums" type="{}ChecksumsType"/>
 *         &lt;element name="Scanner" type="{}ScannerType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="ScanProcessing" type="{}ScanProcessingType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="OCR" type="{}OCRType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ScanType", propOrder = {
    "file",
    "checksums",
    "scanner",
    "scanProcessing",
    "ocr"
})
public class ScanType {

    @XmlElement(name = "File", required = true)
    protected FileType file;
    @XmlElement(name = "Checksums", required = true)
    protected ChecksumsType checksums;
    @XmlElement(name = "Scanner")
    protected List<ScannerType> scanner;
    @XmlElement(name = "ScanProcessing")
    protected List<ScanProcessingType> scanProcessing;
    @XmlElement(name = "OCR")
    protected List<OCRType> ocr;

    /**
     * Obtiene el valor de la propiedad file.
     * 
     * @return
     *     possible object is
     *     {@link FileType }
     *     
     */
    public FileType getFile() {
        return file;
    }

    /**
     * Define el valor de la propiedad file.
     * 
     * @param value
     *     allowed object is
     *     {@link FileType }
     *     
     */
    public void setFile(FileType value) {
        this.file = value;
    }

    /**
     * Obtiene el valor de la propiedad checksums.
     * 
     * @return
     *     possible object is
     *     {@link ChecksumsType }
     *     
     */
    public ChecksumsType getChecksums() {
        return checksums;
    }

    /**
     * Define el valor de la propiedad checksums.
     * 
     * @param value
     *     allowed object is
     *     {@link ChecksumsType }
     *     
     */
    public void setChecksums(ChecksumsType value) {
        this.checksums = value;
    }

    /**
     * Gets the value of the scanner property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the scanner property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getScanner().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ScannerType }
     * 
     * 
     */
    public List<ScannerType> getScanner() {
        if (scanner == null) {
            scanner = new ArrayList<ScannerType>();
        }
        return this.scanner;
    }

    /**
     * Gets the value of the scanProcessing property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the scanProcessing property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getScanProcessing().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ScanProcessingType }
     * 
     * 
     */
    public List<ScanProcessingType> getScanProcessing() {
        if (scanProcessing == null) {
            scanProcessing = new ArrayList<ScanProcessingType>();
        }
        return this.scanProcessing;
    }

    /**
     * Gets the value of the ocr property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the ocr property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getOCR().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link OCRType }
     * 
     * 
     */
    public List<OCRType> getOCR() {
        if (ocr == null) {
            ocr = new ArrayList<OCRType>();
        }
        return this.ocr;
    }

}
