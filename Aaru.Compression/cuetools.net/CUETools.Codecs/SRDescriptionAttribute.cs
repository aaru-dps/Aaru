using System;
using System.ComponentModel;
using System.Reflection;

namespace CUETools.Codecs;

/// <summary>
///     Localized description attribute
/// </summary>
[AttributeUsage(AttributeTargets.All)]
public class SRDescriptionAttribute : DescriptionAttribute
{
    /// <summary>
    ///     Resource manager to use;
    /// </summary>
    readonly Type SR;
    /// <summary>
    ///     Store a flag indicating whether this has been localized
    /// </summary>
    bool localized;

    /// <summary>
    ///     Construct the description attribute
    /// </summary>
    /// <param name="text"></param>
    public SRDescriptionAttribute(Type SR, string text) : base(text)
    {
        localized = false;
        this.SR   = SR;
    }

    /// <summary>
    ///     Override the return of the description text to localize the text
    /// </summary>
    public override string Description
    {
        get
        {
            if(!localized)
            {
                localized = true;

                DescriptionValue = SR.InvokeMember(DescriptionValue,
                                                   BindingFlags.GetProperty |
                                                   BindingFlags.Static      |
                                                   BindingFlags.Public      |
                                                   BindingFlags.NonPublic,
                                                   null,
                                                   null,
                                                   []) as string;
            }

            return base.Description;
        }
    }
}