using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Kanji.Interface.Controls
{
    public class SrsEntryFilterControlBase : TemplatedControl
    {
        #region Dependency properties

        public static readonly StyledProperty<bool> IsInlineProperty = AvaloniaProperty.Register<SrsEntryFilterControlBase, bool>(nameof(IsInline), true);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value indicating if the control is to be
        /// rendered inline or if it may use a more vertical layout.
        /// </summary>
        public bool IsInline
        {
            get { return (bool)GetValue(IsInlineProperty); }
            set { SetValue(IsInlineProperty, value); }
        }

        #endregion

        #region Constructors

        public SrsEntryFilterControlBase()
        {
            
        }

        #endregion
    }
}
