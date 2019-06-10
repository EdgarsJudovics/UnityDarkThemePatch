using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityDarkThemePatch.Models
{
    public class ConsoleChoice
    {
        /// <summary>
        /// The Description of the Choice to display in the console.
        /// </summary>
        public string ChoiceDescription { get; set; }
        /// <summary>
        /// The Action to execute when the choice is selected.
        /// </summary>
        public Action ChoiceAction { get; set; }
    }
}
