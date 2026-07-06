using System.Windows.Controls;
using CavisteApp.ViewModels;

namespace CavisteApp.Views;

public partial class VinListView : UserControl
{
    public VinListView()
    {
        InitializeComponent();
    }

    private async void FournisseurComboBox_DropDownOpened(object? sender, System.EventArgs e)
    {
        if (DataContext is VinListViewModel vm)
        {
            await vm.RafraichirFournisseursAsync();
        }
    }
}