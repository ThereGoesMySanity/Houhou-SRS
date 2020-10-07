using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Kanji.Interface.Controls
{
    public class CollapsiblePanelBase : UserControl
    {
        #region Dependency properties

        public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<CollapsiblePanelBase, ICommand>(nameof(Command));

        public static readonly StyledProperty<object> CommandParameterProperty = AvaloniaProperty.Register<CollapsiblePanelBase, object>(nameof(CommandParameter));

        public static readonly StyledProperty<bool> IsContentShownProperty = AvaloniaProperty.Register<CollapsiblePanelBase, bool>(nameof(IsContentShown));

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<CollapsiblePanelBase, string>(nameof(Text));

        public static readonly StyledProperty<object> HeaderContentProperty = AvaloniaProperty.Register<CollapsiblePanelBase, object>(nameof(HeaderContent));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the command triggered when the panel is activated.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// Gets or sets the parameter of the command triggered when the panel is activated.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// Gets or sets the current visibility of the panel content.
        /// </summary>
        public bool IsContentShown
        {
            get { return (bool)GetValue(IsContentShownProperty); }
            set { SetValue(IsContentShownProperty, value); }
        }

        /// <summary>
        /// Gets or sets the text written in the header (always visible) section.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the content of the header.
        /// </summary>
        public object HeaderContent
        {
            get { return GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        #endregion

        #region Methods



        #endregion
    }

    public partial class CollapsiblePanel : CollapsiblePanelBase
    {
        public CollapsiblePanel()
        {
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            CommandButton = this.FindControl<Button>("CommandButton");
        }
        public Button CommandButton { get; private set; }

        private void OnCommandButtonClick(object sender, RoutedEventArgs e)
        {
            if (Command == null)
            {
                IsContentShown = !IsContentShown;
            }
        }
    }
}
