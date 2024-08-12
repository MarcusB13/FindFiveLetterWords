using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FiveLetterWordFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FiveLetterWordCLass finder;
        private int combinations = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            solutions.Content = "Solutions: ";
            progressbar.Value = 0;
            combinations = 0;
            timerLabel.Content = "";

            finder = new FiveLetterWordCLass(
                file.Text,
                int.Parse(amount_of_words.Text),
                int.Parse(amount_of_letters.Text)
            );

            finder.ProgressChanged += UpdateProgress;
            finder.TotalFoundChanged += UpdateSolutions;
            finder.UpdateTimer += UpdateTimer;
            await Task.Run(() => finder.Start());
        }

        private void UpdateProgress(int progress, int total)
        {
            Dispatcher.Invoke(() => progressbar.Value = (double)progress / total * 100);
        }

        private void UpdateSolutions(int combinationsNumber) 
        {
            if (combinationsNumber > combinations)
            {
                combinations = combinationsNumber;
                Dispatcher.Invoke(() => solutions.Content = "Solutions: " + combinations.ToString());
            }
        }

        private void UpdateTimer(Decimal seconds)
        {
            Dispatcher.Invoke(() => timerLabel.Content = "Time " + seconds.ToString());
        }
    }
}