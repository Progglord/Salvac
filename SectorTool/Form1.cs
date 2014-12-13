using DotSpatial.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using DotSpatial.Topology;

namespace SectorTool
{
    public partial class Form1 : Form
    {
        private FeatureSet _featureSet;

        public Form1()
        {
            InitializeComponent();
            NewFeatureSet();
        }

        private void NewFeatureSet()
        {
            _featureSet = new FeatureSet(FeatureType.Line);
            _featureSet.DataTable.Columns.Add(new DataColumn("bottom", typeof(string)));
            _featureSet.DataTable.Columns.Add(new DataColumn("top", typeof(string)));
            _featureSet.DataTable.Columns.Add(new DataColumn("atc", typeof(string)));
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            MatchCollection matches = Regex.Matches(txtInput.Text, @"([N|S])(\d{3}).(\d{2}).(\d{2}).(\d{3})\s([E|W])(\d{3}).(\d{2}).(\d{2}).(\d{3})");
            List<Coordinate> coords = new List<Coordinate>();
            foreach (Match match in matches)
            {
                Coordinate coord = new Coordinate();
                coord.X = ((double.Parse(match.Groups[10].Value) / 60d + double.Parse(match.Groups[9].Value)) / 60d + double.Parse(match.Groups[8].Value)) / 60d + double.Parse(match.Groups[7].Value);
                if (match.Groups[6].Value == "W")
                    coord.X = -coord.X;

                coord.Y = ((double.Parse(match.Groups[5].Value) / 60d + double.Parse(match.Groups[4].Value)) / 60d + double.Parse(match.Groups[3].Value)) / 60d + double.Parse(match.Groups[2].Value);
                if (match.Groups[1].Value == "S")
                    coord.Y = -coord.Y;

                coords.Add(coord);
            }

            List<Coordinate> currentPolygon = new List<Coordinate>();
            currentPolygon.Add(coords[0]);
            int currentPolygonCount = 1;
            for (int i = 1; i < coords.Count; i += 2)
            {
                if (i < 2 || coords[i - 1] == coords[i - 2])
                {
                    currentPolygon.Add(coords[i]);

                    if (i != coords.Count - 1)
                        continue;
                }

                IGeometry geom = null;
                //if (currentPolygon[0] == currentPolygon[currentPolygon.Count - 1])
                //    geom = new Polygon(currentPolygon.ToArray());
                //else
                geom = new LineString(currentPolygon.ToArray());

                IFeature feature = _featureSet.AddFeature(geom);
                feature.DataRow.BeginEdit();
                feature.DataRow["bottom"] = txtBottom.Text;
                feature.DataRow["top"] = txtTop.Text;
                feature.DataRow["atc"] = txtAtc.Text + currentPolygon.ToString();
                feature.DataRow.EndEdit();
                lstFeatures.Items.Add(txtAtc.Text + currentPolygon.ToString());

                currentPolygon.Clear();
                currentPolygonCount++;
                currentPolygon.Add(coords[i - 1]);
                currentPolygon.Add(coords[i]);
            }

            txtInput.Clear();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.AddExtension = true;
            dlg.Filter = "Shapefile | *.shp";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _featureSet.SaveAs(dlg.FileName, true);
                NewFeatureSet();
                lstFeatures.Items.Clear();
            }
        }

    }
}
