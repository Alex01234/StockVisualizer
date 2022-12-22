//Author: Alexander Dolk

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using System.Windows.Media;
using System.Windows;

namespace StockVisualizer
{
    public class StockDataHandler
    {
        private string startDateAsString;
        private string endDateAsString;
        private string apiKey;
        private string stockSymbol;
        private string technicalIndicator;
        private OhlcSeries ohlcSeries;
        private LineSeries volumeSeries;
        private List<string> labels;
        private LineSeries SMA50;
        private LineSeries SMA200;
        private LineSeries bollingerBandUpper;
        private LineSeries bollingerBandMiddle;
        private LineSeries bollingerBandLower;
        private LineSeries chandelierExitLong;
        private LineSeries chandelierExitShort;

        private string apiResults;
        private List<string> apiResultsCorrectOrder;
        private int startDateIndex;

        public StockDataHandler(string startDateAsString, string endDateAsString, string apiKey, string stockSymbol, string technicalIndicator)
        {
            this.startDateAsString = startDateAsString;
            this.endDateAsString = endDateAsString;
            this.apiKey = apiKey;
            this.stockSymbol = stockSymbol;
            this.technicalIndicator = technicalIndicator;
            this.ohlcSeries = new OhlcSeries() { Title = "OHLC prices", Values = new ChartValues<OhlcPoint> { } };
            this.volumeSeries = new LineSeries() { Title = "Trading Volume", Values = new ChartValues<double> { }, Fill = Brushes.Black, PointGeometry = null };
            this.labels = new List<string>();
            this.SMA50 = new LineSeries { Title = "50-day SMA", Values = new ChartValues<double> { }, Stroke = Brushes.Green, Fill = Brushes.Transparent, PointGeometry = null };
            this.SMA200 = new LineSeries { Title = "200-day SMA", Values = new ChartValues<double> { }, Stroke = Brushes.Blue, Fill = Brushes.Transparent, PointGeometry = null };
            this.bollingerBandUpper = new LineSeries { Title = "Upper Bollinger Band", Values = new ChartValues<double> { }, Stroke = Brushes.Blue, Fill = Brushes.Transparent, PointGeometry = null };
            this.bollingerBandMiddle = new LineSeries { Title = "Middle Bollinger Band", Values = new ChartValues<double> { }, Stroke = Brushes.Red, Fill = Brushes.Transparent, PointGeometry = null };
            this.bollingerBandLower = new LineSeries { Title = "Lower Bollinger Band", Values = new ChartValues<double> { }, Stroke = Brushes.Blue, Fill = Brushes.Transparent, PointGeometry = null };
            this.chandelierExitLong = new LineSeries { Title = "Chandelier Exit (long) 22 day", Values = new ChartValues<double> { }, Stroke = Brushes.Blue, Fill = Brushes.Transparent, PointGeometry = null };
            this.chandelierExitShort = new LineSeries { Title = "Chandelier Exit (short) 22 day", Values = new ChartValues<double> { }, Stroke = Brushes.Orange, Fill = Brushes.Transparent, PointGeometry = null };
        }

