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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CIMModelManager
{
    internal partial class MissingDescriptions : Form
    {
        public EA.Repository m_Repository;
        private SortedList<MissingDescriptionInfo, MissingDescriptionInfo> m_SortedDescriptions = new SortedList<MissingDescriptionInfo, MissingDescriptionInfo>();
        private ObservableCollection<MissingDescriptionInfo> m_observabledescsriptions = new ObservableCollection<MissingDescriptionInfo>();

        public MissingDescriptions() : base()
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
            this.Text = "Missing descriptions.";
            foreach (EA.IDualPackage p in m_Repository.Models)
            {
                RecursePackages(p, string.Empty, false);
            }
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
            if (includeInformative || !ParentPackage.Name.StartsWith("Inf"))
            {
                progressBar1.Increment(1);
                System.Windows.Forms.Application.DoEvents();
                string newpath = path + "/" + ParentPackage.Name;

                if (string.IsNullOrWhiteSpace(ParentPackage.Notes))
                {
                    MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                    {
                        Name = ParentPackage.Name,
                        Path = path,
                        Type = "Package"
                    };
                    m_SortedDescriptions.Add(newinfo, newinfo);
                }

                foreach (EA.IDualElement e in ParentPackage.Elements)
                {
                    if (e.Type == "Object" || e.Type == "Boundary" || e.Type == "Text")
                    { }
                    else //if (e.Type == "Class" || e.Type == "Enumeration")
                    {
                        if (string.IsNullOrWhiteSpace(e.Notes))
                        {
                            MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                            {
                                Name = e.Name,
                                Path = newpath,
                                Type = e.Type
                            };
                            m_SortedDescriptions.Add(newinfo, newinfo);
                        }

                        foreach (EA.IDualAttribute a in e.Attributes)
                        {
                            if (string.IsNullOrWhiteSpace(a.Notes))
                            {
                                MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                                {
                                    Name = a.Name,
                                    Path = newpath + "/" + e.Name,
                                    Type = "Attribute"
                                };
                                m_SortedDescriptions.Add(newinfo, newinfo);
                            }
                        }

                        if (e.Type == "Class")
                        {
                            foreach (EA.IDualConnector c in e.Connectors)
                            {
                                EA.IDualConnectorEnd cce = c.ClientEnd;
                                EA.IDualElement targetc = m_Repository.GetElementByID(c.ClientID);
                                EA.IDualElement targets = m_Repository.GetElementByID(c.SupplierID);
                                if (string.IsNullOrWhiteSpace(cce.RoleNote) && !string.IsNullOrWhiteSpace(cce.Role))
                                {
                                    MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                                    {
                                        Name = cce.Role,
                                        Path = newpath + "/" + targets.Name,
                                        Type = "Role:" + cce.RoleType
                                    };
                                    try { m_SortedDescriptions.Add(newinfo, newinfo); } catch { }
                                }

                                EA.IDualConnectorEnd cse = c.SupplierEnd;
                                if (string.IsNullOrWhiteSpace(cse.RoleNote) && !string.IsNullOrWhiteSpace(cse.Role))
                                {
                                    MissingDescriptionInfo newinfo = new MissingDescriptionInfo()
                                    {
                                        Name = cse.Role,
                                        Path = newpath + "/" + targetc.Name,
                                        Type = "Role:" + cse.RoleType
                                    };
                                    try { m_SortedDescriptions.Add(newinfo, newinfo); } catch { }
                                }
                            }
                        }
                    }
                }

                foreach (EA.IDualPackage child in ParentPackage.Packages)
                {
                    RecursePackages(child, newpath, includeInformative);
                }
            }
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
                    outputFile.WriteLine("Missing Descriptions");
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