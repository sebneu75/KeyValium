using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace KeyValium.Inspector.Controls
{
    public partial class MultiView : UserControl
    {
        public MultiView()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            cmbDisplayAs.Items.Clear();

            foreach (var val in Enum.GetValues(typeof(DisplayType)))
            {
                var name = Enum.GetName(typeof(DisplayType), val);

                cmbDisplayAs.Items.Add(name);
            }

            cmbDisplayAs.SelectedIndex = 0;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get
            {
                return lblTitle.Text;
            }
            set
            {
                lblTitle.Text = value;
            }
        }

        private byte[] _bytes;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
            set
            {
                if (_bytes != value)
                {
                    _bytes = value;
                    UpdateText();
                }
            }
        }

        private void UpdateText()
        {
            var sel = (DisplayType)Enum.Parse(typeof(DisplayType), cmbDisplayAs.SelectedItem as string);

            switch (sel)
            {
                case DisplayType.Hexdump:
                    txtBytes.Text = Display.FormatHexDump(Bytes);
                    break;
                case DisplayType.UTF8:
                    txtBytes.Text = Display.FormatString(Bytes, Encoding.UTF8);
                    break;
                case DisplayType.ANSI:
                    txtBytes.Text = Display.FormatString(Bytes, Encoding.Default);
                    break;
                case DisplayType.ASCII:
                    txtBytes.Text = Display.FormatString(Bytes, Encoding.ASCII);
                    break;
                case DisplayType.UTF16_LE:
                    txtBytes.Text = Display.FormatString(Bytes, Encoding.Unicode);
                    break;
                case DisplayType.UTF16_BE:
                    txtBytes.Text = Display.FormatString(Bytes, Encoding.BigEndianUnicode);
                    break;
                case DisplayType.Integer_LE:
                    txtBytes.Text = Display.FormatInteger(Bytes, false);
                    break;
                case DisplayType.Integer_BE:
                    txtBytes.Text = Display.FormatInteger(Bytes, true);
                    break;
                case DisplayType.JSON:
                    txtBytes.Text = Display.FormatJson(Bytes);
                    break;
                default:
                    txtBytes.Text = "???????";
                    break;
            }
        }

        private void cmbDisplayAs_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        public enum DisplayType
        {
            Hexdump,
            JSON,
            UTF8,
            ANSI,
            ASCII,
            UTF16_LE,
            UTF16_BE,
            Integer_LE,
            Integer_BE
        }
    }
}
