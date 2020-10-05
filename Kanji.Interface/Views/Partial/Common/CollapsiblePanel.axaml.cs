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

        public static readonly AvaloniaProperty CommandProperty = AvaloniaProperty.Register<CollapsiblePanelBase, ICommand>("Command");

        public static readonly AvaloniaProperty CommandParameterProperty = AvaloniaProperty.Register<CollapsiblePanelBase, object>("CommandParameter");

        public static readonly AvaloniaProperty IsContentShownProperty = AvaloniaProperty.Register<CollapsiblePanelBase, bool>("IsContentShown");

        public static readonly AvaloniaProperty TextProperty = AvaloniaProperty.Register<CollapsiblePanelBase, string>("Text");

        public static readonly AvaloniaProperty HeaderContentProperty = AvaloniaProperty.Register<CollapsiblePanelBase, object>("HeaderContent");

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
