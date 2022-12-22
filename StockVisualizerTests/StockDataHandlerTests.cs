//Author: Alexander Dolk

using LiveCharts.Defaults;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Windows.Themes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Media3D;

namespace StockVisualizerTests
{
    [TestClass]
    public class StockDataHandlerTests
    {
        [TestMethod]
        public void RetrieveStockData_DataRetrievedSuccessfully()
        {            
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string startDate = "2022-01-01";
                string endDate = "2022-06-01";
                string apiKey = ""; //INSERT YOUR API KEY TO ALPHA VANTAGE
                string stockSymbol = "BRK.A";
                string technicalIndicator = "";
                StockDataHandler stockDataHandler = new StockDataHandler(startDate, endDate, apiKey, stockSymbol, technicalIndicator);

                //ACT
                stockDataHandler.RetrieveStockData();

                //ASSERT
                Assert.IsFalse(stockDataHandler.GetApiResults().Contains("Either API key or stock symbol is invalid."));
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void RetrieveStockData_InvalidAPICall()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string startDate = "2022-01-01";
                string endDate = "2022-06-01";
                string apiKey = ""; //INSERT YOUR API KEY TO ALPHA VANTAGE
                string stockSymbol = "XXX";
                string technicalIndicator = "";
                StockDataHandler stockDataHandler = new StockDataHandler(startDate, endDate, apiKey, stockSymbol, technicalIndicator);

                //ACT
                stockDataHandler.RetrieveStockData();

                //ASSERT
                Assert.IsTrue(stockDataHandler.GetApiResults().Contains("Either API key or stock symbol is invalid."));
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

        }
        [TestMethod]
        public void TransformApiResultsToCorrectOrder_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\TransformApiResultsToCorrectOrder_Success_input.txt");
                StockDataHandler sdh = new StockDataHandler("", "", "", "", "");
                sdh.SetApiResults(API_results);

                //ACT
                sdh.TransformApiResultsToCorrectOrder();

                string firstObj = sdh.GetApiResultsCorrectOrder().ElementAt(0);
                int firstCommaIndex = firstObj.IndexOf(':');
                string firstDateAsString = firstObj.Substring(0, firstCommaIndex);
                firstDateAsString = firstDateAsString.Replace("\"", "");

                string lastObj = sdh.GetApiResultsCorrectOrder().ElementAt(3);
                int firstCommaIndex2 = lastObj.IndexOf(':');
                string lastDateAsString = lastObj.Substring(0, firstCommaIndex2);
                lastDateAsString = lastDateAsString.Replace("\"", ""); //2002-05-24

                //ASSERT
                Assert.AreEqual("2022-12-12", firstDateAsString);
                Assert.AreEqual("2022-12-15", lastDateAsString);
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

        }
        [TestMethod]
        public void CalculateStartIndex_StartDateInSeries_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateStartIndex_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-05", "", "", "", "");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();

                //ACT
                sdh.CalculateStartIndex();

                //ASSERT
                Assert.AreEqual(2, sdh.GetStartDateIndex());

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void CalculateStartIndex_StartDateNotInSeries_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateStartIndex_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-10", "", "", "", "");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();

                //ACT
                sdh.CalculateStartIndex();

