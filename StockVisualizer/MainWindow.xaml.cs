//Author: Alexander Dolk
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;

namespace StockVisualizer
{
    public partial class StockVisualizerApp : UserControl, INotifyPropertyChanged
    {
        public SeriesCollection SeriesCollectionMainChart { get; set; }
        public SeriesCollection SeriesCollectionVolumeChart { get; set; }
        private List<string> _labelsMainChart;
        private List<string> _labelsVolumeChart;

        string startDate;
        string endDate;
        string apiKey;
        string stockSymbol;
        string technicalIndicator;

        bool startDateSet;
        bool endDateSet;
        bool apiKeySet;
        bool stockSymbolSet;

        public StockVisualizerApp()
        {
            InitializeComponent();
            SeriesCollectionMainChart = new SeriesCollection();
            LabelsMainChart = new List<string>();
            SeriesCollectionVolumeChart = new SeriesCollection();
            LabelsVolumeChart = new List<string>();
            DataContext = this;
        }

        public List<string> LabelsMainChart
        {
            get { return _labelsMainChart; }
            set
            {
                _labelsMainChart = value;
                OnPropertyChanged("LabelsMainChart");
            }
        }

        public List<string> LabelsVolumeChart
        {
            get { return _labelsVolumeChart; }
            set
            {
                _labelsVolumeChart = value;
                OnPropertyChanged("LabelsVolumeChart");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateAllOnClick(object sender, RoutedEventArgs e)
        {
            //Setting data initially / resetting from earlier updates
            this.startDate = "";
            this.endDate = "";
            this.apiKey = "";
            this.stockSymbol = "";
            this.technicalIndicator = "";
            this.startDateSet = false;
            this.endDateSet= false;
            this.apiKeySet = false;
            this.stockSymbolSet = false;

            //Checking the inputs from the user
            DateTime? submittedStartDate = this.StartDate.SelectedDate;
            if (submittedStartDate.HasValue)
            {
                this.startDate = submittedStartDate.Value.ToString("yyyy-MM-dd");
                this.startDateSet = true;
            }
            DateTime? submittedEndDate = this.EndDate.SelectedDate;
            if (submittedEndDate.HasValue)
            {
                this.endDate = submittedEndDate.Value.ToString("yyyy-MM-dd");
                this.endDateSet = true;
            }
            SecureString? submittedApiKey = API_Key.SecurePassword.Copy();
            if (submittedApiKey.Length > 0)
            {
                this.apiKey = API_Key.Password.ToString();
                this.apiKeySet = true;
            }
            string? submittedStockSymbol = Symbol.Text;
            if (submittedStockSymbol != null && submittedStockSymbol != "")
            {
                this.stockSymbol = Symbol.Text;
                this.stockSymbolSet = true;
            }
            if (TechnicalIndicator.SelectedIndex > -1)
            {
                ComboBoxItem cbi = (ComboBoxItem)TechnicalIndicator.SelectedItem;
                this.technicalIndicator = cbi.Content.ToString();
            }

            //Prompt user to supply all necessary inputs if they have not been provided
            if (startDateSet == false || endDateSet == false || apiKeySet == false ||
                stockSymbolSet == false)
            {
                MessageBox.Show("The necessary inputs have not been provided.", "Missing inputs from user.");
            } else
            {
                SeriesCollectionMainChart.Clear();
                SeriesCollectionVolumeChart.Clear();

                StockDataHandler stockDataHandler = new StockDataHandler(startDate, endDate, apiKey, stockSymbol, technicalIndicator);
                stockDataHandler.RetrieveStockData();
                
                if (stockDataHandler.GetApiResults().Contains("Invalid API call.")){
                    MessageBox.Show("Either API key or stock symbol is invalid.");
                } else
                {
                    stockDataHandler.TransformApiResultsToCorrectOrder();
                    stockDataHandler.CalculateStartIndex();
                    stockDataHandler.CalculateData();

                    SeriesCollectionMainChart.Add(stockDataHandler.GetOhlcSeries());
                    SeriesCollectionVolumeChart.Add(stockDataHandler.GetVolumeSeries());
                    List<string> Labels = stockDataHandler.GetLabels();
                    LabelsMainChart = Labels;
                    LabelsVolumeChart = Labels;


                    //Check technical indicator with case statement and retrieve depending on it
                    switch (technicalIndicator)
                    {
                        case "":
                        case "No technical indicator":
                            break;
                        case "Bollinger bands, 20-day SMA, 2 standard deviations":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetUpperBollingerBand());
                            SeriesCollectionMainChart.Add(stockDataHandler.GetMiddleBollingerBand());
                            SeriesCollectionMainChart.Add(stockDataHandler.GetLowerBollingerBand());
                            break;
                        case "50-day SMA":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetSMA50());
                            break;
                        case "200-day SMA":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetSMA200());
                            break;
                        case "50-day and 200-day SMA":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetSMA50());
                            SeriesCollectionMainChart.Add(stockDataHandler.GetSMA200());
                            break;
                        case "Chandelier Exit (long) 22 day":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetChandelierExitLong());
                            break;
                        case "Chandelier Exit (short) 22 day":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetChandelierExitShort());
                            break;
                        case "Chandelier Exit (long and short) 22 day":
                            SeriesCollectionMainChart.Add(stockDataHandler.GetChandelierExitLong());
                            SeriesCollectionMainChart.Add(stockDataHandler.GetChandelierExitShort());
                            break;
                    }
                }                
            }
        }
    }
}