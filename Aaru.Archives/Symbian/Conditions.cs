// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Symbian.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Symbian plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies Symbian installer (.sis) packages and shows information.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Text;

namespace Aaru.Archives;

public sealed partial class Symbian
{
    ConditionalExpression ParseConditionalExpression(BinaryReader   br, uint maxOffset, StringBuilder sb,
                                                     ref Attribute? attribute)
    {
        if(br.BaseStream.Position >= maxOffset)
            return null;

        var type           = (ConditionalType)br.ReadUInt32();
        var operatorString = "";

        SubConditionalExpression     subExpression;
        TwoSubsConditionalExpression twoSubsConditionalExpression;
        switch(type)
        {
            case ConditionalType.Equals:
                if(type == ConditionalType.Equals)
                    operatorString = " == ";

                twoSubsConditionalExpression = new TwoSubsConditionalExpression
                {
                    type = type
                };

                sb.Append("(");
                twoSubsConditionalExpression.leftOperand = ParseConditionalExpression(br, maxOffset, sb, ref attribute);
                sb.Append(operatorString);
                twoSubsConditionalExpression.rightOperand =
                    ParseConditionalExpression(br, maxOffset, sb, ref attribute);
                sb.Append(")");

                attribute = null;

                return twoSubsConditionalExpression;
            case ConditionalType.Differs:
                operatorString = " != ";
                goto case ConditionalType.Equals;
            case ConditionalType.GreaterThan:
                operatorString = " > ";
                goto case ConditionalType.Equals;
            case ConditionalType.LessThan:
                operatorString = " < ";
                goto case ConditionalType.Equals;
            case ConditionalType.GreaterOrEqualThan:
                operatorString = " >= ";
                goto case ConditionalType.Equals;
            case ConditionalType.LessOrEqualThan:
                operatorString = " <= ";
                goto case ConditionalType.Equals;
            case ConditionalType.And:
                operatorString = " && ";
                goto case ConditionalType.Equals;
            case ConditionalType.Or:
                operatorString = " || ";
                goto case ConditionalType.Equals;
            case ConditionalType.Exists:
                sb.Append("exists(");
                subExpression = new SubConditionalExpression
                {
                    type          = type,
                    subExpression = ParseConditionalExpression(br, maxOffset, sb, ref attribute)
                };
                sb.Append(")");

                attribute = null;

                return subExpression;
            case ConditionalType.DeviceCapability:
                sb.Append("devcap(");
                subExpression = new SubConditionalExpression
                {
                    type          = type,
                    subExpression = ParseConditionalExpression(br, maxOffset, sb, ref attribute)
                };
                sb.Append(')');

                attribute = null;

                return subExpression;
            case ConditionalType.ApplicationCapability:
                twoSubsConditionalExpression = new TwoSubsConditionalExpression
                {
                    type = type
                };

                sb.Append("appcap(");
                twoSubsConditionalExpression.leftOperand = ParseConditionalExpression(br, maxOffset, sb, ref attribute);
                sb.Append(", ");
                twoSubsConditionalExpression.rightOperand =
                    ParseConditionalExpression(br, maxOffset, sb, ref attribute);
                sb.Append(')');

                attribute = null;

                return twoSubsConditionalExpression;
            case ConditionalType.Not:
                sb.Append('!');
                subExpression = new SubConditionalExpression
                {
                    type          = type,
                    subExpression = ParseConditionalExpression(br, maxOffset, sb, ref attribute)
                };

                attribute = null;

                return subExpression;
            case ConditionalType.String:
                var stringExpression = new StringConditionalExpression
                {
                    type    = type,
                    length  = br.ReadUInt32(),
                    pointer = br.ReadUInt32()
                };

                long position = br.BaseStream.Position;

                br.BaseStream.Seek(stringExpression.pointer, SeekOrigin.Begin);
                byte[] buffer = br.ReadBytes((int)stringExpression.length);
                stringExpression.@string = _encoding.GetString(buffer);

                br.BaseStream.Seek(position, SeekOrigin.Begin);

                sb.Append($"\"{stringExpression.@string}\"");

                attribute = null;

                break;
            case ConditionalType.Attribute:
                var attributeExpression = new AttributeConditionalExpression
                {
                    type      = type,
                    attribute = (Attribute)br.ReadUInt32(),
                    unused    = br.ReadUInt32()
                };

                if((int)attributeExpression.attribute > 0x2000)
                    sb.Append($"option({attributeExpression.attribute - 0x2000}, ENABLED)");
                else
                    sb.Append($"{attributeExpression.attribute}");

                attribute = attributeExpression.attribute;

                return attributeExpression;
            case ConditionalType.Number:
                var numberExpression = new NumberConditionalExpression
                {
                    type   = type,
                    number = br.ReadUInt32(),
                    unused = br.ReadUInt32()
                };

                if(attribute is null)
                    sb.Append($"0x{numberExpression.number:X8}");
                else if((uint)attribute.Value < 0x2000)
                {
                    switch(attribute)
                    {
                        case Attribute.Manufacturer:
                            sb.Append($"{(ManufacturerCode)numberExpression.number}");
                            break;
                        case Attribute.ManufacturerHardwareRev:
                        case Attribute.ManufacturerSoftwareRev:
                        case Attribute.DeviceFamilyRev:
                            sb.Append($"{numberExpression.number >> 8}.{numberExpression.number & 0xFF}");
                            break;
                        case Attribute.MachineUid:
                            sb.Append($"{DecodeMachineUid(numberExpression.number)}");

                            break;
                        case Attribute.DeviceFamily:
                            sb.Append($"{(DeviceFamilyCode)numberExpression.number}");
                            break;
                        case Attribute.CPU:
                            sb.Append($"{(CpuCode)numberExpression.number}");
                            break;
                        case Attribute.CPUArch:
                            sb.Append($"{(CpuArchitecture)numberExpression.number}");
                            break;
                        case Attribute.CPUABI:
                            sb.Append($"{(CpuAbiCode)numberExpression.number}");
                            break;
                        case Attribute.CPUSpeed:
                            sb.Append($"{numberExpression.number / 1024}MHz");
                            break;
                        case Attribute.SystemTickPeriod:
                            sb.Append($"{numberExpression.number}μs");

                            break;
                        case Attribute.MemoryRAM:
                        case Attribute.MemoryRAMFree:
                        case Attribute.MemoryROM:
                        case Attribute.MemoryPageSize:
                        case Attribute.Keyboard:
                        case Attribute.KeyboardDeviceKeys:
                        case Attribute.KeyboardAppKeys:
                        case Attribute.KeyboardClickVolumeMax:
                        case Attribute.DisplayXPixels:
                        case Attribute.DisplayYPixels:
                        case Attribute.DisplayXTwips:
                        case Attribute.DisplayYTwips:
                        case Attribute.DisplayColors:
                        case Attribute.DisplayContrastMax:
                        case Attribute.PenX:
                        case Attribute.PenY:
                        case Attribute.PenClickVolumeMax:
                        case Attribute.MouseX:
                        case Attribute.MouseY:
                        case Attribute.MouseButtons:
                        case Attribute.LEDs:
                        case Attribute.DisplayBrightnessMax:
                        case Attribute.KeyboardBacklightState:
                        case Attribute.AccessoryPower:
                        case Attribute.NumHalAttributes:
                        case Attribute.Language:
                            sb.Append($"{numberExpression.number}");
                            break;
                        case Attribute.PowerBackup:
                        case Attribute.KeyboardClick:
                        case Attribute.Backlight:
                        case Attribute.Pen:
                        case Attribute.PenDisplayOn:
                        case Attribute.PenClick:
                        case Attribute.Mouse:
                        case Attribute.CaseSwitch:
                        case Attribute.IntegratedPhone:
                        case Attribute.RemoteInstall:
                            sb.Append(numberExpression.number == 0 ? "false" : "true");
                            break;
                        default:
                            sb.Append($"0x{numberExpression.number:X8}");
                            break;
                    }
                }
                else
                    sb.Append($"option({attribute.Value - 0x2000}, {(numberExpression.number > 0 ? "ENABLED" : "DISABLED")})");

                attribute = null;

                return numberExpression;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }
}