﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CheckYourEligibility.TestBase.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CheckYourEligibility.TestBase.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number
        ///tom,SIMPSON,01/01/1990,AB123456C,
        ///dave,smith,01/01/1990,AB123456D,
        ///.
        /// </summary>
        internal static string bulkchecktemplate_small_Valid {
            get {
                return ResourceManager.GetString("bulkchecktemplate_small_Valid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Parent First Name,Parent Last Name,Parent Date of Birth,Parent National Insurance number,Parent asylum support reference number
        ///tom,SIMPSON,1990-01-01,AB123456C,
        ///fred,Jones,1990-01-01,ABCD,
        ///dave,smith,32/01/1990,AB123456D,
        ///,flower,1990-01-01,,
        ///.
        /// </summary>
        internal static string bulkchecktemplate_some_invalid_items {
            get {
                return ResourceManager.GetString("bulkchecktemplate some invalid items", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {&quot;id&quot;:&quot;4579AE90-8B2B-4C02-AC08-756CBBB1C567&quot;,&quot;name&quot;:&quot;Hollinswood Primary School&quot;,&quot;LegalName&quot;:&quot;HOLLINSWOOD PRIMARY SCHOOL&quot;,&quot;category&quot;:{&quot;id&quot;:&quot;001&quot;,&quot;name&quot;:&quot;Establishment&quot;},&quot;type&quot;:{&quot;id&quot;:&quot;01&quot;,&quot;name&quot;:&quot;Community School&quot;},&quot;urn&quot;:&quot;123456&quot;,&quot;uid&quot;:null,&quot;upin&quot;:null,&quot;ukprn&quot;:&quot;10069246&quot;,&quot;establishmentNumber&quot;:&quot;2200&quot;,&quot;status&quot;:{&quot;id&quot;:1,&quot;name&quot;:&quot;Open&quot;},&quot;closedOn&quot;:null,&quot;address&quot;:&quot;Dale Acre Way, Hollinswood, Telford, Shropshire, TF3 2EP&quot;,&quot;telephone&quot;:&quot;01952386920&quot;,&quot;region&quot;:{&quot;id&quot;:&quot;F&quot;,&quot;name&quot;:&quot;West Midlands&quot;},&quot;localAuthority&quot;:{&quot;id&quot;:&quot;24 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ClaimSchool {
            get {
                return ResourceManager.GetString("ClaimSchool", resourceCulture);
            }
        }
    }
}
