using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TopSolid.Kernel.Automating;
using System.Linq;
using TopSolid.Cam.NC.Kernel.Automating;

namespace Beispiel2
{
    class PdmItem
    {
        public string Name { get; set; }
        public PdmObjectId Id;

        public PdmItem(PdmObjectId _id)
        {
            Id = _id;
            Name = TopSolidHost.Pdm.GetName(_id);
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopSolidHost.Connect();
            TopSolidCamHost.Connect();

        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            TopSolidCamHost.Disconnect();
            TopSolidHost.Disconnect();
        }

        private void projectBox_Loaded(object sender, RoutedEventArgs e)
        {
            var project_list = TopSolidHost.Pdm.GetProjects(true, true);
            foreach (PdmObjectId id in project_list)
                projectBox.Items.Add(new PdmItem(id));
            projectBox.SelectedIndex = 0;
        }

        private void AddToColl(PdmObjectId id, ItemCollection items)
        {
            List<PdmObjectId> folderList, documentList;
            TopSolidHost.Pdm.GetConstituents(id, out folderList, out documentList);
            foreach (PdmObjectId did in documentList)
                if (TopSolidCamHost.Documents.IsCam(TopSolidHost.Documents.GetDocument(did)))
                    items.Add(new PdmItem(did));
            foreach (PdmObjectId fid in folderList)
                AddToColl(fid, items);
        }

        private void projectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gew_projekt = projectBox.SelectedItem as PdmItem;
            documentBox.Items.Clear();
            AddToColl(gew_projekt.Id, documentBox.Items);
        }

        private void documentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (documentBox.SelectedItem != null)
            {
                var doc_id = TopSolidHost.Documents.GetDocument((documentBox.SelectedItem as PdmItem).Id);
                var elts = TopSolidHost.Elements.GetElements(doc_id);

                var environElts = elts.Where(
                    (elt) => TopSolidHost.Elements.GetTypeFullName(elt) == "TopSolid.Cam.NC.Kernel.DB.Entities.Part.EnvironmentEntity").Select(
                    (elt) => TopSolidHost.Elements.GetFriendlyName(elt));

                var assyElts = elts.Where(
                    (elt) => TopSolidHost.Elements.GetTypeFullName(elt) == "TopSolid.Cad.Design.DB.AssemblyEntity").Select(
                    (elt) => TopSolidHost.Elements.GetFriendlyName(elt));

                coscomInfo.Text = $"Einbezogene Elemente:\n{assyElts.Aggregate("", (ass, elt) => $"{ass}\n{elt}")}\n\nWerkstückumgebung:\n{environElts.Aggregate("", (ass, elt) => $"{ass}\n{elt}")}";
            }
        }


    }
}
