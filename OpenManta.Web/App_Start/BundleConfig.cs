using System.Web.Optimization;

namespace WebInterface
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			BundleTable.EnableOptimizations = false;

			// Scripts
			bundles.Add(new ScriptBundle("~/resources/js/jquery").Include("~/bower_components/jquery/jquery.js"));
			bundles.Add(new ScriptBundle("~/resources/js/bootstrap").Include("~/bower_components/bootstrap/docs/assets/js/bootstrap.js"));
			bundles.Add(new ScriptBundle("~/resources/js/jqplot").Include("~/bower_components/gbelmm-jqplot/src/core/jquery.jqplot.js"));


			// Styles
			bundles.Add (new StyleBundle ("~/resources/css/bootstrap").Include ("~/bower_components/bootstrap/docs/assets/css/bootstrap.css",
				"~/bower_components/bootstrap/docs/assets/css/bootstrap-responsive.css",
				"~/Content/app.css",
				"~/Content/fonts.css"));
			bundles.Add(new ScriptBundle("~/resources/css/jqplot").Include("~/bower_components/gbelmm-jqplot/src/core/jquery.jqplot.css"));
		}
	}
}