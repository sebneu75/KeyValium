using KeyValium.Inspector.MVP.Presenters;
using Mad.MVP;
using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace KeyValium.Inspector
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.CurrentCulture = CultureInfo.InvariantCulture;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var presenter = ViewHelper.CreateViewPresenter<frmMain, MainPresenter>();
            presenter.AppContext = new MVP.Models.InspectorContext();
            presenter.Model = new MVP.Models.InspectorModel();            

            //var frm = new frmBrowser();
            //frm.ShowDialog();

            Application.Run(presenter.View as Form);
        }
    }
}
