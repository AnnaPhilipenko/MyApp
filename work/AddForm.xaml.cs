using System;
using System.ComponentModel;
using System.Windows;

namespace work
{
    /// <summary>
    /// Interaction logic for AddForm.xaml
    /// </summary>
    public partial class AddForm
    {
        public AddForm()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e) // Добавление плейлиста
        {
            if(textBox.Text != "" && textBox.Text != "Default")
            {
                PlayEntities db = new PlayEntities();
                playlist p = new playlist();
                p.name = textBox.Text; // 
                db.playlist.Add(p);
                db.SaveChanges();
                this.Close();
            }
            else
            {
                MessageBox.Show("Please, enter correct name" , "Error");
            }
        }
    }
       
}
