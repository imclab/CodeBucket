using MonoTouch.Dialog;
using MonoTouch.UIKit;
using CodeFramework.iOS.ViewControllers;
using CodeBucket.Core.Filters;
using System;
using CodeFramework.ViewControllers;
using System.Linq;

namespace CodeBucket.iOS.Views.Filters
{
    public class IssuesFilterViewController : BaseDialogViewController
    {
        private readonly IssuesFilterModel _currentFilter;
        private EntryElement _filterName;
		private EntryElement _assignedTo;
		private EntryElement _reportedBy;
		private MultipleChoiceElement<IssuesFilterModel.StatusModel> _statusChoice;
		private MultipleChoiceElement<IssuesFilterModel.KindModel> _kindChoice;
		private MultipleChoiceElement<IssuesFilterModel.PriorityModel> _priorityChoice;
		private EnumChoiceElement<IssuesFilterModel.Order> _orderby;

        public Action<IssuesFilterModel> CreatedFilterModel;

        public IssuesFilterViewController(IssuesFilterModel currentFilter)
            : base(true)
        {
            _currentFilter = currentFilter.Clone();
            Style = UITableViewStyle.Grouped;
            Title = "Filter & Sort".t();
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {

                if (string.IsNullOrEmpty(_filterName.Value))
                {
                    MonoTouch.Utilities.ShowAlert("Filter Name", "You must name your filter!");
                    return;
                }

                CreatedFilterModel(CreateFilterModel());
                NavigationController.PopViewControllerAnimated(true);
            });
        }

        private IssuesFilterModel CreateFilterModel()
        {
			var model = new IssuesFilterModel();
			model.AssignedTo = _assignedTo.Value;
			model.ReportedBy = _reportedBy.Value;
			model.Status = _statusChoice.Obj;
			model.Priority = _priorityChoice.Obj;
			model.Kind = _kindChoice.Obj;
			model.OrderBy = _orderby.Value;
            model.FilterName = _filterName.Value;
			return model;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			//Load the root
			var root = new RootElement(Title) {
                new Section() {
                    (_filterName = new InputElement("Filter Name", "Filter Name", _currentFilter.FilterName) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None })
                },
				new Section("Filter") {
                    (_assignedTo = new InputElement("Assigned To", "Anybody", _currentFilter.AssignedTo) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_reportedBy = new InputElement("Reported By", "Anybody", _currentFilter.ReportedBy) { TextAlignment = UITextAlignment.Right, AutocorrectionType = UITextAutocorrectionType.No, AutocapitalizationType = UITextAutocapitalizationType.None }),
                    (_kindChoice = CreateMultipleChoiceElement("Kind", _currentFilter.Kind)),
                    (_statusChoice = CreateMultipleChoiceElement("Status", _currentFilter.Status)),
                    (_priorityChoice = CreateMultipleChoiceElement("Priority", _currentFilter.Priority)),
				},
				new Section("Order By") {
                    (_orderby = CreateEnumElement<IssuesFilterModel.Order>("Field", _currentFilter.OrderBy)),
				}
			};

			Root = root;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            TableView.ReloadData();
        }

        public class EnumChoiceElement<T> : MonoTouch.Dialog.StyledStringElement where T : struct, IConvertible
        {
            private T _value;

            public new T Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    base.Value = ((Enum)Enum.ToObject(typeof(T), value)).Description();
                }
            }

            public EnumChoiceElement(string title, T defaultVal)
                : base(title, string.Empty, UITableViewCellStyle.Value1)
            {
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
                Value = defaultVal;
            }
        }

        public EnumChoiceElement<T> CreateEnumElement<T>(string title, T value) where T : struct, IConvertible
        {
            var element = new EnumChoiceElement<T>(title, value);

            element.Tapped += () =>
            {
                var ctrl = new BaseDialogViewController(true);
                ctrl.Title = title;
                ctrl.Style = MonoTouch.UIKit.UITableViewStyle.Grouped;

                var sec = new MonoTouch.Dialog.Section();
                foreach (var x in System.Enum.GetValues(typeof(T)).Cast<System.Enum>())
                {
                    sec.Add(new MonoTouch.Dialog.StyledStringElement(x.Description(), () => { 
                        element.Value = (T)Enum.ToObject(typeof(T), x); 
                        NavigationController.PopViewControllerAnimated(true);
                    }) { 
                        Accessory = object.Equals(x, element.Value) ? 
                            MonoTouch.UIKit.UITableViewCellAccessory.Checkmark : MonoTouch.UIKit.UITableViewCellAccessory.None 
                    });
                }
                ctrl.Root = new MonoTouch.Dialog.RootElement(title) { sec };
                NavigationController.PushViewController(ctrl, true);
            };

            return element;
        }

        public class MultipleChoiceElement<T> : MonoTouch.Dialog.StyledStringElement
        {
            public T Obj;
            public MultipleChoiceElement(string title, T obj)
                : base(title, CreateCaptionForMultipleChoice(obj), UITableViewCellStyle.Value1)
            {
                Obj = obj;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
        }

        protected MultipleChoiceElement<T> CreateMultipleChoiceElement<T>(string title, T o)
        {
            var element = new MultipleChoiceElement<T>(title, o);
            element.Tapped += () =>
            {
                var en = new MultipleChoiceViewController(element.Caption, o);
                en.ViewDisappearing += (sender, e) => {
                    element.Value = CreateCaptionForMultipleChoice(o);
                };
                NavigationController.PushViewController(en, true);
            };

            return element;
        }

        private static string CreateCaptionForMultipleChoice<T>(T o)
        {
            var fields = o.GetType().GetProperties();
            var sb = new System.Text.StringBuilder();
            int trueCounter = 0;
            foreach (var f in fields)
            {
                if ((bool)f.GetValue(o))
                {
                    sb.Append(f.Name);
                    sb.Append(", ");
                    trueCounter++;
                }
            }
            var str = sb.ToString();
            if (str.EndsWith(", "))
                return trueCounter == fields.Length ? "Any".t() : str.Substring(0, str.Length - 2);
            return "None".t();
        }
    }
}

