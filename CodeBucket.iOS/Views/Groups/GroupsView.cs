using CodeFramework.ViewControllers;
using CodeBucket.Core.ViewModels.Groups;
using MonoTouch.Dialog;

namespace CodeBucket.iOS.Views.Groups
{
    public class GroupsView : ViewModelCollectionDrivenDialogViewController
	{
        public override void ViewDidLoad()
        {
            Title = "Groups".t();
            NoItemsText = "No Groups".t();

            base.ViewDidLoad();

			var vm = (GroupsViewModel) ViewModel;
			BindCollection(vm.Organizations, x =>
			{
				var e = new StyledStringElement(x.Name);
				e.Tapped += () => vm.GoToGroupCommand.Execute(x);
				return e;
			});
        }
	}
}

