using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using TopSolid.Kernel.Automating;

namespace Beispiel3
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.DefaultExt = ".top";
            ofd.Filter = "TopSolid'Design (*.top)|*.top";
            if (ofd.ShowDialog().Value)
                filenameBox.Text = ofd.FileName;
        }

        private int GetImporter(string name)
        {
            int imp_count = TopSolidHost.Application.ImporterCount;
            int i = 0;
            while (i < imp_count)
            {
                if (TopSolidHost.Application.IsImporterValid(i))
                {
                    string fileType;
                    string[] extensions;
                    TopSolidHost.Application.GetImporterFileType(i, out fileType, out extensions);
                    if (fileType.Contains(name))
                        return i;
                }
                i++;
            }
            return -1;
        }

        private void import_Click(object sender, RoutedEventArgs eve)
        {
            TopSolidHost.Connect();

            var lib_id = TopSolidHost.Pdm.SearchDocumentByUniversalId(PdmObjectId.Empty,
                "Geislinger", "TopToolLibrary");

            int imp_id = GetImporter("TopSolid'Design");

            if (imp_id >= 0)
            {
                List<string> log;
                List<DocumentId> bad_ids;

                // Importieren mit den Standardeinstellungen
                var good_ids = TopSolidHost.Documents.Import(imp_id, filenameBox.Text, lib_id, out log, out bad_ids);

                // Mögliche Optionen eines Importers abfragen
                // var importOptions = TopSolidHost.Application.GetImporterOptions(imp_id);

                //var importerOptions = new List<KeyValue>()
                //{
                //    new KeyValue()
                //    {
                //        Key = "ASSEMBLY_DOCUMENT_EXTENSION",
                //        Value = ".TopPrt"
                //    }
                //};

                //try
                //{
                //    TopSolidHost.Pdm.EnsureIsDirty(lib_id);
                //    var good_ids = TopSolidHost.Documents.ImportWithOptions(imp_id,
                //    importerOptions,
                //    filenameBox.Text,
                //    lib_id,
                //    out log,
                //    out bad_ids);
                //    foreach (DocumentId doc in good_ids)
                //    {
                //        TopSolidHost.Documents.Save(doc);
                //        TopSolidHost.Documents.Close(doc, false, true);
                //    }
                //}
            }

            TopSolidHost.Disconnect();
        }
    }
}