                //ASSERT
                Assert.AreEqual(7, sdh.GetStartDateIndex());

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void CalculateData_NoTechnicalIndicator_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-16", "", "", "");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //ASSERT
                //Check OhlcSeries, VolumeSeries. Check values for 3 separate dates
                //Date: 2022-12-01
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);

                //Date: 2022-12-07
                Assert.AreEqual(461593.4, ((OhlcPoint)sdh.GetOhlcSeries().Values[4]).Close);
                Assert.AreEqual(2890, (double)sdh.GetVolumeSeries().Values[4]);

                //Date: 2022-12-16
                Assert.AreEqual(454620, ((OhlcPoint)sdh.GetOhlcSeries().Values[11]).Close);
                Assert.AreEqual(2990, (double)sdh.GetVolumeSeries().Values[11]);


                /*
                System.Diagnostics.Trace.WriteLine(sdh.GetOhlcSeries().Values.ToString());
                foreach (OhlcPoint p in sdh.GetOhlcSeries().Values)
                {
                    System.Diagnostics.Trace.WriteLine(p.Close);
                }
                OhlcPoint a = (OhlcPoint)sdh.GetOhlcSeries().Values[0];
                double a_c = a.Close;
                System.Diagnostics.Trace.WriteLine(a_c);
                double b = ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close;
                double c = (double)sdh.GetVolumeSeries().Values[0];
                System.Diagnostics.Trace.WriteLine(c);
                */
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }

        [TestMethod]
        public void CalculateData_BollingerBands_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "Bollinger bands, 20-day SMA, 2 standard deviations");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //Manual calculation of technical indicator

                //2022-12-01:
                double period_average_1 = (438699.9 + 428800 + 432000 + 438575.01 + 442100 + 435945 + 457665 + 469047.25 + 466600 + 468232.75 + 465520 +
                463694.99 + 468290 + 468921.76 + 476980 + 477019.99 + 478675.55 + 472712.01 + 475843 + 480280) / 20;
                double sum_of_closing_prices_minus_period_average_squared_1 = (
                Math.Pow((438699.9 - period_average_1), 2) +
                Math.Pow((428800 - period_average_1), 2) +
                Math.Pow((432000 - period_average_1), 2) +
                Math.Pow((438575.01 - period_average_1), 2) +
                Math.Pow((442100 - period_average_1), 2) +
                Math.Pow((435945 - period_average_1), 2) +
                Math.Pow((457665 - period_average_1), 2) +
                Math.Pow((469047.25 - period_average_1), 2) +
                Math.Pow((466600 - period_average_1), 2) +
                Math.Pow((468232.75 - period_average_1), 2) +
                Math.Pow((465520 - period_average_1), 2) +
                Math.Pow((463694.99 - period_average_1), 2) +
                Math.Pow((468290 - period_average_1), 2) +
                Math.Pow((468921.76 - period_average_1), 2) +
                Math.Pow((476980 - period_average_1), 2) +
                Math.Pow((477019.99 - period_average_1), 2) +
                Math.Pow((478675.55 - period_average_1), 2) +
                Math.Pow((472712.01 - period_average_1), 2) +
                Math.Pow((475843 - period_average_1), 2) +
                Math.Pow((480280 - period_average_1), 2));

                double sd_1 = Math.Sqrt(sum_of_closing_prices_minus_period_average_squared_1 / 20);
                double upper_band_1 = period_average_1 + (sd_1 * 2);
                double lower_band_1 = period_average_1 - (sd_1 * 2);

                //2022-12-02:
                double period_average_2 = (428800 + 432000 + 438575.01 + 442100 + 435945 + 457665 + 469047.25 + 466600 + 468232.75 + 465520 +
                463694.99 + 468290 + 468921.76 + 476980 + 477019.99 + 478675.55 + 472712.01 + 475843 + 480280 + 477085) / 20;
                double sum_of_closing_prices_minus_period_average_squared_2 = (
                Math.Pow((428800 - period_average_2), 2) +
                Math.Pow((432000 - period_average_2), 2) +
                Math.Pow((438575.01 - period_average_2), 2) +
                Math.Pow((442100 - period_average_2), 2) +
                Math.Pow((435945 - period_average_2), 2) +
                Math.Pow((457665 - period_average_2), 2) +
                Math.Pow((469047.25 - period_average_2), 2) +
                Math.Pow((466600 - period_average_2), 2) +
                Math.Pow((468232.75 - period_average_2), 2) +
                Math.Pow((465520 - period_average_2), 2) +
                Math.Pow((463694.99 - period_average_2), 2) +
                Math.Pow((468290 - period_average_2), 2) +
                Math.Pow((468921.76 - period_average_2), 2) +
                Math.Pow((476980 - period_average_2), 2) +
                Math.Pow((477019.99 - period_average_2), 2) +
                Math.Pow((478675.55 - period_average_2), 2) +
                Math.Pow((472712.01 - period_average_2), 2) +
                Math.Pow((475843 - period_average_2), 2) +
                Math.Pow((480280 - period_average_2), 2) + 
                Math.Pow((477085 - period_average_2), 2));

                double sd_2 = Math.Sqrt(sum_of_closing_prices_minus_period_average_squared_2 / 20);
                double upper_band_2 = period_average_2 + (sd_2 * 2);
                double lower_band_2 = period_average_2 - (sd_2 * 2);

                //2022-12-05:
                double period_average_3 = (432000 + 438575.01 + 442100 + 435945 + 457665 + 469047.25 + 466600 + 468232.75 + 465520 +
                463694.99 + 468290 + 468921.76 + 476980 + 477019.99 + 478675.55 + 472712.01 + 475843 + 480280 + 477085 + 477403) / 20;
                double sum_of_closing_prices_minus_period_average_squared_3 = (
                Math.Pow((432000 - period_average_3), 2) +
                Math.Pow((438575.01 - period_average_3), 2) +
                Math.Pow((442100 - period_average_3), 2) +
                Math.Pow((435945 - period_average_3), 2) +
                Math.Pow((457665 - period_average_3), 2) +
                Math.Pow((469047.25 - period_average_3), 2) +
                Math.Pow((466600 - period_average_3), 2) +
                Math.Pow((468232.75 - period_average_3), 2) +
                Math.Pow((465520 - period_average_3), 2) +
                Math.Pow((463694.99 - period_average_3), 2) +
                Math.Pow((468290 - period_average_3), 2) +
                Math.Pow((468921.76 - period_average_3), 2) +
                Math.Pow((476980 - period_average_3), 2) +
                Math.Pow((477019.99 - period_average_3), 2) +
                Math.Pow((478675.55 - period_average_3), 2) +
                Math.Pow((472712.01 - period_average_3), 2) +
                Math.Pow((475843 - period_average_3), 2) +
                Math.Pow((480280 - period_average_3), 2) +
                Math.Pow((477085 - period_average_3), 2) + 
                Math.Pow((477403 - period_average_3), 2));

                double sd_3 = Math.Sqrt(sum_of_closing_prices_minus_period_average_squared_3 / 20);
                double upper_band_3 = period_average_3 + (sd_3 * 2);
                double lower_band_3 = period_average_3 - (sd_3 * 2);


                //ASSERT
                //2022-12-01:
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(Math.Round(period_average_1, 3), Math.Round((double)sdh.GetMiddleBollingerBand().Values[0], 3));
                Assert.AreEqual(Math.Round(upper_band_1, 3), Math.Round((double)sdh.GetUpperBollingerBand().Values[0], 3));
                Assert.AreEqual(Math.Round(lower_band_1, 3), Math.Round((double)sdh.GetLowerBollingerBand().Values[0], 3));

                //2022-12-02:
                Assert.AreEqual(477403, ((OhlcPoint)sdh.GetOhlcSeries().Values[1]).Close);
                Assert.AreEqual(2794, (double)sdh.GetVolumeSeries().Values[1]);
                Assert.AreEqual(Math.Round(period_average_2, 3), Math.Round((double)sdh.GetMiddleBollingerBand().Values[1], 3));
                Assert.AreEqual(Math.Round(upper_band_2, 3), Math.Round((double)sdh.GetUpperBollingerBand().Values[1], 3));
                Assert.AreEqual(Math.Round(lower_band_2, 3), Math.Round((double)sdh.GetLowerBollingerBand().Values[1], 3));

                //2022-12-05:
                Assert.AreEqual(468700, ((OhlcPoint)sdh.GetOhlcSeries().Values[2]).Close);
                Assert.AreEqual(4128, (double)sdh.GetVolumeSeries().Values[2]);
                Assert.AreEqual(Math.Round(period_average_3, 3), Math.Round((double)sdh.GetMiddleBollingerBand().Values[2], 3));
                Assert.AreEqual(Math.Round(upper_band_3, 3), Math.Round((double)sdh.GetUpperBollingerBand().Values[2], 3));
                Assert.AreEqual(Math.Round(lower_band_3, 3), Math.Round((double)sdh.GetLowerBollingerBand().Values[2], 3));

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void CalculateData_50daySMA_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_50daySMA_Success_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "50-day SMA");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                //Date: 2022-12-01
                Assert.AreEqual(82.32, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(1950336, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(78.537, Math.Round((double)sdh.GetSMA50().Values[0], 3));

                //Date: 2022-12-02
                Assert.AreEqual(81.94, ((OhlcPoint)sdh.GetOhlcSeries().Values[1]).Close);
                Assert.AreEqual(1620170, (double)sdh.GetVolumeSeries().Values[1]);
                Assert.AreEqual(78.5142, Math.Round((double)sdh.GetSMA50().Values[1], 4));

                //Date: 2022-12-05
                Assert.AreEqual(78.62, ((OhlcPoint)sdh.GetOhlcSeries().Values[2]).Close);
                Assert.AreEqual(1504292, (double)sdh.GetVolumeSeries().Values[2]);
                Assert.AreEqual(78.5376, Math.Round((double)sdh.GetSMA50().Values[2], 4));

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void CalculateData_200daySMA_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "200-day SMA");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //200, 0
                string API_results_check_200_0 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_1.txt");
                string only_time_series_200_0 = JObject.Parse(API_results_check_200_0)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_0 = JsonConvert.DeserializeObject(only_time_series_200_0);
                double sum_200_0 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_0)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_0 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_0 = Math.Round((double)sum_200_0 / 200, 3);

                //200, 1
                string API_results_check_200_1 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_2.txt");
                string only_time_series_200_1 = JObject.Parse(API_results_check_200_1)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_1 = JsonConvert.DeserializeObject(only_time_series_200_1);
                double sum_200_1 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_1)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_1 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_1 = Math.Round((double)sum_200_1 / 200, 3);

                //200, 2
                string API_results_check_200_2 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_3.txt");
                string only_time_series_200_2 = JObject.Parse(API_results_check_200_2)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_2 = JsonConvert.DeserializeObject(only_time_series_200_2);
                double sum_200_2 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_2)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_2 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_2 = Math.Round((double)sum_200_2 / 200, 3);


                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                //Date: 2022-12-01
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(sum_200_0, Math.Round((double)sdh.GetSMA200().Values[0], 3));

                //Date: 2022-12-02
                Assert.AreEqual(477403, ((OhlcPoint)sdh.GetOhlcSeries().Values[1]).Close);
                Assert.AreEqual(2794, (double)sdh.GetVolumeSeries().Values[1]);
                Assert.AreEqual(sum_200_1, Math.Round((double)sdh.GetSMA200().Values[1], 3));

                //Date: 2022-12-05
                Assert.AreEqual(468700, ((OhlcPoint)sdh.GetOhlcSeries().Values[2]).Close);
                Assert.AreEqual(4128, (double)sdh.GetVolumeSeries().Values[2]);
                Assert.AreEqual(sum_200_2, Math.Round((double)sdh.GetSMA200().Values[2], 3));

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        [TestMethod]
        public void CalculateData_50and200daySMA_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "50-day and 200-day SMA");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //50, 0
                string API_results_check_50_0 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_50and200daySMA_Success_50_1.txt");
                string only_time_series_50_0 = JObject.Parse(API_results_check_50_0)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_50_0 = JsonConvert.DeserializeObject(only_time_series_50_0);
                double sum_50_0 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_50_0)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_50_0 += (double)stockDetailsAsObject["4. close"];
                }
                sum_50_0 = Math.Round((double)sum_50_0/50, 3);

                //50, 1
                string API_results_check_50_1 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_50and200daySMA_Success_50_2.txt");
                string only_time_series_50_1 = JObject.Parse(API_results_check_50_1)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_50_1 = JsonConvert.DeserializeObject(only_time_series_50_1);
                double sum_50_1 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_50_1)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_50_1 += (double)stockDetailsAsObject["4. close"];
                }
                sum_50_1 = Math.Round((double)sum_50_1 / 50, 3);

                //50, 2
                string API_results_check_50_2 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_50and200daySMA_Success_50_3.txt");
                string only_time_series_50_2 = JObject.Parse(API_results_check_50_2)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_50_2 = JsonConvert.DeserializeObject(only_time_series_50_2);
                double sum_50_2 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_50_2)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_50_2 += (double)stockDetailsAsObject["4. close"];
                }
                sum_50_2 = Math.Round((double)sum_50_2 / 50, 3);

                //200, 0
                string API_results_check_200_0 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_1.txt");
                string only_time_series_200_0 = JObject.Parse(API_results_check_200_0)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_0 = JsonConvert.DeserializeObject(only_time_series_200_0);
                double sum_200_0 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_0)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_0 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_0 = Math.Round((double)sum_200_0 / 200, 3);

                //200, 1
                string API_results_check_200_1 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_2.txt");
                string only_time_series_200_1 = JObject.Parse(API_results_check_200_1)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_1 = JsonConvert.DeserializeObject(only_time_series_200_1);
                double sum_200_1 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_1)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_1 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_1 = Math.Round((double)sum_200_1 / 200, 3);

                //200, 2
                string API_results_check_200_2 = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_200daySMA_Success_3.txt");
                string only_time_series_200_2 = JObject.Parse(API_results_check_200_2)["Time Series (Daily)"].ToString();
                dynamic apiResultOnlyTimeSeriesAsObjects_200_2 = JsonConvert.DeserializeObject(only_time_series_200_2);
                double sum_200_2 = 0;
                foreach (Object o in apiResultOnlyTimeSeriesAsObjects_200_2)
                {
                    int firstCommaIndex = o.ToString().IndexOf(':');
                    string stockDetails = o.ToString().Substring(firstCommaIndex + 1);
                    JObject stockDetailsAsObject = JObject.Parse(stockDetails);
                    sum_200_2 += (double)stockDetailsAsObject["4. close"];
                }
                sum_200_2 = Math.Round((double)sum_200_2 / 200, 3);


                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                //Date: 2022-12-01
                //SMA50
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(sum_50_0, Math.Round((double)sdh.GetSMA50().Values[0], 3));
                //SMA200
                Assert.AreEqual(sum_200_0, Math.Round((double)sdh.GetSMA200().Values[0], 3));

                //Date: 2022-12-02
                //SMA50
                Assert.AreEqual(477403, ((OhlcPoint)sdh.GetOhlcSeries().Values[1]).Close);
                Assert.AreEqual(2794, (double)sdh.GetVolumeSeries().Values[1]);
                Assert.AreEqual(sum_50_1, Math.Round((double)sdh.GetSMA50().Values[1], 3));
                //SMA200
                Assert.AreEqual(sum_200_1, Math.Round((double)sdh.GetSMA200().Values[1], 3));

                //Date: 2022-12-05
                //SMA50
                Assert.AreEqual(468700, ((OhlcPoint)sdh.GetOhlcSeries().Values[2]).Close);
                Assert.AreEqual(4128, (double)sdh.GetVolumeSeries().Values[2]);
                Assert.AreEqual(sum_50_2, Math.Round((double)sdh.GetSMA50().Values[2], 3));
                //SMA200
                Assert.AreEqual(sum_200_2, Math.Round((double)sdh.GetSMA200().Values[2], 3));
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }

        private double TR_Calculator(double currentHigh, double currentLow, double yesterdaysClose)
        {
            double option_1 = currentHigh - currentLow;
            double option_2 = Math.Abs(currentHigh - yesterdaysClose);
            double option_3 = Math.Abs(currentLow - yesterdaysClose);
            return Math.Max(option_1, Math.Max(option_2, option_3));
        }

        [TestMethod]
        public void CalculateData_ChandelierExitLong_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "Chandelier Exit (long) 22 day");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //Manual calculation of technical indicator
                double n = 22.0;
                double first_ATR = (1 / n) * (
                TR_Calculator(451970.0, 440052.25, 437900) +
                TR_Calculator(450837.75, 444750.0, 451900.0) +
                TR_Calculator(452079.46, 441879.73, 445050.0) +
                TR_Calculator(449942.4988, 437255.0, 443004.0) +
                TR_Calculator(434427.76, 428465.01, 438699.9) +
                TR_Calculator(437249.9988, 426136.0, 428800.0) +
                TR_Calculator(442216.0, 434837.1, 432000.0) +
                TR_Calculator(445435.0, 436311.86, 438575.01) +
                TR_Calculator(444168.0, 434925.0, 442100.0) +
                TR_Calculator(458384.9888, 444918.42, 435945.0) +
                TR_Calculator(471870.0, 456961.77, 457665.0) +
                TR_Calculator(472470.0, 464516.02, 469047.25) +
                TR_Calculator(474482.75, 464625.12, 466600.0) +
                TR_Calculator(471327.75, 463401.03, 468232.75) +
                TR_Calculator(465000.0, 459043.0, 465520.0) +
                TR_Calculator(469642.75, 465395.0, 463694.99) +
                TR_Calculator(471915.34, 464865.0, 468290.0) +
                TR_Calculator(477409.9988, 469217.0, 468921.76) +
                TR_Calculator(477529.9988, 473716.01, 476980.0) +
                TR_Calculator(481580.44, 476281.01, 477019.99) +
                TR_Calculator(478838.7688, 471000.0, 478675.55) +
                TR_Calculator(475970.9588, 468796.01, 472712.01));

                List<double> highs = new List<double>();
                highs.Add(450837.75); //10-31
                highs.Add(452079.46);
                highs.Add(449942.4988);
                highs.Add(434427.76);
                highs.Add(437249.9988);
                highs.Add(442216.0);
                highs.Add(445435.0);
                highs.Add(444168.0);
                highs.Add(458384.9888);
                highs.Add(471870.0);
                highs.Add(472470.0);
                highs.Add(474482.75);
                highs.Add(471327.75);
                highs.Add(465000.0);
                highs.Add(469642.75);
                highs.Add(471915.34);
                highs.Add(477409.9988);
                highs.Add(477529.9988);
                highs.Add(481580.44);
                highs.Add(478838.7688);
                highs.Add(475970.9588);
                highs.Add(480301.575); //11-30

                double high_1 = highs.Max();

                double ATR_12_01 = ((first_ATR * (n - 1.0)) + TR_Calculator(483162.0, 472863.87, 480280.0)) / n;
                double chandelier_exit_long_12_01 = high_1 - (ATR_12_01 * 3);

                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_01, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[0], 3));

                
                highs.RemoveAt(0);
                highs.Add(483162.0);
                double high_2 = highs.Max();
                double ATR_12_02 = ((ATR_12_01 * (n - 1.0)) + TR_Calculator(477526.0, 470680.0, 477085.0)) / n;
                double chandelier_exit_long_12_02 = high_2 - (ATR_12_02 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_02, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[1], 3));
                
                
                highs.RemoveAt(0);
                highs.Add(477526.0);
                double high_5 = highs.Max();
                double ATR_12_05 = ((ATR_12_02 * (n - 1.0)) + TR_Calculator(476689.0, 466160, 477403)) / n;
                double chandelier_exit_long_12_05 = high_5 - (ATR_12_05 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_05, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[2], 3));

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        
        [TestMethod]
        public void CalculateData_ChandelierExitShort_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "Chandelier Exit (short) 22 day");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //Manual calculation of technical indicator
                double n = 22.0;
                double first_ATR = (1 / n) * (
                TR_Calculator(451970.0, 440052.25, 437900) +
                TR_Calculator(450837.75, 444750.0, 451900.0) +
                TR_Calculator(452079.46, 441879.73, 445050.0) +
                TR_Calculator(449942.4988, 437255.0, 443004.0) +
                TR_Calculator(434427.76, 428465.01, 438699.9) +
                TR_Calculator(437249.9988, 426136.0, 428800.0) +
                TR_Calculator(442216.0, 434837.1, 432000.0) +
                TR_Calculator(445435.0, 436311.86, 438575.01) +
                TR_Calculator(444168.0, 434925.0, 442100.0) +
                TR_Calculator(458384.9888, 444918.42, 435945.0) +
                TR_Calculator(471870.0, 456961.77, 457665.0) +
                TR_Calculator(472470.0, 464516.02, 469047.25) +
                TR_Calculator(474482.75, 464625.12, 466600.0) +
                TR_Calculator(471327.75, 463401.03, 468232.75) +
                TR_Calculator(465000.0, 459043.0, 465520.0) +
                TR_Calculator(469642.75, 465395.0, 463694.99) +
                TR_Calculator(471915.34, 464865.0, 468290.0) +
                TR_Calculator(477409.9988, 469217.0, 468921.76) +
                TR_Calculator(477529.9988, 473716.01, 476980.0) +
                TR_Calculator(481580.44, 476281.01, 477019.99) +
                TR_Calculator(478838.7688, 471000.0, 478675.55) +
                TR_Calculator(475970.9588, 468796.01, 472712.01));

                List<double> lows = new List<double>();
                lows.Add(444750.0);
                lows.Add(441879.73);
                lows.Add(437255.0);
                lows.Add(428465.01);
                lows.Add(426136.0);
                lows.Add(434837.1);
                lows.Add(436311.86);
                lows.Add(434925.0);
                lows.Add(444918.42);
                lows.Add(456961.77);
                lows.Add(464516.02);
                lows.Add(464625.12);
                lows.Add(463401.03);
                lows.Add(459043.0);
                lows.Add(465395.0);
                lows.Add(464865.0);
                lows.Add(469217.0);
                lows.Add(473716.01);
                lows.Add(476281.01);
                lows.Add(471000.0);
                lows.Add(468796.01);
                lows.Add(466100.0);

                double low_1 = lows.Min();

                double ATR_12_01 = ((first_ATR * (n - 1.0)) + TR_Calculator(483162.0, 472863.87, 480280.0)) / n;
                double chandelier_exit_short_12_01 = low_1 + (ATR_12_01 * 3);

                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_01, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[0], 3));


                lows.RemoveAt(0);
                lows.Add(472863.87);
                double low_2 = lows.Min();
                double ATR_12_02 = ((ATR_12_01 * (n - 1.0)) + TR_Calculator(477526.0, 470680.0, 477085.0)) / n;
                double chandelier_exit_short_12_02 = low_2 + (ATR_12_02 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_02, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[1], 3));


                lows.RemoveAt(0);
                lows.Add(470680.0);
                double low_5 = lows.Min();
                double ATR_12_05 = ((ATR_12_02 * (n - 1.0)) + TR_Calculator(476689.0, 466160, 477403)) / n;
                double chandelier_exit_short_12_05 = low_5 + (ATR_12_05 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_05, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[2], 3));

            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        
        [TestMethod]
        public void CalculateData_ChandelierExitLongAndShort_Success()
        {
            Thread STAThread = new Thread(() =>
            {
                //ARRANGE
                string API_results = File.ReadAllText("C:\\Users\\Alexander\\source\\repos\\StockVisualizer\\StockVisualizerTests\\test_data\\CalculateData_tests_input.txt");
                StockDataHandler sdh = new StockDataHandler("2022-12-01", "2022-12-05", "", "", "Chandelier Exit (long and short) 22 day");
                sdh.SetApiResults(API_results);
                sdh.TransformApiResultsToCorrectOrder();
                sdh.CalculateStartIndex();

                //ACT
                sdh.CalculateData();

                //Manual calculation of technical indicator
                double n = 22.0;
                double first_ATR = (1 / n) * (
                TR_Calculator(451970.0, 440052.25, 437900) +
                TR_Calculator(450837.75, 444750.0, 451900.0) +
                TR_Calculator(452079.46, 441879.73, 445050.0) +
                TR_Calculator(449942.4988, 437255.0, 443004.0) +
                TR_Calculator(434427.76, 428465.01, 438699.9) +
                TR_Calculator(437249.9988, 426136.0, 428800.0) +
                TR_Calculator(442216.0, 434837.1, 432000.0) +
                TR_Calculator(445435.0, 436311.86, 438575.01) +
                TR_Calculator(444168.0, 434925.0, 442100.0) +
                TR_Calculator(458384.9888, 444918.42, 435945.0) +
                TR_Calculator(471870.0, 456961.77, 457665.0) +
                TR_Calculator(472470.0, 464516.02, 469047.25) +
                TR_Calculator(474482.75, 464625.12, 466600.0) +
                TR_Calculator(471327.75, 463401.03, 468232.75) +
                TR_Calculator(465000.0, 459043.0, 465520.0) +
                TR_Calculator(469642.75, 465395.0, 463694.99) +
                TR_Calculator(471915.34, 464865.0, 468290.0) +
                TR_Calculator(477409.9988, 469217.0, 468921.76) +
                TR_Calculator(477529.9988, 473716.01, 476980.0) +
                TR_Calculator(481580.44, 476281.01, 477019.99) +
                TR_Calculator(478838.7688, 471000.0, 478675.55) +
                TR_Calculator(475970.9588, 468796.01, 472712.01));

                List<double> highs = new List<double>();
                highs.Add(450837.75); //10-31
                highs.Add(452079.46);
                highs.Add(449942.4988);
                highs.Add(434427.76);
                highs.Add(437249.9988);
                highs.Add(442216.0);
                highs.Add(445435.0);
                highs.Add(444168.0);
                highs.Add(458384.9888);
                highs.Add(471870.0);
                highs.Add(472470.0);
                highs.Add(474482.75);
                highs.Add(471327.75);
                highs.Add(465000.0);
                highs.Add(469642.75);
                highs.Add(471915.34);
                highs.Add(477409.9988);
                highs.Add(477529.9988);
                highs.Add(481580.44);
                highs.Add(478838.7688);
                highs.Add(475970.9588);
                highs.Add(480301.575); //11-30

                List<double> lows = new List<double>();
                lows.Add(444750.0);
                lows.Add(441879.73);
                lows.Add(437255.0);
                lows.Add(428465.01);
                lows.Add(426136.0);
                lows.Add(434837.1);
                lows.Add(436311.86);
                lows.Add(434925.0);
                lows.Add(444918.42);
                lows.Add(456961.77);
                lows.Add(464516.02);
                lows.Add(464625.12);
                lows.Add(463401.03);
                lows.Add(459043.0);
                lows.Add(465395.0);
                lows.Add(464865.0);
                lows.Add(469217.0);
                lows.Add(473716.01);
                lows.Add(476281.01);
                lows.Add(471000.0);
                lows.Add(468796.01);
                lows.Add(466100.0);

                
                double high_1 = highs.Max();
                double low_1 = lows.Min();

                double ATR_12_01 = ((first_ATR * (n - 1.0)) + TR_Calculator(483162.0, 472863.87, 480280.0)) / n;
                double chandelier_exit_long_12_01 = high_1 - (ATR_12_01 * 3);
                double chandelier_exit_short_12_01 = low_1 + (ATR_12_01 * 3);

                //ASSERT
                //Check OhlcSeries, VolumeSeries and series for technical indicator. Check values for 3 separate dates
                Assert.AreEqual(477085, ((OhlcPoint)sdh.GetOhlcSeries().Values[0]).Close);
                Assert.AreEqual(2949, (double)sdh.GetVolumeSeries().Values[0]);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_01, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[0], 3));
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_01, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[0], 3));

                highs.RemoveAt(0);
                highs.Add(483162.0);
                lows.RemoveAt(0);
                lows.Add(472863.87);
                double high_2 = highs.Max();
                double low_2 = lows.Min();
                double ATR_12_02 = ((ATR_12_01 * (n - 1.0)) + TR_Calculator(477526.0, 470680.0, 477085.0)) / n;
                double chandelier_exit_long_12_02 = high_2 - (ATR_12_02 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_02, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[1], 3));
                double chandelier_exit_short_12_02 = low_2 + (ATR_12_02 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_02, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[1], 3));

                highs.RemoveAt(0);
                highs.Add(477526.0);
                lows.RemoveAt(0);
                lows.Add(470680.0);
                double high_5 = highs.Max();
                double low_5 = lows.Min();
                double ATR_12_05 = ((ATR_12_02 * (n - 1.0)) + TR_Calculator(476689.0, 466160, 477403)) / n;
                double chandelier_exit_long_12_05 = high_5 - (ATR_12_05 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_long_12_05, 3), Math.Round((double)sdh.GetChandelierExitLong().Values[2], 3));
                double chandelier_exit_short_12_05 = low_5 + (ATR_12_05 * 3);
                Assert.AreEqual(Math.Round(chandelier_exit_short_12_05, 3), Math.Round((double)sdh.GetChandelierExitShort().Values[2], 3));
            });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
        
    }
}