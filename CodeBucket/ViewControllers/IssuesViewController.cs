using System;
using CodeBucket.Bitbucket.Controllers;
using BitbucketSharp.Models;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch;
using CodeBucket.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Views;
using CodeFramework.Elements;
using CodeBucket.Filters.Models;

namespace CodeBucket.Bitbucket.Controllers.Issues
{
    public class IssuesViewController : BaseListControllerDrivenViewController, IListView<IssueModel>
    {
        public string User { get; private set; }
        public string Slug { get; private set; }

        private readonly UISegmentedControl _viewSegment;
        private readonly UIBarButtonItem _segmentBarButton;
  
        public new IssuesController Controller
        {
            get { return (IssuesController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public IssuesViewController(string user, string slug)
        {
            User = user;
            Slug = slug;
            Style = UITableViewStyle.Plain;
            EnableSearch = true;
            EnableFilter = true;
            Root.UnevenRows = true;
            Title = "Issues".t();
            SearchPlaceholder = "Search Issues".t();
            Controller = new IssuesController(this, user, slug);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(CodeFramework.Images.Buttons.Add, () => {
                var b = new IssueEditViewController {
                    Username = User,
                    RepoSlug = Slug,
                    Success = (issue) => Controller.CreateIssue(issue)
                };
                NavigationController.PushViewController(b, true);
            }));

            _viewSegment = new UISegmentedControl(new string[] { "All".t(), "Open".t(), "Mine".t(), "Custom".t() });
            _viewSegment.ControlStyle = UISegmentedControlStyle.Bar;
            _segmentBarButton = new UIBarButtonItem(_viewSegment);
        }


        public void Render(ListModel<IssueModel> model)
        {
            RenderList(model, x => {
                var assigned = x.Responsible != null ? x.Responsible.Username : "unassigned";
                var kind = x.Metadata.Kind;
                if (kind.ToLower().Equals("enhancement")) 
                    kind = "enhance";

                var el = new IssueElement(x.LocalId.ToString(), x.Title, assigned, x.Status, x.Priority, kind, x.UtcLastUpdated);
                el.Tag = x;
                el.Tapped += () => {
                    //Make sure the first responder is gone.
                    View.EndEditing(true);
                    var info = new IssueInfoViewController(User, Slug, x.LocalId);
                    info.Controller.ModelChanged = newModel => ChildChangedModel(newModel, x);
                    NavigationController.PushViewController(info, true);
                };
                return el;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BeginInvokeOnMainThread(delegate {
                _viewSegment.SelectedSegment = 1;
                _viewSegment.SelectedSegment = 0;

                //Select which one is currently selected
                if (Controller.Filter.Equals(IssuesFilterModel.CreateAllFilter()))
                    _viewSegment.SelectedSegment = 0;
                else if (Controller.Filter.Equals(IssuesFilterModel.CreateOpenFilter()))
                    _viewSegment.SelectedSegment = 1;
                else if (Controller.Filter.Equals(IssuesFilterModel.CreateMineFilter(Application.Account.Username)))
                    _viewSegment.SelectedSegment = 2;
                else
                    _viewSegment.SelectedSegment = 3;
                    
                _viewSegment.ValueChanged += (sender, e) => SegmentValueChanged();
            });

            _segmentBarButton.Width = View.Frame.Width - 10f;
            ToolbarItems = new [] { new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace), _segmentBarButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) };
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }

        private void SegmentValueChanged()
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                Controller.ApplyFilter(IssuesFilterModel.CreateAllFilter(), true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 1)
            {
                Controller.ApplyFilter(IssuesFilterModel.CreateOpenFilter(), true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 2)
            {
                Controller.ApplyFilter(IssuesFilterModel.CreateMineFilter(Application.Account.Username), true, false);
                UpdateAndRender();
            }
            else if (_viewSegment.SelectedSegment == 3)
            {
            }
        }

//        private void ScrollToModel(IssueModel issue, bool animate = false)
//        {
//            int s, r = 0;
//            bool done = false;
//            for (s = 0; s < Root.Count; s++)
//            {
//                for (r = 0; r < Root[s].Count; r++)
//                {
//                    var el = Root[s][r] as IssueElement;
//                    if (el != null && ((IssueModel)el.Tag).LocalId == issue.LocalId)
//                    {
//                        done = true;
//                        break;
//                    }
//                }
//                if (done)
//                    break;
//            }
//            
//            try 
//            {
//                TableView.ScrollToRow(NSIndexPath.FromRowSection(r, s), UITableViewScrollPosition.Top, animate);
//            }
//            catch { }
//        }

        private void ChildChangedModel(IssueModel changedModel, IssueModel oldModel)
        {
            //If null then it's been deleted!
            if (changedModel == null)
            {
                Controller.DeleteIssue(oldModel);

//                var c = TableView.ContentOffset;
//                var m = Model as List<IssueModel>;
//
//                Render();
//                TableView.ContentOffset = c;
            }
            else
            {
                Controller.UpdateIssue(changedModel);
            }
        }

    }
}

