namespace ProControlsDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private HistoryTablePageViewModel? _history;
        private CountriesPageViewModel? _countries;
        private FilesPageViewModel? _files;

        public HistoryTablePageViewModel History
        {
            get => _history ??= new HistoryTablePageViewModel();
        }
 
        public CountriesPageViewModel Countries
        {
            get => _countries ??= new CountriesPageViewModel();
        }

        public FilesPageViewModel Files
        {
            get => _files ??= new FilesPageViewModel();
        }
    }
}
