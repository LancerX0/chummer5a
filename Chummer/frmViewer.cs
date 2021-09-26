/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Xsl;
using Codaxy.WkHtmlToPdf;
using Microsoft.Win32;
using NLog;

namespace Chummer
{
    public partial class frmViewer : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private List<Character> _lstCharacters = new List<Character>(1);
        private XmlDocument _objCharacterXml = new XmlDocument { XmlResolver = null };
        private string _strSelectedSheet = GlobalSettings.DefaultCharacterSheet;
        private bool _blnLoading;
        private CultureInfo _objPrintCulture = GlobalSettings.CultureInfo;
        private string _strPrintLanguage = GlobalSettings.Language;
        private CancellationTokenSource _objRefresherCancellationTokenSource;
        private CancellationTokenSource _objOutputGeneratorCancellationTokenSource;
        private Task _tskRefresher;
        private Task _tskOutputGenerator;
        private readonly string _strFilePathName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Guid.NewGuid().ToString("D", GlobalSettings.InvariantCultureInfo) + ".htm");

        #region Control Events

        public frmViewer()
        {
            if (_strSelectedSheet.StartsWith("Shadowrun 4", StringComparison.Ordinal))
            {
                _strSelectedSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;
            }
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                if (!_strSelectedSheet.Contains(Path.DirectorySeparatorChar))
                    _strSelectedSheet = Path.Combine(GlobalSettings.Language, _strSelectedSheet);
                else if (!_strSelectedSheet.Contains(GlobalSettings.Language) && _strSelectedSheet.Contains(GlobalSettings.Language.Substring(0, 2)))
                {
                    _strSelectedSheet = _strSelectedSheet.Replace(GlobalSettings.Language.Substring(0, 2), GlobalSettings.Language);
                }
            }
            else
            {
                int intLastIndexDirectorySeparator = _strSelectedSheet.LastIndexOf(Path.DirectorySeparatorChar);
                if (intLastIndexDirectorySeparator != -1 && _strSelectedSheet.Contains(GlobalSettings.Language.Substring(0, 2)))
                    _strSelectedSheet = _strSelectedSheet.Substring(intLastIndexDirectorySeparator + 1);
            }

            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION"))
                objRegistry?.SetValue(AppDomain.CurrentDomain.FriendlyName, GlobalSettings.EmulatedBrowserVersion * 1000, RegistryValueKind.DWord);

            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION"))
                objRegistry?.SetValue(AppDomain.CurrentDomain.FriendlyName, GlobalSettings.EmulatedBrowserVersion * 1000, RegistryValueKind.DWord);

            // These two needed to have WebBrowser control obey DPI settings for Chummer
            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_96DPI_PIXEL"))
                objRegistry?.SetValue(AppDomain.CurrentDomain.FriendlyName, 1, RegistryValueKind.DWord);

            using (RegistryKey objRegistry = Registry.CurrentUser.CreateSubKey("Software\\WOW6432Node\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_96DPI_PIXEL"))
                objRegistry?.SetValue(AppDomain.CurrentDomain.FriendlyName, 1, RegistryValueKind.DWord);

            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            ContextMenuStrip[] lstCmsToTranslate = {
                cmsPrintButton,
                cmsSaveButton
            };
            foreach (ContextMenuStrip objCms in lstCmsToTranslate)
            {
                if (objCms == null)
                    continue;
                foreach (ToolStripMenuItem tssItem in objCms.Items.OfType<ToolStripMenuItem>())
                {
                    tssItem.UpdateLightDarkMode();
                    tssItem.TranslateToolStripItemsRecursively();
                }
            }
        }

        private async void frmViewer_Load(object sender, EventArgs e)
        {
            _blnLoading = true;
            // Populate the XSLT list with all of the XSL files found in the sheets directory.
            LanguageManager.PopulateSheetLanguageList(cboLanguage, _strSelectedSheet, _lstCharacters);
            PopulateXsltList();

            cboXSLT.SelectedValue = _strSelectedSheet;
            // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
            if (cboXSLT.SelectedIndex == -1)
            {
                string strLanguage = cboLanguage.SelectedValue?.ToString();
                int intNameIndex;
                if (string.IsNullOrEmpty(strLanguage) || strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    intNameIndex = cboXSLT.FindStringExact(GlobalSettings.DefaultCharacterSheet);
                else
                    intNameIndex = cboXSLT.FindStringExact(GlobalSettings.DefaultCharacterSheet.Substring(GlobalSettings.DefaultLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                if (intNameIndex != -1)
                    cboXSLT.SelectedIndex = intNameIndex;
                else if (cboXSLT.Items.Count > 0)
                {
                    if (string.IsNullOrEmpty(strLanguage) || strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        _strSelectedSheet = GlobalSettings.DefaultCharacterSheetDefaultValue;
                    else
                        _strSelectedSheet = Path.Combine(strLanguage, GlobalSettings.DefaultCharacterSheetDefaultValue);
                    cboXSLT.SelectedValue = _strSelectedSheet;
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                        _strSelectedSheet = cboXSLT.SelectedValue?.ToString();
                    }
                }
            }
            _blnLoading = false;
            await SetDocumentText(LanguageManager.GetString("String_Loading_Characters"));
        }

        private async void cboXSLT_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Re-generate the output when a new sheet is selected.
            if (_blnLoading)
                return;
            _strSelectedSheet = cboXSLT.SelectedValue?.ToString() ?? string.Empty;
            await RefreshSheet();
        }

        private void cmdPrint_Click(object sender, EventArgs e)
        {
            webViewer.ShowPrintDialog();
        }

        private void tsPrintPreview_Click(object sender, EventArgs e)
        {
            webViewer.ShowPrintPreviewDialog();
        }

        private void tsSaveAsHTML_Click(object sender, EventArgs e)
        {
            // Save the generated output as HTML.
            SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Html") + '|' + LanguageManager.GetString("DialogFilter_All");
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsHtml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;

            if (!strSaveFile.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                && !strSaveFile.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
                strSaveFile += ".htm";

            using (TextWriter objWriter = new StreamWriter(strSaveFile, false, Encoding.UTF8))
                objWriter.Write(webViewer.DocumentText);
        }

        private void tsSaveAsXml_Click(object sender, EventArgs e)
        {
            // Save the printout XML generated by the character.
            SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Xml") + '|' + LanguageManager.GetString("DialogFilter_All");
            SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsXml");
            SaveFileDialog1.ShowDialog();
            string strSaveFile = SaveFileDialog1.FileName;

            if (string.IsNullOrEmpty(strSaveFile))
                return;

            if (!strSaveFile.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                strSaveFile += ".xml";

            try
            {
                _objCharacterXml.Save(strSaveFile);
            }
            catch (XmlException)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Save_Error_Warning"));
            }
            catch (UnauthorizedAccessException)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_Save_Error_Warning"));
            }
        }

        private void frmViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objRefresherCancellationTokenSource?.Cancel(false);
            _objOutputGeneratorCancellationTokenSource?.Cancel(false);

            // Remove the mugshots directory when the form closes.
            string mugshotsDirectoryPath = Path.Combine(Utils.GetStartupPath, "mugshots");
            if (Directory.Exists(mugshotsDirectoryPath))
            {
                try
                {
                    Directory.Delete(mugshotsDirectoryPath, true);
                }
                catch (IOException)
                {
                }
            }

            // Clear the reference to the character's Print window.
            foreach (CharacterShared objCharacterShared in Program.MainForm.OpenCharacterForms)
                if (objCharacterShared.PrintWindow == this)
                    objCharacterShared.PrintWindow = null;
        }

        private async void cmdSaveAsPdf_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                // Check to see if we have any "Print to PDF" printers, as they will be a lot more reliable than wkhtmltopdf
                string strPdfPrinter = string.Empty;
                foreach (string strPrinter in PrinterSettings.InstalledPrinters)
                {
                    if (strPrinter == "Microsoft Print to PDF" || strPrinter == "Foxit Reader PDF Printer" ||
                        strPrinter == "Adobe PDF")
                    {
                        strPdfPrinter = strPrinter;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(strPdfPrinter))
                {
                    DialogResult ePdfPrinterDialogResult = Program.MainForm.ShowMessageBox(this,
                        string.Format(GlobalSettings.CultureInfo,
                            LanguageManager.GetString("Message_Viewer_FoundPDFPrinter"), strPdfPrinter),
                        LanguageManager.GetString("MessageTitle_Viewer_FoundPDFPrinter"),
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                    switch (ePdfPrinterDialogResult)
                    {
                        case DialogResult.Cancel:
                        case DialogResult.Yes when DoPdfPrinterShortcut(strPdfPrinter):
                            return;

                        case DialogResult.Yes:
                            Program.MainForm.ShowMessageBox(this,
                                LanguageManager.GetString("Message_Viewer_PDFPrinterError"));
                            break;
                    }
                }

                // Save the generated output as PDF.
                SaveFileDialog1.Filter = LanguageManager.GetString("DialogFilter_Pdf") + '|' +
                                         LanguageManager.GetString("DialogFilter_All");
                SaveFileDialog1.Title = LanguageManager.GetString("Button_Viewer_SaveAsPdf");
                SaveFileDialog1.ShowDialog();
                string strSaveFile = SaveFileDialog1.FileName;

                if (string.IsNullOrEmpty(strSaveFile))
                    return;

                if (!strSaveFile.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    strSaveFile += ".pdf";

                if (!Directory.Exists(Path.GetDirectoryName(strSaveFile)) || !Utils.CanWriteToPath(strSaveFile))
                {
                    Program.MainForm.ShowMessageBox(this,
                        LanguageManager.GetString("Message_File_Cannot_Be_Accessed"));
                    return;
                }

                if (!Utils.SafeDeleteFile(strSaveFile, true))
                {
                    Program.MainForm.ShowMessageBox(this,
                        LanguageManager.GetString("Message_File_Cannot_Be_Accessed"));
                    return;
                }

                // No PDF printer found, let's use wkhtmltopdf

                try
                {
                    PdfDocument objPdfDocument = new PdfDocument
                    {
                        Html = webViewer.DocumentText,
                        ExtraParams = new Dictionary<string, string>
                        {
                            {"encoding", "UTF-8"},
                            {"dpi", "300"},
                            {"margin-top", "13"},
                            {"margin-bottom", "19"},
                            {"margin-left", "13"},
                            {"margin-right", "13"},
                            {"image-quality", "100"},
                            {"print-media-type", string.Empty}
                        }
                    };
                    PdfConvertEnvironment objPdfConvertEnvironment = new PdfConvertEnvironment
                    { WkHtmlToPdfPath = Path.Combine(Utils.GetStartupPath, "wkhtmltopdf.exe") };
                    PdfOutput objPdfOutput = new PdfOutput { OutputFilePath = strSaveFile };
                    await PdfConvert.ConvertHtmlToPdfAsync(objPdfDocument, objPdfConvertEnvironment, objPdfOutput);

                    if (!string.IsNullOrWhiteSpace(GlobalSettings.PdfAppPath))
                    {
                        Uri uriPath = new Uri(strSaveFile);
                        string strParams = GlobalSettings.PdfParameters
                            .Replace("{page}", "1")
                            .Replace("{localpath}", uriPath.LocalPath)
                            .Replace("{absolutepath}", uriPath.AbsolutePath);
                        ProcessStartInfo objPdfProgramProcess = new ProcessStartInfo
                        {
                            FileName = GlobalSettings.PdfAppPath,
                            Arguments = strParams,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        objPdfProgramProcess.Start();
                    }
                }
                catch (Exception ex)
                {
                    Program.MainForm.ShowMessageBox(this, ex.ToString());
                }
            }
        }

        private async void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _strPrintLanguage = cboLanguage.SelectedValue?.ToString() ?? GlobalSettings.Language;
            imgSheetLanguageFlag.Image = Math.Min(imgSheetLanguageFlag.Width, imgSheetLanguageFlag.Height) >= 32
                ? FlagImageGetter.GetFlagFromCountryCode192Dpi(_strPrintLanguage.Substring(3, 2))
                : FlagImageGetter.GetFlagFromCountryCode(_strPrintLanguage.Substring(3, 2));
            try
            {
                _objPrintCulture = CultureInfo.GetCultureInfo(_strPrintLanguage);
            }
            catch (CultureNotFoundException)
            {
            }
            if (_blnLoading)
                return;
            _blnLoading = true;
            string strOldSelected = _strSelectedSheet;
            // Strip away the language prefix
            if (strOldSelected.Contains(Path.DirectorySeparatorChar))
                strOldSelected = strOldSelected.Substring(strOldSelected.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            PopulateXsltList();
            string strNewLanguage = cboLanguage.SelectedValue?.ToString() ?? strOldSelected;
            if (strNewLanguage == strOldSelected)
            {
                _strSelectedSheet = strNewLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase) ? strOldSelected : Path.Combine(strNewLanguage, strOldSelected);
            }
            cboXSLT.SelectedValue = _strSelectedSheet;
            // If the desired sheet was not found, fall back to the Shadowrun 5 sheet.
            if (cboXSLT.SelectedIndex == -1)
            {
                var intNameIndex = cboXSLT.FindStringExact(strNewLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase) ? GlobalSettings.DefaultCharacterSheet : GlobalSettings.DefaultCharacterSheet.Substring(strNewLanguage.LastIndexOf(Path.DirectorySeparatorChar) + 1));
                if (intNameIndex != -1)
                    cboXSLT.SelectedIndex = intNameIndex;
                else if (cboXSLT.Items.Count > 0)
                {
                    _strSelectedSheet = strNewLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase) ? GlobalSettings.DefaultCharacterSheetDefaultValue : Path.Combine(strNewLanguage, GlobalSettings.DefaultCharacterSheetDefaultValue);
                    cboXSLT.SelectedValue = _strSelectedSheet;
                    if (cboXSLT.SelectedIndex == -1)
                    {
                        cboXSLT.SelectedIndex = 0;
                        _strSelectedSheet = cboXSLT.SelectedValue?.ToString();
                    }
                }
            }
            _blnLoading = false;
            await RefreshCharacters();
        }

        private async void frmViewer_CursorChanged(object sender, EventArgs e)
        {
            if (Cursor == Cursors.WaitCursor)
            {
                await Task.WhenAll(this.DoThreadSafeAsync(() =>
                    {
                        tsPrintPreview.Enabled = false;
                        tsSaveAsHtml.Enabled = false;
                    }),
                    cmdPrint.DoThreadSafeAsync(() => cmdPrint.Enabled = false),
                    cmdSaveAsPdf.DoThreadSafeAsync(() => cmdSaveAsPdf.Enabled = false));
            }
            else
            {
                await Task.WhenAll(this.DoThreadSafeAsync(() =>
                    {
                        tsPrintPreview.Enabled = true;
                        tsSaveAsHtml.Enabled = true;
                    }),
                    cmdPrint.DoThreadSafeAsync(() => cmdPrint.Enabled = true),
                    cmdSaveAsPdf.DoThreadSafeAsync(() => cmdSaveAsPdf.Enabled = true));
            }
        }

        #endregion Control Events

        #region Methods

        /// <summary>
        /// Set the text of the viewer to something descriptive. Also disables the Print, Print Preview, Save as HTML, and Save as PDF buttons.
        /// </summary>
        private Task SetDocumentText(string strText)
        {
            return webViewer.DoThreadSafeFuncAsync(() => webViewer.Height)
                .ContinueWith(x =>
                    "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><meta http-equiv=\"x - ua - compatible\" content=\"IE = Edge\"/><meta charset = \"UTF-8\" /></head><body style=\"width:100%;height:" +
                    x.Result.ToString(GlobalSettings.InvariantCultureInfo) +
                    ";text-align:center;vertical-align:middle;font-family:segoe, tahoma,'trebuchet ms',arial;font-size:9pt;\">" +
                    strText.CleanForHtml() + "</body></html>")
                .ContinueWith(x => webViewer.DoThreadSafeAsync(() => webViewer.DocumentText = x.Result));
        }

        /// <summary>
        /// Asynchronously update the characters (and therefore content) of the Viewer window.
        /// </summary>
        public async Task RefreshCharacters()
        {
            _objOutputGeneratorCancellationTokenSource?.Cancel(false);
            _objRefresherCancellationTokenSource?.Cancel(false);
            _objRefresherCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskRefresher?.IsCompleted == false)
                    await _tskRefresher;
            }
            catch (TaskCanceledException) { }
            _tskRefresher = Task.Run(RefreshCharacterXml, _objRefresherCancellationTokenSource.Token);
        }

        /// <summary>
        /// Asynchronously update the sheet of the Viewer window.
        /// </summary>
        private async Task RefreshSheet()
        {
            _objOutputGeneratorCancellationTokenSource?.Cancel(false);
            _objOutputGeneratorCancellationTokenSource = new CancellationTokenSource();
            try
            {
                if (_tskOutputGenerator?.IsCompleted == false)
                    await _tskOutputGenerator;
            }
            catch (TaskCanceledException) { }
            _tskOutputGenerator = Task.Run(AsyncGenerateOutput, _objOutputGeneratorCancellationTokenSource.Token);
        }

        /// <summary>
        /// Update the internal XML of the Viewer window.
        /// </summary>
        private async Task RefreshCharacterXml()
        {
            using (new CursorWait(this, true))
            {
                await Task.WhenAll(this.DoThreadSafeAsync(() =>
                    {
                        tsPrintPreview.Enabled = false;
                        tsSaveAsHtml.Enabled = false;
                    }),
                    cmdPrint.DoThreadSafeAsync(() => cmdPrint.Enabled = false),
                    cmdSaveAsPdf.DoThreadSafeAsync(() => cmdSaveAsPdf.Enabled = false));
                _objCharacterXml = _lstCharacters.Count > 0
                    ? CommonFunctions.GenerateCharactersExportXml(_objPrintCulture, _strPrintLanguage,
                        _objRefresherCancellationTokenSource.Token, _lstCharacters.ToArray())
                    : null;
                await this.DoThreadSafeAsync(() => tsSaveAsXml.Enabled = _objCharacterXml != null);
                if (_objRefresherCancellationTokenSource.IsCancellationRequested)
                    return;
                await RefreshSheet();
            }
        }

        /// <summary>
        /// Run the generated XML file through the XSL transformation engine to create the file output.
        /// </summary>
        private async Task AsyncGenerateOutput()
        {
            using (new CursorWait(this))
            {
                await Task.WhenAll(this.DoThreadSafeAsync(() =>
                    {
                        tsPrintPreview.Enabled = false;
                        tsSaveAsHtml.Enabled = false;
                    }),
                    cmdPrint.DoThreadSafeAsync(() => cmdPrint.Enabled = false),
                    cmdSaveAsPdf.DoThreadSafeAsync(() => cmdSaveAsPdf.Enabled = false));
                await SetDocumentText(LanguageManager.GetString("String_Generating_Sheet"));
                string strXslPath = Path.Combine(Utils.GetStartupPath, "sheets", _strSelectedSheet + ".xsl");
                if (!File.Exists(strXslPath))
                {
                    string strReturn = "File not found when attempting to load " + _strSelectedSheet +
                                       Environment.NewLine;
                    Log.Debug(strReturn);
                    Program.MainForm.ShowMessageBox(this, strReturn);
                    return;
                }
#if DEBUG
                XslCompiledTransform objXslTransform = new XslCompiledTransform(true);
#else
                XslCompiledTransform objXslTransform = new XslCompiledTransform();
#endif
                try
                {
                    objXslTransform.Load(strXslPath);
                }
                catch (Exception ex)
                {
                    string strReturn = "Error attempting to load " + _strSelectedSheet + Environment.NewLine;
                    Log.Debug(strReturn);
                    Log.Error("ERROR Message = " + ex.Message);
                    strReturn += ex.Message;
                    Program.MainForm.ShowMessageBox(this, strReturn);
                    return;
                }

                if (_objOutputGeneratorCancellationTokenSource.IsCancellationRequested)
                    return;

                using (MemoryStream objStream = new MemoryStream())
                {
                    using (XmlTextWriter objWriter = new XmlTextWriter(objStream, Encoding.UTF8))
                    {
                        objXslTransform.Transform(_objCharacterXml, objWriter);
                        if (_objOutputGeneratorCancellationTokenSource.IsCancellationRequested)
                            return;

                        objStream.Position = 0;

                        // This reads from a static file, outputs to an HTML file, then has the browser read from that file. For debugging purposes.
                        //objXSLTransform.Transform("D:\\temp\\print.xml", "D:\\temp\\output.htm");
                        //webBrowser1.Navigate("D:\\temp\\output.htm");

                        if (!GlobalSettings.PrintToFileFirst)
                        {
                            // Populate the browser using DocumentText (DocumentStream would cause issues due to stream disposal).
                            using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            {
                                string strOutput = await objReader.ReadToEndAsync();
                                await this.DoThreadSafeAsync(() => UseWaitCursor = true);
                                await webViewer.DoThreadSafeAsync(() => webViewer.DocumentText = strOutput);
                            }
                        }
                        else
                        {
                            // The DocumentStream method fails when using Wine, so we'll instead dump everything out a temporary HTML file, have the WebBrowser load that, then delete the temporary file.
                            // Read in the resulting code and pass it to the browser.

                            using (StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true))
                            {
                                string strOutput = await objReader.ReadToEndAsync();
                                File.WriteAllText(_strFilePathName, strOutput);
                            }

                            await this.DoThreadSafeAsync(() => UseWaitCursor = true);
                            await webViewer.DoThreadSafeAsync(() => webViewer.Url = new Uri("file:///" + _strFilePathName));

                            if (GlobalSettings.PrintToFileFirst)
                            {
                                Utils.SafeDeleteFile(_strFilePathName, true);
                            }
                        }
                    }
                }
            }
        }

        private async void webViewer_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            await this.DoThreadSafeAsync(() => UseWaitCursor = false);
            if (_tskOutputGenerator?.IsCompleted == true && _tskRefresher?.IsCompleted == true)
            {
                await Task.WhenAll(this.DoThreadSafeAsync(() =>
                    {
                        tsPrintPreview.Enabled = true;
                        tsSaveAsHtml.Enabled = true;
                    }),
                    cmdPrint.DoThreadSafeAsync(() => cmdPrint.Enabled = true),
                    cmdSaveAsPdf.DoThreadSafeAsync(() => cmdSaveAsPdf.Enabled = true));
            }
        }

        private bool DoPdfPrinterShortcut(string strPdfPrinterName)
        {
            // We've got a proper, built-in PDF printer, so let's use that instead of wkhtmltopdf
            string strOldHeader = null;
            string strOldFooter = null;
            string strOldPrintBackground = null;
            string strOldShrinkToFit = null;
            string strOldDefaultPrinter = null;
            try
            {
                strOldDefaultPrinter = NativeMethods.GetDefaultPrinter();
                // Try to remove headers and footers from the printer and set default printer settings to be conducive to sheet printing
                try
                {
                    using (RegistryKey objKey =
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\PageSetup", true))
                    {
                        if (objKey != null)
                        {
                            strOldHeader = objKey.GetValue("header")?.ToString();
                            objKey.SetValue("header", string.Empty);
                            strOldFooter = objKey.GetValue("footer")?.ToString();
                            objKey.SetValue("footer", string.Empty);
                            strOldPrintBackground = objKey.GetValue("Print_Background")?.ToString();
                            objKey.SetValue("Print_Background", "yes");
                            strOldShrinkToFit = objKey.GetValue("Shrink_To_Fit")?.ToString();
                            objKey.SetValue("Shrink_To_Fit", "yes");
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }

                // webBrowser can only print to the default printer, so we (temporarily) change it to the PDF printer
                if (NativeMethods.SetDefaultPrinter(strPdfPrinterName))
                {
                    // There is also no way to silently have it print to a PDF, so we have to show the print dialog
                    // and have the user click through, though the PDF printer will be temporarily set as their default
                    webViewer.ShowPrintDialog();
                }
            }
            catch (Exception)
            {
                // Error of some kind occured, proceed to use wkhtmltopdf instead
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(strOldDefaultPrinter))
                    NativeMethods.SetDefaultPrinter(strOldDefaultPrinter);
                // Try to remove headers and footers from the printer and
                try
                {
                    using (RegistryKey objKey =
                        Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Internet Explorer\\PageSetup", true))
                    {
                        if (objKey != null)
                        {
                            if (strOldHeader != null)
                                objKey.SetValue("header", strOldHeader);
                            if (strOldFooter != null)
                                objKey.SetValue("footer", strOldFooter);
                            if (strOldPrintBackground != null)
                                objKey.SetValue("Print_Background", strOldPrintBackground);
                            if (strOldShrinkToFit != null)
                                objKey.SetValue("Shrink_To_Fit", strOldShrinkToFit);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
            }

            return true;
        }

        private void PopulateXsltList()
        {
            List<ListItem> lstFiles = XmlManager.GetXslFilesFromLocalDirectory(cboLanguage.SelectedValue?.ToString() ?? GlobalSettings.DefaultLanguage, _lstCharacters);

            cboXSLT.BeginUpdate();
            cboXSLT.PopulateWithListItems(lstFiles);
            cboXSLT.EndUpdate();
        }

        /// <summary>
        /// Set the XSL sheet that will be selected by default.
        /// </summary>
        public void SetSelectedSheet(string strSheet)
        {
            _strSelectedSheet = strSheet;
            Task.Run(RefreshSheet);
        }

        /// <summary>
        /// Set List of Characters to print.
        /// </summary>
        public void SetCharacters(params Character[] lstCharacters)
        {
            foreach (Character objCharacter in _lstCharacters)
            {
                objCharacter.PropertyChanged -= ObjCharacterOnPropertyChanged;
            }
            _lstCharacters = lstCharacters != null ? new List<Character>(lstCharacters) : new List<Character>(1);
            foreach (Character objCharacter in _lstCharacters)
            {
                objCharacter.PropertyChanged += ObjCharacterOnPropertyChanged;
            }

            bool blnOldLoading = _blnLoading;
            try
            {
                _blnLoading = true;
                // Populate the XSLT list with all of the XSL files found in the sheets directory.
                LanguageManager.PopulateSheetLanguageList(cboLanguage, _strSelectedSheet, _lstCharacters);
                PopulateXsltList();
                Task.Run(RefreshCharacters);
            }
            finally
            {
                _blnLoading = blnOldLoading;
            }
        }

        private void ObjCharacterOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Task.Run(RefreshCharacters);
        }

        #endregion Methods
    }
}
