using BruteGDStringEditor;
using System;
using System.Windows.Forms;

namespace BGDSE_GUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        GlobalDataStringEditor Editor;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            byte[] Script = System.IO.File.ReadAllBytes(openFileDialog1.FileName);
            Editor = new GlobalDataStringEditor(Script);
            string[] Strs = Editor.Import();
            listBox1.Items.Clear();
            foreach (string str in Strs)
                listBox1.Items.Add(str.Replace("\n", "\\n"));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                Text = "ID: " + i + "/" + listBox1.Items.Count;
                textBox1.Text = listBox1.Items[i].ToString();
            }
            catch { }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                try {
                    listBox1.Items[listBox1.SelectedIndex] = textBox1.Text;
                }
                catch { }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e) {
            string[] Strs = new string[listBox1.Items.Count];
            for (int i = 0; i < Strs.Length; i++)
                Strs[i] = listBox1.Items[i].ToString().Replace("\\n", "\n");
            byte[] Script = Editor.Export(Strs);
            System.IO.File.WriteAllBytes(saveFileDialog1.FileName, Script);
            MessageBox.Show("File Saved.", "VNX+", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
