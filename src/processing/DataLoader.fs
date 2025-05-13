module DataLoader

open System.IO

/// Lê CSV (header “Date,Close”) e retorna só os preços de fechamento.
let private parseCsv (filePath: string) : float[] =
    File.ReadAllLines(filePath)
    |> Array.skip 1
    |> Array.map (fun line -> line.Split(',').[1] |> float)

/// Calcula retornos diários discretos: r_t = p_t / p_{t-1} - 1
let private dailyReturns (prices: float[]) : float[] =
    prices
    |> Array.pairwise
    |> Array.map (fun (prev, cur) -> (cur / prev) - 1.0)

/// Carrega retornos de um ticker
let loadTicker (dataFolder: string) (ticker: string) : string * float[] =
    let path = Path.Combine(dataFolder, $"{ticker}.csv")
    let prices = parseCsv path
    let returns = dailyReturns prices
    ticker, returns

/// Carrega todos os tickers e retorna um Map<string,float[]>
let loadAll (dataFolder: string) (tickers: string[]) =
    tickers
    |> Array.map (loadTicker dataFolder)
    |> Map.ofArray
