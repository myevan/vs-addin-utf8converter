using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;

namespace PbTheCat.UTF8Converter
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidUTF8ConverterPkgString)]
    public sealed class UTF8ConverterPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public UTF8ConverterPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidUTF8ConverterCmdSet, (int)PkgCmdIDList.cmdidConvertUTF8);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
                mcs.AddCommand( menuItem );
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE2;
            var activeDoc = dte.ActiveDocument;
            if (activeDoc == null)
            {
                return;
            }

            string activeFilePath = activeDoc.FullName;
            var stream = new FileStream(activeFilePath, FileMode.Open);
            var utf8Encoding = new UTF8Encoding(false, true);
            var utf8Reader = new StreamReader(stream, utf8Encoding, true);

            try
            {
                string utf8Text = utf8Reader.ReadToEnd();
                stream.Close();

                try
                {
                    Encoding asciiEncoding = Encoding.GetEncoding("us-ascii", new EncoderExceptionFallback(), new DecoderExceptionFallback());
                    asciiEncoding.GetBytes(utf8Text);

                    MessageBox.Show("pure_ascii_file:" + activeFilePath, "UTF8Converter");
                    return;
                }
                catch (EncoderFallbackException)
                {
                    MessageBox.Show("already_converted_file:" + activeFilePath, "UTF8Converter");
                    return;
                }
            }
            catch (DecoderFallbackException)
            {

                stream.Position = 0;

                try
                {
                    var defaultReader = new StreamReader(stream, Encoding.Default);
                    var defaultText = defaultReader.ReadToEnd();
                    stream.Close();

                    activeDoc.Close();
                    MessageBox.Show("convert_file:"+ activeFilePath + " code_page:" + defaultReader.CurrentEncoding.CodePage.ToString(), "UTF8Converter");

                    var activeFileExt = Path.GetExtension(activeFilePath).ToLower();
                    if (activeFileExt == ".lua")
                    {
                        File.WriteAllText(activeFilePath, defaultText, new UTF8Encoding(true, true));
                    }
                    else
                    {
                        File.WriteAllText(activeFilePath, defaultText, new UTF8Encoding(false, true));
                    }
                }
                catch (DecoderFallbackException)
                {
                    MessageBox.Show("broken_encoding_file:" + activeFilePath, "UTF8Converter");
                    return;
                }
            }
        }
    }
}
