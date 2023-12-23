using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vchasno.Base;

namespace VchasnoUI
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Vchasno.Program.TestPrih();
            //Vchasno.Program.GetDeal("0f74d300-0a5f-17f1-563e-f76ecfc98d1c");

            //Vchasno.Program.GetDespatchAdvice("0f74fe49-b948-1287-596f-aea01c06d06b");

            //var document = Vchasno.Program.GetOrderFromSQL(1110, out string error);
            //var fileXml = Vchasno.Program.CreateXmlFile<OrderXml>(document, out error);
        }
    }
}
