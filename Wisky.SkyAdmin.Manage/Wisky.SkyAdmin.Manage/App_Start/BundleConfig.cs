using System.Web;
using System.Web.Optimization;

namespace Wisky.SkyAdmin.Manage
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));
            /*DataTable CSS*/
            bundles.Add(new StyleBundle("~/Content/DataTableCSS").Include(
                      "~/Content/plugins/DataTables/datatables.min.css",
                      "~/Content/plugins/DataTables/datatables.css"));

            /*Vendor CSS*/
            bundles.Add(new StyleBundle("~/Content/VendorsCSS").Include(
                "~/Content/template/template-material/vendors/bower_components/fullcalendar/dist/fullcalendar.min.css",
                "~/Content/template/template-material/vendors/bower_components/animate.css/animate.min.css",
                "~/Content/plugins/swal/sweetalert.css",
                "~/Content/template/template-material/vendors/bower_components/material-design-iconic-font/css/material-design-iconic-font.min.css",
                "~/Content/template/template-material/vendors/bower_components/malihu-custom-scrollbar-plugin/jquery.mCustomScrollbar.min.css",
                "~/Content/template/template-material/vendors/bower_components/bootstrap-select/dist/css/bootstrap-select.css",
                "~/Content/template/template-material/vendors/bootgrid/jquery.bootgrid.min.css"));

            /*General CSS*/
            bundles.Add(new StyleBundle("~/Content/CSS").Include(
                      "~/Content/template/template-material/css/app.min.1.css",
                      "~/Content/template/template-material/css/app.min.2.css",
                      "~/Content/css/sweetalert.css"));

            /*Plugins CSS*/
            bundles.Add(new StyleBundle("~/Content/Plugins").Include(
                "~/Content/plugins/bootstrap-tagsinput/bootstrap-tagsinput.css",
                "~/Content/font-awesome-4.7.0/css/font-awesome.css",
                "~/Content/plugins/Gritter-master/css/jquery.gritter.css",
                "~/Content/plugins/bootstrap-datetimepicker/css/bootstrap-datetimepicker.min.css",
                "~/Content/plugins/select2-3.5.0/select2.css"));

            /*Custom CSS*/
            bundles.Add(new StyleBundle("~/Content/Custom").Include(
                      "~/Content/css/theme.css",
                      "~/Content/css/admin.css",
                      "~/Content/css/style.css",
                      "~/Content/frontend/css/dual-screen.css",
                      "~/Content/ace/assets/css/NewSkin.css",
                      "~/Content/frontend/css/theme.css",
                      "~/Content/css/skyplus-skin.css",
                      "~/Content/ace/assets/css/ace.min.css"));

            /*DateTimePicker CSS*/
            bundles.Add(new StyleBundle("~/Content/DateTimePicker").Include(
                "~/Content/css/daterangepicker.css",
                "~/Content/css/datepicker.css"));

            /*General Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/General").Include(
                "~/Content/template/template-material/vendors/bower_components/bootstrap/dist/js/bootstrap.min.js",
                "~/Content/template/template-material/vendors/bower_components/flot/jquery.flot.js",
                "~/Content/template/template-material/vendors/bower_components/flot/jquery.flot.resize.js",
                "~/Content/template/template-material/vendors/bower_components/flot.curvedlines/curvedLines.js",
                "~/Content/template/template-material/vendors/sparklines/jquery.sparkline.min.js",
                "~/Content/template/template-material/vendors/bower_components/jquery.easy-pie-chart/dist/jquery.easypiechart.min.js",
                "~/Content/template/template-material/vendors/bower_components/moment/min/moment.min.js",
                "~/Content/template/template-material/vendors/bower_components/fullcalendar/dist/fullcalendar.min.js",
                "~/Content/template/template-material/vendors/bower_components/Waves/dist/waves.min.js",
                "~/Content/template/template-material/vendors/bootstrap-growl/bootstrap-growl.min.js",
                "~/Content/plugins/swal/sweetalert-dev.js",
                "~/Content/template/template-material/vendors/bower_components/malihu-custom-scrollbar-plugin/jquery.mCustomScrollbar.concat.min.js",
                "~/Content/template/template-material/vendors/bower_components/bootstrap-select/dist/js/bootstrap-select.js"));

            /*Template Material*/
            bundles.Add(new ScriptBundle("~/bundles/Template").Include(
                "~/Content/plugins/bootbox-4.4.0/bootbox.js",
                "~/Content/plugins/dragsort/jquery.dragsort-0.5.2.min.js",
                "~/Content/template/template-material/vendors/bootgrid/jquery.bootgrid.updated.min.js",
                "~/Content/template/template-material/js/flot-charts/curved-line-chart.js",
                "~/Content/template/template-material/js/flot-charts/line-chart.js",
                "~/Content/template/template-material/js/charts.js",
                "~/Content/template/template-material/js/charts.js",
                "~/Content/template/template-material/js/functions.js"));

            /*Plugins Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/Plugins").Include(
                "~/Content/plugins/bootstrap-tagsinput/bootstrap-tagsinput.js",
                "~/Content/plugins/DataTables/DataTables-1.10.12/js/jquery.dataTables.min.js",
                "~/Content/plugins/DataTables/dataTables.fixedColumns.js",
                "~/Content/plugins/dragsort/jquery.dragsort-0.5.2.min.js",
                "~/Content/plugins/Gritter-master/js/jquery.gritter.min.js",
                "~/Content/js/bootbox.min.js",
                "~/Content/plugins/bootstrap-datetimepicker/js/bootstrap-datetimepicker.js",
                "~/Content/js/jquery.nicescroll.js",
                "~/Content/ace/assets/js/jquery.colorbox-min.js",
                "~/Content/ace/assets/js/fuelux/fuelux.spinner.min.js",
                "~/Content/ace/assets/js/jquery.gritter.min.js",
                "~/Content/plugins/select2-3.5.0/select2.min.js",
                "~/Content/plugins/DataTables/fnSetFilteringDelay.js"));

            /*Accounting Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/Accounting").Include(
                "~/Scripts/accounting.min.js"));

            /*Custom Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/Custom").Include(
                "~/Content/frontend/js/common.js",
                "~/Content/frontend/js/skyplus-products.js",
                "~/Content/frontend/js/skyplus-skin.js",
                "~/Content/frontend/js/skyplus-dualscreen.js",
                "~/Content/frontend/js/skyplus-inventory.js",
                "~/Content/frontend/js/skyplus-room.js",
                "~/Content/frontend/js/skyplus-dashboard.js",
                "~/Content/frontend/js/skyplus-cost.js",
                "~/Content/frontend/js/skyplus-dualscreen.js"));

            /*DateTimePicker Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/DateTimePicker").Include(
                "~/Content/js/daterangepicker.js",
                "~/Content/js/bootstrap-datepicker.min.js",
                "~/Content/js/bootstrap-datetimepicker.min.js"));

            /*HighChart Scripts*/
            bundles.Add(new ScriptBundle("~/bundles/HighChart").Include(
                "~/Content/js/highcharts.js",
                "~/Content/js/exporting.js",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js"));
        }
    }
}
