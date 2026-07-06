using CavisteApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CavisteApp.ViewModels;

public partial class AccueilViewModel : ObservableObject
{
    public bool EstAdministrateur => SessionContext.Instance.EstAdministrateur;

    [RelayCommand]
    private void Naviguer(SectionApplication section) => NavigationService.Instance.NaviguerVers(section);
}
