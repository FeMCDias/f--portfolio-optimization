import os
import yfinance as yf

# Lista de 30 tickers do Dow Jones
TICKERS = [
    "AAPL","AMGN","AXP","BA","CAT","CRM","CSCO","CVX","DIS","DOW",
    "GS","HD","HON","IBM","INTC","JNJ","JPM","KO","MCD","MMM",
    "MRK","MSFT","NKE","PG","TRV","UNH","V","VZ","WBA","WMT"
]

# Períodos a baixar: (nome_da_pasta, início, fim)
PERIODS = [
    ("data",   "2024-08-01", "2024-12-31"),
    ("data_q1","2025-01-01", "2025-04-01")
]

base_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))

for folder, start, end in PERIODS:
    out_dir = os.path.join(base_dir, folder)
    os.makedirs(out_dir, exist_ok=True)
    print(f"Baixando para período {start} → {end} em '{folder}/'...")
    for ticker in TICKERS:
        print(f"  {ticker}...", end="")
        df = yf.download(ticker, start=start, end=end, progress=False)
        df = df[["Close"]].copy()
        df.index.name = "Date"
        df = df.reset_index()
        df.columns = ["Date", "Close"]
        df.to_csv(os.path.join(out_dir, f"{ticker}.csv"), index=False)
        print(" ok")
print("Todos downloads concluídos.")