        public void RetrieveStockData()
        {
            try
            {
                string query = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&outputsize=full&symbol=" + this.stockSymbol + "&apikey=" + this.apiKey;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                this.apiResults = streamReader.ReadToEnd();
                streamReader.Close();

            } catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + ", please try again.");
            }
        }

        public void TransformApiResultsToCorrectOrder()
        {
            try
            {
                string apiResultOnlyTimeSeries = JObject.Parse(this.apiResults)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects = JsonConvert.DeserializeObject(apiResultOnlyTimeSeries);
                this.apiResultsCorrectOrder = new List<string>();

                //Since apiResultOnlyTimeSeriesAsObjects contains the data in reverse order, we need to put the data into correct order
                foreach (Object timeSeriesObject in apiResultOnlyTimeSeriesAsObjects)
                {
                    this.apiResultsCorrectOrder.Insert(0, timeSeriesObject.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + ", please try again.");
            }
        }

        public void CalculateStartIndex()
        {
            try
            {
                //We need to find the index of the start date in the series, in order to know when to start storing data for the simple moving average
                bool startDateIndexFound = false;
                this.startDateIndex = 0;
                foreach (string timeSeriesObjectAsString in this.apiResultsCorrectOrder)
                {
                    if (startDateIndexFound)
                    {
                        break;
                    }
                    if (!startDateIndexFound)
                    {
                        int firstCommaIndex = timeSeriesObjectAsString.IndexOf(':');
                        string dateAsString = timeSeriesObjectAsString.Substring(0, firstCommaIndex);
                        dateAsString = dateAsString.Replace("\"", ""); //2002-05-24
                        DateTime date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime startDate = DateTime.ParseExact(this.startDateAsString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        /*If the current date is the start date we are looking for, or we have passed the start date (because it is missing from the time series (e.g. holiday))*/
                        if (dateAsString == this.startDateAsString || DateTime.Compare(date, startDate) > 0)
                        {
                            this.startDateIndex = apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString);
                            startDateIndexFound = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + ", please try again.");
            }
        }

        public void CalculateData()
        {
            try
            {
                List<double> bollingerBandCalculationSeries = new List<double>();
                List<double> SMA50calculationSeries = new List<double>();
                List<double> SMA200calculationSeries = new List<double>();

                List<double> chandelierExit_highs = new List<double>();
                List<double> chandelierExit_lows = new List<double>();
                List<double> chandelierExit_TRs = new List<double>();
                double lastClose = 0;
                double lastATR = 0;


                //Loop through the results from the API call
                foreach (string timeSeriesObjectAsString in this.apiResultsCorrectOrder)
                {
                    int firstCommaIndex = timeSeriesObjectAsString.IndexOf(':');
                    string dateAsString = timeSeriesObjectAsString.Substring(0, firstCommaIndex);
                    dateAsString = dateAsString.Replace("\"", ""); //2002-05-24
                    string stockDetails = timeSeriesObjectAsString.Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    double open = (double)stockDetailsAsObject["1. open"]; //24,99
                    double high = (double)stockDetailsAsObject["2. high"]; //24,99
                    double low = (double)stockDetailsAsObject["3. low"]; //23,96
                    double close = (double)stockDetailsAsObject["4. close"]; //24,15
                    double volume = (int)stockDetailsAsObject["6. volume"]; //2967400

                    DateTime date = DateTime.ParseExact(dateAsString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime startDate = DateTime.ParseExact(this.startDateAsString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime endDate = DateTime.ParseExact(this.endDateAsString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                    switch (this.technicalIndicator)
                    {
                        case "": case "No technical indicator":
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);
                            }
                            break;
                        case "Bollinger bands, 20-day SMA, 2 standard deviations":
                            //Add the closing prices the 20 days before the start date
                            if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 20 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 1)
                            {
                                bollingerBandCalculationSeries.Add(close);
                            }
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);

                                double movingAverage = bollingerBandCalculationSeries.Sum() / 20;
                                this.bollingerBandMiddle.Values.Add(movingAverage);

                                //Calculate standard deviation of last 20 days
                                double last20closingPricesMinusMovingAverageSquared = 0;
                                foreach (double d in bollingerBandCalculationSeries)
                                {
                                    last20closingPricesMinusMovingAverageSquared += Math.Pow(d - movingAverage, 2);
                                }
                                double last20closingPricesMinusMovingAverageSquaredDividedByN = last20closingPricesMinusMovingAverageSquared / 20;
                                double standardDeviation = Math.Sqrt(last20closingPricesMinusMovingAverageSquaredDividedByN);

                                this.bollingerBandUpper.Values.Add(movingAverage + (standardDeviation * 2));
                                this.bollingerBandLower.Values.Add(movingAverage - (standardDeviation * 2));

                                bollingerBandCalculationSeries.RemoveAt(0);
                                bollingerBandCalculationSeries.Add(close);
                            }

                            break;
                        case "50-day SMA":
                            if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 50 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 1)
                            {
                                SMA50calculationSeries.Add(close);
                            }
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);
                                this.SMA50.Values.Add(SMA50calculationSeries.Sum() / 50);
                                SMA50calculationSeries.RemoveAt(0);
                                SMA50calculationSeries.Add(close);
                            }
                            break;
                        case "200-day SMA":
                            if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 200 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 1)
                            {
                                SMA200calculationSeries.Add(close);
                            }
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);
                                this.SMA200.Values.Add(SMA200calculationSeries.Sum() / 200);
                                SMA200calculationSeries.RemoveAt(0);
                                SMA200calculationSeries.Add(close);
                            }
                            break;
                        case "50-day and 200-day SMA":
                            if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 50 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 1)
                            {
                                SMA50calculationSeries.Add(close);
                            }
                            if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 200 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 1)
                            {
                                SMA200calculationSeries.Add(close);
                            }
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);
                                this.SMA50.Values.Add(SMA50calculationSeries.Sum() / 50);
                                SMA50calculationSeries.RemoveAt(0);
                                SMA50calculationSeries.Add(close);
                                this.SMA200.Values.Add(SMA200calculationSeries.Sum() / 200);
                                SMA200calculationSeries.RemoveAt(0);
                                SMA200calculationSeries.Add(close);
                            }
                            break;
                        case "Chandelier Exit (long) 22 day": 
                        case "Chandelier Exit (short) 22 day":
                        case "Chandelier Exit (long and short) 22 day":
                            if (this.startDateIndex - this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) == 24)
                            {
                                //Setting lastClose for the first time
                                lastClose = close;
                            }
                                if (this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) >= this.startDateIndex - 23 && this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) <= this.startDateIndex - 2)
                            {
                                //Add TRs for the 22 days leading up to the day before the startDateIndex

                                chandelierExit_highs.Add(high);
                                chandelierExit_lows.Add(low);
                                chandelierExit_TRs.Add(CalculateTR(high, low, lastClose));
                                lastClose= close;
                            }
                            if (this.startDateIndex - this.apiResultsCorrectOrder.IndexOf(timeSeriesObjectAsString) == 1)
                            {
                                //Calculate ATR for the first time
                                lastATR = (1.0/22.0)*(chandelierExit_TRs.Sum());

                                chandelierExit_highs.RemoveAt(0);
                                chandelierExit_lows.RemoveAt(0);
                                chandelierExit_highs.Add(high);
                                chandelierExit_lows.Add(low);
                                lastClose = close;

                            }
                            if ((DateTime.Compare(date, startDate) == 0 || DateTime.Compare(date, startDate) > 0) && (DateTime.Compare(date, endDate) == 0 || DateTime.Compare(date, endDate) < 0))
                            {
                                double TR = CalculateTR(high, low, lastClose);
                                double ATR = ((lastATR * (22 - 1)) + TR) / 22;

                                double chandelierExitLong = chandelierExit_highs.Max() - (ATR * 3);
                                double chandelierExitShort = chandelierExit_lows.Min() + (ATR * 3);

                                AddValueToOhlcSeriesAndDateLabel(open, high, low, close, volume, dateAsString);

                                switch (this.technicalIndicator)
                                {
                                    case "Chandelier Exit (long) 22 day":
                                        this.chandelierExitLong.Values.Add(chandelierExitLong);
                                        break;
                                    case "Chandelier Exit (short) 22 day":
                                        this.chandelierExitShort.Values.Add(chandelierExitShort);
                                        break;
                                    case "Chandelier Exit (long and short) 22 day":
                                        this.chandelierExitLong.Values.Add(chandelierExitLong);
                                        this.chandelierExitShort.Values.Add(chandelierExitShort);
                                        break;
                                }

                                chandelierExit_highs.RemoveAt(0);
                                chandelierExit_lows.RemoveAt(0);
                                chandelierExit_highs.Add(high);
                                chandelierExit_lows.Add(low);

                                lastClose = close;
                                lastATR = ATR;
                            }
                            break;
                    }//switch
                }//loop
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + ", please try again.");
            }
        }

        private double CalculateTR(double todaysHigh, double todaysLow, double yesterdaysClose)
        {
            double todaysHighMinusTodaysLow = todaysHigh - todaysLow;
            double absoluteVauleOfTodaysHighMinusYesterdaysClose = Math.Abs(todaysHigh - yesterdaysClose);
            double absoluteValueOfTodaysLowMinusYesterdaysclose = Math.Abs(todaysLow - yesterdaysClose);
            //Return the greatest of the three
            return Math.Max(todaysHighMinusTodaysLow, Math.Max(absoluteVauleOfTodaysHighMinusYesterdaysClose, absoluteValueOfTodaysLowMinusYesterdaysclose));
        }

        private void AddValueToOhlcSeriesAndDateLabel(double open, double high, double low, double close, double volume, string dateAsString)
        {
            try
            {
                this.ohlcSeries.Values.Add(new OhlcPoint(open, high, low, close));
                this.volumeSeries.Values.Add(volume);
                this.labels.Add(dateAsString);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message + ", please try again.");
            }
        }

        public string GetApiResults()
        {
            return this.apiResults;
        }

        public void SetApiResults(string apiResults)
        {
            this.apiResults = apiResults;
        }

        public List<string> GetApiResultsCorrectOrder()
        {
            return this.apiResultsCorrectOrder;
        }

        public int GetStartDateIndex()
        {
            return this.startDateIndex;
        }

        public OhlcSeries GetOhlcSeries()
        {
            return this.ohlcSeries;
        }
        public LineSeries GetVolumeSeries()
        {
            return this.volumeSeries;
        }
        public List<string> GetLabels()
        {
            return this.labels;
        }
        public LineSeries GetUpperBollingerBand()
        {
            return this.bollingerBandUpper;
        }
        public LineSeries GetMiddleBollingerBand()
        {
            return this.bollingerBandMiddle;
        }
        public LineSeries GetLowerBollingerBand()
        {
            return this.bollingerBandLower;
        }
        public LineSeries GetSMA50()
        {
            return this.SMA50;
        }
        public LineSeries GetSMA200()
        {
            return this.SMA200;
        }

        public LineSeries GetChandelierExitLong()
        {
            return this.chandelierExitLong;
        }

        public LineSeries GetChandelierExitShort()
        {
            return this.chandelierExitShort;
        }
    }
}
