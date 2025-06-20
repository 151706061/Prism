using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Prism.Dialogs
{
    /// <summary>Prism's default dialog host.</summary>
    public partial class DialogWindow : Window, IDialogWindow
    {
        /// <summary>The <see cref="IDialogResult"/> of the dialog.</summary>
        public IDialogResult Result { get; set; }

        /// <summary>Initializes a new instance of the <see cref="DialogWindow"/> class.</summary>
        public DialogWindow()
        {
            InitializeComponent();

#if DEBUG
            //// this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
