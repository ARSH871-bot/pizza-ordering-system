using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

      
        private void button1_Click_1(object sender, EventArgs e)
        {
            //Pizza Type Selection Fn1353

            if (radioButton1.Checked == true)
            {
                if (radioButton5.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Normal Crust Small Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("4.00");
                    listView1.Items.Add(item);
         
                }
                else if (radioButton6.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Cheesy Crust Small Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("4.00");
                    listView1.Items.Add(item);
               
                }
                else if (radioButton7.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Sausage Crust Small Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("4.00");
                    listView1.Items.Add(item);
             
                }

            }

            else if (radioButton2.Checked == true)
            {
                if (radioButton5.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Normal Crust Medium Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("7.00");
                    listView1.Items.Add(item);
              
                }
                else if (radioButton6.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Cheesy Crust Medium Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("7.00");
                    listView1.Items.Add(item);
             
                }
                else if (radioButton7.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Sausage Crust Medium Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("7.00");
                    listView1.Items.Add(item);
                
                }
            }

            else if (radioButton3.Checked == true)
            {
                if (radioButton5.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Normal Crust Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("10.00");
                    listView1.Items.Add(item);
              
                }
                else if (radioButton6.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Cheesy Crust Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("10.00");
                    listView1.Items.Add(item);
              
                }
                else if (radioButton7.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Sausage Crust Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("10.00");
                    listView1.Items.Add(item);
            
                }
            }

            else if (radioButton4.Checked == true)
            {
                if (radioButton5.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Normal Crust Extra Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("13.00");
                    listView1.Items.Add(item);
            
                }
                else if (radioButton6.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Cheesy Crust Extra Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("13.00");
                    listView1.Items.Add(item);
               
                }
                else if (radioButton7.Checked == true)
                {
                    ListViewItem item = new ListViewItem("Sausage Crust Extra Large Pizza");
                    item.SubItems.Add("1");
                    item.SubItems.Add("13.00");
                    listView1.Items.Add(item);
              
                }
            }


            //Pizza Topping Selection

            if (checkBox1.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Pepperoni Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
           
            }
            
            if (checkBox2.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Extra Cheese Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
          
            }

            if (checkBox3.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Mushroom Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
         
            }

            if (checkBox4.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Ham Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
             
            }

            if (checkBox5.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Bacon Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
            
            }

            if (checkBox6.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Ground Beef Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
          
            }

            if (checkBox7.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Jalapeno Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
            
            }

            if (checkBox8.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Pineapple Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
       
            }

            if (checkBox9.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Dried Shrimps Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
            
            }

            if (checkBox10.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Anchovies Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
        
            }

            if (checkBox11.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Sun Dried Tomatoes Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
              
            }

            if (checkBox12.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Spinach Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
       
            }


            if (checkBox13.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Roasted Garlic Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
           
            }

            if (checkBox14.Checked == true)
            {
                ListViewItem item = new ListViewItem("  Shredded Chicken Toppings");
                item.SubItems.Add("");
                item.SubItems.Add("0.75");
                listView1.Items.Add(item);
        
            }

            //Drink Selection

            if (checkBox15.Checked == true)
            {
                ListViewItem item = new ListViewItem("Coke - Can");
                item.SubItems.Add(textBox1.Text);
                int qty = Convert.ToInt32(textBox1.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
          
            }

            else
            {
                textBox1.Text = "";
            }

            if (checkBox16.Checked == true)
            {
                ListViewItem item = new ListViewItem("Diet Coke - Can");
                item.SubItems.Add(textBox2.Text);
                int qty = Convert.ToInt32(textBox2.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
             
            }

            else
            {
                textBox2.Text = "";
            }

            if (checkBox17.Checked == true)
            {
                ListViewItem item = new ListViewItem("Iced Tea - Can");
                item.SubItems.Add(textBox3.Text);
                int qty = Convert.ToInt32(textBox3.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
              
            }

            else
            {
                textBox3.Text = "";
            }

            if (checkBox18.Checked == true)
            {
                ListViewItem item = new ListViewItem("Ginger Ale - Can");
                item.SubItems.Add(textBox4.Text);
                int qty = Convert.ToInt32(textBox4.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
            }

            else
            {
                textBox4.Text = "";
            }

            if (checkBox19.Checked == true)
            {
                ListViewItem item = new ListViewItem("Sprite - Can");
                item.SubItems.Add(textBox5.Text);
                int qty = Convert.ToInt32(textBox5.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
            }

            else
            {
                textBox5.Text = "";
            }

            if (checkBox20.Checked == true)
            {
                ListViewItem item = new ListViewItem("Root Beer - Can");
                item.SubItems.Add(textBox6.Text);
                int qty = Convert.ToInt32(textBox6.Text);
                double cost = qty * 1.45;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
            }

            else
            {
                textBox6.Text = "";
            }

            if (checkBox21.Checked == true)
            {
                ListViewItem item = new ListViewItem("Bottled Water");
                item.SubItems.Add(textBox7.Text);
                int qty = Convert.ToInt32(textBox7.Text);
                double cost = qty * 1.25;
                string dCost = cost.ToString();
                item.SubItems.Add(dCost);
                listView1.Items.Add(item);
            }

            else
            {
                textBox7.Text = "";
            }

            //Other Items Selection

            if (checkBox22.Checked == true)
            {
                ListViewItem item = new ListViewItem("Chicken Wings");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
          
            }

            if (checkBox23.Checked == true)
            {
                ListViewItem item = new ListViewItem("Poutine");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
             
            }

            if (checkBox24.Checked == true)
            {
                ListViewItem item = new ListViewItem("Onion Rings");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
               
            }

            if (checkBox25.Checked == true)
            {
                ListViewItem item = new ListViewItem("Cheesy Garlic Bread");
                item.SubItems.Add("");
                item.SubItems.Add("3.00");
                listView1.Items.Add(item);
          
            }

            if (checkBox26.Checked == true)
            {
                ListViewItem item = new ListViewItem("Garlic Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
          
            }


            if (checkBox27.Checked == true)
            {
                ListViewItem item = new ListViewItem("BBQ Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
              
            }

            if (checkBox28.Checked == true)
            {
                ListViewItem item = new ListViewItem("Sour Cream Dip");
                item.SubItems.Add("");
                item.SubItems.Add("0.00");
                listView1.Items.Add(item);
             
            }

  
            double total = 0;
            double hst = 0;
            double totaldue = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                total += Convert.ToDouble(item.SubItems[2].Text);
            }

            hst = total * 0.13;
            totaldue = hst + total;

            string hstDisplay = hst.ToString("c2");
            string totalDisplay = totaldue.ToString("c2");
            string amount = total.ToString("c2");
            
            textBox8.Text = amount;
            textBox9.Text = hstDisplay;
            textBox10.Text = totalDisplay;

            tabControl1.SelectTab("tabPage2");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage1");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage3");
            textBox19.Text = textBox10.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            radioButton1.Checked = true;
            radioButton5.Checked = true;
            textBox8.Enabled = false;
            textBox9.Enabled = false;
            textBox10.Enabled = false;
            textBox19.Enabled = false;
            textBox21.Enabled = false;
            
            comboBox1.Items.Add("Alberta");
            comboBox1.Items.Add("British Columbia");
            comboBox1.Items.Add("Manitoba");
            comboBox1.Items.Add("New Brunswick");
            comboBox1.Items.Add("Newfoundland and Labrador");
            comboBox1.Items.Add("Ontario");
            comboBox1.Items.Add("Prince Edward Island");
            comboBox1.Items.Add("Quebec");
            comboBox1.Items.Add("Saskatchewan");

            comboBox2.Items.Add("Cash");
            comboBox2.Items.Add("Credit Card");
            comboBox2.Items.Add("Debit Card");
            comboBox2.Items.Add("Promo Card");

            button8.Enabled = false;

        }

      
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void textBox7_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8)
            {
                e.Handled = true;
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";

        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox20_KeyPress(object sender, KeyPressEventArgs e)
        {
            char q = e.KeyChar;
            if (!Char.IsDigit(q) && q != 8 && q != 46)
            {
                e.Handled = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tabPage2");
        }

        private void button7_Click(object sender, EventArgs e)
        {


            if (textBox11.Text == "" || textBox12.Text == "" || textBox13.Text == "" || textBox15.Text == "" || textBox20.Text == "" || comboBox2.Text == "")
            {
                MessageBox.Show("Please fill in required fields");
            }
    
            else
            {
                string money = textBox19.Text;
                char[] dollars = { '$' };
                string paymoney = money.TrimStart(dollars);
                double paymentDue = Convert.ToDouble(paymoney);
                double amountPaid = Convert.ToDouble(textBox20.Text);
                double change = 0;
                change = amountPaid - paymentDue;
                textBox21.Text = change.ToString("c2");

                if (change < 0)
                {
                    MessageBox.Show("Please pay your balance");

                }

                else
                {
                    button8.Enabled = true;
                }
                  

            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
           DialogResult dialog = MessageBox.Show("Thanks for ordering at Pizza Express. Your ordered items will be ready and delivered in 30 minutes. Do you want to order some more?", "Exit", MessageBoxButtons.YesNo);

           if (dialog == DialogResult.Yes)
           {

               //Clearing all data
               checkBox1.Checked = false;
               checkBox2.Checked = false;
               checkBox3.Checked = false;
               checkBox4.Checked = false;
               checkBox5.Checked = false;
               checkBox6.Checked = false;
               checkBox7.Checked = false;
               checkBox8.Checked = false;
               checkBox9.Checked = false;
               checkBox10.Checked = false;
               checkBox11.Checked = false;
               checkBox12.Checked = false;
               checkBox13.Checked = false;
               checkBox14.Checked = false;
               checkBox15.Checked = false;
               checkBox16.Checked = false;
               checkBox17.Checked = false;
               checkBox18.Checked = false;
               checkBox19.Checked = false;
               checkBox20.Checked = false;
               checkBox21.Checked = false;
               checkBox22.Checked = false;
               checkBox23.Checked = false;
               checkBox24.Checked = false;
               checkBox25.Checked = false;
               checkBox26.Checked = false;
               checkBox27.Checked = false;
               checkBox28.Checked = false;

               textBox1.Text = "";
               textBox2.Text = "";
               textBox3.Text = "";
               textBox4.Text = "";
               textBox5.Text = "";
               textBox6.Text = "";
               textBox7.Text = "";

               listView1.Items.Clear();
               textBox8.Text = "";
               textBox9.Text = "";
               textBox10.Text = "";

               textBox11.Text = "";
               textBox12.Text = "";
               textBox13.Text = "";
               textBox14.Text = "";
               textBox15.Text = "";
               textBox16.Text = "";
               textBox17.Text = "";
               textBox18.Text = "";
               textBox19.Text = "";
               textBox20.Text = "";
               textBox21.Text = "";
               comboBox1.Text = "";
               comboBox2.Text = "";

               tabControl1.SelectTab("tabPage1");
           }

           else if(dialog == DialogResult.No)
           {
               this.Close();
           }
        
        
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.Text == "Cash")
            {
                textBox18.Enabled = false;
            }
        }
    }
} 
//cpfn
