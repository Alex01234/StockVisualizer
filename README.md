# StockVisualizer
## Overiew
TODO

## Techical indicators
### 50-day Simple Moving Average (SMA)
#### Description and formula
The 50-day SMA is the moving average of the closing prices of the 50 days prior to any given date. In the formula below, *n*=50 <br />
SMA = (A<sub>1</sub> + A<sub>2</sub> + A<sub>3</sub> + ... + A<sub>n</sub>)/*n* <br />
Where: <br />
A<sub>n</sub> = the closing price of an asset at period *n* <br />
*n* = the number of total periods <br />
Source: <br />
https://www.investopedia.com/terms/s/sma.asp
#### Example
![50-day_SMA_sample](https://user-images.githubusercontent.com/39235916/209172299-68df743e-5746-4f82-b8af-628e258192c9.PNG)
---

### 200-day SMA
#### Description and formula
The same formula is used as for 50-day SMA, but *n*=200 <br />
#### Example
![200-day_SMA_sample](https://user-images.githubusercontent.com/39235916/209172390-7dbf703b-90c8-4d66-bf06-fb0a88e0f749.PNG)
---

### 50-day and 200-day SMA
#### Example
![50_and_200-day_SMA_sample](https://user-images.githubusercontent.com/39235916/209172465-1c4c699f-dd8e-4df1-8bc1-277e58a6b5a2.PNG)
---

### Bollinger bands, 20-day SMA, 2 standard deviations
#### Description and formula
Upper band = SMA(20) + (SD x 2) <br />
Lower band = SMA(20) <br />
Middle band = SMA(20) - (SD x 2) <br />
Where SMA(20) = The simple moving average of the closing price of the 20 days prior to the current date <br />
SD = sqrt((SUM((x-y)^2))/22) <br />
x = closing price <br />
y = average closing of period <br />

Sources: <br />
https://www.barchart.com/education/technical-indicators/moving_standard_deviation <br />
https://thetradingbible.com/bollinger-bands-trading-strategy <br />
#### Example
![Bollinger_bands_sample](https://user-images.githubusercontent.com/39235916/209172574-db5cfa67-1a43-42a0-a3a2-5b7618b6b8fe.PNG)
---

### Chandelier Exit (long) 22 day
#### Description and formula
The Chandelier Exit (long) at time *t* = highest high in the 22 days prior to date *t* - ATR<sub>*t*</sub>(n) x 3 <br />
Where ATR = Average True Range and *n*=22<br />
ATR at time *t* = ((ATR<sub>*t*-1</sub> x (*n*-1)) + TR<sub>*t*</sub>) / *n* <br />
Where TR = True Range <br />
TR at time *t* = greatest value of the following: <br />
High price at time *t* - Low price at time *t* <br />
Absolute value(High price a time *t* - Closing price at time *t-1*) <br />
Absolute value(Low price a time *t* - Closing price at time *t-1*) <br />

The first ATR value is calculated with the following formula: <br />
ATR = 1/*n*(SUM(*TR*<sub>*i*</sub>)|*i*=1 to *n*) <br />

The first ATR value in this implementation is always the date prior to the start date selected by the user.

Sources: <br />
https://school.stockcharts.com/doku.php?id=technical_indicators:chandelier_exit <br />
https://school.stockcharts.com/doku.php?id=technical_indicators:average_true_range_atr <br />
https://www.fidelity.com/learning-center/trading-investing/technical-analysis/technical-indicator-guide/atr <br />
https://en.wikipedia.org/wiki/Average_true_range <br />
#### Example
![Chandelier_exit_long_sample](https://user-images.githubusercontent.com/39235916/209172632-43a8ab1b-d71f-4e87-8865-ac8070dd5c15.PNG)
---

### Chandelier Exit (short) 22 day
#### Description and formula
The Chandelier Exit (short) at time *t* = lowest low in the 22 days prior to date *t* + ATR<sub>*t*</sub>(n) x 3 <br />
See formula and sources in above section for Chandelier Exit (long) 22 day 
#### Example
![Chandelier_exit_short_sample](https://user-images.githubusercontent.com/39235916/209172670-d1c64233-2140-4241-b40e-0b64ada7a3dc.PNG)
---

### Chandelier Exit (long and short) 22 day
#### Example
![Chandelier_exit_long_and_short_sample](https://user-images.githubusercontent.com/39235916/209172700-27de119c-7d6f-4bc3-9771-84edf7bb8bfe.PNG)
---

## Testing
TODO

## Libraries and packages
TODO
