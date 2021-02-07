/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2021 Github Contributors
 * 
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace libWiiSharp
{
    /// <summary>
    /// A static class that provides functions to dump a byte array to view it like in a hex-editor.
    /// In combination with a DataGridView, it's even able to act like a hex-editor.
    /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
    /// </summary>
    public static class HexView
    {
        private static string savedValue;

        #region Public Functions
        /// <summary>
        /// Displays the byte array like a hex editor in a RichTextBox.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="richTextBox"></param>
        public static void DumpToRichTextBox(byte[] data, RichTextBox richTextBox)
        {
            richTextBox.Clear();
            richTextBox.Font = new Font("Courier New", 9f);
            richTextBox.ReadOnly = true;
            richTextBox.Text = DumpAsString(data);
        }

        /// <summary>
        /// Displays the byte array like a hex editor in a TextBox.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="textBox"></param>
        public static void DumpToTextBox(byte[] data, TextBox textBox)
        {
            textBox.Multiline = true;
            textBox.Font = new Font("Courier New", 9f);
            textBox.ReadOnly = true;
            textBox.Text = DumpAsString(data).Replace("\n", "\r\n");
        }

        /// <summary>
        /// Displays the byte array like a hex editor as a string.
        /// Be sure to use "Courier New" as a font, so every char has the same width.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DumpAsString(byte[] data)
        {
            return string.Join("\n", DumpAsStringArray(data));
        }

        /// <summary>
        /// Link your DataGridView's CellEndEdit event with this function.
        /// The dump and byte values will be synchronized.
        /// Don't forget to also link the CellBeginEdit event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView dataGridView = sender as DataGridView;
                if (dataGridView.Columns[e.ColumnIndex].HeaderText.ToLower() == "dump")
                {
                    string str = (string)dataGridView.Rows[e.RowIndex].Cells[17].Value;
                    if (!(str != savedValue))
                    {
                        return;
                    }

                    if (str.Length != 16)
                    {
                        throw new Exception();
                    }

                    for (int index = 0; index < 16; ++index)
                    {
                        if (ToAscii(byte.Parse((string)dataGridView.Rows[e.RowIndex].Cells[index + 1].Value, NumberStyles.HexNumber)) != str[index])
                        {
                            dataGridView.Rows[e.RowIndex].Cells[index + 1].Value = FromAscii(str[index]).ToString("x2");
                        }
                    }
                }
                else
                {
                    if (((string)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length == 1)
                    {
                        dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "0" + dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                    }

                    int startIndex = int.Parse(dataGridView.Columns[e.ColumnIndex].HeaderText, NumberStyles.HexNumber);
                    string str = ((string)dataGridView.Rows[e.RowIndex].Cells[17].Value).Remove(startIndex, 1).Insert(startIndex, ToAscii(byte.Parse((string)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, NumberStyles.HexNumber)).ToString());
                    dataGridView.Rows[e.RowIndex].Cells[17].Value = str;
                    if (((string)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length <= 2)
                    {
                        return;
                    }

                    dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ((string)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Remove(0, ((string)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length - 2);
                }
            }
            catch
            {
                ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = savedValue;
            }
        }

        /// <summary>
        /// Link your DataGridView's CellBeginEdit event with this function.
        /// The dump and byte values will be synchronized.
        /// Don't forget to also link the CellEndEdit event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            savedValue = (string)((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Displays the byte array like a hex editor as a string array.
        /// Be sure to use "Courier New" as a font, so every char has the same width.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string[] DumpAsStringArray(byte[] data)
        {
            List<string> stringList = new List<string>();
            int num;
            char ascii;
            for (num = 0; (data.Length - num) / 16.0 >= 1.0; num += 16)
            {
                string str1 = string.Empty + num.ToString("x8") + "   ";
                string str2 = string.Empty;
                for (int index = 0; index < 16; ++index)
                {
                    str1 = str1 + data[num + index].ToString("x2") + " ";
                    string str3 = str2;
                    ascii = ToAscii(data[num + index]);
                    string str4 = ascii.ToString();
                    str2 = str3 + str4;
                }
                string str5 = str1 + "  " + str2;
                stringList.Add(str5);
            }
            if (data.Length > num)
            {
                string str1 = string.Empty + num.ToString("x8") + "   ";
                string str2 = string.Empty;
                for (int index = 0; index < 16; ++index)
                {
                    if (index < data.Length - num)
                    {
                        str1 = str1 + data[num + index].ToString("x2") + " ";
                        string str3 = str2;
                        ascii = ToAscii(data[num + index]);
                        string str4 = ascii.ToString();
                        str2 = str3 + str4;
                    }
                    else
                    {
                        str1 += "   ";
                    }
                }
                string str5 = str1 + "  " + str2;
                stringList.Add(str5);
            }
            return stringList.ToArray();
        }
        // Unused
        /*
        /// <summary>
        /// Dumps a DataGridView back to a byte array.
        /// The DataGridView must have the right format.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="dataGridView"></param>
        /// <returns></returns>
        private static byte[] DumpFromDataGridView(DataGridView dataGridView)
        {
            try
            {
                List<byte> byteList = new List<byte>();
                for (int index1 = 0; !string.IsNullOrEmpty((string)dataGridView.Rows[index1].Cells[1].Value); ++index1)
                {
                    for (int index2 = 0; index2 < 16; ++index2)
                    {
                        if (!string.IsNullOrEmpty((string)dataGridView.Rows[index1].Cells[index2 + 1].Value))
                            byteList.Add(byte.Parse((string)dataGridView.Rows[index1].Cells[index2 + 1].Value, NumberStyles.HexNumber));
                    }
                    if (index1 == dataGridView.Rows.Count - 1)
                        break;
                }
                return byteList.ToArray();
            }
            catch
            {
                throw new Exception("An error occured. The DataGridView might have the wrong format!");
            }
        }

        /// <summary>
        /// Displays the byte array like a hex editor in a DataGridView.
        /// Columns will be created, estimated width is ~685 px.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataGridView"></param>
        private static void DumpToDataGridView(byte[] data, DataGridView dataGridView)
        {
            dataGridView.Columns.Clear();
            dataGridView.Rows.Clear();
            dataGridView.Font = new Font("Courier New", 9f);
            dataGridView.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "Offset",
                Width = 80,
                CellTemplate = (DataGridViewCell)new DataGridViewTextBoxCell()
            });
            for (int index = 0; index < 16; ++index)
                dataGridView.Columns.Add(new DataGridViewColumn()
                {
                    HeaderText = index.ToString("x1"),
                    Width = 30,
                    CellTemplate = (DataGridViewCell)new DataGridViewTextBoxCell()
                });
            dataGridView.Columns.Add(new DataGridViewColumn()
            {
                HeaderText = "Dump",
                Width = 125,
                CellTemplate = (DataGridViewCell)new DataGridViewTextBoxCell()
            });
            int num;
            for (num = 0; (double)(data.Length - num) / 16.0 >= 1.0; num += 16)
            {
                DataGridViewRow dataGridViewRow = new DataGridViewRow();
                int index1 = dataGridViewRow.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
                dataGridViewRow.Cells[index1].Value = (object)num.ToString("x8");
                dataGridViewRow.Cells[index1].ReadOnly = true;
                string empty = string.Empty;
                for (int index2 = 0; index2 < 16; ++index2)
                {
                    int index3 = dataGridViewRow.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
                    dataGridViewRow.Cells[index3].Value = (object)data[num + index2].ToString("x2");
                    empty += HexView.ToAscii(data[num + index2]).ToString();
                }
                int index4 = dataGridViewRow.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
                dataGridViewRow.Cells[index4].Value = (object)empty;
                dataGridView.Rows.Add(dataGridViewRow);
            }
            if (data.Length <= num)
                return;
            DataGridViewRow dataGridViewRow1 = new DataGridViewRow();
            int index5 = dataGridViewRow1.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
            dataGridViewRow1.Cells[index5].Value = (object)num.ToString("x8");
            dataGridViewRow1.Cells[index5].ReadOnly = true;
            string empty1 = string.Empty;
            for (int index1 = 0; index1 < 16; ++index1)
            {
                if (index1 < data.Length - num)
                {
                    int index2 = dataGridViewRow1.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
                    dataGridViewRow1.Cells[index2].Value = (object)data[num + index1].ToString("x2");
                    empty1 += HexView.ToAscii(data[num + index1]).ToString();
                }
                else
                    dataGridViewRow1.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
            }
            int index6 = dataGridViewRow1.Cells.Add((DataGridViewCell)new DataGridViewTextBoxCell());
            dataGridViewRow1.Cells[index6].Value = (object)empty1;
            dataGridView.Rows.Add(dataGridViewRow1);
        }

        /// <summary>
        /// Displays the byte array like a hex editor in a ListView.
        /// Columns will be created, estimated width is ~685 px.
        /// Big files (25kB ++) will take quite a long time, so don't use this for big files.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="listView"></param>
        private static void DumpToListView(byte[] data, ListView listView)
        {
            listView.Columns.Clear();
            listView.Items.Clear();
            listView.View = View.Details;
            listView.Font = new Font("Courier New", 9f);
            listView.Columns.Add("Offset", "Offset", 80, HorizontalAlignment.Left, string.Empty);
            for (int index = 0; index < 16; ++index)
                listView.Columns.Add(index.ToString("x1"), index.ToString("x1"), 30, HorizontalAlignment.Left, string.Empty);
            listView.Columns.Add("Dump", "Dump", 125, HorizontalAlignment.Left, string.Empty);
            int num;
            for (num = 0; (double)(data.Length - num) / 16.0 >= 1.0; num += 16)
            {
                ListViewItem listViewItem = new ListViewItem(num.ToString("x8"));
                string empty = string.Empty;
                for (int index = 0; index < 16; ++index)
                {
                    listViewItem.SubItems.Add(data[num + index].ToString("x2"));
                    empty += HexView.ToAscii(data[num + index]).ToString();
                }
                listViewItem.SubItems.Add(empty);
                listView.Items.Add(listViewItem);
            }
            if (data.Length <= num)
                return;
            ListViewItem listViewItem1 = new ListViewItem(num.ToString("x8"));
            string empty1 = string.Empty;
            for (int index = 0; index < 16; ++index)
            {
                if (index < data.Length - num)
                {
                    listViewItem1.SubItems.Add(data[num + index].ToString("x2"));
                    empty1 += HexView.ToAscii(data[num + index]).ToString();
                }
                else
                    listViewItem1.SubItems.Add(string.Empty);
            }
            listViewItem1.SubItems.Add(empty1);
            listView.Items.Add(listViewItem1);
        }
        */


        private static char ToAscii(byte value)
        {
            return value >= 32 && value <= 126 ? (char)value : '.';
        }

        private static byte FromAscii(char value)
        {
            return (byte)value;
        }
        #endregion
    }
}