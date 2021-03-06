using System;
using MonoTouch.Foundation;

namespace CodeBucket.iOS.Views.Source
{
	public class SourceView : FileSourceView
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ViewModel.Bind(x => x.IsLoading, x =>
			{
				if (x) return;
				if (!string.IsNullOrEmpty(ViewModel.ContentPath))
				{
					var data = System.IO.File.ReadAllText(ViewModel.ContentPath, System.Text.Encoding.UTF8);
					LoadContent(data, System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "SourceBrowser"));
				}
				else if (!string.IsNullOrEmpty(ViewModel.FilePath))
				{
					LoadFile(ViewModel.FilePath);
				}
			});
		}
    }
}

