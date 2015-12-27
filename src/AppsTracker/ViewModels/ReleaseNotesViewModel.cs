using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service.Web;

namespace AppsTracker.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class ReleaseNotesViewModel : ViewModelBase
    {
        public override string Title
        {
            get { return "release notes"; }
        }

        private readonly IReleaseNotesService releaseNotesService;

        public AsyncProperty<IEnumerable<ReleaseNote>> ReleaseNotes { get; private set; }

        [ImportingConstructor]
        public ReleaseNotesViewModel(IReleaseNotesService releaseNotesService)
        {
            this.releaseNotesService = releaseNotesService;
            ReleaseNotes = new TaskObserver<IEnumerable<ReleaseNote>>(
                this.releaseNotesService.GetReleaseNotesAsync, this);
        }
    }
}
