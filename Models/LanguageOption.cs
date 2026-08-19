using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    /// <summary>
    /// Represents a single language option for UI selection.
    /// 
    /// Why this class exists:
    /// - LanguageManager tracks only the *current* language and handles persistence.
    /// - The ComboBox needs a *list of available choices* to display.
    /// - Each choice should have metadata: Code (e.g. "en-US"), Name (e.g. "English"),
    ///   and FlagPath (for showing a flag icon).
    /// 
    /// In short:
    /// LanguageManager = "what is selected now"
    /// LanguageOption = "what choices are available"
    /// </summary>

    public class LanguageOption
    {
        public string? Code { get; set; }   // e.g. "en-US", "ar", "fr-FR"
        public string? Name { get; set; }   // e.g. "English", "العربية", "Français"
        public string? FlagPath { get; set; } // e.g. "/Assets/images/uk_flag.png"
    }
}
