using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TopSolid.Kernel.Automating;

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

    public class ParameterItem
    {
        string _friendlyName;
        string _value;
        string _unit;
        string _int_name;
        DocumentId _doc_id;

        ParameterType _type;

        public string Name
        {
            get
            {
                return _friendlyName;
            }
        }

        public string Unit
        {
            get
            {
                return _unit;
            }
        }

        /*
         * Das Problem hier war, daß wir uns die ElementId des Parameters gemerkt haben.
         * Wenn wir ein Dokument ändern, wird dieses aber ausgecheckt und es entsteht ein
         * neues Dokument und das hat dann neue ElementIds. Wir können also nicht
         * davon ausgehen, da0 die ElementId immer gleich bleibt.
         * Wir suchen daher jetzt den Parameter mit dem Namen.
         */

        public string Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
                TopSolidHost.Documents.Open(ref _doc_id);
                if (!TopSolidHost.Application.StartModification("Editing parameter", true))
                    return;
                try
                {
                    TopSolidHost.Documents.EnsureIsDirty(ref _doc_id);
                    ElementId el = TopSolidHost.Elements.SearchByName(_doc_id, _int_name);
                    switch (_type)
                    {
                        case ParameterType.Boolean:
                            TopSolidHost.Parameters.SetBooleanValue(el, value == "true");
                            break;
                        case ParameterType.Integer:
                            TopSolidHost.Parameters.SetIntegerValue(el, int.Parse(value));
                            break;
                        case ParameterType.Text:
                            TopSolidHost.Parameters.SetTextValue(el, value);
                            break;
                        case ParameterType.Real:
                            TopSolidHost.Parameters.SetRealValue(el, double.Parse(value));
                            break;
                    }
                    TopSolidHost.Application.EndModification(true, true);
                }
                catch (Exception)
                {
                    TopSolidHost.Application.EndModification(false, false);
                }
                TopSolidHost.Documents.Close(_doc_id, false, true);
            }
        }

        public ParameterItem(ElementId elem, DocumentId doc_id)
        {
            _int_name = TopSolidHost.Elements.GetName(elem);
            _doc_id = doc_id;
            _friendlyName = TopSolidHost.Elements.GetFriendlyName(elem);
            _type = TopSolidHost.Parameters.GetParameterType(elem);
            switch (_type)
            {
                case ParameterType.Boolean:
                    _value = TopSolidHost.Parameters.GetBooleanValue(elem).ToString();
                    _unit = "";
                    break;
                case ParameterType.Text:
                    _value = TopSolidHost.Parameters.GetTextValue(elem);
                    _unit = "";
                    break;
                case ParameterType.Integer:
                    _value = TopSolidHost.Parameters.GetIntegerValue(elem).ToString();
                    _unit = "";
                    break;
                case ParameterType.Real:
                    UnitType u;
                    _value = TopSolidHost.Parameters.GetRealValue(elem).ToString();
                    string uni;
                    TopSolidHost.Parameters.GetRealUnit(elem, out u, out uni);
                    _unit = uni;
                    break;
            }
        }
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ParameterItem> plist { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TopSolidHost.Connect();
            plist = new ObservableCollection<ParameterItem>();
            paramGrid.DataContext = plist;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
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
                items.Add(new PdmItem(did));
            foreach (PdmObjectId fid in folderList)
                AddToColl(fid, items);
        }

        private void projectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var gew_projekt = projectBox.SelectedItem as PdmItem;
            documentBox.Items.Clear();
            AddToColl(gew_projekt.Id, documentBox.Items);
            button.IsEnabled = false;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var gew_dok = documentBox.SelectedItem as PdmItem;
            if (gew_dok != null)
            {
                var doc_id = TopSolidHost.Documents.GetDocument(gew_dok.Id);
                TopSolidHost.Documents.Open(ref doc_id);
            }
        }

        private void documentBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            button.IsEnabled = documentBox.SelectedItem != null;

            var gew_dok = documentBox.SelectedItem as PdmItem;
            if (gew_dok != null)
            {
                var doc_id = TopSolidHost.Documents.GetDocument(gew_dok.Id);
                var list_param = TopSolidHost.Parameters.GetParameters(doc_id);
                plist.Clear();
                foreach (ElementId elem in list_param)
                    if (TopSolidHost.Elements.IsModifiable(elem))
                    {
                        var item = new ParameterItem(elem, doc_id);
                        if (item.Value != "")
                            plist.Add(item);
                    }
            }
        }

        private int GetExporter(string name)
        {
            int exp_count = TopSolidHost.Application.ExporterCount;
            int i = 0;
            while (i < exp_count)
            {
                if (TopSolidHost.Application.IsExporterValid(i))
                {
                    string fileType;
                    string[] extensions;
                    TopSolidHost.Application.GetExporterFileType(i, out fileType, out extensions);
                    if (fileType.Contains(name))
                        return i;
                }
                i++;
            }
            return -1;
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            var gew_dok = documentBox.SelectedItem as PdmItem;
            if (gew_dok != null)
            {
                var doc_id = TopSolidHost.Documents.GetDocument(gew_dok.Id);
                var exp_id = GetExporter("Parasolid");
                if (exp_id >= 0)
                    if (TopSolidHost.Documents.CanExport(exp_id, doc_id))
                    {
                        var sfd = new SaveFileDialog();
                        sfd.DefaultExt = ".x_t";
                        sfd.Filter = "Parasolid files (*.x_t)|*.x_t";
                        if (sfd.ShowDialog().Value)
                            TopSolidHost.Documents.Export(exp_id, doc_id, sfd.FileName);
                    }
            }
        }

    }
}
