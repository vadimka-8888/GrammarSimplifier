using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace GrammarSimplifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            combo.Items.Add("-");
            string[] files = Directory.GetFiles("Files");
            foreach (string file in files)
            {
                combo.Items.Add(file.Substring(6));
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GrammarReformer GR = new GrammarReformer();
            if (combo.SelectedItem != combo.Items[0])
            {
                GR.Read($"./Files/{(string)combo.SelectedItem}");
                l1.Items.Add(GR.ToString());
                GR.RemoveUslessSymbols();
                l2.Items.Add(GR.ToString());
            }
            else 
            b1_Click(null, null);
        }

        private void b1_Click(object sender, RoutedEventArgs e)
        {
            l1.Items.Clear();
            l2.Items.Clear();
            combo.SelectedItem = combo.Items[0];
        }
    }
}
