﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.R.Host.Client;

namespace ApiTest {
    public partial class RHostApiTestForm : Form {
        private IRHostSession _session;
        private Task _sessionStartTask;
        private Control _currentDisplay;

        public RHostApiTestForm() {
            InitializeComponent();
            CreateHostSession();
        }

        private void CreateHostSession() {
            _session = RHostSession.Create("Test");
            _sessionStartTask = _session.StartHostAsync(new RHostSessionCallback());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            _session?.StopHostAsync();
        }

        private async void executeButton_Click(object sender, EventArgs e) {
            try {
                await _sessionStartTask;
                var result = await _session.ExecuteAndOutputAsync(codeBox.Text);
                DisplayResult(new TextBox() { Text = result.Output });
            } catch (Exception ex) {
                DisplayResult(ex.Message);
            }
        }

        private async void plotButton_Click(object sender, EventArgs e) {
            try {
                await _sessionStartTask;
                var data = await _session.PlotAsync(codeBox.Text, 640, 480, 96);
                var pb = new PictureBox();
                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                pb.Image = new Bitmap(new MemoryStream(data));
                DisplayResult(pb);
            } catch (Exception ex) {
                DisplayResult(ex.Message);
            }
        }

        private async void getListButton_Click(object sender, EventArgs e) {
            try {
                await _sessionStartTask;
                var list = await _session.GetListAsync(codeBox.Text);
                DisplayResult(list);
            } catch (Exception ex) {
                DisplayResult(ex.Message);
            }
        }

        private async void getDataframe_Click(object sender, EventArgs e) {
            try {
                await _sessionStartTask;
                var df = await _session.GetDataFrameAsync(codeBox.Text);
                DisplayResult(df);
            } catch (Exception ex) {
                DisplayResult(ex.Message);
            }
        }

        private void DisplayResult(Control control) {
            if(_currentDisplay != null) {
                _currentDisplay.Parent = null;
                _currentDisplay.Dispose();
            }

            if (control != null) {
                control.Width = resultBox.Width;
                control.Height = resultBox.Height;
                control.Parent = resultBox;
            }
            _currentDisplay = control;
        }

        private void DisplayResult(IEnumerable<object> collection) {
            var sb = new StringBuilder();
            foreach (var e in collection) {
                sb.AppendLine(e.ToString());
            }
            DisplayResult(sb.ToString());
        }


        private void DisplayResult(DataFrame df) {
            var table = new DataTable();
            int i = 1;

            foreach (var name in df.ColumnNames) {
                var col = new DataColumn(string.IsNullOrEmpty(name) ? i.ToString() : name);
                table.Columns.Add(col);
                i++;
            }

            for (int rnum = 0; rnum < df.RowNames.Count; rnum++) {
                var row = table.NewRow();
                for (int cnum = 0; cnum < df.ColumnNames.Count; cnum++) {
                    var colName = df.ColumnNames[cnum];
                    row[colName] = df.Data[cnum][rnum];
                }
                table.Rows.Add(row);
            }

            var ds = new DataSet();
            ds.Tables.Add(table);

            var grid = new DataGrid();
            grid.DataSource = ds;
            grid.DataMember = "Table1";

            DisplayResult(grid);
        }

        private void DisplayResult(string result) {
            DisplayResult(CreateTextBox(result));
        }

        private TextBox CreateTextBox(string text = null) {
            var pb = new TextBox();
            pb.Multiline = true;
            pb.ReadOnly = true;
            pb.Text = text;
            return pb;
        }
    }
}
