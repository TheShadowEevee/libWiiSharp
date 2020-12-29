// Decompiled with JetBrains decompiler
// Type: libWiiSharp.HexView
// Assembly: libWiiSharp, Version=0.4.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FBF36F3D-B5D6-481F-B5F5-1BD3C19E13B2
// Assembly location: C:\Users\theso\Downloads\NCPatcher\pack\libWiiSharp.dll

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace libWiiSharp
{
  public static class HexView
  {
    private static string savedValue;

    public static void DumpToListView(byte[] data, ListView listView) => HexView.dumpToListView(data, listView);

    public static void DumpToDataGridView(byte[] data, DataGridView dataGridView) => HexView.dumpToDataGridView(data, dataGridView);

    public static byte[] DumpFromDataGridView(DataGridView dataGridView) => HexView.dumpFromDataGridView(dataGridView);

    public static void DumpToRichTextBox(byte[] data, RichTextBox richTextBox)
    {
      richTextBox.Clear();
      richTextBox.Font = new Font("Courier New", 9f);
      richTextBox.ReadOnly = true;
      richTextBox.Text = HexView.DumpAsString(data);
    }

    public static void DumpToTextBox(byte[] data, TextBox textBox)
    {
      textBox.Multiline = true;
      textBox.Font = new Font("Courier New", 9f);
      textBox.ReadOnly = true;
      textBox.Text = HexView.DumpAsString(data).Replace("\n", "\r\n");
    }

    public static string[] DumpAsStringArray(byte[] data) => HexView.dumpAsStringArray(data);

    public static string DumpAsString(byte[] data) => string.Join("\n", HexView.dumpAsStringArray(data));

    public static void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
      try
      {
        DataGridView dataGridView = sender as DataGridView;
        if (dataGridView.Columns[e.ColumnIndex].HeaderText.ToLower() == "dump")
        {
          string str = (string) dataGridView.Rows[e.RowIndex].Cells[17].Value;
          if (!(str != HexView.savedValue))
            return;
          if (str.Length != 16)
            throw new Exception();
          for (int index = 0; index < 16; ++index)
          {
            if ((int) HexView.toAscii(byte.Parse((string) dataGridView.Rows[e.RowIndex].Cells[index + 1].Value, NumberStyles.HexNumber)) != (int) str[index])
              dataGridView.Rows[e.RowIndex].Cells[index + 1].Value = (object) HexView.fromAscii(str[index]).ToString("x2");
          }
        }
        else
        {
          if (((string) dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length == 1)
            dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = (object) ("0" + dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString());
          int startIndex = int.Parse(dataGridView.Columns[e.ColumnIndex].HeaderText, NumberStyles.HexNumber);
          string str = ((string) dataGridView.Rows[e.RowIndex].Cells[17].Value).Remove(startIndex, 1).Insert(startIndex, HexView.toAscii(byte.Parse((string) dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value, NumberStyles.HexNumber)).ToString());
          dataGridView.Rows[e.RowIndex].Cells[17].Value = (object) str;
          if (((string) dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length <= 2)
            return;
          dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = (object) ((string) dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Remove(0, ((string) dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value).Length - 2);
        }
      }
      catch
      {
        ((DataGridView) sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = (object) HexView.savedValue;
      }
    }

    public static void DataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) => HexView.savedValue = (string) ((DataGridView) sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

    private static string[] dumpAsStringArray(byte[] data)
    {
      List<string> stringList = new List<string>();
      string empty = string.Empty;
      int num;
      char ascii;
      for (num = 0; (double) (data.Length - num) / 16.0 >= 1.0; num += 16)
      {
        string str1 = string.Empty + num.ToString("x8") + "   ";
        string str2 = string.Empty;
        for (int index = 0; index < 16; ++index)
        {
          str1 = str1 + data[num + index].ToString("x2") + " ";
          string str3 = str2;
          ascii = HexView.toAscii(data[num + index]);
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
            ascii = HexView.toAscii(data[num + index]);
            string str4 = ascii.ToString();
            str2 = str3 + str4;
          }
          else
            str1 += "   ";
        }
        string str5 = str1 + "  " + str2;
        stringList.Add(str5);
      }
      return stringList.ToArray();
    }

    private static byte[] dumpFromDataGridView(DataGridView dataGridView)
    {
      try
      {
        List<byte> byteList = new List<byte>();
        for (int index1 = 0; !string.IsNullOrEmpty((string) dataGridView.Rows[index1].Cells[1].Value); ++index1)
        {
          for (int index2 = 0; index2 < 16; ++index2)
          {
            if (!string.IsNullOrEmpty((string) dataGridView.Rows[index1].Cells[index2 + 1].Value))
              byteList.Add(byte.Parse((string) dataGridView.Rows[index1].Cells[index2 + 1].Value, NumberStyles.HexNumber));
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

    private static void dumpToDataGridView(byte[] data, DataGridView dataGridView)
    {
      dataGridView.Columns.Clear();
      dataGridView.Rows.Clear();
      dataGridView.Font = new Font("Courier New", 9f);
      dataGridView.Columns.Add(new DataGridViewColumn()
      {
        HeaderText = "Offset",
        Width = 80,
        CellTemplate = (DataGridViewCell) new DataGridViewTextBoxCell()
      });
      for (int index = 0; index < 16; ++index)
        dataGridView.Columns.Add(new DataGridViewColumn()
        {
          HeaderText = index.ToString("x1"),
          Width = 30,
          CellTemplate = (DataGridViewCell) new DataGridViewTextBoxCell()
        });
      dataGridView.Columns.Add(new DataGridViewColumn()
      {
        HeaderText = "Dump",
        Width = 125,
        CellTemplate = (DataGridViewCell) new DataGridViewTextBoxCell()
      });
      int num;
      for (num = 0; (double) (data.Length - num) / 16.0 >= 1.0; num += 16)
      {
        DataGridViewRow dataGridViewRow = new DataGridViewRow();
        int index1 = dataGridViewRow.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
        dataGridViewRow.Cells[index1].Value = (object) num.ToString("x8");
        dataGridViewRow.Cells[index1].ReadOnly = true;
        string empty = string.Empty;
        for (int index2 = 0; index2 < 16; ++index2)
        {
          int index3 = dataGridViewRow.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
          dataGridViewRow.Cells[index3].Value = (object) data[num + index2].ToString("x2");
          empty += HexView.toAscii(data[num + index2]).ToString();
        }
        int index4 = dataGridViewRow.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
        dataGridViewRow.Cells[index4].Value = (object) empty;
        dataGridView.Rows.Add(dataGridViewRow);
      }
      if (data.Length <= num)
        return;
      DataGridViewRow dataGridViewRow1 = new DataGridViewRow();
      int index5 = dataGridViewRow1.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
      dataGridViewRow1.Cells[index5].Value = (object) num.ToString("x8");
      dataGridViewRow1.Cells[index5].ReadOnly = true;
      string empty1 = string.Empty;
      for (int index1 = 0; index1 < 16; ++index1)
      {
        if (index1 < data.Length - num)
        {
          int index2 = dataGridViewRow1.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
          dataGridViewRow1.Cells[index2].Value = (object) data[num + index1].ToString("x2");
          empty1 += HexView.toAscii(data[num + index1]).ToString();
        }
        else
          dataGridViewRow1.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
      }
      int index6 = dataGridViewRow1.Cells.Add((DataGridViewCell) new DataGridViewTextBoxCell());
      dataGridViewRow1.Cells[index6].Value = (object) empty1;
      dataGridView.Rows.Add(dataGridViewRow1);
    }

    private static void dumpToListView(byte[] data, ListView listView)
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
      for (num = 0; (double) (data.Length - num) / 16.0 >= 1.0; num += 16)
      {
        ListViewItem listViewItem = new ListViewItem(num.ToString("x8"));
        string empty = string.Empty;
        for (int index = 0; index < 16; ++index)
        {
          listViewItem.SubItems.Add(data[num + index].ToString("x2"));
          empty += HexView.toAscii(data[num + index]).ToString();
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
          empty1 += HexView.toAscii(data[num + index]).ToString();
        }
        else
          listViewItem1.SubItems.Add(string.Empty);
      }
      listViewItem1.SubItems.Add(empty1);
      listView.Items.Add(listViewItem1);
    }

    private static char toAscii(byte value) => value >= (byte) 32 && value <= (byte) 126 ? (char) value : '.';

    private static byte fromAscii(char value) => (byte) value;
  }
}
