/*  Tool for performing model manager tasks in Enterprise Architect for the IEC CIM standard
    Copyright (C) 2022  Martin E. Miller

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CIMModelManager
{
    internal partial class DoesNotInheritFromIdentifiedObject : Form
    {
        public EA.Repository m_Repository;
        private SortedList<MissingDescriptionInfo, MissingDescriptionInfo> m_SortedDescriptions = new SortedList<MissingDescriptionInfo, MissingDescriptionInfo>();
        private ObservableCollection<MissingDescriptionInfo> m_observabledescsriptions = new ObservableCollection<MissingDescriptionInfo>();
        private const string c_IdentifiedObject = "IdentifiedObject";

        public DoesNotInheritFromIdentifiedObject() : base()
        {
            InitializeComponent();
        }

        public void Calculate()
        {
            int counter = 0;
            foreach (EA.IDualPackage p in m_Repository.Models)
            {
                counter += CountPackages(p, false);
            }
            progressBar1.Maximum = counter;
            System.Windows.Forms.Application.UseWaitCursor = true;
            this.Text = "Classes not inheriting from IdentifiedObject";
            foreach (EA.IDualPackage p in m_Repository.Models)
            {
                RecursePackages(p, string.Empty, false);
            }
            lblCurrentClass.Text = string.Empty;
            foreach (var x in m_SortedDescriptions.Values)
            {
                m_observabledescsriptions.Add(x);
            }
            dataGridView1.DataSource = m_observabledescsriptions;
            System.Windows.Forms.Application.UseWaitCursor = false;
        }

        private int CountPackages(EA.IDualPackage ParentPackage, bool includeInformative)
        {
            int counter = 0;
            if (includeInformative || !ParentPackage.Name.StartsWith("Inf"))
            {
                counter++;
                foreach (EA.IDualPackage child in ParentPackage.Packages)
                {
                    counter += CountPackages(child, includeInformative);
                }
            }
            return counter;
        }

        private void RecursePackages(EA.IDualPackage ParentPackage, string path, bool includeInformative)
        {
            lblCurrentClass.Text = $"Searching Package {ParentPackage.Name}...";
            System.Windows.Forms.Application.DoEvents();
            if (includeInformative || !ParentPackage.Name.StartsWith("Inf"))
            {
                progressBar1.Increment(1);
                System.Windows.Forms.Application.DoEvents();
                string newpath = path + "/" + ParentPackage.Name;

                foreach (EA.IDualElement e in ParentPackage.Elements)
                {
                    if (e.Type == "Class"
                        && e.FQStereotype != "CIMDatatype"
                        && e.FQStereotype != "Primitive"
                        && e.FQStereotype != "Compound"
                        && e.FQStereotype != "EAUML::enumeration"
                        && e.FQStereotype != "enumeration"
                        )
                    {
                        lblCurrentClass.Text = $"Searching Package {ParentPackage.Name}, Class {e.Name}...";
                        System.Windows.Forms.Application.DoEvents();
                        if (RecursiveSearch(e))
                            continue;

                        MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                        {
                            Name = e.Name,
                            Path = newpath,
                            Type = "Class " + e.FQStereotype
                        };
                        try { m_SortedDescriptions.Add(newinfo, newinfo); } catch { }
                    }
                }

                foreach (EA.IDualPackage child in ParentPackage.Packages)
                {
                    RecursePackages(child, newpath, includeInformative);
                }
            }
        }

        private bool RecursiveSearch(EA.IDualElement e)
        {

            if (e.Name == c_IdentifiedObject)
                return true;

            foreach (EA.IDualConnector c in e.Connectors)
            {
                if (c.Type == "Generalization")
                {
                    EA.IDualConnectorEnd cce = c.ClientEnd;
                    EA.IDualElement targetClient = m_Repository.GetElementByID(c.ClientID);
                    EA.IDualElement targetSource = m_Repository.GetElementByID(c.SupplierID);
                    if (targetClient.Name == e.Name)
                        return (RecursiveSearch(targetSource));
                }
            }
            return false;
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.AutoUpgradeEnabled = true;
            saveFileDialog1.CheckFileExists = false;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.CreatePrompt = false;
            saveFileDialog1.DefaultExt = ".csv";
            saveFileDialog1.DereferenceLinks = true;
            saveFileDialog1.Filter = "Spreadsheet files (*.csv)|.csv";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.SupportMultiDottedExtensions = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.Windows.Forms.Application.UseWaitCursor = true;
                using (var outputFile = new System.IO.StreamWriter(saveFileDialog1.FileName))
                {
                    outputFile.WriteLine("Does Not Inherit From IdentifiedObject");
                    outputFile.WriteLine("Type,Path,Name");
                    int i = 0;
                    foreach (var x in m_SortedDescriptions.Values)
                    {
                        i++;
                        outputFile.Write("\"" + x.Type?.Replace("\"", "\"\"") + "\",");
                        outputFile.Write("\"" + x.Path?.Replace("\"", "\"\"") + "\",");
                        outputFile.WriteLine("\"" + x.Name?.Replace("\"", "\"\"") + "\"");
                    }
                }
                System.Windows.Forms.Application.UseWaitCursor = false;
                this.Close();
            }
        }
    }
}