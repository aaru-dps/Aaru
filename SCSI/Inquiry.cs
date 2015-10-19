/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : SCSI.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes SCSI structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders
{
    /// <summary>
    /// Information from the following standards:
    /// T9/375-D revision 10l
    /// T10/995-D revision 10
    /// T10/1236-D revision 20
    /// T10/1416-D revision 23
    /// T10/1731-D revision 16
    /// T10/502 revision 05
    /// RFC 7144
    /// </summary>
    public static class SCSI
    {
        #region Enumerations

        enum SCSIPeripheralQualifiers : byte
        {
            /// <summary>
            /// Peripheral qualifier: Device is connected and supported
            /// </summary>
            SCSIPQSupported = 0x00,
            /// <summary>
            /// Peripheral qualifier: Device is supported but not connected
            /// </summary>
            SCSIPQUnconnected = 0x01,
            /// <summary>
            /// Peripheral qualifier: Reserved value
            /// </summary>
            SCSIPQReserved = 0x02,
            /// <summary>
            /// Peripheral qualifier: Device is connected but unsupported
            /// </summary>
            SCSIPQUnsupported = 0x03,
            /// <summary>
            /// Peripheral qualifier: Vendor values: 0x04, 0x05, 0x06 and 0x07
            /// </summary>
            SCSIPQVendorMask = 0x04
        }

        public enum SCSIPeripheralDeviceTypes : byte
        {
            /// <summary>
            /// Direct-access device
            /// </summary>
            SCSIPDTDirectAccess = 0x00,
            /// <summary>
            /// Sequential-access device
            /// </summary>
            SCSIPDTSequentialAccess = 0x01,
            /// <summary>
            /// Printer device
            /// </summary>
            SCSIPDTPrinterDevice = 0x02,
            /// <summary>
            /// Processor device
            /// </summary>
            SCSIPDTProcessorDevice = 0x03,
            /// <summary>
            /// Write-once device
            /// </summary>
            SCSIPDTWriteOnceDevice = 0x04,
            /// <summary>
            /// CD-ROM/DVD/etc device
            /// </summary>
            SCSIPDTMultiMediaDevice = 0x05,
            /// <summary>
            /// Scanner device
            /// </summary>
            SCSIPDTScannerDevice = 0x06,
            /// <summary>
            /// Optical memory device
            /// </summary>
            SCSIPDTOpticalDevice = 0x07,
            /// <summary>
            /// Medium change device
            /// </summary>
            SCSIPDTMediumChangerDevice = 0x08,
            /// <summary>
            /// Communications device
            /// </summary>
            SCSIPDTCommsDevice = 0x09,
            /// <summary>
            /// Graphics arts pre-press device (defined in ASC IT8)
            /// </summary>
            SCSIPDTPrePressDevice1 = 0x0A,
            /// <summary>
            /// Graphics arts pre-press device (defined in ASC IT8)
            /// </summary>
            SCSIPDTPrePressDevice2 = 0x0B,
            /// <summary>
            /// Array controller device
            /// </summary>
            SCSIPDTArrayControllerDevice = 0x0C,
            /// <summary>
            /// Enclosure services device
            /// </summary>
            SCSIPDTEnclosureServiceDevice = 0x0D,
            /// <summary>
            /// Simplified direct-access device
            /// </summary>
            SCSIPDTSimplifiedDevice = 0x0E,
            /// <summary>
            /// Optical card reader/writer device
            /// </summary>
            SCSIPDTOCRWDevice = 0x0F,
            /// <summary>
            /// Bridging Expanders
            /// </summary>
            SCSIPDTBridgingExpander = 0x10,
            /// <summary>
            /// Object-based Storage Device
            /// </summary>
            SCSIPDTObjectDevice = 0x11,
            /// <summary>
            /// Automation/Drive Interface
            /// </summary>
            SCSIPDTADCDevice = 0x12,
            /// <summary>
            /// Security Manager Device
            /// </summary>
            SCSISecurityManagerDevice = 0x13,
            /// <summary>
            /// Host managed zoned block device
            /// </summary>
            SCSIZonedBlockDEvice = 0x14,
            /// <summary>
            /// Well known logical unit
            /// </summary>
            SCSIPDTWellKnownDevice = 0x1E,
            /// <summary>
            /// Unknown or no device type
            /// </summary>
            SCSIPDTUnknownDevice = 0x1F
        }

        enum SCSIANSIVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ANSI version
            /// </summary>
            SCSIANSINoVersion = 0x00,
            /// <summary>
            /// Device complies with ANSI X3.131:1986
            /// </summary>
            SCSIANSI1986Version = 0x01,
            /// <summary>
            /// Device complies with ANSI X3.131:1994
            /// </summary>
            SCSIANSI1994Version = 0x02,
            /// <summary>
            /// Device complies with ANSI X3.301:1997
            /// </summary>
            SCSIANSI1997Version = 0x03,
            /// <summary>
            /// Device complies with ANSI X3.351:2001
            /// </summary>
            SCSIANSI2001Version = 0x04,
            /// <summary>
            /// Device complies with ANSI X3.408:2005.
            /// </summary>
            SCSIANSI2005Version = 0x05,
            /// <summary>
            /// Device complies with SPC-4
            /// </summary>
            SCSIANSI2008Version = 0x06
        }

        enum SCSIECMAVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ECMA version
            /// </summary>
            SCSIECMANoVersion = 0x00,
            /// <summary>
            /// Device complies with an obsolete ECMA standard
            /// </summary>
            SCSIECMAObsolete = 0x01
        }

        enum SCSIISOVersions : byte
        {
            /// <summary>
            /// Device does not claim conformance to any ISO/IEC version
            /// </summary>
            SCSIISONoVersion = 0x00,
            /// <summary>
            /// Device complies with ISO/IEC 9316:1995
            /// </summary>
            SCSIISO1995Version = 0x02
        }

        enum SCSISPIClocking : byte
        {
            /// <summary>
            /// Supports only ST
            /// </summary>
            SCSIClockingST = 0x00,
            /// <summary>
            /// Supports only DT
            /// </summary>
            SCSIClockingDT = 0x01,
            /// <summary>
            /// Reserved value
            /// </summary>
            SCSIClockingReserved = 0x02,
            /// <summary>
            /// Supports ST and DT
            /// </summary>
            SCSIClockingSTandDT = 0x03,
        }

        enum SCSITGPSValues : byte
        {
            /// <summary>
            /// Assymetrical access not supported
            /// </summary>
            NotSupported = 0x00,
            /// <summary>
            /// Only implicit assymetrical access is supported
            /// </summary>
            OnlyImplicit = 0x01,
            /// <summary>
            /// Only explicit assymetrical access is supported
            /// </summary>
            OnlyExplicit = 0x02,
            /// <summary>
            /// Both implicit and explicit assymetrical access are supported
            /// </summary>
            Both = 0x03
        }

        #endregion Enumerations

        #region Private methods

        static string PrettifySCSIVendorString(string SCSIVendorString)
        {
            switch (SCSIVendorString)
            {
                case "0B4C":
                    return "MOOSIK Ltd.";
                case "13FE":
                    return "PHISON";
                case "2AI":
                    return "2AI (Automatisme et Avenir Informatique)";
                case "3M":
                    return "3M Company";
                case "3nhtech":
                    return "3NH Technologies";
                case "3PARdata":
                    return "3PARdata, Inc.";
                case "A-Max":
                    return "A-Max Technology Co., Ltd";
                case "ABSOLUTE":
                    return "Absolute Analysis";
                case "ACARD":
                    return "ACARD Technology Corp.";
                case "Accusys":
                    return "Accusys INC.";
                case "Acer":
                    return "Acer, Inc.";
                case "ACL":
                    return "Automated Cartridge Librarys, Inc.";
                case "Actifio":
                    return "Actifio";
                case "Acuid":
                    return "Acuid Corporation Ltd.";
                case "AcuLab":
                    return "AcuLab, Inc. (Tulsa, OK)";
                case "ADAPTEC":
                    return "Adaptec";
                case "ADIC":
                    return "Advanced Digital Information Corporation";
                case "ADSI":
                    return "Adaptive Data Systems, Inc. (a Western Digital subsidiary)";
                case "ADTX":
                    return "ADTX Co., Ltd.";
                case "ADVA":
                    return "ADVA Optical Networking AG";
                case "AEM":
                    return "AEM Performance Electronics";
                case "AERONICS":
                    return "Aeronics, Inc.";
                case "AGFA":
                    return "AGFA";
                case "Agilent":
                    return "Agilent Technologies";
                case "AIC":
                    return "Advanced Industrial Computer, Inc.";
                case "AIPTEK":
                    return "AIPTEK International Inc.";
                case "Alcohol":
                    return "Alcohol Soft";
                case "ALCOR":
                    return "Alcor Micro, Corp.";
                case "AMCC":
                    return "Applied Micro Circuits Corporation";
                case "AMCODYNE":
                    return "Amcodyne";
                case "Amgeon":
                    return "Amgeon LLC";
                case "AMI":
                    return "American Megatrends, Inc.";
                case "AMPEX":
                    return "Ampex Data Systems";
                case "Amphenol":
                    return "Amphenol";
                case "Amtl":
                    return "Tenlon Technology Co.,Ltd";
                case "ANAMATIC":
                    return "Anamartic Limited (England)";
                case "Ancor":
                    return "Ancor Communications, Inc.";
                case "ANCOT":
                    return "ANCOT Corp.";
                case "ANDATACO":
                    return "Andataco";
                case "andiamo":
                    return "Andiamo Systems, Inc.";
                case "ANOBIT":
                    return "Anobit";
                case "ANRITSU":
                    return "Anritsu Corporation";
                case "ANTONIO":
                    return "Antonio Precise Products Manufactory Ltd.";
                case "AoT":
                    return "Art of Technology AG";
                case "APPLE":
                    return "Apple Computer, Inc.";
                case "ARCHIVE":
                    return "Archive";
                case "ARDENCE":
                    return "Ardence Inc";
                case "Areca":
                    return "Areca Technology Corporation";
                case "Arena":
                    return "MaxTronic International Co., Ltd.";
                case "Argent":
                    return "Argent Data Systems, Inc.";
                case "ARIO":
                    return "Ario Data Networks, Inc.";
                case "ARISTOS":
                    return "Aristos Logic Corp.";
                case "ARK":
                    return "ARK Research Corporation";
                case "ARL:UT@A":
                    return "Applied Research Laboratories : University of Texas at Austin";
                case "ARTECON":
                    return "Artecon Inc.";
                case "Artistic":
                    return "Artistic Licence (UK) Ltd";
                case "ARTON":
                    return "Arton Int.";
                case "ASACA":
                    return "ASACA Corp.";
                case "ASC":
                    return "Advanced Storage Concepts, Inc.";
                case "ASPEN":
                    return "Aspen Peripherals";
                case "AST":
                    return "AST Research";
                case "ASTEK":
                    return "Astek Corporation";
                case "ASTK":
                    return "Alcatel STK A/S";
                case "AStor":
                    return "AccelStor, Inc.";
                case "ASTUTE":
                    return "Astute Networks, Inc.";
                case "AT&T":
                    return "AT&T";
                case "ATA":
                    return "SCSI / ATA Translator Software (Organization Not Specified)";
                case "ATARI":
                    return "Atari Corporation";
                case "ATech":
                    return "ATech electronics";
                case "ATG CYG":
                    return "ATG Cygnet Inc.";
                case "ATL":
                    return "Quantum|ATL Products";
                case "ATTO":
                    return "ATTO Technology Inc.";
                case "ATTRATEC":
                    return "Attratech Ltd liab. Co";
                case "ATX":
                    return "Alphatronix";
                case "AURASEN":
                    return "Aurasen Limited";
                case "Avago":
                    return "Avago Technologies";
                case "AVC":
                    return "AVC Technology Ltd";
                case "AVIDVIDR":
                    return "AVID Technologies, Inc.";
                case "AVR":
                    return "Advanced Vision Research";
                case "AXSTOR":
                    return "AXSTOR";
                case "Axxana":
                    return "Axxana Ltd.";
                case "B*BRIDGE":
                    return "Blockbridge Networks LLC";
                case "BALLARD":
                    return "Ballard Synergy Corp.";
                case "Barco":
                    return "Barco";
                case "BAROMTEC":
                    return "Barom Technologies Co., Ltd.";
                case "Bassett":
                    return "Bassett Electronic Systems Ltd";
                case "BC Hydro":
                    return "BC Hydro";
                case "BDT":
                    return "BDT AG";
                case "BECEEM":
                    return "Beceem Communications, Inc";
                case "BENQ":
                    return "BENQ Corporation.";
                case "BERGSWD":
                    return "Berg Software Design";
                case "BEZIER":
                    return "Bezier Systems, Inc.";
                case "BHTi":
                    return "Breece Hill Technologies";
                case "biodata":
                    return "Biodata Devices SL";
                case "BIOS":
                    return "BIOS Corporation";
                case "BIR":
                    return "Bio-Imaging Research, Inc.";
                case "BiT":
                    return "BiT Microsystems";
                case "BITMICRO":
                    return "BiT Microsystems, Inc.";
                case "Blendlgy":
                    return "Blendology Limited";
                case "BLOOMBAS":
                    return "Bloombase Technologies Limited";
                case "BlueArc":
                    return "BlueArc Corporation";
                case "bluecog":
                    return "bluecog";
                case "BME-HVT":
                    return "Broadband Infocommunicatons and Electromagnetic Theory Department";
                case "BNCHMARK":
                    return "Benchmark Tape Systems Corporation";
                case "Bosch":
                    return "Robert Bosch GmbH";
                case "Botman":
                    return "Botmanfamily Electronics";
                case "BoxHill":
                    return "Box Hill Systems Corporation";
                case "BRDGWRKS":
                    return "Bridgeworks Ltd.";
                case "BREA":
                    return "BREA Technologies, Inc.";
                case "BREECE":
                    return "Breece Hill LLC";
                case "BreqLabs":
                    return "BreqLabs Inc.";
                case "Broadcom":
                    return "Broadcom Corporation";
                case "BROCADE":
                    return "Brocade Communications Systems, Incorporated";
                case "BUFFALO":
                    return "BUFFALO INC.";
                case "BULL":
                    return "Bull Peripherals Corp.";
                case "BUSLOGIC":
                    return "BusLogic Inc.";
                case "BVIRTUAL":
                    return "B-Virtual N.V.";
                case "CACHEIO":
                    return "CacheIO LLC";
                case "CalComp":
                    return "CalComp, A Lockheed Company";
                case "CALCULEX":
                    return "CALCULEX, Inc.";
                case "CALIPER":
                    return "Caliper (California Peripheral Corp.)";
                case "CAMBEX":
                    return "Cambex Corporation";
                case "CAMEOSYS":
                    return "Cameo Systems Inc.";
                case "CANDERA":
                    return "Candera Inc.";
                case "CAPTION":
                    return "CAPTION BANK";
                case "CAST":
                    return "Advanced Storage Tech";
                case "CATALYST":
                    return "Catalyst Enterprises";
                case "CCDISK":
                    return "iSCSI Cake";
                case "CDC":
                    return "Control Data or MPI";
                case "CDP":
                    return "Columbia Data Products";
                case "Celsia":
                    return "A M Bromley Limited";
                case "CenData":
                    return "Central Data Corporation";
                case "Cereva":
                    return "Cereva Networks Inc.";
                case "CERTANCE":
                    return "Certance";
                case "Chantil":
                    return "Chantil Technology";
                case "CHEROKEE":
                    return "Cherokee Data Systems";
                case "CHINON":
                    return "Chinon";
                case "CHRISTMA":
                    return "Christmann Informationstechnik + Medien GmbH & Co KG";
                case "CIE&YED":
                    return "YE Data, C.Itoh Electric Corp.";
                case "CIPHER":
                    return "Cipher Data Products";
                case "Ciprico":
                    return "Ciprico, Inc.";
                case "CIRRUSL":
                    return "Cirrus Logic Inc.";
                case "CISCO":
                    return "Cisco Systems, Inc.";
                case "CLEARSKY":
                    return "ClearSky Data, Inc.";
                case "CLOVERLF":
                    return "Cloverleaf Communications, Inc";
                case "CLS":
                    return "Celestica";
                case "CMD":
                    return "CMD Technology Inc.";
                case "CMTechno":
                    return "CMTech";
                case "CNGR SFW":
                    return "Congruent Software, Inc.";
                case "CNSi":
                    return "Chaparral Network Storage, Inc.";
                case "CNT":
                    return "Computer Network Technology";
                case "COBY":
                    return "Coby Electronics Corporation, USA";
                case "COGITO":
                    return "Cogito";
                case "COMAY":
                    return "Corerise Electronics";
                case "COMPAQ":
                    return "Compaq Computer Corporation";
                case "COMPELNT":
                    return "Compellent Technologies, Inc.";
                case "COMPORT":
                    return "Comport Corp.";
                case "COMPSIG":
                    return "Computer Signal Corporation";
                case "COMPTEX":
                    return "Comptex Pty Limited";
                case "CONNER":
                    return "Conner Peripherals";
                case "COPANSYS":
                    return "COPAN SYSTEMS INC";
                case "CORAID":
                    return "Coraid, Inc";
                case "CORE":
                    return "Core International, Inc.";
                case "CORERISE":
                    return "Corerise Electronics";
                case "COVOTE":
                    return "Covote GmbH & Co KG";
                case "COWON":
                    return "COWON SYSTEMS, Inc.";
                case "CPL":
                    return "Cross Products Ltd";
                case "CPU TECH":
                    return "CPU Technology, Inc.";
                case "CREO":
                    return "Creo Products Inc.";
                case "CROSFLD":
                    return "Crosfield Electronics";
                case "CROSSRDS":
                    return "Crossroads Systems, Inc.";
                case "crosswlk":
                    return "Crosswalk, Inc.";
                case "CSCOVRTS":
                    return "Cisco - Veritas";
                case "CSM, INC":
                    return "Computer SM, Inc.";
                case "Cunuqui":
                    return "CUNUQUI SLU";
                case "CYBERNET":
                    return "Cybernetics";
                case "Cygnal":
                    return "Dekimo";
                case "CYPRESS":
                    return "Cypress Semiconductor Corp.";
                case "D Bit":
                    return "Digby's Bitpile, Inc. DBA D Bit";
                case "DALSEMI":
                    return "Dallas Semiconductor";
                case "DANEELEC":
                    return "Dane-Elec";
                case "DANGER":
                    return "Danger Inc.";
                case "DAT-MG":
                    return "DAT Manufacturers Group";
                case "Data Com":
                    return "Data Com Information Systems Pty. Ltd.";
                case "DATABOOK":
                    return "Databook, Inc.";
                case "DATACOPY":
                    return "Datacopy Corp.";
                case "DataCore":
                    return "DataCore Software Corporation";
                case "DataG":
                    return "DataGravity";
                case "DATAPT":
                    return "Datapoint Corp.";
                case "DATARAM":
                    return "Dataram Corporation";
                case "DATC":
                    return "Datum Champion Technology Co., Ltd";
                case "DAVIS":
                    return "Daviscomms (S) Pte Ltd";
                case "DCS":
                    return "ShenZhen DCS Group Co.,Ltd";
                case "DDN":
                    return "DataDirect Networks, Inc.";
                case "DDRDRIVE":
                    return "DDRdrive LLC";
                case "DE":
                    return "Dimension Engineering LLC";
                case "DEC":
                    return "Digital Equipment Corporation";
                case "DEI":
                    return "Digital Engineering, Inc.";
                case "DELL":
                    return "Dell, Inc.";
                case "Dell(tm)":
                    return "Dell, Inc";
                case "DELPHI":
                    return "Delphi Data Div. of Sparks Industries, Inc.";
                case "DENON":
                    return "Denon/Nippon Columbia";
                case "DenOptix":
                    return "DenOptix, Inc.";
                case "DEST":
                    return "DEST Corp.";
                case "DFC":
                    return "DavioFranke.com";
                case "DFT":
                    return "Data Fault Tolerance System CO.,LTD.";
                case "DGC":
                    return "Data General Corp.";
                case "DIGIDATA":
                    return "Digi-Data Corporation";
                case "DigiIntl":
                    return "Digi International";
                case "Digital":
                    return "Digital Equipment Corporation";
                case "DILOG":
                    return "Distributed Logic Corp.";
                case "DISC":
                    return "Document Imaging Systems Corp.";
                case "DiscSoft":
                    return "Disc Soft Ltd";
                case "DLNET":
                    return "Driveline";
                case "DNS":
                    return "Data and Network Security";
                case "DNUK":
                    return "Digital Networks Uk Ltd";
                case "DotHill":
                    return "Dot Hill Systems Corp.";
                case "DP":
                    return "Dell, Inc.";
                case "DPT":
                    return "Distributed Processing Technology";
                case "Drewtech":
                    return "Drew Technologies, Inc.";
                case "DROBO":
                    return "Data Robotics, Inc.";
                case "DSC":
                    return "DigitalStream Corporation";
                case "DSI":
                    return "Data Spectrum, Inc.";
                case "DSM":
                    return "Deterner Steuerungs- und Maschinenbau GmbH & Co.";
                case "DSNET":
                    return "Cleversafe, Inc.";
                case "DT":
                    return "Double-Take Software, INC.";
                case "DTC QUME":
                    return "Data Technology Qume";
                case "DXIMAGIN":
                    return "DX Imaging";
                case "E-Motion":
                    return "E-Motion LLC";
                case "EARTHLAB":
                    return "EarthLabs";
                case "EarthLCD":
                    return "Earth Computer Technologies, Inc.";
                case "ECCS":
                    return "ECCS, Inc.";
                case "ECMA":
                    return "European Computer Manufacturers Association";
                case "EDS":
                    return "Embedded Data Systems";
                case "EIM":
                    return "InfoCore";
                case "ELE Intl":
                    return "ELE International";
                case "ELEGANT":
                    return "Elegant Invention, LLC";
                case "Elektron":
                    return "Elektron Music Machines MAV AB";
                case "elipsan":
                    return "Elipsan UK Ltd.";
                case "Elms":
                    return "Elms Systems Corporation";
                case "ELSE":
                    return "ELSE Ltd.";
                case "ELSEC":
                    return "Littlemore Scientific";
                case "EMASS":
                    return "EMASS, Inc.";
                case "EMC":
                    return "EMC Corp.";
                case "EMiT":
                    return "EMiT Conception Eletronique";
                case "EMTEC":
                    return "EMTEC Magnetics";
                case "EMULEX":
                    return "Emulex";
                case "ENERGY-B":
                    return "Energybeam Corporation";
                case "ENGENIO":
                    return "Engenio Information Technologies, Inc.";
                case "ENMOTUS":
                    return "Enmotus Inc";
                case "Entacore":
                    return "Entacore";
                case "EPOS":
                    return "EPOS Technologies Ltd.";
                case "EPSON":
                    return "Epson";
                case "EQLOGIC":
                    return "EqualLogic";
                case "Eris/RSI":
                    return "RSI Systems, Inc.";
                case "ETERNE":
                    return "EterneData Technology Co.,Ltd.(China PRC.)";
                case "EuroLogc":
                    return "Eurologic Systems Limited";
                case "evolve":
                    return "Evolution Technologies, Inc";
                case "EXABYTE":
                    return "Exabyte Corp.";
                case "EXATEL":
                    return "Exatelecom Co., Ltd.";
                case "EXAVIO":
                    return "Exavio, Inc.";
                case "Exsequi":
                    return "Exsequi Ltd";
                case "Exxotest":
                    return "Annecy Electronique";
                case "FAIRHAVN":
                    return "Fairhaven Health, LLC";
                case "FALCON":
                    return "FalconStor, Inc.";
                case "FDS":
                    return "Formation Data Systems";
                case "FFEILTD":
                    return "FujiFilm Electonic Imaging Ltd";
                case "Fibxn":
                    return "Fiberxon, Inc.";
                case "FID":
                    return "First International Digital, Inc.";
                case "FILENET":
                    return "FileNet Corp.";
                case "FirmFact":
                    return "Firmware Factory Ltd";
                case "FLYFISH":
                    return "Flyfish Technologies";
                case "FOXCONN":
                    return "Foxconn Technology Group";
                case "FRAMDRV":
                    return "FRAMEDRIVE Corp.";
                case "FREECION":
                    return "Nable Communications, Inc.";
                case "FRESHDTK":
                    return "FreshDetect GmbH";
                case "FSC":
                    return "Fujitsu Siemens Computers";
                case "FTPL":
                    return "Frontline Technologies Pte Ltd";
                case "FUJI":
                    return "Fuji Electric Co., Ltd. (Japan)";
                case "FUJIFILM":
                    return "Fuji Photo Film, Co., Ltd.";
                case "FUJITSU":
                    return "Fujitsu";
                case "FUNAI":
                    return "Funai Electric Co., Ltd.";
                case "FUSIONIO":
                    return "Fusion-io Inc.";
                case "FUTURED":
                    return "Future Domain Corp.";
                case "G&D":
                    return "Giesecke & Devrient GmbH";
                case "G.TRONIC":
                    return "Globaltronic - Electronica e Telecomunicacoes, S.A.";
                case "Gadzoox":
                    return "Gadzoox Networks, Inc.";
                case "Gammaflx":
                    return "Gammaflux L.P.";
                case "GDI":
                    return "Generic Distribution International";
                case "GEMALTO":
                    return "gemalto";
                case "Gen_Dyn":
                    return "General Dynamics";
                case "Generic":
                    return "Generic Technology Co., Ltd.";
                case "GENSIG":
                    return "General Signal Networks";
                case "GEO":
                    return "Green Energy Options Ltd";
                case "GIGATAPE":
                    return "GIGATAPE GmbH";
                case "GIGATRND":
                    return "GigaTrend Incorporated";
                case "Global":
                    return "Global Memory Test Consortium";
                case "Gnutek":
                    return "Gnutek Ltd.";
                case "Goidelic":
                    return "Goidelic Precision, Inc.";
                case "GoldKey":
                    return "GoldKey Security Corporation";
                case "GoldStar":
                    return "LG Electronics Inc.";
                case "GOOGLE":
                    return "Google, Inc.";
                case "GORDIUS":
                    return "Gordius";
                case "GOULD":
                    return "Gould";
                case "HAGIWARA":
                    return "Hagiwara Sys-Com Co., Ltd.";
                case "HAPP3":
                    return "Inventec Multimedia and Telecom co., ltd";
                case "HDS":
                    return "Horizon Data Systems, Inc.";
                case "Helldyne":
                    return "Helldyne, Inc";
                case "Heydays":
                    return "Mazo Technology Co., Ltd.";
                case "HGST":
                    return "HGST a Western Digital Company";
                case "HI-TECH":
                    return "HI-TECH Software Pty. Ltd.";
                case "HITACHI":
                    return "Hitachi America Ltd or Nissei Sangyo America Ltd";
                case "HL-DT-ST":
                    return "Hitachi-LG Data Storage, Inc.";
                case "HONEYWEL":
                    return "Honeywell Inc.";
                case "Hoptroff":
                    return "HexWax Ltd";
                case "HORIZONT":
                    return "Horizontigo Software";
                case "HP":
                    return "Hewlett Packard";
                case "HPE":
                    return "Hewlett Packard Enterprise";
                case "HPI":
                    return "HP Inc.";
                case "HPQ":
                    return "Hewlett Packard";
                case "HUALU":
                    return "CHINA HUALU GROUP CO., LTD";
                case "HUASY":
                    return "Huawei Symantec Technologies Co., Ltd.";
                case "HYLINX":
                    return "Hylinx Ltd.";
                case "HYUNWON":
                    return "HYUNWON inc";
                case "i-cubed":
                    return "i-cubed ltd.";
                case "IBM":
                    return "International Business Machines";
                case "Icefield":
                    return "Icefield Tools Corporation";
                case "Iceweb":
                    return "Iceweb Storage Corp";
                case "ICL":
                    return "ICL";
                case "ICP":
                    return "ICP vortex Computersysteme GmbH";
                case "IDE":
                    return "International Data Engineering, Inc.";
                case "IDG":
                    return "Interface Design Group";
                case "IET":
                    return "ISCSI ENTERPRISE TARGET";
                case "IFT":
                    return "Infortrend Technology, Inc.";
                case "IGR":
                    return "Intergraph Corp.";
                case "IMAGINE":
                    return "Imagine Communications Corp.";
                case "IMAGO":
                    return "IMAGO SOFTWARE SL";
                case "IMATION":
                    return "Imation";
                case "IMPLTD":
                    return "Integrated Micro Products Ltd.";
                case "IMPRIMIS":
                    return "Imprimis Technology Inc.";
                case "INCIPNT":
                    return "Incipient Technologies Inc.";
                case "INCITS":
                    return "InterNational Committee for Information Technology";
                case "INDCOMP":
                    return "Industrial Computing Limited";
                case "Indigita":
                    return "Indigita Corporation";
                case "INFOCORE":
                    return "InfoCore";
                case "INITIO":
                    return "Initio Corporation";
                case "INRANGE":
                    return "INRANGE Technologies Corporation";
                case "Insight":
                    return "L-3 Insight Technology Inc";
                case "INSITE":
                    return "Insite Peripherals";
                case "integrix":
                    return "Integrix, Inc.";
                case "INTEL":
                    return "Intel Corporation";
                case "Intransa":
                    return "Intransa, Inc.";
                case "IOC":
                    return "I/O Concepts, Inc.";
                case "iofy":
                    return "iofy Corporation";
                case "IOMEGA":
                    return "Iomega";
                case "IOT":
                    return "IO Turbine, Inc.";
                case "iPaper":
                    return "intelliPaper, LLC";
                case "iqstor":
                    return "iQstor Networks, Inc.";
                case "iQue":
                    return "iQue";
                case "ISi":
                    return "Information Storage inc.";
                case "Isilon":
                    return "Isilon Systems, Inc.";
                case "ISO":
                    return "International Standards Organization";
                case "iStor":
                    return "iStor Networks, Inc.";
                case "ITC":
                    return "International Tapetronics Corporation";
                case "iTwin":
                    return "iTwin Pte Ltd";
                case "IVIVITY":
                    return "iVivity, Inc.";
                case "IVMMLTD":
                    return "InnoVISION Multimedia Ltd.";
                case "JABIL001":
                    return "Jabil Circuit";
                case "JETWAY":
                    return "Jetway Information Co., Ltd";
                case "JMR":
                    return "JMR Electronics Inc.";
                case "JOFEMAR":
                    return "Jofemar";
                case "JOLLYLOG":
                    return "Jolly Logic";
                case "JPC Inc.":
                    return "JPC Inc.";
                case "JSCSI":
                    return "jSCSI Project";
                case "Juniper":
                    return "Juniper Networks";
                case "JVC":
                    return "JVC Information Products Co.";
                case "KASHYA":
                    return "Kashya, Inc.";
                case "KENNEDY":
                    return "Kennedy Company";
                case "KENWOOD":
                    return "KENWOOD Corporation";
                case "KEWL":
                    return "Shanghai KEWL Imp&Exp Co., Ltd.";
                case "Key Tech":
                    return "Key Technologies, Inc";
                case "KMNRIO":
                    return "Kaminario Technologies Ltd.";
                case "KODAK":
                    return "Eastman Kodak";
                case "KONAN":
                    return "Konan";
                case "koncepts":
                    return "koncepts International Ltd.";
                case "KONICA":
                    return "Konica Japan";
                case "KOVE":
                    return "KOVE";
                case "KSCOM":
                    return "KSCOM Co. Ltd.,";
                case "KUDELSKI":
                    return "Nagravision SA - Kudelski Group";
                case "Kyocera":
                    return "Kyocera Corporation";
                case "Lapida":
                    return "Gonmalo Electronics";
                case "LAPINE":
                    return "Lapine Technology";
                case "LASERDRV":
                    return "LaserDrive Limited";
                case "LASERGR":
                    return "Lasergraphics, Inc.";
                case "LeapFrog":
                    return "LeapFrog Enterprises, Inc.";
                case "LEFTHAND":
                    return "LeftHand Networks";
                case "Leica":
                    return "Leica Camera AG";
                case "Lexar":
                    return "Lexar Media, Inc.";
                case "LEYIO":
                    return "LEYIO";
                case "LG":
                    return "LG Electronics Inc.";
                case "LGE":
                    return "LG Electronics Inc.";
                case "LIBNOVA":
                    return "LIBNOVA, SL Digital Preservation Systems";
                case "LION":
                    return "Lion Optics Corporation";
                case "LMS":
                    return "Laser Magnetic Storage International Company";
                case "LoupTech":
                    return "Loup Technologies, Inc.";
                case "LSI":
                    return "LSI Corp. (was LSI Logic Corp.)";
                case "LSILOGIC":
                    return "LSI Logic Storage Systems, Inc.";
                case "LTO-CVE":
                    return "Linear Tape - Open, Compliance Verification Entity";
                case "LUXPRO":
                    return "Luxpro Corporation";
                case "MacroSAN":
                    return "MacroSAN Technologies Co., Ltd.";
                case "Malakite":
                    return "Malachite Technologies (New VID is: Sandial)";
                case "MarcBoon":
                    return "marcboon.com";
                case "Marner":
                    return "Marner Storage Technologies, Inc.";
                case "MARVELL":
                    return "Marvell Semiconductor, Inc.";
                case "Matrix":
                    return "Matrix Orbital Corp.";
                case "MATSHITA":
                    return "Matsushita";
                case "MAXELL":
                    return "Hitachi Maxell, Ltd.";
                case "MAXIM-IC":
                    return "Maxim Integrated Products";
                case "MaxOptix":
                    return "Maxoptix Corp.";
                case "MAXSTRAT":
                    return "Maximum Strategy, Inc.";
                case "MAXTOR":
                    return "Maxtor Corp.";
                case "MaXXan":
                    return "MaXXan Systems, Inc.";
                case "MAYCOM":
                    return "maycom Co., Ltd.";
                case "MBEAT":
                    return "K-WON C&C Co.,Ltd";
                case "MCC":
                    return "Measurement Computing Corporation";
                case "McDATA":
                    return "McDATA Corporation";
                case "MCUBE":
                    return "Mcube Technology Co., Ltd.";
                case "MDI":
                    return "Micro Design International, Inc.";
                case "MEADE":
                    return "Meade Instruments Corporation";
                case "mediamat":
                    return "mediamatic";
                case "MegaElec":
                    return "Mega Electronics Ltd";
                case "MEII":
                    return "Mountain Engineering II, Inc.";
                case "MELA":
                    return "Mitsubishi Electronics America";
                case "MELCO":
                    return "Mitsubishi Electric (Japan)";
                case "mellanox":
                    return "Mellanox Technologies Ltd.";
                case "MEMOREX":
                    return "Memorex Telex Japan Ltd.";
                case "MEMREL":
                    return "Memrel Corporation";
                case "MEMTECH":
                    return "MemTech Technology";
                case "Mendocin":
                    return "Mendocino Software";
                case "MendoCno":
                    return "Mendocino Software";
                case "MERIDATA":
                    return "Oy Meridata Finland Ltd";
                case "METHODEI":
                    return "Methode Electronics India pvt ltd";
                case "METRUM":
                    return "Metrum, Inc.";
                case "MHTL":
                    return "Matsunichi Hi-Tech Limited";
                case "MICROBTX":
                    return "Microbotics Inc.";
                case "Microchp":
                    return "Microchip Technology, Inc.";
                case "MICROLIT":
                    return "Microlite Corporation";
                case "MICRON":
                    return "Micron Technology, Inc.";
                case "MICROP":
                    return "Micropolis";
                case "MICROTEK":
                    return "Microtek Storage Corp";
                case "Minitech":
                    return "Minitech (UK) Limited";
                case "Minolta":
                    return "Minolta Corporation";
                case "MINSCRIB":
                    return "Miniscribe";
                case "MiraLink":
                    return "MiraLink Corporation";
                case "Mirifica":
                    return "Mirifica s.r.l.";
                case "MITSUMI":
                    return "Mitsumi Electric Co., Ltd.";
                case "MKM":
                    return "Mitsubishi Kagaku Media Co., LTD.";
                case "Mobii":
                    return "Mobii Systems (Pty.) Ltd.";
                case "MOL":
                    return "Petrosoft Sdn. Bhd.";
                case "MOSAID":
                    return "Mosaid Technologies Inc.";
                case "MOTOROLA":
                    return "Motorola";
                case "MP-400":
                    return "Daiwa Manufacturing Limited";
                case "MPC":
                    return "MPC Corporation";
                case "MPCCORP":
                    return "MPC Computers";
                case "MPEYE":
                    return "Touchstone Technology Co., Ltd";
                case "MPIO":
                    return "DKT Co.,Ltd";
                case "MPM":
                    return "Mitsubishi Paper Mills, Ltd.";
                case "MPMan":
                    return "MPMan.com, Inc.";
                case "MSFT":
                    return "Microsoft Corporation";
                case "MSI":
                    return "Micro-Star International Corp.";
                case "MST":
                    return "Morning Star Technologies, Inc.";
                case "MSystems":
                    return "M-Systems Flash Disk Pioneers";
                case "MTI":
                    return "MTI Technology Corporation";
                case "MTNGATE":
                    return "MountainGate Data Systems";
                case "MXI":
                    return "Memory Experts International";
                case "nac":
                    return "nac Image Technology Inc.";
                case "NAGRA":
                    return "Nagravision SA - Kudelski Group";
                case "NAI":
                    return "North Atlantic Industries";
                case "NAKAMICH":
                    return "Nakamichi Corporation";
                case "NatInst":
                    return "National Instruments";
                case "NatSemi":
                    return "National Semiconductor Corp.";
                case "NCITS":
                    return "InterNational Committee for Information Technology Standards (INCITS)";
                case "NCL":
                    return "NCL America";
                case "NCR":
                    return "NCR Corporation";
                case "NDBTECH":
                    return "NDB Technologie Inc.";
                case "Neartek":
                    return "Neartek, Inc.";
                case "NEC":
                    return "NEC";
                case "NETAPP":
                    return "NetApp, Inc. (was Network Appliance)";
                case "NetBSD":
                    return "The NetBSD Foundation";
                case "Netcom":
                    return "Netcom Storage";
                case "NETENGIN":
                    return "NetEngine, Inc.";
                case "NEWISYS":
                    return "Newisys Data Storage";
                case "Newtech":
                    return "Newtech Co., Ltd.";
                case "NEXSAN":
                    return "Nexsan Technologies, Ltd.";
                case "NFINIDAT":
                    return "Infinidat Ltd.";
                case "NHR":
                    return "NH Research, Inc.";
                case "Nike":
                    return "Nike, Inc.";
                case "Nimble":
                    return "Nimble Storage";
                case "NISCA":
                    return "NISCA Inc.";
                case "NISHAN":
                    return "Nishan Systems Inc.";
                case "Nitz":
                    return "Nitz Associates, Inc.";
                case "NKK":
                    return "NKK Corp.";
                case "NRC":
                    return "Nakamichi Research Corporation";
                case "NSD":
                    return "Nippon Systems Development Co.,Ltd.";
                case "NSM":
                    return "NSM Jukebox GmbH";
                case "nStor":
                    return "nStor Technologies, Inc.";
                case "NT":
                    return "Northern Telecom";
                case "NUCONNEX":
                    return "NuConnex";
                case "NUSPEED":
                    return "NuSpeed, Inc.";
                case "NVIDIA":
                    return "NVIDIA Corporation";
                case "NVMe":
                    return "NVM Express Working Group";
                case "OAI":
                    return "Optical Access International";
                case "OCE":
                    return "Oce Graphics";
                case "ODS":
                    return "ShenZhen DCS Group Co.,Ltd";
                case "OHDEN":
                    return "Ohden Co., Ltd.";
                case "OKI":
                    return "OKI Electric Industry Co.,Ltd (Japan)";
                case "Olidata":
                    return "Olidata S.p.A.";
                case "OMI":
                    return "Optical Media International";
                case "OMNIFI":
                    return "Rockford Corporation - Omnifi Media";
                case "OMNIS":
                    return "OMNIS Company (FRANCE)";
                case "Ophidian":
                    return "Ophidian Designs";
                case "opslag":
                    return "Tyrone Systems";
                case "Optelec":
                    return "Optelec BV";
                case "Optiarc":
                    return "Sony Optiarc Inc.";
                case "OPTIMEM":
                    return "Cipher/Optimem";
                case "OPTOTECH":
                    return "Optotech";
                case "ORACLE":
                    return "Oracle Corporation";
                case "ORANGE":
                    return "Orange Micro, Inc.";
                case "ORCA":
                    return "Orca Technology";
                case "Origin":
                    return "Origin Energy";
                case "OSI":
                    return "Optical Storage International";
                case "OSNEXUS":
                    return "OS NEXUS, Inc.";
                case "OTL":
                    return "OTL Engineering";
                case "OVERLAND":
                    return "Overland Storage Inc.";
                case "pacdigit":
                    return "Pacific Digital Corp";
                case "Packard":
                    return "Parkard Bell";
                case "Panasas":
                    return "Panasas, Inc.";
                case "PARALAN":
                    return "Paralan Corporation";
                case "PASCOsci":
                    return "Pasco Scientific";
                case "PATHLGHT":
                    return "Pathlight Technology, Inc.";
                case "PCS":
                    return "Pro Charging Systems, LLC";
                case "PerStor":
                    return "Perstor";
                case "PERTEC":
                    return "Pertec Peripherals Corporation";
                case "PFTI":
                    return "Performance Technology Inc.";
                case "PFU":
                    return "PFU Limited";
                case "Phigment":
                    return "Phigment Technologies";
                case "PHILIPS":
                    return "Philips Electronics";
                case "PICO":
                    return "Packard Instrument Company";
                case "PIK":
                    return "TECHNILIENT & MCS";
                case "Pillar":
                    return "Pillar Data Systems";
                case "PIONEER":
                    return "Pioneer Electronic Corp.";
                case "Pirus":
                    return "Pirus Networks";
                case "PIVOT3":
                    return "Pivot3, Inc.";
                case "PLASMON":
                    return "Plasmon Data";
                case "Pliant":
                    return "Pliant Technology, Inc.";
                case "PMCSIERA":
                    return "PMC-Sierra";
                case "PME":
                    return "Precision Measurement Engineering";
                case "PNNMed":
                    return "PNN Medical SA";
                case "POKEN":
                    return "Poken SA";
                case "POLYTRON":
                    return "PT. HARTONO ISTANA TEKNOLOGI";
                case "PRAIRIE":
                    return "PrairieTek";
                case "PREPRESS":
                    return "PrePRESS Solutions";
                case "PRESOFT":
                    return "PreSoft Architects";
                case "PRESTON":
                    return "Preston Scientific";
                case "PRIAM":
                    return "Priam";
                case "PRIMAGFX":
                    return "Primagraphics Ltd";
                case "PRIMOS":
                    return "Primos";
                case "PROCOM":
                    return "Procom Technology";
                case "PROLIFIC":
                    return "Prolific Technology Inc.";
                case "PROMISE":
                    return "PROMISE TECHNOLOGY, Inc";
                case "PROSTOR":
                    return "ProStor Systems, Inc.";
                case "PROSUM":
                    return "PROSUM";
                case "PROWARE":
                    return "Proware Technology Corp.";
                case "PTI":
                    return "Peripheral Technology Inc.";
                case "PTICO":
                    return "Pacific Technology International";
                case "PURE":
                    return "PURE Storage";
                case "Qi-Hardw":
                    return "Qi Hardware";
                case "QIC":
                    return "Quarter-Inch Cartridge Drive Standards, Inc.";
                case "QLogic":
                    return "QLogic Corporation";
                case "QNAP":
                    return "QNAP Systems";
                case "Qsan":
                    return "QSAN Technology, Inc.";
                case "QUALSTAR":
                    return "Qualstar";
                case "QUANTEL":
                    return "Quantel Ltd.";
                case "QUANTUM":
                    return "Quantum Corp.";
                case "QUIX":
                    return "Quix Computerware AG";
                case "R-BYTE":
                    return "R-Byte, Inc.";
                case "RACALREC":
                    return "Racal Recorders";
                case "RADITEC":
                    return "Radikal Technologies Deutschland GmbH";
                case "RADSTONE":
                    return "Radstone Technology";
                case "RAIDINC":
                    return "RAID Inc.";
                case "RASSYS":
                    return "Rasilient Systems Inc.";
                case "RASVIA":
                    return "Rasvia Systems, Inc.";
                case "rave-mp":
                    return "Go Video";
                case "RDKMSTG":
                    return "MMS Dipl. Ing. Rolf-Dieter Klein";
                case "RDStor":
                    return "Rorke China";
                case "Readboy":
                    return "Readboy Ltd Co.";
                case "Realm":
                    return "Realm Systems";
                case "realtek":
                    return "Realtek Semiconductor Corp.";
                case "REDUXIO":
                    return "Reduxio Systems Ltd.";
                case "rehanltd":
                    return "Rehan Electronics Ltd";
                case "REKA":
                    return "REKA HEALTH PTE LTD";
                case "RELDATA":
                    return "RELDATA Inc";
                case "RENAGmbH":
                    return "RENA GmbH";
                case "ReThinkM":
                    return "RETHINK MEDICAL, INC";
                case "Revivio":
                    return "Revivio, Inc.";
                case "RGBLaser":
                    return "RGB Lasersysteme GmbH";
                case "RGI":
                    return "Raster Graphics, Inc.";
                case "RHAPSODY":
                    return "Rhapsody Networks, Inc.";
                case "RHS":
                    return "Racal-Heim Systems GmbH";
                case "RICOH":
                    return "Ricoh";
                case "RODIME":
                    return "Rodime";
                case "Rorke":
                    return "RD DATA Technology (ShenZhen) Limited";
                case "Royaltek":
                    return "RoyalTek company Ltd.";
                case "RPS":
                    return "RPS";
                case "RTI":
                    return "Reference Technology";
                case "S-D":
                    return "Sauer-Danfoss";
                case "S-flex":
                    return "Storageflex Inc";
                case "S-SYSTEM":
                    return "S-SYSTEM";
                case "S1":
                    return "storONE";
                case "SAMSUNG":
                    return "Samsung Electronics Co., Ltd.";
                case "SAN":
                    return "Storage Area Networks, Ltd.";
                case "Sandial":
                    return "Sandial Systems, Inc.";
                case "SanDisk":
                    return "SanDisk Corporation";
                case "SANKYO":
                    return "Sankyo Seiki";
                case "SANRAD":
                    return "SANRAD Inc.";
                case "SANYO":
                    return "SANYO Electric Co., Ltd.";
                case "SC.Net":
                    return "StorageConnections.Net";
                case "SCALE":
                    return "Scale Computing, Inc.";
                case "SCIENTEK":
                    return "SCIENTEK CORP";
                case "SCInc.":
                    return "Storage Concepts, Inc.";
                case "SCREEN":
                    return "Dainippon Screen Mfg. Co., Ltd.";
                case "SDI":
                    return "Storage Dimensions, Inc.";
                case "SDS":
                    return "Solid Data Systems";
                case "SEAC":
                    return "SeaChange International, Inc.";
                case "SEAGATE":
                    return "Seagate";
                case "SEAGRAND":
                    return "SEAGRAND In Japan";
                case "Seanodes":
                    return "Seanodes";
                case "Sec. Key":
                    return "SecureKey Technologies Inc.";
                case "SEQUOIA":
                    return "Sequoia Advanced Technologies, Inc.";
                case "SGI":
                    return "Silicon Graphics International";
                case "Shannon":
                    return "Shannon Systems Co., Ltd.";
                case "Shinko":
                    return "Shinko Electric Co., Ltd.";
                case "SIEMENS":
                    return "Siemens";
                case "SigmaTel":
                    return "SigmaTel, Inc.";
                case "SII":
                    return "Seiko Instruments Inc.";
                case "SIMPLE":
                    return "SimpleTech, Inc.";
                case "SIVMSD":
                    return "IMAGO SOFTWARE SL";
                case "SKhynix":
                    return "SK hynix Inc.";
                case "SLCNSTOR":
                    return "SiliconStor, Inc.";
                case "SLI":
                    return "Sierra Logic, Inc.";
                case "SMCI":
                    return "Super Micro Computer, Inc.";
                case "SmrtStor":
                    return "Smart Storage Systems";
                case "SMS":
                    return "Scientific Micro Systems/OMTI";
                case "SMSC":
                    return "SMSC Storage, Inc.";
                case "SMX":
                    return "Smartronix, Inc.";
                case "SNYSIDE":
                    return "Sunnyside Computing Inc.";
                case "SoftLock":
                    return "Softlock Digital Security Provider";
                case "SolidFir":
                    return "SolidFire, Inc.";
                case "SONIC":
                    return "Sonic Solutions";
                case "SoniqCas":
                    return "SoniqCast";
                case "SONY":
                    return "Sony Corporation Japan";
                case "SOUL":
                    return "Soul Storage Technology (Wuxi) Co., Ltd";
                case "SPD":
                    return "Storage Products Distribution, Inc.";
                case "SPECIAL":
                    return "Special Computing Co.";
                case "SPECTRA":
                    return "Spectra Logic, a Division of Western Automation Labs, Inc.";
                case "SPERRY":
                    return "Sperry";
                case "Spintso":
                    return "Spintso International AB";
                case "STARBORD":
                    return "Starboard Storage Systems, Inc.";
                case "STARWIND":
                    return "StarWind Software, Inc.";
                case "STEC":
                    return "STEC, Inc.";
                case "Sterling":
                    return "Sterling Diagnostic Imaging, Inc.";
                case "STK":
                    return "Storage Technology Corporation";
                case "STNWOOD":
                    return "Stonewood Group";
                case "STONEFLY":
                    return "StoneFly Networks, Inc.";
                case "STOR":
                    return "StorageNetworks, Inc.";
                case "STORAPP":
                    return "StorageApps, Inc.";
                case "STORCIUM":
                    return "Intelligent Systems Services Inc.";
                case "STORCOMP":
                    return "Storage Computer Corporation";
                case "STORM":
                    return "Storm Technology, Inc.";
                case "StorMagc":
                    return "StorMagic";
                case "Stratus":
                    return "Stratus Technologies";
                case "StrmLgc":
                    return "StreamLogic Corp.";
                case "SUMITOMO":
                    return "Sumitomo Electric Industries, Ltd.";
                case "SUN":
                    return "Sun Microsystems, Inc.";
                case "SUNCORP":
                    return "SunCorporation";
                case "suntx":
                    return "Suntx System Co., Ltd";
                case "SUSE":
                    return "SUSE Linux";
                case "Swinxs":
                    return "Swinxs BV";
                case "SYMANTEC":
                    return "Symantec Corporation";
                case "SYMBIOS":
                    return "Symbios Logic Inc.";
                case "SYMWAVE":
                    return "Symwave, Inc.";
                case "SYNCSORT":
                    return "Syncsort Incorporated";
                case "SYNERWAY":
                    return "Synerway";
                case "SYNOLOGY":
                    return "Synology, Inc.";
                case "SyQuest":
                    return "SyQuest Technology, Inc.";
                case "SYSGEN":
                    return "Sysgen";
                case "T-MITTON":
                    return "Transmitton England";
                case "T-MOBILE":
                    return "T-Mobile USA, Inc.";
                case "T11":
                    return "INCITS Technical Committee T11";
                case "TALARIS":
                    return "Talaris Systems, Inc.";
                case "TALLGRAS":
                    return "Tallgrass Technologies";
                case "TANDBERG":
                    return "Tandberg Data A/S";
                case "TANDEM":
                    return "Tandem";
                case "TANDON":
                    return "Tandon";
                case "TCL":
                    return "TCL Shenzhen ASIC MIcro-electronics Ltd";
                case "TDK":
                    return "TDK Corporation";
                case "TEAC":
                    return "TEAC Japan";
                case "TECOLOTE":
                    return "Tecolote Designs";
                case "TEGRA":
                    return "Tegra Varityper";
                case "Teilch":
                    return "Teilch";
                case "Tek":
                    return "Tektronix";
                case "TELLERT":
                    return "Tellert Elektronik GmbH";
                case "TENTIME":
                    return "Laura Technologies, Inc.";
                case "TFDATACO":
                    return "TimeForge";
                case "TGEGROUP":
                    return "TGE Group Co.,LTD.";
                case "Thecus":
                    return "Thecus Technology Corp.";
                case "TI-DSG":
                    return "Texas Instruments";
                case "TiGi":
                    return "TiGi Corporation";
                case "TILDESGN":
                    return "Tildesign bv";
                case "Tite":
                    return "Tite Technology Limited";
                case "TKS Inc.":
                    return "TimeKeeping Systems, Inc.";
                case "TLMKS":
                    return "Telemakus LLC";
                case "TMS":
                    return "Texas Memory Systems, Inc.";
                case "TMS100":
                    return "TechnoVas";
                case "TOLISGRP":
                    return "The TOLIS Group";
                case "TOSHIBA":
                    return "Toshiba Japan";
                case "TOYOU":
                    return "TOYOU FEIJI ELECTRONICS CO.,LTD.";
                case "Tracker":
                    return "Tracker, LLC";
                case "TRIOFLEX":
                    return "Trioflex Oy";
                case "TRIPACE":
                    return "Tripace";
                case "TRLogger":
                    return "TrueLogger Ltd.";
                case "TROIKA":
                    return "Troika Networks, Inc.";
                case "TRULY":
                    return "TRULY Electronics MFG. LTD.";
                case "TRUSTED":
                    return "Trusted Data Corporation";
                case "TSSTcorp":
                    return "Toshiba Samsung Storage Technology Corporation";
                case "TZM":
                    return "TZ Medical";
                case "UD-DVR":
                    return "Bigstone Project.";
                case "UDIGITAL":
                    return "United Digital Limited";
                case "UIT":
                    return "United Infomation Technology";
                case "ULTRA":
                    return "UltraStor Corporation";
                case "UNISTOR":
                    return "Unistor Networks, Inc.";
                case "UNISYS":
                    return "Unisys";
                case "USCORE":
                    return "Underscore, Inc.";
                case "USDC":
                    return "US Design Corp.";
                case "Top VASCO":
                    return "Vasco Data Security";
                case "VDS":
                    return "Victor Data Systems Co., Ltd.";
                case "VELDANA":
                    return "VELDANA MEDICAL SA";
                case "VENTANA":
                    return "Ventana Medical Systems";
                case "Verari":
                    return "Verari Systems, Inc.";
                case "VERBATIM":
                    return "Verbatim Corporation";
                case "Vercet":
                    return "Vercet LLC";
                case "VERITAS":
                    return "VERITAS Software Corporation";
                case "Vexata":
                    return "Vexata Inc";
                case "VEXCEL":
                    return "VEXCEL IMAGING GmbH";
                case "VICOMSL1":
                    return "Vicom Systems, Inc.";
                case "VicomSys":
                    return "Vicom Systems, Inc.";
                case "VIDEXINC":
                    return "Videx, Inc.";
                case "VIOLIN":
                    return "Violin Memory, Inc.";
                case "VIRIDENT":
                    return "Virident Systems, Inc.";
                case "VITESSE":
                    return "Vitesse Semiconductor Corporation";
                case "VIXEL":
                    return "Vixel Corporation";
                case "VLS":
                    return "Van Lent Systems BV";
                case "VMAX":
                    return "VMAX Technologies Corp.";
                case "VMware":
                    return "VMware Inc.";
                case "Vobis":
                    return "Vobis Microcomputer AG";
                case "VOLTAIRE":
                    return "Voltaire Ltd.";
                case "VRC":
                    return "Vermont Research Corp.";
                case "VRugged":
                    return "Vanguard Rugged Storage";
                case "VTGadget":
                    return "Vermont Gadget Company";
                case "Waitec":
                    return "Waitec NV";
                case "WangDAT":
                    return "WangDAT";
                case "WANGTEK":
                    return "Wangtek";
                case "Wasabi":
                    return "Wasabi Systems";
                case "WAVECOM":
                    return "Wavecom";
                case "WD":
                    return "Western Digital Corporation";
                case "WDC":
                    return "Western Digital Corporation";
                case "WDIGTL":
                    return "Western Digital";
                case "WDTI":
                    return "Western Digital Technologies, Inc.";
                case "WEARNES":
                    return "Wearnes Technology Corporation";
                case "WeeraRes":
                    return "Weera Research Pte Ltd";
                case "Wildflwr":
                    return "Wildflower Technologies, Inc.";
                case "WSC0001":
                    return "Wisecom, Inc.";
                case "X3":
                    return "InterNational Committee for Information Technology Standards (INCITS)";
                case "XEBEC":
                    return "Xebec Corporation";
                case "XENSRC":
                    return "XenSource, Inc.";
                case "Xerox":
                    return "Xerox Corporation";
                case "XIOtech":
                    return "XIOtech Corporation";
                case "XIRANET":
                    return "Xiranet Communications GmbH";
                case "XIV":
                    return "XIV";
                case "XtremIO":
                    return "XtremIO";
                case "XYRATEX":
                    return "Xyratex";
                case "YINHE":
                    return "NUDT Computer Co.";
                case "YIXUN":
                    return "Yixun Electronic Co.,Ltd.";
                case "YOTTA":
                    return "YottaYotta, Inc.";
                case "Zarva":
                    return "Zarva Digital Technology Co., Ltd.";
                case "ZETTA":
                    return "Zetta Systems, Inc.";
                case "ZTE":
                    return "ZTE Corporation";
                case "ZVAULT":
                    return "Zetavault";
                default:
                    return SCSIVendorString;
            }
        }

        #endregion Private methods

        #region Public methods

        public static SCSIInquiry? DecodeSCSIInquiry(byte[] SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            if (SCSIInquiryResponse.Length < 36)
            {
                DicConsole.DebugWriteLine("SCSI INQUIRY decoder", "INQUIRY response is less than minimum of 36 bytes, decoded data can be incorrect, not decoding.");
                return null;
            }

            if (SCSIInquiryResponse.Length != SCSIInquiryResponse[4] + 5)
            {
                DicConsole.DebugWriteLine("SCSI INQUIRY decoder", "INQUIRY response length ({0} bytes) is different than specified in length field ({1} bytes), decoded data can be incorrect, not decoding.", SCSIInquiryResponse.Length, SCSIInquiryResponse[4] + 4);
                return null;
            }

            SCSIInquiry decoded = new SCSIInquiry();

            if (SCSIInquiryResponse.Length >= 1)
            {
                decoded.PeripheralQualifier = (byte)((SCSIInquiryResponse[0] & 0xE0) >> 5);
                decoded.PeripheralDeviceType = (byte)(SCSIInquiryResponse[0] & 0x1F);
            }
            if (SCSIInquiryResponse.Length >= 2)
            {
                decoded.RMB = Convert.ToBoolean((SCSIInquiryResponse[1] & 0x80));
                decoded.DeviceTypeModifier = (byte)(SCSIInquiryResponse[1] & 0x7F);
            }
            if (SCSIInquiryResponse.Length >= 3)
            {
                decoded.ISOVersion = (byte)((SCSIInquiryResponse[2] & 0xC0) >> 6);
                decoded.ECMAVersion = (byte)((SCSIInquiryResponse[2] & 0x38) >> 3);
                decoded.ANSIVersion = (byte)(SCSIInquiryResponse[2] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 4)
            {
                decoded.AERC = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x80));
                decoded.TrmTsk = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x40));
                decoded.NormACA = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x20));
                decoded.HiSup = Convert.ToBoolean((SCSIInquiryResponse[3] & 0x10));
                decoded.ResponseDataFormat = (byte)(SCSIInquiryResponse[3] & 0x07);
            }
            if (SCSIInquiryResponse.Length >= 5)
                decoded.AdditionalLength = SCSIInquiryResponse[4];
            if (SCSIInquiryResponse.Length >= 6)
            {
                decoded.SCCS = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x80));
                decoded.ACC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x40));
                decoded.TPGS = (byte)((SCSIInquiryResponse[5] & 0x30) >> 4);
                decoded.ThreePC = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x08));
                decoded.Reserved2 = (byte)((SCSIInquiryResponse[5] & 0x06) >> 1);
                decoded.Protect = Convert.ToBoolean((SCSIInquiryResponse[5] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 7)
            {
                decoded.BQue = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x80));
                decoded.EncServ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x40));
                decoded.VS1 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x20));
                decoded.MultiP = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x10));
                decoded.MChngr = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x08));
                decoded.ACKREQQ = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x04));
                decoded.Addr32 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x02));
                decoded.Addr16 = Convert.ToBoolean((SCSIInquiryResponse[6] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 8)
            {
                decoded.RelAddr = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x80));
                decoded.WBus32 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x40));
                decoded.WBus16 = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x20));
                decoded.Sync = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x10));
                decoded.Linked = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x08));
                decoded.TranDis = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x04));
                decoded.CmdQue = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x02));
                decoded.SftRe = Convert.ToBoolean((SCSIInquiryResponse[7] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 16)
            {
                decoded.VendorIdentification = new byte[8];
                Array.Copy(SCSIInquiryResponse, 8, decoded.VendorIdentification, 0, 8);
            }
            if (SCSIInquiryResponse.Length >= 32)
            {
                decoded.ProductIdentification = new byte[16];
                Array.Copy(SCSIInquiryResponse, 16, decoded.ProductIdentification, 0, 16);
            }
            if (SCSIInquiryResponse.Length >= 36)
            {
                decoded.ProductRevisionLevel = new byte[4];
                Array.Copy(SCSIInquiryResponse, 32, decoded.ProductRevisionLevel, 0, 4);
            }
            if (SCSIInquiryResponse.Length >= 56)
            {
                decoded.VendorSpecific = new byte[20];
                Array.Copy(SCSIInquiryResponse, 36, decoded.VendorSpecific, 0, 20);
            }
            if (SCSIInquiryResponse.Length >= 57)
            {
                decoded.Reserved3 = (byte)((SCSIInquiryResponse[56] & 0xF0) >> 4);
                decoded.Clocking = (byte)((SCSIInquiryResponse[56] & 0x0C) >> 2);
                decoded.QAS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x02));
                decoded.IUS = Convert.ToBoolean((SCSIInquiryResponse[56] & 0x01));
            }
            if (SCSIInquiryResponse.Length >= 58)
                decoded.Reserved4 = SCSIInquiryResponse[57];
            if (SCSIInquiryResponse.Length >= 60)
            {
                int descriptorsNo;

                if (SCSIInquiryResponse.Length >= 74)
                    descriptorsNo = 8;
                else
                    descriptorsNo = (SCSIInquiryResponse.Length - 58) / 2;
                
                decoded.VersionDescriptors = new ushort[descriptorsNo];
                for (int i = 0; i < descriptorsNo; i++)
                {
                    decoded.VersionDescriptors[i] = BitConverter.ToUInt16(SCSIInquiryResponse, 58 + (i * 2));
                }
            }
            if (SCSIInquiryResponse.Length >= 75 && SCSIInquiryResponse.Length < 96)
            {
                decoded.Reserved5 = new byte[SCSIInquiryResponse.Length - 74];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 74);
            }
            if (SCSIInquiryResponse.Length >= 96)
            {
                decoded.Reserved5 = new byte[22];
                Array.Copy(SCSIInquiryResponse, 74, decoded.Reserved5, 0, 22);
            }
            if (SCSIInquiryResponse.Length > 96)
            {
                decoded.VendorSpecific2 = new byte[SCSIInquiryResponse.Length - 96];
                Array.Copy(SCSIInquiryResponse, 96, decoded.Reserved5, 0, SCSIInquiryResponse.Length - 96);
            }

            return decoded;
        }

        public static string PrettifySCSIInquiry(SCSIInquiry? SCSIInquiryResponse)
        {
            if (SCSIInquiryResponse == null)
                return null;

            SCSIInquiry response = SCSIInquiryResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Device vendor: {0}", PrettifySCSIVendorString(StringHandlers.SpacePaddedToString(response.VendorIdentification))).AppendLine();
            sb.AppendFormat("Device name: {0}", StringHandlers.SpacePaddedToString(response.ProductIdentification)).AppendLine();
            sb.AppendFormat("Device release level: {0}", StringHandlers.SpacePaddedToString(response.ProductRevisionLevel)).AppendLine();
            switch ((SCSIPeripheralQualifiers)response.PeripheralQualifier)
            {
                case SCSIPeripheralQualifiers.SCSIPQSupported:
                    sb.AppendLine("Device is connected and supported.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQUnconnected:
                    sb.AppendLine("Device is supported but not connected.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQReserved:
                    sb.AppendLine("Reserved value set in Peripheral Qualifier field.");
                    break;
                case SCSIPeripheralQualifiers.SCSIPQUnsupported:
                    sb.AppendLine("Device is connected but unsupported.");
                    break;
                default:
                    sb.AppendFormat("Vendor value {0} set in Peripheral Qualifier field.", response.PeripheralQualifier).AppendLine();
                    break;
            }

            switch ((SCSIPeripheralDeviceTypes)response.PeripheralDeviceType)
            {
                case SCSIPeripheralDeviceTypes.SCSIPDTDirectAccess: //0x00,
                    sb.AppendLine("Direct-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTSequentialAccess: //0x01,
                    sb.AppendLine("Sequential-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrinterDevice: //0x02,
                    sb.AppendLine("Printer device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTProcessorDevice: //0x03,
                    sb.AppendLine("Processor device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTWriteOnceDevice: //0x04,
                    sb.AppendLine("Write-once device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTMultiMediaDevice: //0x05,
                    sb.AppendLine("CD-ROM/DVD/etc device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTScannerDevice: //0x06,
                    sb.AppendLine("Scanner device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTOpticalDevice: //0x07,
                    sb.AppendLine("Optical memory device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTMediumChangerDevice: //0x08,
                    sb.AppendLine("Medium change device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTCommsDevice: //0x09,
                    sb.AppendLine("Communications device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrePressDevice1: //0x0A,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTPrePressDevice2: //0x0B,
                    sb.AppendLine("Graphics arts pre-press device (defined in ASC IT8)");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTArrayControllerDevice: //0x0C,
                    sb.AppendLine("Array controller device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTEnclosureServiceDevice: //0x0D,
                    sb.AppendLine("Enclosure services device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTSimplifiedDevice: //0x0E,
                    sb.AppendLine("Simplified direct-access device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTOCRWDevice: //0x0F,
                    sb.AppendLine("Optical card reader/writer device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTBridgingExpander: //0x10,
                    sb.AppendLine("Bridging Expanders");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTObjectDevice: //0x11,
                    sb.AppendLine("Object-based Storage Device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTADCDevice: //0x12,
                    sb.AppendLine("Automation/Drive Interface");
                    break;
                case SCSIPeripheralDeviceTypes.SCSISecurityManagerDevice: //0x13,
                    sb.AppendLine("Security Manager Device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIZonedBlockDEvice: //0x14
                    sb.AppendLine("Host managed zoned block device");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTWellKnownDevice: //0x1E,
                    sb.AppendLine("Well known logical unit");
                    break;
                case SCSIPeripheralDeviceTypes.SCSIPDTUnknownDevice: //0x1F
                    sb.AppendLine("Unknown or no device type");
                    break;
                default:
                    sb.AppendFormat("Unknown device type field value 0x{0:X2}", response.PeripheralDeviceType).AppendLine();
                    break;
            }

            switch ((SCSIANSIVersions)response.ANSIVersion)
            {
                case SCSIANSIVersions.SCSIANSINoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ANSI standard");
                    break;
                case SCSIANSIVersions.SCSIANSI1986Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1986 (SCSI-1)");
                    break;
                case SCSIANSIVersions.SCSIANSI1994Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.131:1994 (SCSI-2)");
                    break;
                case SCSIANSIVersions.SCSIANSI1997Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.301:1997 (SPC-1)");
                    break;
                case SCSIANSIVersions.SCSIANSI2001Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.351:2001 (SPC-2)");
                    break;
                case SCSIANSIVersions.SCSIANSI2005Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.408:2005 (SPC-3)");
                    break;
                case SCSIANSIVersions.SCSIANSI2008Version:
                    sb.AppendLine("Device claims to comply with ANSI X3.408:2005 (SPC-4)");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ANSI standard value 0x{0:X2})", response.ANSIVersion).AppendLine();
                    break;
            }

            switch ((SCSIECMAVersions)response.ECMAVersion)
            {
                case SCSIECMAVersions.SCSIECMANoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ECMA standard");
                    break;
                case SCSIECMAVersions.SCSIECMAObsolete:
                    sb.AppendLine("Device claims to comply with an obsolete SCSI ECMA standard");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ECMA standard value 0x{0:X2})", response.ECMAVersion).AppendLine();
                    break;
            }

            switch ((SCSIISOVersions)response.ISOVersion)
            {
                case SCSIISOVersions.SCSIISONoVersion:
                    sb.AppendLine("Device does not claim to comply with any SCSI ISO/IEC standard");
                    break;
                case SCSIISOVersions.SCSIISO1995Version:
                    sb.AppendLine("Device claims to comply with ISO/IEC 9316:1995");
                    break;
                default:
                    sb.AppendFormat("Device claims to comply with unknown SCSI ISO/IEC standard value 0x{0:X2})", response.ISOVersion).AppendLine();
                    break;
            }

            if (response.RMB)
                sb.AppendLine("Device is removable");
            if (response.AERC)
                sb.AppendLine("Device supports Asynchronous Event Reporting Capability");
            if (response.TrmTsk)
                sb.AppendLine("Device supports TERMINATE TASK command");
            if (response.NormACA)
                sb.AppendLine("Device supports setting Normal ACA");
            if (response.HiSup)
                sb.AppendLine("Device supports LUN hierarchical addressing");
            if (response.SCCS)
                sb.AppendLine("Device contains an embedded storage array controller");
            if (response.ACC)
                sb.AppendLine("Device contains an Access Control Coordinator");
            if (response.ThreePC)
                sb.AppendLine("Device supports third-party copy commands");
            if (response.Protect)
                sb.AppendLine("Device supports protection information");
            if (response.BQue)
                sb.AppendLine("Device supports basic queueing");
            if (response.EncServ)
                sb.AppendLine("Device contains an embedded enclosure services component");
            if (response.MultiP)
                sb.AppendLine("Multi-port device");
            if (response.MChngr)
                sb.AppendLine("Device contains or is attached to a medium changer");
            if (response.ACKREQQ)
                sb.AppendLine("Device supports request and acknowledge handshakes");
            if (response.Addr32)
                sb.AppendLine("Device supports 32-bit wide SCSI addresses");
            if (response.Addr16)
                sb.AppendLine("Device supports 16-bit wide SCSI addresses");
            if (response.RelAddr)
                sb.AppendLine("Device supports relative addressing");
            if (response.WBus32)
                sb.AppendLine("Device supports 32-bit wide data transfers");
            if (response.WBus16)
                sb.AppendLine("Device supports 16-bit wide data transfers");
            if (response.Sync)
                sb.AppendLine("Device supports synchronous data transfer");
            if (response.Linked)
                sb.AppendLine("Device supports linked commands");
            if (response.TranDis)
                sb.AppendLine("Device supports CONTINUE TASK and TARGET TRANSFER DISABLE commands");
            if (response.QAS)
                sb.AppendLine("Device supports Quick Arbitration and Selection");
            if (response.CmdQue)
                sb.AppendLine("Device supports TCQ queue");
            if (response.IUS)
                sb.AppendLine("Device supports information unit transfers");
            if (response.SftRe)
                sb.AppendLine("Device implements RESET as a soft reset");
            #if DEBUG
            if (response.VS1)
                sb.AppendLine("Vendor specific bit 5 on byte 6 of INQUIRY response is set");
            #endif

            switch ((SCSITGPSValues)response.TPGS)
            {
                case SCSITGPSValues.NotSupported:
                    sb.AppendLine("Device does not support assymetrical access");
                    break;
                case SCSITGPSValues.OnlyImplicit:
                    sb.AppendLine("Device only supports implicit assymetrical access");
                    break;
                case SCSITGPSValues.OnlyExplicit:
                    sb.AppendLine("Device only supports explicit assymetrical access");
                    break;
                case SCSITGPSValues.Both:
                    sb.AppendLine("Device supports implicit and explicit assymetrical access");
                    break;
                default:
                    sb.AppendFormat("Unknown value in TPGS field 0x{0:X2}", response.TPGS).AppendLine();
                    break;
            }

            switch ((SCSISPIClocking)response.Clocking)
            {
                case SCSISPIClocking.SCSIClockingST:
                    sb.AppendLine("Device supports only ST clocking");
                    break;
                case SCSISPIClocking.SCSIClockingDT:
                    sb.AppendLine("Device supports only DT clocking");
                    break;
                case SCSISPIClocking.SCSIClockingReserved:
                    sb.AppendLine("Reserved value 0x02 found in SPI clocking field");
                    break;
                case SCSISPIClocking.SCSIClockingSTandDT:
                    sb.AppendLine("Device supports ST and DT clocking");
                    break;
                default:
                    sb.AppendFormat("Unknown value in SPI clocking field 0x{0:X2}", response.Clocking).AppendLine();
                    break;
            }

            if (response.VersionDescriptors != null)
            {
                foreach (UInt16 VersionDescriptor in response.VersionDescriptors)
                {
                    switch (VersionDescriptor)
                    {
                        case 0xFFFF:
                        case 0x0000:
                            break;
                        case 0x0020:
                            sb.AppendLine("Device complies with SAM (no version claimed)");
                            break;
                        case 0x003B:
                            sb.AppendLine("Device complies with SAM T10/0994-D revision 18");
                            break;
                        case 0x003C:
                            sb.AppendLine("Device complies with SAM ANSI INCITS 270-1996");
                            break;
                        case 0x0040:
                            sb.AppendLine("Device complies with SAM-2 (no version claimed)");
                            break;
                        case 0x0054:
                            sb.AppendLine("Device complies with SAM-2 T10/1157-D revision 23");
                            break;
                        case 0x0055:
                            sb.AppendLine("Device complies with SAM-2 T10/1157-D revision 24");
                            break;
                        case 0x005C:
                            sb.AppendLine("Device complies with SAM-2 ANSI INCITS 366-2003");
                            break;
                        case 0x005E:
                            sb.AppendLine("Device complies with SAM-2 ISO/IEC 14776-412");
                            break;
                        case 0x0060:
                            sb.AppendLine("Device complies with SAM-3 (no version claimed)");
                            break;
                        case 0x0062:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 7");
                            break;
                        case 0x0075:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 13");
                            break;
                        case 0x0076:
                            sb.AppendLine("Device complies with SAM-3 T10/1561-D revision 14");
                            break;
                        case 0x0077:
                            sb.AppendLine("Device complies with SAM-3 ANSI INCITS 402-2005");
                            break;
                        case 0x0080:
                            sb.AppendLine("Device complies with SAM-4 (no version claimed)");
                            break;
                        case 0x0087:
                            sb.AppendLine("Device complies with SAM-4 T10/1683-D revision 13");
                            break;
                        case 0x008B:
                            sb.AppendLine("Device complies with SAM-4 T10/1683-D revision 14");
                            break;
                        case 0x0090:
                            sb.AppendLine("Device complies with SAM-4 ANSI INCITS 447-2008");
                            break;
                        case 0x0092:
                            sb.AppendLine("Device complies with SAM-4 ISO/IEC 14776-414");
                            break;
                        case 0x00A0:
                            sb.AppendLine("Device complies with SAM-5 (no version claimed)");
                            break;
                        case 0x00A2:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 4");
                            break;
                        case 0x00A4:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 20");
                            break;
                        case 0x00A6:
                            sb.AppendLine("Device complies with SAM-5 T10/2104-D revision 21");
                            break;
                        case 0x00C0:
                            sb.AppendLine("Device complies with SAM-6 (no version claimed)");
                            break;
                        case 0x0120:
                            sb.AppendLine("Device complies with SPC (no version claimed)");
                            break;
                        case 0x013B:
                            sb.AppendLine("Device complies with SPC T10/0995-D revision 11a");
                            break;
                        case 0x013C:
                            sb.AppendLine("Device complies with SPC ANSI INCITS 301-1997");
                            break;
                        case 0x0140:
                            sb.AppendLine("Device complies with MMC (no version claimed)");
                            break;
                        case 0x015B:
                            sb.AppendLine("Device complies with MMC T10/1048-D revision 10a");
                            break;
                        case 0x015C:
                            sb.AppendLine("Device complies with MMC ANSI INCITS 304-1997");
                            break;
                        case 0x0160:
                            sb.AppendLine("Device complies with SCC (no version claimed)");
                            break;
                        case 0x017B:
                            sb.AppendLine("Device complies with SCC T10/1047-D revision 06c");
                            break;
                        case 0x017C:
                            sb.AppendLine("Device complies with SCC ANSI INCITS 276-1997");
                            break;
                        case 0x0180:
                            sb.AppendLine("Device complies with SBC (no version claimed)");
                            break;
                        case 0x019B:
                            sb.AppendLine("Device complies with SBC T10/0996-D revision 08c");
                            break;
                        case 0x019C:
                            sb.AppendLine("Device complies with SBC ANSI INCITS 306-1998");
                            break;
                        case 0x01A0:
                            sb.AppendLine("Device complies with SMC (no version claimed)");
                            break;
                        case 0x01BB:
                            sb.AppendLine("Device complies with SMC T10/0999-D revision 10a");
                            break;
                        case 0x01BC:
                            sb.AppendLine("Device complies with SMC ANSI INCITS 314-1998");
                            break;
                        case 0x01BE:
                            sb.AppendLine("Device complies with SMC ISO/IEC 14776-351");
                            break;
                        case 0x01C0:
                            sb.AppendLine("Device complies with SES (no version claimed)");
                            break;
                        case 0x01DB:
                            sb.AppendLine("Device complies with SES T10/1212-D revision 08b");
                            break;
                        case 0x01DC:
                            sb.AppendLine("Device complies with SES ANSI INCITS 305-1998");
                            break;
                        case 0x01DD:
                            sb.AppendLine("Device complies with SES T10/1212 revision 08b w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01DE:
                            sb.AppendLine("Device complies with SES ANSI INCITS 305-1998 w/ Amendment ANSI INCITS.305/AM1-2000");
                            break;
                        case 0x01E0:
                            sb.AppendLine("Device complies with SCC-2 (no version claimed)");
                            break;
                        case 0x01FB:
                            sb.AppendLine("Device complies with SCC-2 T10/1125-D revision 04");
                            break;
                        case 0x01FC:
                            sb.AppendLine("Device complies with SCC-2 ANSI INCITS 318-1998");
                            break;
                        case 0x0200:
                            sb.AppendLine("Device complies with SSC (no version claimed)");
                            break;
                        case 0x0201:
                            sb.AppendLine("Device complies with SSC T10/0997-D revision 17");
                            break;
                        case 0x0207:
                            sb.AppendLine("Device complies with SSC T10/0997-D revision 22");
                            break;
                        case 0x021C:
                            sb.AppendLine("Device complies with SSC ANSI INCITS 335-2000");
                            break;
                        case 0x0220:
                            sb.AppendLine("Device complies with RBC (no version claimed)");
                            break;
                        case 0x0238:
                            sb.AppendLine("Device complies with RBC T10/1240-D revision 10a");
                            break;
                        case 0x023C:
                            sb.AppendLine("Device complies with RBC ANSI INCITS 330-2000");
                            break;
                        case 0x0240:
                            sb.AppendLine("Device complies with MMC-2 (no version claimed)");
                            break;
                        case 0x0255:
                            sb.AppendLine("Device complies with MMC-2 T10/1228-D revision 11");
                            break;
                        case 0x025B:
                            sb.AppendLine("Device complies with MMC-2 T10/1228-D revision 11a");
                            break;
                        case 0x025C:
                            sb.AppendLine("Device complies with MMC-2 ANSI INCITS 333-2000");
                            break;
                        case 0x0260:
                            sb.AppendLine("Device complies with SPC-2 (no version claimed)");
                            break;
                        case 0x0267:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 12");
                            break;
                        case 0x0269:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 18");
                            break;
                        case 0x0275:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 19");
                            break;
                        case 0x0276:
                            sb.AppendLine("Device complies with SPC-2 T10/1236-D revision 20");
                            break;
                        case 0x0277:
                            sb.AppendLine("Device complies with SPC-2 ANSI INCITS 351-2001");
                            break;
                        case 0x0278:
                            sb.AppendLine("Device complies with SPC-2 ISO/IEC 14776-452");
                            break;
                        case 0x0280:
                            sb.AppendLine("Device complies with OCRW (no version claimed)");
                            break;
                        case 0x029E:
                            sb.AppendLine("Device complies with OCRW ISO/IEC 14776-381");
                            break;
                        case 0x02A0:
                            sb.AppendLine("Device complies with MMC-3 (no version claimed)");
                            break;
                        case 0x02B5:
                            sb.AppendLine("Device complies with MMC-3 T10/1363-D revision 9");
                            break;
                        case 0x02B6:
                            sb.AppendLine("Device complies with MMC-3 T10/1363-D revision 10g");
                            break;
                        case 0x02B8:
                            sb.AppendLine("Device complies with MMC-3 ANSI INCITS 360-2002");
                            break;
                        case 0x02E0:
                            sb.AppendLine("Device complies with SMC-2 (no version claimed)");
                            break;
                        case 0x02F5:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 5");
                            break;
                        case 0x02FC:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 6");
                            break;
                        case 0x02FD:
                            sb.AppendLine("Device complies with SMC-2 T10/1383-D revision 7");
                            break;
                        case 0x02FE:
                            sb.AppendLine("Device complies with SMC-2 ANSI INCITS 382-2004");
                            break;
                        case 0x0300:
                            sb.AppendLine("Device complies with SPC-3 (no version claimed)");
                            break;
                        case 0x0301:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 7");
                            break;
                        case 0x0307:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 21");
                            break;
                        case 0x030F:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 22");
                            break;
                        case 0x0312:
                            sb.AppendLine("Device complies with SPC-3 T10/1416-D revision 23");
                            break;
                        case 0x0314:
                            sb.AppendLine("Device complies with SPC-3 ANSI INCITS 408-2005");
                            break;
                        case 0x0316:
                            sb.AppendLine("Device complies with SPC-3 ISO/IEC 14776-453");
                            break;
                        case 0x0320:
                            sb.AppendLine("Device complies with SBC-2 (no version claimed)");
                            break;
                        case 0x0322:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 5a");
                            break;
                        case 0x0324:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 15");
                            break;
                        case 0x033B:
                            sb.AppendLine("Device complies with SBC-2 T10/1417-D revision 16");
                            break;
                        case 0x033D:
                            sb.AppendLine("Device complies with SBC-2 ANSI INCITS 405-2005");
                            break;
                        case 0x033E:
                            sb.AppendLine("Device complies with SBC-2 ISO/IEC 14776-322");
                            break;
                        case 0x0340:
                            sb.AppendLine("Device complies with OSD (no version claimed)");
                            break;
                        case 0x0341:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 0");
                            break;
                        case 0x0342:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 7a");
                            break;
                        case 0x0343:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 8");
                            break;
                        case 0x0344:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 9");
                            break;
                        case 0x0355:
                            sb.AppendLine("Device complies with OSD T10/1355-D revision 10");
                            break;
                        case 0x0356:
                            sb.AppendLine("Device complies with OSD ANSI INCITS 400-2004");
                            break;
                        case 0x0360:
                            sb.AppendLine("Device complies with SSC-2 (no version claimed)");
                            break;
                        case 0x0374:
                            sb.AppendLine("Device complies with SSC-2 T10/1434-D revision 7");
                            break;
                        case 0x0375:
                            sb.AppendLine("Device complies with SSC-2 T10/1434-D revision 9");
                            break;
                        case 0x037D:
                            sb.AppendLine("Device complies with SSC-2 ANSI INCITS 380-2003");
                            break;
                        case 0x0380:
                            sb.AppendLine("Device complies with BCC (no version claimed)");
                            break;
                        case 0x03A0:
                            sb.AppendLine("Device complies with MMC-4 (no version claimed)");
                            break;
                        case 0x03B0:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 5");
                            break;
                        case 0x03B1:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 5a");
                            break;
                        case 0x03BD:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 3");
                            break;
                        case 0x03BE:
                            sb.AppendLine("Device complies with MMC-4 T10/1545-D revision 3d");
                            break;
                        case 0x03BF:
                            sb.AppendLine("Device complies with MMC-4 ANSI INCITS 401-2005");
                            break;
                        case 0x03C0:
                            sb.AppendLine("Device complies with ADC (no version claimed)");
                            break;
                        case 0x03D5:
                            sb.AppendLine("Device complies with ADC T10/1558-D revision 6");
                            break;
                        case 0x03D6:
                            sb.AppendLine("Device complies with ADC T10/1558-D revision 7");
                            break;
                        case 0x03D7:
                            sb.AppendLine("Device complies with ADC ANSI INCITS 403-2005");
                            break;
                        case 0x03E0:
                            sb.AppendLine("Device complies with SES-2 (no version claimed)");
                            break;
                        case 0x03E1:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 16");
                            break;
                        case 0x03E7:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 19");
                            break;
                        case 0x03EB:
                            sb.AppendLine("Device complies with SES-2 T10/1559-D revision 20");
                            break;
                        case 0x03F0:
                            sb.AppendLine("Device complies with SES-2 ANSI INCITS 448-2008");
                            break;
                        case 0x03F2:
                            sb.AppendLine("Device complies with SES-2 ISO/IEC 14776-372");
                            break;
                        case 0x0400:
                            sb.AppendLine("Device complies with SSC-3 (no version claimed)");
                            break;
                        case 0x0403:
                            sb.AppendLine("Device complies with SSC-3 T10/1611-D revision 04a");
                            break;
                        case 0x0407:
                            sb.AppendLine("Device complies with SSC-3 T10/1611-D revision 05");
                            break;
                        case 0x0409:
                            sb.AppendLine("Device complies with SSC-3 ANSI INCITS 467-2011");
                            break;
                        case 0x040B:
                            sb.AppendLine("Device complies with SSC-3 ISO/IEC 14776-333:2013");
                            break;
                        case 0x0420:
                            sb.AppendLine("Device complies with MMC-5 (no version claimed)");
                            break;
                        case 0x042F:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 03");
                            break;
                        case 0x0431:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 03b");
                            break;
                        case 0x0432:
                            sb.AppendLine("Device complies with MMC-5 T10/1675-D revision 04");
                            break;
                        case 0x0434:
                            sb.AppendLine("Device complies with MMC-5 ANSI INCITS 430-2007");
                            break;
                        case 0x0440:
                            sb.AppendLine("Device complies with OSD-2 (no version claimed)");
                            break;
                        case 0x0444:
                            sb.AppendLine("Device complies with OSD-2 T10/1729-D revision 4");
                            break;
                        case 0x0446:
                            sb.AppendLine("Device complies with OSD-2 T10/1729-D revision 5");
                            break;
                        case 0x0448:
                            sb.AppendLine("Device complies with OSD-2 ANSI INCITS 458-2011");
                            break;
                        case 0x0460:
                            sb.AppendLine("Device complies with SPC-4 (no version claimed)");
                            break;
                        case 0x0461:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 16");
                            break;
                        case 0x0462:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 18");
                            break;
                        case 0x0463:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 23");
                            break;
                        case 0x0466:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 36");
                            break;
                        case 0x0468:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 37");
                            break;
                        case 0x0469:
                            sb.AppendLine("Device complies with SPC-4 T10/BSR INCITS 513 revision 37a");
                            break;
                        case 0x046C:
                            sb.AppendLine("Device complies with SPC-4 ANSI INCITS 513-2015");
                            break;
                        case 0x0480:
                            sb.AppendLine("Device complies with SMC-3 (no version claimed)");
                            break;
                        case 0x0482:
                            sb.AppendLine("Device complies with SMC-3 T10/1730-D revision 15");
                            break;
                        case 0x0484:
                            sb.AppendLine("Device complies with SMC-3 T10/1730-D revision 16");
                            break;
                        case 0x0486:
                            sb.AppendLine("Device complies with SMC-3 ANSI INCITS 484-2012");
                            break;
                        case 0x04A0:
                            sb.AppendLine("Device complies with ADC-2 (no version claimed)");
                            break;
                        case 0x04A7:
                            sb.AppendLine("Device complies with ADC-2 T10/1741-D revision 7");
                            break;
                        case 0x04AA:
                            sb.AppendLine("Device complies with ADC-2 T10/1741-D revision 8");
                            break;
                        case 0x04AC:
                            sb.AppendLine("Device complies with ADC-2 ANSI INCITS 441-2008");
                            break;
                        case 0x04C0:
                            sb.AppendLine("Device complies with SBC-3 (no version claimed)");
                            break;
                        case 0x04C3:
                            sb.AppendLine("Device complies with SBC-3 T10/BSR INCITS 514 revision 35");
                            break;
                        case 0x04C5:
                            sb.AppendLine("Device complies with SBC-3 T10/BSR INCITS 514 revision 36");
                            break;
                        case 0x04C8:
                            sb.AppendLine("Device complies with SBC-3 ANSI INCITS 514-2014");
                            break;
                        case 0x04E0:
                            sb.AppendLine("Device complies with MMC-6 (no version claimed)");
                            break;
                        case 0x04E3:
                            sb.AppendLine("Device complies with MMC-6 T10/1836-D revision 02b");
                            break;
                        case 0x04E5:
                            sb.AppendLine("Device complies with MMC-6 T10/1836-D revision 02g");
                            break;
                        case 0x04E6:
                            sb.AppendLine("Device complies with MMC-6 ANSI INCITS 468-2010");
                            break;
                        case 0x04E7:
                            sb.AppendLine("Device complies with MMC-6 ANSI INCITS 468-2010 + MMC-6/AM1 ANSI INCITS 468-2010/AM 1");
                            break;
                        case 0x0500:
                            sb.AppendLine("Device complies with ADC-3 (no version claimed)");
                            break;
                        case 0x0502:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 04");
                            break;
                        case 0x0504:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 05");
                            break;
                        case 0x0506:
                            sb.AppendLine("Device complies with ADC-3 T10/1895-D revision 05a");
                            break;
                        case 0x050A:
                            sb.AppendLine("Device complies with ADC-3 ANSI INCITS 497-2012");
                            break;
                        case 0x0520:
                            sb.AppendLine("Device complies with SSC-4 (no version claimed)");
                            break;
                        case 0x0523:
                            sb.AppendLine("Device complies with SSC-4 T10/BSR INCITS 516 revision 2");
                            break;
                        case 0x0525:
                            sb.AppendLine("Device complies with SSC-4 T10/BSR INCITS 516 revision 3");
                            break;
                        case 0x0527:
                            sb.AppendLine("Device complies with SSC-4 ANSI INCITS 516-2013");
                            break;
                        case 0x0560:
                            sb.AppendLine("Device complies with OSD-3 (no version claimed)");
                            break;
                        case 0x0580:
                            sb.AppendLine("Device complies with SES-3 (no version claimed)");
                            break;
                        case 0x05A0:
                            sb.AppendLine("Device complies with SSC-5 (no version claimed)");
                            break;
                        case 0x05C0:
                            sb.AppendLine("Device complies with SPC-5 (no version claimed)");
                            break;
                        case 0x05E0:
                            sb.AppendLine("Device complies with SFSC (no version claimed)");
                            break;
                        case 0x05E3:
                            sb.AppendLine("Device complies with SFSC BSR INCITS 501 revision 01");
                            break;
                        case 0x0600:
                            sb.AppendLine("Device complies with SBC-4 (no version claimed)");
                            break;
                        case 0x0620:
                            sb.AppendLine("Device complies with ZBC (no version claimed)");
                            break;
                        case 0x0622:
                            sb.AppendLine("Device complies with ZBC BSR INCITS 536 revision 02");
                            break;
                        case 0x0640:
                            sb.AppendLine("Device complies with ADC-4 (no version claimed)");
                            break;
                        case 0x0820:
                            sb.AppendLine("Device complies with SSA-TL2 (no version claimed)");
                            break;
                        case 0x083B:
                            sb.AppendLine("Device complies with SSA-TL2 T10.1/1147-D revision 05b");
                            break;
                        case 0x083C:
                            sb.AppendLine("Device complies with SSA-TL2 ANSI INCITS 308-1998");
                            break;
                        case 0x0840:
                            sb.AppendLine("Device complies with SSA-TL1 (no version claimed)");
                            break;
                        case 0x085B:
                            sb.AppendLine("Device complies with SSA-TL1 T10.1/0989-D revision 10b");
                            break;
                        case 0x085C:
                            sb.AppendLine("Device complies with SSA-TL1 ANSI INCITS 295-1996");
                            break;
                        case 0x0860:
                            sb.AppendLine("Device complies with SSA-S3P (no version claimed)");
                            break;
                        case 0x087B:
                            sb.AppendLine("Device complies with SSA-S3P T10.1/1051-D revision 05b");
                            break;
                        case 0x087C:
                            sb.AppendLine("Device complies with SSA-S3P ANSI INCITS 309-1998");
                            break;
                        case 0x0880:
                            sb.AppendLine("Device complies with SSA-S2P (no version claimed)");
                            break;
                        case 0x089B:
                            sb.AppendLine("Device complies with SSA-S2P T10.1/1121-D revision 07b");
                            break;
                        case 0x089C:
                            sb.AppendLine("Device complies with SSA-S2P ANSI INCITS 294-1996");
                            break;
                        case 0x08A0:
                            sb.AppendLine("Device complies with SIP (no version claimed)");
                            break;
                        case 0x08BB:
                            sb.AppendLine("Device complies with SIP T10/0856-D revision 10");
                            break;
                        case 0x08BC:
                            sb.AppendLine("Device complies with SIP ANSI INCITS 292-1997");
                            break;
                        case 0x08C0:
                            sb.AppendLine("Device complies with FCP (no version claimed)");
                            break;
                        case 0x08DB:
                            sb.AppendLine("Device complies with FCP T10/0993-D revision 12");
                            break;
                        case 0x08DC:
                            sb.AppendLine("Device complies with FCP ANSI INCITS 269-1996");
                            break;
                        case 0x08E0:
                            sb.AppendLine("Device complies with SBP-2 (no version claimed)");
                            break;
                        case 0x08FB:
                            sb.AppendLine("Device complies with SBP-2 T10/1155-D revision 04");
                            break;
                        case 0x08FC:
                            sb.AppendLine("Device complies with SBP-2 ANSI INCITS 325-1998");
                            break;
                        case 0x0900:
                            sb.AppendLine("Device complies with FCP-2 (no version claimed)");
                            break;
                        case 0x0901:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 4");
                            break;
                        case 0x0915:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 7");
                            break;
                        case 0x0916:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 7a");
                            break;
                        case 0x0917:
                            sb.AppendLine("Device complies with FCP-2 ANSI INCITS 350-2003");
                            break;
                        case 0x0918:
                            sb.AppendLine("Device complies with FCP-2 T10/1144-D revision 8");
                            break;
                        case 0x0920:
                            sb.AppendLine("Device complies with SST (no version claimed)");
                            break;
                        case 0x0935:
                            sb.AppendLine("Device complies with SST T10/1380-D revision 8b");
                            break;
                        case 0x0940:
                            sb.AppendLine("Device complies with SRP (no version claimed)");
                            break;
                        case 0x0954:
                            sb.AppendLine("Device complies with SRP T10/1415-D revision 10");
                            break;
                        case 0x0955:
                            sb.AppendLine("Device complies with SRP T10/1415-D revision 16a");
                            break;
                        case 0x095C:
                            sb.AppendLine("Device complies with SRP ANSI INCITS 365-2002");
                            break;
                        case 0x0960:
                            sb.AppendLine("Device complies with iSCSI (no version claimed)");
                            break;
                        case 0x0961:
                        case 0x0962:
                        case 0x0963:
                        case 0x0964:
                        case 0x0965:
                        case 0x0966:
                        case 0x0967:
                        case 0x0968:
                        case 0x0969:
                        case 0x096A:
                        case 0x096B:
                        case 0x096C:
                        case 0x096D:
                        case 0x096E:
                        case 0x096F:
                        case 0x0970:
                        case 0x0971:
                        case 0x0972:
                        case 0x0973:
                        case 0x0974:
                        case 0x0975:
                        case 0x0976:
                        case 0x0977:
                        case 0x0978:
                        case 0x0979:
                        case 0x097A:
                        case 0x097B:
                        case 0x097C:
                        case 0x097D:
                        case 0x097E:
                        case 0x097F:
                            sb.AppendFormat("Device complies with iSCSI revision {0}", VersionDescriptor & 0x1F).AppendLine();
                            break;
                        case 0x0980:
                            sb.AppendLine("Device complies with SBP-3 (no version claimed)");
                            break;
                        case 0x0982:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 1f");
                            break;
                        case 0x0994:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 3");
                            break;
                        case 0x099A:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 4");
                            break;
                        case 0x099B:
                            sb.AppendLine("Device complies with SBP-3 T10/1467-D revision 5");
                            break;
                        case 0x099C:
                            sb.AppendLine("Device complies with SBP-3 ANSI INCITS 375-2004");
                            break;
                        case 0x09C0:
                            sb.AppendLine("Device complies with ADP (no version claimed)");
                            break;
                        case 0x09E0:
                            sb.AppendLine("Device complies with ADT (no version claimed)");
                            break;
                        case 0x09F9:
                            sb.AppendLine("Device complies with ADT T10/1557-D revision 11");
                            break;
                        case 0x09FA:
                            sb.AppendLine("Device complies with ADT T10/1557-D revision 14");
                            break;
                        case 0x09FD:
                            sb.AppendLine("Device complies with ADT ANSI INCITS 406-2005");
                            break;
                        case 0x0A00:
                            sb.AppendLine("Device complies with FCP-3 (no version claimed)");
                            break;
                        case 0x0A07:
                            sb.AppendLine("Device complies with FCP-3 T10/1560-D revision 3f");
                            break;
                        case 0x0A0F:
                            sb.AppendLine("Device complies with FCP-3 T10/1560-D revision 4");
                            break;
                        case 0x0A11:
                            sb.AppendLine("Device complies with FCP-3 ANSI INCITS 416-2006");
                            break;
                        case 0x0A1C:
                            sb.AppendLine("Device complies with FCP-3 ISO/IEC 14776-223");
                            break;
                        case 0x0A20:
                            sb.AppendLine("Device complies with ADT-2 (no version claimed)");
                            break;
                        case 0x0A22:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 06");
                            break;
                        case 0x0A27:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 08");
                            break;
                        case 0x0A28:
                            sb.AppendLine("Device complies with ADT-2 T10/1742-D revision 09");
                            break;
                        case 0x0A2B:
                            sb.AppendLine("Device complies with ADT-2 ANSI INCITS 472-2011");
                            break;
                        case 0x0A40:
                            sb.AppendLine("Device complies with FCP-4 (no version claimed)");
                            break;
                        case 0x0A42:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 01");
                            break;
                        case 0x0A44:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 02");
                            break;
                        case 0x0A45:
                            sb.AppendLine("Device complies with FCP-4 T10/1828-D revision 02b");
                            break;
                        case 0x0A46:
                            sb.AppendLine("Device complies with FCP-4 ANSI INCITS 481-2012");
                            break;
                        case 0x0A60:
                            sb.AppendLine("Device complies with ADT-3 (no version claimed)");
                            break;
                        case 0x0AA0:
                            sb.AppendLine("Device complies with SPI (no version claimed)");
                            break;
                        case 0x0AB9:
                            sb.AppendLine("Device complies with SPI T10/0855-D revision 15a");
                            break;
                        case 0x0ABA:
                            sb.AppendLine("Device complies with SPI ANSI INCITS 253-1995");
                            break;
                        case 0x0ABB:
                            sb.AppendLine("Device complies with SPI T10/0855-D revision 15a with SPI Amnd revision 3a");
                            break;
                        case 0x0ABC:
                            sb.AppendLine("Device complies with SPI ANSI INCITS 253-1995 with SPI Amnd ANSI INCITS 253/AM1-1998");
                            break;
                        case 0x0AC0:
                            sb.AppendLine("Device complies with Fast-20 (no version claimed)");
                            break;
                        case 0x0ADB:
                            sb.AppendLine("Device complies with Fast-20 T10/1071 revision 06");
                            break;
                        case 0x0ADC:
                            sb.AppendLine("Device complies with Fast-20 ANSI INCITS 277-1996");
                            break;
                        case 0x0AE0:
                            sb.AppendLine("Device complies with SPI-2 (no version claimed)");
                            break;
                        case 0x0AFB:
                            sb.AppendLine("Device complies with SPI-2 T10/1142-D revision 20b");
                            break;
                        case 0x0AFC:
                            sb.AppendLine("Device complies with SPI-2 ANSI INCITS 302-1999");
                            break;
                        case 0x0B00:
                            sb.AppendLine("Device complies with SPI-3 (no version claimed)");
                            break;
                        case 0x0B18:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 10");
                            break;
                        case 0x0B19:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 13a");
                            break;
                        case 0x0B1A:
                            sb.AppendLine("Device complies with SPI-3 T10/1302-D revision 14");
                            break;
                        case 0x0B1C:
                            sb.AppendLine("Device complies with SPI-3 ANSI INCITS 336-2000");
                            break;
                        case 0x0B20:
                            sb.AppendLine("Device complies with EPI (no version claimed)");
                            break;
                        case 0x0B3B:
                            sb.AppendLine("Device complies with EPI T10/1134 revision 16");
                            break;
                        case 0x0B3C:
                            sb.AppendLine("Device complies with EPI ANSI INCITS TR-23 1999");
                            break;
                        case 0x0B40:
                            sb.AppendLine("Device complies with SPI-4 (no version claimed)");
                            break;
                        case 0x0B54:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 7");
                            break;
                        case 0x0B55:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 9");
                            break;
                        case 0x0B56:
                            sb.AppendLine("Device complies with SPI-4 ANSI INCITS 362-2002");
                            break;
                        case 0x0B59:
                            sb.AppendLine("Device complies with SPI-4 T10/1365-D revision 10");
                            break;
                        case 0x0B60:
                            sb.AppendLine("Device complies with SPI-5 (no version claimed)");
                            break;
                        case 0x0B79:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 3");
                            break;
                        case 0x0B7A:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 5");
                            break;
                        case 0x0B7B:
                            sb.AppendLine("Device complies with SPI-5 T10/1525-D revision 6");
                            break;
                        case 0x0B7C:
                            sb.AppendLine("Device complies with SPI-5 ANSI INCITS 367-2003");
                            break;
                        case 0x0BE0:
                            sb.AppendLine("Device complies with SAS (no version claimed)");
                            break;
                        case 0x0BE1:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 01");
                            break;
                        case 0x0BF5:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 03");
                            break;
                        case 0x0BFA:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFB:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 04");
                            break;
                        case 0x0BFC:
                            sb.AppendLine("Device complies with SAS T10/1562-D revision 05");
                            break;
                        case 0x0BFD:
                            sb.AppendLine("Device complies with SAS ANSI INCITS 376-2003");
                            break;
                        case 0x0C00:
                            sb.AppendLine("Device complies with SAS-1.1 (no version claimed)");
                            break;
                        case 0x0C07:
                            sb.AppendLine("Device complies with SAS-1.1 T10/1601-D revision 9");
                            break;
                        case 0x0C0F:
                            sb.AppendLine("Device complies with SAS-1.1 T10/1601-D revision 10");
                            break;
                        case 0x0C11:
                            sb.AppendLine("Device complies with SAS-1.1 ANSI INCITS 417-2006");
                            break;
                        case 0x0C12:
                            sb.AppendLine("Device complies with SAS-1.1 ISO/IEC 14776-151");
                            break;
                        case 0x0C20:
                            sb.AppendLine("Device complies with SAS-2 (no version claimed)");
                            break;
                        case 0x0C23:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 14");
                            break;
                        case 0x0C27:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 15");
                            break;
                        case 0x0C28:
                            sb.AppendLine("Device complies with SAS-2 T10/1760-D revision 16");
                            break;
                        case 0x0C2A:
                            sb.AppendLine("Device complies with SAS-2 ANSI INCITS 457-2010");
                            break;
                        case 0x0C40:
                            sb.AppendLine("Device complies with SAS-2.1 (no version claimed)");
                            break;
                        case 0x0C48:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 04");
                            break;
                        case 0x0C4A:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 06");
                            break;
                        case 0x0C4B:
                            sb.AppendLine("Device complies with SAS-2.1 T10/2125-D revision 07");
                            break;
                        case 0x0C4E:
                            sb.AppendLine("Device complies with SAS-2.1 ANSI INCITS 478-2011");
                            break;
                        case 0x0C4F:
                            sb.AppendLine("Device complies with SAS-2.1 ANSI INCITS 478-2011 w/ Amnd 1 ANSI INCITS 478/AM1-2014");
                            break;
                        case 0x0C52:
                            sb.AppendLine("Device complies with SAS-2.1 ISO/IEC 14776-153");
                            break;
                        case 0x0C60:
                            sb.AppendLine("Device complies with SAS-3 (no version claimed)");
                            break;
                        case 0x0C63:
                            sb.AppendLine("Device complies with SAS-3 T10/BSR INCITS 519 revision 05a");
                            break;
                        case 0x0C65:
                            sb.AppendLine("Device complies with SAS-3 T10/BSR INCITS 519 revision 06");
                            break;
                        case 0x0C68:
                            sb.AppendLine("Device complies with SAS-3 ANSI INCITS 519-2014");
                            break;
                        case 0x0C80:
                            sb.AppendLine("Device complies with SAS-4 (no version claimed)");
                            break;
                        case 0x0D20:
                            sb.AppendLine("Device complies with FC-PH (no version claimed)");
                            break;
                        case 0x0D3B:
                            sb.AppendLine("Device complies with FC-PH ANSI INCITS 230-1994");
                            break;
                        case 0x0D3C:
                            sb.AppendLine("Device complies with FC-PH ANSI INCITS 230-1994 with Amnd 1 ANSI INCITS 230/AM1-1996");
                            break;
                        case 0x0D40:
                            sb.AppendLine("Device complies with FC-AL (no version claimed)");
                            break;
                        case 0x0D5C:
                            sb.AppendLine("Device complies with FC-AL ANSI INCITS 272-1996");
                            break;
                        case 0x0D60:
                            sb.AppendLine("Device complies with FC-AL-2 (no version claimed)");
                            break;
                        case 0x0D61:
                            sb.AppendLine("Device complies with FC-AL-2 T11/1133-D revision 7.0");
                            break;
                        case 0x0D63:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with AM1-2003 & AM2-2006");
                            break;
                        case 0x0D64:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 2 AM2-2006");
                            break;
                        case 0x0D65:
                            sb.AppendLine("Device complies with FC-AL-2 ISO/IEC 14165-122 with AM1 & AM2");
                            break;
                        case 0x0D7C:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999");
                            break;
                        case 0x0D7D:
                            sb.AppendLine("Device complies with FC-AL-2 ANSI INCITS 332-1999 with Amnd 1 AM1-2003");
                            break;
                        case 0x0D80:
                            sb.AppendLine("Device complies with FC-PH-3 (no version claimed)");
                            break;
                        case 0x0D9C:
                            sb.AppendLine("Device complies with FC-PH-3 ANSI INCITS 303-1998");
                            break;
                        case 0x0DA0:
                            sb.AppendLine("Device complies with FC-FS (no version claimed)");
                            break;
                        case 0x0DB7:
                            sb.AppendLine("Device complies with FC-FS T11/1331-D revision 1.2");
                            break;
                        case 0x0DB8:
                            sb.AppendLine("Device complies with FC-FS T11/1331-D revision 1.7");
                            break;
                        case 0x0DBC:
                            sb.AppendLine("Device complies with FC-FS ANSI INCITS 373-2003");
                            break;
                        case 0x0DBD:
                            sb.AppendLine("Device complies with FC-FS ISO/IEC 14165-251");
                            break;
                        case 0x0DC0:
                            sb.AppendLine("Device complies with FC-PI (no version claimed)");
                            break;
                        case 0x0DDC:
                            sb.AppendLine("Device complies with FC-PI ANSI INCITS 352-2002");
                            break;
                        case 0x0DE0:
                            sb.AppendLine("Device complies with FC-PI-2 (no version claimed)");
                            break;
                        case 0x0DE2:
                            sb.AppendLine("Device complies with FC-PI-2 T11/1506-D revision 5.0");
                            break;
                        case 0x0DE4:
                            sb.AppendLine("Device complies with FC-PI-2 ANSI INCITS 404-2006");
                            break;
                        case 0x0E00:
                            sb.AppendLine("Device complies with FC-FS-2 (no version claimed)");
                            break;
                        case 0x0E02:
                            sb.AppendLine("Device complies with FC-FS-2 ANSI INCITS 242-2007");
                            break;
                        case 0x0E03:
                            sb.AppendLine("Device complies with FC-FS-2 ANSI INCITS 242-2007 with AM1 ANSI INCITS 242/AM1-2007");
                            break;
                        case 0x0E20:
                            sb.AppendLine("Device complies with FC-LS (no version claimed)");
                            break;
                        case 0x0E21:
                            sb.AppendLine("Device complies with FC-LS T11/1620-D revision 1.62");
                            break;
                        case 0x0E29:
                            sb.AppendLine("Device complies with FC-LS ANSI INCITS 433-2007");
                            break;
                        case 0x0E40:
                            sb.AppendLine("Device complies with FC-SP (no version claimed)");
                            break;
                        case 0x0E42:
                            sb.AppendLine("Device complies with FC-SP T11/1570-D revision 1.6");
                            break;
                        case 0x0E45:
                            sb.AppendLine("Device complies with FC-SP ANSI INCITS 426-2007");
                            break;
                        case 0x0E60:
                            sb.AppendLine("Device complies with FC-PI-3 (no version claimed)");
                            break;
                        case 0x0E62:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 2.0");
                            break;
                        case 0x0E68:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 2.1");
                            break;
                        case 0x0E6A:
                            sb.AppendLine("Device complies with FC-PI-3 T11/1625-D revision 4.0");
                            break;
                        case 0x0E6E:
                            sb.AppendLine("Device complies with FC-PI-3 ANSI INCITS 460-2011");
                            break;
                        case 0x0E80:
                            sb.AppendLine("Device complies with FC-PI-4 (no version claimed)");
                            break;
                        case 0x0E82:
                            sb.AppendLine("Device complies with FC-PI-4 T11/1647-D revision 8.0");
                            break;
                        case 0x0E88:
                            sb.AppendLine("Device complies with FC-PI-4 ANSI INCITS 450-2009");
                            break;
                        case 0x0EA0:
                            sb.AppendLine("Device complies with FC 10GFC (no version claimed)");
                            break;
                        case 0x0EA2:
                            sb.AppendLine("Device complies with FC 10GFC ANSI INCITS 364-2003");
                            break;
                        case 0x0EA3:
                            sb.AppendLine("Device complies with FC 10GFC ISO/IEC 14165-116");
                            break;
                        case 0x0EA5:
                            sb.AppendLine("Device complies with FC 10GFC ISO/IEC 14165-116 with AM1");
                            break;
                        case 0x0EA6:
                            sb.AppendLine("Device complies with FC 10GFC ANSI INCITS 364-2003 with AM1 ANSI INCITS 364/AM1-2007");
                            break;
                        case 0x0EC0:
                            sb.AppendLine("Device complies with FC-SP-2 (no version claimed)");
                            break;
                        case 0x0EE0:
                            sb.AppendLine("Device complies with FC-FS-3 (no version claimed)");
                            break;
                        case 0x0EE2:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 0.9");
                            break;
                        case 0x0EE7:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 1.0");
                            break;
                        case 0x0EE9:
                            sb.AppendLine("Device complies with FC-FS-3 T11/1861-D revision 1.10");
                            break;
                        case 0x0EEB:
                            sb.AppendLine("Device complies with FC-FS-3 ANSI INCITS 470-2011");
                            break;
                        case 0x0F00:
                            sb.AppendLine("Device complies with FC-LS-2 (no version claimed)");
                            break;
                        case 0x0F03:
                            sb.AppendLine("Device complies with FC-LS-2 T11/2103-D revision 2.11");
                            break;
                        case 0x0F05:
                            sb.AppendLine("Device complies with FC-LS-2 T11/2103-D revision 2.21");
                            break;
                        case 0x0F07:
                            sb.AppendLine("Device complies with FC-LS-2 ANSI INCITS 477-2011");
                            break;
                        case 0x0F20:
                            sb.AppendLine("Device complies with FC-PI-5 (no version claimed)");
                            break;
                        case 0x0F27:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 2.00");
                            break;
                        case 0x0F28:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 3.00");
                            break;
                        case 0x0F2A:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 6.00");
                            break;
                        case 0x0F2B:
                            sb.AppendLine("Device complies with FC-PI-5 T11/2118-D revision 6.10");
                            break;
                        case 0x0F2E:
                            sb.AppendLine("Device complies with FC-PI-5 ANSI INCITS 479-2011");
                            break;
                        case 0x0F40:
                            sb.AppendLine("Device complies with FC-PI-6 (no version claimed)");
                            break;
                        case 0x0F60:
                            sb.AppendLine("Device complies with FC-FS-4 (no version claimed)");
                            break;
                        case 0x0F80:
                            sb.AppendLine("Device complies with FC-LS-3 (no version claimed)");
                            break;
                        case 0x12A0:
                            sb.AppendLine("Device complies with FC-SCM (no version claimed)");
                            break;
                        case 0x12A3:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.0");
                            break;
                        case 0x12A5:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.1");
                            break;
                        case 0x12A7:
                            sb.AppendLine("Device complies with FC-SCM T11/1824DT revision 1.4");
                            break;
                        case 0x12AA:
                            sb.AppendLine("Device complies with FC-SCM INCITS TR-47 2012");
                            break;
                        case 0x12C0:
                            sb.AppendLine("Device complies with FC-DA-2 (no version claimed)");
                            break;
                        case 0x12C3:
                            sb.AppendLine("Device complies with FC-DA-2 T11/1870DT revision 1.04");
                            break;
                        case 0x12C5:
                            sb.AppendLine("Device complies with FC-DA-2 T11/1870DT revision 1.06");
                            break;
                        case 0x12C9:
                            sb.AppendLine("Device complies with FC-DA-2 INCITS TR-49 2012");
                            break;
                        case 0x12E0:
                            sb.AppendLine("Device complies with FC-DA (no version claimed)");
                            break;
                        case 0x12E2:
                            sb.AppendLine("Device complies with FC-DA T11/1513-DT revision 3.1");
                            break;
                        case 0x12E8:
                            sb.AppendLine("Device complies with FC-DA ANSI INCITS TR-36 2004");
                            break;
                        case 0x12E9:
                            sb.AppendLine("Device complies with FC-DA ISO/IEC 14165-341");
                            break;
                        case 0x1300:
                            sb.AppendLine("Device complies with FC-Tape (no version claimed)");
                            break;
                        case 0x1301:
                            sb.AppendLine("Device complies with FC-Tape T11/1315 revision 1.16");
                            break;
                        case 0x131B:
                            sb.AppendLine("Device complies with FC-Tape T11/1315 revision 1.17");
                            break;
                        case 0x131C:
                            sb.AppendLine("Device complies with FC-Tape ANSI INCITS TR-24 1999");
                            break;
                        case 0x1320:
                            sb.AppendLine("Device complies with FC-FLA (no version claimed)");
                            break;
                        case 0x133B:
                            sb.AppendLine("Device complies with FC-FLA T11/1235 revision 7");
                            break;
                        case 0x133C:
                            sb.AppendLine("Device complies with FC-FLA ANSI INCITS TR-20 1998");
                            break;
                        case 0x1340:
                            sb.AppendLine("Device complies with FC-PLDA (no version claimed)");
                            break;
                        case 0x135B:
                            sb.AppendLine("Device complies with FC-PLDA T11/1162 revision 2.1");
                            break;
                        case 0x135C:
                            sb.AppendLine("Device complies with FC-PLDA ANSI INCITS TR-19 1998");
                            break;
                        case 0x1360:
                            sb.AppendLine("Device complies with SSA-PH2 (no version claimed)");
                            break;
                        case 0x137B:
                            sb.AppendLine("Device complies with SSA-PH2 T10.1/1145-D revision 09c");
                            break;
                        case 0x137C:
                            sb.AppendLine("Device complies with SSA-PH2 ANSI INCITS 293-1996");
                            break;
                        case 0x1380:
                            sb.AppendLine("Device complies with SSA-PH3 (no version claimed)");
                            break;
                        case 0x139B:
                            sb.AppendLine("Device complies with SSA-PH3 T10.1/1146-D revision 05b");
                            break;
                        case 0x139C:
                            sb.AppendLine("Device complies with SSA-PH3 ANSI INCITS 307-1998");
                            break;
                        case 0x14A0:
                            sb.AppendLine("Device complies with IEEE 1394 (no version claimed)");
                            break;
                        case 0x14BD:
                            sb.AppendLine("Device complies with ANSI IEEE 1394-1995");
                            break;
                        case 0x14C0:
                            sb.AppendLine("Device complies with IEEE 1394a (no version claimed)");
                            break;
                        case 0x14E0:
                            sb.AppendLine("Device complies with IEEE 1394b (no version claimed)");
                            break;
                        case 0x15E0:
                            sb.AppendLine("Device complies with ATA/ATAPI-6 (no version claimed)");
                            break;
                        case 0x15FD:
                            sb.AppendLine("Device complies with ATA/ATAPI-6 ANSI INCITS 361-2002");
                            break;
                        case 0x1600:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 (no version claimed)");
                            break;
                        case 0x1602:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 T13/1532-D revision 3");
                            break;
                        case 0x161C:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 ANSI INCITS 397-2005");
                            break;
                        case 0x161E:
                            sb.AppendLine("Device complies with ATA/ATAPI-7 ISO/IEC 24739");
                            break;
                        case 0x1620:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AAM (no version claimed)");
                            break;
                        case 0x1621:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-APT Parallel Transport (no version claimed)");
                            break;
                        case 0x1622:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AST Serial Transport (no version claimed)");
                            break;
                        case 0x1623:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-ACS ATA/ATAPI Command Set (no version claimed)");
                            break;
                        case 0x1628:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-AAM ANSI INCITS 451-2008");
                            break;
                        case 0x162A:
                            sb.AppendLine("Device complies with ATA/ATAPI-8 ATA8-ACS ANSI INCITS 452-2009 w/ Amendment 1");
                            break;
                        case 0x1728:
                            sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 1.1");
                            break;
                        case 0x1729:
                            sb.AppendLine("Device complies with Universal Serial Bus Specification, Revision 2.0");
                            break;
                        case 0x1730:
                            sb.AppendLine("Device complies with USB Mass Storage Class Bulk-Only Transport, Revision 1.0");
                            break;
                        case 0x1740:
                            sb.AppendLine("Device complies with UAS (no version claimed)");
                            break;
                        case 0x1743:
                            sb.AppendLine("Device complies with UAS T10/2095-D revision 02");
                            break;
                        case 0x1747:
                            sb.AppendLine("Device complies with UAS T10/2095-D revision 04");
                            break;
                        case 0x1748:
                            sb.AppendLine("Device complies with UAS ANSI INCITS 471-2010");
                            break;
                        case 0x1749:
                            sb.AppendLine("Device complies with UAS ISO/IEC 14776-251:2014");
                            break;
                        case 0x1761:
                            sb.AppendLine("Device complies with ACS-2 (no version claimed)");
                            break;
                        case 0x1762:
                            sb.AppendLine("Device complies with ACS-2 ANSI INCITS 482-2013");
                            break;
                        case 0x1765:
                            sb.AppendLine("Device complies with ACS-3 (no version claimed)");
                            break;
                        case 0x1780:
                            sb.AppendLine("Device complies with UAS-2 (no version claimed)");
                            break;
                        case 0x1EA0:
                            sb.AppendLine("Device complies with SAT (no version claimed)");
                            break;
                        case 0x1EA7:
                            sb.AppendLine("Device complies with SAT T10/1711-D revision 8");
                            break;
                        case 0x1EAB:
                            sb.AppendLine("Device complies with SAT T10/1711-D revision 9");
                            break;
                        case 0x1EAD:
                            sb.AppendLine("Device complies with SAT ANSI INCITS 431-2007");
                            break;
                        case 0x1EC0:
                            sb.AppendLine("Device complies with SAT-2 (no version claimed)");
                            break;
                        case 0x1EC4:
                            sb.AppendLine("Device complies with SAT-2 T10/1826-D revision 06");
                            break;
                        case 0x1EC8:
                            sb.AppendLine("Device complies with SAT-2 T10/1826-D revision 09");
                            break;
                        case 0x1ECA:
                            sb.AppendLine("Device complies with SAT-2 ANSI INCITS 465-2010");
                            break;
                        case 0x1EE0:
                            sb.AppendLine("Device complies with SAT-3 (no version claimed)");
                            break;
                        case 0x1EE2:
                            sb.AppendLine("Device complies with SAT-3 T10/BSR INCITS 517 revision 4");
                            break;
                        case 0x1EE4:
                            sb.AppendLine("Device complies with SAT-3 T10/BSR INCITS 517 revision 7");
                            break;
                        case 0x1EE8:
                            sb.AppendLine("Device complies with SAT-3 ANSI INCITS 517-2015");
                            break;
                        case 0x1F00:
                            sb.AppendLine("Device complies with SAT-4 (no version claimed)");
                            break;
                        case 0x20A0:
                            sb.AppendLine("Device complies with SPL (no version claimed)");
                            break;
                        case 0x20A3:
                            sb.AppendLine("Device complies with SPL T10/2124-D revision 6a");
                            break;
                        case 0x20A5:
                            sb.AppendLine("Device complies with SPL T10/2124-D revision 7");
                            break;
                        case 0x20A7:
                            sb.AppendLine("Device complies with SPL ANSI INCITS 476-2011");
                            break;
                        case 0x20A8:
                            sb.AppendLine("Device complies with SPL ANSI INCITS 476-2011 + SPL AM1 INCITS 476/AM1 2012");
                            break;
                        case 0x20AA:
                            sb.AppendLine("Device complies with SPL ISO/IEC 14776-261:2012");
                            break;
                        case 0x20C0:
                            sb.AppendLine("Device complies with SPL-2 (no version claimed)");
                            break;
                        case 0x20C2:
                            sb.AppendLine("Device complies with SPL-2 T10/BSR INCITS 505 revision 4");
                            break;
                        case 0x20C4:
                            sb.AppendLine("Device complies with SPL-2 T10/BSR INCITS 505 revision 5");
                            break;
                        case 0x20C8:
                            sb.AppendLine("Device complies with SPL-2 ANSI INCITS 505-2013");
                            break;
                        case 0x20E0:
                            sb.AppendLine("Device complies with SPL-3 (no version claimed)");
                            break;
                        case 0x20E4:
                            sb.AppendLine("Device complies with SPL-3 T10/BSR INCITS 492 revision 6");
                            break;
                        case 0x20E6:
                            sb.AppendLine("Device complies with SPL-3 T10/BSR INCITS 492 revision 7");
                            break;
                        case 0x20E8:
                            sb.AppendLine("Device complies with SPL-3 ANSI INCITS 492-2015");
                            break;
                        case 0x2100:
                            sb.AppendLine("Device complies with SPL-4 (no version claimed)");
                            break;
                        case 0x21E0:
                            sb.AppendLine("Device complies with SOP (no version claimed)");
                            break;
                        case 0x21E4:
                            sb.AppendLine("Device complies with SOP T10/BSR INCITS 489 revision 4");
                            break;
                        case 0x21E6:
                            sb.AppendLine("Device complies with SOP T10/BSR INCITS 489 revision 5");
                            break;
                        case 0x21E8:
                            sb.AppendLine("Device complies with SOP ANSI INCITS 489-2014");
                            break;
                        case 0x2200:
                            sb.AppendLine("Device complies with PQI (no version claimed)");
                            break;
                        case 0x2204:
                            sb.AppendLine("Device complies with PQI T10/BSR INCITS 490 revision 6");
                            break;
                        case 0x2206:
                            sb.AppendLine("Device complies with PQI T10/BSR INCITS 490 revision 7");
                            break;
                        case 0x2208:
                            sb.AppendLine("Device complies with PQI ANSI INCITS 490-2014");
                            break;
                        case 0x2220:
                            sb.AppendLine("Device complies with SOP-2 (no version claimed)");
                            break;
                        case 0x2240:
                            sb.AppendLine("Device complies with PQI-2 (no version claimed)");
                            break;
                        case 0xFFC0:
                            sb.AppendLine("Device complies with IEEE 1667 (no version claimed)");
                            break;
                        case 0xFFC1:
                            sb.AppendLine("Device complies with IEEE 1667-2006");
                            break;
                        case 0xFFC2:
                            sb.AppendLine("Device complies with IEEE 1667-2009");
                            break;
                        default:
                            sb.AppendFormat("Device complies with unknown standard code 0x{0:X4}", VersionDescriptor).AppendLine();
                            break;
                    }
                }
            }

            #if DEBUG
            if(response.DeviceTypeModifier != 0)
                sb.AppendFormat("Vendor's device type modifier = 0x{0:X2}", response.DeviceTypeModifier).AppendLine();
            if(response.Reserved2 != 0)
                sb.AppendFormat("Reserved byte 5, bits 2 to 1 = 0x{0:X2}", response.Reserved2).AppendLine();
            if(response.Reserved3 != 0)
                sb.AppendFormat("Reserved byte 56, bits 7 to 4 = 0x{0:X2}", response.Reserved3).AppendLine();
            if(response.Reserved4 != 0)
                sb.AppendFormat("Reserved byte 57 = 0x{0:X2}", response.Reserved4).AppendLine();

            if (response.Reserved5 != null)
            {
                sb.AppendLine("Reserved bytes 74 to 95");
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.Reserved5, 60));
                sb.AppendLine("============================================================");
            }

            if (response.VendorSpecific != null)
            {
                sb.AppendLine("Vendor-specific bytes 36 to 55");
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific, 60));
                sb.AppendLine("============================================================");
            }

            if (response.VendorSpecific2 != null)
            {
                sb.AppendFormat("Vendor-specific bytes 96 to {0}", response.AdditionalLength+4).AppendLine();
                sb.AppendLine("============================================================");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VendorSpecific2, 60));
                sb.AppendLine("============================================================");
            }
            #endif

            return sb.ToString();
        }

        public static string PrettifySCSIInquiry(byte[] SCSIInquiryResponse)
        {
            SCSIInquiry? decoded = DecodeSCSIInquiry(SCSIInquiryResponse);
            return PrettifySCSIInquiry(decoded);
        }

        #endregion Public methods

        #region Public structures

        // SCSI INQUIRY command response
        public struct SCSIInquiry
        {
            /// <summary>
            /// Peripheral qualifier
            /// Byte 0, bits 7 to 5
            /// </summary>
            public byte PeripheralQualifier;
            /// <summary>
            /// Peripheral device type
            /// Byte 0, bits 4 to 0
            /// </summary>
            public byte PeripheralDeviceType;
            /// <summary>
            /// Removable device
            /// Byte 1, bit 7
            /// </summary>
            public bool RMB;
            /// <summary>
            /// SCSI-1 vendor-specific qualification codes
            /// Byte 1, bits 6 to 0
            /// </summary>
            public byte DeviceTypeModifier;
            /// <summary>
            /// ISO/IEC SCSI Standard Version
            /// Byte 2, bits 7 to 6, mask = 0xC0, >> 6
            /// </summary>
            public byte ISOVersion;
            /// <summary>
            /// ECMA SCSI Standard Version
            /// Byte 2, bits 5 to 3, mask = 0x38, >> 3
            /// </summary>
            public byte ECMAVersion;
            /// <summary>
            /// ANSI SCSI Standard Version
            /// Byte 2, bits 2 to 0, mask = 0x07
            /// </summary>
            public byte ANSIVersion;
            /// <summary>
            /// Asynchronous Event Reporting Capability supported
            /// Byte 3, bit 7
            /// </summary>
            public bool AERC;
            /// <summary>
            /// Device supports TERMINATE TASK command
            /// Byte 3, bit 6
            /// </summary>
            public bool TrmTsk;
            /// <summary>
            /// Supports setting Normal ACA
            /// Byte 3, bit 5
            /// </summary>
            public bool NormACA;
            /// <summary>
            /// Supports LUN hierarchical addressing
            /// Byte 3, bit 4
            /// </summary>
            public bool HiSup;
            /// <summary>
            /// Responde data format
            /// Byte 3, bit 3 to 0
            /// </summary>
            public byte ResponseDataFormat;
            /// <summary>
            /// Lenght of total INQUIRY response minus 4
            /// Byte 4
            /// </summary>
            public byte AdditionalLength;
            /// <summary>
            /// Device contains an embedded storage array controller
            /// Byte 5, bit 7
            /// </summary>
            public bool SCCS;
            /// <summary>
            /// Device contains an Access Control Coordinator
            /// Byte 5, bit 6
            /// </summary>
            public bool ACC;
            /// <summary>
            /// Supports asymetrical logical unit access
            /// Byte 5, bits 5 to 4
            /// </summary>
            public byte TPGS;
            /// <summary>
            /// Supports third-party copy commands
            /// Byte 5, bit 3
            /// </summary>
            public bool ThreePC;
            /// <summary>
            /// Reserved
            /// Byte 5, bits 2 to 1
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Supports protection information
            /// Byte 5, bit 0
            /// </summary>
            public bool Protect;
            /// <summary>
            /// Supports basic queueing
            /// Byte 6, bit 7
            /// </summary>
            public bool BQue;
            /// <summary>
            /// Device contains an embedded enclosure services component
            /// Byte 6, bit 6
            /// </summary>
            public bool EncServ;
            /// <summary>
            /// Vendor-specific
            /// Byte 6, bit 5
            /// </summary>
            public bool VS1;
            /// <summary>
            /// Multi-port device
            /// Byte 6, bit 4
            /// </summary>
            public bool MultiP;
            /// <summary>
            /// Device contains or is attached to a medium changer
            /// Byte 6, bit 3
            /// </summary>
            public bool MChngr;
            /// <summary>
            /// Device supports request and acknowledge handshakes
            /// Byte 6, bit 2
            /// </summary>
            public bool ACKREQQ;
            /// <summary>
            /// Supports 32-bit wide SCSI addresses
            /// Byte 6, bit 1
            /// </summary>
            public bool Addr32;
            /// <summary>
            /// Supports 16-bit wide SCSI addresses
            /// Byte 6, bit 0
            /// </summary>
            public bool Addr16;
            /// <summary>
            /// Device supports relative addressing
            /// Byte 7, bit 7
            /// </summary>
            public bool RelAddr;
            /// <summary>
            /// Supports 32-bit wide data transfers
            /// Byte 7, bit 6
            /// </summary>
            public bool WBus32;
            /// <summary>
            /// Supports 16-bit wide data transfers
            /// Byte 7, bit 5
            /// </summary>
            public bool WBus16;
            /// <summary>
            /// Supports synchronous data transfer
            /// Byte 7, bit 4
            /// </summary>
            public bool Sync;
            /// <summary>
            /// Supports linked commands
            /// Byte 7, bit 3
            /// </summary>
            public bool Linked;
            /// <summary>
            /// Supports CONTINUE TASK and TARGET TRANSFER DISABLE commands
            /// Byte 7, bit 2
            /// </summary>
            public bool TranDis;
            /// <summary>
            /// Supports TCQ queue
            /// Byte 7, bit 1
            /// </summary>
            public bool CmdQue;
            /// <summary>
            /// Indicates that the devices responds to RESET with soft reset
            /// Byte 7, bit 0
            /// </summary>
            public bool SftRe;
            /// <summary>
            /// Vendor identification
            /// Bytes 8 to 15
            /// </summary>
            public byte[] VendorIdentification;
            /// <summary>
            /// Product identification
            /// Bytes 16 to 31
            /// </summary>
            public byte[] ProductIdentification;
            /// <summary>
            /// Product revision level
            /// Bytes 32 to 35
            /// </summary>
            public byte[] ProductRevisionLevel;
            /// <summary>
            /// Vendor-specific data
            /// Bytes 36 to 55
            /// </summary>
            public byte[] VendorSpecific;
            /// <summary>
            /// Byte 56, bits 7 to 4
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Supported SPI clocking
            /// Byte 56, bits 3 to 2
            /// </summary>
            public byte Clocking;
            /// <summary>
            /// Device supports Quick Arbitration and Selection
            /// Byte 56, bit 1
            /// </summary>
            public bool QAS;
            /// <summary>
            /// Supports information unit transfers
            /// Byte 56, bit 0
            /// </summary>
            public bool IUS;
            /// <summary>
            /// Reserved
            /// Byte 57
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Array of version descriptors
            /// Bytes 58 to 73
            /// </summary>
            public UInt16[] VersionDescriptors;
            /// <summary>
            /// Reserved
            /// Bytes 74 to 95
            /// </summary>
            public byte[] Reserved5;
            /// <summary>
            /// Reserved
            /// Bytes 96 to end
            /// </summary>
            public byte[] VendorSpecific2;
        }

        #endregion Public structures
    }
}

