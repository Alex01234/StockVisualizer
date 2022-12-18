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
Insert screenshot and describe TODO

---

### 200-day SMA
#### Description and formula
The same formula is used as for 50-day SMA, but *n*=200 <br />
#### Example
Insert screenshot and describe TODO

---

### 50-day and 200-day SMA
#### Example
Insert screenshot and describe TODO

---

### Bollinger bands, 20-day SMA, 2 standard deviations
#### Description and formula


Sources: <br />
https://www.investopedia.com/terms/b/bollingerbands.asp <br />
https://www.barchart.com/education/technical-indicators/moving_standard_deviation <br />
https://thetradingbible.com/bollinger-bands-trading-strategy <br />
#### Example
Insert screenshot and describe TODO

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
Insert screenshot and describe TODO

---

### Chandelier Exit (short) 22 day
#### Description and formula
The Chandelier Exit (short) at time *t* = lowest low in the 22 days prior to date *t* - ATR<sub>*t*</sub>(n) x 3 <br />
See formula and sources in above section for Chandelier Exit (long) 22 day 
#### Example
Insert screenshot and describe TODO

---

### Chandelier Exit (long and short) 22 day
#### Example
Insert screenshot and describe TODO

---

## Testing
TODO

## Libraries and packages
TODO
