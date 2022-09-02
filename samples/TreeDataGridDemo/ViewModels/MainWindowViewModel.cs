namespace TreeDataGridDemo.ViewModels
{
    internal class MainWindowViewModel
    {
        private CountriesPageViewModel? _countries;
        private FilesPageViewModel? _files;
        private DragDropPageViewModel? _dragDrop;

        public CountriesPageViewModel Countries
        {
            get => _countries ??= new CountriesPageViewModel();
        }

        public FilesPageViewModel Files
        {
            get => _files ??= new FilesPageViewModel();
        }

        public DragDropPageViewModel DragDrop
        {
            get => _dragDrop ??= new DragDropPageViewModel();
        }
    }
}
